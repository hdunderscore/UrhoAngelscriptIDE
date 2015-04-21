using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class ControlData : PluginLib.IExternalControlData
    {
        PluginLib.IFileEditor editor_;
        object control_;

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
            }
        }

        public bool IsDirty
        {
            get { return false; }
        }

        public void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
