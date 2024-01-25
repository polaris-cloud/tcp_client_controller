using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Bee.Modules.Communication.Setting;
using Bee.Modules.Communication.Shared;
using Polaris.Connect.Tool;
using Prism;
using Bee.Core.Utils;
using System.Windows.Media;
using Bee.Core.DataSource;
using Bee.Services.Interfaces;
using Polaris.Connect.Tool.SerialPort;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Polaris.Connect.Tool.Base;
using System.Reflection.Metadata;
using Bee.Core.Controls;

namespace Bee.Modules.Communication.ViewModels
{
    public class TcpDebuggerViewModel : BindableBase,IEasyLoggingBindView,IActiveAware,IAppDataApply<TcpDebuggerSetting>
    {
        


        public TcpDebuggerViewModel(
            ComManager comRegistry,
            TcpServer server, //todo : use ComBase in base class
            AppDataCache appDataCache,
            IMessageService messageService,
            ScopedAppDataSourceManager manager,
            ComModuleCommand comModuleCommand
            )
        {
            _server = server;
            _messageService = messageService;
            _manager = manager;

            comRegistry.Register("Tcp Server",server);

            var moduleSetting = appDataCache.GetMapping<CommunicationModuleSetting>(); 
             _debuggerSetting= appDataCache.GetMapping<TcpDebuggerSetting>();
             _communicationModuleSetting=appDataCache.GetMapping<CommunicationModuleSetting>();
             ApplyAppData(_debuggerSetting);
            BindingComEvents(moduleSetting);
            NetTypes = GenerateNetTypes().ToArray(); 
            IPAddresses = GetLocalIPAddress();
            CanOpenState = true;
            ConnectClients=Array.Empty<string>();
            SaveCommand = new DelegateCommand(Save);
            comModuleCommand.SaveCommand.RegisterCommand(SaveCommand);
            EmptyReceivedCommand = new DelegateCommand(TriggerEmptyReceive);
            RefreshLocalIPCommand = new DelegateCommand(() => IPAddresses = GetLocalIPAddress());
            ConnectCommand = new DelegateCommand(Connect); 
            DisconnectCommand = new DelegateCommand(Disconnect);
            EmptySendCommand = new DelegateCommand(() => InputData = "");
            SendCommand = new DelegateCommand(Send, CanSend).ObservesProperty(()=>IsStillSending).ObservesProperty(()=>ConnectClients); 
        }

        


        #region Fields ForXamlBinding



        public NetType[] NetTypes { get; }

        public NetType SelNetType { get; set;  }

        public IPAddress[] IPAddresses
        {
            get => _ipAddresses;
            set => SetProperty(ref _ipAddresses, value);
        }


        public IPAddress SelIPAddress { get; set; }

        public int Port { get; set; }

public FrameFormat SendFrameFormat
{
    get => _sendFrameFormat;
    set => SetProperty(ref _sendFrameFormat, value);
}

public FrameFormat ReceiveFrameFormat
{
    get => _receiveFrameFormat;
    set => SetProperty(ref _receiveFrameFormat, value);
}

public bool IsOutputAsLog { get; set; }
public bool IsAddNewLineWhenSend { get; set; }
public bool IsSendAtRegularTime { get; set; }
public int SendCycleTime { get; set; }

public string[] ConnectClients
{
    get => _connectClients;
    set => SetProperty(ref _connectClients, value);
}

public string SelClient
{
    get => _selClient;
    set => SetProperty(ref _selClient, value);
}


public string InputData
{
    get => _inputData;
    set => SetProperty(ref _inputData, value);
}

#region UI State

public bool CanOpenState
{
    get => _canOpenState;
    set => SetProperty(ref _canOpenState, value);
}

public bool IsStillSending
{
    get => _isStillSending;
    set => SetProperty(ref _isStillSending, value);
}

#endregion
        
        
private IPAddress[] _ipAddresses;
private FrameFormat _sendFrameFormat;
private FrameFormat _receiveFrameFormat;
private string[] _connectClients;
private string _selClient;
        private bool _isStillSending;
        private string _inputData;

        #endregion


        #region Commands ForXamlBinding  & relative methods


        public DelegateCommand SaveCommand { get; } //  method from  #region IAppDataApply 

