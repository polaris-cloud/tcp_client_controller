using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Bee.Module.ModuleName.ProtocolParser.Protocol;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Bee.Module.ModuleName.ProtocolParser
{
    [TestClass]
    public class UnitTest
    {
        string sendRule =
            @"<device>:1 | <function>:1 | <datalength>:1 | <x��:(-32767-32767)>:4 |<y��:(-32767-32767)>:4 |<�Ƿ���Ӧ:>:1 |<crc?CRC16>:2
                       device={a2}
                        function={00}
                     datalength={09}";

        string reponseRule = @"<device>:1 | <function>:1 | <datalength>:1 | <crc?CRC16>:2
 device={a2}
 function={00}
 datalength={00}";
        
        
        
        //string reponseRule = "";
        private FrameSectionFactory factory = new FrameSectionFactory();

        [TestMethod]
        public void TestParseFrameRule()
        {


            foreach (var p in factory.ParseFrameRule(sendRule))
            {
                Trace.Write($"{p.SectionName} {p.Operator} {p.Action} {p.Length}\n");
            }
        }


        [TestMethod]
        public void TestParseValuePair()
        {
            foreach (var p in factory.ParseValuePair(sendRule))
            {
                Trace.Write($" {p.Key} {string.Join(" ", p.Value.Select(t => t.ToString("X2")))}\n");
            }
        }

        [TestMethod]
        public void TestParseSectionTokens()
        {

            foreach (var p in factory.ParseSection(sendRule))
            {
                Trace.Write($"{p.GetType().FullName} {p.Name} {p.Length} \n");
            }
        }



        [TestMethod]
        public void TestParseProtocol()
        {
            ProtocolFormat format = new ProtocolFormat()
            {
                Name = "���ƶ�", BehaviorKeyword = "MoveOsc", SendFrameRule = sendRule, ResponseFrameRule = reponseRule
            };
            ProtocolScript protocolScript = Converter.ToProtocolScript(format);
            string sendScript = protocolScript.GenerateSendScript();
            //string responseScript = protocolScript.GenerateResponseScript();

            Trace.WriteLine(sendScript);
            //Trace.WriteLine(responseScript);
        }

        [TestMethod]
        public void TestConvertValue()
        {
            ProtocolFormat format = new ProtocolFormat()
                { Name = "���ƶ�", SendFrameRule = sendRule, ResponseFrameRule = reponseRule };
            ProtocolScript protocolScript = Converter.ToProtocolScript(format);
            var ar = protocolScript.ConvertValueStringToBytes(ProtocolEncodeFormat.Hex, "12040",4);
        }

        [TestMethod]
        public void TestGenerateSendFrame()
        {
            ProtocolFormat format = new ProtocolFormat()
                { Name = "���ƶ�", SendFrameRule = sendRule, ResponseFrameRule = reponseRule };
            ProtocolScript protocolScript = Converter.ToProtocolScript(format);
            var ar = protocolScript.GenerateSendByteFrame("MoveOsc x��=1000 y��=1000 �Ƿ���Ӧ=1");
            var s = ar.Select(a => a.ToString("X2"));
            Trace.WriteLine(string.Join(" ", s));
        }
        [TestMethod]
        public void TestGenerateResponseResult()
        {
            ProtocolFormat format = new ProtocolFormat()
                { Name = "���ƶ�", SendFrameRule = sendRule, ResponseFrameRule = sendRule };
            ProtocolScript protocolScript = Converter.ToProtocolScript(format);
            protocolScript.ParseResponseByteFrame(new byte[] { 0xa2, 00, 09,00,00,03,0xe8,0,0,0x03,0xe8,0x01, 0x95,0xc3});

        }

        [TestMethod]
        public void TestGenerateResponseScript()
        {
            ProtocolFormat format = new ProtocolFormat()
                { Name = "���ƶ�",BehaviorKeyword = "MoveOsc", SendFrameRule = sendRule, ResponseFrameRule = sendRule };
            ProtocolScript protocolScript = Converter.ToProtocolScript(format);
            var res=protocolScript.GenerateResponseScript(new byte[] { 0xa2, 00, 09, 00, 00, 03, 0xe8, 0, 0, 0x03, 0xe8, 0x01, 0x95, 0xc3 });
Trace.WriteLine(res);
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
                return new Test(){Num= i};
            }).Where(test=>
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