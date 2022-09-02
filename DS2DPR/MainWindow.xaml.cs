using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using forms = System.Windows.Forms;
using DiscordRPC;
using DiscordRPC.Logging;

namespace DS2DPR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool isConnected = false;
        public bool drp_on = false;
        private readonly forms.NotifyIcon _notifyicon;
        public DS2_hook hook = new DS2_hook();
        public DiscordRpcClient client;


        public MainWindow()
        {
            InitializeComponent();

            // initialise notiflications
            _notifyicon = new forms.NotifyIcon();
            _notifyicon.Icon = new System.Drawing.Icon("Resources/ds2logo.ico");
            _notifyicon.Text = "DS2 DPR";
            _notifyicon.Click += NotifyIcon_Click;
            _notifyicon.ContextMenuStrip = new forms.ContextMenuStrip();
            _notifyicon.ContextMenuStrip.Items.Add(new forms.ToolStripLabel("DS2 DPR", System.Drawing.Image.FromFile("Resources/clearlogo.png")));
            _notifyicon.ContextMenuStrip.Items.Add(new forms.ToolStripLabel($"Deaths: {hook.Death}"));
            _notifyicon.ContextMenuStrip.Items.Add(new forms.ToolStripLabel($"00:00 elapsed"));
            _notifyicon.ContextMenuStrip.Items.Add("Connect", null, connect_ds2_notif);
            _notifyicon.ContextMenuStrip.Items.Add("Exit", null, OnExitClick);
            _notifyicon.Visible = true;
        }

        // connect to ds2 tray button
        private void connect_ds2_notif(object? sender, EventArgs e)
        {
            connect_ds2_method();
        }

        // connect to ds2 app button
        private async void connect_ds2(object sender, RoutedEventArgs e)
        {
            connect_ds2_method();
        }

        // actual connect method
        private async void connect_ds2_method()
        {
            // starts hook
            hook.Start();
            await Task.Delay(150);
            var soulLvl = hook.slLvl;



            if (isConnected == false)
            {

                // if the sl level is 0, you are not in game. My brain is expanding.
                if (soulLvl != 0)
                {
                    var timer1 = new System.Timers.Timer(150);
                    DateTime _start;                                  // starts timer
                    _start = DateTime.Now;
                    timer1.Start();


                    // notiflication thingy
                    _notifyicon.ShowBalloonTip(3000, "Connected!", "Successfully connected to your game", forms.ToolTipIcon.Info);
                    connectbtn.Content = "Disconnect";
                    isConnected = true;
                    _notifyicon.ContextMenuStrip.Items[3].Text = "Disconnect";
                    while (isConnected == true)
                    {
                        TimeSpan duration = DateTime.Now - _start;
                        var deaths = hook.Death;
                        string time = duration.ToString(@"mm\:ss");

                        if (duration.TotalMinutes >= 60)
                        {
                            time = duration.ToString(@"hh\:mm\:ss");
                        }
                        else
                        {
                             time = duration.ToString(@"mm\:ss");
                        }


                        // updates textblocks
                        deathText.Text = $"Death #{deaths}";
                        Timer_label.Text = $"{time} elapsed";
                        update_dpr(deaths, time);

                        // updates death in tray
                        _notifyicon.ContextMenuStrip.Items[1].Text = $"Deaths: {hook.Death}";
                        _notifyicon.ContextMenuStrip.Items[2].Text = $"{time} elapsed";
                        _notifyicon.Text = $"DS2 DPR\nDeaths: {hook.Death}";

                        // stop throttling
                        await Task.Delay(3);
                    }
                }
                else
                {
                    // incase you cant connect
                    _notifyicon.ShowBalloonTip(3000, "Error", "Couldn't connected to your game", forms.ToolTipIcon.Error);
                    hook.Stop();
                }
            }
            else
            {
                // update notif tray
                // update button to reflect change
                // stop hook
                _notifyicon.Text = "DS2 DPR";
                _notifyicon.ContextMenuStrip.Items[3].Text = "Connect";
                _notifyicon.ShowBalloonTip(3000, "Disconnected", "Disconnected form your game", forms.ToolTipIcon.Info);

                _notifyicon.ContextMenuStrip.Items[1].Text = $"Deaths: 0";
                _notifyicon.ContextMenuStrip.Items[2].Text = $"00:00 elapsed";
                deathText.Text = "Death #0";
                Timer_label.Text = "00:00 elapsed";

                hook.Stop();
                isConnected = false;
                connectbtn.Content = "Connect";
            }
        }


        // closing method
        // closing in app and closing from tray
        private void OnExitClick(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            _notifyicon.Dispose();
            hook.Stop();
        }

        // closes 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _notifyicon.Dispose();
            hook.Stop();
        }

        // open app from tray
        private void NotifyIcon_Click(object? sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            if (this.Visibility == Visibility.Collapsed)
            {
                WindowState = WindowState.Minimized;
            }
        }

        // close app hide
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Collapsed;
        }

        // enable discord rich presence button
        private void enable_drp(object sender, RoutedEventArgs e)
        {
            enable_dpr_method();
        }

        private void enable_dpr_method()
        {
            if (drp_on == false)
            {
                drp_on = true;
                drp_btn.Content = "Disable";

                client = new DiscordRpcClient("<Client ID>");
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                client.OnReady += (sender, e) =>
                {
                    //System.Windows.MessageBox.Show($"Received Ready from user {e.User.Username}"); debug
                };
                client.OnPresenceUpdate += (sender, e) =>
                {
                    //System.Windows.MessageBox.Show($"Received Update! {e.Presence}");   debug
                };

                client.Initialize();
            }
            else
            {
                drp_on = false;
                client.Dispose();
                drp_btn.Content = "Enable";
            }
        }




        // update discord rich presence
        private void update_dpr(int death_count, string time)
        {
            try
            {
                client.SetPresence(new RichPresence()
                {
                    Details = $"Death #{death_count}",
                    State = $"{time} elapsed",
                    Assets = new Assets()
                    {
                        LargeImageKey = "large-image",
                        LargeImageText = "Dark Souls II: SoTFS",

                    }
                });
            }
            catch
            {

            }

        }
    }
}