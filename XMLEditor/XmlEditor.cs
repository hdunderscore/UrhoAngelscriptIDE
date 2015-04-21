using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLEditor
{
    public class XmlEditor : PluginLib.IFileEditor
    {
        public bool CanEditFile(string filePath, string fileExtension)
        {
            if (fileExtension.ToLowerInvariant().Equals(".xml"))
                return true;
            return false;
        }

        public PluginLib.IExternalControlData CreateEditorContent(string filePath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                ControlData ret = new ControlData(doc, filePath);
                Views.TreeEditorView view = new Views.TreeEditorView();
                view.CommandBindings.Add(new System.Windows.Input.CommandBinding(
                    System.Windows.Input.ApplicationCommands.Save, //CTRL+S
                    (sender, e) =>
                    { //Exectued
                        ret.SaveData();
                    },
                    (sender, e) =>
                    { //CanExecute
                        e.CanExecute = ret.IsDirty;
                    }));
                view.Tag = ret;
                Action<object> callback = delegate(object o) {
                    ret.OnPropertyChanged("IsDirty");
                };

                view.DataContext = new ViewModels.TreeEditorViewModel(doc, callback, filePath, System.IO.Path.GetFileName(filePath));
                view.DataChanged = callback;
                ret.Control = view;
                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
