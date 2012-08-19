using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConnorsNetworkingSuite;
using SslStream = ConnorsNetworkingSuite.SslStream;

namespace BukkitServiceWinforms {
    public partial class LoginForm : Form {
        internal static NetStream Stream;

        public LoginForm() {
            InitializeComponent();
            userbox.Text = Properties.Settings.Default.username;
            passbox.Text = Properties.Settings.Default.password;
            hostbox.Text = Properties.Settings.Default.hostname;
            rempass.Checked = Properties.Settings.Default.remeberpass;
        }

        private void LoginFormLoad(object sender, EventArgs e) {
            loginToolTips.SetToolTip(hostbox, "Server to connect to. Usage: host:port");
            loginToolTips.SetToolTip(userbox, "Username for the server console.");
            loginToolTips.SetToolTip(passbox, "Password for the server console.");
        }

        private async void ConnectClick(object sender, EventArgs e) {
            hostbox.Enabled = false;
            userbox.Enabled = false;
            passbox.Enabled = false;
            var hsplit = hostbox.Text.Split(':');
            var host = hsplit[0];
            if (string.IsNullOrWhiteSpace(host)) {
                Invoke(new Action(() => MessageBox.Show(this, "Please enter a host")));
                hostbox.Enabled = true;
                userbox.Enabled = true;
                passbox.Enabled = true;
                return;
            }
            var port = 3000;
            if (hsplit.Length > 1) {
                if (!int.TryParse(hsplit[1], out port)) {
                    Invoke(new Action(() => MessageBox.Show(this, "Port must be a number")));
                    hostbox.Enabled = true;
                    userbox.Enabled = true;
                    passbox.Enabled = true;
                    return;
                }
            }
            SslStream ssl;
            try {
                var tcp = new TcpClient();
                await tcp.ConnectAsync(host, port);
                ssl = new SslStream(tcp);
            } catch {
                Invoke(new Action(() => MessageBox.Show(this, "Error connecting")));
                hostbox.Enabled = true;
                userbox.Enabled = true;
                passbox.Enabled = true;
                return;
            }
            try {
                await ssl.AuthenticateAsClientAsync(host, ValidationCallback);
            } catch {
                Invoke(new Action(() => MessageBox.Show(this, "Error securing connection")));
                hostbox.Enabled = true;
                userbox.Enabled = true;
                passbox.Enabled = true;
                return;
            }
            Stream = ssl;

            Stream.Encoding = Encoding.UTF8;
            Stream.Write("utf-8");
            Stream.Read();
            Stream.Write(userbox.Text + ":" + passbox.Text);
            var res = Stream.Read();
            if (res.StartsWith("ERR")) {
                if (res == "ERR_AUTH_INVALID_CREDENTIALS") {
                    Invoke(new Action(() => MessageBox.Show(this, "Bad username or password")));
                } else if (res == "ERR_ACCOUNT_NOT_AUTHORIZED") {
                    Invoke(new Action(() => MessageBox.Show(this, "Account not authorized")));
                } else {
                    Invoke(new Action(() => MessageBox.Show(this, "An unkown error occured during authentication")));
                }
                hostbox.Enabled = true;
                userbox.Enabled = true;
                passbox.Enabled = true;
                return;
            }
            Properties.Settings.Default.hostname = hostbox.Text;
            Properties.Settings.Default.username = userbox.Text;

#pragma warning disable 665
            if (Properties.Settings.Default.remeberpass = rempass.Checked) {
#pragma warning restore 665
                Properties.Settings.Default.password = passbox.Text;
            } else {
                Properties.Settings.Default.password = "";
            }

            Properties.Settings.Default.Save();

            Hide();
            new ConsoleForm().ShowDialog();
            Close();
        }

        private bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch) {
                var d = DialogResult.No;
                Invoke(
                    new Action(() => d = MessageBox.Show(this, string.Format("The name on the security certificate ({0}) " +
                                                                         "did not match the hostname entered ({1})\r\nProceed anyways?",
                                                                         GetCN(certificate.Subject),
                                                                         hostbox.Text.Split(':')[0]),
                                                     "Bukkit Service Client", MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Exclamation)));
                return d == DialogResult.Yes;
            }
            var dr = DialogResult.No;
            Invoke(
                new Action(() => dr = MessageBox.Show(this, "There was an error validating the certificate. Proceed?",
                                                      "Bukkit Service Client", MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Exclamation)));
            return dr == DialogResult.Yes;
        }
        static string GetCN(string certSubject) {
            return certSubject.Split(',').Select(s => s.Trim()).First(s => s.Split('=')[0].Equals("CN")).Split('=')[1].Trim();
        }
    }
}
