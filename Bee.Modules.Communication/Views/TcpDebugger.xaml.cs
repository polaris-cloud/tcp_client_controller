using System.Windows.Controls;
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
            ((IOutputDataReceived)DataContext).OnOutputReceivedData += (o, b) => ReceiveRichTextBoxUtil.WriteOutputToReceivedDataRegion(
                ReceiveRichTextBox, Dispatcher, o, b);

            ((IOutputDataReceived)DataContext).OnOutputEmpty += (o, b) => ReceiveRichTextBox.Document.Blocks.Clear();
        }
    }
}
