using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Polaris.Connect.Tool.Base;

namespace Polaris.Connect.Tool.SerialPort
{
    public class ComSerialPort:SinglePointComBase
    {
        private readonly System.IO.Ports.SerialPort _port = new System.IO.Ports.SerialPort();
        

        /// <summary>获得可用的串口列表</summary>
        public static string[] PortNameList => System.IO.Ports.SerialPort.GetPortNames();

        public ComState ComState => _port.IsOpen ? ComState.Connect : ComState.Disconnect;

        /// <summary>构造函数</summary>
        public ComSerialPort():base()
        {
            ActiveClients = Ports; 
        }

        private CancellationTokenSource? _cts; 

        private List<string> Ports = new List<string>();

        public override IEnumerable<string> ActiveClients { get; }
        //fake method
        public override async Task WriteTo(string client, byte[] content, CancellationToken token)
        {
            var unit =  Ports[0];
            await WriteAsync(content, token);
        }

        //fake method
        public override async Task WriteTo(string client, string content, CancellationToken token)
        {
            var unit = Ports[0];
            await WriteAsync(content, token);
        }

        public void Open(
            string portName,
            int baudRate,
            int dataBits,
            StopBits stopBits,
            Parity parity,
            bool dtrEnable)
        {
            _cts=new CancellationTokenSource();
            _port.PortName = portName;
            _port.BaudRate = baudRate;
            _port.DataBits = dataBits;
            _port.StopBits = stopBits;
            _port.Parity = parity;
            _port.DtrEnable = dtrEnable;
            _port.Open();
            _port.DiscardInBuffer();
            AddPort(portName);
            SetSingleStream(_port.BaseStream);
                StartReceiveTask(portName, _cts.Token);
            RaiseConnectChanged(new ComEventArg(ComState.Connect, $"Port: {portName}"));
        }

        void AddPort(string contract) => Ports.Add(contract);
        void RemovePort(string contract) => Ports.Remove(contract);
        
        /// <summary>关闭串口，提供串口的切换操作,并返回状态</summary>
        /// <returns></returns>
        public void Close()
        {
            foreach(var port in ActiveClients)
                Dispose(port);
            }
        

protected   override void Dispose(string contract)
{
    _cts?.Dispose();
            _port.Close();
             RemovePort(contract);
            RaiseConnectChanged(new ComEventArg(ComState.Disconnect, $"Port: {_port.PortName}"));
        }

        protected override void RaiseReceivedExceptionOccur(ComErrorEventArg e)
        {
            base.RaiseReceivedExceptionOccur(e);
            Dispose(e.Contract);
        }

        
        //public async Task<byte[]> SendAndWaitResponseAsync(
        //    byte[] bytes,
        //    int responseLength,
        //    CancellationToken token,
        //    int millisecondsTimeout = 1000)
        //{
        //    Write(bytes);
        //    List<byte> res = new List<byte>();
        //    var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        //    byte[] buffer = new byte[responseLength];
        //    try
        //    {

        //        while (!token.IsCancellationRequested)
        //        {
        //            var readTask = _port.BaseStream.ReadAsync(buffer, 0, responseLength, timeoutCts.Token);


        //            if (await Task.WhenAny(readTask, Task.Delay(millisecondsTimeout, token)) == readTask)
        //            {
        //                //_connectingCts.Cancel();
        //                if (readTask.Result == 0)
        //                    return Array.Empty<byte>();
        //                res.AddRange(buffer);
        //                if (res.Count >= responseLength)
        //                    break;
        //            }
        //            else
        //            {
        //                timeoutCts.Cancel();
        //            }

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        await _port.BaseStream.FlushAsync(token);
        //        if (timeoutCts.IsCancellationRequested)
        //            throw new TimeoutException("ReadTimeOut");
        //        throw;
        //    }

        //    return res.GetRange(0, Math.Min(res.Count, responseLength)).ToArray();
        //}
    }
}
