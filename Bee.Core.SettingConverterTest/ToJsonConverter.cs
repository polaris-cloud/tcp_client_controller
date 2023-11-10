using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MotorDetection.SettingManager;

namespace Bee.Core.SettingConverterTest
{
    internal class ToJsonConverter : ISettingConverter
    {
        public void SaveSetting(object? value, string containerPath, string fileName)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Formatting = Formatting.Indented;

            var json = JsonConvert.SerializeObject(value, settings);
            if (!Directory.Exists(containerPath))
                Directory.CreateDirectory(containerPath);
            var path = Path.Combine(containerPath, fileName);
            File.WriteAllText(path, json);

        }

        public T? GetSetting<T>(string containerPath, string fileName)
        {
            var path = Path.Combine(containerPath, fileName);
            if (File.Exists(path))
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.All;
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new IPEndPointConverter());
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), settings);
            }
            return default(T);
        }
    }
}
