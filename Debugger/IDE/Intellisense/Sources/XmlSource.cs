using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Sources
{
    /// <summary>
    /// Intellisense source for XML documents, will need to extract information from schema
    /// </summary>
    [IntelSourceDescriptor(Ext = ".xml")]
    public class XmlSource : IntellisenseSource
    {
        public Globals GetGlobals()
        {
            throw new NotImplementedException();
        }

        public void HookEditor(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            throw new NotImplementedException();
        }

        public void DocumentChanged(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            throw new NotImplementedException();
        }

        public void EditorKeyUp(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner depthScanner, System.Windows.Input.KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void EditorMouseHover(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner depthScanner, System.Windows.Input.MouseEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
