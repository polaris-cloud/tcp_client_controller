using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bee.Modules.Script.ViewModels
{
    public class AskDialogViewModel : BindableBase
    {
        private string _content;

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public AskDialogViewModel()
        {

        }
    }
}
