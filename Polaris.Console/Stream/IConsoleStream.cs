namespace Polaris.Console.Stream
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
