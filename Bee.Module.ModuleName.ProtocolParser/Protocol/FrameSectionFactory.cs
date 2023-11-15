using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol
{
    internal class FrameSectionFactory
    {

        private const string DefaultTokenPattern = @"<([^:?=]*)([:?=]+)([^<>]*)>";
        //匹配device={a2}   function={00}   
        private const string DefaultPairPattern = @"(\b[a-zA-Z0-9\u4e00-\u9fa5]+\b)=\{([a-zA-Z0-9]+)\}";
        private readonly string _tokenPattern;
        private readonly string _pairPattern;

        public FrameSectionFactory(string tokenRule= DefaultTokenPattern, string pairRule = DefaultPairPattern)
        {
            _tokenPattern=tokenRule ;
            _pairPattern=pairRule ;
        }
        
        
/// <summary>
///  解析帧 ，形如&lt;device&gt;:1 | &lt;function&gt;:1 | &lt;dataLength&gt;:1 | &lt;x轴:(-32767-32767)&gt;:4 |&lt;y轴:(-32767-32767)&gt;:4 |&lt;是否响应:&gt;:1 |&lt;crc?CRC16&gt;:2
/// </summary>
/// <param name="frameRule"></param>
/// <returns></returns>
protected internal IEnumerable<FrameSectionToken> ParseFrameRule(string frameRule)
        {
            var matches=Regex.Matches(frameRule, _tokenPattern);
            foreach (Match match in matches)
            {
                yield return new FrameSectionToken()
                {
                    SectionName = match.Groups[1].Value,
                    Operator = match.Groups[2].Value,
                    Action = match.Groups[3].Value,
                    Length = match.Groups[4].Value
                };
            }
        }

/// <summary>
/// 解析帧部分对应键值，形如device={a2} function={00}
/// </summary>
/// <param name="sectionKeyValuePairs"></param>
/// <returns></returns>
protected internal IEnumerable<KeyValuePair<string, byte[]>> ParseValuePair(string sectionKeyValuePairs)
{
    
    var matches = Regex.Matches(sectionKeyValuePairs, _pairPattern);
    foreach (Match match in matches)
    {
        yield return new KeyValuePair<string, byte[]>(match.Groups[1].Value, 
            match.Groups[2].Value.Split(' ').Select(s=>Convert.ToByte(s,16)).ToArray());
        //Enumerable.Range(0, hexValues.Length / 2)
        //    .Select(i => Convert.ToByte(hexValues.Substring(i * 2, 2), 16))
        //    .ToArray();
            }
    
}

        internal  IEnumerable<FrameSectionBase> ParseSectionTokens(
            IEnumerable<FrameSectionToken> tokens,
            IEnumerable<KeyValuePair<string, byte[]>> pairs)
        {
            var dic =pairs.ToDictionary(t => t.Key, t => t.Value);
            foreach (var token in tokens)
            {
                if (string.IsNullOrEmpty(token.Operator))
                {
                    dic.TryGetValue(token.SectionName, out byte[]?value);
                    yield return new FrameSectionBase(token,value?? Array.Empty<byte>() );
                }
                if (token.Operator == ":")
                    yield return new FrameRuntimeSection(token);
                if (token.Operator == "?")
                    yield return new FrameCheckedSection(token);
            }
        }

        internal IEnumerable<FrameSectionBase> ParseSection(string rule)
        {
            return ParseSectionTokens(ParseFrameRule(rule), ParseValuePair(rule));
        }
        
        

    }
}
