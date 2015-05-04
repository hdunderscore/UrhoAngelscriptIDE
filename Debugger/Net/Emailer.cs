using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger.Net
{
    public static class Emailer
    {
        public static void MailTo(Exception ex)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = String.Format("mailto:j.sandusky@hotmail.com?subject=Bug Report&body={0}{1}{2}", ex.Message, ex.StackTrace, ex.Source);
            proc.Start();
        }

        public static void MailTo(string msg)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = String.Format("mailto:j.sandusky@hotmail.com?subject=Bug Report&body={0}", msg);
            proc.Start();
        }
    }
}
