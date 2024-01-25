using System;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Bee.Core.DataSource;
using Bee.Core.Utils;
using Bee.Modules.Communication.Setting;
using Bee.Modules.Communication.Shared;
using Bee.Services.Interfaces;
using Polaris.Connect.Tool.Base;
using Polaris.Connect.Tool.SerialPort;
using Prism;
using Prism.Commands;
using System.Reflection.Metadata;
using Bee.Core.Controls;
using Polaris.Connect.Tool;

namespace Bee.Modules.Communication.ViewModels
{
    

    internal class ComUsartDebuggerViewModel:BindableBase,IEasyLoggingBindView, IActiveAware
    {

        private readonly ComSerialPort _comSerialPort;
        private readonly IMessageService _messageService;
        private readonly ScopedAppDataSourceManager _manager;

        private string[] _comNames;
        private string _selectedCom;
        private CustomNumItem _selectedBaudRate;
        private CustomNumItem _selectedDataBit;

        public string[] ComNames
        {
            get => _comNames;
            set => SetProperty(ref _comNames, value);
        }

        public string SelectedCom
        {
            get => _selectedCom;
            set => SetProperty(ref _selectedCom, value);
        }


        public CustomNumItem[] BaudRates { get;  }=
            new CustomNumItem[]
        {
            new CustomNumItem {Unit = "bps",IsStandard = false} ,
            new CustomNumItem(){Num = 300, Unit = "bps"},
            new CustomNumItem(){Num = 1200, Unit = "bps"},
            new CustomNumItem(){Num = 2400, Unit = "bps"},
            new CustomNumItem(){Num = 4800, Unit = "bps"},
            new CustomNumItem(){Num = 9600, Unit = "bps"},
            new CustomNumItem(){Num = 19200, Unit = "bps"},
            new CustomNumItem(){Num = 38400, Unit = "bps"},
            new CustomNumItem(){Num = 57600, Unit = "bps"},
            new CustomNumItem(){Num =115200, Unit = "bps"},
        };

        public CustomNumItem SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => SetProperty(ref _selectedBaudRate, value);
        }


        public string[] StopBits { get; } = new string[] { "One" , "OnePointFive", "Two" };

        public string SelectedStopBit { get; set; }

        public CustomNumItem[] DataBits { get; } = new CustomNumItem[]
        {
            new CustomNumItem { Unit = "bit", IsStandard = false  },
            new CustomNumItem() { Num = 8, Unit = "bit" },
            new CustomNumItem() { Num = 7, Unit = "bit" },
            new CustomNumItem() { Num = 6, Unit = "bit" },
            new CustomNumItem() { Num = 5, Unit = "bit" },
        };

        public CustomNumItem SelectedDataBit
        {
            get => _selectedDataBit;
            set => SetProperty(ref _selectedDataBit, value);
        }

        public string[] Parities { get; }= new string[]
        {
            "None", 
            "Odd", 
            "Even",
            "Mark",
            "Space"
        };

        
        private readonly SerialPortDebuggerSetting _spDebuggerSetting;
        private readonly CommunicationModuleSetting _moduleSetting;
        
        
        private bool _openState=true;
        private FrameFormat _sendFrameFormat;
        private FrameFormat _receiveFrameFormat;

        public string SelectedParity { get; set; }

