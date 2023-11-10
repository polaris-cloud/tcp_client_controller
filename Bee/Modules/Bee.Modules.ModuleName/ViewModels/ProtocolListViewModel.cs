using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Commands;
using Bee.Modules.ModuleName.Connect;
using Newtonsoft.Json;
using System.IO;
using MotorDetection.SettingManager;

namespace Bee.Modules.ModuleName.ViewModels
{
    public class ProtocolListViewModel:BindableBase
    {
        private ObservableCollection<Protocol> _protocols;
 public  ObservableCollection<Protocol> Protocols 
        { 
            get=> _protocols; 
            set=> SetProperty(ref _protocols,value); 
        }

        private int? _protocolNum;
public int? ProtocolNum {
            get => _protocolNum;
            set=> SetProperty(ref _protocolNum, value);
        }

public DelegateCommand<int?> GenerateCommand { get; private set; }

        public ProtocolListViewModel()
        {
            
                GenerateCommand = new DelegateCommand<int?>(GenerateItems);
            }
            
        

        private void GenerateItems(int? num)
        {
            Protocols = new ObservableCollection<Protocol>();
for (int i = 0; i < num; i++) {
                Protocols.Add(new Protocol { Num=i+1}); 
}
        }

        private void SaveAllProtocols()
        {
            

        }
    }
}
