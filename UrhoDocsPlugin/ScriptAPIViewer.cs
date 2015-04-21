using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrhoDocsPlugin
{
    public class ScriptAPIViewer : PluginLib.IInfoTab
    {
        public string GetTabName()
        {
            return "Script API";
        }

        public PluginLib.IExternalControlData CreateTabContent(string projectPath)
        {
            ControlData data = new ControlData();
            DocViewer view = new DocViewer();
            // Script API, user problems with the "source tree" in settings can lead to this
            view.DataContext = Data.inst().APIDocumentation.DocumentNode.Children.FirstOrDefault(p => p.Name.Equals("Scripting API"));
            if (view.DataContext == null)
                return null;
            data.Control = view;
            return data;
        }
    }
}
