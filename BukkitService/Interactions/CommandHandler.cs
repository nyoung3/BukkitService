using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BukkitService.Plugins;
using BukkitServiceAPI;

namespace BukkitService.Interactions {
    static class CommandHandler {
        private const string LineEnd = "\u001b[0m\r\n";

        private readonly static List<String> killables = new List<string>(); 

        private static string Escape(string code) {
            return "\u001b[" + code + "m";
        }
        internal static readonly Dictionary<String, ServerPlugin> Hooks = new Dictionary<string, ServerPlugin>();

        internal static void RegisterCommand(ServerPlugin plugin, string label) {
            Hooks[label] = plugin;
        }

        internal static void HandleCommand(Client sender, string message) {
            var cmdsplit = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (BuiltInCmds(sender, message, cmdsplit)) {
                return;
            }

            if ((message.StartsWith("/") || message.StartsWith("#")) && Hooks.ContainsKey(cmdsplit[0])) {
                try {
                    if (Hooks[cmdsplit[0]].OnCommand(message, sender)) {
                        Logger.Log("<" + sender.Username + "> " + message, true, "command");
                    }
                    return;
                } catch (Exception ex) {
                    Logger.Log("Error executing command " + cmdsplit[0] + "\r\n" + ex);
                    return;
                }
            }

            if (message.StartsWith("/")) {
                if (!sender.HasPermission(cmdsplit[0], true)) {
                    sender.Stream.Write("\u001B[31mYou do not have permission to do that\u001B[0m");
                    return;
                }
                if (Server.Instance.Command(message.Remove(0, 1))) {
                    Logger.Log("<" + sender.Username + "> " + message, true, "command");
                } else {
                    sender.Stream.Write(Escape("31") + "The server is not running" + LineEnd);
                }
                return;
            }

            if (message.StartsWith("#")) {
                if (message.Length < 2) return;
                var chatcmd = string.Format(Main.config["chat-format"], sender.Username, message.Substring(1));
                Logger.Log("ServerChat: <" + sender.Username + "> " + message, false, "command");
                Server.Instance.Command(chatcmd);
                return;
            }

            Logger.Log("<" + sender.Username + "> " + message, true, "command");
        }

        #region DefaultCommands

