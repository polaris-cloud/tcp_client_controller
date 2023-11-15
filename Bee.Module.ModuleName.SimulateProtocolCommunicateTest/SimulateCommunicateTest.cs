using System.Diagnostics;
using System.Runtime.InteropServices;
using Bee.Module.ModuleName.ProtocolParser.Protocol;
using TcpServerTest;

namespace Bee.Module.ModuleName.SimulateProtocolCommunicateTest
{
    [TestClass]
    public class SimulateCommunicateTest
    {

        string sendRule =
            @"<device>:1 | <function>:1 | <datalength>:1 | <x��:(-32767-32767)>:4 |<y��:(-32767-32767)>:4 |<�Ƿ���Ӧ:>:1 |<crc?CRC16>:2
                       device={a2}
                        function={00}
                     datalength={09}";

        string reponseRule = @"<device>:1 | <function>:1 | <datalength>:1 | <crc?CRC16>:2
 device={a2}
 function={00}
 datalength={00}";
        private TcpServer server;
        
        
        
        
        
        [TestMethod]
        public async Task  TestTransReceive()
        {
            ProtocolFormat format = new ProtocolFormat()
                { Name = "���ƶ�", SendFrameRule = sendRule, ResponseFrameRule = reponseRule };
            ProtocolScript protocolScript = Converter.ToProtocolScript(format);
            var ar = protocolScript.GenerateSendByteFrame("MoveOsc x��=1000 y��=1000 �Ƿ���Ӧ=1");
            server = new TcpServer();
              server.ListenTo("192.168.4.2", 8088);
              Thread.Sleep(10000);
            byte[] response=await server.SendProtocolSyncReceive(ar, protocolScript.ResponseFrameLength);
            var resString = ar.Select(a => a.ToString("X2"));
            Trace.WriteLine(string.Join(" ", resString));
            server.Close();

        }
    }
}