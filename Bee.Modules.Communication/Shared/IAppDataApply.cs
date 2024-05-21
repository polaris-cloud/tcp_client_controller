using Bee.Core.DataSource;

namespace Bee.Modules.Communication.Shared
{
    internal interface IAppDataApply<in T> where T : IAppData
    {
          void ApplyAppData(T appData);
         void SaveAppData(T appData);
    }
}
