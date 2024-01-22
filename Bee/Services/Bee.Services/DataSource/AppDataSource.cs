using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Bee.Core.Converters;
using Bee.Core.DataSource;
using Bee.Core.Json;
using Newtonsoft.Json;

namespace Bee.Services.DataSource
{

    public class AppDataSource : IAppDataSource
    {
        private readonly string _extension = ".json";

        public async Task SaveDataAsync(string rootPath, IAppData data)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Formatting = Formatting.Indented;

            var json = JsonConvert.SerializeObject(data, settings);
            var file = Path.Combine(rootPath, data.SubDir,data.Contract + _extension);
            await File.WriteAllTextAsync(file, json);

        }

        public async Task<T> LoadDataAsync<T>(string folder, string contract) where T : IAppData
        {
            return (T)await LoadDataAsync(typeof(T), folder, contract);
        }

        public  async Task<IAppData> LoadDataAsync(Type type, string folder, string contract)
        {


            var appDataPath = Path.Combine(folder, contract + ".json");
            if (File.Exists(appDataPath))
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new IPEndPointConverter());

                 
                var appData =
                    JsonConvert.DeserializeObject(await File.ReadAllTextAsync(appDataPath),
                        type,
                        settings);
                return appData as IAppData;
            }

            return default;
        }

    }
}
