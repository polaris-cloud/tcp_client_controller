
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ScriptEditorTest.ScriptConsole.Process;
using ScriptEditorTest.ScriptConsole.Stream;

namespace ScriptEditorTest.ScriptConsole.Bridge
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
                _stream.OnConsoleRead -= RaiseConsoleReadEvent;
            _stream =stream;
            _stream.OnConsoleRead += RaiseConsoleReadEvent;

        }

        public void RaiseConsoleReadEvent(object? sender, ConsoleStreamEventArgs args)
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
