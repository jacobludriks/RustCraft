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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace RustCraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        //API-declaration
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void updateTotal(object sender, TextChangedEventArgs e)
        {
            //Get originating textbox
            System.Windows.Controls.TextBox txtbox = (System.Windows.Controls.TextBox)sender;

            //Get value from textbox
            int value;
            if (int.TryParse(txtbox.Text, out value) == false)
            {
                value = 0;
            }

            //Multiply by crafting time
            switch (txtbox.Name.ToString())
            {
                case "txtGunpowder":
                    value = value * 10;
                    break;
                case "txtExplosives":
                    value = value * 10;
                    break;
                case "txt556Ammo":
                    value = value * 10;
                    break;
                case "txtLGF":
                    value = value * 5;
                    break;
                case "txtRockets":
                    value = value * 10;
                    break;
                case "txtC4":
                    value = value * 30;
                    break;
                case "txtGuns":
                    value = value * 180;
                    break;
                case "txtArmor":
                    value = value * 60;
                    break;
                case "txtClothes":
                    value = value * 30;
                    break;
                case "txtHESW":
                    value = value * 180;
                    break;
                default:
                    break;
            }

            //Put value in the "Total" textbox
            string totaltextbox = txtbox.Name.ToString() + "Total";
            System.Windows.Controls.TextBox Total = (System.Windows.Controls.TextBox)this.FindName(totaltextbox);
            TimeSpan crafttime = TimeSpan.FromSeconds(value);
            Total.Text = crafttime.ToString();

            //Calculate total and shutdown time
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            //Get values
            int gunpowder, explosives, ammo, lgf, rockets, c4, guns, armor, clothes, hesw;
            if (int.TryParse(txtGunpowder.Text, out gunpowder) == false)
            {
                gunpowder = 0;
            }
            if (int.TryParse(txtExplosives.Text, out explosives) == false)
            {
                explosives = 0;
            }
            if (int.TryParse(txt556Ammo.Text, out ammo) == false)
            {
                ammo = 0;
            }
            if (int.TryParse(txtLGF.Text, out lgf) == false)
            {
                lgf = 0;
            }
            if (int.TryParse(txtRockets.Text, out rockets) == false)
            {
                rockets = 0;
            }
            if (int.TryParse(txtC4.Text, out c4) == false)
            {
                c4 = 0;
            }
            if (int.TryParse(txtGuns.Text, out guns) == false)
            {
                guns = 0;
            }
            if (int.TryParse(txtArmor.Text, out armor) == false)
            {
                armor = 0;
            }
            if (int.TryParse(txtClothes.Text, out clothes) == false)
            {
                clothes = 0;
            }
            if (int.TryParse(txtHESW.Text, out hesw) == false)
            {
                hesw = 0;
            }
            gunpowder = gunpowder * 10;
            explosives = explosives * 10;
            ammo = ammo * 10;
            lgf = lgf * 5;
            rockets = rockets * 10;
            c4 = c4 * 30;
            guns = guns * 180;
            armor = armor * 60;
            clothes = clothes * 30;
            hesw = hesw * 180;


            //Get subtotal and put in subtotal field
            int subtotal = gunpowder + explosives + ammo + lgf + rockets + c4 + guns + armor + clothes + hesw;
            TimeSpan crafttime = TimeSpan.FromSeconds(subtotal);
            txtSubtotal.Text = crafttime.ToString(@"hh\:mm\:ss");

            if (subtotal > 0)
            {
                //Get Rust shutdown time and put in field
                var timenow = DateTime.Now;
                var shutdowntime = timenow.AddSeconds(subtotal);

                txtShutdown.Text = shutdowntime.ToString("hh:mm:ss tt");

                btnToggle.IsEnabled = true;
            }
            else
            {
                txtShutdown.Text = "";
                if (btnToggle.IsEnabled)
                {
                    btnToggle.IsEnabled = false;
                }
            }
        }

        private System.Threading.Timer timer;
        private System.Timers.Timer tSendKey;
        private bool blShutdown = false;

        //Start timer to set focus and send keys
        private void StartSendKey()
        {
            if (tSendKey.Enabled  == false)
            {
                tSendKey = new System.Timers.Timer(TimeSpan.FromMinutes(60).TotalMilliseconds);
                tSendKey.Elapsed += SendKeyHandler;
                tSendKey.AutoReset = true;
                tSendKey.Enabled = true;
            }
        }
        
        //Stop the timer
        private void StopSendKey()
        {
            if(tSendKey.Enabled == true)
            {
                tSendKey.Stop();
                tSendKey.Dispose();
            }

        }

        private void SendKeyHandler(Object source, ElapsedEventArgs e)
        {
            SetFocusSendKeys("6");
            NewLogEntry("Auto Eating food in Slot 6.");
        }

        //This called when the SendKey timer ticks
        private void SetFocusSendKeys(string key)
        {
            if (blShutdown)
            {
                string strProcessName;
                strProcessName = "RustClient";
                //Find the Rust Client
                Process[] arrProcesses = Process.GetProcessesByName(strProcessName);
                if (arrProcesses.Length > 0)
                {
                    //Use the very first process (if there is many of them)
                    IntPtr ipHwnd = arrProcesses[0].MainWindowHandle;
                    //Set focus to the window
                    bool fSuccess = SetForegroundWindow(ipHwnd);
                    //Send the 6 key to eat food
                    SendKeys.SendWait(key);
                }
            }  
        }

        private void SetUpTimer(TimeSpan alertTime)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                //Time has already passed
                return;
            }
            this.timer = new System.Threading.Timer(x =>
            {
                this.ShutdownRust();
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private void ShutdownRust()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (chkCampfire.IsEnabled)
                {
                    //Send E to turn Campfire off
                    SetFocusSendKeys("e");
                    NewLogEntry("Turning off Campfire");
                }
            }));

            //Wait 5 seconds
            Thread.Sleep(5000);

                //Find processes with the name RustClient
                foreach (Process proc in Process.GetProcessesByName("RustClient"))
                    {
                    //Kill each client, even though there should only be one
                    proc.Kill();
                    }

            //Stop the Sendkey timer
            StopSendKey();

            this.Dispatcher.Invoke((Action)(() =>
                {

                        
                //Reset all text fields and update totals
                    txtGunpowder.Text = "";
                    txtExplosives.Text = "";
                    txt556Ammo.Text = "";
                    txtLGF.Text = "";
                    txtRockets.Text = "";
                    txtC4.Text = "";
                    txtGuns.Text = "";
                    txtArmor.Text = "";
                    txtClothes.Text = "";
                    txtHESW.Text = "";
                    CalculateTotal();

                //Change timer to never resume and change button text
                    this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                    this.timer.Dispose();
                    blShutdown = false;
                    btnToggle.Content = "Enable Rust Shutdown";

                //Create new log item and insert to listbox
                DateTime date = DateTime.Now;
                    NewLogEntry("Rust shut down at " + date.ToString("hh:mm:ss tt"));
                }));
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            //Calculate totals
            CalculateTotal();

            //Check if timer is already enabled
            if (blShutdown)
            {
                //Change timer to never resume and change button text
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                btnToggle.Content = "Enable Rust Shutdown";
                blShutdown = false;

                //Create new log item and insert to listbox
                NewLogEntry("Disabled Rust Shutdown");
            }
            else
            {
                //Set new timer
                DateTime date = DateTime.MinValue;
                if (DateTime.TryParse(txtShutdown.Text, out date))
                {
                    TimeSpan ts = new TimeSpan(date.Hour, date.Minute, date.Second);
                    SetUpTimer(ts);
                    btnToggle.Content = "Disable Rust Shutdown";
                    blShutdown = true;

                    //Check if campfire mode is enabled, and turn on campfire.
                    if(chkCampfire.IsChecked == true)
                    {
                        SetFocusSendKeys("e");
                        NewLogEntry("Turning on Campfire.");
                    }

                    Thread.Sleep(1000);

                    //Check if Auto Eat is enabled, and send 6 to rust.
                    if (tSendKey.Enabled == true)
                    {
                            SetFocusSendKeys("6");
                            NewLogEntry("Selecting food in slot 6.");
                    }
                    //Create new log item and insert to listbox
                    NewLogEntry("Enabled Rust Shutdown for " + date.ToString("hh:mm:ss tt"));
                }
                else
                {
                    System.Windows.MessageBox.Show("Unable to set timer");
                }
            }
        }

        private void NewLogEntry(string msg)
        {
            //Create new log item and insert to listbox
            this.Dispatcher.Invoke((Action)(() =>
            {
                var timenow = DateTime.Now.ToString("hh:mm:ss tt");
                ListBoxItem itm = new ListBoxItem();
                itm.Content = timenow + " - " + msg;
                lbLog.Items.Insert(0, itm);
            }));
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Capture Ctrl + Shift + F8
            if (e.Key == Key.F8 && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                //start or stop the timer.
                if(tSendKey.Enabled == false)
                {
                    StartSendKey();
                    NewLogEntry("Auto Eat Enabled.");

                    //If the shutdown timer has already started set rust focus and press 6.
                    if (blShutdown)
                    {
                        SetFocusSendKeys("6");
                    }
                }
                else
                {

                }

            }
        }

        private void chkCampfire_Checked(object sender, RoutedEventArgs e)
        {
            if(blShutdown)
            {
                SetFocusSendKeys("e");
                NewLogEntry("Turning on Campfire.");
            }
        }

        private void chkAutoEat_Checked(object sender, RoutedEventArgs e)
        {
            if (tSendKey.Enabled == false)
            {
                StartSendKey();
                NewLogEntry("Auto Eat Enabled.");

                //If the shutdown timer has already started set rust focus and press 6.
                if (blShutdown)
                {
                    SetFocusSendKeys("6");
                }
            }
        }

        private void chkAutoEat_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tSendKey.Enabled == true)
            {
                StopSendKey();
                NewLogEntry("Auto Eat Disabled.");
            }
        }
    }
}
