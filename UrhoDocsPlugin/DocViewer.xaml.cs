using System;
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

namespace UrhoDocsPlugin
{
    /// <summary>
    /// Treeview for IDE.API.APIDocumentation nodes and leaves
    /// Three instances are seen in tabs to the right of the UI
    /// </summary>
    public partial class DocViewer : UserControl
    {
        public DocViewer()
        {
            InitializeComponent();
            tree.PreviewMouseRightButtonDown += tree_PreviewMouseRightButtonDown;
        }

        public string[] CommandText { get; set; }
        public string[] LowerText { get; set; }

        public string[] CommandFormats { get; set; }
        public string[] LowerCommands { get; set; }

        private void tree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                //e.Handled = true;
                API.APINode nd = treeViewItem.DataContext as API.APINode;
                if (nd.ParentCount >= 3 && CommandText != null && CommandText.Length > 0)
                {
                    ContextMenu cmenu = new ContextMenu();
                    TextBlock os = e.OriginalSource as TextBlock;
                    string[] CmdText = null;
                    string[] FmtText = null;
                    if (nd.ParentCount == 3)
                    {
                        CmdText = CommandText;
                        FmtText = CommandFormats;
                    } else if (nd.ParentCount == 4)
                    {
                        CmdText = LowerText;
                        FmtText = LowerCommands;
                    }

                    if (CmdText != null && FmtText != null)
                    {
                        for (int i = 0; i < CmdText.Length; ++i)
                        {
                            int idx = i;
                            cmenu.Items.Add(new MenuItem
                            {
                                Header = CmdText[idx],
                                Command = new RelayCommand(p =>
                                {
                                    string txt = FmtText[idx];
                                    if (os.Text.Contains(":"))
                                        System.Windows.Clipboard.SetText(string.Format(txt, os.Text.Substring(0, os.Text.IndexOf(":")).Trim()));
                                    else
                                        System.Windows.Clipboard.SetText(string.Format(txt, os.Text.Trim()));
                                })
                            });
                        }
                        treeViewItem.ContextMenu = cmenu;
                    }
                }
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }


        public TreeView Tree { get { return tree; } }
    }
}
