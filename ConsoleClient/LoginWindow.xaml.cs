using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace ConsoleClient {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {
        public LoginWindow() {
            InitializeComponent();
            Connection.GetConfirmation = message => {
                var mbr = MessageBoxResult.No;
                Dispatcher.Invoke(
                    () =>
                    mbr = MessageBox.Show(message, "Bukkit Console",
                    MessageBoxButton.YesNo, MessageBoxImage.Question));
                return mbr == MessageBoxResult.Yes;
            };
            Connection.ClientAlert = message => MessageBox.Show(message, "Bukkit Console",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Hostbox.Text = Properties.Settings.Default.Hostname;
            Userbox.Text = Properties.Settings.Default.Username;

            if ((bool) (rempass.IsChecked = Properties.Settings.Default.RememberPass)) {
                Passbox.Password = Encoding.Unicode.GetString(ProtectedData.Unprotect(
                    Convert.FromBase64String(Properties.Settings.Default.Password), 
                    new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes("Send it to the moon!")), 
                    DataProtectionScope.CurrentUser));
            }
        }

        private async void ConnectClick(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(Hostbox.Text)) {
                MessageBox.Show("Host cannot be blank");
                return;
            }
            if (string.IsNullOrWhiteSpace(Userbox.Text)) {
                MessageBox.Show("User cannot be blank");
                return;
            }
            if (string.IsNullOrWhiteSpace(Passbox.Password)) {
                MessageBox.Show("Pass cannot be blank");
                return;
            }

            IsEnabled = false;

            Properties.Settings.Default.Hostname = Hostbox.Text;
            Properties.Settings.Default.Username = Userbox.Text;
#pragma warning disable 665
            if (Properties.Settings.Default.RememberPass = (rempass.IsChecked.HasValue && rempass.IsChecked.Value)) {
                Properties.Settings.Default.Password = Convert.ToBase64String(
                    ProtectedData.Protect(Encoding.Unicode.GetBytes(Passbox.Password), 
                    new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes("Send it to the moon!")), 
                    DataProtectionScope.CurrentUser));
            } else {
                Properties.Settings.Default.Password = "";
            }
#pragma warning restore 665
            Properties.Settings.Default.Save();

            var hsplit = Hostbox.Text.Trim().Split(':');
            var port = 3000;
            if (hsplit.Length > 1) {
                if (!int.TryParse(hsplit[1], out port)) {
                    MessageBox.Show("The port must be a number!", "Bukkit Console",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                    IsEnabled = true;
                }
            }

            try {
                if (!await Connection.Connect(hsplit[0], port, Userbox.Text, Passbox.Password)) {
                    IsEnabled = true;
                    return;
                }
            } catch {
                MessageBox.Show("There was an error connecting to the server", "Bukkit Console",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                IsEnabled = true;
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
