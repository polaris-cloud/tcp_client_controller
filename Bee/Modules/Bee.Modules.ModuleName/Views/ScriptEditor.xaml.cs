using Bee.Core.ModuleExtension;
using System.Windows.Controls;

namespace Bee.Modules.Script.Views
{
    /// <summary>
    /// Interaction logic for ScriptEditor
    /// </summary>
    ///
    ///
    [ModuleSubViewTabItem("Script编辑器", "ScriptTextPlayOutline",NavigateUri = nameof(ScriptEditor))]
    public partial class ScriptEditor : UserControl
    {
        public ScriptEditor()
        {
            InitializeComponent();
        }
    }
}
