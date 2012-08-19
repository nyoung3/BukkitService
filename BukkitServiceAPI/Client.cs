using System;
using System.Linq;
using ConnorsNetworkingSuite;

namespace BukkitServiceAPI {
    public class Client {
        public readonly NetStream Stream;
        public readonly String Username;
        public readonly Int32 SecurityLevel;
        public readonly System.Net.IPEndPoint Ip;

        internal Client(NetStream stream, String user, Int32 sec, System.Net.IPEndPoint ip) {
            Stream = stream;
            Username = user;
            SecurityLevel = sec;
            Ip = ip;
        }

        public bool HasPermission(string node, bool default_ = false) {
            if (SecurityLevel > 9000) return true;

            var usernodes = Util.PermissionsConfig["users." + Username];
            if (usernodes.Split(';').Contains(node)) return true;

            int val;
            var valInConfig = Util.PermissionsConfig["nodevals." + node];
            if (int.TryParse(valInConfig, out val)) {
                return SecurityLevel >= val;
            }

            valInConfig = ServerPlugin.defaultnodes.ContainsKey(node) ? ServerPlugin.defaultnodes[node] : "";
            if (int.TryParse(valInConfig, out val)) {
                return SecurityLevel >= val;
            }

            return default_;
        }
    }
}
