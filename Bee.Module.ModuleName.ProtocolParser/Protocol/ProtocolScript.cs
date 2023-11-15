using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol
{
    public class ProtocolScript
    {
private  readonly string _behavior;
private  readonly string? _description;
private readonly ProtocolEndian _endian; 
private readonly ProtocolEncodeFormat _encodeFormat;
private  readonly List<FrameSectionBase> _sendFrameSections;
//private  readonly List<FrameRuntimeSection> _sendImportSectionsCache;
//private  readonly IEnumerable<FrameSectionBase> _sendCheckedSections;
private  readonly List<FrameSectionBase> _responseFrameSections;
//private  readonly List<FrameRuntimeSection> _responseImportCache;
//private  readonly IEnumerable<FrameSectionBase> _responseCheckedSections;
        private  readonly string _importValuePattern = @"(\b[a-zA-Z0-9\u4e00-\u9fa5]+\b)=([a-zA-Z0-9]+)";

        public int ResponseFrameLength => GetFrameLength(_responseFrameSections);
        public int SendFrameLength => GetFrameLength(_sendFrameSections);
        
internal ProtocolScript(
    string behavior, 
    string? description,
    List<FrameSectionBase> sendFrameSections,
    List<FrameSectionBase> responseFrameSections, 
    ProtocolEndian endian, 
    ProtocolEncodeFormat encodeFormat)
{
    _behavior = behavior;
    _description = description;
    _sendFrameSections = sendFrameSections;
    _responseFrameSections = responseFrameSections;
    _endian = endian;
    _encodeFormat = encodeFormat;
            //_sendImportSectionsCache = _sendFrameSections.OfType<FrameRuntimeSection>().ToList();
            //_responseImportCache = _responseFrameSections.OfType<FrameRuntimeSection>().ToList();

        }


        public string GenerateSendScript()
        {
            StringBuilder sb = new StringBuilder($"{_behavior} ");
            foreach (var section in _sendFrameSections.OfType<FrameRuntimeSection>())
                     sb.Append($"{section.Name}= ");
            return sb.ToString();
        }


        private byte[] GenerateCheckBytes(FrameCheckMethod checkMethod,byte[] toCheck)
        {

            byte[] data;
            switch (checkMethod)
            {
                case FrameCheckMethod.None:
data=Array.Empty<byte>();
                    break;
                case FrameCheckMethod.Sum:
                    data= BitConverter.GetBytes(FrameChecker.sum_check(toCheck));
                    break;
                case FrameCheckMethod.CRC16:
                    data= BitConverter.GetBytes(FrameChecker.CRC16_Check_T(toCheck, toCheck.Length));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(checkMethod), checkMethod, null);
            }
            if (BitConverter.IsLittleEndian ^ _endian == ProtocolEndian.LittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }

internal  byte[] ConvertValueStringToBytes(ProtocolEncodeFormat format,string value, int dataLength)
        {
            byte[] data;

            switch (format)
            {
                case ProtocolEncodeFormat.Hex:
                    data = ToHexArray(value,dataLength);
                    break;
                case ProtocolEncodeFormat.String:
                    data = Encoding.ASCII.GetBytes(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
            if (BitConverter.IsLittleEndian ^ _endian == ProtocolEndian.LittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }


internal void ConvertValueStringToBytes(
    ProtocolEncodeFormat format,
    FrameRuntimeSection section,
    string value, int dataLength)
{
    byte[] data;

    switch (format)
    {
        case ProtocolEncodeFormat.Hex:
            data = ToHexArray(value, dataLength);
            break;
        case ProtocolEncodeFormat.String:
            data = Encoding.ASCII.GetBytes(value);
            break;
        default:
            throw new ArgumentOutOfRangeException(nameof(format), format, null);
    }
    if (BitConverter.IsLittleEndian ^ _endian == ProtocolEndian.LittleEndian)
    {
        Array.Reverse(data);
    }

    section.Value = data;
}

        private static byte[] ToHexArray(string value,int dataLength)
        {
            //return value.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
            byte[] data;
            switch (dataLength)
            {
                case 4:
                    data=BitConverter.GetBytes(Convert.ToInt32(value));
                    break;
                case 2:
                    data= BitConverter.GetBytes(Convert.ToInt16(value));
                    break;
                case 1:
                    data= new[]{Convert.ToByte(value)};
                    break;
                default:throw new ArgumentOutOfRangeException(nameof(dataLength),dataLength, null);
                    
            }
            return data;
        }

        private static int ToHexNum(byte[] value, int dataLength)
        {

            int data;
            switch (dataLength)
            {
                case 4:
                    data = BitConverter.ToInt32(value,0);
                    break;
                case 2:
                    data = BitConverter.ToInt16(value,0);
                    break;
                case 1:
                    data = value[0];
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(dataLength), dataLength, null);

            }
            return data;
        }
        private string ConvertBytesToValueString(ProtocolEncodeFormat format,byte[] value,int dataLength)
        {
            string data;
            if (BitConverter.IsLittleEndian ^ _endian == ProtocolEndian.LittleEndian)
            {
                Array.Reverse(value);
            }
            switch (format)
            {
                case ProtocolEncodeFormat.Hex:
                    data =ToHexNum(value,dataLength).ToString();
                    break;
                case ProtocolEncodeFormat.String:
                    data = Encoding.UTF8.GetString(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
            
            return data;
        }


        private IEnumerable<KeyValuePair<string,string>> ParseImportValue(string complete)
        {
            var matches = Regex.Matches(complete, _importValuePattern);
            return matches.Select(match =>
                new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value));
        }

/// <summary>
/// extract bytesFrame from sections
/// </summary>
/// <param name="sections"></param>
/// <returns>返回完整字节帧</returns>
        private byte[] ExtractCompleteFrameFrom(List<FrameSectionBase> sections)
        {
            var toCheckedByteArray = sections.Where(s => s.GetType() != typeof(FrameCheckedSection))
                .Select(t => t.Value).SelectMany(a => a).ToArray();
            var checkedToken = sections.OfType<FrameCheckedSection>().FirstOrDefault();
            var check = checkedToken != null
                ? GenerateCheckBytes(checkedToken.CheckMethod,
                    toCheckedByteArray)
                : Array.Empty<byte>();
            byte[] complete = toCheckedByteArray.Concat(check).ToArray();
            return complete;
        }

        /// <summary>
        /// 生成完整发送帧
        /// </summary>
        /// <param name="complete"></param>
        /// <returns>发送帧</returns>
        /// <exception cref="T:System.ArgumentException">存在重复的section</exception>>
        public byte[] GenerateSendByteFrame(string complete)
        {
var runtimeTokens= _sendFrameSections.OfType<FrameRuntimeSection>().ToDictionary(t => t.Name, t =>t);
            foreach (var pair in ParseImportValue(complete))
            {
                if (runtimeTokens.TryGetValue(pair.Key, out var token))
                {
                    //Todo: 加入值范围验证  Validation Rules
                    token.Value= ConvertValueStringToBytes(_encodeFormat, pair.Value,token.Length);
                }
                else
                {
                    throw new ArgumentException($"Name '{pair.Key}' in '{nameof(complete)}' does not correspond to a runtime token.", nameof(complete));
                }
            }
            //if (checkedToken == null)
            //{
            //    throw new ArgumentException("No checked section found in '_sendFrameSections'.", nameof(_sendFrameSections));
            //}
            return ExtractCompleteFrameFrom(_sendFrameSections);
        }
        
private  int GetFrameLength(IEnumerable<FrameSectionBase> sections)
{
     return sections.Select(s => s.Length).Sum();
}
/// <summary>
/// 抽出返回帧中的参数值
/// </summary>
/// <param name="response"></param>
/// <param name="sections"></param>
/// <exception cref="T:System.ArgumentOutOfException">Array.Copy可能产生的异常</exception>>
private void ExtractResponseValue(byte[] response, List<FrameSectionBase> sections)
{
    int curIndex = 0;
    foreach (var section in sections)
    {
        if (section is FrameRuntimeSection)
           Array.Copy(
               response,curIndex, 
               section.Value ,0,
               section.Length);
        curIndex += section.Length;
    }
}

/// <summary>
        /// 解析返回帧
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentException">存在重复的section</exception>>
public   bool ParseResponseByteFrame(byte[] response)
        {
            if (GetFrameLength(_responseFrameSections) != response.Length)
                throw new ArgumentException(
                    $"the length of {nameof(response)} is not equal to the total length of{nameof(_responseFrameSections)}",
                    nameof(response));
            
            ExtractResponseValue(response, _responseFrameSections);
            var complete = ExtractCompleteFrameFrom(_responseFrameSections);
            
            return complete.SequenceEqual(response);
        }


public string GenerateResponseScript(byte[] response)
{
    StringBuilder sb = new StringBuilder($"{_behavior} return ");
    if (ParseResponseByteFrame(response))
    {
        //_responseImportCache.ForEach(section => sb.Append($"{section.Name}= "));
        foreach (var section in _responseFrameSections.OfType<FrameRuntimeSection>())
        {
            string res=ConvertBytesToValueString(_encodeFormat, section.Value, section.Length);
            sb.Append($"{section.Name}={res} ");
        }
    }
    else sb.Append("Result Error");
    
    return sb.ToString();
}


    }
    
    }

