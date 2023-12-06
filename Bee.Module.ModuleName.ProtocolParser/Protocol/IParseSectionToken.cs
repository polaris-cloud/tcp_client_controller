using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Module.ModuleName.ProtocolParser.Protocol.Model;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol
{
    [Obsolete]
    internal interface IParseSectionToken
    {

        bool IsRuntimeImportValue { get; }
        void Parse(FrameSectionToken token);
    }
}
