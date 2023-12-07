using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Bee.Core.Connect.TcpServer;
using Bee.Core.Protocol;
using ScriptEditorTest.ScriptConsole.Stream;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OperationCanceledException = System.OperationCanceledException;

namespace ScriptEditorTest.ScriptConsole.COM
{
    internal class ComWrapper : IConsoleStream
    {

        private readonly TcpServer _com;

        private Dictionary<string, ComOperation> _dic;

        //private readonly ConcurrentQueue<string> _orderQueue;
        
        private CancellationTokenSource _cts;
        private Queue<string> _orderQueue;
        private  AutoResetEvent _dataReadyEvent=new AutoResetEvent(false); 
        private  AutoResetEvent _dataProcessCompletedEvent= new AutoResetEvent(true);
        
        
        public ComWrapper(TcpServer server)
        {
            _com = server;
            _com.OnConnectionReceived += (s, e) => { RaiseConsoleRead(MessageRank.Diagnostic, e.Message); };
_orderQueue=new Queue<string>();
            //_orderQueue=new ConcurrentQueue<string>();
        }

        public event EventHandler<ConsoleStreamEventArgs>? OnConsoleRead;


        public void Start()
        {
            
            _dic = GenerateOrderDic();
            //_stream = new MemoryStream();
            _cts = new CancellationTokenSource();
            _operationCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            //_writer = new StreamWriter(_stream);
            //_reader =TextReader.Synchronized(new StreamReader(_stream));
            ReadFromStream(_cts);
            RaiseConsoleRead(MessageRank.None, "Bee project");
            RaiseConsoleRead(MessageRank.None, "Bee Test>", false);
        }

        public void Close()
        {
            RaiseConsoleRead(MessageRank.None, "Bee project exit");
            _cts.Cancel();
            Dispose();
        }

        public void Dispose()
        {
            //_writer.Dispose();
            //_reader.Dispose();
            //_stream.Dispose();
             _orderQueue.Clear();
            _cts.Dispose();
        }



        private void RaiseConsoleRead(MessageRank rank, string content,bool addReturn=true)
        {
            OnConsoleRead?.Invoke(this, new ConsoleStreamEventArgs(content + (addReturn?"\r\n":""), rank));
        }


        private delegate Task ComOperation(string[] orderAndParas);
        

        

        private Dictionary<string,ComOperation> GenerateOrderDic()
        {
            return new Dictionary<string, ComOperation>()
            {
                { "listen",Listen },
                {"connect",_=>Connect()},
                {"help",_=>GetHelp()},
                { "test",_=>Test()},
                
            };
        }


        private readonly int _timeoutMilliseconds = 5000;
        private CancellationTokenSource _operationCts; 
        private async Task Listen(string[] info)
        {

            if (info.Length != 2)
            {
                RaiseConsoleRead(MessageRank.Error, $"连接参数错误,{nameof(info.Length)}:{info.Length}!=2");
                return;
            }

            var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_operationCts.Token); 
            timeoutCts.CancelAfter(5000);
            try
            {
                if (int.TryParse(info[1], out int value))
                {
                    await _com.Listen(info[0], value/*,timeoutCts*/);
                    RaiseConsoleRead(MessageRank.Diagnostic, $"开始侦听");
                }
                else
                    RaiseConsoleRead(MessageRank.Error, $"端口参数错误,Port:{info[1]}");

            }
            catch (TaskCanceledException e)
            {
                RaiseConsoleRead(MessageRank.Diagnostic, $"连接超时,Time:{_timeoutMilliseconds} ms");
            }
            catch (OperationCanceledException e)
            {
                if (!_operationCts.IsCancellationRequested)
                    RaiseConsoleRead(MessageRank.Diagnostic, $"连接超时,Time:{_timeoutMilliseconds} ms");
                else
                    throw new TaskCanceledException();
            }
            catch (Exception e)
            {
                RaiseConsoleRead(MessageRank.Error, e.Message);
            }

        }