        public DelegateCommand OpenCommand { get; }
        public DelegateCommand CloseCommand { get; } 
public DelegateCommand RefreshPortCommand { get;}
public DelegateCommand SaveCommand { get; }
//public DelegateCommand EmptyReceivedCommand { get; }
public DelegateCommand EmptyInputCommand { get; }
public DelegateCommand SendCommand { get; }


public ComUsartDebuggerViewModel(
            ComManager comRegistry,
            ComSerialPort serialPort, 
            AppDataCache appDataCache,
            IMessageService messageService,
            ScopedAppDataSourceManager manager,
            ComModuleCommand comModuleCommand
)
        {
            _comSerialPort = serialPort;
            _messageService = messageService;
            _manager = manager;

            comRegistry.Register("SerialPort",serialPort);

            _moduleSetting = appDataCache.GetMapping<CommunicationModuleSetting>();
            _spDebuggerSetting = appDataCache.GetMapping<SerialPortDebuggerSetting>();
            


            OpenCommand = new DelegateCommand(Open, CanOpen).ObservesProperty(() => SelectedCom).
                ObservesProperty(() => SelectedDataBit).ObservesProperty(() => SelectedBaudRate);

            CloseCommand = new DelegateCommand(Close);

            RefreshPortCommand = new DelegateCommand(RefreshPort);
            SaveCommand = new DelegateCommand(SaveSetting, CanSave).
                ObservesProperty(() => SelectedDataBit).ObservesProperty(() => SelectedBaudRate);

            //EmptyReceivedCommand = new DelegateCommand(EmptyReceive); 
            EmptyInputCommand= new DelegateCommand(EmptyInput);
            SendCommand = new DelegateCommand(()=>SendMessage(InputData),()=>!CanOpenState && !IsStillSending).ObservesProperty(()=> CanOpenState).ObservesProperty(()=>IsStillSending);
            comModuleCommand.SaveCommand.RegisterCommand(SaveCommand);

            InitializeSetting(_spDebuggerSetting);


            _comSerialPort.WhenDataReceived +=
                (s, d) =>
                    RaiseOutputReceivedData(d);
            
            _comSerialPort.WhenReceivedExceptionOccur += (s, e) =>
            {
                RaiseReceivedErrorOutput(e);
            };
            _comSerialPort.WhenConnectChanged += (s, e) =>
            {
                RaiseLogOutput(e.ComDetails + (e.State == ComState.Connect ? " Connect" : " Disconnect"));
                RefreshPortCommand.Execute();
                CanOpenState = e.State != ComState.Connect;
            };
            
        }


        public  bool IsOutputAsLog { get; set; }

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

        public bool IsAddNewLineWhenSend { get; set; }
        public bool IsSendAtRegularTime { get; set; }
        public int SendCycleTime { get; set; }

        public string InputData
        {
            get => _inputData;
            set => SetProperty(ref _inputData, value);
        }


        public event LogHandler OnLogData;
        public event EventHandler OnOutputEmpty;


        public bool CanOpenState
        {
            get => _openState;
            set => SetProperty(ref _openState, value);
        }

        private void RefreshPort()
        {
            ComNames = ComSerialPort.PortNameList; 
        }


        private void Open()
        {
            try
            {
                _comSerialPort.Open(SelectedCom, (int)SelectedBaudRate.Num!, (int)SelectedDataBit.Num!,
                    Enum.Parse<StopBits>(SelectedStopBit), Enum.Parse<Parity>(SelectedParity),
                    true);
            }
            catch(Exception ex)
            {
                RaiseErrorOutput(ex);
            }
        }

        private bool CanOpen()
        {
            return SelectedCom != null && SelectedBaudRate.Num != null && SelectedDataBit.Num != null;
        }

        private void Close()
        {
            try
            {
                _comSerialPort.Close();
            }
            catch (Exception ex)
            {
                RaiseErrorOutput(ex);
            }
            CanOpenState = true;
        }

        //private void EmptyReceive()
        //{
        //    OnOutputEmpty?.Invoke(this,EventArgs.Empty);
        //}

        private void EmptyInput()
        {
            //OnInputEmpty?.Invoke(this, EventArgs.Empty);
            InputData = "";
        }


        private void RaiseOutputReceivedData(ComReceivedEventArg arg)
        {

            
            string parsedContent =ReceiveFrameFormat==FrameFormat.Hex?
                EncodeUtil.HexArrayToString(arg.Data.ToArray()," ") :
                EncodeUtil.ConvertBackUtf8String(arg.Data.ToArray());
            if (IsOutputAsLog)
            {
                   OnLogData?.Invoke($"[{DateTime.Now}]: ", LogLevel.Other);
                OnLogData?.Invoke(parsedContent +"\r", LogLevel.Info);
            }
            else
                OnLogData?.Invoke(parsedContent,LogLevel.Info);
        }
        private void RaiseLogOutput(string content)
        {
            OnLogData?.Invoke($"[{DateTime.Now}]: {content}\r", LogLevel.Debug);
        }


        private void RaiseErrorOutput(Exception e)
        {
            string content = _moduleSetting.IsDebugMode ? e.ToString() : e.Message;
            OnLogData?.Invoke($"[{DateTime.Now}](Rank:Error): {content}\r", LogLevel.Error);
        }

        private void RaiseReceivedErrorOutput(ComErrorEventArg e)
        {
            string content = _moduleSetting.IsDebugMode ? e.Exception.ToString() : e.Exception.Message;
            OnLogData?.Invoke($"[{DateTime.Now}](Rank:Error): {e.Contract}: {e.Exception}\r",LogLevel.Error);
        }

