using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Modules.Script.Models
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class KeyValuePairIgnoreAttribute : Attribute
    {
        // You can include properties or methods specific to the attribute if needed
    }
}
