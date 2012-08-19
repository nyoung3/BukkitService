using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SslStream = ConnorsNetworkingSuite.SslStream;

namespace ConsoleClient {
    public class Connection {
        public delegate void ReadCallback(string message);
        public delegate bool ClientConfirmMessage(string message);
        public delegate void ClientMessage(string message);

        private static TcpClient client;
        private static SslStream stream;
        public static ReadCallback Callback { private get; set; }
        public static ClientConfirmMessage GetConfirmation { private get; set; }
        public static ClientMessage ClientAlert { private get; set; }
        public static string ServerIp;

        static Connection() {
            Callback = message => { };
        }

        public static async Task<bool> Connect(string host, int port, string user, string pass) {
            client = new TcpClient();
            await client.ConnectAsync(host, port);
            stream = new SslStream(client);
            try {
                await stream.AuthenticateAsClientAsync(host, (sender, certificate, chain, errors) => {
                    CertificateInfo.certificate = certificate.ToString(true);

                    if (errors == SslPolicyErrors.None) {
                        return true;
                    }
                    if (errors == SslPolicyErrors.RemoteCertificateNameMismatch) {
                        return GetConfirmation(string.Format("The name on the certificate ({0}) " +
                                                             "does not match the hostname you entered ({1})" +
                                                             "\r\nConnect anyways?",
                                                             GetCN(certificate.Subject), host));
                    }
                    return GetConfirmation("There is a problem with the certificate\r\nContinue anyways?");
                });
            } catch (System.Security.Authentication.AuthenticationException) {
                ClientAlert("There was an error authenticating with the server.");
                return false;
            }

            stream.Encoding = Encoding.ASCII;
            await stream.WriteAsync("Unicode");
            stream.Encoding = Encoding.Unicode;
            await stream.ReadAsync();
            await stream.WriteAsync(user + ':' + pass);
            var ures = stream.Read();
            if (ures != "LOGIN_SUCCESS") {
                ClientAlert(ures.Replace("ERR_", "").Replace('_', ' '));
                return false;
            }

            await stream.WriteAsync("#serverip");
            ServerIp = await stream.ReadAsync();

            new Thread(() => {
                string msg;
                while ((msg = stream.Read()) != null) {
                    Callback.BeginInvoke(msg, null, null);
                }
            }).Start();

            return true;
        }

        public static Task<bool> Write(string message) {
            return stream.WriteAsync(message);
        }

        public static Task<string> Read() {
            return stream.ReadAsync();
        }

        static string GetCN(string certSubject) {
            return certSubject.Split(',').Select(s => s.Trim()).First(s => s.Split('=')[0].Equals("CN")).Split('=')[1].Trim();
        }
    }
}
