using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Modules.Communication.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SupportViewSelectAttribute:Attribute
    {
        public string ItemName { get;  }
        public string Icon { get; }

        public SupportViewSelectAttribute(string itemName,string icon)
        {
            if (string.IsNullOrEmpty(itemName))
                throw new ArgumentNullException(nameof(itemName));
            if (string.IsNullOrEmpty(icon))
                throw new ArgumentNullException(nameof(icon));
            ItemName =itemName;
            Icon =icon;
        }
    }
}
