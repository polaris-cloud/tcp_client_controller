using System.Diagnostics;
using System.Text;
using Polaris.Console.Stream;
using OperationCanceledException = System.OperationCanceledException;

namespace Polaris.Console.Wrapper 
{
    public  class ComWrapper : IConsoleStream,IConsoleWriter
    {
        private readonly ComMappingProvider _comProvider;


        private CancellationTokenSource _cts;
        private CancellationTokenSource _operationCts;
        private Queue<string> _orderQueue;
        private  AutoResetEvent _dataReadyEvent=new AutoResetEvent(false); 
        private  AutoResetEvent _dataProcessCompletedEvent= new AutoResetEvent(true);
        
        
        public ComWrapper(ComMappingProvider comProvider)
        {
            _comProvider = comProvider;
            //_com.OnConnectionReceived += (s, e) => { RaiseConsoleRead(MessageRank.Diagnostic, e.Message); };
_orderQueue=new Queue<string>();
            //_orderQueue=new ConcurrentQueue<string>();
        }

        public event EventHandler<ConsoleStreamEventArgs>? OnConsoleRead;


        public void Start()
        {
            
            AddDefaultOrder();
            //_stream = new MemoryStream();
            _cts = new CancellationTokenSource();
            _operationCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            //_writer = new StreamWriter(_stream);
            //_reader =TextReader.Synchronized(new StreamReader(_stream));
            ReadFromStream(_cts);
            RaiseConsoleRead(MessageRank.None, "Bee project"+Environment.NewLine);
            RaiseConsoleRead(MessageRank.None, "Bee Test>");
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
        
        private void RaiseConsoleRead(MessageRank rank, string content)
        {
            OnConsoleRead?.Invoke(this, new ConsoleStreamEventArgs(content, rank));
        }


        private void AddDefaultOrder()
        {
            _comProvider.TryRegister("help",(k,c,t)=>GetHelp());
                  _comProvider.TryRegister("test",(k, c, t) => Test(t));
        }

        
        
        
        private Task GetHelp()
        {
            var sb = new StringBuilder("");
            foreach (var order in _comProvider.GetOrders())
                sb.Append(order+"\r\n");
            RaiseConsoleRead(MessageRank.Diagnostic,sb.ToString());
            return Task.CompletedTask;
        }
        private async Task Test(CancellationToken token)
        {
            await Task.Delay(1000, token);
            Debug.WriteLine("commit once");
            RaiseConsoleRead(MessageRank.Diagnostic, "测试结束"+ Environment.NewLine);
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
            return info.Length != 0 && _comProvider.RegisteredOrder(info[0]);
        }

        /// <summary>
        ///  传入命令行的参数
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private async Task DoOperation(string[] info)
        {
            
            try
            {
                var op = _comProvider.GetMapping(info[0]);
                await op(this,info.Skip(1).ToArray(),_operationCts.Token);
            }
            catch (TaskCanceledException e)
            {
                RaiseConsoleRead(MessageRank.Diagnostic, "操作取消"+Environment.NewLine);
            }
            finally
            {
                _operationCts = new CancellationTokenSource(); 
                RaiseConsoleRead(MessageRank.None, "Bee Test>");
            }


            
        }
        
        public async Task HandleRead(string orderLine)
        {
            string[] rawTokens = GetRawTokens(orderLine);
            if (string.IsNullOrEmpty(orderLine))
            {
                RaiseConsoleRead(MessageRank.None, "Bee Test>");
                return;
            }


            if (!IsValidOrder(rawTokens))
            {
                RaiseConsoleRead(MessageRank.Error, "指令错误" + Environment.NewLine);
                RaiseConsoleRead(MessageRank.None, "Bee Test>");
                return;
            }  
            await DoOperation(rawTokens);
        }
        
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


            if (_dataProcessCompletedEvent.WaitOne(0))
            {
                _orderQueue.Enqueue(content);
                Debug.WriteLine($"input:{content}");
                _dataReadyEvent.Set();
                return true;
            }

            if (content.Contains("p"))
            {
                _operationCts.Cancel();
                _dataReadyEvent.Reset(); 
                _dataProcessCompletedEvent.Set();
                return false;
            }

            return false;
            
        }
        
        public void ConsoleWrite(MessageRank rank, string content)
        {
            RaiseConsoleRead(rank,content);
        }
    }
}
