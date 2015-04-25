using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Debugger.IDE.Intellisense
{
    public interface IntellisenseSource
    {
        Globals GetGlobals();
        void HookEditor(TextEditor editor, FileBaseItem item);
        void DocumentChanged(TextEditor editor, FileBaseItem item);
        void EditorKeyUp(TextEditor editor, DepthScanner depthScanner, KeyEventArgs e);
        void EditorMouseHover(TextEditor editor, DepthScanner depthScanner, MouseEventArgs e);
    }
}
