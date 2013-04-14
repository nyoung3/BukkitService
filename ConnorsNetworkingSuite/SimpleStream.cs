using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnorsNetworkingSuite {
    public sealed class SimpleStream : NetStream {
        public static Encoding DefaultEncoding = Encoding.UTF8;
        private readonly object _readLock = new object();
        readonly byte[] b = new byte[4096];
        private bool connected = true;
        private Thread asyncread;

        internal SimpleStream() {
            Encoding = DefaultEncoding;
        }

        public SimpleStream(Stream basestream)
            : this() {
            UnderlyingStream = basestream;
        }

        public override string Read() {
            if (!connected) return null;
            lock (_readLock) {
                try {
                    var br = UnderlyingStream.Read(b, 0, 4096);
                    if (br < 1) connected = false;
                    return br < 1 ? null : Encoding.GetString(b, 0, br);
                } catch (Exception e) {
                    LastError = e;
                    connected = false;
                    return null;
                }
            }
        }

        public override bool Write(string message) {
            if (!connected) return false;
            try {
                var m = Encoding.GetBytes(message);
                UnderlyingStream.Write(m, 0, m.Length);
                return true;
            } catch (Exception e) {
                LastError = e;
                connected = false;
                return false;
            }
        }

        public override void BeginWrite(string message) {
            new Thread(() => Write(message)).Start();
        }

        public override void BeginRead(AsyncReadCallback callback) {
            if (asyncread != null && asyncread.IsAlive) {
                CancelAsync();
            }
            (asyncread = new Thread(
                () => {
                    while (true) {
                        try {
                            var br = UnderlyingStream.Read(b, 0, 4096);
                            lock (_readLock) {
                                if (br < 1) connected = false;
                                callback.BeginInvoke(this, br < 1 ? null : Encoding.GetString(b, 0, br), null, null);
                            }
                        } catch (Exception e) {
                            LastError = e;
                            connected = false;
                            return;
                        }
                    }
                })).Start();
        }

        public static NetStream ConnectToEndpoint(IPEndPoint end) {
            var tcp = new TcpClient(end);
            return new SimpleStream(tcp.GetStream());
        }

        public static NetStream ConnectToHost(string host, int port) {
            var tcp = new TcpClient(host, port);
            return new SimpleStream(tcp.GetStream());
        }

        public override void EndRead() {
            asyncread.Abort();
        }

        public override Task<string> ReadAsync() {
            throw new NotImplementedException();
        }

        public override async Task<bool> WriteAsync(string message) {
            if (!connected) return false;
            try {
                var m = Encoding.GetBytes(message);
                await UnderlyingStream.WriteAsync(m, 0, m.Length);
                return true;
            } catch (Exception e) {
                LastError = e;
                connected = false;
                return false;
            }
        }

        public override void Close() {
            UnderlyingStream.Close();
        }

        public override bool Connected {
            get { return connected; }
        }

        private void CancelAsync() {
            lock (_readLock) {
                asyncread.Abort();
            }
        }

        public static explicit operator SimpleStream(Stream s) {
            return new SimpleStream(s);
        }
    }
}
