using System.Windows;
using System.Windows.Data;
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
    }
}