        private async Task Connect()
        {

            

            var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_operationCts.Token);
            timeoutCts.CancelAfter(5000);
            try
            {
                
                    await _com.Connect(timeoutCts);
            }
            catch (TaskCanceledException e)
            {
                RaiseConsoleRead(MessageRank.Diagnostic, $"连接超时,Time:{_timeoutMilliseconds} ms");
            }
            catch (OperationCanceledException e)
            {
                if (!_operationCts.IsCancellationRequested)
                    RaiseConsoleRead(MessageRank.Diagnostic, $"连接超时,Time:{_timeoutMilliseconds} ms");
                else
                    throw new TaskCanceledException();
            }
            catch (Exception e)
            {
                RaiseConsoleRead(MessageRank.Error, e.Message);
            }

        }


        private Task GetHelp()
        {
            var sb = new StringBuilder("");
            foreach (var pair in _dic)
                sb.Append(pair.Key+"\r\n");
            RaiseConsoleRead(MessageRank.Diagnostic,sb.ToString());
            return Task.CompletedTask;
        }

        private async Task Test()
        {
            
                await Task.Delay(1000, _operationCts.Token);
                await Task.Delay(5000, _operationCts.Token);
            Debug.WriteLine("commit once");
            RaiseConsoleRead(MessageRank.Diagnostic, "测试结束");
        }


        /// <summary>
        ///  get tokens : 指令名  参数1  参数2 ... 
        /// </summary>
        /// <returns></returns>
        private string[] GetRawTokens(string content)
        {
            return content.Split(" ");
            ;
        }

        /// <summary>
        /// validate order
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool IsValidOrder(string[] info)
        {
            return info.Length != 0 && _dic.ContainsKey(info[0]);
        }

        private async Task DoOperation(string[] info)
        {
            var op = _dic[info[0]];
            try
            {
                await op(info.Skip(1).ToArray());
            }
            catch (TaskCanceledException e)
            {
                RaiseConsoleRead(MessageRank.Diagnostic, "操作取消");
            }
            finally
            {
                _operationCts = new CancellationTokenSource(); 
                RaiseConsoleRead(MessageRank.None, "Bee Test>", false);
            }


            
        }

        //private System.IO.Stream _stream=new System.IO.Stream;



        public async Task HandleRead(string orderLine)
        {
            string[] rawTokens = GetRawTokens(orderLine);
            if (string.IsNullOrEmpty(orderLine))
            {
                RaiseConsoleRead(MessageRank.None, "Bee Test>", false);
                return;
            }


            if (!IsValidOrder(rawTokens))
            {
                RaiseConsoleRead(MessageRank.Error, "指令错误");
                RaiseConsoleRead(MessageRank.None, "Bee Test>", false);
                return;
            }  
            await DoOperation(rawTokens);
        }

            //private Mutex _synRoot=new Mutex(); 

        public void ReadFromStream(CancellationTokenSource cts)
        {
            Task.Run(async() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    //_dataReadyEvent.WaitOne();
                    _dataReadyEvent.WaitOne();
                    
                    string line = _orderQueue.Dequeue();
                             await HandleRead(line);
                    _dataProcessCompletedEvent.Set();
                    Debug.WriteLine($"set once");
                }
            }, cts.Token);
        }
        
        
        public  bool Write(string content)
        {
            //_writer.WriteLine(content);
            //_writer.Flush();


            if (content.Contains( "p"))
            {
                _operationCts.Cancel();
                 _dataReadyEvent.Reset();
                 _dataProcessCompletedEvent.Set();
                return false;
            }

            if (_dataProcessCompletedEvent.WaitOne(0))
            {
                _orderQueue.Enqueue(content);
                Debug.WriteLine($"input:{content}");
                _dataReadyEvent.Set();
                return true;
            }
            return false;
            
            
        }
    }
}
