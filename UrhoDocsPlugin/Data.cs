using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrhoDocsPlugin
{
    public class Data
    {
        public API.APIDocumentation APIDocumentation { get; set; }

        static Data inst_;
        public static Data inst()
        {
            if (inst_ == null)
                inst_ = new Data();
            return inst_;
        }

        Data()
        {
            APIDocumentation = new API.APIDocumentation();
        }
    }
}
