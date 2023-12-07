using ScriptEditorTest.ScriptConsole.Bridge;
using ScriptEditorTest.Util;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace ScriptEditorTest.ScriptConsole
{

    //Based on code of https://github.com/adamecr/RadProjectsExtension
    /// <summary>
    /// Console.xaml 的交互逻辑 ,不与Process进行交互，与
    /// </summary>

    public partial class Console : UserControl
    {

        #region XAML properties
        private static readonly DependencyProperty DiagnosticsTextColorProperty =
            DependencyProperty.Register("DiagnosticsTextColor", typeof(Brush), typeof(Console),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))));
        /// <summary>
        /// Brush for the diagnostics text
        /// </summary>
        [Category("Brush")]
        [Description("Brush for the diagnostics text")]
        public Brush DiagnosticsTextBrush
        {
            get => (Brush)GetValue(DiagnosticsTextColorProperty);
            set => SetValue(DiagnosticsTextColorProperty, value);
        }

        private static readonly DependencyProperty OutputTextColorProperty =
            DependencyProperty.Register("OutputTextColor", typeof(Brush), typeof(Console),
                new PropertyMetadata(Brushes.LightGray));
        /// <summary>
        /// Brush for the standard output text
        /// </summary>
        [Category("Brush")]
        [Description("Brush for the standard output text")]
        public Brush OutputTextBrush
        {
            get => (Brush)GetValue(OutputTextColorProperty);
            set => SetValue(OutputTextColorProperty, value);
        }

        private static readonly DependencyProperty ErrorTextColorProperty =
            DependencyProperty.Register("ErrorTextColor", typeof(Brush), typeof(Console),
                new PropertyMetadata(Brushes.Red));
        /// <summary>
        /// Brush for the error output text
        /// </summary>
        [Category("Brush")]
        [Description("Brush for the error output text")]
        public Brush ErrorTextBrush
        {
            get => (Brush)GetValue(ErrorTextColorProperty);
            set => SetValue(ErrorTextColorProperty, value);
        }

        private static readonly DependencyProperty InputTextColorProperty =
            DependencyProperty.Register("InputTextColor", typeof(Brush), typeof(Console),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 224, 234, 9))));
        /// <summary>
        /// Brush for the input text
        /// </summary>
        [Category("Brush")]
        [Description("Brush for the input text")]
        public Brush InputTextBrush
        {
            get => (Brush)GetValue(InputTextColorProperty);
            set => SetValue(InputTextColorProperty, value);
        }

        private static readonly DependencyProperty ShowDiagnosticsProperty =
            DependencyProperty.Register("ShowDiagnostics", typeof(bool), typeof(Console),
                new PropertyMetadata(false));

        /// <summary>
        /// Flag whether to show diagnostics.
        /// </summary>
        [Description("Flag whether to show diagnostics.")]
        public bool ShowDiagnostics
        {
            get => (bool)GetValue(ShowDiagnosticsProperty);
            set => SetValue(ShowDiagnosticsProperty, value);
        }

        private static readonly DependencyProperty IsInputEnabledProperty =
            DependencyProperty.Register("IsInputEnabled", typeof(bool), typeof(Console),
                new PropertyMetadata(true));







        /// <summary>
        /// Flag whether the user input is enabled.
        /// </summary>
        [Category("Common")]
        [Description("Flag whether the user input is enabled.")]
        public bool IsInputEnabled
        {
            get => (bool)GetValue(IsInputEnabledProperty);
            set => SetValue(IsInputEnabledProperty, value);
        }

        internal static readonly DependencyPropertyKey IsProcessRunningPropertyKey =
            DependencyProperty.RegisterReadOnly("IsProcessRunning", typeof(bool), typeof(Console),
                new PropertyMetadata(false));

        private static readonly DependencyProperty IsProcessRunningProperty = IsProcessRunningPropertyKey.DependencyProperty;
        /// <summary>
        /// Flag whether there is a process running
        /// </summary>
        [Obsolete]
        [Description("Flag whether there is a process running")]
        public bool IsProcessRunning
        {
            get => (bool)GetValue(IsProcessRunningProperty);
            private set => SetValue(IsProcessRunningPropertyKey, value);
        }


        internal static readonly DependencyPropertyKey ConsoleBridgePropertyKey =
            DependencyProperty.RegisterReadOnly("ConsoleBridge", typeof(ConsoleBridge), typeof(Console),
                new PropertyMetadata(new ConsoleBridge()));


