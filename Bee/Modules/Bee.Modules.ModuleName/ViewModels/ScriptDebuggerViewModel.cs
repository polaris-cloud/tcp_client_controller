using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bee.Core.DataSource;
using Bee.Modules.Script.Settings;
using Polaris.Connect.Tool;
using Polaris.Connect.Tool.Base;
using Polaris.Protocol.Model;
using Polaris.Storage.Json;
using Polaris.Storage.WinSystem;
using Prism.Regions;
using Exception = System.Exception;
using Bee.Core.Controls;
using ICSharpCode.AvalonEdit.Document;
using Polaris.Protocol.Parser;
using Bee.Core.Utils;
using Bee.Modules.Script.Shared.Advance;

namespace Bee.Modules.Script.ViewModels
{


    public class ProtocolBuildInfo
    {
        public string DebugInfo;
        public byte[] RealBytes;
    public ProtocolScriptParser Parser;
    }

    public class ScriptDebuggerViewModel : BindableBase, INavigationAware, IEasyLoggingBindView
    {
        
        #region Fileds for xaml binding

        public ObservableCollection<string> ActiveComs
        {
            get => _activeComs;
            set => SetProperty(ref _activeComs, value);
        }

        public string SelectedCom
        {
            get => _selectedCom;
            set => SetProperty(ref _selectedCom, value, () => { SelectedActiveComChanged(value); });
        }

        public string[] ActivePorts
        {
            get => _activePorts;
            set => SetProperty(ref _activePorts, value);
        }

        public string SelectedPort
        {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
        }

        private ObservableCollection<string> _activeComs;
        private string[] _activePorts;
        private string _selectedCom;
        private string _selectedPort;
        
        
        public string[] InstructionSets
        {
            get => _instructionSets;
            set => SetProperty(ref _instructionSets, value);
        }

        public bool CanBuildFlag
        {
            get => _canBuildFlag;
            set => SetProperty(ref _canBuildFlag, value);
        }

        #endregion


        #region Commands for binding


        public DelegateCommand RefreshPortsCommand { get; }
        public DelegateCommand ImportInstructionSetCommand { get; }
        public DelegateCommand<TextDocument> BuildScriptCommand { get; }
        public DelegateCommand StopBuildScriptCommand { get; }
        public DelegateCommand<TextDocument> RunScriptCommand { get; }

        private async void BuildScriptMethod(TextDocument document)
        {
            try
            {
                CanBuildFlag = false;
                await BuildScriptAsync(document);
            }
            catch (Exception e)
            {
                RaiseErrorOutput(e);
            }
            
            CanBuildFlag=true;
        }

        private void StopBuildMethod()
        {
            StopBuildScript();
            CanBuildFlag = true; 
        }

        private async void RunMethod(TextDocument document)
        {
            try
            {
                if (await BuildScriptAsync(document))
                    await RunScripts();

            }
            catch (Exception e)
            {
                RaiseErrorOutput(e);
            }
        }

        #endregion


        #region core

        private readonly ComManager _manager;
        private readonly ScopedAppDataSourceManager _scopedAppDataSourceManager;
        private readonly AskDialogHost _dialogHost;
        private ICom _com;

        public List<ProtocolFormat> ProtocolFormats
        {
            get => _protocolFormats;
            set =>SetProperty(ref _protocolFormats, value); 
            }
            
        
        
        

        private readonly ModuleSetting _moduleSetting;
        private readonly ScriptDebuggerSetting _scriptDebuggerSetting;

        private void ManagerComMappingChanged(object? sender, ComMappingChangedEventArg e)
        {

            switch (e.State)
            {
                case MappingChangedState.Add:
                    ActiveComs.Add(e.Contract);
                    break;
                case MappingChangedState.Remove:
                    ActiveComs.Remove(e.Contract);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }

            ;
        }

        private void SelectedActiveComChanged(string newValue)
        {
            if (newValue == null)
            {
                _com = null;
                ActivePorts = null;
            }


            _com = _manager.GetComMapping(newValue);
            ActivePorts = _com.ActiveClients.ToArray();
        }

