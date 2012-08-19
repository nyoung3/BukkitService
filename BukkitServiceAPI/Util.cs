using System;
using System.IO;
using Microsoft.Win32;

namespace BukkitServiceAPI {
    static class Util {
        private const string JreKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\JavaSoft\\Java Runtime Environment";

        public static readonly string StorageDir;
        public static readonly string ServerFilesDir;

        private static Config _permConf;
        internal static Config PermissionsConfig {
            get { return _permConf ?? (_permConf = new Config(Path.Combine(StorageDir, "permissions.conf"))); }
        }

        static Util() {
            StorageDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData,
                                          Environment.SpecialFolderOption.Create),
                "BukkitService");
            ServerFilesDir = Path.Combine(StorageDir, "server_files");
            if (!Directory.Exists(StorageDir)) {
                Directory.CreateDirectory(StorageDir);
            }
            if (!Directory.Exists(ServerFilesDir)) {
                Directory.CreateDirectory(ServerFilesDir);
            }
        }

        private static string _java;
        internal static string JavaExecutable {
            get { return _java ?? (_java = FindJava()); }
        }

        private static string FindJava() {
            var cv = Registry.GetValue(JreKey, "CurrentVersion", null);
            var path = Registry.GetValue(JreKey + "\\" + cv, "JavaHome", null);
            return path + "\\bin\\java.exe";
        }
    }
}
