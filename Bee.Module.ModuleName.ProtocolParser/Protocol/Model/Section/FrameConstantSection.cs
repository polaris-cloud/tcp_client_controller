using Bee.Module.ModuleName.ProtocolParser.Protocol.enums;
using Bee.Module.ModuleName.ProtocolParser.Protocol.Model;
using System;
using System.Linq;
using System.Text;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol.Model.Section
{
    internal class FrameConstantSection : FrameSectionBase
    {



        protected FrameConstantSection(FrameSectionToken token) : base(token)
        {
            Name = token.SectionName;
            Storage = Array.Empty<byte>();
            Length = 0;
        }

        public FrameConstantSection(FrameSectionToken token, ProtocolEncodeFormat format, ProtocolEndian endian) :
            this(token)
        {
            if (token.Action != null)
            {
                Storage = ToBytesInternal(token.Action, format, endian);
                Length = Storage.Length;
            }
        }
        private byte[] ToBytesInternal(string value, ProtocolEncodeFormat format, ProtocolEndian endian)
        {


            byte[] data;

            switch (format)
            {
                case ProtocolEncodeFormat.Hex:
                    data = value.Trim('{', '}').Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                    break;
                case ProtocolEncodeFormat.String:
                    data = Encoding.ASCII.GetBytes(value);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            //if (BitConverter.IsLittleEndian ^ endian == ProtocolEndian.LittleEndian)
            //{
            //    Array.Reverse(data);
            //}

            return data;

        }
    }
}
