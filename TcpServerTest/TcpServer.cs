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

        private TaskCompletionSource _continuousListening;

        //public string IP{ get; set; }
        //public int Port { get; set; }
        public int MaxReceiveLength { get; set; }
        public IAnalysisReceived AnalysisStrategy { get; set; }

internal   class Default : IAnalysisReceived
        {
            public Task AnalysisReceived(BlockingCollection<byte[]> msgQueue, CancellationTokenSource cts)
            {
                return Task.CompletedTask;
            }
        }


        public TcpServer()
        {
            AnalysisStrategy = new Default();
            MaxReceiveLength = 1024; 
        }

        public async Task ListenTo(string ip, int port, bool isSyncTransRec = true)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
            _continuousListening = new TaskCompletionSource();
            using (_cts = new CancellationTokenSource())
            using (_msgQueue = new BlockingCollection<byte[]>())

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
                            if (isSyncTransRec)
                                await Task.Delay(-1, _cts.Token);
                            else
                                await Task.WhenAll(AnalysisStrategy.AnalysisReceived(_msgQueue,_cts), ReceiveMessage());
                        }

                    }
                }
                finally
                {
                    _listener.Stop();
                    Trace.WriteLine($"stop listener");
                }
        }

        [Obsolete]
        public async Task ListenToOnce(string ip, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
            _continuousListening = new TaskCompletionSource();
            _cts = new CancellationTokenSource();
            _msgQueue = new BlockingCollection<byte[]>();
                    _listener.Start();
                        // 接受一个客户端连接
                        _client = await _listener.AcceptTcpClientAsync(_cts.Token);
                            Trace.WriteLine($"Connect To {_client.Client.RemoteEndPoint}");
        }



        public void Close()
        {
                    _cts.Cancel();
                    //_continuousListening.SetResult();
            }

        
        
        
            public  Task SendProtocol(byte[] bytes)
        {
              NetworkStream stream = _client.GetStream();
             return stream.WriteAsync(bytes, 0, bytes.Length, _cts.Token);
        }

        public async Task<byte[]> SendProtocolSyncReceive(byte[] bytes, int responseLength, int millisecondsTimeout = 1000)
        {


            NetworkStream stream = _client.GetStream();
            await stream.WriteAsync(bytes, 0, bytes.Length, _cts.Token);
            stream.ReadTimeout = millisecondsTimeout;
            List<byte> res = new List<byte>();
            byte[] buffer = new byte[responseLength];
            try
            {

                while (!_cts.IsCancellationRequested &&
                       (await stream.ReadAsync(buffer, 0, responseLength, _cts.Token)) > 0)
                {
                    res.AddRange(buffer);
                    if (res.Count >= responseLength)
                        break;
                }
            }
            catch (Exception e)
            {
                stream.Flush();
                throw;
            }

            return res.GetRange(0, Math.Min(res.Count, responseLength)).ToArray();

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

