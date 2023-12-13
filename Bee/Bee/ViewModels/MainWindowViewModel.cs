using Prism.Mvvm;
namespace Bee.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Bee";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        

        public MainWindowViewModel()
        {

        }
    }
}
