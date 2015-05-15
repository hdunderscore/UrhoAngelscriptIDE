using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.IDE.Intellisense.Sources
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IntelSourceDescriptor : Attribute
    {
        public string Ext { get; set; }
    }
}
