using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol
{

    public enum ProtocolEncodeFormat
    {
        Hex,
        String
    }


    public enum ProtocolEndian
    {
        BigEndian,
        LittleEndian
        
    }


    
    public class ProtocolFormat
    {
        
        public string? Name {get; set;}
        public string? BehaviorKeyword { get; set;}
        
        public ProtocolEncodeFormat EncodeFormat { get; set;}
        public ProtocolEndian ProtocolEndian { get; set; }

        public string? SendFrameRule { get; set; }
        public string? ResponseFrameRule { get; set; }

    }
}
