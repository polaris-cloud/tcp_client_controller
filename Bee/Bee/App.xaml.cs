
using System;
using Bee.Services;
using Bee.Services.Interfaces;
using Bee.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Bee.Modules.Console;
using Bee.Modules.Script;
using Unity;

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

         //我们可以自定义一个ModuleCatalog用来外部导入已经写好的模块文件,amazing
        //protected override IModuleCatalog CreateModuleCatalog()
        //{
        //    return new DirectoryModuleCatalog() {ModulePath = @"D:\Users\Polaris\source\repos\tcp_controller" };
        //}

        

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {

            moduleCatalog.AddModule<ScriptModule>();
            moduleCatalog.AddModule<ConsoleModule>();
            
        }
    }
}
