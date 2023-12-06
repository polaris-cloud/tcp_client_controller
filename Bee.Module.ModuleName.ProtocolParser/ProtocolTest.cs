using System.Diagnostics;
using System.Runtime.CompilerServices;
using Bee.Core.Protocol;
using Bee.Core.Protocol.enums;
using Bee.Core.Protocol.Model;
using Bee.Core.Protocol.Model.Section;

namespace Bee.Module.Script.ProtocolParser
{
    [TestClass]
    public class ProtocolTest
    {
        string sendRule =
            @"<={a2 00 09}><x��:(-32767/32767)|4><y��:(-32767/32767)|4><�Ƿ���Ӧ:(0/1)|1><?CRC16|2>";

        string reponseRule = @"<={a2 00 00}><?crc16|2>}";



        //string reponseRule = "";
        private FrameSectionFactory factory = new FrameSectionFactory();

        [TestMethod]
        public void TestParseFrameRule()
        {

            Trace.WriteLine($"��������");
            foreach (var p in factory.ParseFrameRule(sendRule))
            {
                Trace.WriteLine($"{p.SectionName}| {p.Operator} |{p.Action}| {p.Length}");
            }
            Trace.WriteLine($"��������");
            foreach (var p in factory.ParseFrameRule(reponseRule))
            {
                Trace.WriteLine($"{p.SectionName}| {p.Operator} |{p.Action}| {p.Length}");
            }
        }


        //[TestMethod]
        //[Obsolete]
        //public void TestParseValuePair()
        //{
        //    foreach (var p in factory.ParseValuePair(sendRule))
        //    {
        //        Trace.Write($" {p.Key} {string.Join(" ", p.Value.Select(t => t.ToString("X2")))}\n");
        //    }
        //}

        [TestMethod]
        public void TestParseSectionTokens()
        {

            foreach (var p in factory.ParseSection(sendRule, ProtocolEndian.BigEndian, ProtocolEncodeFormat.Hex))
            {
                Trace.Write($"{p.GetType().FullName} \n");
            }
        }

        [TestMethod]
        public void TestValidateRuntimeToken()
        {

            foreach (var p in factory.ParseSection(sendRule, ProtocolEndian.BigEndian, ProtocolEncodeFormat.Hex))
            {
                if (p is FrameRuntimeSection r)
                    Trace.WriteLine(r.Validate("33000"));
            }
        }

        [TestMethod]
        public void TestParseProtocol()
        {
            ProtocolFormat format = new ProtocolFormat()
            {
                SendFrameDescription = "���ƶ�",
                BehaviorKeyword = "MoveOsc",
                SendFrameRule = sendRule,
                ResponseFrameRule = reponseRule
            };
            ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            string sendScript = protocolScriptParser.GenerateSendScript();
            //string responseScript = protocolScriptParser.GenerateResponseScript();

            Trace.WriteLine(sendScript);
            //Trace.WriteLine(responseScript);
        }

        [TestMethod]
        public void TestConvertValue()
        {
            ProtocolFormat format = new ProtocolFormat()
            { SendFrameDescription = "���ƶ�", SendFrameRule = sendRule, ResponseFrameRule = reponseRule };
            ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            //var ar = protocolScriptParser.ConvertValueStringToBytes(ProtocolEncodeFormat.Hex, "12040",4);
        }

        [TestMethod]
        public void TestGenerateSendFrame()
        {
            ProtocolFormat format = new ProtocolFormat()
            { SendFrameDescription = "��x���ƶ�{x��},y���ƶ�{y��}", SendFrameRule = sendRule, ResponseFrameRule = reponseRule };
            ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            var ar = protocolScriptParser.GenerateSendFrame("MoveOsc x��=1000 y��=1000 �Ƿ���Ӧ=1", out string debugLine);
            var s = ar.Select(a => a.ToString("X2"));
            Trace.WriteLine(string.Join(" ", s));
            Trace.WriteLine(debugLine);
        }
        [TestMethod]
        public void TestGenerateResponseResult()
        {
            ProtocolFormat format = new ProtocolFormat()
            { SendFrameDescription = "���ƶ�", SendFrameRule = sendRule, ResponseFrameRule = sendRule };
            ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            protocolScriptParser.ParseResponseByteFrame(new byte[] { 0xa2, 00, 09, 00, 00, 03, 0xe8, 0, 0, 0x03, 0xe8, 0x01, 0x95, 0xc3 });

        }

