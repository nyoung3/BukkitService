using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BukkitService.Interactions;
using BukkitServiceAPI;
using BukkitServiceAPI.Event;

namespace BukkitService.Plugins {
    class PluginLoader {
        private static readonly string PluginFolder = Path.Combine(Util.StorageDir, "extensions");
        internal static readonly List<ServerPlugin> Plugins = new List<ServerPlugin>();

        public static void LoadPlugins() {
            if (!Directory.Exists(PluginFolder)) {
                Directory.CreateDirectory(PluginFolder);
            }
            var files = Directory.GetFiles(PluginFolder, "*.plugin");
            var data = files.Select(File.ReadAllText);

            foreach (var plugin in data) {
                if (plugin.StartsWith("script\r\n")) {
                    ScriptLoader.LoadScript(plugin);
                } else if (plugin.StartsWith("library\r\n")) {
                    LibraryLoader.LoadLibrary(plugin);
                }
            }
        }

        public static void UnloadPlugins() {
            EventManager.UnregisterHooks();
            Plugins.Clear();
            CommandHandler.Hooks.Clear();
        }
    }
}