using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Bee.Core.Connect.TcpServer
{

    public class ConnectEventArgs : EventArgs
    {
        public bool IsConnecting { get; set; }
        public EndPoint IpEndPoint { get; set; }
        public string Message { get; set; }
    }

    public class TcpServer
    {

        private TcpListener _listener;
        private TcpClient _client;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _connectingCts;
        private BlockingCollection<byte[]> _msgQueue;

        public event EventHandler<ConnectEventArgs> OnConnectionReceived;
        
        //public string IP{ get; set; }
        //public int Port { get; set; }
        public int MaxReceiveLength { get; set; }
        public IAnalysisReceived AnalysisStrategy { get; set; }

        internal class Default : IAnalysisReceived
        {
            public Task AnalysisReceived(BlockingCollection<byte[]> msgQueue, CancellationTokenSource cts)
            {
                return Task.CompletedTask;
            }
        }


        public TcpServer()
        {
            AnalysisStrategy = new Default();
            MaxReceiveLength = 1024;
        }


        protected virtual void RaiseConnectionReceived(bool isConnecting,EndPoint ipEndPoint,string message)
        {
            // 确保事件有订阅者
            OnConnectionReceived?.Invoke(this, new ConnectEventArgs(){IsConnecting = isConnecting,IpEndPoint = ipEndPoint,Message =message});
        }

        /// <summary>
        /// 侦听客户端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="isSyncTransRec"></param>
        /// <returns></returns>
        /// <exception cref="SocketException"> 重复连接</exception>
        public async Task ListenTo(string ip, int port, bool isSyncTransRec = true)
        {
            
            
            _listener = new TcpListener(IPAddress.Parse(ip), port);

            _cts = new CancellationTokenSource();
            _connectingCts = new CancellationTokenSource();
            using (_msgQueue = new BlockingCollection<byte[]>())
            {
                try
                {
                    _listener.Start();
                    // 开始监听客户端连接
                    while (!_cts.IsCancellationRequested)
                    {
                        // 接受一个客户端连接
                        using (_client = await _listener.AcceptTcpClientAsync(_cts.Token))
                        {
                            RaiseConnectionReceived(true, _client.Client.RemoteEndPoint, $"{_client.Client.RemoteEndPoint} 已连接");
                            Trace.WriteLine($"Connect To {_client.Client.RemoteEndPoint}");
                            if (isSyncTransRec)
                                await Task.Delay(-1, _connectingCts.Token);
                            else
                                await Task.WhenAll(AnalysisStrategy.AnalysisReceived(_msgQueue, _cts), ReceiveMessage());
                            ;
                        }
                    }
                    
                }
                finally
                {
                    _listener.Stop();
                    Trace.WriteLine($"stop listener");
                }
            }

            
        }

        /// <summary>
        /// 单次连接 
        ///</summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException">连接超时抛出异常</exception>
        public async Task ConnectTo(string ip, int port, int milliseconds=-1)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
            _cts = new CancellationTokenSource(milliseconds);
            _msgQueue = new BlockingCollection<byte[]>();
            _listener.Start();
            // 接受一个客户端连接
            _client = await _listener.AcceptTcpClientAsync(_cts.Token);
            RaiseConnectionReceived(true, _client.Client.RemoteEndPoint, $"{_client.Client.RemoteEndPoint} 已连接");
        }



        public void Close()
        {
            _cts.Cancel();
            _msgQueue.Dispose();
            _connectingCts.Cancel();
            
            //_continuousListening.SetResult();
        }




        public Task SendProtocol(byte[] bytes)
        {
            NetworkStream stream = _client.GetStream();
            return stream.WriteAsync(bytes, 0, bytes.Length, _cts.Token);
        }

        public async Task<byte[]> SendProtocolSyncReceive(byte[] bytes, int responseLength, int millisecondsTimeout = 1000)
        {


            NetworkStream stream = _client.GetStream();
            await stream.WriteAsync(bytes, 0, bytes.Length, _cts.Token);
            stream.ReadTimeout = millisecondsTimeout;
            List<byte> res = new List<byte>();
            byte[] buffer = new byte[responseLength];
            try
            {

                while (!_cts.IsCancellationRequested )
                {

                    if (await stream.ReadAsync(buffer, 0, responseLength, _cts.Token) == 0)
                    {
                        _connectingCts.Cancel();
                        return Array.Empty<byte>();
                    }
                    res.AddRange(buffer);
                    if (res.Count >= responseLength)
                        break;
                }
            }
            catch (Exception e)
            {
                stream.Flush();
                throw;
            }

            return res.GetRange(0, Math.Min(res.Count, responseLength)).ToArray();

        }

        private async Task ReceiveMessage()
        {
            await using NetworkStream stream = _client.GetStream();
            await Task.Run(async () =>
            {
                byte[] buffer = new byte[MaxReceiveLength];
                try
                {
                    var byteRead = 0;
                    while (!_cts.IsCancellationRequested &&
                           (byteRead = await stream.ReadAsync(buffer, 0, MaxReceiveLength, _cts.Token)) > 0)
                    {
                        _msgQueue.Add(buffer.Take(byteRead).ToArray());
                    }
                }
                catch (Exception e)
                {
                    stream.Flush();
                    throw;
                }
            }, _cts.Token);
        }



    }
}

