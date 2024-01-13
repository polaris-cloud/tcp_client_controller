using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Core.DataSource
{
    public interface IAppDataSource
    {
        
Task   SaveDataAsync(string rootPath, IAppData data);
        Task<T> LoadDataAsync<T>(string path,string contract) where T : IAppData;
         Task<IAppData> LoadDataAsync(Type type, string path, string contract); 
    }
}
