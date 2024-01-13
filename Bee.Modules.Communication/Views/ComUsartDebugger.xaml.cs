using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Bee.Core.Controls;
using Bee.Modules.Communication.Shared;

namespace Bee.Modules.Communication.Views
{
    /// <summary>
    /// Interaction logic for COMUsartDebugger
    /// </summary>
    [SupportViewSelect("串口调试", "SerialPort")]
    public partial class ComUsartDebugger : UserControl
    {
        public ComUsartDebugger()
        {
            InitializeComponent();
            ((IOutputDataReceived)DataContext).OnOutputReceivedData+=(o,b)=> ReceiveRichTextBoxUtil.WriteOutputToReceivedDataRegion(
                ReceiveRichTextBox, Dispatcher,o,b);

            ((IOutputDataReceived)DataContext).OnOutputEmpty += (o, b) => ReceiveRichTextBox.Document.Blocks.Clear();
        }

        public static bool IsHexadecimal(string input)
        {
            try
            {
                // 尝试将字符串解析为十六进制数
                int result = Convert.ToInt32(input, 16);
                return true;
            }
            catch (FormatException)
            {
                // 如果出现格式异常，则字符串无法转换为十六进制
                return false;
            }
        }
        
        
        
    }
}
