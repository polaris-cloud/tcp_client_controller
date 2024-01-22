using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Storage.Core
{
    internal interface ISettingConverter
    {
        void SaveSetting(object value, string containerPath, string fileName);
        T? GetSetting<T>(string containerPath, string fileName);
    }
}
