using Bee.Core.Mvvm;
using Bee.Services.Interfaces;
using Prism.Commands;
using Prism.Regions;

namespace Bee.Modules.ModuleName.ViewModels
{
    public class ViewAViewModel : RegionViewModelBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        public DelegateCommand<string> NavigateCommand { get; private set; }
        public ViewAViewModel(IRegionManager regionManager, IMessageService messageService) :
            base(regionManager)
        {
            Message = messageService.GetMessage();
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

  
        
        private void Navigate(string navigatePath)
        {
            if (navigatePath != null)
                RegionManager.RequestNavigate("TcpControlRegion", navigatePath);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //do something
            
        }
    }
}
