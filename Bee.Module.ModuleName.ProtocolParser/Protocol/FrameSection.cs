using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    internal  class FrameSectionBase
    {
        public string Name { get; set; }
        public byte[] Value { get; set; }
        public int Length { get; set; }

        public  bool IsRuntimeImportValue { get; protected set; } 

        public FrameSectionBase(FrameSectionToken token, byte[] value)
        {
            Name = token.SectionName;
            Length = int.Parse(token.Length);


            if (value.Length == 0)
                value = new byte[Length];
            if (value.Length != Length)
                throw new ArgumentException($"预设值错误({Name}): {nameof(value)}.Length ({value.Length}) 不等于 token.{nameof(Length)} ({Length})", nameof(value));
            Value = value;
            IsRuntimeImportValue= false;
        }

        
    }

    internal class FrameCheckedSection : FrameSectionBase
    {
        public FrameCheckMethod CheckMethod { get; set; }

        public FrameCheckedSection(FrameSectionToken token) : base(token, Array.Empty<byte>())
        {
            IsRuntimeImportValue = true;
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

        public FrameRuntimeSection(FrameSectionToken token) : base(token,Array.Empty<byte>())
        {
            IsRuntimeImportValue= true;
            string[]? limits = token.Operator?.Split('-');
            if (limits?.Length != 2)
                return;
            LowerLimit = Convert.ToInt32(limits[0]);
            UpperLimit = Convert.ToInt32(limits[1]);
        }

    }

}
