using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Core.DataSource;

namespace Bee.Modules.Communication.Shared
{
    internal interface IAppDataApply<in T> where T : IAppData
    {
          void ApplyAppData(T appData);
         void SaveAppData(T appData);
    }
}
