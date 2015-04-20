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

namespace Debugger.Controls {
    /// <summary>
    /// Treeview for the JWrapper and JLeaf classes
    /// Used by the Debugger's locals/this/globals
    /// </summary>
    public partial class JWrapView : UserControl {
        public JWrapView() {
            InitializeComponent();
            PrefixWatches = "";
        }

        public string PrefixWatches { get; set; }
        public TreeView View { get { return globalTree; } }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem item = sender as MenuItem;
            ContextMenu menu = item.CommandParameter as ContextMenu;
            Json.JLeaf target = (menu.PlacementTarget as StackPanel).Tag as Json.JLeaf;
            if (target != null) {
                Debugger.Debug.WatchValue watch = new Debugger.Debug.WatchValue { Variable = PrefixWatches + target.GetDotPath() };
                Debugger.Debug.SessionData.inst().AddWatch(watch);
            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Debugger.Net.DebugClient.inst() == null)
                return;
            Label lbl = sender as Label;
            Json.JLeaf wrapper = lbl.Tag as Json.JLeaf;
            if (wrapper != null)
            {
                string newValue = Dlg.InputDlg.Show(String.Format("Set Value of {0}", wrapper.Name), "New Value:", wrapper.Value);
                if (newValue != null && newValue != wrapper.Value)
                {
                    string tildePath = wrapper.GetTildePath();
                    Json.JWrapper topLevel = wrapper.GetTopMost();
                    wrapper.Value = newValue;
                    if (topLevel.Name.Equals("This"))
                    {
                        // The userdata int is stored as "Depth" but need the index here
                        int value = Debug.SessionData.inst().CallStack.Count - 1;
                        tildePath = String.Format("{0}~{1}", value - ((int)topLevel.UserData), tildePath);
                        Debugger.Net.DebugClient.inst().SetThisValue(tildePath, newValue);
                    }
                    else if (topLevel.Name.Equals("Globals"))
                    {
                        Debugger.Net.DebugClient.inst().SetGlobalValue(tildePath, newValue);
                    } else
                        Debugger.Net.DebugClient.inst().SetStackValue(tildePath, newValue);
                }
            }
        }
    }
}
