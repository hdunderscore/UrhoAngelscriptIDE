using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrhoDocsPlugin
{
    public class EventsViewer : PluginLib.IInfoTab
    {
        public string GetTabName()
        {
            return "Events";
        }

        public PluginLib.IExternalControlData CreateTabContent(string projectPath)
        {
            ControlData data = new ControlData();
            DocViewer view = new DocViewer();
            // Events
            view.DataContext = Data.inst().APIDocumentation.DocumentNode.Children.FirstOrDefault(p => p.Name.Equals("Events"));
            if (view.DataContext == null)
                return null;
            view.CommandText = new string[] { "Copy Subscription", "Copy Unsubscription" };
            view.CommandFormats = new string[] { "SubscribeToEvent(\"{0}\",\"Handle{0}\");", "UnsubscribeFromEvent(\"Handle{0}\");" };
            view.LowerText = new string[] { "Copy event getter" };
            view.LowerCommands = new string[] { "eventData[\"{0}\"];" };
            data.Control = view;
            return data;
        }
    }
}
