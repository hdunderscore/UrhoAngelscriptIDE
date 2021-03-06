﻿using Debugger.Debug;
using FirstFloor.ModernUI.Presentation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Debugger.Editor {
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl {
        FileData data;
        BreakpointMargin bpMargin;
        System.Timers.Timer t;
        IDE.Intellisense.IntellisenseSource intelSource;
        IDE.Intellisense.DepthScanner depthScanner;
        FoldingManager foldingManager;
        BraceFoldingStrategy codeFolding;

        public CodeEditor(FileData aModelData) {
            InitializeComponent();
            aModelData.AttachEditor(editor);
            this.data = aModelData;
            editor.ShowLineNumbers = true;
            editor.Options.ConvertTabsToSpaces = true;
            editor.Options.IndentationSize = 4;
            editor.Foreground = new SolidColorBrush(Colors.White);
            editor.SyntaxHighlighting = AvalonExtensions.LoadHighlightingDefinition("Debugger.Resources.Angelscript.xshd");
            editor.Text = data.Code;
            editor.FontFamily = new FontFamily("Consolas");
            editor.TextArea.TextView.CurrentLineBackground = new SolidColorBrush(Colors.LightGray);
            editor.TextArea.TextView.BackgroundRenderers.Add(new LineHighlighter());
            editor.TextArea.LeftMargins.Insert(0, bpMargin = new BreakpointMargin(aModelData));
            SearchPanel panel = SearchPanel.Install(editor.TextArea);
            aModelData.PropertyChanged += aModelData_PropertyChanged;
            foldingManager = FoldingManager.Install(editor.TextArea);
            codeFolding = new BraceFoldingStrategy();
            UpdateFolding();

            // If using IDE data then give ourselves an IntellisenseSource
            if (Debug.SessionData.inst().Settings.UseIDEData)
                intelSource = IDE.Intellisense.Sources.SourceBuilder.GetSourceForExtension(System.IO.Path.GetExtension(aModelData.SectionName));

            depthScanner = new IDE.Intellisense.DepthScanner();
            depthScanner.Process(editor.Text);

            editor.MouseHover += editor_MouseHover;
            editor.TextArea.MouseWheel += editor_MouseWheel;
            editor.KeyUp += editor_KeyUp;
            editor.TextChanged += editor_TextChanged;

            t = new System.Timers.Timer();
            t.Interval = 175;
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        void editor_TextChanged(object sender, EventArgs e)
        {
            string text = editor.Text;
            new Thread(delegate()
            {
                IDE.Intellisense.DepthScanner newScan = new IDE.Intellisense.DepthScanner();
                newScan.Process(text);
                MainWindow.inst().Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                {
                    depthScanner = newScan;
                });
            }).Start();
            UpdateFolding();
        }

        void editor_KeyUp(object sender, KeyEventArgs e)
        {
            if (intelSource != null)
            {
                intelSource.EditorKeyUp(editor, depthScanner, e);
            }
        }

        void t_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                MainWindow.inst().Dispatcher.Invoke(delegate()
                {
                    bpMargin.InvalidateVisual();
                });
            }
            catch (Exception ex)
            {
                // Swallow the exception, it's probably just a shut down issue
            }
        }

        public static ICommand SetBPCommand = new RoutedCommand();

        void editor_MouseHover(object sender, MouseEventArgs e) {
            var pos = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (pos != null) {
                string wordHovered = editor.Document.GetWordUnderMouse(pos.Value, true);
                if (Debugger.Debug.SessionData.inst().LocalData != null)
                {
                    Json.JWrapper wrapper = null;

                    // Try to find it in "this"
                    string[] words = wordHovered.Split('.');
                    if (Debugger.Debug.SessionData.inst().ThisData != null)
                        wrapper = Debugger.Debug.SessionData.inst().ThisData.ResolveDotPath(words);
                    if (wrapper != null && wrapper.Parent == null)
                        wrapper = null; //reset to null so other checks have an opportunity

                    // Check the Stack
                    if (wrapper == null)
                        wrapper = Debugger.Debug.SessionData.inst().LocalData.ResolveDotPath(words);
                    if (wrapper != null && wrapper.Parent == null)
                        wrapper = null; //reset to null so globals can have a chance
                    
                    // Check the globals
                    if (wrapper == null && Debugger.Debug.SessionData.inst().GlobalData != null)
                        wrapper = Debugger.Debug.SessionData.inst().GlobalData.ResolveDotPath(words);

                    // If something has been found then show it in AvalonEdit's "Insight Window"
                    if (wrapper != null && wrapper.Parent != null) { //null check prevents display of all stack levels
                        InsightWindow window = new InsightWindow(editor.TextArea);
                        window.Background = new SolidColorBrush(Colors.Black);
                        if (wrapper is Json.JLeaf)
                            window.Content = new Label { Content = ((Json.JLeaf)wrapper).Value };
                        else
                            window.Content = new Controls.JWrapView() { DataContext = wrapper };
                        window.MaxHeight = 240;
                        window.SizeToContent = SizeToContent.WidthAndHeight;
                        window.Left = Mouse.GetPosition(this).X;
                        window.Top = Mouse.GetPosition(this).X;
                        window.Show();
                    }
                }
                e.Handled = true;
            }
        }

        void editor_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                var newFontSize = editor.TextArea.FontSize + e.Delta / 50;
                editor.TextArea.FontSize = Math.Max(1, newFontSize);
                e.Handled = true;
            }
        }

        void aModelData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName.Equals("CurrentLine") || e.PropertyName.Equals("CurrentSection"))
                bpMargin.InvalidateVisual();
        }

        public TextEditor Editor { get { return editor; } }

        public void SetCode(string code) {
            editor.Text = code;
        }

        public void Invalidate() {
            bpMargin.InvalidateVisual();
        }

        void setBPAtCursor(object who, RoutedEventArgs args) {
            CodeEditor editor = who as CodeEditor;
            int ln = editor.editor.TextArea.TextView.HighlightedLine;
            if (ln <= 0)
                return;
            Debugger.Debug.Breakpoint bp = editor.data.BreakPoints.FirstOrDefault(b => b.LineNumber == ln);
            if (bp == null) {
                editor.data.BreakPoints.Add(new Debugger.Debug.Breakpoint
                {
                    LineNumber = ln,
                    File = editor.data.SectionName,
                    SectionID = editor.data.SectionID,
                    Active = true
                });
            } else
                bp.Active = !bp.Active;
            bpMargin.InvalidateVisual();
        }

        private void UpdateFolding()
        {
            if (codeFolding != null)
                codeFolding.UpdateFoldings(foldingManager, editor.Document);
            bpMargin.InvalidateVisual();
        }
    }
}
