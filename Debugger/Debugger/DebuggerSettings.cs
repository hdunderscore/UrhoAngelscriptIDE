using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.Debug
{
    [Serializable]
    public class DebuggerSettings : BaseClass
    {
        bool doNotSave = true;
        string connectionURI_ = "localhost:9002";
        bool playBeepSound_ = true;
        bool useIDEData_ = false;

        public string ConnectionURI { 
            get { return connectionURI_; }
            set { connectionURI_ = value; OnPropertyChanged("ConnectionURI"); if (!doNotSave) Save(); }
        }
        public bool PlayBeepSound { 
            get {return playBeepSound_; }
            set { playBeepSound_ = value; OnPropertyChanged("PlayBeepSound"); if (!doNotSave) Save(); }
        }
        public bool UseIDEData {
            get { return useIDEData_; }
            set { useIDEData_ = value; OnPropertyChanged("UseIDEData"); if (!doNotSave) Save(); }
        }

        public static DebuggerSettings LoadSettings()
        {
            string fileName = UserData.GetAppDataPath("DebuggerSettings.xml");
            try
            {
                if (File.Exists(fileName))
                {
                    System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DebuggerSettings));

                    DebuggerSettings ud = new DebuggerSettings();
                    using (System.IO.FileStream file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        ud = (DebuggerSettings)reader.Deserialize(file);
                        file.Close();
                    }
                    ud.doNotSave = false;
                    return ud;
                }
                return new DebuggerSettings();
            }
            catch (Exception ex)
            {
                ErrorHandler.inst().Error(ex);
                DebuggerSettings ret = new DebuggerSettings();
                ret.doNotSave = false;
                return ret;
            }
            finally { }
        }

        void Save()
        {
            string fileName = UserData.GetAppDataPath("DebuggerSettings.xml");
            try
            {
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DebuggerSettings));

                using (System.IO.FileStream file = new System.IO.FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    writer.Serialize(file, this);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.inst().Error(ex);
            }
        }
    }
}
