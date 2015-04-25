using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Debugger.IDE.Intellisense.Sources
{
    public class GLSLSource : SourceBase
    {
        static Globals GLSLGlobals;
        Globals documentGlobals_;
        public override Globals GetGlobals()
        {
            if (GLSLGlobals == null)
            {
                //parse and load the GLSL globals
                ASParser parser = new ASParser();
                Globals globs = new Globals(true);
                parser.ParseDumpFile(FileOperationAPIWrapper.GetResourceStringReader("Debugger.Resources.GLSL.h"), globs);
                GLSLGlobals = globs;
            }
            if (documentGlobals_ != null)
            {
                documentGlobals_.Parent = GLSLGlobals;
                return documentGlobals_;
            }
            return GLSLGlobals;
        }

        public override void HookEditor(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            editor.SyntaxHighlighting = AvalonExtensions.LoadHighlightingDefinition("Debugger.Resources.GLSL.xshd");

            editor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Insert #include",
                Command = new RelayCommand(p =>
                {
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.DefaultExt = "*";
                    dlg.Filter = "All files (*.*)|*.*";
                    if (dlg.ShowDialog() == true)
                    {
                        editor.Document.BeginUpdate();
                        editor.Document.Insert(editor.CaretOffset, string.Format("#include \"{0}\"", dlg.FileName));
                        editor.Document.EndUpdate();
                    }
                })
            });
        }

        public override void DocumentChanged(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            MainWindow.inst().Dispatcher.Invoke(delegate()
            {
                IDEProject.inst().LocalTypes = documentGlobals_ = AngelscriptParser.Parse(item.Path, System.IO.File.ReadAllText(item.Path), IDEProject.inst().Settings.GetIncludeDirectories());
            });
        }
    }
}
