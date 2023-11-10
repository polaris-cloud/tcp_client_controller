using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
namespace tcp_client_controller
{
   //public class tcp_client
   // {
   //     public string IP;
   //     public string Port;
   //     public string send_msg;
   //     public string recvMsg;
   //     public bool bIsHex;
   //     public bool bIsApplyCycleSend;
   //     public bool CycleTime;


        

   //     public delegate void deal_recv_data(string msg);
   //     public event deal_recv_data deal_recv_event;

   //     public tcp_client()
   //     {

   //     }


   //     public void Send(string msg)
   //     {
   //         byte[] temp = Encoding.ASCII.GetBytes(msg);
   //         client.Send(temp);

   //     }

   //     public void Send(byte[] msg)
   //     {
   //         client.Send(msg);
   //     }

   //     public async Task SendAsync(string msg)
   //     {
   //         // await client()
   //         await Task.Run(() => {

   //             Send(msg);
   //         });
   //     }

   //     public async Task SendAsync(byte[] msg)
   //     {
   //         // await client()
   //         await Task.Run(() => {

   //             Send(msg);
   //         });
   //     }

   //     public async Task ConnectAsync(IPEndPoint ip_end)
   //     {
   //         await Task.Run(() => { client.Connect(ip_end); });
   //     }

   //     public async Task<bool> connect(string ip, int port)
   //     {
            
   //             IPAddress ip_addr = IPAddress.Parse(ip);
   //             IPEndPoint ip_end = new IPEndPoint(ip_addr, port);
   //             await Task.Run(() => {
   //                 try
   //                 {

   //                     client.Connect(ip_end);
   //                 }
   //                 catch (SocketException e)
   //                 {
   //                     Console.Write("Fail to connect server");
   //                     Console.Write(e.ToString());
                       
   //                 }
   //             });
   //             if (client.Connected)
   //             {
   //                 await receive();
   //                 return true;
   //             }
   //                return false;  
                
            
   //     }

   //     public async Task<bool> disconnect()
   //     {
          
   //             await Task.Run(() =>
   //             {
   //                 try
   //                 {
   //                     client.Shutdown(SocketShutdown.Both);
   //                     client.Close();
   //                 }
   //                 catch
   //                 {

   //                 }
   //             });

   //         return client.Connected;

   //     }

    

   //     public async Task receive()
   //     {
   //         await Task.Run(() =>
   //         {
   //             byte[] buffer = new byte[1024];
   //             while (true)
   //             {
   //                 int len = client.Receive(buffer);
   //                 recvMsg = Encoding.UTF8.GetString(buffer, 0, len);
   //                 deal_recv_event(recvMsg);
   //             }
   //         }
   //              );
   //     }

   // }
}
