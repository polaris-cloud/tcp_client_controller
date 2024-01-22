using Prism.Commands;
using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Bee.Core.DataSource;
using Bee.Core.Json;
using Bee.Modules.Script.Settings;
using Bee.Modules.Script.Shared;
using Bee.Services.Interfaces;
using Polaris.Protocol.Model;
using Polaris.Storage.Json;
using Newtonsoft.Json.Linq;
using Polaris.Storage.WinSystem;
using Exception = System.Exception;

namespace Bee.Modules.Script.ViewModels
{
    public class ScriptEditorViewModel : BindableBase
    {



        #region fields For xaml Binding

        public ObservableCollection<ProtocolFormat> Protocols
        {
            get => _protocols;
            set
            {
                if (_protocols != value  &&  _protocols!=null)
                {
                    _protocols.CollectionChanged -= _protocolsChangedHandler;
                }
                SetProperty(ref _protocols, value, () => { OnProtocolCollectionCreated(value); });
            }
            }
        

        public ProtocolFormat SelectedProtocolFormat
        {
            get => _selectedProtocolFormat;
            set => SetProperty(ref _selectedProtocolFormat, value);
        }

        public bool IsProtocolsEmpty
        {
            get => _isProtocolsEmpty;
            set => SetProperty(ref _isProtocolsEmpty, value);
        }

        public int ModifyNum
        {
            get => _modifyNum;
            set => SetProperty(ref _modifyNum, value);
        }

        //public ObservableCollection<string> ModifiedItems
        //{
        //    get => _modifiedItems;
        //    set => SetProperty(ref _modifiedItems, value);
        //}

        public string InstructionName
        {
            get => _instructionName;
            set => SetProperty(ref _instructionName, value);
        }

        public string InstructionPath
        {
            get => _instructionPath;
            set => SetProperty(ref _instructionPath, value);
        }

        private ObservableCollection<ProtocolFormat> _protocols;
        private ProtocolFormat _selectedProtocolFormat;
        private bool _isProtocolsEmpty;

        private int _modifyNum;

        //        private ObservableCollection<string> _modifiedItems;
        private string _instructionName;
        private string _instructionPath;

        #endregion

        #region commands For xaml Binding

        public DelegateCommand CreateInstructionCommand { get; }
        public DelegateCommand DeleteInstructionCommand { get; }
        public DelegateCommand SaveItemsCommand { get; }
        public DelegateCommand OpenExistInstructionSetCommand { get; }
        public DelegateCommand CreateInstructionSetCommand { get; }

        private async void CreateInstruction()
        {
            if (string.IsNullOrEmpty(InstructionName))
                return;
            
            var result = await _dialogHost.ShowInputDialog("添加指令");
            if (!result.ButtonResult)
                return;
            if (!string.IsNullOrWhiteSpace(result.Output))
            {
                string parse = result.Output.Trim();
                if (Protocols.Select(p => p.BehaviorKeyword).Contains(parse))
                    return;
                var newFormat = new ProtocolFormat() { BehaviorKeyword = parse };
                Protocols.Add(newFormat);
            }

        }

        private async void DeleteItem()
        {
            if (string.IsNullOrEmpty(InstructionName))
                return;

            if (Protocols.Count == 0) return;
            if (await _dialogHost.ShowDialog("是否删除这条指令 ?"))
            {
                Protocols.Remove(SelectedProtocolFormat);
            }
        }

        
        
        
        
        private async void SaveInstructionItems()
        {
            try
            {
                await SaveInstructionSet();
                _messageService.Notice($"{InstructionName} 已保存");
            }
            catch (Exception e)
            {
                _messageService.Notice(e.Message);
            }


        }


        private async void OpenExistInstructions()
        {
            try
            {
                if (ModifyNum > 0)
                    await SaveInstructionSet(); 
                await  OpenNewInstructionSet();
            }
            catch (Exception e)
            {
                _messageService.Notice(e.Message);
            }
            
        }

