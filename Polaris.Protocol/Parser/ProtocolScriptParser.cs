using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Polaris.Protocol.Converter;
using Polaris.Protocol.enums;
using Polaris.Protocol.Exceptions;
using Polaris.Protocol.Model;
using Polaris.Protocol.Model.Section;

namespace Polaris.Protocol.Parser
{
    public class ProtocolScriptParser
    {
        private readonly string _behavior;
        private readonly string _sendDescription;
        private readonly string _responseDescription;
        private readonly ProtocolEndian _endian;
        private readonly ProtocolEncodeFormat _encodeFormat;
        private readonly List<FrameSectionBase> _sendFrameSections;
        private readonly List<FrameSectionBase> _responseFrameSections;
        private readonly string _importValuePattern = @"(\b[a-zA-Z0-9\u4e00-\u9fa5]+\b)=([a-zA-Z0-9]+)";
        public int ResponseFrameLength => GetFrameLength(_responseFrameSections);
        public int SendFrameLength => GetFrameLength(_sendFrameSections);

        private ProtocolScriptParser(
            string behavior,
            string sendDescription,
            string responseDescription,
            List<FrameSectionBase> sendFrameSections,
            List<FrameSectionBase> responseFrameSections,
            ProtocolEndian endian,
            ProtocolEncodeFormat encodeFormat)
        {
            _behavior = behavior;
            _sendDescription = sendDescription;
            _responseDescription = responseDescription;
            _sendFrameSections = sendFrameSections;
            _responseFrameSections = responseFrameSections;
            _endian = endian;
            _encodeFormat = encodeFormat;
        }

        public static ProtocolScriptParser BuildScriptParser(ProtocolFormat format)
        {
            var behavior = format.BehaviorKeyword!;
            var sendDescription = format.SendFrameDescription;
            var responseDescription = format.ResponseFrameDescription;
            var endian = format.ProtocolEndian;
            var encodeFormat = format.EncodeFormat;
            var factory = new FrameSectionFactory();
            if (format.SendFrameRule == null || format.ResponseFrameRule == null)
                throw new ArgumentNullException($"{nameof(format.SendFrameRule)} or {nameof(format.ResponseFrameRule)}");
            var parseSend = factory.ParseSection(format.SendFrameRule, format.ProtocolEndian, format.EncodeFormat);
            var parseResponse = factory.ParseSection(format.ResponseFrameRule, format.ProtocolEndian, format.EncodeFormat);

            return new ProtocolScriptParser(behavior, sendDescription, responseDescription, parseSend.ToList(), parseResponse.ToList(), endian, encodeFormat);
        }

        public string GenerateSendScript()
        {
            StringBuilder sb = new StringBuilder($"{_behavior}> ");
            foreach (var section in _sendFrameSections.OfType<FrameRuntimeSection>())
                sb.Append($"{section.Name}= ");
            return sb.ToString();
        }


        private IEnumerable<KeyValuePair<string, string>> ParseImportValue(string complete)
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
                .Select(t => t.Storage).SelectMany(a => a).ToArray();
            var checkedSection = sections.OfType<FrameCheckedSection>().FirstOrDefault();
            var check = checkedSection != null ?
                checkedSection.GetBytes(_endian, toCheckedByteArray)
                : Array.Empty<byte>();
            byte[] complete = toCheckedByteArray.Concat(check).ToArray();
            return complete;
        }

        private bool ValidateRawRulesInternal(IEnumerable<FrameRuntimeSection> runtimeTokens)
        {
            return FrameRuntimeSection.Validate(runtimeTokens); 
        }

        public bool ValidateRawRules()
        {
            var sendTokens = _sendFrameSections.OfType<FrameRuntimeSection>().ToList();
            var responseTokens=_responseFrameSections.OfType<FrameRuntimeSection>().ToList();
            return FrameRuntimeSection.Validate(sendTokens) && FrameRuntimeSection.Validate(responseTokens);
        }


