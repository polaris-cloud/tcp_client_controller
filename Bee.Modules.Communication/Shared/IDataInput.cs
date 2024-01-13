using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Modules.Communication.Shared
{
    interface IDataInput
    {
        event EventHandler OnInputEmpty;
    }
}
