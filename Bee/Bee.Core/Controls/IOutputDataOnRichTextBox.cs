using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
namespace Bee.Core.Controls
{
    public delegate void OutputDataOnRichTbxHandler(string output, Brush brush);

    public interface IOutputDataOnRichTextBox
    {
        bool IsOutputAsLog { get; set; }
        event OutputDataOnRichTbxHandler OnOutputVariantData;
        event EventHandler OnOutputEmpty;

    }
}
