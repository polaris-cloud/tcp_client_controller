using System;
using Newtonsoft.Json;
using System.IO;
using Bee.Core.Converters;

namespace Bee.Core.Json
{
public  class ToJsonConverter : ISettingConverter
    {
        public  void SaveSetting(object value, string containerPath, string fileName)
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
                var file =File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(file, settings);
            }
            return default;
        }
    }
}
