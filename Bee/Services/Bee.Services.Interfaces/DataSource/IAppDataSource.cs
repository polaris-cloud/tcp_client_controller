using System;

namespace Bee.Services.Interfaces.DataSource
{
    public interface IAppDataSource : ICloneable
    {
        string Contract { get; }
    }
}