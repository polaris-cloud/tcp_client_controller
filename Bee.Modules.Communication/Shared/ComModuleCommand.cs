using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Modules.Communication.Shared
{
    public class ComModuleCommand
    {
        public CompositeCommand SaveCommand { get; } = new CompositeCommand(true);
    }
}
