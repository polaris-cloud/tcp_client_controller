using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServerTest
{
    public interface IAnalysisReceived
    {
        Task AnalysisReceived(BlockingCollection<byte[]> msgQueue,CancellationTokenSource cts);
    }
}
