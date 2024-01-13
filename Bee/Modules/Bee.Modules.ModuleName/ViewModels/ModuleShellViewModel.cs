using Bee.Core.DataSource;
using Bee.Services.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Bee.Core.ModuleExtension;
using Bee.Modules.Script.Shared;
using Prism.Mvvm;

namespace Bee.Modules.Script.ViewModels
{
    internal class ModuleShellViewModel:BindableBase
    {
        private readonly ScopedAppDataSourceManager _scopedAppDataSourceManager;
        private readonly IMessageService _messageService;
        public ModuleCommand ModuleCommand { get; }
        public List<ModuleSubViewTabItem> Items { get; set; } = new List<ModuleSubViewTabItem>();
        private ModuleSubViewTabItem _selComViewTabItem;
        public ModuleSubViewTabItem SelComViewTabItem
        {
            set
            {
                ViewActiveAware(_selComViewTabItem, false);
                ViewActiveAware(value, true);
                _selComViewTabItem = value;
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
        private bool _isDebugMode;
        private bool _isOpenConfigBox;

        public bool IsDebugMode
        {
            get => _isDebugMode;
            set => SetProperty(ref _isDebugMode, value);
        }



        #endregion

        public ModuleShellViewModel(
            IContainerExtension containerProvider,
            ScopedAppDataSourceManager scopedAppDataSourceManager,
            AppDataCache appDataCache,
            IMessageService messageService,
            ModuleCommand moduleCommand
            )
        {
            _scopedAppDataSourceManager = scopedAppDataSourceManager;
            _messageService = messageService;
            ModuleCommand = moduleCommand;
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 获取定义了 Attribute 的所有类型
            var types = assembly.GetTypes();
            var typesWithAttribute = types
                .Where(type => Attribute.IsDefined(type, typeof(ModuleSubViewTabItemAttribute)));


            // 输出这些类型的名称
            foreach (var type in typesWithAttribute)
            {
                var attr = Attribute.GetCustomAttribute(type, typeof(ModuleSubViewTabItemAttribute)) as ModuleSubViewTabItemAttribute;
                Items.Add(new ModuleSubViewTabItem()
                {
                    View = containerProvider.Resolve(type),
                    Icon = attr?.Icon,
                    TabName = attr?.ItemName
                });
            }

            
            //_setting = appDataCache.GetMapping<CommunicationModuleSetting>();
            //IsDebugMode = _setting.IsDebugMode;
            SaveSettingCommand = new DelegateCommand(SaveSetting);
        }

        public DelegateCommand SaveSettingCommand { get; }

        public bool IsOpenConfigBox
        {
            get => _isOpenConfigBox;
            set => SetProperty(ref _isOpenConfigBox, value);
        }



        private async void SaveSetting()
        {
            try
            {
                //_setting.IsDebugMode = IsDebugMode;
                //await _scopedAppDataSourceManager.SaveToDataSourceAsync(_setting);
                //_messageService.Notice($" NOTICE:   {_setting.Contract} saved");
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

    }
}
