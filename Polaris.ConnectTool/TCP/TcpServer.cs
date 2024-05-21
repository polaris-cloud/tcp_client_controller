using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Polaris.Connect.Tool.Base;
using System.Diagnostics.Contracts;
using System.IO;
using Exception = System.Exception;

namespace Polaris.Connect.Tool
{

    public class TcpServer:ICom
    {
        private TcpListener? _listener;
        readonly int _maxReceiveLength = 1024;
        
        
        
        private readonly Dictionary<string, TcpClientUnit> _activePoints = new Dictionary<string, TcpClientUnit>();
        

        #region Events
        public event EventHandler<ComEventArg> WhenConnectChanged;
        public event EventHandler<ComReceivedEventArg> WhenDataReceived;
        public event EventHandler<ComErrorEventArg> WhenReceivedExceptionOccur;

        void RaiseDataReceived(ComReceivedEventArg content)
        {
            WhenDataReceived?.Invoke(this, content);
        }

        void RaiseConnectChanged(ComEventArg newState)
        {
            WhenConnectChanged?.Invoke(this, newState);
        }
         void RaiseReceivedExceptionOccur(ComErrorEventArg e)
        {
            //UpdateActiveClients(_currentTcpMap!);
            //base.RaiseReceivedExceptionOccur(e);
            
            WhenReceivedExceptionOccur?.Invoke(this, e);
            
        }

        #endregion


        #region  core methods
        
        
        private Task WriteAsyncInternal(Stream stream, byte[] bytes, CancellationToken token) =>
            stream.WriteAsync(bytes, 0, bytes.Length, token);

        private Task WriteAsyncInternal(Stream stream, string content, CancellationToken token)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            return WriteAsyncInternal(stream, bytes, token);
        }
        
        
        
        
        private  Task StartReceiveTask(string contract, TcpClientUnit clientUnit)
        {
            return Task.Run(async() =>
            {
                try
                {
                    byte[] buffer = new byte[_maxReceiveLength];
                    var cts = clientUnit.ReceiveCts;
                    await using var  stream = clientUnit.Connection.GetStream();
                    while (!cts.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, _maxReceiveLength, cts.Token);
                        if (bytesRead == 0)
                        {
                            DisposeComPoint(contract, clientUnit);
                            break;
                        }
                        RaiseDataReceived(new ComReceivedEventArg(contract, buffer.AsMemory(0, bytesRead)));
                    }
                }
                catch (Exception ex)
                {
                    RaiseReceivedExceptionOccur(new ComErrorEventArg(contract, ex));
                    DisposeComPoint(contract, clientUnit);
                }
            }); 
            
        }

        private TcpClientUnit GetClientMapping(string contract)
        {
            if (string.IsNullOrEmpty(contract))
                throw new ArgumentNullException(nameof(contract));
            if (!_activePoints.ContainsKey(contract))
                throw new ArgumentException($"not contain {nameof(contract)}:{contract} in mappings",nameof(contract));

            return _activePoints[contract];
        }

         void RemoveClientMapping(string contract)
        {
            if (string.IsNullOrEmpty(contract))
                throw new ArgumentNullException(nameof(contract));
            if (!_activePoints.ContainsKey(contract))
                throw new ArgumentException($"not contain {nameof(contract)}:{contract} in mappings", nameof(contract));
            _activePoints.Remove(contract);
        }

         void AddClientMapping(string contract,TcpClientUnit clientUnit)
        {
            if (string.IsNullOrEmpty(contract))
                throw new ArgumentNullException(nameof(contract));
            if (_activePoints.ContainsKey(contract))
                throw new ArgumentException($" already contains {nameof(contract)}:{contract} in mappings", nameof(contract));
            _activePoints.Add(contract, clientUnit);
        }

        void DisposeComPoint(string contract,TcpClientUnit clientUnit)
        {
clientUnit.ReceiveCts.Dispose();
            RemoveClientMapping(contract);
            clientUnit.Connection.Close();
            RaiseConnectChanged(new ComEventArg(ComState.Disconnect, $"Client: {contract}"));
            
        }
        
        
        void StopAll()
        {
            _listener?.Stop();
            foreach (var tcpUnit in _activePoints.Values)
            {
                tcpUnit.Connection.Client.Close();
            }
            _activePoints.Clear();
            RaiseConnectChanged(new ComEventArg(ComState.Disconnect, $" All ActiveClients"));
        }
        
        #endregion
        
        public async Task Connect(
            string ip,
            int port,
            CancellationToken token)
        {
            _listener?.Stop();
            _listener = new TcpListener(IPAddress.Parse(ip), port);
            _listener.Start();

            
            while (!token.IsCancellationRequested)
            {
                var receivedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                var client = await _listener.AcceptTcpClientAsync(token);
                string contract = client.Client.RemoteEndPoint!.ToString()!;
                var comPoint = new TcpClientUnit(client, receivedCts, _maxReceiveLength);
                AddClientMapping(contract, comPoint);
                _=StartReceiveTask(contract,comPoint);
                RaiseConnectChanged(new ComEventArg(ComState.Connect, $"Client: {contract}"));
            }
        }


        public async Task WriteTo(string client, byte[] bytes, CancellationToken token)
        {
            var unit = GetClientMapping(client);
            await WriteAsyncInternal(unit.Connection.GetStream(), bytes, token);
        }

        public async Task WriteTo(string client, string content, CancellationToken token)
        {
            var unit = GetClientMapping(client);
            await WriteAsyncInternal(unit.Connection.GetStream(),content, token);
        }

        public IEnumerable<string> ActiveClients => _activePoints.Keys; 

        public void DisconnectMore()
        {
            StopAll();
        }
    }
}
