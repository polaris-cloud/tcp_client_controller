using ScriptEditorTest.ScriptConsole.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Bee.Core.Connect.TcpServer;
using ScriptEditorTest.ScriptConsole.COM;
using System.IO;

namespace ScriptEditorTest.ScriptConsole
{
    /// <summary>
    /// ConsoleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private MemoryStream _stream;
        private StreamWriter _writer;
        private StreamReader _reader;

        private object _lock = new object();
        
        public ConsoleWindow()
        {
            InitializeComponent();
            //ReadFromStream();
        }





        //public void ReadFromStream()
        //{
        //    _stream = new MemoryStream();
        //    _writer = new StreamWriter(_stream);
        //    _reader = new StreamReader(_stream);
        //    Task.Run(() =>
        //    {
        //        while (true)
        //        {

        //            lock (_lock)
        //            {
        //                var line =_reader.ReadLine();
        //                if (line != null)
        //                    Console.WriteOutput(line+Environment.NewLine,Brushes.DeepPink);
        //                _stream.
        //            }
        //            Thread.Sleep(200);
                    
        //        }
        //    });
        //}






        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            //ProcessWrapper wrapper=new ProcessWrapper();

            ComWrapper wrapper = new ComWrapper(new TcpServer());
            
             Console.ConsoleBridge.SwitchStream(wrapper);
            wrapper.Start();
             
             //wrapper.StartProcess("cmd.exe");
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
            _writer.WriteLine("fasdfasd");
            _writer.Flush();
            lock (_lock)
            {
                _stream.Position = 0;
            }
            
            
        }
    }
}
