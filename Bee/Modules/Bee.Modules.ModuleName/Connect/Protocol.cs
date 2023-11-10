using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace Bee.Modules.ModuleName.Connect
{

    public enum ProtocolReadWrite
    {
        [Display(Name = "read", ResourceType = typeof(Properties.Resources))]
        Read,
        [Display(Name = "write", ResourceType = typeof(Properties.Resources))]
        Write
    }

    public enum ProtocolEndPattern
    {
        [Display(Name = "little_endian", ResourceType = typeof(Properties.Resources))]
        LittleEnd,
        [Display(Name = "big_endian", ResourceType = typeof(Properties.Resources))]
        BigEnd
    }


    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class Protocol
    {
        [JsonIgnore]
        public int Num { get; set; }
        public byte[] Header { get; set; }
        public byte[] DataLength { get; set; }

        public byte[] Data { get; set; }
        public byte[] Roar { get; set; }
        public ProtocolReadWrite ProtocolReadWrite { get; set; }
        public string FunctionName { get; set; }
        public ProtocolEndPattern ProtocolEndPattern { get; set; }

        public static int ParseData(byte[] data, ProtocolEndPattern endPattern)
        {

            if (BitConverter.IsLittleEndian ^ endPattern == ProtocolEndPattern.LittleEnd)
            {
                Array.Reverse(data);
            }

            switch (data.Length)
            {
                case 4:
                    return BitConverter.ToInt32(data, 0);
                case 2:
                    return BitConverter.ToInt16(data, 0);
                default:
                    throw new NotImplementedException("未实现其他字节长度");
            }

        }

    }
}