        private void Save()
        {
            SaveAppData(_debuggerSetting);
        }

        public DelegateCommand EmptyReceivedCommand { get; }

        private void TriggerEmptyReceive()
        {
            OnOutputEmpty?.Invoke(this,EventArgs.Empty);
        }

        public DelegateCommand RefreshLocalIPCommand { get;  }
        public DelegateCommand ConnectCommand { get;  }
        public DelegateCommand DisconnectCommand { get; }
        public DelegateCommand EmptySendCommand { get; }
        public DelegateCommand SendCommand { get; }

        private async void Connect()
        {
            try
            {

                RaiseLogOutput("Start Listen");
                CanOpenState = false;
                await CoreOpenCom();

            }
            catch (OperationCanceledException ex)
            {
                RaiseLogOutput("Stop Listen");
                
            }
            catch (Exception ex)
            {
                RaiseErrorOutput(ex);
            }
            CanOpenState = true;

        }
        private void Disconnect()
        {
            try
            {
                CoreCloseCom();
            }
            catch (Exception ex)
            {
                RaiseErrorOutput(ex);
            }

            
        }
        private async void Send()
        {
            IsStillSending = true;
            try
            {
                await CycleSend(SelClient,InputData,IsAddNewLineWhenSend);
            }
            catch (Exception e)
            {
                RaiseErrorOutput(e);
            }
            IsStillSending = false;

        }
        private bool CanSend() => !IsStillSending && ConnectClients.Length>0; 

        
        
        #endregion
        
        #region  core(others) &  bottom methods  尽量不涉及界面交互，只读取所需要的参数readonly dont write
        
        private readonly TcpServer _server;
        private readonly TcpDebuggerSetting _debuggerSetting;
        private readonly IMessageService _messageService;
        private readonly ScopedAppDataSourceManager _manager;
private readonly CommunicationModuleSetting _communicationModuleSetting;
        private void BindingComEvents(CommunicationModuleSetting moduleSetting)
        {
            _server.WhenDataReceived +=
                (s, d) =>
                    RaiseOutputReceivedData(d);

            _server.WhenReceivedExceptionOccur += (s, e) =>
            {
                RaiseReceivedErrorOutput(e);
            };
            _server.WhenConnectChanged += (s, e) =>
            {
                RaiseLogOutput(e.ComDetails + (e.State == ComState.Connect ? " Connect" : " Disconnect"));
                OnConnectionChanged(e);
            };
        }
        //todo: extract to base class
        private void OnConnectionChanged(ComEventArg e)
        {   
            // 刷新本机地址 刷新连接的客户端
            IPAddresses = GetLocalIPAddress();
            ConnectClients = GetConnectClients();
            if (ConnectClients.Length > 0 && !ConnectClients.Contains(SelClient))
            {
                 SelClient= ConnectClients[0];
            } 

            
        }

        private IEnumerable<NetType> GenerateNetTypes()
        {
            yield return NetType.TcpServer;
        }
        
        private IPAddress[] GetLocalIPAddress()
        {
            try
            {
                return Dns.GetHostAddresses(Dns.GetHostName());
            }
            catch (Exception ex)
            {
                RaiseErrorOutput(ex);
                return Array.Empty<IPAddress>();
            }
        }

        private CancellationTokenSource _connectCancellationTokenSource;
        
        /// <summary>
        /// core exception
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>>
        private async Task CoreOpenCom()
        {
            _connectCancellationTokenSource=new CancellationTokenSource();
            await _server.Connect(SelIPAddress.ToString(), Port, _connectCancellationTokenSource.Token);
        }

        private void CoreCloseCom()
        {
            _connectCancellationTokenSource.Cancel();
            _server.DisconnectMore();
            
            
        }

        private string[] GetConnectClients()
        {
            return _server.ActiveClients.ToArray(); 
        }
        
        

