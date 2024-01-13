using Bee.Core.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Modules.Script.Settings
{
    internal class ModuleSetting: IAppData
    {
        public object Clone()
        {
            throw new NotImplementedException();
        }

        public string SubDir { get; }
        public string Contract { get; }
    }
}
