using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace Bee.Modules.Communication.Shared
{

    public delegate void OutputDataReceived(string output, Brush brush);
    
    public  interface IOutputDataReceived
    {
         bool IsOutputAsLog { get; set; }
        event OutputDataReceived OnOutputReceivedData;
        event EventHandler OnOutputEmpty;
        
    }
}
