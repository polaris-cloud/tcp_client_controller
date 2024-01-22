using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bee.Modules.Script.ViewModels
{
    public class AddFileDialogViewModel : BindableBase
    {
        private string _content;
        private string _output;

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }


        public AddFileDialogViewModel()
        {

        }
    }
}
