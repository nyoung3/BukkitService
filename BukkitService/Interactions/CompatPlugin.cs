using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnorsNetworkingSuite;

namespace BukkitService.Interactions {
    public static class CompatPlugin {
        private static NetStream stream;
        public static bool Connected { get; private set; }

        static CompatPlugin() {
            Server.Instance.Stopped += () => { Connected = false; };
        }

        internal static void Hook(NetStream compatstream) {
            stream = compatstream;
            Connected = true;
        }

        public static string Stop(string message) {
            if (!Connected) return "offline";
            lock (stream) {
                stream.Write("stop " + message);
                return stream.Read();
            }
        }

        public static string Kick(string player, string message) {
            if (!Connected) return "offline";
            lock (stream) {
                stream.Write("kick " + player + " " + message);
                return stream.Read();
            }
        }

        public static string KickAll(string message) {
            if (!Connected) return "offline";
            lock (stream) {
                stream.Write("kickall " + message);
                return stream.Read();
            }
        }
    }
}
