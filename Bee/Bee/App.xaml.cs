using Bee.Modules.ModuleName;
using Bee.Services;
using Bee.Services.Interfaces;
using Bee.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;

namespace Bee
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ModuleNameModule>();
            
        }
    }
}
