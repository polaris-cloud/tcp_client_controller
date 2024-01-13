using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Prism.Mvvm;

namespace Bee.Modules.Communication.Shared
{
    public class ComViewTabItem
    {
        public string Icon { get; set; }
        public string TabName { get; set; }
        public object View { get; set; }
        
    }
}
