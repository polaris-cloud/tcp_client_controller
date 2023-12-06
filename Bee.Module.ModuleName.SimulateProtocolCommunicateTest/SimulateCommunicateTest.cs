using System.Diagnostics;
using System.Runtime.InteropServices;
using Bee.Core.Connect.TcpServer;
using Bee.Core.Protocol;
using Bee.Core.Protocol.Model;


namespace Bee.Module.Script.SimulateProtocolCommunicateTest
{
    [TestClass]
    public class SimulateCommunicateTest
    {

        string sendRule =
            @"<={a2 00 09}><x��:(-32767/32767)|4><y��:(-32767/32767)|4><�Ƿ���Ӧ:(0/1)|1><?CRC16|2>";

        string reponseRule = @"<={a2 00 00}><?crc16|2>}";
        private TcpServer? server;


        [TestMethod]
        public async Task TestTransReceive()
        {
            ProtocolFormat format = new ProtocolFormat()
            { SendFrameDescription = "���ƶ�", SendFrameRule = sendRule, ResponseFrameRule = reponseRule };
            ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            var ar = protocolScriptParser.GenerateSendFrame("MoveOsc x��=1000 y��=1000 �Ƿ���Ӧ=1", out string debugLine);
            Trace.WriteLine(debugLine);
            server = new TcpServer();
            server.ListenTo("192.168.4.2", 8088);

            Thread.Sleep(10000);
            byte[] response = await server.SendProtocolSyncReceive(ar, protocolScriptParser.ResponseFrameLength);
            var resString = ar.Select(a => a.ToString("X2"));
            Trace.WriteLine(string.Join(" ", resString));
            Trace.WriteLine(string.Join(" ", response.Select(a => a.ToString("X2"))));
            server.Close();

        }
    }
}