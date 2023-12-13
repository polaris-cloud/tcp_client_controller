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
using System.Windows.Shapes;

namespace rectangle_drag_test
{
    /// <summary>
    /// lineMotion.xaml 的交互逻辑
    /// </summary>
    public partial class lineMotion : Window
    {
        private bool isDragging = false;
        public lineMotion()
        {
            InitializeComponent();
        }
        private void Car_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            car.CaptureMouse();
        }

        private void Car_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                double mouseX = e.GetPosition(trackCanvas).X;

                // 限制小车在导轨内移动
                if (mouseX < 0)
                    mouseX = 0;
                else if (mouseX > trackCanvas.ActualWidth - car.Width)
                    mouseX = trackCanvas.ActualWidth - car.Width;

                // 设置小车位置
                Canvas.SetLeft(car, mouseX);
            }
        }

        private void Car_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            car.ReleaseMouseCapture();
        }
    }
}
