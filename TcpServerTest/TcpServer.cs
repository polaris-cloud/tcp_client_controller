using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Transactions;

namespace TcpServerTest
{
    public class TcpServer
    {

        private TcpListener _listener;
        private TcpClient _client;
        private CancellationTokenSource _cts;
        private BlockingCollection<byte[]> _msgQueue ;



        //public string IP{ get; set;  }
        //public int Port { get; set; }
        public int MaxReceiveLength { get; set; }
        public IAnalysisReceived AnalysisStrategy { get; set; }
        
        public async Task ListenTo(string ip, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ip),port);
            using (_cts = new CancellationTokenSource()) 
                using (_msgQueue =new BlockingCollection<byte[]>())

                    try
                    {
                        _listener.Start();
                        // 开始监听客户端连接
                        while (!_cts.IsCancellationRequested)
                        {

                            // 接受一个客户端连接
                            using (_client = await _listener.AcceptTcpClientAsync(_cts.Token))
                            {
                                Trace.WriteLine($"Connect To {_client.Client.RemoteEndPoint}");
                                await Task.WhenAll(AnalysisStrategy.AnalysisReceived(_msgQueue), ReceiveMessage());
                            }

                        }
                    }
                    finally
                    {
                        _listener.Stop();
                    }




        }


        public  void Close()
        {
                    _cts.Cancel();
            }

        
        
        
            public  Task SendProtocol(byte[] bytes)
        {
              NetworkStream stream = _client.GetStream();
             return stream.WriteAsync(bytes, 0, bytes.Length, _cts.Token);
        }
            
        
        
        private async Task ReceiveMessage()
        {
             await using NetworkStream stream = _client.GetStream();
             await Task.Run(async () =>
             {
                 byte[] buffer = new byte[MaxReceiveLength];
                 try
                 {
                     var byteRead = 0;
                     while (!_cts.IsCancellationRequested && 
                            (byteRead= await stream.ReadAsync(buffer, 0, MaxReceiveLength, _cts.Token))>0)
                     {
                         _msgQueue.Add(buffer.Take(byteRead).ToArray());
                     }
                 }
                 catch (Exception e)
                 {
                     stream.Flush();
                     throw;
                 }
             }, _cts.Token);
        }


        
    }
}

