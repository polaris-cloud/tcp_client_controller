using Bee.Core.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Modules.Script.Models
{
    public class InstructionSetDao
    {
        public List<ProtocolFormatDao> Protocols { get; set; }
        public string Name { get; set; }

        public InstructionSetDao()
        {


        }

        public void GetStorage()
        {
            ToJsonConverter converter = new ToJsonConverter();
            var dao = converter.GetSetting<InstructionSetDao>(AppDomain.CurrentDomain.BaseDirectory, "test.json");
            if (dao == null)
                return;
            Protocols = dao.Protocols;
            Name = dao.Name;
        }
    }
}
