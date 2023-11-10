using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol
{
    internal enum FrameCheckMethod
    {
        None, 
        Sum,
        CRC16
    }

    internal class FrameSectionBase: IParseSectionToken
    {
        public string Name { get; set; }
        public byte[] Value { get; set; }
        public int Length { get; set; }

        public virtual bool IsRuntimeImportValue { get; } = true;
        public virtual void Parse(FrameSectionToken token)
        {
             Name = token.SectionName;
             Length = int.Parse(token.Length);
        }
    }

    internal class FrameCheckedSection : FrameSectionBase
    {
        public FrameCheckMethod CheckMethod { get; set; }
        public override bool IsRuntimeImportValue { get; } = false;

        
        
        public override void Parse(FrameSectionToken token)
        {
            base.Parse(token);
            switch (token.Action?.ToUpper())
            {
                case "SUM": 
                    CheckMethod = FrameCheckMethod.Sum;
                    break;
                case "CRC16":
                    CheckMethod = FrameCheckMethod.CRC16;
                    break;
                default:
                    CheckMethod = FrameCheckMethod.None;
                    break;
            }

        }
    }

    internal class FrameRuntimeSection:  FrameSectionBase
    {
        
        public int LowerLimit { get; set; }
        public int UpperLimit { get; set; }
        

    }

}
