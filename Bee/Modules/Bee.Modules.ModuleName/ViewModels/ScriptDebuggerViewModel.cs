using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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

namespace Bee.Modules.Script.ViewModels
{
    public class ScriptDebuggerViewModel : BindableBase, INavigationAware
    {

        #region  Fileds for xaml binding

        public ObservableCollection<string> ActiveComs
        {
            get => _activeComs;
            set => SetProperty(ref _activeComs, value);
        }

        public string SelectedCom
        {
            get => _selectedCom;
            set => SetProperty(ref _selectedCom, value, () => { SelectedActiveComChanged(value);});
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
        #endregion


        #region Commands for binding

        public DelegateCommand RefreshPortsCommand { get; }

        #endregion



        #region  core

        private readonly ComManager _manager;
        private readonly ScopedAppDataSourceManager _scopedAppDataSourceManager;
        private  ICom _com;
        private List<ProtocolFormat> _protocolFormats;
        private readonly  ModuleSetting _moduleSetting;
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

            };
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
            if(_com!=null)
            ActivePorts = _com.ActiveClients.ToArray();
        }


        private async Task ImportInstructionFiles(ModuleSetting moduleSetting, ScriptDebuggerSetting thisSetting)
        {
            //open the file explorer

            var dialog = FileBrowserUtil.CreateOpenFileExplorer(moduleSetting.SavePath);
            if (dialog.ShowDialog() is { } result)
            {
                if (result)
                {
                    await LoadInstructionList(dialog.FileNames,_protocolFormats); 
                }
            }

            setting.InstructionList= 
        }


        private async Task LoadInstructionList(string[] instructionPaths, List<ProtocolFormat> protocolFormats)
        {
            if (instructionPaths == null)
                return;

            if (protocolFormats == null)
                protocolFormats = new List<ProtocolFormat>();
            else protocolFormats.Clear();
            
            foreach (var path in instructionPaths)
            {
                var setting=await JsonFile.GetDataAsync<InstructionSetting>(path, new CancellationToken());
                protocolFormats.AddRange(setting?.Protocols??Array.Empty<ProtocolFormat>());
            }
        }

        private void SaveInstructionList()
        {
            
        }

        #endregion





        public ScriptDebuggerViewModel(
            ComManager manager,
            AppDataCache appDataCache,
            ScopedAppDataSourceManager scopedAppDataSourceManager
        )
        {
            _manager = manager;
            _scopedAppDataSourceManager = scopedAppDataSourceManager;
            ActiveComs =new ObservableCollection<string>(_manager.ActiveComs);
            _manager.ComMappingChanged += ManagerComMappingChanged;
            RefreshPortsCommand = new DelegateCommand(ManualRefreshActivePorts);
            
            
        }


        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                await LoadInstructionList(_scriptDebuggerSetting.InstructionList, _protocolFormats);
            }
            catch(Exception ex)
            {
                
            }


        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
    }
}
