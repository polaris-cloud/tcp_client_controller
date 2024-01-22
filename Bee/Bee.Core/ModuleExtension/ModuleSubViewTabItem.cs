using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Prism.Mvvm;

namespace Bee.Core.ModuleExtension
{
    /// <summary>
    /// 支持模块子视图的导航显示或者直接通过wpf绑定content显示
    /// </summary>
    public class ModuleSubViewTabItem
    {
        public string Icon { get; set; }
        public string TabName { get; set; }
        public object View { get; set; }
        public string NavigateUri { get; set;  }
    }
}
