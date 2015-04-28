using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Xml;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Presentation;

namespace Debugger.IDE.Dlg
{
    public class FileSpecItem
    {
        public string Ext { get; set; } // The file extension WITH a dot
        public string FileType { get; set; } // The type of the file
        public string ContentTemplate { get; set; } // The text the file will be created with

        public static List<FileSpecItem> ConstructFileSpecItems()
        {
            List<FileSpecItem> ret = new List<FileSpecItem>();
            string path = Assembly.GetAssembly(typeof(FileSpecItem)).Location;
            path = System.IO.Path.GetDirectoryName(path);
            path = System.IO.Path.Combine(path, "filespecs");
            if (!System.IO.Directory.Exists(path))
                return ret;
            foreach (string file in System.IO.Directory.EnumerateFiles(path))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                ret.Add(new FileSpecItem {
                    FileType = doc.DocumentElement.GetAttribute("name"),
                    Ext = doc.DocumentElement.GetAttribute("ext"),
                    ContentTemplate = doc.DocumentElement.InnerText
                });
            }
            return ret;
        }
    }

    /// <summary>
    /// Interaction logic for NewFileDlg.xaml
    /// </summary>
    public partial class NewFileDlg : ModernDialog
    {
        static int lastIndex = -1;
        Button createBtn;

        public string Path { get; set; }

        public NewFileDlg(string path)
        {
            InitializeComponent();
            dgFileTypes.ItemsSource = FileSpecItem.ConstructFileSpecItems();
            if (lastIndex == -1)
                dgFileTypes.SelectedIndex = 0;
            else
                dgFileTypes.SelectedIndex = lastIndex;
            txtFileName.Focus();
            NewFileDlg me = this;

            Buttons = new Button[] {
                createBtn = new Button {
                    Content = "Create",
                    Style = FindResource("StyledButton") as Style,
                    Command = new RelayCommand(o => {
                        FileSpecItem fi = dgFileTypes.SelectedItem as FileSpecItem;
                        string fileName = txtFileName.Text.Trim().Replace(fi.Ext, "");
                        lastIndex = dgFileTypes.SelectedIndex;
                        Path = System.IO.Path.Combine(path, fileName + fi.Ext);
                        System.IO.File.WriteAllText(Path,
                            fi.ContentTemplate.Replace("{FILENAME}", fileName));
                        DialogResult = true;
                        Close();
                    },
                    (p) => {
                        if (txtFileName.Text.Trim().Length != 0 && dgFileTypes.SelectedIndex != -1)
                            return true;
                        return false;
                    })
                },
                CancelButton
            };
        }
    }
}
