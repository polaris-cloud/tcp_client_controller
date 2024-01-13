using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Modules.Script.Shared
{
    public class ModuleCommand
    {
        public CompositeCommand SaveCommand { get; } = new CompositeCommand(true);
    }
}
