using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BukkitServiceAPI;

namespace BukkitService.Interactions {
    static class ClientLoop {
        internal static readonly List<Client> Clients = new List<Client>(); 

        static ClientLoop() {
            Logger.OnMessage = OutputLog;
        }

        internal static void BeginClient(Client client) {
            Logger.Log(client.Username + " has logged in", false, "user");
            Clients.Add(client);

            try {
                while (client.Stream.Connected) {
                    var msg = client.Stream.Read();
                    if (msg == null) {
                        Debug.WriteLine(client.Stream.LastError);
                        break;
                    }
                    CommandHandler.HandleCommand(client, msg);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        internal static void ConsoleOutput(string message) {
            if (message == null) return;
            var ctr = Clients.Where(client => !client.Stream.Write(message.Trim() + "\r\n")).ToArray();
            foreach (var client in ctr) {
                Clients.Remove(client);
            }
        }

        private static void OutputLog(string message) {
            if (message == null) return;
            var ctr = Clients.Where(client => !client.Stream.Write(message + "\u001b[0m\r\n")).ToArray();
            foreach (var client in ctr) {
                Clients.Remove(client);
            }
        }
    }
}
