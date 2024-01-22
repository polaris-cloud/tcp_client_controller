using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Core.DataSource;

namespace Bee.Modules.Script.Settings
{
    internal class ScriptDebuggerSetting:IAppData
    {

        public string[] InstructionList { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public string SubDir { get; } = "scr";
        public string Contract { get; } = "sds";
    }
}
