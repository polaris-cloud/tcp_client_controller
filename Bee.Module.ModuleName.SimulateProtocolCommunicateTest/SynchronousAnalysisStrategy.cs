using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpServerTest;

namespace Bee.Module.ModuleName.SimulateProtocolCommunicateTest
{
    internal class SynchronousAnalysisStrategy:IAnalysisReceived
    {
        private readonly int _millisecondsTimeout;
        public SynchronousAnalysisStrategy(int millisecondsTimeout)
        {
            _millisecondsTimeout=millisecondsTimeout;
        }

        public int ReceiveLength { get; set; }
        
        
        public  Task AnalysisReceived(BlockingCollection<byte[]> msgQueue,CancellationTokenSource cts)
        {
            return Task.Run(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    
                        msgQueue.TryTake(out byte[] response, _millisecondsTimeout, cts.Token);
                    ReceiveLength = 0;
                }

                
                
            });
            
        }
    }
}
