using System;
using Polaris.Protocol.Converter;
using Polaris.Protocol.enums;
using Polaris.Protocol.Model;

namespace Polaris.Protocol.Model.Section;

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