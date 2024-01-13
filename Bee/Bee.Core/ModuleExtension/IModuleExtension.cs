using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;

namespace Bee.Core.ModuleExtension
{
    public  interface IModuleExtension
    {
        object Icon { get; }
        string Name { get; }
        string Uri { get; }
        void OnAppDataInitialized(IContainerRegistry containerRegistry); 
    }
}
