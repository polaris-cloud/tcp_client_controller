using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Bee.Core.SettingConverterTest
{
    [TestClass]
    public class UnitTest
    {

        public class TestObject
        {
            public int Num;
            public string Name;
        }

        [TestMethod]
        public async Task SerializeSetting()
        {
            ToJsonConverter converter = new ToJsonConverter();
            TestObject testObject = new TestObject(){Num =1, Name ="game"};
            converter.SaveSetting(testObject,AppDomain.CurrentDomain.BaseDirectory,"test.json");
            //File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.json"));
        }
        [TestMethod]
        public async Task DeserializeSetting()
        {
            ToJsonConverter converter = new ToJsonConverter();
            TestObject? obj=converter.GetSetting<TestObject>(AppDomain.CurrentDomain.BaseDirectory, "test.json");
            Trace.WriteLine($"{obj?.Name},{obj?.Num}");
        }
    }
}