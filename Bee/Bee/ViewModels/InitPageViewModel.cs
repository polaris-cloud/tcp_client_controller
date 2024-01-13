using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Core.DataSource;
using Bee.Core.Events;
using Prism.Events;
using Prism.Mvvm;

namespace Bee.ViewModels
{
    public class InitPageViewModel:BindableBase
    {
        private string _message;

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        
        public InitPageViewModel(IEventAggregator eventAggregator, ScopedAppDataSourceManager manager)
        {
            Message = " ";
            
        }


        
        
        
    }
}
