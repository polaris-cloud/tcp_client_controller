using System.Windows.Media;

namespace ScriptEditorTest.ScriptConsole;

public class ConsoleBridgeArgs(string content, Brush brush) : EventArgs
{
    public string Content { get;  } = content;
    public Brush Brush { get ; } =brush ;
}