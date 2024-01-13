using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Prism.Regions;

namespace Bee.CORE.RegionRegister
{
    internal class RegionViewRegistry:IRegionViewRegistry
    {
        public IEnumerable<object> GetContents(string regionName)
        {
            throw new NotImplementedException();
            
        }

        public void RegisterViewWithRegion(string regionName, Type viewType)
        {
            throw new NotImplementedException();
        }

        public void RegisterViewWithRegion(string regionName, Func<object> getContentDelegate)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ViewRegisteredEventArgs> ContentRegistered;
    }
}
