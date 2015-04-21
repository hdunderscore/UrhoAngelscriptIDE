using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class AudioViewer : PluginLib.IFileEditor
    {
        public bool CanEditFile(string filePath, string fileExtension)
        {
            if (fileExtension.ToLowerInvariant().Equals(".wav") ||
                fileExtension.ToLowerInvariant().Equals(".ogg"))
            {
                return true;
            }
            return false;
        }

        public PluginLib.IExternalControlData CreateEditorContent(string filePath)
        {
            ControlData ret = new ControlData();
            ret.Control = new AudioPanel(filePath);
            return ret;
        }
    }
}
