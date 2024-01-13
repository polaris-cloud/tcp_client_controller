using System.Windows;
using System.IO;
using Polaris.Console;
using Polaris.Console.Wrapper;

namespace ScriptEditorTest.ConsoleTest
{
    /// <summary>
    /// ConsoleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private MemoryStream _stream;
        private StreamWriter _writer;
        private StreamReader _reader;

        private readonly object _lock= new object();
        
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

            ComWrapper wrapper = new ComWrapper(new ComMappingProvider());
            
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
