using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Connect.Tool
{
    public class TcpClientUnit
    {
        public TcpClientUnit(TcpClient connection, CancellationTokenSource receiveCts, int maxReceiveLength)
        {
            Connection = connection;
            ReceiveCts = receiveCts;
            MaxReceiveLength = maxReceiveLength;
        }

        public TcpClient Connection { get; }
        public CancellationTokenSource ReceiveCts { get; }
        public int MaxReceiveLength { get; }

    }

}
