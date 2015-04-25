using FirstFloor.ModernUI.Presentation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Debugger.IDE.Intellisense.Sources
{
    public class HLSLSource : SourceBase
    {
        static Globals HLSLGlobals;
        Globals documentGlobals_;
        public override Globals GetGlobals()
        {
            if (HLSLGlobals == null)
            {
                //parse and load the HLSL globals
                ASParser parser = new ASParser();
                Globals globs = new Globals(true);
                parser.ParseDumpFile(FileOperationAPIWrapper.GetResourceStringReader("Debugger.Resources.HLSL.h"), globs);
                HLSLGlobals = globs;
            }
            if (documentGlobals_ != null)
            {
                documentGlobals_.Parent = HLSLGlobals;
                return documentGlobals_;
            }
            return HLSLGlobals;
        }

        public override void HookEditor(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            editor.SyntaxHighlighting = AvalonExtensions.LoadHighlightingDefinition("Debugger.Resources.HLSL.xshd");

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

        public override void DocumentChanged(TextEditor editor, FileBaseItem item)
        {
            MainWindow.inst().Dispatcher.Invoke(delegate()
            {
                IDEProject.inst().LocalTypes = documentGlobals_ = AngelscriptParser.Parse(item.Path, System.IO.File.ReadAllText(item.Path), IDEProject.inst().Settings.GetIncludeDirectories());
            });
        }
    }
}
