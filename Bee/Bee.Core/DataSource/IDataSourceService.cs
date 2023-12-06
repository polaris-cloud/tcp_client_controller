using System;
using System.Collections.Generic;
using System.Text;

namespace Bee.Core.DataSource
{

    internal interface IDataSource : ICloneable
    {
string Contract { get; }
    }





    internal interface IDataSourceService
    {
        

        IDataSource FetchDataSource(string contract);

        T FetchDataSource<T>() where T : IDataSource, new();

        void Initialize();
        
        void SaveDataSource(IDataSource source);
        

    }
}
