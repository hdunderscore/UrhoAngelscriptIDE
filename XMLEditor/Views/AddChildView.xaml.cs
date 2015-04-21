using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using XMLEditor.Common;

namespace XMLEditor.Views
{
    /// <summary>
    /// Interaction logic for AddChild.xaml
    /// </summary>
    public partial class AddChildView : Window
    {

        private XmlNode newNode;
        private XmlDocument document;




        public XmlNodeType DisplayMode
        {
            get { return (XmlNodeType)GetValue(DisplayModeProperty); }

            set
            {
                SetValue(DisplayModeProperty, value);
                if (DisplayMode == XmlNodeType.Attribute)
                {
                    radioButtonAttribute.IsChecked = true;
                }
                else if (DisplayMode == XmlNodeType.Element)
                {
                    radioButtonElement.IsChecked = true;
                }
                else if (DisplayMode == XmlNodeType.Text)
                {
                    radioButtonText.IsChecked = true;
                }
            }
        }

        // Using a DependencyProperty as the backing store for DisplayMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(XmlNodeType), typeof(AddChildView), new UIPropertyMetadata(XmlNodeType.Attribute));



        public XmlNode NewNode
        {
            get { return newNode; }

        }

        public AddChildView(XmlDocument document, XmlNodeType displayMode)
        {
            InitializeComponent();
            this.document = document;
            if (displayMode == XmlNodeType.None)
            {
                displayMode = XmlNodeType.Attribute;
            }
            this.DisplayMode = displayMode;
        }



        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (radioButtonText.IsChecked.HasValue && radioButtonText.IsChecked.Value)
            {
                XmlText text = document.CreateTextNode(this.nameText.Text);
                this.newNode = text;

            }
            else if (radioButtonAttribute.IsChecked.HasValue && radioButtonAttribute.IsChecked.Value)
            {
                XmlAttribute attribute = document.CreateAttribute(this.prefixText.Text, this.nameText.Text, this.namespaceText.Text);
                attribute.Value = this.valueText.Text;
                this.newNode = attribute;
            }

            else if (radioButtonElement.IsChecked.HasValue && radioButtonElement.IsChecked.Value)
            {
                XmlElement element = document.CreateElement(this.prefixText.Text, this.nameText.Text, this.namespaceText.Text);
                this.newNode = element;
            }
            this.DialogResult = true;
            this.Close();

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            newNode = null;
            this.DialogResult = false;
            this.Close();

        }
    }
}
