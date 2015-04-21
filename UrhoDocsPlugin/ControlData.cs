using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrhoDocsPlugin
{
    class ControlData : PluginLib.BasePropertyBound, PluginLib.IExternalControlData
    {
        PluginLib.IFileEditor editor_;
        DocViewer docView_;

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
                return docView_;
            }
            set
            {
                if (value is DocViewer)
                {
                    docView_ = value as DocViewer;
                }
                OnPropertyChanged("Control");
            }
        }

        public bool IsDirty
        {
            get { return false; } //Unused
        }

        public void SaveData()
        {
            //Unusued
        }
    }
}
