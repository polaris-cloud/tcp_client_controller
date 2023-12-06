using System;
using Bee.Module.ModuleName.ProtocolParser.Protocol.Converter;
using Bee.Module.ModuleName.ProtocolParser.Protocol.enums;
using Bee.Module.ModuleName.ProtocolParser.Protocol.Model;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol.Model.Section;

internal class FrameCheckedSection : FrameSectionBase
{
    private FrameCheckMethod CheckMethod { get; set; }

    public FrameCheckedSection(FrameSectionToken token) : base(token)
    {
        IsRuntimeImportValue = true;
        Length = Convert.ToInt32(token.Length);
        Storage = new byte[Length];
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

    public byte[] GetBytes(ProtocolEndian endian, byte[] toChecked)
    {
        return FrameConverter.GenerateCheckBytes(endian, CheckMethod, toChecked, Length);
    }
}