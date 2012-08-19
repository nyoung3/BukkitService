using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using BukkitServiceAPI;

namespace BukkitService {
    class Main {
        internal static Config config = new Config(Path.Combine(Util.StorageDir, "config.conf"));
        internal static Config strings = new Config(Path.Combine(Util.StorageDir, "strings.conf"));
        private static Thread keepalivethread;

        public static void Start(bool console = false) {
            if (Interactions.NewClientHandler.Userconf["user.root.enabled"] != "true") {
                Interactions.NewClientHandler.Userconf["user.root.enabled"] = "true";
                Interactions.NewClientHandler.Userconf["user.root.hash"] =
                    Interactions.NewClientHandler.Hash("please_change_this");
                Interactions.NewClientHandler.Userconf["user.root.sec"] =
                    int.MaxValue.ToString(CultureInfo.InvariantCulture);
            }

            var port = 3000;
            int port_;

            if ((port_ = config.GetInt32("port")) != 0) {
                port = port_;
            }

            var cmdargs = Environment.GetCommandLineArgs();
            for (var i = 0; i < cmdargs.Length - 1; ++i) {
                if (cmdargs[i] != "-port") continue;
                if (int.TryParse(cmdargs[i + 1], out port_)) {
                    port = port_;
                }
                break;
            }

            if (console) Console.WriteLine("Started on port " + port);

            keepalivethread = new Thread(ServerKeepAlive);
            keepalivethread.Start();

            Interface.StartTcpServer(port);


        }

        public static void Stop() {
            foreach (var c in Interactions.ClientLoop.Clients) {
                c.Stream.Write("SERVICE STOPPING");
                c.Stream.Close();
            }
            if (Server.Instance.Command("bsc_kick_all Sudden server shutdown (REASON UNKNOWN)")) {
                Server.Instance.Command("save-all");
                Server.Instance.Command("stop");
            }
            Interface.StopTcpServer();
            keepalivethread.Abort();
        }

// ReSharper disable FunctionNeverReturns
        static void ServerKeepAlive() {
            while (true) {
                var interval = Math.Max((ushort)1, config.GetUInt16("keepalive-interval", 1));
                Thread.Sleep(60000 * interval);
                if (!Server.Instance.IsRunning) continue;
                if (Server.Instance.LastStarted > DateTime.Now.AddMinutes(-1)) continue;

                Server.Instance.Command("ping");

                if (Server.Instance.LastConsoleMessage > DateTime.Now.AddMinutes((interval * -1) - 0.5)) continue;

                Logger.Log("PROCESS KILLED BY SERVER KEEPALIVE");
                Server.Instance.Process.Kill();
                Thread.Sleep(1000);
                Server.Instance.Start();
            }
        }
// ReSharper restore FunctionNeverReturns
    }
}
