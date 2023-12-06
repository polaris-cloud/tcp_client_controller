using System;
using System.Text;
using Bee.Module.ModuleName.ProtocolParser.Protocol.Model;
using Newtonsoft.Json.Linq;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol.Model.Section;

internal abstract class FrameSectionBase
{
    internal string Name { get; set; }

    internal int Length { get; set; }

    public byte[] Storage { get; set; }

    public bool IsRuntimeImportValue { get; protected set; }

    protected FrameSectionBase(FrameSectionToken token)
    {
        Name = token.SectionName;
        Length = 0;
        Storage = Array.Empty<byte>();
    }
}