using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
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
using ConnorsNetworkingSuite;
using SslStream = ConnorsNetworkingSuite.SslStream;

namespace BukkitServiceOnlineApp {
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 {

        public static NetStream Stream;

        public Page1() {
            InitializeComponent();
        }

        private void PageLoaded1(object sender, RoutedEventArgs e) {

        }

        private void ButtonClick1(object sender, RoutedEventArgs e) {
            var hsplit = hostbox.Text.Split(':');
            if (hsplit.Length < 2) {
                MessageBox.Show("Please specify the host in the form of address:port",
                    "Bukkit Service Client", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int port;
            if (!int.TryParse(hsplit[1], out port)) {
                MessageBox.Show("Port must be a number",
                    "Bukkit Service Client", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var tcp = new TcpClient(hsplit[0], port);
            var ssl = new SslStream(tcp);
            ssl.AuthenticateAsClient
                (hsplit[0],
                 delegate(object o, X509Certificate certificate, X509Chain chain,
                          SslPolicyErrors errors) {
                     if (errors == SslPolicyErrors.None) {
                         return true;
                     }

                     if (errors == SslPolicyErrors.RemoteCertificateNameMismatch) {
                         return MessageBox.Show("The name on the remote certificate (" +
                             GetCN(certificate.Subject) + ") does not match the host entered (" +
                             hsplit[0] + ")\r\nWould you like to connect anyways?",
                             "Bukkit Service Client", MessageBoxButton.YesNo, MessageBoxImage.Exclamation)
                             == MessageBoxResult.Yes;
                     }

                     return MessageBox.Show("An unknown error has occured validating the certificate " +
                         "(Does your machine not recognize the root CA?)\r\n" +
                         "Would you like to proceed anyways?",
                         "Bukkit Service Client", MessageBoxButton.YesNo, MessageBoxImage.Exclamation)
                         == MessageBoxResult.Yes;
                 });
            if (!ssl.Write(userbox.Text + ":" + passbox.Password)) {
                LostConnection();
                return;
            }

            var cred_result = ssl.Read();
            if (cred_result == null) {
                LostConnection();
                return;
            }
            if (cred_result.StartsWith("ERR")) {
                ssl.Close();
                if (cred_result == "ERR_AUTH_INVALID_CREDENTIALS") {
                    MessageBox.Show("Incorrect username or password");
                    return;
                }
                if (cred_result == "ERR_ACCOUNT_NOT_AUTHORIZED") {
                    MessageBox.Show("This account is not authorized to connect");
                    return;
                }
                MessageBox.Show("An unknown error has occured during authentication");
                return;
            }
            if (cred_result != "LOGIN_SUCCESS") {
                MessageBox.Show("An unknown error has occured during authentication");
                return;
            }

            Debug.Assert(NavigationService != null, "NavigationService != null");
            NavigationService.Navigate(new ConsolePage());
        }

        static string GetCN(string certSubject) {
            return certSubject.Split(',').Select(s => s.Trim()).First(s => s.Split('=')[0].Equals("CN")).Split('=')[1].Trim();
        }

        static void LostConnection() {
            MessageBox.Show("Lost connection to the server");
        }
    }
}
