using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee.Core.Protocol.Exceptions
{
    internal class SectionParseException : ArgumentException
    {
        public SectionParseException(string message, string paraName) : base(message, paraName) { }
    }
}
