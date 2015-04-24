using FirstFloor.ModernUI.Windows;
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
using System.IO;
using System.Collections.ObjectModel;
using FirstFloor.ModernUI.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using Debugger.IDE.Intellisense;

namespace Debugger.IDE {
    /// <summary>
    /// Interaction logic for IDEView.xaml
    /// </summary>
    public partial class IDEView : UserControl, IContent, PluginLib.ISearchPublisher {
        IDEProject project_;
        Folder folder_;
        ObservableCollection<PluginLib.SearchResult> searchResults_ = new ObservableCollection<PluginLib.SearchResult>();

        public ObservableCollection<PluginLib.SearchResult> SearchResults { get { return searchResults_; } }

        static IDEView inst_;
        public static IDEView inst() { return inst_; }

        public IDEView() {
            InitializeComponent();
            inst_ = this;
            gridSearch.DataContext = this;
            Activity.IDBBuilderActivity.BuildIntellisenseDatabase();

            foreach (PluginLib.ISearchService searchService in PluginManager.inst().SearchServices.OrderBy(l => l.Name))
                comboSearchType.Items.Add(searchService.Name);
            comboSearchType.SelectedIndex = 0;
        }

        //Compile the current file only
        void onCompileFile(object sender, EventArgs e) {
        }

        //Compile everything
        void onCompile(object sender, EventArgs e) {
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) {
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {
            if (e.NavigationType == FirstFloor.ModernUI.Windows.Navigation.NavigationType.New)
            {
                if (IDEProject.inst() == null)
                {
                    IDEProject.open();
                    System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        IDEProject.inst().Settings = IDESettings.GetOrCreate(dlg.SelectedPath);
                        IDEProject.inst().ProjectDir = dlg.SelectedPath;
                        UserData.inst().AddRecentFile(dlg.SelectedPath);
                    }
                    else
                    {
                        MainWindow.inst().ContentSource = new Uri("Screens/LaunchScreen.xaml", UriKind.Relative);
                        IDEProject.inst().destroy();
                        return;
                    }
                }

                if (IDEProject.inst() == null)
                {
                    project_ = new IDEProject();
                }
                else
                    project_ = IDEProject.inst();
                if (folder_ == null)
                    folder_ = new Folder { Path = project_.ProjectDir };
                if (fileTree.DataContext == null)
                    fileTree.DataContext = folder_;
                Action<object> searchAction = delegate(object o)
                {
                    if (o == null)
                        return;
                    leftSideTabs.SelectedItem = classesTab;
                    objectTree.SelectItemNamed(((PropInfo)o).Type.Name);
                };
                objectTree.DataContext = IDEProject.inst();
                objectTree.ItemBinding = new Binding("GlobalTypes.TypeInfo");
                objectTree.CallOnViewType = searchAction;

                globalsTree.DataContext = IDEProject.inst();
                globalsTree.CallOnViewType = searchAction;
                globalsTree.ItemBinding = new Binding("GlobalTypes.UIView");

                txtConsole.DataContext = IDEProject.inst();
                gridErrors.DataContext = IDEProject.inst();
                errorTabCount.DataContext = IDEProject.inst();
                stackErrorHeader.DataContext = IDEProject.inst();

                if (infoTabs.Items.Count == 0)
                {
                    foreach (PluginLib.IInfoTab infoTab in PluginManager.inst().InfoTabs)
                    {
                        PluginLib.IExternalControlData data = infoTab.CreateTabContent(IDEProject.inst().ProjectDir);
                        if (data == null)
                            continue;
                        TabItem item = new TabItem
                        {
                            Header = infoTab.GetTabName(),
                            Tag = data,
                            Content = data.Control
                        };
                        infoTabs.Items.Add(item);
                    }
                }
            }
            else
            {
                infoTabs.Items.Clear();
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) {
        }


        private void fileTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileBaseItem item = fileTree.SelectedItem as FileBaseItem;
            if (sender is TreeViewItem && ((TreeViewItem)sender).DataContext is FileLeafItem)
            {
                e.Handled = true;
                ideTabs.OpenFile(item);
            }
        }

        void onNewFolder(object sender, EventArgs e) {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            FileBaseItem target = (menu.PlacementTarget as StackPanel).Tag as FileBaseItem;

            string r = Debugger.Dlg.InputDlg.Show("Create Folder", "Name of new folder:");
            if (r != null) {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(target.Path, r));
            }
        }

