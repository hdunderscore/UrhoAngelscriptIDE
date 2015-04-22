﻿using FirstFloor.ModernUI.Presentation;
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
        public override Globals GetGlobals()
        {
            if (GLSLGlobals == null)
            {
                //parse and load the GLSL globals
                ASParser parser = new ASParser();
                Globals globs = new Globals();
                parser.ParseDumpFile(FileOperationAPIWrapper.GetResourceStringReader("Debugger.Resources.GLSL.h"), globs);
                GLSLGlobals = globs;
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
    }
}
