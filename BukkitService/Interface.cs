using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using BukkitServiceAPI;
using ConnorsNetworkingSuite;

namespace BukkitService {
    static class Interface {
        private static readonly List<Thread> ClientThreads = new List<Thread>();
        private static readonly SimpleStreamHost Ssh = new SimpleStreamHost();
        private static readonly SimpleStreamHost Ssh2 = new SimpleStreamHost();

        static Interface() {
            Ssh.NewClient += Interactions.NewClientHandler.HandleClient;
            Ssh.NewClient += Interactions.NewClientHandler.HandleClientPlaintext;
        }

        public static void StartTcpServer(int port, int port2 = 3001) {
            Ssh.StartInThread(port);
            Ssh2.StartInThread(port2);
        }

        public static void StopTcpServer() {
            Ssh.Stop();
            Ssh2.Stop();
        }
    }
}
