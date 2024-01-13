using Bee.Core;
using Bee.Core.ModuleExtension;
using Bee.Modules.Console.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Reflection;

namespace Bee.Modules.Console
{
    public class ConsoleModule : IModule,IModuleExtension
    {
        private readonly IRegionManager _regionManager;

        public ConsoleModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            Uri = Assembly.GetExecutingAssembly().GetName().Name;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            //_regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, "ViewA");
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        public object Icon { get; } = "Console";
        public string Name { get; } = "Console";
        public string Uri { get; }

        public void OnAppDataInitialized(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewA>(Uri);
        }
    }
}