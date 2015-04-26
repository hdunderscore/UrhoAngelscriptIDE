using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace Debugger.IDE.Intellisense.Sources
{
    public abstract class SourceBase : IntellisenseSource
    {
        public abstract Globals GetGlobals();

        public abstract void HookEditor(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item);

        public abstract void DocumentChanged(TextEditor editor, FileBaseItem item);

        public void EditorKeyUp(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner scanner, System.Windows.Input.KeyEventArgs e)
        {
            Globals g = GetGlobals();
            if (g == null)
                return;
            // These keys halt and terminate intellisense
            switch (e.Key)
            {
                case Key.Home:
                case Key.End:
                case Key.Left:
                case Key.Right:
                case Key.Escape:
                case Key.LWin:
                case Key.RWin:
                case Key.Space:
                    if (currentComp != null)
                        currentComp.Close();
                    return;
            }
            // These keys halt further checks
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageDown:
                case Key.PageUp:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.Scroll:
                case Key.Capital:
                case Key.CrSel:
                case Key.Clear:
                case Key.Insert:
                case Key.PrintScreen:
                case Key.Print:
                    return;
            }

            char KEY = KeyHelpers.GetCharFromKey(e.Key);
            if (KEY == ')' || KEY == ';')
            {
                if (currentComp != null)
                    currentComp.Close();
                return;
            }
            int curOfs = editor.TextArea.Caret.Offset;
            int line = editor.TextArea.Caret.Line;

            // Do not attempt intellisense inside of comments
            string txt = editor.Document.GetText(editor.Document.Lines[editor.TextArea.Caret.Line - 1]);
            if (txt.Trim().StartsWith("//"))
                return;

            if (e.Key == Key.OemPeriod || KEY == ':')
            {
                //IntellisenseHelper.ResemblesDotPath(editor.Document, curOfs-1, line-1)) {
                int ofs = TextUtilities.GetNextCaretPosition(editor.Document, curOfs, LogicalDirection.Backward, CaretPositioningMode.WordStart);
                ofs = TextUtilities.GetNextCaretPosition(editor.Document, ofs, LogicalDirection.Backward, CaretPositioningMode.WordStart);
                string word = "";
                for (; ofs < curOfs; ++ofs)
                {
                    if (editor.Document.Text[ofs] != '.')
                        word += editor.Document.Text[ofs];
                }

                NameResolver reso = new NameResolver(GetGlobals(), scanner);
                BaseTypeInfo info = null;
                string[] words = IntellisenseHelper.DotPath(editor.Document, curOfs - 1, line - 1);
                if (words.Length > 1)
                {
                    for (int i = 0; i < words.Length; ++i)
                    {
                        if (info == null)
                        {
                            info = reso.GetClassType(editor.Document, editor.TextArea.Caret.Line, words[i]);
                            if (info == null && GetGlobals().ContainsProperty(words[i]))
                                info = GetGlobals().GetProperty(words[i], true);
                        }
                        else if (info != null)
                        {
                            info = info.ResolvePropertyPath(GetGlobals(), words[i]);
                        }
                    }
                }

                bool functionsOnly = false;
                //attempt to resolve it locally
                if (info == null)
                    info = reso.GetClassType(editor.Document, editor.TextArea.Caret.Line, word);
                //attempt to resolve it from globals
                if (info == null && GetGlobals() != null && GetGlobals().ContainsProperty(word))
                    info = GetGlobals().GetProperty(word, true);
                if (info == null && word.Contains("::"))
                {
                    if (GetGlobals() == null)
                        return;
                    if (word.Length > 2)
                    {
                        string ns = word.Replace("::", "");
                        if (GetGlobals().ContainsTypeInfo(ns))
                        {
                            EnumInfo ti = GetGlobals().GetTypeInfo(word.Replace("::", "")) as EnumInfo;
                            if (ti != null)
                            {
                                currentComp = new CompletionWindow(editor.TextArea);
                                IList<ICompletionData> data = currentComp.CompletionList.CompletionData;
                                foreach (string str in ti.Values)
                                    data.Add(new BaseCompletionData(null, str));
                                currentComp.Show();
                                currentComp.Closed += comp_Closed;
                                return;
                            }
                            else
                            {
                                TypeInfo ty = GetGlobals().GetTypeInfo(word.Replace("::", ""));
                                if (ty != null)
                                {
                                    info = ty;
                                    functionsOnly = true;
                                }
                            }
                        }
                        else
                        { //list global functions
                            Globals globs = GetGlobals();
                            bool asNS = false;
                            if (globs.ContainsNamespace(ns))
                            {
                                asNS = true;
                                globs = globs.GetNamespace(ns);
                            }
                            currentComp = new CompletionWindow(editor.TextArea);
                            IList<ICompletionData> data = currentComp.CompletionList.CompletionData;
                            foreach (string str in globs.GetPropertyNames())
                            {
                                TypeInfo ti = globs.GetProperty(str, !asNS) as TypeInfo;
                                if (ti != null)
                                    data.Add(new PropertyCompletionData(ti, str));
                            }
                            foreach (FunctionInfo fi in globs.GetFunctions(null, false))
                                data.Add(new FunctionCompletionData(fi));
                            if (asNS) //Include classes
                            {
                                foreach (TypeInfo ti in globs.TypeInfo)
                                    data.Add(new ClassCompletionData(ti));
                            }
                            currentComp.Show();
                            currentComp.Closed += comp_Closed;
                            return;
                        }
                    }
                }

                //build the list
                if (info != null && info is TypeInfo)
                {
                    TypeInfo ti = info as TypeInfo;
                    currentComp = new CompletionWindow(editor.TextArea);
                    IList<ICompletionData> data = currentComp.CompletionList.CompletionData;
                    if (!functionsOnly)
                    {
                        foreach (string str in ti.Properties.Keys)
                        {
                            data.Add(new PropertyCompletionData(ti.Properties[str], str,
                                ti.ReadonlyProperties.Contains(str) ? PropertyAccess.Readonly :
                                (ti.ProtectedProperties.Contains(str) ? PropertyAccess.Protected : PropertyAccess.Public)));
                        }
                    }
                    foreach (FunctionInfo fi in ti.Functions)
                        data.Add(new FunctionCompletionData(fi));
                    currentComp.Show();
                    currentComp.Closed += comp_Closed;
                }
            }
            else if (KEY == '(' && IntellisenseHelper.ResemblesDotPath(editor.Document, curOfs - 2, line - 1))
            {

                NameResolver reso = new NameResolver(GetGlobals(), scanner);
                BaseTypeInfo info = null;
                FunctionInfo func = null;
                string[] words = IntellisenseHelper.DotPath(editor.Document, curOfs - 2, line - 1);
                if (words.Length > 1)
                {
                    for (int i = 0; i < words.Length; ++i)
                    {
                        if (i == words.Length - 1 && info != null)
                        {
                            if (info is TypeInfo)
                                func = ((TypeInfo)info).Functions.FirstOrDefault(f => f.Name.Equals(words[i]));
                        }
                        else
                        {
                            if (info == null)
                            {
                                info = reso.GetClassType(editor.Document, editor.TextArea.Caret.Line, words[i]);
                            }
                            else if (info != null && info is TypeInfo)
                            {
                                if (((TypeInfo)info).Properties.ContainsKey(words[i]))
                                    info = ((TypeInfo)info).Properties[words[i]];
                            }
                        }
                    }
                }
                if (func != null && info != null)
                {
                    List<FunctionInfo> data = new List<FunctionInfo>();
                    if (info is TypeInfo)
                    {
                        TypeInfo ti = (TypeInfo)info;
                        foreach (FunctionInfo fi in ti.Functions.Where(f => { return f.Name.Equals(func.Name); }))
                            data.Add(fi);
                        if (data.Count > 0)
                        {
                            OverloadInsightWindow window = new OverloadInsightWindow(editor.TextArea);
                            window.Provider = new OverloadProvider(ti, data.ToArray());
                            window.Show();
                            //compWindow.Closed += comp_Closed;
                        }
                    }
                }
                else if (func == null && info == null) // Found nothing
                {
                    List<FunctionInfo> data = new List<FunctionInfo>();
                    foreach (FunctionInfo fi in GetGlobals().GetFunctions(words[1], true))
                        data.Add(fi);
                    if (data.Count > 0)
                    {
                        OverloadInsightWindow window = new OverloadInsightWindow(editor.TextArea);
                        window.Provider = new OverloadProvider(null, data.ToArray());
                        window.Show();
                        //compWindow.Closed += comp_Closed;
                    }
                }
            }
            else if (Char.IsLetter(KEY))
            {
                if (currentComp != null || editor.TextArea.Caret.Line == 1)
                    return;

                int ofs = TextUtilities.GetNextCaretPosition(editor.Document, curOfs, LogicalDirection.Backward, CaretPositioningMode.WordStart);
                int nextOfs = TextUtilities.GetNextCaretPosition(editor.Document, ofs, LogicalDirection.Backward, CaretPositioningMode.WordStart);
                if (nextOfs > 0)
                {
                    if (editor.Document.Text[nextOfs] == '.')
                        return;
                }
                string word = "";
                if (ofs < 0)
                    return;
                for (; ofs < curOfs; ++ofs)
                {
                    if (editor.Document.Text[ofs] != '.')
                        word += editor.Document.Text[ofs];
                }
                if (word.Contains("."))
                {
                    if (currentComp != null)
                        currentComp.Close();
                    //editor_KeyUp(sender, e);
                    return;
                }

                NameResolver reso = new NameResolver(GetGlobals(), scanner);
                List<string> suggestions = new List<string>();
                reso.GetNameMatch(editor.Document, editor.TextArea.Caret.Line - 1, word, ref suggestions);

                CompletionWindow compWindow = new CompletionWindow(editor.TextArea);
                compWindow.StartOffset = TextUtilities.GetNextCaretPosition(editor.Document, curOfs, LogicalDirection.Backward, CaretPositioningMode.WordStart);
                IList<ICompletionData> data = compWindow.CompletionList.CompletionData;
                //attempt local name resolution first
                if (suggestions.Count > 0)
                {
                    foreach (string str in suggestions) //text suggestions are of lower priority
                        data.Add(new BaseCompletionData(null, str) { Priority = 0.5 });
                }
                //Scan globals
                if (GetGlobals() != null)
                {
                    foreach (string str in GetGlobals().GetTypeInfoNames())
                    {
                        if (str.StartsWith(word))
                            data.Add(new ClassCompletionData(GetGlobals().GetTypeInfo(str)));
                    }
                    foreach (FunctionInfo fun in GetGlobals().GetFunctions(null, true))
                    {
                        if (fun.Name.StartsWith(word))
                            data.Add(new FunctionCompletionData(fun));
                    }
                }
                if (data.Count > 0)
                {
                    currentComp = compWindow;
                    currentComp.Show();
                    currentComp.Closed += comp_Closed;
                }
            }
        }

        public void EditorMouseHover(ICSharpCode.AvalonEdit.TextEditor editor, DepthScanner scanner, MouseEventArgs e)
        {
            TextViewPosition? pos = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (pos != null)
            {
                try
                {
                    int line = pos.Value.Line;
                    int offset = editor.Document.GetOffset(pos.Value.Location);
                    Globals globs = GetGlobals();
                    if (globs != null)
                    {
                        bool isFunc = false;
                        string[] words = IntellisenseHelper.ExtractPath(editor.Document, offset, pos.Value.Location.Line, out isFunc);
                        if (words != null && words.Length > 0)
                        {
                            BaseTypeInfo info = null;
                            FunctionInfo func = null;
                            NameResolver reso = new NameResolver(globs, scanner);
                            if (words.Length > 1)
                            {
                                for (int i = 0; i < words.Length; ++i)
                                {
                                    if (i == words.Length - 1 && info != null && isFunc)
                                    {
                                        if (info is TypeInfo)
                                            func = ((TypeInfo)info).Functions.FirstOrDefault(f => f.Name.Equals(words[i]));
                                    }
                                    else
                                    {
                                        if (info == null)
                                        {
                                            info = reso.GetClassType(editor.Document, editor.TextArea.Caret.Line, words[i]);
                                        }
                                        else if (info != null && info is TypeInfo)
                                        {
                                            if (((TypeInfo)info).Properties.ContainsKey(words[i]))
                                                info = ((TypeInfo)info).Properties[words[i]];
                                        }
                                    }
                                }
                            }
                            else if (isFunc && words.Length == 1)
                            {
                                func = globs.GetFunction(words[0]);
                            }
                            else if (!isFunc && words.Length == 1)
                            {
                                info = reso.GetClassType(editor.Document, line, words[0]);
                                if (info == null)
                                {
                                    TypeInfo ty = globs.GetTypeInfo(words[0]);
                                    if (ty != null)
                                        info = ty;
                                }
                            }

                            string msg = "";
                            // Ask documentation for the information
                            if (info != null && func != null && info is TypeInfo)
                            { //member function
                                msg = func.ReturnType.Name + " " + func.Name;
                                string m = IDEProject.inst().DocDatabase.GetDocumentationFor(((TypeInfo)info).Name + "::" + func.Name + func.Inner);
                                if (m != null)
                                    msg += "\r\n" + m;
                            }
                            else if (func != null)
                            { //global function
                                msg = func.ReturnType.Name + " " + func.Name;
                                string m = IDEProject.inst().DocDatabase.GetDocumentationFor(func.Name + func.Inner);
                                if (m != null)
                                    msg += "\r\n" + m;
                            }
                            else if (info != null && info is TypeInfo)
                            { //global or member type
                                msg = ((TypeInfo)info).Name;
                                string m = IDEProject.inst().DocDatabase.GetDocumentationFor(((TypeInfo)info).Name);
                                if (m != null)
                                    msg += "\r\n" + m;
                            }

                            if (msg.Length > 0)
                            {
                                InsightWindow window = new InsightWindow(editor.TextArea);
                                window.Content = msg;
                                window.Show();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //deliberately swallow any exceptions here
                }
            }
        }

        void comp_Closed(object sender, EventArgs e)
        {
            currentComp = null;
        }
        CompletionWindow currentComp = null;
    }
}
