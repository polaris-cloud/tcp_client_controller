using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using ScriptEditorTest.ScriptConsole.Stream;

namespace ScriptEditorTest.ScriptConsole
{
    //public class ConsoleStream:IConsoleStream
    //{
    //    public event EventHandler<ConsoleInteractiveArgs>? OnConsoleRead;
    //    public void Write(string content)
    //    {
    //        Debug.WriteLine(content);
    //    }
    //    public event EventHandler<ConsoleInteractiveArgs>? OnConsoleError;


    //    public static readonly ConsoleStream Empty  = new ConsoleStream();
    //}


    public interface IConsoleStream
    {
        event EventHandler<ConsoleStreamEventArgs> OnConsoleRead;
bool  Write(string content);
        
    }
}
