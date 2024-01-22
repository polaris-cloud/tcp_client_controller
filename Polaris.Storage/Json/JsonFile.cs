using System;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace Polaris.Storage.Json
{
    public class JsonFile
    {
        public static async Task SaveDataAsync(object value, string folderPath, string fileName,CancellationToken token)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            settings.Converters.Add(new IPAddressConverter());
            settings.Formatting = Formatting.Indented;

            var json = JsonConvert.SerializeObject(value, settings);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var path = Path.Combine(folderPath, fileName);
            await File.WriteAllTextAsync(path, json,token);

        }


        public static async Task SaveDataAsync(object value, string fullPath, CancellationToken token)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            settings.Converters.Add(new IPAddressConverter());
            settings.Formatting = Formatting.Indented;

            var json = JsonConvert.SerializeObject(value, settings);
            
            await File.WriteAllTextAsync(fullPath, json, token);

        }


        
        
        public static async Task<T?> GetDataAsync<T>(string fullPath, CancellationToken token)
        {
            if (File.Exists(fullPath))
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                settings.Converters.Add(new IPAddressConverter());
                var file = await File.ReadAllTextAsync(fullPath,token);
                return JsonConvert.DeserializeObject<T>(file, settings);
            }
            return default;
        }
    }
}
