using System.Linq;
using System.Reflection;
using Bee.Core;
using Bee.Core.DataSource;
using Bee.Core.ModuleExtension;
using Bee.Modules.Script.Models;
using Bee.Modules.Script.Settings;
using Bee.Modules.Script.Shared;
using Bee.Modules.Script.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Bee.Modules.Script
{
    public class ScriptModule : IModule,IModuleExtension
    {
        private readonly IRegionManager _regionManager;

        public object Icon { get; } = "ScriptTextOutline";
        public string Name { get; } = "Script";
        public string Uri { get; }
        
        
        public ScriptModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            Uri = ModuleUris.ModuleUri;
        }
        

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppData, ModuleSetting>(Uri+nameof(ModuleSetting));
            containerRegistry.RegisterSingleton<IAppData, ScriptDebuggerSetting>(Uri + nameof(ScriptDebuggerSetting));
            containerRegistry.RegisterSingleton<ModuleCommand>();
            containerRegistry.RegisterSingleton<AskDialogHost>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }


        public void OnAppDataInitialized(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ModuleShell>(Uri);
        }
    }
}