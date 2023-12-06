using System.Windows.Forms;

namespace ScriptEditorTest.ScriptConsole.Stream;

public enum MessageRank
{
    None,
    Rule,
    Error,
    Tip,
    Diagnostic
}



/// <inheritdoc />
/// <summary>
/// The ProcessEventArgs are arguments for a console event.
/// </summary>
public class ConsoleStreamEventArgs : EventArgs
{
    /// <summary>
    /// Gets the process input/output content.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Flag whether the <see cref="Content"></see> is from  stream 
    /// </summary>
    public MessageRank MessageRank { get; }


    /// <inheritdoc />
    /// <summary>
    /// CTOR with content
    /// </summary>
    /// <param name="content">The content output from or input to process</param>
    /// <param name="rank"></param>
    public ConsoleStreamEventArgs(string content, MessageRank rank)
    {
        Content = content;
        MessageRank = rank;
    }


}