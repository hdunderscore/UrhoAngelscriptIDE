using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XMLEditor.ViewModels
{
    public class ChildViewModel : BaseViewModel
    {
        public XmlNode DataModel { get; private set; }
        public ChildViewModel(XmlNode childNode)
        {
            this.DataModel = childNode;
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

    }
}
