using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Bee.Core.Utils;
using Polaris.Connect.Tool.Base;
using Polaris.Protocol.Parser;

namespace Bee.Modules.Script.Shared.Advance
{

    internal enum ReceiveState
    {
        NotComplete = 0,
        Success = 1,
        Timeout = 2,
        Faulted = 3,
        Failed = 4,
    }

    internal class ProtocolSendInfo
    {
        public string Connection;
        public byte[] Content;
        public int Timeout;
        public ProtocolScriptParser Parser;
    }

    internal class ProtocolReceiveInfo
    {
        public string Debug;
        public string Raw;
        public ReceiveState State;
    }


    internal class AutoSynProtocolComQueueWrapper
    {
        private readonly ICom _com;

        //private readonly Channel<ProtocolSendInfo> _sendChannel;
        private Channel<ComReceivedEventArg> _receiveChannel;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<ProtocolSendInfo> _synSendTask = new TaskCompletionSource<ProtocolSendInfo>();

        private TaskCompletionSource<ProtocolReceiveInfo> _synReceiveTask =
            new TaskCompletionSource<ProtocolReceiveInfo>();

        private AutoResetEvent _synCancelReceive=new AutoResetEvent(false); 
        
        public static AutoSynProtocolComQueueWrapper BuildWrapper(ICom com)
        {
            var wrapper = new AutoSynProtocolComQueueWrapper(com);
            wrapper.StartParseTask(wrapper._cancellationTokenSource);
            return wrapper;
        }

        public void Close()
        {
            _com.WhenDataReceived -= ProducingRawReceiveQueue;
            _cancellationTokenSource.Cancel();
            _synReceiveTask.SetCanceled();
            _synSendTask.SetCanceled();
            _receiveChannel.Writer.Complete();
        }



        public AutoSynProtocolComQueueWrapper(ICom com)
        {
            _com = com ?? throw new ArgumentNullException(nameof(com));

            // 初始化发送和接收队列
            //_sendChannel = Channel.CreateUnbounded<ProtocolSendInfo>();
            _receiveChannel = Channel.CreateUnbounded<ComReceivedEventArg>();
            BindingComDataReceived();
            // 初始化 CancellationTokenSource 用于取消队列操作
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #region send

        public async Task<ProtocolReceiveInfo> Post(ProtocolSendInfo info, CancellationToken token)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (_cancellationTokenSource.IsCancellationRequested)
                throw new InvalidOperationException("Connection finished");
            ProtocolReceiveInfo receiveInfo=default;
            try
            {

                await _com.WriteTo(info.Connection, info.Content, token);
                _synSendTask.SetResult(info);
                var recvTask = _synReceiveTask.Task;
                var delayCts= CancellationTokenSource.CreateLinkedTokenSource(token);
                var delayTask = Task.Delay(info.Timeout*1000, delayCts.Token);
                var completeTask = await Task.WhenAny(recvTask, delayTask);

                if (recvTask == completeTask)
                {
                    receiveInfo = recvTask.Result;
                    delayCts.Cancel();
                    try
                    {
                        await delayTask;
                    }
                    catch
                    {

                    }
                }
                else {

                    if (_receiveChannel.Writer.TryComplete())
                        _receiveChannel = Channel.CreateUnbounded<ComReceivedEventArg>();

                    await recvTask;
                }
            }
            catch (ChannelClosedException ex)
            {
                receiveInfo = new ProtocolReceiveInfo()
                {
                    Raw = $"[{info.Connection}][return]: Timeout",
                    Debug = $"[{info.Connection}][return]: Timeout",
                    State = ReceiveState.Timeout
                };
            }
            catch (Exception ex)
            {
                ////丢弃接收 
                //if (_receiveChannel.Writer.TryComplete())
                //    _receiveChannel = Channel.CreateUnbounded<ComReceivedEventArg>();
                receiveInfo = new ProtocolReceiveInfo()
                {
                    Raw = $"[{info.Connection}][return]: Exception",
                    Debug = $"[{info.Connection}][return]: Timeout",
                    State = ReceiveState.Failed
                };
            }

            _synReceiveTask = new();
            
            return receiveInfo;
        }

        #endregion

        #region Receive

        void ProducingRawReceiveQueue(object sender, ComReceivedEventArg e)
        {
            _receiveChannel.Writer.TryWrite(e);
        }

        void BindingComDataReceived()
        {
            _com.WhenDataReceived += ProducingRawReceiveQueue;
        }

        #endregion

        #region Parse

        void StartParseTask(CancellationTokenSource cts)
        {
            try
            {
                Task.Run(async () =>
                {

                    while (!cts.IsCancellationRequested)
                    {
                        var info = await _synSendTask.Task;
                        _synSendTask = new();
                        await SynReceiveProtocol(cts, info);
                    }
                }, cts.Token);
            }
            catch (OperationCanceledException ex)
            {

            }
            catch (Exception ex)
            {

            }
        }

        private async Task SynReceiveProtocol(CancellationTokenSource cts, ProtocolSendInfo info)
        {
            try
            {

                List<byte> buffer = new List<byte>();
                int want = info.Parser.GetResponseFrameLength();
                while (want > buffer.Count)
                {
                    var data = await _receiveChannel.Reader.ReadAsync(cts.Token);
                    buffer.AddRange(data.Data.ToArray());
                }

                var parsed = buffer.ToArray();
                if (want == parsed.Length)
                {
                    if (info.Parser.ParseResponseByteFrame(parsed))
                    {
                        _synReceiveTask.SetResult(new ProtocolReceiveInfo()
                        {
                            Raw =
                                $"[{info.Connection}][return]: {EncodeUtil.HexArrayToString(parsed, " ")}",
                            Debug =
                                $"[{info.Connection}][return]: {info.Parser.GenerateResponseDebugLine(parsed)}",
                            State = ReceiveState.Success
                        });
                    }
                    else
                        _synReceiveTask.SetResult(new ProtocolReceiveInfo()
                        {
                            Raw =
                                $"[{info.Connection}][return]: Error {EncodeUtil.HexArrayToString(parsed, " ")}",
                            Debug =
                                $"[{info.Connection}][return]: Timeout",
                            State = ReceiveState.Failed
                        });
                }
else 
                _synReceiveTask.SetResult(new ProtocolReceiveInfo()
                {
                    Raw = $"[{info.Connection}][return]: Error {EncodeUtil.HexArrayToString(parsed, " ")}",
                    Debug = $"[{info.Connection}][return]:  Return 'Length' Parsed Error",
                    State = ReceiveState.Failed
                });
            }

            catch (Exception ex)
            {
                //new ProtocolReceiveInfo()
                //{
                //    Raw = $"[{info.Connection}][return]: Timeout",
                //    Debug = $"[{info.Connection}][return]: {info.Order} Timeout",
                //    State = ReceiveState.Timeout
                //}
                _synReceiveTask.SetException(ex);
                if (ex is OperationCanceledException)
                                throw;
            }

            
        }

#endregion
    }
}

