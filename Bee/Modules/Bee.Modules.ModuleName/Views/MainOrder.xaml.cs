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
using Bee.Core.ModuleExtension;

namespace Bee.Modules.Script.Views
{
    /// <summary>
    /// MainOrderView.xaml 的交互逻辑
    /// </summary>

    [ModuleSubViewTabItem("指令调试", "SemanticWeb")]
    public partial class MainOrder: UserControl
    {
        public MainOrder()
        {
            InitializeComponent();
        }
    }
}
