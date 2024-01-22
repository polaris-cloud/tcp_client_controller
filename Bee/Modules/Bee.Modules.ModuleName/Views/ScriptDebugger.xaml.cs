using System.Windows.Controls;
using Bee.Core.ModuleExtension;

namespace Bee.Modules.Script.Views
{
    /// <summary>
    /// Interaction logic for ScriptDebugger
    /// </summary>
 [ModuleSubViewTabItem("Script调试器", "SemanticWeb",NavigateUri = nameof(ScriptDebugger))]
    public partial class ScriptDebugger : UserControl
    {
        public ScriptDebugger()
        {
            InitializeComponent();
        }
    }
}
