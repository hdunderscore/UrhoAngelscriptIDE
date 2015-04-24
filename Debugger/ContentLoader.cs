using FirstFloor.ModernUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger
{
    class ContentLoader : DefaultContentLoader
    {
        /// <summary>
        /// Overriden implementation prevents multiplicitous construction of main UI pages
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected override object LoadContent(Uri uri)
        {
            //moveable to an interface? How to get at the instance?
            if (uri.OriginalString.EndsWith("IDEView.xaml"))
            {
                if (IDE.IDEView.inst() != null)
                    return IDE.IDEView.inst();
            } else if (uri.OriginalString.EndsWith("DebugScreen.xaml"))
            {
                if (Screens.DebugScreen.inst() != null)
                    return Screens.DebugScreen.inst();
            }
            return base.LoadContent(uri);
        }
    }
}