        void onNewFile(object sender, EventArgs e) {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            FileBaseItem target = (menu.PlacementTarget as StackPanel).Tag as FileBaseItem;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = target.Path;
            dlg.DefaultExt = "as";
            dlg.Filter = "Script (*.as)|*.as|Material (*.xml)|*.xml";
            if (dlg.ShowDialog() == true) {
                File.WriteAllText(dlg.FileName, "");
                ideTabs.OpenFile(new FileBaseItem { Path = dlg.FileName, Name = dlg.FileName });
            }
        }

        void onEditFile(object sender, EventArgs e) {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            FileBaseItem target = (menu.PlacementTarget as StackPanel).Tag as FileBaseItem;
            if (target is FileLeafItem)
                ideTabs.OpenFile(target);
        }

        void onRenameFolder(object sender, EventArgs e) {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            FileBaseItem target = (menu.PlacementTarget as StackPanel).Tag as FileBaseItem;
            string newName = Debugger.Dlg.RenameDlg.Show(target.Name);
            if (newName.Length > 0) {
                try {
                    string dir = System.IO.Path.GetDirectoryName(target.Path);
                    Directory.Move(target.Path, System.IO.Path.Combine(dir, newName));
                } catch (Exception ex) {
                    ErrorHandler.inst().Error(ex);
                }
            }
        }
        void onRenameFile(object sender, EventArgs e) {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            FileBaseItem target = (menu.PlacementTarget as StackPanel).Tag as FileBaseItem;
            string newName = Debugger.Dlg.RenameDlg.Show(target.Name);
            if (newName.Length > 0) {
                try {
                    string dir = System.IO.Path.GetDirectoryName(target.Path);
                    File.Move(target.Path, System.IO.Path.Combine(dir, newName));
                } catch (Exception ex) {
                    ErrorHandler.inst().Error(ex);
                }
            }
        }
        void onDeleteFile(object sender, EventArgs e) {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            FileBaseItem target = (menu.PlacementTarget as StackPanel).Tag as FileBaseItem;
            if (Debugger.Dlg.ConfirmDlg.Show(string.Format("Delete {1} '{0}'?", target.Name, (target is Folder) ? "folder" : "file")) == true) {
                try {
                    if (target.Parent is Folder)
                        ((Folder)target.Parent).Children.Remove(target);
                    FileOperationAPIWrapper.MoveToRecycleBin(target.Path);
                } catch (Exception ex) {
                    ErrorHandler.inst().Error(ex);
                }
            }
        }