        private  void InitializeSetting(SerialPortDebuggerSetting debuggerSetting)
        {
            RefreshPortCommand.Execute();
            
            SelectedCom = ComNames.FirstOrDefault(t => t == debuggerSetting.ComName)?? debuggerSetting.ComName;
            if ((SelectedBaudRate =BaudRates.FirstOrDefault(t => t.Num == debuggerSetting.BaudRate))==null)
            {
                BaudRates[0].Num = debuggerSetting.BaudRate;
                SelectedBaudRate= BaudRates[0];
            }
            
            
            SelectedStopBit = StopBits.FirstOrDefault(t=>t==debuggerSetting.StopBit.ToString());

            if ((SelectedDataBit = DataBits.FirstOrDefault(t => t.Num == debuggerSetting.DataBit)) == null)
            {
                DataBits[0].Num = debuggerSetting.DataBit;
                SelectedDataBit = DataBits[0];
            }

            SelectedParity = Parities.FirstOrDefault(t=>t==debuggerSetting.Parity.ToString());
            IsOutputAsLog = debuggerSetting.IsOutAsLog;
            SendFrameFormat = debuggerSetting.SendFrameFormat;
            ReceiveFrameFormat=debuggerSetting.ReceiveFrameFormat;
IsAddNewLineWhenSend=debuggerSetting.IsAddNewLineWhenSend;
IsSendAtRegularTime=debuggerSetting.IsSendAtRegularTime;
SendCycleTime=debuggerSetting.SendCycleTime;

        }

        private async void SaveSetting()
        {
            try
            {
                _spDebuggerSetting.ComName = SelectedCom ?? "";
                _spDebuggerSetting.BaudRate = (int)SelectedBaudRate.Num!;
                _spDebuggerSetting.StopBit = Enum.Parse<StopBits>(SelectedStopBit);
                _spDebuggerSetting.DataBit = (int)SelectedDataBit.Num!;
                _spDebuggerSetting.Parity = Enum.Parse<Parity>(SelectedParity);
                _spDebuggerSetting.IsOutAsLog = IsOutputAsLog;
                _spDebuggerSetting.SendFrameFormat= SendFrameFormat;
                _spDebuggerSetting.ReceiveFrameFormat= ReceiveFrameFormat;
                _spDebuggerSetting.IsAddNewLineWhenSend = IsAddNewLineWhenSend;
                _spDebuggerSetting.IsSendAtRegularTime = IsSendAtRegularTime; 
                _spDebuggerSetting.SendCycleTime= SendCycleTime;
                await _manager.SaveToDataSourceAsync(_spDebuggerSetting);
                _messageService.Notice($" NOTICE:   {_spDebuggerSetting.Contract} saved");
            }
            catch (Exception ex)
            {
                _messageService.Notice(ex.Message);
            }
        }

        private async void SendMessage(string content)
        {
            try
            {
                await CycleSend(content,IsAddNewLineWhenSend); 
            }
            catch (Exception e)
            {
                RaiseErrorOutput(e); 
            }
            
        }


        public bool IsStillSending
        {
            get => _isStillSending;
            set => SetProperty(ref _isStillSending, value);
        }

        private async Task CycleSend(string content,bool isAddNewline)
        {
            string newLine = isAddNewline ? "\r\n" : "";
            IsStillSending = true;
            do
            {
                await Task.Delay(SendCycleTime);
                if (SendFrameFormat == FrameFormat.Hex)
                {
                    var parsed = EncodeUtil.ConvertHexStringToByteEnumerable(hexLongString: content, separator: " ")
                        .Concat(EncodeUtil.ConvertUtf8StringToByteArray(newLine)).ToArray();
                    await _comSerialPort.WriteAsync(
                        parsed,
                        token: new CancellationToken());
                    if (IsOutputAsLog)
                        RaiseLogOutput($"Send:{EncodeUtil.HexArrayToString(parsed," ")}");
                }
                else
                {
                    await _comSerialPort.WriteAsync(content: content + newLine, token: new CancellationToken());
                    if (IsOutputAsLog)
                        RaiseLogOutput($"Send: {content+newLine}");
                }

                
                
                
            } while (IsSendAtRegularTime);

                IsStillSending = false;

        }



        private bool CanSave ()
        {
             return SelectedBaudRate.Num != null && SelectedDataBit.Num != null;
        }


        bool _isActive;
        private bool _isStillSending;
        private string _inputData;

        public  bool IsActive
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
        //public event EventHandler OnInputEmpty;
    }
}
