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

        private void txtItems_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            updateTotal(sender);
        }

        private void updateTotal(object sender)
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
                default:
                    break;
            }

            //Put value in the "Total" textbox
            string totaltextbox = txtbox.Name.ToString() + "Total";
            TextBox Total = (TextBox)this.FindName(totaltextbox);
            Total.Text = value.ToString();

            //Calculate total and shutdown time
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            //Get values
            int gunpowder, explosives, ammo, lgf;
            if (int.TryParse(txtGunpowderTotal.Text, out gunpowder) == false)
            {
                gunpowder = 0;
            }
            if (int.TryParse(txtExplosivesTotal.Text, out explosives) == false)
            {
                explosives = 0;
            }
            if (int.TryParse(txt556AmmoTotal.Text, out ammo) == false)
            {
                ammo = 0;
            }
            if (int.TryParse(txtLGFTotal.Text, out lgf) == false)
            {
                lgf = 0;
            }

            //Get subtotal and put in subtotal field
            int subtotal = gunpowder + explosives + ammo + lgf;
            txtSubtotal.Text = subtotal.ToString();

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
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            //Check if timer is already enabled
            if (enabled)
            {
                //Change timer to never resume and change button text
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                btnToggle.Content = "Enable Rust Shutdown";
                enabled = false;
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
                }
                else
                {
                    MessageBox.Show("Unable to set timer");
                }
            }
        }
    }
}
