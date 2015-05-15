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
            view.DataContext = Data.inst().APIDocumentation.DocumentNode.Children.FirstOrDefault(p => p.Name.Equals("Event list"));
            if (view.DataContext == null)
                return null;
            view.CommandText = new string[] { "Copy Subscription", "Copy Unsubscription", "Copy Handler Method" };
            view.CommandFormats = new string[] { "SubscribeToEvent(\"{0}\",\"Handle{0}\");", "UnsubscribeFromEvent(\"Handle{0}\");", "void Handle{0}(StringHash eventType, VariantMap& eventData)\r\n{{\r\n}}" };
            view.LowerText = new string[] { "Copy event getter" };
            view.LowerCommands = new string[] { "eventData[\"{0}\"]{1};" };
            view.RemapAnnotes["float"] = ".GetFloat()";
            view.RemapAnnotes["unsigned"] = ".GetUInt()";
            view.RemapAnnotes["int"] = ".GetInt()";
            view.RemapAnnotes["bool"] = ".GetBool()";
            view.RemapAnnotes["String"] = ".GetString()";
            view.RemapAnnotes["StringHash"] = ".GetStringHash()";
            view.RemapAnnotes["VariantVector"] = ".GetVariantVector()";
            view.RemapAnnotes["IntVector2"] = ".GetIntVector2()";
            view.RemapAnnotes["IntRect"] = ".GetIntRect()";
            view.RemapAnnotes["Vector2"] = ".GetVector2()";
            view.RemapAnnotes["Vector3"] = ".GetVector3()";
            view.RemapAnnotes["Vector4"] = ".GetVector4()";
            view.RemapAnnotes["Quaternion"] = ".GetQuaternion()";
            view.RemapAnnotes["Matrix3"] = ".GetMatrix3()";
            view.RemapAnnotes["Matrix3x4"] = ".GetMatrix3x4()";
            view.RemapAnnotes["Matrix4"] = ".GetMatrix4()";
            view.RemapAnnotes["Color"] = ".GetColor()";
            view.RemapAnnotes["ResourceRef"] = ".GetResourceRef()";
            view.RemapAnnotes["ResourceRefList"] = ".GetResourceRefList()";
            view.RemapAnnotes["VariantMap"] = ".GetVariantMap()";
            view.RemapAnnotes["VectorBuffer"] = ".GetBuffer()";

            view.RemapAnnotes["pointer"] = ".GetPtr()";
            data.Control = view;
            return data;
        }
    }
}
