using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace TcpServerTest
{
    [TestClass]
    public class UnitTest
    {
        private TcpServer server = new TcpServer();

        public class internalS : IAnalysisReceived
        {
            public Task AnalysisReceived(BlockingCollection<byte[]>msgQueue)
            {
                return Task.CompletedTask;
            }
        }
        [TestMethod]
        public async Task ConnectToClient()
        {
            //server.IP = "192.168.4.2";
            //server.Port = 8088;
            server.MaxReceiveLength = 20;
            server.AnalysisStrategy = new internalS();
            //5秒后自动关闭
            Task.Delay(5000).ContinueWith(t => server.Close());
            try
            {
                await server.ListenTo("192.168.4.2",8088);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            await server.ListenTo("192.168.4.2", 8088);


        }

        
    }
}
    