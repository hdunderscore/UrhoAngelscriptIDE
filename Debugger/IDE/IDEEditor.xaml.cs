﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Debugger.Editor;
using ICSharpCode.AvalonEdit;
using Debugger.IDE.Intellisense;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.CodeCompletion;
using FirstFloor.ModernUI.Presentation;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Debugger.IDE {
    /// <summary>
    /// Interaction logic for IDEEditor.xaml
    /// </summary>
    /// 


    public partial class IDEEditor : UserControl {
        FileBaseItem item;
        DepthScanner scanner;
        IntellisenseSource intelSource;

        public IDEEditor(FileBaseItem aItem) {
            InitializeComponent();
            item = aItem;
            changeChecker = new DataChanged { editor = editor };
            SetCode(aItem);
            editor.ShowLineNumbers = true;

            editor.FontFamily = new FontFamily("Consolas");
            editor.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDCDCCC"));
            editor.TextArea.TextView.BackgroundRenderers.Add(new LineHighlighter());
            editor.TextArea.TextView.BackgroundRenderers.Add(new ErrorLineHighlighter(aItem));
            Debugger.Editor.SearchPanel.Install(editor);
            editor.TextChanged += editor_TextChanged;
            scanner = new DepthScanner();
            scanner.Process(editor.Text);
            editor.MouseHover += editor_MouseHover;
            editor.TextArea.MouseWheel += editor_MouseWheel;
            editor.KeyUp += editor_KeyUp;

            intelSource = Intellisense.Sources.SourceBuilder.GetSourceForExtension(System.IO.Path.GetExtension(item.Path));
            editor.ContextMenu = new ContextMenu();

            // Hook the source, so we can get context menu commands from it
            if (intelSource != null)
                intelSource.HookEditor(editor, item);

            editor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Snippet",
                Command = new RelayCommand(p =>
                {
                    ObservableCollection<Snippets.CodeSnippet> snips = new ObservableCollection<Snippets.CodeSnippet>();
                    string ext = System.IO.Path.GetExtension(item.Path);
                    foreach (Snippets.CodeSnippet snip in Snippets.SnippetData.inst().Snippets)
                    {
                        if (snip.Extension.Equals(ext))
                            snips.Add(snip);
                    }
                    if (snips.Count > 0)
                    {
                        Snippets.SnippetDlg dlg = new Snippets.SnippetDlg(editor, snips);
                        dlg.ShowDialog();
                    }
                }, sp =>
                {
                    ObservableCollection<Snippets.CodeSnippet> snips = new ObservableCollection<Snippets.CodeSnippet>();
                    string exte = System.IO.Path.GetExtension(item.Path);
                    foreach (Snippets.CodeSnippet snip in Snippets.SnippetData.inst().Snippets)
                    {
                        if (snip.Extension.Equals(exte))
                            return true;
                    }
                    return false;
                })
            });

            editor.ContextMenu.Items.Add(new Separator());
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Cut",
                Command = ApplicationCommands.Cut
            });
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Copy",
                Command = ApplicationCommands.Copy
            });
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Paste",
                Command = ApplicationCommands.Paste
            });
            editor.ContextMenu.Items.Add(new Separator());
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Undo",
                Command = ApplicationCommands.Undo
            });
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Redo",
                Command = ApplicationCommands.Redo
            });
            editor.ContextMenu.Items.Add(new Separator());
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Save",
                Command = ApplicationCommands.Save
            });
            editor.ContextMenu.Items.Add(new Separator());
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Save As",
                Command = ApplicationCommands.SaveAs
            });
            editor.ContextMenu.Items.Add(new Separator());
            editor.ContextMenu.Items.Add(new MenuItem {
                Header = "Open",
                Command = ApplicationCommands.Open
            });
        }

        void editor_MouseHover(object sender, MouseEventArgs e) {
            if (intelSource != null)
                intelSource.EditorMouseHover(editor, scanner, e);
        }

        void editor_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                var newFontSize = editor.TextArea.FontSize + e.Delta / 50;
                editor.TextArea.FontSize = Math.Max(1, newFontSize);
                e.Handled = true;
            }
        }

        void editor_KeyUp(object sender, KeyEventArgs e) {
            if (intelSource != null)
            {
                intelSource.EditorKeyUp(editor, scanner, e);
            }
        }

        public DataChanged changeChecker { get; set; }

        public class DataChanged : BaseClass {
            public string code;
            public TextEditor editor;

            public void Recheck() {
                backing_ = !code.Equals(editor.Text);
                OnPropertyChanged("IsDirty");
            }

            bool backing_;
            public bool IsDirty {
                get { return backing_; }
                set {
                    backing_ = value;
                    OnPropertyChanged("IsDirty");
                }
            }
        }

        public bool IsDirty {
            get {
                if (item is FileLeafItem) {
                    return changeChecker.IsDirty;
                }
                return false;
            }
        }

        void editor_TextChanged(object sender, EventArgs e) {
            MainWindow.inst().Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate() {
                changeChecker.Recheck();
                DepthScanner newScan = new DepthScanner();
                newScan.Process(editor.Text);
                scanner = newScan;
            });
        }

        public TextEditor Editor { get { return editor; } }

        public void SetCode(FileBaseItem item) {
            try {
                this.item = item;
                editor.Text = File.ReadAllText(item.Path);
                changeChecker.code = editor.Text;
                changeChecker.IsDirty = false;
            }
            catch (Exception ex) {
                ErrorHandler.inst().Error(ex);
            }
        }

        public void Save() {
            File.WriteAllText(item.Path, editor.Text);
            changeChecker.code = editor.Text;
            MainWindow.inst().Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate() {
                changeChecker.Recheck();
            });
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            Save();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }
    }
}
