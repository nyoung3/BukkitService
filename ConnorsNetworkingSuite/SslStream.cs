using System;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ConnorsNetworkingSuite {
    public sealed class SslStream : NetStream {
        private System.Net.Security.SslStream ssl;
        private readonly byte[] b = new byte[4096];
        private Thread asyncread;
        private bool finnished;

        public SslStream(TcpClient client) {
            UnderlyingStream = client.GetStream();
        }

        public SslStream(NetStream client) {
            Encoding = client.Encoding;
            UnderlyingStream = client.UnderlyingStream;
        }

        public SslStream(System.IO.Stream stream) {
            UnderlyingStream = stream;
        }



        public void AuthenticateAsClient(string targethost) {
            var t = AuthenticateAsClientAsync(targethost);
            t.Wait();
        }

        public void AuthenticateAsClient(string targethost, System.Net.Security.RemoteCertificateValidationCallback validationCallback) {
            ssl = new System.Net.Security.SslStream(UnderlyingStream, false, validationCallback, null);
            ssl.AuthenticateAsClient(targethost, null,
                                     SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl3,
                                     false);
        }

        public async Task AuthenticateAsClientAsync(string targethost) {
            ssl = new System.Net.Security.SslStream(UnderlyingStream, false, (s, c, h, l) => true, null);
            await ssl.AuthenticateAsClientAsync(targethost, null, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl3, false);
            UnderlyingStream = ssl;
        }

        public async Task AuthenticateAsClientAsync(string targethost, System.Net.Security.RemoteCertificateValidationCallback validationCallback) {
            ssl = new System.Net.Security.SslStream(UnderlyingStream, false, validationCallback, null);
            await ssl.AuthenticateAsClientAsync(targethost, null, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl3, false);
            UnderlyingStream = ssl;
        }

        public void AuthenticateAsServer(X509Certificate certificate) {
            ssl = new System.Net.Security.SslStream(UnderlyingStream);
            ssl.AuthenticateAsServer(certificate, false, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl3,
                                     false);
            UnderlyingStream = ssl;
        }

        public override string Read() {
            try {
                var br = ssl.Read(b, 0, 4096);
                return br < 1 ? null : Encoding.GetString(b, 0, br);
            } catch (Exception e) {
                LastError = e;
                finnished = true;
                return null;
            }
        }

        public override bool Write(string message) {
            try {
                var bytes = Encoding.GetBytes(message);
                ssl.Write(bytes);
                return true;
            } catch (Exception e) {
                LastError = e;
                finnished = true;
                return false;
            }
        }

        public override void BeginWrite(string message) {
            new Thread(() => Write(message)).Start();
        }

        public override void BeginRead(AsyncReadCallback callback) {
            if (asyncread != null && asyncread.IsAlive) asyncread.Abort();
            asyncread = new Thread(
                () => {
                    while (true) {
                        callback(this, Read());
                    }
                    // ReSharper disable FunctionNeverReturns
                });
            // ReSharper restore FunctionNeverReturns
            asyncread.Start();
        }

        public override void EndRead() {
            if (asyncread != null && asyncread.IsAlive) asyncread.Abort();
        }

        public override async Task<string> ReadAsync() {
            try {
                var br = await ssl.ReadAsync(b, 0, 4096);
                return br < 1 ? null : Encoding.GetString(b, 0, br);
            } catch (Exception e) {
                LastError = e;
                finnished = true;
                return null;
            }
        }

        public override async Task<bool> WriteAsync(string message) {
            try {
                var bytes = Encoding.GetBytes(message);
                await ssl.WriteAsync(bytes, 0, bytes.Length);
                return true;
            } catch (Exception e) {
                LastError = e;
                finnished = true;
                return false;
            }
        }

        public override void Close() {
            UnderlyingStream.Close();
        }

        public override bool Connected {
            get { return !finnished; }
        }
    }
}