        private void txtSearchString_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (comboSearchType.SelectedItem == null)
                {
                    return;
                }
                searchResults_.Clear();
                string selSearchKind = comboSearchType.SelectedItem.ToString();
                foreach (PluginLib.ISearchService searchService in PluginManager.inst().SearchServices) 
                {
                    if (searchService.Name.Equals(selSearchKind))
                    {
                        searchService.Search(IDEProject.inst().ProjectDir, new string[] { txtSearchString.Text }, this);
                        break;
                    }
                }
            }
        }

        void searchDoubleClick(object sender, MouseEventArgs args) {
            DataGridRow row = sender as DataGridRow;
            PluginLib.SearchResult result = row.DataContext as PluginLib.SearchResult;
            ideTabs.OpenFile(new FileLeafItem {
                Path = result.File,
                Name = System.IO.Path.GetFileName(result.File)
            }, result.Line);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            IDESettingsDlg dlg = new IDESettingsDlg();
            dlg.ShowDialog();
        }

        private void errorDoubleClick(object sender, MouseEventArgs args) {
            DataGridRow row = sender as DataGridRow;
            PluginLib.CompileError result = row.DataContext as PluginLib.CompileError;
            IDEEditor editor = ideTabs.OpenFile(new FileLeafItem {
                Path = result.File,
                Name = result.File.Replace(IDEProject.inst().ProjectDir, "")
            });
            if (result.Line != -1) {
                editor.Editor.TextArea.Caret.Line = result.Line;
                editor.Editor.ScrollToLine(result.Line);
            }
        }

        private void btnCompile_Click(object sender, RoutedEventArgs e) {
            Compile();
        }

        public static void Compile() {
            if (IDEProject.inst().Settings.CompilerPath == null || IDEProject.inst().Settings.CompilerPath.Trim().Length == 0) {
                if (ModernDialog.ShowMessage("You need to set a compile file in settings", "No princess here", System.Windows.MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                    IDESettingsDlg dlg = new IDESettingsDlg();
                    dlg.ShowDialog();
                }
                return;
            }
            IDEProject.inst().CompilerOutput = "";
            IDEProject.inst().CompileErrors.Clear();
            PluginLib.ICompilerService comp = null;

            if (IDEProject.inst().Settings.Compiler != null && IDEProject.inst().Settings.Compiler.Length > 0)
            {
                comp = PluginManager.inst().Compilers.FirstOrDefault(c => c.Name.Equals(IDEProject.inst().Settings.Compiler));
                if (comp == null) {
                    ModernDialog.ShowMessage(String.Format("Unable to find compiler: \"{0}\"", IDEProject.inst().Settings.Compiler), "Error", MessageBoxButton.OK);
                    return;
                }
            }
            else
            {
                comp = PluginManager.inst().Compilers.FirstOrDefault();
                if (comp == null)
                {
                    ModernDialog.ShowMessage("No compiler plugins are installed", "Error", MessageBoxButton.OK);
                    return;
                }
            }
            
            
            Parago.Windows.ProgressDialogResult result = Parago.Windows.ProgressDialog.Execute(null, "Compiling...", (a,b) => {
                comp.CompileFile(IDEProject.inst().Settings.CompilerPath, IDEProject.inst(), ErrorHandler.inst());

                MainWindow.inst().Dispatcher.Invoke(delegate() {
                    if (IDEProject.inst().CompileErrors.Count != 0)
                    {
                        Dlg.CompErrDlg dlg = new Dlg.CompErrDlg();
                        dlg.ShowDialog();
                    }

                    if (IDEProject.inst().CompileErrors.Count == 0)
                    {
                        foreach (PluginLib.ICompilerService c in PluginManager.inst().Compilers)
                            c.PostCompile(IDEProject.inst().Settings.CompilerPath, IDEProject.inst().Settings.SourceTree, ErrorHandler.inst());
                        Dlg.CompSuccessDlg dlg = new Dlg.CompSuccessDlg();
                        dlg.ShowDialog();
                    }
                });
            });
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {

        }

        private void btnRun_Click(object sender, RoutedEventArgs e) {
            if (IDEProject.inst().Settings.RunExe == null || IDEProject.inst().Settings.RunExe.Trim().Length == 0) {
                if (ModernDialog.ShowMessage("You need to set a 'run' target", "Run what?", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                    btnSettings_Click(null, null);
                }
                return;
            }
            Process pi = new Process();
            pi.StartInfo.FileName = IDEProject.inst().Settings.RunExe;
            pi.StartInfo.Arguments = IDEProject.inst().Settings.CompilerPath + " " + IDEProject.inst().Settings.RunParams;
            pi.EnableRaisingEvents = true;
            pi.StartInfo.UseShellExecute = false;
            pi.StartInfo.CreateNoWindow = false;
            pi.StartInfo.RedirectStandardOutput = false;
            pi.Start();
        }

        private void btnDebug_Click(object sender, RoutedEventArgs e) {
            if (IDEProject.inst().Settings.RunExe == null || IDEProject.inst().Settings.RunExe.Trim().Length == 0) {
                if (ModernDialog.ShowMessage("You need to set a 'debug' target", "Debug what?", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                    btnSettings_Click(null, null);
                }
                return;
            }
            Process pi = new Process();
            pi.StartInfo.FileName = IDEProject.inst().Settings.DebugExe;
            pi.StartInfo.Arguments = IDEProject.inst().Settings.CompilerPath + " " + IDEProject.inst().Settings.DebugParams;
            pi.EnableRaisingEvents = true;
            pi.StartInfo.UseShellExecute = false;
            pi.StartInfo.CreateNoWindow = false;
            pi.StartInfo.RedirectStandardOutput = false;
            pi.Start();
        }

        public void PublishSearchResult(PluginLib.SearchResult result)
        {
            Dispatcher.Invoke(delegate()
            {
                searchResults_.Add(result);
            });
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == splitRightVertical) // Info Tabs
            {
                if (ideGrid.ColumnDefinitions[4].Width.Value == 0)
                    ideGrid.ColumnDefinitions[4].Width = new GridLength(200, GridUnitType.Pixel);
                else
                    ideGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
            }
            else if (sender == splitLeftVertical) //Files
            {
                if (ideGrid.ColumnDefinitions[0].Width.Value == 0)
                    ideGrid.ColumnDefinitions[0].Width = new GridLength(200, GridUnitType.Pixel);
                else
                    ideGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
            }
            else if (sender == splitLog) //Log/Errors
            {
                if (ideGrid.RowDefinitions[2].Height.Value == 26)
                    ideGrid.RowDefinitions[2].Height = new GridLength(200, GridUnitType.Pixel);
                else
                    ideGrid.RowDefinitions[2].Height = new GridLength(26, GridUnitType.Pixel);
            }
        }
    }
}