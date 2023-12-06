using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Core.Protocol.Model;

namespace Bee.Core.Protocol
{
    [Obsolete]
    internal interface IParseSectionToken
    {

        bool IsRuntimeImportValue { get; }
        void Parse(FrameSectionToken token);
    }
}