        private async Task CycleSend(string selClient,string content, bool isAddNewline)
        {
            string newLine = isAddNewline ? "\r\n" : "";

            do
            {
                await Task.Delay(SendCycleTime);
                if (SendFrameFormat == FrameFormat.Hex)
                {
                    var parsed = EncodeUtil.ConvertHexStringToByteEnumerable(hexLongString: content, separator: " ")
                        .Concat(EncodeUtil.ConvertUtf8StringToByteArray(newLine)).ToArray();
                    await _server.WriteTo(selClient, parsed, new CancellationToken());
                    if(IsOutputAsLog)
                    RaiseLogOutput($"Send:{EncodeUtil.HexArrayToString(parsed, " ")}");
                }
                else
                {
                    await _server.WriteTo(selClient, content: content + newLine, token: new CancellationToken());
                    if (IsOutputAsLog)
                        RaiseLogOutput($"Send: {content + newLine}");
                }

                
            } while (IsSendAtRegularTime);
        }



        #endregion

        
        #region  IOutputVariantData  & logMethods

        public event LogHandler OnLogData;
        public event EventHandler OnOutputEmpty;


        private void RaiseOutputReceivedData(ComReceivedEventArg args)
        {
            if (args.Contract != SelClient)
                return;

            string parsedContent = ReceiveFrameFormat == FrameFormat.Hex ?
                EncodeUtil.HexArrayToString(args.Data.ToArray(), " ") :
                EncodeUtil.ConvertBackUtf8String(args.Data.ToArray());
            if (IsOutputAsLog)
            {
                OnLogData?.Invoke($"[{DateTime.Now}]: ", LogLevel.Other);
                OnLogData?.Invoke(parsedContent + "\r",LogLevel.Info);
            }
            else
                OnLogData?.Invoke(parsedContent, LogLevel.Info);
        }
        private void RaiseLogOutput(string content)
        {
            OnLogData?.Invoke($"[{DateTime.Now}]: {content}\r", LogLevel.Debug);
        }
        
        
        private void RaiseErrorOutput(Exception e)
        {
            string content = _communicationModuleSetting.IsDebugMode ? e.ToString() : e.Message;
            OnLogData?.Invoke($"[{DateTime.Now}](Rank:Error): {content}\r", LogLevel.Error);
        }

        private void RaiseReceivedErrorOutput(ComErrorEventArg e)
        {
            string content = _communicationModuleSetting.IsDebugMode ? e.Exception.ToString() : e.Exception.Message;
            OnLogData?.Invoke($"[{DateTime.Now}](Rank:Error):{e.Contract} : {content}\r", LogLevel.Error);
        }

        #endregion

        #region IActiveAware

        private bool _isActive;
        private bool _canOpenState;
        

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnIsActiveChanged();
            }
        }
        private void OnIsActiveChanged()
        {
            SaveCommand.IsActive = IsActive;
            IsActiveChanged?.Invoke(this, new EventArgs());
        }
        public event EventHandler IsActiveChanged;
        #endregion

        #region IAppDataApply
        public void ApplyAppData(TcpDebuggerSetting appData)
        {
            
            IsOutputAsLog = appData.IsOutAsLog;
            SendFrameFormat = appData.SendFrameFormat;
            ReceiveFrameFormat = appData.ReceiveFrameFormat;
            IsAddNewLineWhenSend = appData.IsAddNewLineWhenSend;
            IsSendAtRegularTime = appData.IsSendAtRegularTime;
            SendCycleTime = appData.SendCycleTime;
            SelNetType = appData.NetType;
            SelIPAddress = appData.IPAddress; 
            Port= appData.Port;
        }

        public async void SaveAppData(TcpDebuggerSetting appData)
        {
            try
            {
                //todo :extract these 
                appData.IsOutAsLog = IsOutputAsLog;
                appData.SendFrameFormat = SendFrameFormat;
                appData.ReceiveFrameFormat = ReceiveFrameFormat;
                appData.IsAddNewLineWhenSend = IsAddNewLineWhenSend;
                appData.IsSendAtRegularTime = IsSendAtRegularTime;
                appData.SendCycleTime = SendCycleTime;
                appData.NetType = SelNetType;
                appData.IPAddress = SelIPAddress;
                appData.Port = Port;
                
                await _manager.SaveToDataSourceAsync(appData);
                _messageService.Notice($" NOTICE:   {appData.Contract} saved");
            }
            catch (Exception ex)
            {
                _messageService.Notice(ex.Message);
            }
        }

#endregion
    }
    
    
    
}
