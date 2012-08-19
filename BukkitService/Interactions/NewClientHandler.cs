using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using BukkitServiceAPI;
using BukkitServiceAPI.Authentication;
using ConnorsNetworkingSuite;
using System.Linq;

namespace BukkitService.Interactions {
    internal static class NewClientHandler {
        private static X509Certificate cert;
        private static X509Certificate Certificate {
            get {
                return cert ?? (cert = new X509Certificate2(
                    Main.config["certificate-path"],
                    Main.config["certificate-pass"],
                    X509KeyStorageFlags.MachineKeySet));
            }
        }

        internal static readonly Config Userconf = new Config(
            Path.Combine(
                Util.StorageDir, "basicuserstore.conf"));

        internal static void HandleClient(NetStream stream, IPEndPoint ip) {
            try {
                var ssl = new SslStream(stream);
                ssl.AuthenticateAsServer(Certificate);
                stream = ssl;
            } catch (Exception e) {
                stream.Write("ERR_SSL\r\n");
                stream.Write(e.ToString());
                return;
            }

            stream.Encoding = Encoding.ASCII;
            var encdodingstring = stream.Read();
            int codepage;
            if (int.TryParse(encdodingstring, out codepage)) {
                try {
                    var cpenc = Encoding.GetEncoding(codepage);
                    stream.Encoding = cpenc;
                    stream.Write("success");
                } catch {
                    stream.Write("ERR_ENCODING_NOT_FOUND");
                    stream.Close();
                    return;
                }
            } else {
                try {
                    var encoding = Encoding.GetEncoding(encdodingstring);
                    stream.Encoding = encoding;
                    stream.Write("success");
                } catch {
                    stream.Write("ERR_ENCODING_NOT_FOUND");
                    stream.Close();
                    return;
                }
            }

            try {
                var cred = Authenticate(stream);
                if (!cred.Successful) {
                    stream.Close();
                    return;
                }
                stream.Write("LOGIN_SUCCESS");
                ClientLoop.BeginClient(new Client(stream, cred.Username, cred.SecurityLevel, ip));
            } catch (Exception ex) {
                stream.Write("ERR\r\n");
                stream.Write(ex.ToString());
            }
        }

        internal static void HandleClientPlaintext(NetStream stream, IPEndPoint ip) {
            stream.Encoding = Encoding.ASCII;
            var encdodingstring = stream.Read();
            int codepage;
            if (int.TryParse(encdodingstring, out codepage)) {
                try {
                    var cpenc = Encoding.GetEncoding(codepage);
                    stream.Encoding = cpenc;
                    stream.Write("success");
                } catch {
                    stream.Write("ERR_ENCODING_NOT_FOUND");
                    stream.Close();
                    return;
                }
            } else {
                try {
                    var encoding = Encoding.GetEncoding(encdodingstring);
                    stream.Encoding = encoding;
                    stream.Write("success");
                } catch {
                    stream.Write("ERR_ENCODING_NOT_FOUND");
                    stream.Close();
                    return;
                }
            }

            try {
                var cred = Authenticate(stream);
                if (!cred.Successful) {
                    stream.Close();
                    return;
                }
                stream.Write("LOGIN_SUCCESS");
                ClientLoop.BeginClient(new Client(stream, cred.Username, cred.SecurityLevel, ip));
            } catch (Exception ex) {
                stream.Write("ERR\r\n");
                stream.Write(ex.ToString());
            }
        }

        private static NewClientCredentials Authenticate(NetStream stream) {
            var ncc = CustomAuthentication.CustomAuth(stream);
            return ncc ?? ConfigAuth(stream);
        }

        private static NewClientCredentials ConfigAuth(NetStream stream) {
            var ncc = new NewClientCredentials { Successful = false, SecurityLevel = -1, Username = null };
            var userpass = stream.Read();
            var ups = userpass.Split(new[] { ':' }, 2);
            if (ups.Length < 2) {
                stream.Write("ERR_AUTH_INVALID_USER_FORMAT");
                stream.Close();
                return ncc;
            }
            if (Userconf["user." + ups[0] + ".enabled"] != "true") {
                stream.Write("ERR_AUTH_INVALID_CREDENTIALS");
                return ncc;
            }
            var validhash = Userconf["user." + ups[0] + ".hash"];
            var thishash = Hash(ups[1]);
            if (validhash != thishash) {
                stream.Write("ERR_AUTH_INVALID_CREDENTIALS");
                return ncc;
            }
            var seclvlstr = Userconf["user." + ups[0] + ".sec"];
            int sec;
            if (!int.TryParse(seclvlstr, out sec)) {
                stream.Write("ERR_ACCOUNT_NOT_AUTHORIZED");
                return ncc;
            }

            ncc.Username = ups[0];
            ncc.SecurityLevel = sec;
            ncc.Successful = true;

            Thread.CurrentThread.Name = ncc.Username;

            return ncc;
        }

        internal static bool CheckUserPass(string user, string pass) {
            var thishash = Hash(pass);
            var validhash = Userconf["user." + user + ".hash"];
            return thishash.Equals(validhash, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string Hash(string input) {
            var sha = new System.Security.Cryptography.SHA512CryptoServiceProvider();
            var bytes = Encoding.Unicode.GetBytes(input);
            bytes = bytes.Where(b => b != 0).ToArray();
            bytes = sha.ComputeHash(bytes);
            Array.Reverse(bytes);
            bytes = sha.ComputeHash(bytes);
            Array.Reverse(bytes);
            bytes = sha.ComputeHash(bytes);
            Array.Reverse(bytes);
            bytes = sha.ComputeHash(bytes);
            Array.Reverse(bytes);
            bytes = sha.ComputeHash(bytes);
            return Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        }
    }
}