        /// <summary>
        /// 生成完整发送帧
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="debugLine"></param>
        /// <returns>发送帧</returns>
        /// <exception cref="T:System.ArgumentException">runtimeSection验证失败</exception>>
        public byte[] GenerateSendFrame(string complete, out string debugLine)
        {
            debugLine = _sendDescription;
            var runtimeTokens = _sendFrameSections.OfType<FrameRuntimeSection>().ToList();
            if (!ValidateRawRulesInternal(runtimeTokens))
                throw new SectionParseException($"'{nameof(runtimeTokens)}' in '{nameof(complete)}' 规则验证失败:存在空值或者重复项", nameof(complete));
            
            var runtimeTokensDic = runtimeTokens.ToDictionary(t => t.Name!, t => t);
            var parsedPairs = ParseImportValue(complete).ToList();
             if(runtimeTokensDic.Count!=parsedPairs.Count)
                 throw new SectionParseException($"'{nameof(runtimeTokens)}' in '{nameof(complete)}' 运行时验证失败:存在空值或token识别错误", nameof(complete));
            foreach (var pair in parsedPairs)
            {
                if (runtimeTokensDic.TryGetValue(pair.Key, out var token))
                {
                    if (!token.Validate(pair.Value))
                        throw new SectionParseException($"'{pair.Key}' in '{complete}' 运行时验证失败: 参数设定值超过范围( {token.LowerLimit}~{token.UpperLimit} )", nameof(complete));

                    debugLine = debugLine.Replace($"{{{pair.Key}}}", pair.Value);

                    token.Storage = token.GetBytes(_encodeFormat, _endian, pair.Value);
                }
                else
                {
                    throw new ArgumentException($"SendFrameDescription '{pair.Key}' in '{nameof(complete)}' does not correspond to a runtime token.", nameof(complete));
                }
            }
            
            return ExtractCompleteFrameFrom(_sendFrameSections);
        }

        private int GetFrameLength(IEnumerable<FrameSectionBase> sections)
        {
            int sum = 0;
            foreach (var section in sections)
            {
                sum += section.Length;
            }
            return sum;
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
                        response, curIndex,
                        section.Storage, 0,
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
        public bool ParseResponseByteFrame(byte[] response)
        {
            
                //throw new ArgumentException(
                //    $"the length of {nameof(response)} is not equal to the total length of{nameof(_responseFrameSections)}",
                //    nameof(response));

            ExtractResponseValue(response, _responseFrameSections);
            var complete = ExtractCompleteFrameFrom(_responseFrameSections);

            return complete.SequenceEqual(response);
        }

        public bool CheckResponseFrameLength(IEnumerable<byte> response)
        {
            return GetFrameLength(_responseFrameSections) != response.Count();
        }

        public int  GetResponseFrameLength()
        {
            return GetFrameLength(_responseFrameSections);
        }
        public string GenerateResponseScript(byte[] response)
        {
            StringBuilder sb = new StringBuilder($"{_behavior} return ");
            if (ParseResponseByteFrame(response))
            {
                //_responseImportCache.ForEach(section => sb.Append($"{section.SendFrameDescription}= "));
                foreach (var section in _responseFrameSections.OfType<FrameRuntimeSection>())
                {
                    string res = FrameConverter.ConvertBytesToValueString(_endian, _encodeFormat, section.Storage, section.Length);
                    sb.Append($"{section.Name}={res} ");
                }
            }
            else sb.Append("Result Error");

            return sb.ToString();
        }

        public string GenerateResponseDebugLine(byte[] response)
        {
            StringBuilder sb = new StringBuilder($"{_behavior} => ");
            string output = _responseDescription;
            if (ParseResponseByteFrame(response))
            {
                //_responseImportCache.ForEach(section => sb.Append($"{section.SendFrameDescription}= "));
                foreach (var section in _responseFrameSections.OfType<FrameRuntimeSection>())
                {
                    string res = FrameConverter.ConvertBytesToValueString(_endian, _encodeFormat, section.Storage, section.Length);
                    output = output.Replace($"{{{section.Name}}}", res);
                }
                sb.Append(output);
            }
            else sb.Append("Result Error");

            return sb.ToString();
        }

    }

}

