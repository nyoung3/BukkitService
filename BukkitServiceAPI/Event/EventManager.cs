using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukkitServiceAPI.Event {
    public static class EventManager {
        private static readonly List<ServerStart> serverStarts = new List<ServerStart>();
        private static BukkitCommandPermission permissionCheck = (cmd, client) => true;

        internal static bool CheckCmdPerm(string cmd, Client client) {
            try {
                return permissionCheck(cmd, client);
            } catch {
                Logger.Log("Exception in permission check\r\n" + cmd);
                return false;
            }
        }

        internal static void UnregisterHooks() {
            serverStarts.Clear();
        }

        public static void RegisterEvent(ServerStart d) {
            serverStarts.Add(d);
        }

        public static void RegisterEvent(BukkitCommandPermission d) {
            permissionCheck = d;
        }
    }


    public delegate bool BukkitCommandPermission(string cmd, Client client);
    public delegate void ServerStart(ServerStartEvent e);
}
