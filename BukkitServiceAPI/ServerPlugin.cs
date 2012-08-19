using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BukkitServiceAPI;

namespace BukkitServiceAPI
{
    public class ServerPlugin {
        protected internal Config Config { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        internal static Dictionary<String, String> defaultnodes = new Dictionary<string, string>();
        internal static Func<ServerPlugin, string, bool> consolecmd; 

        public virtual void OnLoad() {
        }
        public virtual bool OnCommand(string command, Client sender) {
            return false;
        }

        protected void Log(string message) {
            Logger.Log(message);
        }

        protected bool ConsoleCommand(string message) {
            return consolecmd(this, message);
        }

        protected bool ServerRunning { get { return false; } }
    }
}