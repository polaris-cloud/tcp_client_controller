using Bee.Core.DataSource;
using Bee.Services.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Bee.Core.ModuleExtension;
using Bee.Modules.Script.Settings;
using Bee.Modules.Script.Shared;
using Prism.Mvvm;
using Bee.Core;
using Polaris.Storage.WinSystem;
using Prism.Regions;

namespace Bee.Modules.Script.ViewModels
{
    internal class ModuleShellViewModel:BindableBase,INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private readonly ScopedAppDataSourceManager _scopedAppDataSourceManager;
        private readonly IMessageService _messageService;
        public ModuleCommand ModuleCommand { get; }
        public List<ModuleSubViewTabItem> SubViewTabs { get;  } = new List<ModuleSubViewTabItem>();
        private ModuleSubViewTabItem _selComViewTabItem;
        public ModuleSubViewTabItem SelComViewTabItem
        {
            get => _selComViewTabItem;
            set
            {
                //ViewActiveAware(_selComViewTabItem, false);
                //ViewActiveAware(value, true);
                //_selComViewTabItem = value;
                SetProperty(ref _selComViewTabItem, value, () =>
                {
                    OnSelectedModuleItemChanged(value, _regionManager);
                });
            }
            }
        

        private static void ViewActiveAware(ModuleSubViewTabItem value, bool isActive)
        {
            if (value is { View: FrameworkElement v })
            {
                if (v!.DataContext is IActiveAware vm)
                {
                    vm.IsActive = isActive;
                }
            }

        }


        #region 配置       
        
        private bool _isOpenConfigBox;
        
        private string _scriptSavePath;

        public string ScriptSavePath
        {
            get => _scriptSavePath;
            set => SetProperty(ref _scriptSavePath, value);
        }

        #endregion

        private readonly ModuleSetting _setting;  
        
        
        public ModuleShellViewModel(
            IContainerExtension containerRegistry,
            IRegionManager manager,
            ScopedAppDataSourceManager scopedAppDataSourceManager,
            AppDataCache appDataCache,
            IMessageService messageService,
            ModuleCommand moduleCommand
            )
        {
            _regionManager = manager;
            _scopedAppDataSourceManager = scopedAppDataSourceManager;
            _messageService = messageService;
            ModuleCommand = moduleCommand;
            _setting = appDataCache.GetMapping<ModuleSetting>();
            ScriptSavePath = string.IsNullOrWhiteSpace(_setting.SavePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : _setting.SavePath;
            SaveSettingCommand = new DelegateCommand(SaveSetting);
            GetFolderPathCommand=new DelegateCommand(GetFolderPath);
            
            GetSubViews(containerRegistry);
        }

        private void GetSubViews(IContainerExtension containerRegistry)
        {
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 获取定义了 Attribute 的所有类型
            var types = assembly.GetTypes();
            var typesWithAttribute = types
                .Where(type => Attribute.IsDefined(type, typeof(ModuleSubViewTabItemAttribute)));


            // 输出这些类型的名称
            foreach (var type in typesWithAttribute)
            {
                var attr =
                    Attribute.GetCustomAttribute(type, typeof(ModuleSubViewTabItemAttribute)) as ModuleSubViewTabItemAttribute;
                containerRegistry.RegisterForNavigation(type, attr?.NavigateUri);
                SubViewTabs.Add(new ModuleSubViewTabItem()
                {
                    Icon = attr?.Icon,
                    TabName = attr?.ItemName,
                    NavigateUri = attr?.NavigateUri
                });
            }
        }

        public DelegateCommand SaveSettingCommand { get; }
        public DelegateCommand GetFolderPathCommand { get; }

        public bool IsOpenConfigBox
        {
            get => _isOpenConfigBox;
            set => SetProperty(ref _isOpenConfigBox, value);
        }

        private void OnSelectedModuleItemChanged(ModuleSubViewTabItem sel, IRegionManager regionManager)
        {
            regionManager.RequestNavigate(ModuleUris.ContentRegion, sel.NavigateUri, NavigationComplete);
        }
        private void NavigationComplete(NavigationResult result)
        {
            if (result.Result is false)
                Debug.WriteLine($" Uri: {result.Context.Uri} ,Error: {result.Error?.Message}");
        }

        private void NavigateToFirstSub()
        {
            if (SubViewTabs.Count > 0)
                SelComViewTabItem = SubViewTabs[0];
        }

        private void GetFolderPath()
        {
            var dialog=FileBrowserUtil.CreateFolderExplorer(ScriptSavePath, false);
            if (dialog.ShowDialog() is { } result)
            {
                if(result)
                  ScriptSavePath= dialog.FolderName; 
            }
        }


        private async void SaveSetting()
        {
            try
            {
                _setting.SavePath = ScriptSavePath;
                await _scopedAppDataSourceManager.SaveToDataSourceAsync(_setting);
                _messageService.Notice($" NOTICE:   {_setting.Contract} saved");
            }
            catch (Exception ex)
            {
                _messageService.Notice(ex.Message);
            }
            finally
            {
                IsOpenConfigBox = false;
            }
            
        }

        
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            NavigateToFirstSub();
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
