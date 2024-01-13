using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Polaris.MaterialDesignWindow.WPF.Resources;

namespace Bee.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MaterialDesignWindow.RegisterCommands(this);
            InitializeComponent();
        }


        private void TitleTbx_OnMouseEnter(object sender, MouseEventArgs e)
        {
            AuthorTbx.Visibility = Visibility.Visible;
        }

        private void TitleTbx_OnMouseLeave(object sender, MouseEventArgs e)
        {
            AuthorTbx.Visibility = Visibility.Hidden;
        }
    }
}
