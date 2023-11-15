using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol
{
    public class Converter
    {
        public static ProtocolScript ToProtocolScript(ProtocolFormat format)
        {
            var behavior = format.BehaviorKeyword!;
            var description = format.Name;
            var endian = format.ProtocolEndian;
            var encodeFormat = format.EncodeFormat;
            var factory = new FrameSectionFactory();
            if (format.SendFrameRule == null || format.ResponseFrameRule == null)
                throw new ArgumentNullException($"{nameof(format.SendFrameRule)} or {nameof(format.ResponseFrameRule)}");
            var parseSend = factory.ParseSection(format.SendFrameRule);
            var parseResponse = factory.ParseSection(format.ResponseFrameRule);

            return new ProtocolScript(behavior, description, parseSend.ToList(),parseResponse.ToList(),endian,encodeFormat);
        }
    }
}
