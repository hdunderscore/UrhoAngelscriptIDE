using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;
using System.Windows.Input;
using XMLEditor.Common;

namespace XMLEditor.ViewModels
{
    public class SelectedElementViewModel : BaseViewModel
    {
        private XmlNode currentNode;
        private ObservableCollection<ChildViewModel> children;
        public SelectedElementViewModel(XmlNode dataModel)
        {
            this.DataModel = dataModel;
            removeChildrenCommand = new RelayCommand((p) => { RemoveChildren(); });

            addAttributeCommand = new RelayCommand<XmlNodeType>(p => { AddElement(p); });
            Children = new ObservableCollection<ChildViewModel>();
            UpdateChildren();
        }

        private XmlNode dataModel;

        /// <summary>
        /// 
        /// </summary>
        ///        
        public XmlNode DataModel { get { return dataModel; } private set { dataModel = value; OnPropertyChanged("DataModel"); } }

        /// <summary>
        /// 
        /// </summary>
        public XmlNode CurrentNode
        {
            get { return currentNode; }
            set
            {
                currentNode = value;
                OnPropertyChanged("CurrentNode");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<ChildViewModel> Children
        {
            get { return children; }
            set
            {
                children = value;

                OnPropertyChanged("Children");
            }
        }


        /// <summary>
        /// To get the value from UI
        /// </summary>
        public Func<XmlNodeType, XmlNode> AddXmlNode { get; set; }

        #region Commands

        private ICommand removeChildrenCommand;

        public ICommand RemoveChildrenCommand
        {
            get { return removeChildrenCommand; }
        }

        private ICommand addAttributeCommand;

        public ICommand AddAttributeCommand
        {
            get { return addAttributeCommand; }
        }


        private void RemoveChildren()
        {
            foreach (var childVM in Children)
            {
                if (childVM.IsSelected && childVM.DataModel.NodeType == XmlNodeType.Attribute)
                {
                    this.DataModel.Attributes.Remove(childVM.DataModel as XmlAttribute);
                }
                else if (childVM.IsSelected && childVM.DataModel.NodeType == XmlNodeType.Text && this.DataModel.ParentNode != null)
                {
                    this.DataModel.ParentNode.RemoveChild(this.DataModel);
                }
            }

            UpdateChildren();
        }

        private void AddElement(XmlNodeType param)
        {
            XmlNode xmlNode = null;
            if (AddXmlNode != null)
            {
                xmlNode = AddXmlNode(param);
            }
            if (xmlNode == null)
            {
                return;
            }
            if (xmlNode.NodeType == XmlNodeType.Attribute)
            {
                DataModel.Attributes.Append(xmlNode as XmlAttribute);
            }
            else if (xmlNode.NodeType == XmlNodeType.Text)
            {
                DataModel.AppendChild(xmlNode as XmlText);
            }

            UpdateChildren();
        }

        #endregion


        private void UpdateChildren()
        {
            if (this.DataModel == null)
            {
                return;
            }
            OnPropertyChanged("DataModel");
            this.Children.Clear();
            if (this.DataModel.NodeType == XmlNodeType.Element && this.DataModel.Attributes != null)
            {
                foreach (XmlAttribute item in this.DataModel.Attributes)
                {
                    var childVM = new ChildViewModel(item);
                    Children.Add(childVM);
                }
            }

            else if (this.DataModel.NodeType == XmlNodeType.Text)
            {
                var childVM = new ChildViewModel(this.DataModel);
                Children.Add(childVM);
            }
        }
    }
}
