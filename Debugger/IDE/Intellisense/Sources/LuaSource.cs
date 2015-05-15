using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Sources
{
    [IntelSourceDescriptor(Ext = ".lua")]
    public class LuaSource : IntellisenseSource
    {
        public Globals GetGlobals()
        {
            return null;
        }

        public void HookEditor(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item) { }
        public void DocumentChanged(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item) { }
        public void EditorKeyUp(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner depthScanner, System.Windows.Input.KeyEventArgs e) { }
        public void EditorMouseHover(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner depthScanner, System.Windows.Input.MouseEventArgs e) { }
    }
}
