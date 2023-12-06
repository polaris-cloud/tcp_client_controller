using System;
using System.Collections.Generic;
using System.Linq;
using Bee.Module.ModuleName.ProtocolParser.Protocol.Converter;
using Bee.Module.ModuleName.ProtocolParser.Protocol.enums;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol.Model.Section;

internal class FrameRuntimeSection : FrameSectionBase
{

    private int? _lowerLimit;
    private int? _upperLimit;

    private string[] _right;

    public FrameRuntimeSection(FrameSectionToken token, ProtocolEncodeFormat format) : base(token)
    {
        IsRuntimeImportValue = true;
        Length = Convert.ToInt32(token.Length);
        Storage = new byte[Length];
        if (token.Action != null)
            GetRangeForValidate(token.Action, format);
    }

    private void GetRangeForValidate(string value, ProtocolEncodeFormat format)
    {
        if (format == ProtocolEncodeFormat.Hex)
        {
            string[] limits = value.Trim('(', ')').Split('/');
            if (limits?.Length != 2)
                return;
            _lowerLimit = Convert.ToInt32(limits[0]);
            _upperLimit = Convert.ToInt32(limits[1]);
        }
        else if (format == ProtocolEncodeFormat.String)
            _right = value.Trim('(', ')').Split('/');


    }

    public byte[] GetBytes(ProtocolEncodeFormat format, ProtocolEndian endian, string value)
    {
        return FrameConverter.ConvertValueStringToBytes(endian, format, value, Length);
    }

    public static bool Validate(IEnumerable<FrameRuntimeSection> sections)
    {
        var list = sections.Select(t => t.Name).ToList();
        bool isExistEmptyName = list.All(t => !string.IsNullOrEmpty(t));
        bool isExistRepeat = list.Distinct().Count() == list.Count;
        return isExistEmptyName && isExistRepeat;
    }

    public bool Validate(string value)
    {
        if (_right != null)
            return _right.Contains(value);
        if (_lowerLimit != null && _upperLimit != null)
        {
            var num = Convert.ToInt32(value);
            return num >= _lowerLimit && num <= _upperLimit;
        }

        return true;
    }

}