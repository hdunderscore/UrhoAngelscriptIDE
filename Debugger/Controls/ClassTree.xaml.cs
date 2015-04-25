using Debugger.IDE;
using Debugger.IDE.Intellisense;
using System;
using System.Collections;
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

namespace Debugger.Controls
{
    /// <summary>
    /// Interaction logic for ClassTree.xaml
    /// </summary>
    public partial class ClassTree : UserControl
    {
        public Action<object> CallOnViewType;

        public ClassTree()
        {
            InitializeComponent();
            DataContextChanged += ClassTree_DataContextChanged;
        }

        void ClassTree_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is IEnumerable)
                objectTree.ItemsSource = (IEnumerable)DataContext;
        }

        public Binding ItemBinding
        {
            set
            {
                objectTree.SetBinding(TreeView.ItemsSourceProperty, value);
            }
        }

        private void txtSearchClasses_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtSearchClasses.Text.Trim().Length > 0)
                {
                    string searchString = txtSearchClasses.Text.Trim().ToLower();

                    SelectItemNamed(searchString);
                }
            }
        }

        private void txtSearchClasses_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearchClasses.SelectAll();
        }

        public void SelectItemNamed(string name)
        {
            foreach (object o in objectTree.Items)
            {
                if (o is TypeInfo)
                {
                    if (((TypeInfo)o).Name.ToLower().Equals(name.ToLower()))
                    {
                        TreeViewItem titem = ((TreeViewItem)objectTree.ItemContainerGenerator.ContainerFromItem(o));
                        if (titem != null)
                        {
                            titem.BringIntoView();
                            titem.IsSelected = true;
                            return;
                        }
                    }
                } else if (o is PropInfo) {
                    if (((PropInfo)o).Name.ToLower().Equals(name.ToLower()))
                    {
                        TreeViewItem titem = ((TreeViewItem)objectTree.ItemContainerGenerator.ContainerFromItem(o));
                        if (titem != null)
                        {
                            titem.BringIntoView();
                            titem.IsSelected = true;
                            return;
                        }
                    }
                }
                else if (o is FunctionInfo)
                {
                    if (((FunctionInfo)o).Name.ToLower().Equals(name.ToLower()))
                    {
                        TreeViewItem titem = ((TreeViewItem)objectTree.ItemContainerGenerator.ContainerFromItem(o));
                        if (titem != null)
                        {
                            titem.BringIntoView();
                            titem.IsSelected = true;
                            return;
                        }
                    }
                }
            }
        }

        private void onDocumentClasses(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;

            TypeInfo ti = (menu.PlacementTarget as StackPanel).Tag as TypeInfo;
            if (ti != null)
            {
                IDEProject.inst().DocDatabase.Document(ti.Name);
                return;
            }
            PropInfo pi = (menu.PlacementTarget as StackPanel).Tag as PropInfo;
            if (pi != null)
            {
                // Get the parent treeitem
                TreeViewItem treeViewItem = VisualUpwardSearch(VisualTreeHelper.GetParent(VisualUpwardSearch(menu.PlacementTarget as DependencyObject)));
                if (treeViewItem == null)
                    IDEProject.inst().DocDatabase.Document(pi.Name);
                else
                {
                    TypeInfo parentType = treeViewItem.DataContext as TypeInfo;
                    IDEProject.inst().DocDatabase.Document(parentType.Name + "::" + pi.Name);
                }
                return;
            }
            FunctionInfo fi = (menu.PlacementTarget as StackPanel).Tag as FunctionInfo;
            if (fi != null)
            {
                TreeViewItem treeViewItem = VisualUpwardSearch(VisualTreeHelper.GetParent(VisualUpwardSearch(menu.PlacementTarget as DependencyObject)));
                if (treeViewItem == null)
                    IDEProject.inst().DocDatabase.Document(fi.Name + fi.Inner);
                else
                {
                    TypeInfo parentType = treeViewItem.DataContext as TypeInfo;
                    IDEProject.inst().DocDatabase.Document(parentType.Name + "::" + fi.Name + fi.Inner);
                }
                return;
            }
        }


        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void onViewType(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            PropInfo pi = (menu.PlacementTarget as StackPanel).Tag as PropInfo;
            if (pi != null && CallOnViewType != null)
                CallOnViewType(pi);
        }

        private void onGoToDef(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            FunctionInfo fi = (menu.PlacementTarget as StackPanel).Tag as FunctionInfo;
            if (fi != null)
            {
                IDEView.inst().ideTabs.OpenFile(new FileLeafItem {
                        Path = fi.SourceFile,
                        Name = System.IO.Path.GetFileName(fi.SourceFile)
                    }, fi.SourceLine);
            }
            PropInfo pi = (menu.PlacementTarget as StackPanel).Tag as PropInfo;
            if (pi != null)
            {
                IDEView.inst().ideTabs.OpenFile(new FileLeafItem
                {
                    Path = pi.SourceFile,
                    Name = System.IO.Path.GetFileName(pi.SourceFile)
                }, pi.SourceLine);
            }
            TypeInfo ti = (menu.PlacementTarget as StackPanel).Tag as TypeInfo;
            if (ti != null)
            {
                IDEView.inst().ideTabs.OpenFile(new FileLeafItem
                {
                    Path = ti.SourceFile,
                    Name = System.IO.Path.GetFileName(ti.SourceFile)
                }, ti.SourceLine);
            }
        }
    }
}
