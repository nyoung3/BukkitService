using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnorsNetworkingSuite {
    public class LoopbackStream : NetStream {
        private readonly object readlock = new object();
        private readonly List<string> buffer = new List<string>();
        private readonly AutoResetEvent are = new AutoResetEvent(false);
        private Thread readthread;

        public override string Read() {
            lock (readlock) {
                if (buffer.Count > 0) {
                    var vtr = buffer[0];
                    buffer.RemoveAt(0);
                    return vtr;
                }

                are.WaitOne();
                var lm = buffer[0];
                buffer.RemoveAt(0);
                return lm;
            }
        }

        public override bool Write(string message) {
            buffer.Add(message);
            are.Set();
            return true;
        }

        public override void BeginWrite(string message) {
            new Thread(() => Write(message)).Start();
        }

        public override void BeginRead(AsyncReadCallback callback) {
            EndRead();
            (readthread = new Thread(
                              () => {
                                  string msg;
                                  while ((msg = Read()) != null) {
                                      callback(this, msg);
                                  }
                              })).Start();
        }

        public override void EndRead() {
            if (readthread != null && readthread.IsAlive) readthread.Abort();
        }

        public override Task<string> ReadAsync() {
            throw new NotImplementedException();
        }

        public override Task<bool> WriteAsync(string message) {
            throw new NotImplementedException();
        }

        public override void Close() {
            UnderlyingStream.Close();
        }

        public override bool Connected {
            get { return true; }
        }
    }
}
