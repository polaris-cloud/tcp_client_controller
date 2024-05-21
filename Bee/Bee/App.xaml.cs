
using System;
using System.Threading.Tasks;
using Bee.Services;
using Bee.Services.Interfaces;
using Bee.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using System.Windows.Controls;
using Bee.Core.DataSource;
using Bee.CORE.RegionAdapters;
using Bee.Modules.Communication;
using Bee.Modules.Console;
using Bee.Modules.Script;
using Prism.Regions;
using Bee.Services.DataSource;
using Bee.ViewModels;
using Bee.Core.ModuleExtension;
using MaterialDesignThemes.Wpf;

namespace Bee
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private MainWindow _mainWindow;
        private InitPage _initPage; 
        protected override Window CreateShell()
        {
            _mainWindow= Container.Resolve<MainWindow>(); //step1.1 ,包含在Initialize()中,生成mainwindow
            _initPage=Container.Resolve<InitPage>();
            return MainWindow;
        }


        protected override async void OnInitialized()
        {  
            // jump init page 
            _mainWindow.SwitchContentTransitioner.SelectedIndex = 0;
            
            
            // load app data
            await InitializeAppData();
            
            //jump modules Page
            //_mainWindow.SwitchContentTransitioner.SelectedIndex = 1;
            
            // after app data loaded
            OnAppDataInitialized();
            
            base.OnInitialized();  //step4  : show mainwindow

        }

         void OnAppDataInitialized()
         {
             var mainVm = _mainWindow.DataContext as MainWindowViewModel;
            foreach (IModuleInfo module in Container.Resolve<IModuleCatalog>().Modules)
            {
                var type = Type.GetType(module.ModuleType);
                if (type == null)
                    throw new ArgumentNullException(nameof(type));
                if (type.IsAssignableTo(typeof(IModule)) && type.IsAssignableTo(typeof(IModuleExtension)))
                {
                    if (Container.Resolve(type) is IModuleExtension moduleEx)
                    {
                        moduleEx.OnAppDataInitialized(Container as IContainerRegistry);
                        
                        
                        // 是否使用 IEventAggregator代替？ 
                        mainVm!.CreateModuleTab(moduleEx);
                        mainVm.SelectFirst();
                    }
                }
                
            }
            _initPage.Close();
            
         }

        private async Task InitializeAppData()
        {
            var manager = Container.Resolve<ScopedAppDataSourceManager>();
            //var init = _mainWindow.InitPage;
            _initPage.Show();
            try
            {
                manager.InitializeAppCache(AppDomain.CurrentDomain.BaseDirectory);
                manager.WhenAppDataLoad += (s, e) => { _initPage.LogTextBlock.Text += e + Environment.NewLine; };
                await Task.WhenAll(Task.Delay(1000), manager.LoadFromDataSourceAsync());

            }
            catch (Exception e)
            {
                _initPage.LogTextBlock.Text += $"Error: {e.Message + Environment.NewLine} ";
            }
            await Task.Delay(1000);
            
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e); //5
        }

        protected override void Initialize()
        {
            base.Initialize();  //step1
        }

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell); //step1.2 ,包含在Initialize()中
        }

        protected override void InitializeModules()
        {
            base.InitializeModules();//step1.3 ,包含在Initialize()中
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            containerRegistry.RegisterSingleton<IAppDataSource, AppDataSource>();
            containerRegistry.RegisterSingleton(typeof(ScopedAppDataSourceManager));
            containerRegistry.RegisterSingleton(typeof(AppDataCache));
            containerRegistry.RegisterSingleton<ISnackbarMessageQueue,SnackbarMessageQueue>();
        }

         //我们可以自定义一个ModuleCatalog用来外部导入已经写好的模块文件,amazing
        //protected override IModuleCatalog CreateModuleCatalog()
        //{
        //    return new DirectoryModuleCatalog() {ModulePath = @"D:\Users\Polaris\source\repos\tcp_controller" };
        //}

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping(typeof(StackPanel),Container.Resolve<StackPanelRegionAdapter>());
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<CommunicationModule>();
            moduleCatalog.AddModule<ScriptModule>();
            moduleCatalog.AddModule<ConsoleModule>();
        }
    }
}
