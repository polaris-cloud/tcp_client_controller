using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Connect.Tool.Base
{
     public interface ICom
     {
         IEnumerable<string> ActiveClients { get; }
         Task  WriteTo(string client, byte[] content, CancellationToken token);
        Task  WriteTo(string client, string content, CancellationToken token);
        public event EventHandler<ComEventArg> WhenConnectChanged;
        public event EventHandler<ComReceivedEventArg> WhenDataReceived;
        public event EventHandler<ComErrorEventArg> WhenReceivedExceptionOccur;
    }
}
