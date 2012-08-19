using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BukkitServiceAPI {
    static class Logger {
        static Logger() {
            var logdir = Path.Combine(Util.StorageDir, "logs");
            if (!Directory.Exists(logdir)) Directory.CreateDirectory(logdir);
        }

        internal static MessageEventHandler OnMessage;

        private static readonly Dictionary<String, String> _paths = new Dictionary<string, string>();
        private static string GetPath(string name) {
            if (_paths.ContainsKey(name)) return _paths[name];
            _paths[name] = Path.Combine(Util.StorageDir, "logs", name + ".log");
            return _paths[name];
        }

        internal static void Log(string message, bool broadcast = true, string log = "wrapper") {
            try {
                if (broadcast)
                    OnMessage(message);
                lock (GetPath(log)) {
                    message = message.Trim();
                    File.AppendAllText(GetPath(log), "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + "\r\n", Encoding.Unicode);
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }
    }

    public delegate void MessageEventHandler(string message);
}
