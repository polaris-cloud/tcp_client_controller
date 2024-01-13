using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Bee.Core;
using Bee.CORE;
using Bee.Core.ModuleExtension;
using Bee.Modules.Communication.Shared;
using Bee.Services;
using MaterialDesignThemes.Wpf;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Bee.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string _title = "Bee";

        private string _author = "by Polaris";

//        private int _selectedPageIndex;
        private ModuleNavigateTab _selectedModuleItem;


        public ObservableCollection<ModuleNavigateTab> ModuleItems { get; set; } = new ObservableCollection<ModuleNavigateTab>();

        public ModuleNavigateTab SelectedModuleItem
        {
            get => _selectedModuleItem;
            set
            {
                SetProperty(ref _selectedModuleItem, value,
                    () => { OnSelectedModuleItemChanged(value, _regionManager); });
            }

        }

        public ISnackbarMessageQueue MainSnackbarMessageQueue { get; }


        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        //public int SelectedPageIndex
        //{
        //    get => _selectedPageIndex;
        //    set => SetProperty(ref _selectedPageIndex, value);
        //}

        private void NavigationComplete(NavigationResult result)
        {
            if (result.Result is false)
                MainSnackbarMessageQueue.Enqueue($" Uri: {result.Context.Uri} ,Error: {result.Error?.Message}");
        }

        private void OnSelectedModuleItemChanged(ModuleNavigateTab sel, IRegionManager regionManager)
        {
            regionManager.RequestNavigate(RegionNames.ContentRegion, sel.NavigateUri, NavigationComplete);
        }



        public MainWindowViewModel(
            IRegionManager regionManager,
            ISnackbarMessageQueue messageQueue
        )
        {
            _regionManager = regionManager;
            MainSnackbarMessageQueue = messageQueue;
        }

        public void CreateModuleTab(IModuleExtension moduleEx)
        {
            ModuleItems.Add(new ModuleNavigateTab()
            {
                Icon = moduleEx.Icon.ToString(), 
                 Name= moduleEx.Name,
                NavigateUri = moduleEx.Uri
            });
        }

        public void SelectFirst()
        {
            if (ModuleItems.Count > 0)
                SelectedModuleItem = ModuleItems[0];
        }
    }

}
