using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows;
using Prism.Unity;
using Unity;

namespace tcp_client_controller
{



    public class MyBootstrapper : PrismBootstrapper
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册依赖项
            //containerRegistry.Register<IMyService, MyService>();
            //containerRegistry.
        }
    }
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 创建 Prism 容器

            
            // 创建 Prism 应用程序，并使用上面创建的容器
            var bootstrapper = new MyBootstrapper();
            bootstrapper.Run();
        }
    }
}
