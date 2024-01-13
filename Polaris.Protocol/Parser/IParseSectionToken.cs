using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polaris.Protocol.Model;

namespace Polaris.Protocol.Parser
{
    [Obsolete]
    internal interface IParseSectionToken
    {

        bool IsRuntimeImportValue { get; }
        void Parse(FrameSectionToken token);
    }
}
