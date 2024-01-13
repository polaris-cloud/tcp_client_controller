using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Polaris.Connect.Tool;
using Polaris.Protocol.Model;
using Polaris.Protocol.Parser;


namespace Bee.Module.Script.SimulateProtocolCommunicateTest
{
    [TestClass]
    public class SimulateCommunicateTest
    {

        string sendRule =
            @"<={a2 00 09}><xÖá:(-32767/32767)|4><yÖá:(-32767/32767)|4><ÊÇ·ñÏìÓ¦:(0/1)|1><?CRC16|2>";

        string reponseRule = @"<={a2 00 00}><?crc16|2>}";
        private TcpServer? server;


        [TestMethod]
        public async Task TestTransReceive()
        {
            ProtocolFormat format = new ProtocolFormat()
            { SendFrameDescription = "Õñ¾µÒÆ¶¯", SendFrameRule = sendRule, ResponseFrameRule = reponseRule };
            ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            var ar = protocolScriptParser.GenerateSendFrame("MoveOsc xÖá=1000 yÖá=1000 ÊÇ·ñÏìÓ¦=1", out string debugLine);
            Trace.WriteLine(debugLine);
            server = new TcpServer();
            await server.Connect("192.168.4.2", 8088,new CancellationToken());
            
            
            server.WhenDataReceived += (s, d) =>
            {
                Trace.WriteLine(string.Join(" ", d.Data.ToArray().Select(a => a.ToString("X2"))));
            };
            
            await server.WriteTo(server.ActiveClients.ToArray()[0],ar ,new CancellationToken());
            var resString = ar.Select(a => a.ToString("X2"));
            Trace.WriteLine(string.Join(" ", resString));
            Thread.Sleep(5000);
            server.DisconnectMore();
            
        }
    }
}