using System.Windows.Media;

namespace Polaris.Console;

public class ConsoleBridgeArgs : EventArgs
{
    public ConsoleBridgeArgs(string content,Brush brush)
    {
        Content=content;
        Brush=brush;
    }
    public string Content { get;  } 
    public Brush Brush { get ; } 
}