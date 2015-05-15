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
            view.CommandText = new string[] { "Copy Getter", "Copy Setter"};
            view.CommandFormats = new string[] { "GetAttribute(\"{0}\"){1};", "SetAttribute(\"{0}\", VALUE);" };

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

            return data;
        }
    }

    
}
