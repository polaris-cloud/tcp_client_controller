using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using Polaris.Connect.Tool;

namespace TcpServerTest
{
    [TestClass]
    public class TcpServerTest
    {
        private TcpServer server = new TcpServer();

        
        
        [TestMethod]
        public async Task ConnectToClientThenClose()
        {
            //server.IP = "192.168.4.2";
            //server.Port = 8088;

            Task.Delay(5000).ContinueWith(t => server.DisconnectMore());
            try
            {
                await server.Connect("192.168.1.30", 8088, new CancellationToken()); 
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            

        }
        [TestMethod]
        public async Task ConnectToClientAndSend()
        {
            //server.IP = "192.168.4.2";
            //server.Port = 8088;
            try
            {
                await server.Connect("192.168.1.30", 8088, new CancellationToken());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            
            server.WhenDataReceived += (s, d) =>
            {
                Trace.WriteLine(Encoding.UTF8.GetString(d.Data.ToArray()));
            };

            var clients = server.ActiveClients; 
            await server.WriteTo(clients.ToArray()[0],"fasdfsadf",new());

            while (true)
            {
                 
            }
        }
    }
}
    