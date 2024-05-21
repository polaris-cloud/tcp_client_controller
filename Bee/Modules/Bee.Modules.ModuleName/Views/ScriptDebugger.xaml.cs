using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Bee.Core.Controls;
using Bee.Core.ModuleExtension;
using Bee.Modules.Script.Shared.Intellisense;
using Bee.Modules.Script.ViewModels;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using Polaris.Protocol.Model;
using Polaris.Protocol.Parser;

namespace Bee.Modules.Script.Views
{

    class CompletionData : ICompletionData
    {
        private readonly ProtocolFormat _format;

        public CompletionData(ProtocolFormat format)
        {
            _format = format;
            Text = format.BehaviorKeyword;
        }


        public ImageSource Image => null;

        public string Text { get; }

        public object Content => Text;

        public object Description => null;

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            try
            {
                // 在文本编辑器中插入自动完成的文本
                ProtocolScriptParser protocolScriptParser = ProtocolScriptParser.BuildScriptParser(_format);
                string sendScript = protocolScriptParser.GenerateSendScript();

                int caretOffset = textArea.Caret.Offset;
                int startOffset = TextUtilities.GetNextCaretPosition(textArea.Document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

                textArea.Document.Replace(startOffset, caretOffset - startOffset, sendScript);
            }
            catch (Exception e)
            {
                textArea.Document.Replace(completionSegment,$"Error:{e.Message}");
            }
            
        }
    }


    /// <summary>
    /// Interaction logic for ScriptDebugger
    /// </summary>
    [ModuleSubViewTabItem("Script调试器", "SemanticWeb",NavigateUri = nameof(ScriptDebugger))]
    public partial class ScriptDebugger : UserControl
    {
        // 自动完成数据的定义
        
        private CompletionWindow completionWindow;

        public ScriptDebugger()
        {
            RegisterHighlightRuleForAvalonEdit();
            InitializeComponent();
            SetHighlightRuleForAvalonEdit();

            ((IEasyLoggingBindView)DataContext).OnLogData += Log;
            // 监听文本编辑器的键盘事件
            textEditor.TextArea.TextEntering += TextArea_TextEntering;
            textEditor.TextArea.TextEntered += TextArea_TextEntered;



        }

        private void RegisterHighlightRuleForAvalonEdit()
        {
            IHighlightingDefinition customHighlighting;
            
            using (Stream s = typeof(ScriptDebugger).Assembly.GetManifestResourceStream("Bee.Modules.Script.Resources.BeeHighlight.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting =
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("BeeHighlight", new string[] { ".bee" },
                customHighlighting);
        }
        private void SetHighlightRuleForAvalonEdit()
        {
            textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".bee");
        }

        
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                //if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0]!='.')
                //{
                //    // 如果输入的字符不是字母或数字，关闭自动完成窗口
                    completionWindow.Close();
                //}
            }
        }


        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            string currentWord = GetCurrentWord();
            // 显示自动完成窗口
            var col =FuzzySearchUtil.FilterAndSortByFuzzySharp(currentWord, BindingProtocols.GetProtocolFormats(textEditor));
            ShowCompletionWindow(col);
            
            
            
        }

        private void FilterData(string text)
        {
            
        }
        private string GetCurrentWord()
        {
            // Get the current word based on the caret position
            int caretOffset = textEditor.CaretOffset;
            int startOffset = TextUtilities.GetNextCaretPosition(textEditor.Document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);
            
            // word endoff
            int endOffset = TextUtilities.GetNextCaretPosition(textEditor.Document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);

            //判断是否超出了当前的正在输入的单词边界 
            if (caretOffset > endOffset && endOffset > startOffset)
            {
                return string.Empty; 
            }


            if (startOffset >= 0 && caretOffset > startOffset)
            {
                return textEditor.Document.GetText(startOffset, caretOffset - startOffset);
            }

            return string.Empty;
        }
        
        
        private void ShowCompletionWindow(IEnumerable<ProtocolFormat> filterCache)
        {

            completionWindow = new CompletionWindow(textEditor.TextArea);
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            if (filterCache == null)
                return;
            
            foreach (var item in filterCache)
            {
                data.Add(new CompletionData(item));
            }

            if (data.Count == 0)
                return;
            // 显示自动完成窗口
            completionWindow.Show();
            
        }


        public void Log(string content, LogLevel logLevel)
        {
            Dispatcher.RunOnUiDispatcher(
                () =>
                {
                    switch (logLevel)
                    {
                        case LogLevel.Info:
                            outputRichTextBox.WriteOutputData(Dispatcher, content, Brushes.Blue);
                            break;
                        case LogLevel.Debug:
                            DebugRichTextBox.WriteOutputData(Dispatcher, content, Brushes.Black);
                            break;
                        case LogLevel.Warn:
                            break;
                        case LogLevel.Error:
                            DebugRichTextBox.WriteOutputData(Dispatcher, content, Brushes.Red);
                            break;
                        case LogLevel.Fatal:
                            break;
                        case LogLevel.Other:
                            LoggerRichTextBox.WriteOutputData(Dispatcher, content, Brushes.Blue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                    }
                }
            );
            
            
        }

        private void OutputEmptyButton_OnClick(object sender, RoutedEventArgs e)
        {
            outputRichTextBox.Document.Blocks.Clear();
        }

        private void DebugEmptyButton_OnClickEmptyButton_OnClick(object sender, RoutedEventArgs e)
        {
            DebugRichTextBox.Document.Blocks.Clear();
        }
        private void LoggerEmptyButton_OnClickEmptyButton_OnClickEmptyButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoggerRichTextBox.Document.Blocks.Clear();
        }

        private void TextEditor_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double fontSize = textEditor.FontSize + e.Delta / 40.0;

                if (fontSize < 6)
                    textEditor.FontSize = 6;
                else
                {
                    if (fontSize > 200)
                        textEditor.FontSize = 200;
                    else
                        textEditor.FontSize = fontSize;
                }

                e.Handled = true;
            }
        }

        
    }
}