        private static bool BuiltInCmds(Client sender, string message, IList<string> split) {
            switch (split[0]) {
                case "/reloadplugins":
                    if (!sender.HasPermission("builtin.reloadplugs")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    PluginLoader.UnloadPlugins();
                    PluginLoader.LoadPlugins();
                    Logger.Log("Plugins reloaded");
                    return true;
                case "/changepass":
                    if (split.Count != 3) {
                        sender.Stream.Write("\u001B[33mUsage: /changepass [old_password] [new_password]" + LineEnd);
                        return true;
                    }
                    if (!NewClientHandler.CheckUserPass(sender.Username, split[1])) {
                        sender.Stream.Write("\u001B[31mIncorrect Password" + LineEnd);
                        return true;
                    }

                    NewClientHandler.Userconf["user." + sender.Username + ".hash"] = NewClientHandler.Hash(split[2]);

                    return true;
                case "/setpass":
                    if (!sender.HasPermission("builtin.setpass")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    if (split.Count != 3) {
                        sender.Stream.Write("\u001B[33mUsage: /setpass [username] [new_password]" + LineEnd);
                        return true;
                    }

                    NewClientHandler.Userconf["user." + split[1] + ".hash"] = NewClientHandler.Hash(split[2]);
                    sender.Stream.Write("\u001B[32mChanged the password successfully!" + LineEnd);
                    return true;
                case "/config":
                    if (!sender.HasPermission("builtin.viewconfig")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    var sp = message.Split(new[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
                    if (sp.Length < 2) {
                        sender.Stream.Write("\u001B[33mValid configs: main, user, perm, string" + LineEnd);
                        return true;
                    }
                    Config c;
                    switch (sp[1]) {
                        case "main":
                            c = Main.config;
                            break;
                        case "user":
                            c = NewClientHandler.Userconf;
                            break;
                        case "perm":
                            c = Util.PermissionsConfig;
                            break;
                        case "string":
                            c = Main.strings;
                            break;
                        default:
                            sender.Stream.Write("\u001B[31mCould not find config '" + sp[1] + "'" + LineEnd);
                            sender.Stream.Write("\u001B[33mValid configs: main, user, perm, string" + LineEnd);
                            return true;
                    }
                    if (sp.Length < 3) {
                        sender.Stream.Write("\u001B[32m");
                        foreach (var kvp in c.Data) {
                            sender.Stream.Write(kvp.Key + ": " + kvp.Value + "\r\n");
                        }
                        sender.Stream.Write("\u001B[0m");
                        return true;
                    }
                    if (sp.Length < 4) {
                        sender.Stream.Write("\u001B[32m" + sp[2] + ": " + c[sp[2]] + LineEnd);
                        return true;
                    }

                    if (!sender.HasPermission("builtin.editconfig")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    if (sp[2].Equals("Delete", StringComparison.InvariantCultureIgnoreCase)) {
                        sender.Stream.Write(c.DeleteKey(sp[3])
                                                ? "\u001B[33mKey deleted!" + LineEnd
                                                : "\u001B[31mKey did not exist." + LineEnd);
                        Logger.Log(sender.Username + " changed the config: Deleted '" + sp[3] + "'", false, "config");
                        return true;
                    }

                    c[sp[2]] = sp[3];
                    sender.Stream.Write("\u001B[32m" + sp[2] + " has been changed to " + sp[3] + LineEnd);
                    Logger.Log(sender.Username + " changed the config: Changed '" + sp[2] + "' to '" + sp[3] + "'", false, "config");

                    return true;
                case "/createuser":
                    if (!sender.HasPermission("builtin.createuser")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    split = message.Split(new[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Count != 4) {
                        sender.Stream.Write("\u001B[33mUsage: /createuser [username] [security_level] [password]" + LineEnd);
                        return true;
                    }
                    int sec;
                    if (!int.TryParse(split[2], out sec)) {
                        sender.Stream.Write("\u001B[33mSecurity level must be a 32-bit integer" + LineEnd);
                        return true;
                    }
                    if (sec >= sender.SecurityLevel) {
                        sender.Stream.Write(
                            Escape("31") + "You are not allowed to create accounts with a security level greater than yours" + LineEnd);
                        return true;
                    }
                    NewClientHandler.Userconf["user." + split[1] + ".enabled"] = "true";
                    NewClientHandler.Userconf["user." + split[1] + ".hash"] = NewClientHandler.Hash(split[3]);
                    NewClientHandler.Userconf["user." + split[1] + ".sec"] = sec.ToString(CultureInfo.InvariantCulture);
                    sender.Stream.Write(Escape("32") + "User created successfully!" + LineEnd);
                    Logger.Log("User '" + split[1] + "' created by '" + sender.Username + "'", false, "user");
                    return true;
                case "#status":
                    if (!Server.Instance.IsRunning) {
                        sender.Stream.Write(Escape("33") + "Server is offline" + LineEnd);
                        return true;
                    }
                    Server.Instance.Process.Refresh();
                    sender.Stream.Write(Escape("32") + "Memory: " + Server.Instance.Process.WorkingSet64 + LineEnd);
                    sender.Stream.Write(Escape("32") + "Online: " + (DateTime.Now - Server.Instance.LastStarted) + LineEnd);
                    return true;
                case "/start":
                    if (Server.restartinprogress) {
                        return true;
                    }
                    if (!sender.HasPermission("server.start")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    if (Server.Instance.IsRunning) {
                        sender.Stream.Write(Escape("33") + "The server is already running" + LineEnd);
                        return true;
                    }
                    Server.Instance.Start();
                    return true;
                case "/stop":
                    if (Server.restartinprogress) {
                        return true;
                    }
                    if (!sender.HasPermission("server.stop")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    if (!Server.Instance.IsRunning) {
                        sender.Stream.Write(Escape("33") + "The server is not running" + LineEnd);
                        return true;
                    }
                    var msg = "";
                    for (var i = 1; i < split.Count; ++i) {
                        msg += split[i] + " ";
                    }
                    Server.Instance.Command("bsc_kick_all " + msg);
                    Server.Instance.Command("save-all");
                    Server.Instance.Command("stop");
                    return true;
                case "/restart":
                    if (Server.restartinprogress) {
                        return true;
                    }
                    if (!sender.HasPermission("server.restart")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    if (!Server.Instance.IsRunning) {
                        Server.Instance.Start();
                        return true;
                    }
                    Server.restartinprogress = true;
                    var msg2 = "";
                    for (var i = 1; i < split.Count; ++i) {
                        msg2 += split[i] + " ";
                    }
                    Server.Instance.Command("bsc_kick_all " + msg2);
                    Thread.Sleep(50);
                    Server.Instance.Command("save-all");
                    Server.Instance.Command("stop");
                    new Thread(
                        () => {
                            Server.Instance.WaitForStop();
                            Server.Instance.Start();
                            Server.restartinprogress = false;
                        }).Start();
                    return true;
                case "#ckick":
                    if (!sender.HasPermission("console.kick")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    if (split.Count != 2) {
                        sender.Stream.Write("\u001B[33mUsage: #ckick [name]");
                        return true;
                    }
                    foreach (var client in ClientLoop.Clients.
                        Where(client => client.
                        Username.Equals(split[1],
                        StringComparison.InvariantCultureIgnoreCase))
                        .ToArray()) {
                        sender.Stream.Write("\u001b[32mKicked: " + client.Username + ", logged in from " + client.Ip + "\u001b[0m\r\n");
                        client.Stream.Close();
                    }
                    return true;
                case "#consoleusers":
                    foreach (var client in ClientLoop.Clients) {
                        sender.Stream.Write("\u001b[32m" + client.Username + ", logged in from " + client.Ip + "\u001b[0m\r\n");
                    }
                    return true;
                case "#kill":
                    if (killables.Contains(sender.Username)) {
                        if (Server.Instance.IsRunning) {
                            Server.Instance.Process.Kill();
                        }
                        return true;
                    }
                    if (!sender.HasPermission("server.kill")) {
                        sender.Stream.Write("\u001B[31mYou do not have permission to do that." + LineEnd);
                        return true;
                    }
                    sender.Stream.Write("\u001B[31mAre you sure? Type #kill again to kill the server.\u001B[0m\r\n");
                    killables.Add(sender.Username);
                    new Thread(() => {
                        var usr = sender.Username;
                        Thread.Sleep(10000);
                        killables.Remove(usr);
                    }).Start();
                    return true;
                case "#serverip":
                    sender.Stream.Write(Main.config["serverip"]);
                    return true;
                case "###pong":
                    return true;
            }

            return false;
        }

        #endregion
    }
}