private static readonly DependencyProperty ConsoleBridgeProperty = ConsoleBridgePropertyKey.DependencyProperty;


/// <summary>
///  console bridge
/// </summary>
[Description("连接stream和console")]
        public ConsoleBridge ConsoleBridge
        {
            get => (ConsoleBridge)GetValue(ConsoleBridgeProperty);
            private set => SetValue(ConsoleBridgePropertyKey, value);
        }

        


        #endregion

        /// <summary>
        /// Current position that input starts at
        /// </summary>
        private int _inputStartPos;

        /// <summary>
        /// The last input string (used so that we can make sure we don't echo input twice).
        /// </summary>
        private string _lastInput;

        private readonly List<string> _commandBuffer = new List<string>();
        private int _commandBufferIndex = -1;



        ///// <summary>
        ///// The internal process wrapper used to interact with the process.
        ///// </summary>
        //public IConsoleStream Stream { get; set; }


        public Console()
        {
            InitializeComponent();
            ConsoleBridge.OnConsoleRead += OnConsoleOutputHandler;
            
            
            //Setup the console rich text box
            ContentRichTextBox.PreviewKeyDown += ConsoleKeyDownHandler;
            ContentRichTextBox.Foreground = InputTextBrush;
            DataObject.AddCopyingHandler(ContentRichTextBox, (s, e) =>
            {
                if (e.IsDragDrop) e.CancelCommand();
            });
        }
        ///// <summary>
        ///// Occurs when console output is produced.
        ///// </summary>
        //public event EventHandler<string> OnProcessOutput;

        /// <summary>
        /// Handles the OnProcessOutput event of the process wrapper
        /// Provides the standard output or error output from the process
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event data.</param>
        private void OnConsoleOutputHandler(object? sender, ConsoleBridgeArgs args)
        {
            
            WriteOutput(args.Content,args.Brush);

            //Raise the output event.
            //RaiseProcessOutputEvent(args);
        }



        /// <summary>
        /// Handles the KeyDown event of the richTextBoxConsole control.
        /// 处理按键的输入事件
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void ConsoleKeyDownHandler(object sender, KeyEventArgs e)
        {
            var caretPosition = ContentRichTextBox.GetCaretPosition();
            var delta = caretPosition - _inputStartPos;
            var inReadOnlyZone = delta < 0;

            //Command buffer - Ctrl-up / Ctrl-down
            if ((e.Key == Key.Up || e.Key == Key.Down)
                //&& Keyboard.Modifiers.HasFlag(ModifierKeys.Control) 
                &&
                _commandBuffer.Count > 0 &&
                IsInputEnabled)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (e.Key)
                {
                    case Key.Up:
                        {
                            _commandBufferIndex--;
                            if (_commandBufferIndex < 0) _commandBufferIndex = 0;
                            break;
                        }
                    case Key.Down:
                        {
                            _commandBufferIndex++;
                            if (_commandBufferIndex > _commandBuffer.Count - 1)
                                _commandBufferIndex = _commandBuffer.Count - 1;
                            break;
                        }
                }

                var command = _commandBuffer[_commandBufferIndex];
                var _ = new TextRange(ContentRichTextBox.GetPointerAt(_inputStartPos), ContentRichTextBox.GetEndPointer()) { Text = command };
                ContentRichTextBox.SetCaretToEnd();
                e.Handled = true;
                return;

            }

            //ESC to clear
            if (e.Key == Key.Escape && IsInputEnabled)
            {
                var _ = new TextRange(ContentRichTextBox.GetPointerAt(_inputStartPos), ContentRichTextBox.GetEndPointer()) { Text = "" };
                e.Handled = true;
                return;
            }

            //Always allow arrows and Ctrl-C.
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) return;

            
            
            //Input allowed?
            //Backspace needs to prevent +1 character after the read only zone
            if (inReadOnlyZone || !IsInputEnabled || e.Key == Key.Back && delta <= 0)
            {
                e.Handled = true;
                return;
            }


            
            //If not Return key, just let WPF process it
            if (e.Key != Key.Return) return;

            //Process return key
            var input = new TextRange(ContentRichTextBox.GetPointerAt(_inputStartPos), ContentRichTextBox.GetEndPointer()).Text;
            
            //Write the input (without echoing).
            
            Debug.WriteLine($"console input:{input}");
            if (!WriteInput(input, InputTextBrush, false))
            {
                e.Handled = true;
                return;
            }

            ContentRichTextBox.SetCaretToEnd();
        }





        
        
        
        #region Core方法

        private string _preInput; 

        /// <summary>
        /// Writes the input to the console control.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="brush">The color.</param>
        /// <param name="echo">if set to <c>true</c> echo the input.</param>
        public bool WriteInput(string input, Brush brush, bool echo)
        {
            input = input.TrimEnd('\r', '\n');
            bool isSuccess = false;
            RunOnUiDispatcher(() =>
            {
                if (echo)
                {
                    ContentRichTextBox.Selection.ApplyPropertyValue(TextBlock.ForegroundProperty, brush);
                    ContentRichTextBox.AppendText(input + Environment.NewLine); 
                    _inputStartPos = ContentRichTextBox.GetEndPosition();
                }

                _lastInput = input;

                //Write the input.

                if (ConsoleBridge.Write(input))
                {
                    //update command buffer
                    if (!string.IsNullOrEmpty(input))
                    {
                        if (_preInput != input)
                        {
                            _commandBuffer.Add(input);
                            _commandBufferIndex = _commandBuffer.Count;
                            _preInput = input;
                        }

                        
                    }

                    // Raise the input event.
                    //RaiseProcessInputEvent(new ProcessEventArgs(input));
                    isSuccess = true;
                }
                else isSuccess = false;


            });
            return isSuccess;
        }
        
        /// <summary>
        /// Writes the output to the console control.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="brush">The color.</param>
        public void WriteOutput(string output, Brush brush)
        {
            if (string.IsNullOrEmpty(_lastInput) == false &&
                (output == _lastInput || output.Replace("\r\n", "") == _lastInput))
                return;

            RunOnUiDispatcher(() =>
            {
                //Write the output.
                var range = new TextRange(ContentRichTextBox.GetEndPointer(), ContentRichTextBox.GetEndPointer())
                {
                    Text = output
                };
                range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);

                //Get to the end
                ContentRichTextBox.ScrollToEnd();
                ContentRichTextBox.SetCaretToEnd();
                //Set the start of the input zone
                _inputStartPos = ContentRichTextBox.GetCaretPosition();

                //Switch back to input color , 转换回输入
                var _ = new Run("", ContentRichTextBox.CaretPosition) { Foreground = InputTextBrush };
                
                ContentRichTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, InputTextBrush);
                ContentRichTextBox.Foreground = InputTextBrush;
                ContentRichTextBox.Focus();
            });
        }

        /// <summary>
        /// Clears the output.
        /// </summary>
        public void Clear()
        {
            ContentRichTextBox.Document.Blocks.Clear();
            _commandBuffer.Clear();
            _inputStartPos = 0;  //init to  <0>  pos
        }

        /// <summary>
        /// Runs the action on UI dispatcher.
        /// </summary>
        /// <param name="action">The action to run</param>
        private void RunOnUiDispatcher(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.BeginInvoke(action, null);
            }
        }
        
#endregion

    }
}
