using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLEditor
{
    public class ControlData : PluginLib.BasePropertyBound, PluginLib.IExternalControlData
    {
        PluginLib.IFileEditor editor_;
        object control_;
        bool dirty_;
        XmlDocument referenceDoc_;
        XmlDocument editDoc_;
        string path_;

        public ControlData(XmlDocument editDoc, string path)
        {
            path_ = path;
            referenceDoc_ = new XmlDocument();
            editDoc_ = editDoc;
            referenceDoc_.Load(path);
        }

        public PluginLib.IFileEditor SourceEditor
        {
            get
            {
                return editor_;
            }
            set
            {
                editor_ = value;
            }
        }

        public object Control
        {
            get
            {
                return control_;
            }
            set
            {
                control_ = value;
                OnPropertyChanged("Control");
            }
        }

        public bool IsDirty
        {
            get {
                return referenceDoc_.OuterXml != editDoc_.OuterXml;
            }
        }

        public void SaveData()
        {
            editDoc_.Save(path_);
            referenceDoc_ = new XmlDocument();
            referenceDoc_.Load(path_);
            OnPropertyChanged("IsDirty");
        }
    }
}
