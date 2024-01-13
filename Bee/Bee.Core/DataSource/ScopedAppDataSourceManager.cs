using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Win32;
using Prism.Ioc;

namespace Bee.Core.DataSource
{
    public  class ScopedAppDataSourceManager
    {
        private readonly  IAppDataSource _appDataSource;
        private readonly AppDataCache _cache;
        private readonly IEnumerable<IAppData> _appTypes;

        //private readonly Dictionary<Type, IAppData> _mappings = new Dictionary<Type, IAppData>();

        private string root; 
        
        public EventHandler<string> WhenAppDataLoad; 

        public ScopedAppDataSourceManager(
            IAppDataSource dataSource,
            AppDataCache cache ,
            IEnumerable<IAppData> appTypes
            )
        {
            _appDataSource=dataSource;
            _cache = cache;
            _appTypes = appTypes;
        }
        
        
        public void InitializeAppCache(
            IEnumerable<Type> appTypes
        )
        {
            foreach (var type  in appTypes)
            {
                if (typeof(IAppData).IsAssignableFrom(type))
                {
                    var placeHolderInstance = Activator.CreateInstance(type) as IAppData;
                    //_mappings.Add(type,placeHolderInstance);
                    _cache.Register(type,placeHolderInstance);
                }
                else
                {
                    throw new  ArgumentException($"Type : {type?.Name} does not implement IAppData",nameof(_appTypes));
                }
                 
            }
        }

        public void InitializeAppCache(string rootDir)
        {
            foreach (var appData in _appTypes)
            {

                _cache.Register(appData.GetType(), appData);
                }

            root = rootDir;
        }
        

        public async Task LoadFromDataSourceAsync()
        {
            IAppData appData = default;
            try
            {
                if (string.IsNullOrEmpty(root))
                    throw new ArgumentNullException(nameof(root));


                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);

                foreach (var type in _cache.GetAppTypes)
                {

                     appData = _cache.GetMapping(type);
                    var folder = Path.Combine(root, appData.SubDir);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var load = await _appDataSource.LoadDataAsync(type, folder, appData.Contract);
                    WhenAppDataLoad?.Invoke(this,
                        load == null ?
                            $"Module: {appData.SubDir} Setting: {appData.Contract} Loaded failed" :
                            $"Module: {appData.SubDir} Setting: {appData.Contract} Loaded");
                    if (load != default)
                    {
                        _cache.ReplaceOld(type, load);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Module: {appData?.SubDir} Setting: {appData?.Contract} Loaded failed  ( Error: {ex.Message} )", ex); 
            }

            
        }

        public async Task SaveToDataSourceAsync(IAppData data)
        {
            await _appDataSource.SaveDataAsync(root,data);

            if (!ReferenceEquals(data,_cache.GetMapping(data.GetType())))
            {
                _cache.ReplaceOld(data.GetType(), data);
            }
            
        }
    }

    
}
