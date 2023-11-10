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

namespace canvas_test
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point currentPoint;
        private Point startPoint;
        public MainWindow()
        {
            InitializeComponent();
            Canvas.SetLeft(dragRectangle, 30);
            Canvas.SetTop(dragRectangle, 30);
        }

        private void DragRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            currentPoint = e.GetPosition(canvas);
            startPoint = new Point(Canvas.GetLeft(dragRectangle), Canvas.GetTop(dragRectangle));
            dragRectangle.CaptureMouse();
        }

        private void DragRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newPoint = e.GetPosition(canvas);
                double offsetX = newPoint.X - currentPoint.X;
                double offsetY = newPoint.Y - currentPoint.Y;

                Canvas.SetLeft(dragRectangle, Canvas.GetLeft(dragRectangle) + offsetX);
                Canvas.SetTop(dragRectangle, Canvas.GetTop(dragRectangle) + offsetY);

                currentPoint = newPoint;
            }
        }

        private void DragRectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(canvas);
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            if (mousePosition.X < 0 || mousePosition.X > canvasWidth ||
                mousePosition.Y < 0 || mousePosition.Y > canvasHeight)
            {
                Canvas.SetLeft(dragRectangle, startPoint.X);
                Canvas.SetTop(dragRectangle, startPoint.Y);
            }

            isDragging = false;
            dragRectangle.ReleaseMouseCapture();
        }
    }
}
