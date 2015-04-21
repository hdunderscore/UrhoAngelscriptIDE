using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for AudioPanel.xaml
    /// </summary>
    public partial class AudioPanel : UserControl
    {
        MediaPlayer player;
        public AudioPanel(string file)
        {
            InitializeComponent();
            player = new MediaPlayer();
            player.Open(new Uri(file));
            lblPosition.Content = String.Format("{0} / {1}", player.Position.ToString(@"mm\:ss"), player.NaturalDuration.ToString());
            slidePosition.Value = 0;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;
            timer.Start();
            if (player.NaturalDuration.HasTimeSpan)
                slidePosition.Maximum = player.NaturalDuration.TimeSpan.Ticks;
        }

        bool ignoreSlideChange = false;
        void timer_Tick(object sender, EventArgs e)
        {
            if (player.Source != null)
                lblPosition.Content = String.Format("{0} / {1}", player.Position.ToString(@"mm\:ss\.ff"), player.NaturalDuration.ToString());
            else
                lblPosition.Content = "No file selected...";
            ignoreSlideChange = true;
            slidePosition.Value = (double)player.Position.Ticks;
            if (player.NaturalDuration.HasTimeSpan)
                slidePosition.Maximum = player.NaturalDuration.TimeSpan.Ticks;
            ignoreSlideChange = false;

            if (player.NaturalDuration.HasTimeSpan)
            {
                if (player.NaturalDuration.TimeSpan.Ticks == player.Position.Ticks)
                {
                    player.Stop();
                    player.Position = new TimeSpan(0);
                    btnReset.IsEnabled = false;
                    btnPause.IsEnabled = false;
                    btnPlay.IsEnabled = true;
                }
            }
        }

        private void slidePosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!ignoreSlideChange)
            {
                if (player.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan ts = new TimeSpan((long)slidePosition.Value);
                    player.Position = ts;
                }
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.IsEnabled = false;
            btnPause.IsEnabled = true;
            btnReset.IsEnabled = true;
            player.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.IsEnabled = true;
            btnPause.IsEnabled = false;
            player.Pause();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.IsEnabled = true;
            btnPause.IsEnabled = false;
            btnReset.IsEnabled = false;
            player.Stop();
        }
    }
}
