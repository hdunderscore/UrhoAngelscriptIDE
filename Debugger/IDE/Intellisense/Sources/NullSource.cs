using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Sources
{
    [IntelSourceDescriptor(Ext = ".txt")]
    public class NullSource : IntellisenseSource
    {
        public Globals GetGlobals()
        {
            return null;
        }

        public void HookEditor(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            // Do nothing
        }

        public void DocumentChanged(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            // Do nothing
        }

        public void EditorKeyUp(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner depthScanner, System.Windows.Input.KeyEventArgs e)
        {
            // Do nothing
        }

        public void EditorMouseHover(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner depthScanner, System.Windows.Input.MouseEventArgs e)
        {
            // Do nothing
        }
    }
}
