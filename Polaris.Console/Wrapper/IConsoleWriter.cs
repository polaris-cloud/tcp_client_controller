using Polaris.Console.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Console.Wrapper
{
    
    public interface IConsoleWriter
    {
        public void ConsoleWrite(MessageRank rank, string content);
    }
}
