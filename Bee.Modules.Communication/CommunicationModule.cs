using System.Linq;
using Bee.Core;
using Bee.Core.DataSource;
using Bee.Modules.Communication.Views;
using Polaris.Connect.Tool;
using Polaris.Connect.Tool.SerialPort;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Reflection;
using System.Threading.Tasks;
using Bee.Core.Events;
using Bee.Core.ModuleExtension;
using Bee.Modules.Communication.Setting;
using Bee.Modules.Communication.Shared;
using Prism.Events;

namespace Bee.Modules.Communication
{
    public class CommunicationModule : IModule,IModuleExtension
    {
        private readonly IRegionManager _regionManager;

        

        public CommunicationModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            Uri = Assembly.GetExecutingAssembly().GetName().Name;
        }

        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            //_regionManager.RegisterViewWithRegion("ContentRegion", typeof(ModuleShell));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppData, CommunicationModuleSetting>(Uri + nameof(CommunicationModuleSetting));
            containerRegistry.RegisterSingleton<IAppData,SerialPortDebuggerSetting>(Uri+nameof(SerialPortDebuggerSetting));
            containerRegistry.RegisterSingleton<IAppData, TcpDebuggerSetting>(Uri + nameof(TcpDebuggerSetting));
            containerRegistry.RegisterSingleton<ComModuleCommand>();
            containerRegistry.RegisterSingleton<ComManager>();
        }

        public object Icon { get; } = "Connection";
        public string Name { get; } = "通信模块";
        public string Uri { get; }
        public void OnAppDataInitialized(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ModuleShell>(Uri);
        }
    }
}