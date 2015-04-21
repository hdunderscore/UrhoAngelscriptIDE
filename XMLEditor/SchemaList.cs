using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace XMLEditor
{
    public class SchemaModel : PluginLib.BasePropertyBound
    {
        public string Name { get; set; }
        public XmlSchema Schema { get; set; }
    }

    static class SchemaList
    {
        public static List<SchemaModel> GetSchemas()
        {
            List<SchemaModel> schemas = new List<SchemaModel>();
            string path = System.Reflection.Assembly.GetCallingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);
            path = System.IO.Path.Combine(path, "schemas");
            if (System.IO.Directory.Exists(path))
            {
                foreach (string file in System.IO.Directory.EnumerateFiles(path))
                {
                    try
                    {
                        XmlSchema schema = XmlSchema.Read(new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite), null);
                        schemas.Add(new SchemaModel { Name = System.IO.Path.GetFileNameWithoutExtension(file), Schema = schema });
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return schemas;
        }
    }
}
