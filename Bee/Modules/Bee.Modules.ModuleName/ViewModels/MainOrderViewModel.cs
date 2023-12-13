using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bee.Core.Connect.TcpServer;
using Bee.Core.Protocol;
using Bee.Core.Protocol.Model;
using Bee.Modules.Script.Models;
using Microsoft.Xaml.Behaviors.Media;
using Prism.Commands;
using Prism.Mvvm;

namespace Bee.Modules.Script.ViewModels
{
    public class MainOrderViewModel : BindableBase
    {
        private Dictionary<string, ProtocolFormat> protocolFormats;
        private string _sendScript;
        private string _debugMessage;
        public string SendScript { get => _sendScript; set => SetProperty(ref _sendScript, value); }

        public bool IsConnect
        {
            get => _isConnect;
            set => SetProperty(ref _isConnect, value);
        }

        public string DebugMessage
        {
            get => _debugMessage;
            set => SetProperty(ref _debugMessage, value);
        }

        public string InstructionSet
        {
            get => _instructionSet;
            set => SetProperty(ref _instructionSet, value);
        }



        private TcpServer _server = new TcpServer();
        private bool _isConnect;
        private string _ip;
        private string _port;
        private string _instructionSet;

        public string IP
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }
        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand DisconnectCommand { get; private set; }
        public DelegateCommand ParseCommand { get; private set; }
        public DelegateCommand SendCommand { get; private set; }
        public MainOrderViewModel(InstructionSetDao dao)
        {
            if (dao.Protocols == null)
                dao.GetStorage();
            protocolFormats = dao.Protocols?.Select(t => new ProtocolFormat
            {
                ProtocolEndian = t.ProtocolEndian,
                SendFrameDescription = t.SendDescription,
                ResponseFrameDescription = t.ResponseDescription,
                EncodeFormat = t.EncodeFormat,
                BehaviorKeyword = t.BehaviorKeyword,
                SendFrameRule = t.SendFrameRule,
                ResponseFrameRule = t.ResponseFrameRule,

            }).ToDictionary(t => t.BehaviorKeyword, t => t);
            _server.OnConnectionReceived += (s, e) =>
            {
                IsConnect = e.IsConnecting;
                WriteLine(e.Message);

            };
            ConnectCommand = new DelegateCommand(Connect);
            DisconnectCommand = new DelegateCommand(Disconnect);
            ParseCommand = new DelegateCommand(ProcessLine);
            SendCommand = new DelegateCommand(SendProtocol);
            //Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
            //        handler=>(s,e)=>handler(s,e),
            //        h => PropertyChanged += h,
            //        h => PropertyChanged -= h)
            //    .Where(pattern => pattern.EventArgs.PropertyName == nameof(InstructionSet))
            //    .Throttle(TimeSpan.FromSeconds(1))
            //    .Subscribe(_ => { 
            //         ProcessLine();
            //    });
        }


        private async void Connect()
        {
            try
            {
                if (string.IsNullOrEmpty(IP) || string.IsNullOrEmpty(Port))
                    return;
                await _server.Listen(IP, Convert.ToInt32(Port));
            }
            catch (TaskCanceledException e)
            {

            }
            catch (SocketException e)
            {
                WriteLine(e.Message);
            }

        }
        private void Disconnect()
        {
            _server.CloseListen();

        }


        private async void SendProtocol()
        {
            try
            {
                foreach (var instruction in InstructionSet.Split("\r\n"))
                {
                    string[] behaviorAndValues = instruction.Split(">");
                    if (behaviorAndValues.Length > 2)
                    {
                        WriteLine($"执行中断:{instruction}");
                        return;
                    }

                    string behavior = behaviorAndValues[0];
                    if (protocolFormats.TryGetValue(behavior, out ProtocolFormat format))
                    {
                        ProtocolScriptParser parser = ProtocolScriptParser.BuildScriptParser(format);
                        byte[] send = parser.GenerateSendFrame(instruction, out string debugLine);
                        WriteLine(debugLine);
                        byte[] response = await _server.SendProtocolSyncReceive(send, parser.ResponseFrameLength);
                        string responseDebugLine = parser.GenerateResponseDebugLine(response);
                        WriteLine(responseDebugLine);
                    }
                    else
                    {
                        WriteLine($"执行中断(找不到该指令):{instruction}");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                _server.CloseListen();
            }
        }




        //private Dictionary<string,ProtocolScriptParser> parsers = new Dictionary<string, ProtocolScriptParser>();

        private void ProcessLine()
        {
            //parsers.Clear();
            string userInput = InstructionSet;
            var lines = userInput.Trim().Split("\r\n");
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                if (line?.Trim().EndsWith('>') is true)
                {
                    string behavior = line.TrimEnd('>') ?? "";
                    if (protocolFormats.TryGetValue(behavior, out ProtocolFormat format))
                    {
                        ProtocolScriptParser parser = ProtocolScriptParser.BuildScriptParser(format);
                        var result = parser.GenerateSendScript();
                        //parsers.Add(behavior,parser);
                        newLines.Add(result);
                    }
                }

            }

            InstructionSet = string.Join("\r\n", newLines);
        }






        private void WriteLine(string message)
        {
            DebugMessage += $"{message}\n";
        }

    }
}
