using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Bee.Core.Controls;

namespace Bee.Modules.Communication.Shared
{
    internal   class ReceiveRichTextBoxUtil
    {
        internal static void WriteOutputToReceivedDataRegion(RichTextBox receiveRichTextBox,Dispatcher dispatcher ,string output, Brush brush)
        {

            dispatcher.RunOnUiDispatcher(() =>
            {
                //Write the output.
                var range = new TextRange(
                    receiveRichTextBox.GetEndPointer(),
                    receiveRichTextBox.GetEndPointer())
                {
                    Text = output
                };
                range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);

                //Get to the end
                receiveRichTextBox.ScrollToEnd();
                receiveRichTextBox.SetCaretToEnd();
                //ContentRichTextBox.Focus();
            });
        }
    }
}
