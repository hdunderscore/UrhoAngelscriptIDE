using FirstFloor.ModernUI.Windows.Controls;
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

namespace Debugger.Dlg
{
    /// <summary>
    /// Interaction logic for DebugSettingsDlg.xaml
    /// </summary>
    public partial class DebugSettingsDlg : ModernDialog
    {
        public DebugSettingsDlg()
        {
            InitializeComponent();
            chkPlayInterruptSound.DataContext = Debug.SessionData.inst().Settings;
            chkUseIDECode.DataContext = Debug.SessionData.inst().Settings;
            chkPlayInterruptSound.ToolTip = "When the debugger halts the attached program it will play a beep sound";
            chkUseIDECode.ToolTip = "Will use the intellisense information from the IDE mode, requires an open IDE project";
        }
    }
}
