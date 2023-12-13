using Bee.Core;
using Bee.Modules.Script.Models;
using Bee.Modules.Script.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Bee.Modules.Script
{
    public class ScriptModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ScriptModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, "ViewA");
            _regionManager.RegisterViewWithRegion(RegionNames.ScriptRegion, typeof(ViewA)); 
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewA>();
            containerRegistry.RegisterForNavigation<MainOrderView>();
            containerRegistry.RegisterForNavigation<ProtocolListView>();
            containerRegistry.RegisterSingleton<InstructionSetDao>();
        }
    }
}