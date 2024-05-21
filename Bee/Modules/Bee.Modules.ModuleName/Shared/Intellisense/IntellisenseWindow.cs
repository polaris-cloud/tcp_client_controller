using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Bee.Modules.Script.Shared.Intellisense
{
    public class IntellisenseWindow:CompletionWindowBase
    {
        private readonly CompletionList completionList = new CompletionList();
        private ToolTip toolTip = new ToolTip();

        /// <summary>
        /// Gets the completion list used in this completion window.
        /// </summary>
        public CompletionList CompletionList => this.completionList;

        /// <summary>Creates a new code completion window.</summary>
        public IntellisenseWindow(TextArea textArea)
          : base(textArea)
        {
            this.FontSize = 18;
            this.CloseAutomatically = true;
            this.SizeToContent = SizeToContent.Height;
            this.MaxHeight = 300.0;
            this.Width = 175.0;
            this.Content = (object)this.completionList;
            this.MinHeight = 15.0;
            this.MinWidth = 30.0;
            this.toolTip.PlacementTarget = (UIElement)this;
            this.toolTip.Placement = PlacementMode.Right;
            this.toolTip.Closed += new RoutedEventHandler(this.toolTip_Closed);
            this.AttachEvents();
        }

        private void toolTip_Closed(object sender, RoutedEventArgs e)
        {
            if (this.toolTip == null)
                return;
            this.toolTip.Content = (object)null;
        }

        private void completionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ICompletionData selectedItem = this.completionList.SelectedItem;
            if (selectedItem == null)
                return;
            object description = selectedItem.Description;
            if (description != null)
            {
                if (description is string str)
                    this.toolTip.Content = (object)new TextBlock()
                    {
                        Text = str,
                        TextWrapping = TextWrapping.Wrap
                    };
                else
                    this.toolTip.Content = description;
                this.toolTip.IsOpen = true;
            }
            else
                this.toolTip.IsOpen = false;
        }

        private void completionList_InsertionRequested(object sender, EventArgs e)
        {
            this.Close();
            this.completionList.SelectedItem?.Complete(this.TextArea, (ISegment)new AnchorSegment(this.TextArea.Document, this.StartOffset, this.EndOffset - this.StartOffset), e);
        }

        private void AttachEvents()
        {
            this.completionList.InsertionRequested += new EventHandler(this.completionList_InsertionRequested);
            this.completionList.SelectionChanged += new SelectionChangedEventHandler(this.completionList_SelectionChanged);
            this.TextArea.Caret.PositionChanged += new EventHandler(this.CaretPositionChanged);
            this.TextArea.MouseWheel += new MouseWheelEventHandler(this.textArea_MouseWheel);
            this.TextArea.PreviewTextInput += new TextCompositionEventHandler(this.textArea_PreviewTextInput);
        }

        /// <inheritdoc />
        protected override void DetachEvents()
        {
            this.completionList.InsertionRequested -= new EventHandler(this.completionList_InsertionRequested);
            this.completionList.SelectionChanged -= new SelectionChangedEventHandler(this.completionList_SelectionChanged);
            this.TextArea.Caret.PositionChanged -= new EventHandler(this.CaretPositionChanged);
            this.TextArea.MouseWheel -= new MouseWheelEventHandler(this.textArea_MouseWheel);
            this.TextArea.PreviewTextInput -= new TextCompositionEventHandler(this.textArea_PreviewTextInput);
            base.DetachEvents();
        }

        /// <inheritdoc />
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (this.toolTip == null)
                return;
            this.toolTip.IsOpen = false;
            this.toolTip = (ToolTip)null;
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled)
                return;
            this.completionList.HandleKey(e);
        }

        private void textArea_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = CompletionWindowBase.RaiseEventPair((UIElement)this, UIElement.PreviewTextInputEvent, UIElement.TextInputEvent, (RoutedEventArgs)new TextCompositionEventArgs(e.Device, e.TextComposition));

        private void textArea_MouseWheel(object sender, MouseWheelEventArgs e) => e.Handled = CompletionWindowBase.RaiseEventPair(this.GetScrollEventTarget(), UIElement.PreviewMouseWheelEvent, UIElement.MouseWheelEvent, (RoutedEventArgs)new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta));

        private UIElement GetScrollEventTarget() => this.completionList == null ? (UIElement)this : (UIElement)this.completionList.ScrollViewer ?? (UIElement)this.completionList.ListBox ?? (UIElement)this.completionList;

        /// <summary>
        /// Gets/Sets whether the completion window should close automatically.
        /// The default value is true.
        /// </summary>
        public bool CloseAutomatically { get; set; }

        /// <inheritdoc />
        protected override bool CloseOnFocusLost => this.CloseAutomatically;

        /// <summary>
        /// When this flag is set, code completion closes if the caret moves to the
        /// beginning of the allowed range. This is useful in Ctrl+Space and "complete when typing",
        /// but not in dot-completion.
        /// Has no effect if CloseAutomatically is false.
        /// </summary>
        public bool CloseWhenCaretAtBeginning { get; set; }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            int offset = this.TextArea.Caret.Offset;
            if (offset == this.StartOffset)
            {
                if (this.CloseAutomatically && this.CloseWhenCaretAtBeginning)
                    this.Close();
                else
                    this.completionList.SelectItem(string.Empty);
            }
            else if (offset < this.StartOffset || offset > this.EndOffset)
            {
                if (!this.CloseAutomatically)
                    return;
                this.Close();
            }
            else
            {
                TextDocument document = this.TextArea.Document;
                if (document == null)
                    return;
                this.completionList.SelectItem(document.GetText(this.StartOffset, offset - this.StartOffset));
            }
        }
    }
}
