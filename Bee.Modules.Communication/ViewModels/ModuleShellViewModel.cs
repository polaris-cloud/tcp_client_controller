using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bee.Modules.Communication.Shared;
using Prism.Ioc;
using Prism.Regions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using Bee.Core.DataSource;
using Bee.Modules.Communication.Setting;
using Bee.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism;

namespace Bee.Modules.Communication.ViewModels
{
	public class ModuleShellViewModel : BindableBase
	{
        private readonly ScopedAppDataSourceManager _scopedAppDataSourceManager;
        private readonly IMessageService _messageService;
        public ComModuleCommand ComModuleCommand { get; }
        public List<ComViewTabItem> Items { get; set; }=new List<ComViewTabItem>();
private readonly CommunicationModuleSetting _setting;
private ComViewTabItem _selComViewTabItem; 
public ComViewTabItem SelComViewTabItem
{
    set
    {
        ViewActiveAware(_selComViewTabItem, false);
        ViewActiveAware(value, true);
        _selComViewTabItem = value;
            }
}

private static void ViewActiveAware(ComViewTabItem value,bool isActive)
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
            IMessageService  messageService,
            ComModuleCommand comModuleCommand
            )
        {
            _scopedAppDataSourceManager = scopedAppDataSourceManager;
            _messageService = messageService;
            ComModuleCommand = comModuleCommand;
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 获取定义了 Attribute 的所有类型
            var types = assembly.GetTypes();
            var typesWithAttribute = types
                .Where(type => Attribute.IsDefined(type, typeof(SupportViewSelectAttribute)));

            
            // 输出这些类型的名称
            foreach (var type in typesWithAttribute)
            {
                 var attr=Attribute.GetCustomAttribute(type, typeof(SupportViewSelectAttribute)) as SupportViewSelectAttribute;
                Items.Add(new ComViewTabItem()
                {
                    View = containerProvider.Resolve(type),
                    Icon = attr?.Icon,
                    TabName= attr?.ItemName
                });
            }


            //_setting = dataCache.GetMapping<CommunicationModuleSetting>();
            //IsDebugMode = _setting.IsDebugMode;
            _setting = appDataCache.GetMapping<CommunicationModuleSetting>();
            IsDebugMode = _setting.IsDebugMode;
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
                _setting.IsDebugMode = IsDebugMode;
                await _scopedAppDataSourceManager.SaveToDataSourceAsync(_setting);
                _messageService.Notice( $" NOTICE:   {_setting.Contract} saved");
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
