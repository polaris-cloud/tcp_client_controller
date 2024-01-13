using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Bee.Core.Json;
using Prism.Mvvm;
using Prism.Commands;
using Bee.Modules.Script.Models;

namespace Bee.Modules.Script.ViewModels
{
    public class ProtocolListViewModel : BindableBase
    {
        private ObservableCollection<ProtocolFormatDao> _protocols;
        public ObservableCollection<ProtocolFormatDao> Protocols
        {
            get => _protocols;
            set => SetProperty(ref _protocols, value);
        }

        private ProtocolFormatDao _selectedFormat;
        public ProtocolFormatDao SelectedFormat
        {
            get => _selectedFormat;
            set => SetProperty(ref _selectedFormat, value);
        }



        private ProtocolFormatDao _editFormat;
        public ProtocolFormatDao EditFormat
        {
            get => _editFormat;
            set => SetProperty(ref _editFormat, value);
        }





        public DelegateCommand GenerateCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }
        public ProtocolListViewModel(InstructionSetDao dao)
        {

            GenerateCommand = new DelegateCommand(GenerateItem);
            SaveCommand = new DelegateCommand(SaveInstructionSet);
            Protocols = new ObservableCollection<ProtocolFormatDao>();
            if (dao.Protocols == null)
                dao.GetStorage();



            Protocols = new ObservableCollection<ProtocolFormatDao>(dao.Protocols??new List<ProtocolFormatDao>());
            SelectedFormat = Protocols.FirstOrDefault();


            EditFormat = new ProtocolFormatDao()
            {
                BehaviorKeyword = "MoveOsc",
                SendDescription = "振镜x轴移动{x轴}步，y轴移动{y轴}步",
                ResponseDescription = "返回",
                SendFrameRule = @"<={a2 00 09}><x轴:(-32767/32767)|4><y轴:(-32767/32767)|4><是否响应:(0/1)|1><?CRC16|2>",
                ResponseFrameRule = @"<={a2 00 00}><?crc16|2>}"
            };
        }




        public void ReadStorage()
        {




        }

        public void SaveInstructionSet()
        {
            ToJsonConverter converter = new ToJsonConverter();
            foreach (var protocolFormatDao in Protocols)
            {
                protocolFormatDao.ModifyFromKeyValueString();
            }
            InstructionSetDao instructionSetDao = new InstructionSetDao()
            {
                Protocols = new List<ProtocolFormatDao>(Protocols),
                Name = "测试集"
            };

            converter.SaveSetting(instructionSetDao, AppDomain.CurrentDomain.BaseDirectory, "test.json");
            MessageBox.Show("已保存");
        }

        private void GenerateItem()
        {
            var dic = Protocols.Select(s => s.BehaviorKeyword);
            if (dic.Contains(EditFormat.BehaviorKeyword))
            {
                MessageBox.Show("已存在该指令");
                return;
            }



            Protocols.Add(EditFormat.Clone());

            SelectedFormat = Protocols.LastOrDefault();
        }



    }
}
