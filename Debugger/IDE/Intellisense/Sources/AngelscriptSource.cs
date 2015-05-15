using FirstFloor.ModernUI.Presentation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Debugger.IDE.Intellisense.Sources
{
    [IntelSourceDescriptor(Ext = ".as")]
    public class AngelscriptSource : SourceBase
    {
        Globals documentGlobals_;
        public override Globals GetGlobals()
        {
            if (IDEProject.inst() == null)
                return null;
            if (documentGlobals_ != null)
            {
                documentGlobals_.Parent = IDEProject.inst().GlobalTypes;
                return documentGlobals_;
            }
            return IDEProject.inst().GlobalTypes;
        }


        public override void HookEditor(ICSharpCode.AvalonEdit.TextEditor editor, FileBaseItem item)
        {
            editor.SyntaxHighlighting = AvalonExtensions.LoadHighlightingDefinition("Debugger.Resources.Angelscript.xshd");

            editor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Compile",
                Command = new RelayCommand(p =>
                {
                    IDEView.Compile();
                })
            });
            editor.ContextMenu.Items.Add(new Separator());
            editor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "New ScriptObject",
                Command = new RelayCommand(p =>
                {
                    editor.Document.BeginUpdate();
                    editor.Document.Insert(editor.CaretOffset, SOScript);
                    editor.Document.EndUpdate();
                })
            });
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
                        foreach (string incDir in IDEProject.inst().Settings.GetIncludeDirectories())
                        {
                            if (AngelscriptSource.isSubDir(incDir, dlg.FileName))
                            {
                                // Everything came from System.IO.Path, should be normal form
                                string strippedString = dlg.FileName.Replace(incDir + "\\", ""); //AngelscriptSource.MakeRelativePath(incDir, dlg.FileName);
                                editor.Document.Insert(editor.CaretOffset, string.Format("#include \"{0}\"", strippedString).Replace("\\","/"));
                                editor.Document.EndUpdate();
                                return;
                            }
                        }
                        editor.Document.Insert(editor.CaretOffset, string.Format("#include \"{0}\"", dlg.FileName));
                        editor.Document.EndUpdate();
                    }
                })
            });
            editor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Doxygen Comment",
                Command = new RelayCommand(p =>
                {
                    editor.Document.BeginUpdate();
                    editor.Document.Insert(editor.CaretOffset,
@"/////////////////////////////////////////////////
/// DOCUMENT_HERE
/////////////////////////////////////////////////", AnchorMovementType.AfterInsertion);
                    editor.Document.EndUpdate();
                })
            });
            editor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Property Comment",
                Command = new RelayCommand(p =>
                {
                    editor.Document.BeginUpdate();
                    editor.Document.Insert(editor.CaretOffset, "///< DOCUMENT", AnchorMovementType.AfterInsertion);
                    editor.Document.EndUpdate();
                })
            });
        }

        static string SOScript = @"class MY_SCRIPTOBJECT : ScriptObject
{
    //UNADVISED
    void Start() //called when we're made
    {
    }
    
    void Stop() //called right before we die
    {
    }
    
    //FAVOR THIS INSTEAD OF START
    void DelayedStart() //called after we've been fully init'd (after Start)
    {
    }
    //UNADVISED
    void Update(float td) //ad-hoc update method
    {
    }
    //UNADVISED
    void PostUpdate(float td) //ad-hoc post update method, called after everyone has Update'd
    {
    }
    
    void FixedUpdate(float td) //fixed interval update, use this preferably
    {
    }
    
    void FixedPostUpdate(float td) //See PostUpdate, only for FixedUpdate
    {
    }
    //UNADVISED
    void Save(Serializer& serializer) //Save to disk
    {
    }
    //UNADVISED
    void Load(Deserializer& deserializer) //Read from disk
    {
    }
    //UNADVISED
    void WriteNetworkUpdate(Serializer& serializer) //Save to network packet
    {
    }
    //UNADVISED
    void ReadNetworkUpdate(Deserializer& deserializer) //Read from network packet
    {
    }
    //UNADVISED
    void ApplyAttributes() //Called after are attributes have been set during load
    {
    }

    void TransformChanged() //Called whenever our node's position/rotation/scale changes
    {
    }
}";

        public override void DocumentChanged(TextEditor editor, FileBaseItem item)
        {
            MainWindow.inst().Dispatcher.Invoke(delegate()
            {
                Parsers.AngelscriptParser asp = new Parsers.AngelscriptParser();
                IDEProject.inst().LocalTypes = documentGlobals_ = asp.Parse(item.Path, System.IO.File.ReadAllText(item.Path), IDEProject.inst().Settings.GetIncludeDirectories());
            });
        }

        // http://stackoverflow.com/questions/26102809/c-sharp-check-if-a-file-path-contains-a-specific-directory
        private static bool isSubDir(string parentPath, string childPath)
        {
            var parentUri = new Uri(parentPath);
            var childUri = new DirectoryInfo(childPath).Parent;
            while(childUri != null)
            {
                if(new Uri(childUri.FullName) == parentUri)
                {
                    return true ;
                }
                childUri = childUri.Parent;
            }
            return false;
        }

        // http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        private static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
