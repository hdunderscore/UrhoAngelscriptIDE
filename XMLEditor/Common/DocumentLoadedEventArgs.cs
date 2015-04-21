using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XMLEditor.Common
{
    public class DocumentLoadedEventArgs : EventArgs
    {
        public XmlDocument Document { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }

    }

    public class SearchRequestedEventArgs : EventArgs
    {
        public string XPath { get; set; }

    }

    public class SaveAsEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string FileName { get; set; }

    }
}
