using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Core.Events
{
    public class InitModuleConfigEventArgs
    {
        public string AppRootPath { get; set; }
        public Action<string> LogOutput { get; set; }
    }
}
