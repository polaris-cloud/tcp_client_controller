using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Core.SettingConverterTest
{
    internal interface ISettingConverter
    {
        void SaveSetting(Object? value, string containerPath,string fileName);
        T? GetSetting<T>(string containerPath,string fileName);
    }
}
