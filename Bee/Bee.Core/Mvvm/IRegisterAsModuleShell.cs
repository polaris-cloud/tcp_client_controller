using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Core.Mvvm
{
    public interface IRegisterAsModuleShell
    {
         string ShellIcon { get; }
        string ShellHeader { get; }
    }
}
