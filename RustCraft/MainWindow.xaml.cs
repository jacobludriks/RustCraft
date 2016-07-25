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

        private void updateTotal(object sender, TextChangedEventArgs e)
        {
            //Get originating textbox
            TextBox txtbox = (TextBox)sender;

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
                default:
                    break;
            }

            //Put value in the "Total" textbox
            string totaltextbox = txtbox.Name.ToString() + "Total";
            TextBox Total = (TextBox)this.FindName(totaltextbox);
            TimeSpan crafttime = TimeSpan.FromSeconds(value);
            Total.Text = crafttime.ToString();

            //Calculate total and shutdown time
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            //Get values
            int gunpowder, explosives, ammo, lgf, rockets, c4, guns, armor, clothes;
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
            gunpowder = gunpowder * 10;
            explosives = explosives * 10;
            ammo = ammo * 10;
            lgf = lgf * 10;
            rockets = rockets * 10;
            c4 = c4 * 30;
            guns = guns * 180;
            armor = armor * 60;
            clothes = clothes * 30;

            //Get subtotal and put in subtotal field
            int subtotal = gunpowder + explosives + ammo + lgf + rockets + c4 + guns + armor + clothes;
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

        private Timer timer;
        private bool enabled = false;

        private void SetUpTimer(TimeSpan alertTime)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                //Time has already passed
                return;
            }
            this.timer = new Timer(x =>
            {
                this.ShutdownRust();
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private void ShutdownRust()
        {
            try
            {
                //Find processes with the name RustClient
                foreach (Process proc in Process.GetProcessesByName("RustClient"))
                {
                    //Kill each client, even though there should only be one
                    proc.Kill();
                }

                //Do actions on the main thread instead of the timer background thread
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
                    CalculateTotal();

                    //Change timer to never resume and change button text
                    this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                    enabled = false;
                    btnToggle.Content = "Enable Rust Shutdown";

                    //Create new log item and insert to listbox
                    DateTime date = DateTime.Now;
                    NewLogEntry("Rust shut down at " + date.ToString("hh:mm:ss tt"));
                }));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            //Calculate totals
            CalculateTotal();

            //Check if timer is already enabled
            if (enabled)
            {
                //Change timer to never resume and change button text
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                btnToggle.Content = "Enable Rust Shutdown";
                enabled = false;

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
                    enabled = true;

                    //Create new log item and insert to listbox
                    NewLogEntry("Enabled Rust Shutdown for " + date.ToString("hh:mm:ss tt"));
                }
                else
                {
                    MessageBox.Show("Unable to set timer");
                }
            }
        }

        private void NewLogEntry(string msg)
        {
            //Create new log item and insert to listbox
            var timenow = DateTime.Now.ToString("hh:mm:ss tt");
            ListBoxItem itm = new ListBoxItem();
            itm.Content = timenow + " - " + msg;
            lbLog.Items.Insert(0, itm);
        }
    }
}
