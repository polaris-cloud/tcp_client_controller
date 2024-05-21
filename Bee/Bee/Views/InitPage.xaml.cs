using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace Bee.Views
{
    /// <summary>
    /// InitPage.xaml 的交互逻辑
    /// </summary>
    public partial class InitPage :Window
    {
        public InitPage()
        {
            
            InitializeComponent();
            // 使用 WindowChrome 隐藏窗口装饰
            WindowChrome.SetWindowChrome(this, new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(2),
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(0),
            });
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 模拟窗口拖动
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
