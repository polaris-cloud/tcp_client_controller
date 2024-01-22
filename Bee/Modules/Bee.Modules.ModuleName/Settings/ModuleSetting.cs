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
        public string SavePath { get; set;  }


        public object Clone()
        {
            throw new NotImplementedException();
        }

        public string SubDir { get; } = "scr";
        public string Contract { get; } = "mo"; 
    }
}
