using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Frameworks;

namespace Bee.Module.ModuleName.ProtocolParser.Protocol
{
    internal interface IParseSectionToken
    {

         bool IsRuntimeImportValue { get; }
         void Parse(FrameSectionToken token); 
    }
}
