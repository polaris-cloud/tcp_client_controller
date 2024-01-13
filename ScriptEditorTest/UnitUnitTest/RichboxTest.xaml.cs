using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ScriptEditorTest.UnitUnitTest.Util;
using static System.Collections.Specialized.BitVector32;
using Section = System.Windows.Documents.Section;

namespace ScriptEditorTest
{
    /// <summary>
    /// RichboxTest.xaml 的交互逻辑
    /// </summary>
    public partial class RichboxTest : Window
    {
        public RichboxTest()
        {
            InitializeComponent();
            
            TestTextRange();
            //Test_RichTextBoxElement();
            //Test_paragraphLineBreak();
            //Test_paragraphInsertElements();
        }


        private void TestTextRange()
        {
            TextRange textRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);

            
            Debug.WriteLine($"pos:{textRange.Start.GetOffsetToPosition(Editor.Document.ContentStart)}");
            Debug.WriteLine($"pos:{textRange.End.GetOffsetToPosition(Editor.Document.ContentStart)}");
            Debug.WriteLine($"Document length: {Editor.Document.ContentEnd.GetOffsetToPosition(Editor.Document.ContentStart)}");


            // 创建两个 Run 元素，分别设置不同的样式
            Run run1 = new Run("This is the first run. ");
            Run run2 = new Run("This is the second run.");

            // 在 TextRange 中插入这两个 Run 元素
            textRange.Start.Paragraph.Inlines.Add(run1);
            textRange.Start.Paragraph.Inlines.Add(run2);
            Debug.WriteLine($"pos:{textRange.End.GetOffsetToPosition(Editor.Document.ContentStart)}");
            Debug.WriteLine($"pos:{Editor.CaretPosition.GetOffsetToPosition(Editor.Document.ContentStart)}");
            Editor.ScrollToEnd();
            Editor.SetCaretToEnd();
            Debug.WriteLine($"pos:{Editor.CaretPosition.GetOffsetToPosition(Editor.Document.ContentStart)}");
            Debug.WriteLine($"pos:{textRange.End.GetOffsetToPosition(Editor.Document.ContentStart)}");
            Editor.Focus();
        }

        
        /// <summary>
        /// 测试richtextbox各元素的用法
        /// </summary>
        private void Test_RichTextBoxElement()
        {
            // 创建一个 RichTextBox 控件
            RichTextBox richTextBox = Editor;

            // 创建一个 FlowDocument
            FlowDocument flowDocument = new FlowDocument();

            // 创建一个 Paragraph
            Paragraph paragraph = new Paragraph();

            // 创建两个 Run 元素，并设置它们的属性
            Run run1 = new Run("This is a ");
            Run run2 = new Run("sample text.");
            run2.Foreground = Brushes.Blue; // 设置文本颜色

            // 将 Run 元素添加到 Paragraph 中
            paragraph.Inlines.Add(run1);
            paragraph.Inlines.Add(run2);

            // 将 Paragraph 添加到 FlowDocument 中
            flowDocument.Blocks.Add(paragraph);

            // 将 FlowDocument 设置为 RichTextBox 的文档
            richTextBox.Document = flowDocument;

            
        }

        private void Test_paragraphLineBreak()
        {
            // 创建一个 RichTextBox 控件
            RichTextBox richTextBox = Editor;

            // 创建一个 FlowDocument
            FlowDocument flowDocument = new FlowDocument();

            // 创建一个 Paragraph，包含多个 Run 元素
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run("This is the first line."));
            paragraph.Inlines.Add(new LineBreak()); // 换行
            paragraph.Inlines.Add(new Run("This is the second line."));

            // 设置第二行的文本样式
            paragraph.Inlines.LastInline.Foreground = Brushes.Blue;

            // 将 Paragraph 添加到 FlowDocument 中
            flowDocument.Blocks.Add(paragraph);

            // 将 FlowDocument 设置为 RichTextBox 的文档
            richTextBox.Document = flowDocument;

            
        }

        private void Test_paragraphInsertElements()
        {
            Paragraph paragraph = new Paragraph();

            // 插入 Run 元素
            paragraph.Inlines.Add(new Run("This is a simple "));
            paragraph.Inlines.Add(new Run("Run") { FontWeight = FontWeights.Bold });

            // 插入 LineBreak 元素
            paragraph.Inlines.Add(new LineBreak());

            // 插入 Hyperlink 元素
            Hyperlink hyperlink = new Hyperlink(new Run("Click me"));
            hyperlink.NavigateUri = new Uri("https://www.example.com");
            paragraph.Inlines.Add(hyperlink);
            paragraph.Inlines.Add(new LineBreak());
            // 插入 BlockUIContainer 元素（示例中假设 button 是一个已经创建的 Button 对象）
            var button = new Button() { Content = "哈哈哈哈哈哈", IsEnabled = true,Margin = new Thickness(5)};
            button.MouseEnter += (s, e) => { MessageBox.Show("Button clicked!"); };
            InlineUIContainer inlineUIContainer = new InlineUIContainer(button);

            // 创建一个 Hyperlink 元素
            Hyperlink hyperlink_1 = new Hyperlink(new Run("Click me again"));

            //hyperlink_1.IsEnabled = true;
            
            
            Debug.WriteLine(paragraph.IsEnabled);
            Debug.WriteLine(hyperlink_1.IsEnabled);

            // 设置 Hyperlink 的点击事件处理程序
            hyperlink_1.Click += (sender, e) =>
            {
                MessageBox.Show("Button clicked!");
            };

            hyperlink_1.MouseDown += (s,e) => { MessageBox.Show("Button clicked!"); };
            paragraph.Inlines.Add(hyperlink_1);
            paragraph.Inlines.Add(new LineBreak());
            paragraph.Inlines.Add(inlineUIContainer);
            Editor.Document.Blocks.Add(paragraph);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hyperlink clicked!");
        }
    }
}