        private void ManualRefreshActivePorts()
        {
            if (_com != null)
                ActivePorts = _com.ActiveClients.ToArray();
        }


        private async void ImportInstructionFiles()
        {
            //open the file explorer
            string[] safeCache=Array.Empty<string>();
            try
            {
                var dialog = FileBrowserUtil.CreateOpenFileExplorer(_moduleSetting.SavePath);
                safeCache = _scriptDebuggerSetting.InstructionList; 
                if (dialog.ShowDialog() is { } result)
                {
                    if (result)
                    {
                        _scriptDebuggerSetting.InstructionList = dialog.FileNames;
                        await WaitLoadInstruction();
                        await _scopedAppDataSourceManager.SaveToDataSourceAsync(_scriptDebuggerSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                _scriptDebuggerSetting.InstructionList = safeCache;
                RaiseErrorOutput(ex);
            }
            
            }
        
        private async Task LoadInstructionList(string[] instructionPaths,
            CancellationToken token)
        {
            if (instructionPaths == null)
                return;

            
                var protocolFormats = new List<ProtocolFormat>();
            

            var list = new List<string>();
            foreach (var path in instructionPaths)
            {
                var setting = await JsonFile.GetDataAsync<InstructionSetting>(path, token);
                protocolFormats.AddRange(setting?.Protocols ?? Array.Empty<ProtocolFormat>());
                list.Add(setting?.Name);
                RaiseDebugOutput($"Load file :{setting?.Name}");
            }

            InstructionSets = list.ToArray();
            ProtocolFormats = protocolFormats; 
        }
        
        
        private bool _initOnceFlag;
        private CancellationTokenSource _cts;
        private string[] _instructionSets;
        private List<ProtocolFormat> _protocolFormats;
        private ProtocolBuildInfo[] _buildScripts;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _canBuildFlag;

        private async Task WaitLoadingCore(CancellationToken token)
        {
            try
            {
                await Task.WhenAll(Task.Delay(500,token), LoadInstructionList(_scriptDebuggerSetting.InstructionList, token)) ;
            }
            catch (Exception e)
            {
                RaiseErrorOutput(e);
            }
            
        }
        
private async Task WaitLoadInstruction()
{
    _cts = new CancellationTokenSource();
    await _dialogHost.ShowLoadingDialog("Instructions...", WaitLoadingCore, _cts);
    RaiseDebugOutput("Loading Step Over");
        }


#region build Script methods


        private IEnumerable<Tuple<int,string,ProtocolFormat>> GetAllInstructionsFrom(TextDocument document,IDictionary<string ,ProtocolFormat> dic)
{
    
    foreach (var line in document.Lines)
    {
        string lineText = document.GetText(line);
        string orderHead = lineText.Split('>')[0];
        if (!dic.TryGetValue(orderHead, out ProtocolFormat protocolFormat))
        {
            throw new ArgumentException($"Line {line.LineNumber} : Protocol Script Format Error");
                }

        yield return new Tuple<int ,string, ProtocolFormat>(line.LineNumber,lineText.Trim(),protocolFormat);
    }
}

private  ProtocolBuildInfo ParseSingleProtocol(string complete, ProtocolFormat protocolFormat)
{
    
        ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(protocolFormat);
        var cache = protocolScriptParser.GenerateSendFrame(complete, out string debug);
        //RaiseStandardOutput(debug);
        return new ProtocolBuildInfo()
        {
            DebugInfo = debug ,
            RealBytes = cache,
    Parser= protocolScriptParser
        };
        
}

/// <summary>
/// 
/// </summary>
/// <param name="document"></param>
/// <returns></returns>
/// <exception cref="ArgumentException"></exception>
private async Task<bool> BuildScriptAsync(TextDocument document)
{
    
    
        if(string.IsNullOrEmpty(document.Text))
              return false;
        _cancellationTokenSource = new CancellationTokenSource();
        // 获取当前document中的所有指令
        var dic = ProtocolFormats.ToDictionary(p => p.BehaviorKeyword, p => p);
        var protocolFormatsWithComplete = GetAllInstructionsFrom(document, dic).ToArray();
        // 将所以指令并行解析为<key,byte[]>，此时的顺序打乱了
        _buildScripts = new ProtocolBuildInfo[protocolFormatsWithComplete.Length];
        bool[] errors = new bool[protocolFormatsWithComplete.Length]; 
        await Parallel.ForAsync(0, _buildScripts.Length, _cancellationTokenSource.Token, async (index, token) =>
        {
            var p = protocolFormatsWithComplete[index];
            // 顺序不变
            try
            {
                _buildScripts[index] = await Task.Run(() => ParseSingleProtocol(p.Item2, p.Item3), token);
            }
            catch(Exception e)
            {
                RaiseErrorOutput($"Line {p.Item1} : Protocol Script Format Error \r {e.Message}");
                errors[index] = true;
            }
            
        });
        var count = errors.Count(b => b);
        RaiseDebugOutput($" Finished building project ,Error : {count}");
                //根据指令的顺序进行重新排列（已经用数组实现）
                return count ==0;
}

private void StopBuildScript()
{
    _cancellationTokenSource.Cancel();
}
        #endregion

        #region  Run Script methods


        private async Task RunScripts()
        {
            var control=AutoSynProtocolComQueueWrapper.BuildWrapper(_com);
            var cts = new CancellationTokenSource();
            foreach (var script in _buildScripts)
            {
                RaiseStandardOutput(script.DebugInfo);
RaiseLogOutput(EncodeUtil.HexArrayToString(script.RealBytes, " "));
                var receiveInfo= await control.Post(new ProtocolSendInfo {
                    Connection = SelectedPort,
                    Content = script.RealBytes,
                    Timeout = 10, 
                Parser=script.Parser
                },cts.Token);
                RaiseStandardOutput(receiveInfo.Debug);
                RaiseLogOutput(receiveInfo.Raw);
            }
            control.Close();
        }


        #endregion




        #endregion



        public ScriptDebuggerViewModel(
            ComManager manager,
            AppDataCache appDataCache,
            ScopedAppDataSourceManager scopedAppDataSourceManager,
            AskDialogHost dialogHost
        )
        {
            _manager = manager;
            _scopedAppDataSourceManager = scopedAppDataSourceManager;
            _dialogHost = dialogHost;
            ActiveComs = new ObservableCollection<string>(_manager.ActiveComs);
            _moduleSetting = appDataCache.GetMapping<ModuleSetting>();
            _scriptDebuggerSetting = appDataCache.GetMapping<ScriptDebuggerSetting>(); 
            _manager.ComMappingChanged += ManagerComMappingChanged;
            RefreshPortsCommand = new DelegateCommand(ManualRefreshActivePorts);
            ImportInstructionSetCommand = new DelegateCommand(ImportInstructionFiles);
            BuildScriptCommand = new DelegateCommand<TextDocument>(BuildScriptMethod);
            StopBuildScriptCommand=new DelegateCommand(StopBuildMethod);
            RunScriptCommand = new DelegateCommand<TextDocument>(RunMethod);
            CanBuildFlag = true;
        }

        #region  INavigationAware
        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (_initOnceFlag) return;

            await WaitLoadInstruction();
            _initOnceFlag = true;
        }




        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }


        #endregion

        #region IEasyLoggingBindView

        public event LogHandler OnLogData;

        private void RaiseDebugOutput(string content)
        {
            OnLogData?.Invoke($"[{DateTime.Now}]: {content}\r", LogLevel.Debug);
        }

        private void RaiseStandardOutput(string content)
        {
            OnLogData?.Invoke($"[{DateTime.Now}]: {content}\r", LogLevel.Info);
        }
        

        private void RaiseErrorOutput(Exception e)
        {
            OnLogData?.Invoke($"[{DateTime.Now}](Rank:Error): {e.Message}\r", LogLevel.Error);
        }

        private void RaiseErrorOutput(string content)
        {
            OnLogData?.Invoke($"[{DateTime.Now}](Rank:Error): {content}\r", LogLevel.Error);
        }

        private void RaiseLogOutput(string content)
        {
            OnLogData?.Invoke($"[{DateTime.Now}] : {content}\r", LogLevel.Other);
        }

        #endregion

    }

}
