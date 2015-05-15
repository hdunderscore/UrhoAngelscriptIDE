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
    [IntelSourceDescriptor(Ext = ".hlsl")]
    public class HLSLSource : SourceBase
    {
        static Globals HLSLGlobals;
        Globals documentGlobals_;

        public Globals GetHLSLGlobals()
        {
            if (HLSLGlobals == null)
            {
                //parse and load the GLSL globals
                DumpParser parser = new DumpParser();
                Globals globs = new Globals(true);
                parser.ParseDumpFile(FileOperationAPIWrapper.GetResourceStringReader("Debugger.Resources.HLSL.h"), globs);
                HLSLGlobals = globs;
            }
            return HLSLGlobals;
        }

        public override Globals GetGlobals()
        {
            if (documentGlobals_ != null)
            {
                documentGlobals_.Parent = GetHLSLGlobals();
                return documentGlobals_;
            }
            return GetHLSLGlobals();
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
                Parsers.HLSLParser asp = new Parsers.HLSLParser(GetHLSLGlobals());
                IDEProject.inst().LocalTypes = documentGlobals_ = asp.Parse(item.Path, System.IO.File.ReadAllText(item.Path), IDEProject.inst().Settings.GetIncludeDirectories());
            });
        }
    }
}
