using System.Windows.Media;
using Polaris.Console.Stream;

namespace Polaris.Console
{
    //Based on code of https://github.com/dwmkerr/consolecontrol


    /// <summary>
    /// take a bridge between Console Control and IConsoleStream
    /// </summary>
    public class ConsoleBridge
    {

        public event EventHandler<ConsoleBridgeArgs> OnConsoleRead;
        
        public bool Write(string content)
        {
            return _stream.Write(content);
        }


        /// <summary>
        /// CTOR
        /// </summary>
        public ConsoleBridge()
        {
            
        }

        
        
        private IConsoleStream? _stream;

        public void SwitchStream(IConsoleStream stream)
        {
            if(_stream!=null)
                _stream.OnConsoleRead -= ConsoleReadHandler;
            _stream =stream;
            _stream.OnConsoleRead += ConsoleReadHandler;

        }

        public void ConsoleReadHandler(object? sender, ConsoleStreamEventArgs args)
        {
            Brush brush=args.MessageRank  switch{
                MessageRank.None=> Brushes.Black,
                MessageRank.Rule => Brushes.Blue,
                MessageRank.Error =>Brushes.Red,
                MessageRank.Tip => Brushes.Blue,
                MessageRank.Diagnostic=>Brushes.Green,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            OnConsoleRead.Invoke(sender, new ConsoleBridgeArgs(args.Content,brush));
        }
        
        
    }
}