        private async void CreateNewInstructionSet()
        {
            try
            {
                if (ModifyNum > 0)
                    await SaveInstructionSet();
                await CreateInstructionSet();
            }
            catch (Exception e)
            {
                _messageService.Notice(e.Message);
            }
        }

        #endregion


        #region Core  methods or others

        private readonly AskDialogHost _dialogHost;
        private readonly IMessageService _messageService;
        private readonly ModuleSetting _moduleSetting;



        NotifyCollectionChangedEventHandler _protocolsChangedHandler ;

        private void OnProtocolCollectionCreated(ObservableCollection<ProtocolFormat> newValue)
        {
            newValue.CollectionChanged += _protocolsChangedHandler;
            if(newValue.Count>0)
                SelectedProtocolFormat = newValue[0];
            IsProtocolsEmpty = newValue.Count == 0;
            ModifyNum = 0;
        }


        

        private async Task  CreateInstructionSet()
        {

            var re = await _dialogHost.ShowInputDialog
                ("输入指令集名称:");
            if (!re.ButtonResult)
                return;

            var dialog = FileBrowserUtil.CreateSaveFileExplorer(_moduleSetting.SavePath);
            if (dialog.ShowDialog() is { } result)
            {
                if (result)
                {
                    string filter = re.Output.Trim(); 
                    await JsonFile.SaveDataAsync(new InstructionSetting()
                    {
                        Name = filter,
                        Protocols = Array.Empty<ProtocolFormat>()
                    }, dialog.FileName, new CancellationToken());

                    InstructionName = filter;
                    InstructionPath = dialog.FileName;
                    Protocols = new ObservableCollection<ProtocolFormat>();
                }
            }
            ModifyNum = 0;
        }

        private async Task SaveInstructionSet()
        {
            
            var result = await _dialogHost.ShowDialog
            ("是否保存指令集,将覆盖原有数据 ?");
            if (!result)
                return;

            


            await JsonFile.SaveDataAsync(new InstructionSetting()
            {
                Name = InstructionName,
                Protocols = Protocols.ToArray()
            }, InstructionPath, new CancellationToken());


            ModifyNum = 0;
            

        }


        private async Task  OpenNewInstructionSet()
        {
            var dialog = FileBrowserUtil.CreateOpenFileExplorer(_moduleSetting.SavePath);
            if (dialog.ShowDialog() is { } result)
            {
                if (result)
                {

                     var instruction=await JsonFile.GetDataAsync<InstructionSetting>(dialog.FileName, new CancellationToken());

                    InstructionName = instruction!.Name;
                    InstructionPath = dialog.FileName;
                    Protocols = new ObservableCollection<ProtocolFormat>(instruction!.Protocols);
                }
            }
            ModifyNum = 0;
        }
        
        

        #endregion
        

        public ScriptEditorViewModel(
            AskDialogHost dialogHost,
            AppDataCache  appDataCache,
            IMessageService messageService
        )
        {
            _dialogHost=dialogHost;
            _messageService = messageService;
            
            
            _moduleSetting = appDataCache.GetMapping<ModuleSetting>(); 
CreateInstructionCommand = new DelegateCommand(CreateInstruction);
DeleteInstructionCommand = new DelegateCommand(DeleteItem);
SaveItemsCommand=new DelegateCommand(SaveInstructionItems);
OpenExistInstructionSetCommand = new DelegateCommand(OpenExistInstructions);
CreateInstructionSetCommand = new DelegateCommand(CreateNewInstructionSet);
_protocolsChangedHandler = (s, e) =>
{
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add:
            SelectedProtocolFormat = Protocols[^1];
            IsProtocolsEmpty = Protocols.Count == 0;
            break;
        case NotifyCollectionChangedAction.Remove:
            SelectedProtocolFormat = Protocols.Count > 0 ? Protocols[^1] : null;
            IsProtocolsEmpty = Protocols.Count == 0;
            break;
    }

    ModifyNum++;
    
};
IsProtocolsEmpty = true;
        }
    }
}
