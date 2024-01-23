using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bee.Core;
using Bee.Modules.Script.Settings;
using Bee.Modules.Script.Shared;
using Bee.Modules.Script.ViewModels;
using Bee.Modules.Script.Views;
using MaterialDesignThemes.Wpf;
using Polaris.Protocol.Model;

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
        private readonly WaitLoadingDialog _dialogLoading;
        public AskDialogHost(AskDialog dialog,AddFileDialog dialogfile, WaitLoadingDialog dialogLoading)
        {
            _view = dialog;
            _dialogfile = dialogfile;
            _dialogLoading = dialogLoading;
        }

    #region contnetDialog

    public async Task<bool> ShowDialog(string content)
    {
        (_view.DataContext as AskDialogViewModel)!.Content = content;

        if (await DialogHost.Show(_view, RegionNames.ContentRegion) is bool result)
        {
            return result;
        }
        throw new NotImplementedException();
    }

    #endregion
    
    #region InputDialog
    public async Task<DialogResult> ShowInputDialog(string content, string initOutput = "")
    {
        var vm = (_dialogfile.DataContext as AddFileDialogViewModel)!;
        vm.Content = content;
        vm.Output = initOutput;
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

    #endregion

    #region progressDialog
    
    public async Task<bool> ShowLoadingDialog(string content,Func<CancellationToken,Task> waitLoading,CancellationTokenSource cts)
    {

         async void OpenedDialogHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            await waitLoading(cts.Token);
            if (!eventArgs.Session.IsEnded && !cts.IsCancellationRequested)
                    eventArgs.Session.Close(true);
        }


        void CloseDialogHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter is bool re && re == false) 
                cts.Cancel();
        }
        

        var vm = (_dialogLoading.DataContext as WaitLoadingDialogViewModel)!;
        vm.Content = content;
        var re = await DialogHost.Show(_dialogLoading, RegionNames.ContentRegion, OpenedDialogHandler, CloseDialogHandler);
        if (re is bool result)
        {
            return result;
        }
        throw new NotSupportedException();
    }
    
    #endregion



}
