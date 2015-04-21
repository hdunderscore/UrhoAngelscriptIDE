using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrhoDocsPlugin
{
    public class AttributesViewer : PluginLib.IInfoTab
    {
        public string GetTabName()
        {
            return "Attributes";
        }

        public PluginLib.IExternalControlData CreateTabContent(string projectPath)
        {
            ControlData data = new ControlData();
            DocViewer view = new DocViewer();
            // Attributes
            view.DataContext = Data.inst().APIDocumentation.DocumentNode.Children.FirstOrDefault(p => p.Name.Equals("Attribute list"));
            if (view.DataContext == null)
                return null;
            data.Control = view;
            return data;
        }
    }

    
}
