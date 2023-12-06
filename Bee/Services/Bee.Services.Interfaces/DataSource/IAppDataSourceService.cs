using System.Collections.Generic;
using System.Text;

namespace Bee.Services.Interfaces.DataSource
{
public interface IAppDataSourceService
    {


        IAppDataSource FetchDataSource(string contract);

        T FetchDataSource<T>() where T : IAppDataSource, new();

        void SaveDataSource(IAppDataSource source);


    }
}
