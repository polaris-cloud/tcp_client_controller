using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
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
            
            ((IEasyLoggingBindView)DataContext).OnLogData += Log;
        }


        void Log(string content, LogLevel level)
        {
            Brush brush;
            switch (level)
            {
                case LogLevel.Info:
                    brush = Brushes.Black; 
                    break;
                case LogLevel.Debug:
                    brush = Brushes.Blue;
                    break;
                case LogLevel.Warn:
                    brush = Brushes.Yellow;
                    break;
                case LogLevel.Error:
                    brush = Brushes.Red;
                    break;
                case LogLevel.Fatal:
                    brush = Brushes.Red;
                    break;
                case LogLevel.Other:
                    brush = Brushes.Brown;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            ReceiveRichTextBoxUtil.WriteOutputToReceivedDataRegion(
                ReceiveRichTextBox, Dispatcher, content, brush);

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


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ReceiveRichTextBox.Document.Blocks.Clear();
        }
    }
}
