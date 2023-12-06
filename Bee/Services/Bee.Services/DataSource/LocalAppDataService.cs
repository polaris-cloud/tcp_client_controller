using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using Bee.Core.Converters;
using Bee.Services.Interfaces.DataSource;
using Newtonsoft.Json;

namespace Bee.Services.DataSource
{
    
    public class LocalAppDataService :IAppDataSourceService
    {

        private readonly Dictionary<string, IAppDataSource> _dataSourcesDictionary ;

        private string _folderPath;

        /// <summary>
        /// 初始化app service
        /// </summary>
        /// <param name="types"></param>
        /// <exception cref="ArgumentException"> 加入</exception>>
        public LocalAppDataService(IEnumerable<IAppDataSource> types)
        {
            _dataSourcesDictionary=new Dictionary<string, IAppDataSource>();
            foreach (var type in types)
            {
                _dataSourcesDictionary.Add(type.Contract,type);
            }
            
        }

/// <summary>
/// 
/// </summary>
/// <param name="appDataFolderPath"></param>
/// <exception cref="ArgumentException"></exception>> 
        public void LoadAppDataSources(string appDataFolderPath)
        {
            _folderPath= appDataFolderPath;
            if (string.IsNullOrEmpty(_folderPath))
                throw new ArgumentException($"{nameof(appDataFolderPath)} is null or empty",nameof(appDataFolderPath));
            
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
            foreach (var appSetting in  _dataSourcesDictionary)
            {
                var appDataPath = Path.Combine(appDataFolderPath, appSetting.Key + ".json");
                if (File.Exists(appDataPath))
                {
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    settings.Converters.Add(new IPAddressConverter());
                    settings.Converters.Add(new IPEndPointConverter());
                    var appData =
                        (IAppDataSource)JsonConvert.DeserializeObject(File.ReadAllText(appDataPath),
                            appSetting.GetType(),
                            settings);
                    //iAppSetting?.Initialize();
                    _dataSourcesDictionary[appSetting.Key] = appData;
                }
            }
            
        }




        public IAppDataSource FetchDataSource(string contract)
        {
            if(_dataSourcesDictionary.TryGetValue(contract, out var source))
                  return source;
            return default;
        }

        public T FetchDataSource<T>() where T : IAppDataSource, new()
        {
            var appData = _dataSourcesDictionary[typeof(T).Name];
            return (T)appData;
        }
        
        
        public void SaveDataSource(IAppDataSource source)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Formatting = Formatting.Indented;
            
            var json = JsonConvert.SerializeObject(source, settings);
            var file = Path.Combine(_folderPath, source.Contract + ".json");
            File.WriteAllText(file, json);
            
            //if all filed equals
            if (!Equals(source, _dataSourcesDictionary[source.Contract]))
            {
                _dataSourcesDictionary[source.Contract] = source;
            }

            
        }
    }
}
