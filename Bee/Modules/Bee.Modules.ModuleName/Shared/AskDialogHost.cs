using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Bee.Core;
using Bee.Modules.Script.Shared;
using Bee.Modules.Script.ViewModels;
using Bee.Modules.Script.Views;
using MaterialDesignThemes.Wpf;

namespace Bee.Modules.Script.Shared
{
    public struct DialogResult
    {
        public bool ButtonResult;
        public string Output;
    }
        
    }



    public  class AskDialogHost
    {
        private readonly FrameworkElement _view;
        private readonly AddFileDialog _dialogfile;

        public AskDialogHost(AskDialog dialog,AddFileDialog dialogfile)
        {
            _view = dialog;
            _dialogfile = dialogfile;
        }

        public async Task<bool> ShowDialog(string content)
        {
            (_view.DataContext as AskDialogViewModel)!.Content=content;
            
            if (await DialogHost.Show(_view, RegionNames.ContentRegion) is bool result)
            {
                return result;
            }
            throw new NotImplementedException(); 
        }


    public async Task<DialogResult> ShowInputDialog(string content,string initOutput="")
    {
        var vm = (_dialogfile.DataContext as AddFileDialogViewModel)!; 
            vm.Content = content;
            vm.Output=initOutput;
            var re = await DialogHost.Show(_dialogfile, RegionNames.ContentRegion,
                null, FileDialogClosingEventHandler, null);
        if (re is bool result)
        {
            return new DialogResult() { ButtonResult = result, Output = vm.Output }; 
        }
        throw new NotSupportedException();
    }

    private void FileDialogClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
    {
        
        if (eventArgs.Parameter is bool parameter &&
            parameter == false) return;

        //OK, lets cancel the close...

        var vm = (_dialogfile.DataContext as AddFileDialogViewModel)!;
        if (!string.IsNullOrWhiteSpace(vm.Output))
            return; 
        vm.Content = "输入不能为空";

        eventArgs.Cancel();
        
        
    }



}