        [TestMethod]
        public void TestGenerateResponseScript()
        {
            ProtocolFormat format = new ProtocolFormat()
            { ResponseFrameDescription = "��x���ƶ�{x��},y���ƶ�{y��}", BehaviorKeyword = "MoveOsc", SendFrameRule = sendRule, ResponseFrameRule = sendRule };
            ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            var res = protocolScriptParser.GenerateResponseDebugLine(new byte[] { 0xa2, 00, 09, 00, 00, 03, 0xe8, 0, 0, 0x03, 0xe8, 0x01, 0x95, 0xc3 });
            Trace.WriteLine(res);
        }



        [TestMethod]
        public void TestReplacePlaceholders()
        {
            //ProtocolFormat format = new ProtocolFormat()
            //    { Name = "���ƶ�", BehaviorKeyword = "MoveOsc", SendFrameRule = sendRule, ResponseFrameRule = sendRule };
            //ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(format);
            //var res = protocolScriptParser.GenerateResponseScript(new byte[] { 0xa2, 00, 09, 00, 00, 03, 0xe8, 0, 0, 0x03, 0xe8, 0x01, 0x95, 0xc3 });
            //Trace.WriteLine(res);

            string inputString = "��x���ƶ�{x��}����y���ƶ�{y��}��";
            string[] valuepair = new string[]
            {
                "x��=1000",
                "y��=1000"
            };
            var dic = valuepair.Select(t => t.Split("=")).
                ToDictionary(t => t[0], t => t[1]);

            foreach (var pair in dic)
            {
                inputString = inputString.Replace($"{{{pair.Key}}}", pair.Value);
            }

            Trace.WriteLine(inputString);

        }




        public class Test
        {
            public string Name { get; set; }
            public int Num { get; set; }

            public Test()
            {
                Trace.WriteLine("call constructor");
            }
        }
        [TestMethod]
        //��һ��ö�ٵ�һЩ����
        public void TestEnumerable()
        {
            //var enumerable = Enumerable.Range(1, 5).Select(_ =>
            //{
            //     Trace.WriteLine("������Test");
            //    return new Test();
            //});
            //Trace.WriteLine("����һ��ö����,�ӳټ��ء����±�ö�ٱ߹������");
            //foreach (var i in enumerable)
            //{
            //    int hashCode1 = RuntimeHelpers.GetHashCode(i);
            //    Trace.WriteLine($"hashcode :{hashCode1}");
            //}
            //Debug Trace:
            //����һ��ö����,�ӳټ��ء����±�ö�ٱ߹������
            //call constructor
            //hashcode :8970412
            //call constructor
            //hashcode: 22933728
            //call constructor
            //hashcode: 36472305
            //call constructor
            //hashcode: 39392417
            //call constructor
            //hashcode: 45286344

            //var list = Enumerable.Range(1, 5).Select(_ => new Test()).ToList();
            //Trace.WriteLine("����һ���б�,�Ѿ���ʼ�����ж���");
            //foreach (var i in list)
            //{
            //    int hashCode1 = RuntimeHelpers.GetHashCode(i);
            //    Trace.WriteLine($"hashcode :{hashCode1}");
            //}
            //Debug Trace:
            //call constructor
            //call constructor
            //call constructor
            //call constructor
            //call constructor
            //����һ���б�,�Ѿ���ʼ�����ж���
            //hashcode :8970412
            //hashcode: 22933728
            //hashcode: 36472305
            //hashcode: 39392417
            //hashcode: 45286344

            var enumerable = Enumerable.Range(1, 5).Select(i =>
            {
                return new Test() { Num = i };
            }).Where(test =>
            {
                Trace.WriteLine(test.Num);
                return test.Num > 3;
            });
            Trace.WriteLine("����һ��ö����,�ӳټ��ء����±�ö�ٱ߹������");
            foreach (var i in enumerable)
            {
                int hashCode1 = RuntimeHelpers.GetHashCode(i);
                Trace.WriteLine($"hashcode :{hashCode1}");
            }
        }

    }
}