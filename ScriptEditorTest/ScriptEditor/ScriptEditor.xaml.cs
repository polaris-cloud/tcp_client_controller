using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScriptEditorTest.Util;

namespace ScriptEditorTest.ScriptEditor
{
    /// <summary>
    /// ScriptEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptEditor : UserControl
    {

        public static readonly DependencyProperty SuggestionListProperty =
            DependencyProperty.Register("Suggestion List", typeof(IEnumerable<string>), typeof(ScriptEditor));


        public IEnumerable<string> SuggestionList
        {
            get => (IEnumerable<string>)GetValue(SuggestionListProperty);
            set => SetValue(SuggestionListProperty, value);
        }
        
        
        public ScriptEditor()
        {
            InitializeComponent();
            IntellisenseBox.Visibility = Visibility.Collapsed;
            Editor.ContextMenu!.ItemsSource = SuggestionList;

        }

        private void ShowIntellisense(TextPointer caretPosition, List<string> suggestions)
        {



            if (suggestions.Count > 0)
            {

                //IntellisenseBox = new ComboBox
                //{
                //    ItemsSource = suggestions,
                //    IsDropDownOpen = true,
                //    HorizontalAlignment = HorizontalAlignment.Left,
                //    VerticalAlignment = VerticalAlignment.Top
                //};
                IntellisenseBox.ItemsSource = suggestions.ToArray();
                // Find the position to display the ComboBox
                var rect = caretPosition.GetCharacterRect(LogicalDirection.Backward);
                var point = new Point(rect.Left, rect.Bottom);

                // Convert point to screen coordinates
                point = Editor.TransformToAncestor(Application.Current.MainWindow)
                                   .Transform(point);

                // Set ComboBox position
                IntellisenseBox.Margin = new Thickness(point.X, point.Y, 0, 0);
                IntellisenseBox.Visibility = Visibility.Visible;
                IntellisenseBox.SelectedIndex = -1;
                IntellisenseBox.Focus();
                //Add a handler to insert the selected item
                //IntellisenseBox.SelectionChanged += (s, e) =>
                //{
                //    if (IntelliSenseBox.SelectedItem != null)
                //    {
                //        var selectedText = IntelliSenseBox.SelectedItem.ToString();

                //        InsertTextAtCaret(caretPosition,selectedText);
                //        TextPointer tp = caretPosition.GetPositionAtOffset(selectedText.Length+2, LogicalDirection.Forward);
                //        if (tp != null)
                //            scriptEditor.CaretPosition = tp;
                //        HideIntellisense();
                //        scriptEditor.Focus();
                //    }
                //};

                //Add ComboBox to the layout
                //IntellisenseBox.SelectedValuePath = suggestions[0];
                //scriptEditor.AddChild(IntelliSenseBox);

                // 获取 RichTextBox 中当前光标位置的 TextPointer


                //// 获取当前光标位置所在的 Run 元素
                //Run currentRun = caretPosition.Parent as Run;

                //if (currentRun != null)
                //{
                //    // 创建一个 TextRange，用于操作 Run 元素的内容
                //    TextRange range = new TextRange(currentRun.ContentStart, currentRun.ContentEnd);

                //    // 设置文本的颜色
                //    range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);

                //    // 如果需要修改其他属性，可以类似地使用 ApplyPropertyValue 方法
                //}

            }
        }
        
        private void HideIntellisense()
        {
            //scriptEditor.RemoveChild(IntelliSenseBox);
            IntellisenseBox.Visibility = Visibility.Collapsed;
        }

        private void InsertTextAtCaret(TextPointer textPointer, string text)
        {
            textPointer.InsertTextInRun(text);
        }

        private void InsertTextAtCaret(string text)
        {
            Editor.CaretPosition.InsertTextInRun(text + " ");
        }

        private void InsertCommand_Click(object sender, RoutedEventArgs e)
        {
            InsertTextAtCaret(">Command ");
        }
        
        private List<string> GetCommandSuggestions(string prefix)
        {
            return SuggestionList.Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        
        
        
        private void IntellisenseBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IntellisenseBox.SelectedItem != null)
            {
                var selectedText = IntellisenseBox.SelectedItem.ToString();

                //var caretPosition = Editor.CaretPosition;
                var range = new TextRange(Editor.GetEndPointer(), Editor.GetEndPointer())
                {
                    Text = selectedText
                };

                Debug.WriteLine($"pos:{range.Start.GetOffsetToPosition(Editor.Document.ContentStart)}");
                Debug.WriteLine($"pos:{range.End.GetOffsetToPosition(Editor.Document.ContentStart)}");


                range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DeepPink);
                //InsertTextAtCaret(caretPosition, selectedText);
                //Editor.ScrollToEnd();
                Editor.SetCaretToEnd();
                Debug.WriteLine($"pos:{range.End.GetOffsetToPosition(Editor.Document.ContentStart)}");
                Debug.WriteLine($"pos:{Editor.CaretPosition.GetOffsetToPosition(Editor.Document.ContentStart)}");
                var run = new Run("<===>") { Foreground =Brushes.Blue  };
                range.End.Paragraph.Inlines.Add(run);
                Editor.SetCaretToEnd();
                HideIntellisense();
                Editor.Focus();
            }
        }

        private void ScriptEditor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Check if the current line starts with '>'
            var caretPosition = Editor.CaretPosition;
            var currentLine = /*GetLineText(caretPosition)*/e.Text;
            if (currentLine.TrimStart().StartsWith("."))
            {
                // Extract the current command
                var commandPrefix = currentLine.TrimStart('.', ' ');
                var suggestions = GetCommandSuggestions(commandPrefix);
                
                // Show IntelliSense ComboBox
                ShowIntellisense(caretPosition, suggestions);
                e.Handled = true;
            }
            else
            {
                // Hide IntelliSense ComboBox
                HideIntellisense();
            }
        }

        private void SimulateMouseOver(ListBox listBox, int index)
        {
            if (index >= 0 && index < listBox.Items.Count)
            {
                ListBoxItem listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(index);
                if (listBoxItem != null)
                {
                    VisualStateManager.GoToState(listBoxItem, "MouseOver", true);
                }
            }
        }
        private int _lookupPos = -1;
        private void IntellisenseBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            int cur = _lookupPos;
            switch (e.Key)
            {
                case Key.Up:
                {
                    int next = cur <=0 ? cur : cur - 1;
                    if (next > -1)
                    {
                            //var item = (ListBoxItem)IntellisenseBox.ItemContainerGenerator.ContainerFromItem(IntellisenseBox.Items[next]);
                            //    ;
                            //IntellisenseBox.ScrollIntoView(item);
                            //item.Background = Brushes.Red; 
                            SimulateMouseOver(IntellisenseBox, next);
                    }
                    
                    e.Handled = true;
                    break;
                }
                case Key.Down:
                {
                    int next = cur == IntellisenseBox.Items.Count-1 ? cur : cur +1;
                    //var item =(ListBoxItem)IntellisenseBox.ItemContainerGenerator.ContainerFromItem(IntellisenseBox.Items[now]);
                    //    IntellisenseBox.ScrollIntoView(item);
                    //    item.Background = Brushes.Red;
                        SimulateMouseOver(IntellisenseBox, next);
                        e.Handled = true;
                        break;
                }
                case Key.Enter:
                    break; 
                default: e.Handled = true;
                    break;
            }
        }
    }
}
