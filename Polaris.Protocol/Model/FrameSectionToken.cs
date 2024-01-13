using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Protocol.Model
{
    public class FrameSectionToken
    {
        public string SectionName { get; set; }
        public string Operator { get; set; }
        public string Action { get; set; }
        public string Length { get; set; }

    }
}
