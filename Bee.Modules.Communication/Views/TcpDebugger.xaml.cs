using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Bee.Core.Controls;
using Bee.Modules.Communication.Shared;

namespace Bee.Modules.Communication.Views
{
    /// <summary>
    /// Interaction logic for TcpDebugger
    /// </summary>
    [SupportViewSelect("TCP设置", "Ethernet")]
    public partial class TcpDebugger : UserControl
    {
        public TcpDebugger()
        {
            InitializeComponent();
            ((IEasyLoggingBindView)DataContext).OnLogData+=Log;
            
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
                case LogLevel.Empty:
                    ReceiveRichTextBox.Document.Blocks.Clear();
                    return;
                case LogLevel.Other:
                    brush = Brushes.Brown;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            ReceiveRichTextBoxUtil.WriteOutputToReceivedDataRegion(
                ReceiveRichTextBox, Dispatcher, content, brush);

        }
        
    }
}
