using Prism.Commands;
using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Common;
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
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Exception = System.Exception;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Bee.Core.Controls;

namespace Bee.Modules.Script.ViewModels
{
    public class ScriptDebuggerViewModel : BindableBase, INavigationAware, IOutputDataOnRichTextBox
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

        #endregion


        #region Commands for binding

        public DelegateCommand RefreshPortsCommand { get; }
        public DelegateCommand ImportInstructionSetCommand { get; }
        public DelegateCommand BuildScriptCommand { get; }


        #endregion



        #region core

        private readonly ComManager _manager;
        private readonly ScopedAppDataSourceManager _scopedAppDataSourceManager;
        private readonly AskDialogHost _dialogHost;
        private ICom _com;
        private List<ProtocolFormat> _protocolFormats;
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
        
        private async Task LoadInstructionList(string[] instructionPaths, List<ProtocolFormat> protocolFormats,
            CancellationToken token)
        {
            if (instructionPaths == null)
                return;

            if (protocolFormats == null)
                protocolFormats = new List<ProtocolFormat>();
            else protocolFormats.Clear();

            var list = new List<string>();
            foreach (var path in instructionPaths)
            {
                var setting = await JsonFile.GetDataAsync<InstructionSetting>(path, token);
                protocolFormats.AddRange(setting?.Protocols ?? Array.Empty<ProtocolFormat>());
                list.Add(setting?.Name);
            }

            InstructionSets = list.ToArray();
        }
        
        
        private bool _initOnceFlag;
        private CancellationTokenSource _cts;
        private string[] _instructionSets;

        private async Task WaitLoadingCore(CancellationToken token)
        {
            try
            {
                await Task.WhenAll(Task.Delay(500,token), LoadInstructionList(_scriptDebuggerSetting.InstructionList, _protocolFormats, token)) ;
                
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
    RaiseLogOutput("Loading Step Over");
        }

private void BuildScript()
{
     
}



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
        
        #region IOutputVariantData

        public bool IsOutputAsLog { get; set; }
        public event OutputDataOnRichTbxHandler OnOutputVariantData;
        public event EventHandler OnOutputEmpty;

        private void RaiseLogOutput(string content)
        {
            OnOutputVariantData?.Invoke($"[{DateTime.Now}]: {content}\r", Brushes.Blue);
        }


        private void RaiseErrorOutput(Exception e)
        {
            OnOutputVariantData?.Invoke($"[{DateTime.Now}](Rank:Error): {e.Message}\r", Brushes.Red);
        }

        #endregion

    }

}
