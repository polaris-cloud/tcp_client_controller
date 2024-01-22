using Polaris.Protocol.enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Polaris.Protocol.Model
{
    public class ProtocolFormat
    {

        public ProtocolEncodeFormat EncodeFormat { get; set; }
        public ProtocolEndian ProtocolEndian { get; set; }

        //TODO: 添加 int timeout字段 

        public string SendFrameDescription { get; set; }
        public string ResponseFrameDescription { get; set; }
        public string BehaviorKeyword { get; set; }
        public string SendFrameRule { get; set; }
        public string ResponseFrameRule { get; set; }

        
    }
}
