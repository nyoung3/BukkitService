using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ConnorsNetworkingSuite {
    public class SimpleStreamHost : NetStreamHost {
        private bool run = true;
        private TcpListener listener;

        public override void Start(IPEndPoint end) {
            listener = new TcpListener(end);
            listener.Start();
            while (run) {
                try {
                    var c = listener.AcceptTcpClient();
                    RaiseNewClient(new SimpleStream(c.GetStream()), (IPEndPoint) c.Client.RemoteEndPoint);
                } catch (Exception e) {
                    Debug.WriteLine("Error in client accept. Details below\r\n" + e);
                }
            }
        }

        public override void Stop() {
            run = false;
            listener.Stop();
        }
    }
}
