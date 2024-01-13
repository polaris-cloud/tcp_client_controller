using System;

namespace Bee.Core.DataSource
{
    public interface IAppData : ICloneable
    {
        string SubDir { get; }
        string Contract { get; }
    }
}