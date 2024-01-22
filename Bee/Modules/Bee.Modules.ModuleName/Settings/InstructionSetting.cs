using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Core.DataSource;
using Newtonsoft.Json;
using Polaris.Protocol.enums;
using Polaris.Protocol.Model;

namespace Bee.Modules.Script.Settings
{
    
    internal class InstructionSetting
    {
        public ProtocolFormat[]  Protocols{ get; set; }
        public string Name { get; set; }
        public object Clone()
        {
            throw new NotImplementedException();
        }
        
    }
}
