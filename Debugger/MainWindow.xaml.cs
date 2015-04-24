﻿using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Debugger {
    /// <summary>
    /// Master program window
    /// </summary>
    public partial class MainWindow : ModernWindow {
        static MainWindow inst_;
        Timer errTimer;
        ErrorHandler errHandler;
        PluginManager pluginManager;

        public static MainWindow inst() { return inst_; }

        public MainWindow() {
            inst_ = this;
            errHandler = new ErrorHandler();
            this.ContentLoader = new ContentLoader();

            Net.DebugClient client = new Net.DebugClient(new Debugger.Debug.SessionData(""));
            pluginManager = new PluginManager("plugins");
            InitializeComponent();
            AppearanceManager.Current.ThemeSource = AppearanceManager.DarkThemeSource;
            AppearanceManager.Current.AccentColor = Colors.DarkOliveGreen;
            ContentSource = new Uri("Screens/LaunchScreen.xaml", UriKind.Relative);

            LinkNavigator.Commands.Add(new Uri("cmd://showPlugins", UriKind.Absolute), new RelayCommand(o => showPlugins()));
            errTimer = new Timer();
            errTimer.Enabled = true;
            errTimer.AutoReset = true;
            errTimer.Interval = 200;
            errTimer.Elapsed += errTimer_Elapsed;
            errTimer.Start();
        }

        void errTimer_Elapsed(object sender, ElapsedEventArgs e) {
            this.Dispatcher.BeginInvoke((Action)delegate() {
                checkErrs();
            });
        }

        void checkErrs(params object[] para) {
            if (errHandler.Check()) {
                string msg = errHandler.GetMessage();
                if (msg.Length > 0)
                    Dlg.ErrorDlg.Show(msg);
            }
        }

        void showPlugins()
        {
            Dlg.PluginDlg dlg = new Dlg.PluginDlg();
            dlg.ShowDialog();
        }
    }
}
