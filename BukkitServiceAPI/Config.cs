using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace BukkitServiceAPI {
    public class Config {
        public bool AutoSave { get; set; }
        private readonly string path;
        private readonly Dictionary<String, String> data = new Dictionary<string, string>();

        public ReadOnlyDictionary<String, String> Data {
            get { return new ReadOnlyDictionary<string, string>(data); }
        }

        public bool DeleteKey(string key) {
            return data.Remove(key);
        }

        public Config(string configpath) {
            AutoSave = Defaults.AutoSave;
            path = Path.GetFullPath(configpath);

            if (!File.Exists(path)) {
                return;
            }

            Load();
        }

        public string this[string key] {
            get {
                key = key.Trim().ToLower();

                if (key.Contains(':')) {
                    throw new ArgumentException("Key cannot contain ':'");
                }
                if (key.Contains("\r") || key.Contains("\n")) {
                    throw new ArgumentException("Key cannot contain linebreaks");
                }

                return data.ContainsKey(key) ? data[key] : "";
            }
            set {
                value = value.Trim();
                key = key.Trim().ToLower();

                if (key.Contains("\r") || key.Contains("\n")) {
                    throw new ArgumentException("Key cannot contain linebreaks");
                }
                if (key.Contains(':')) {
                    throw new ArgumentException("Key cannot contain ':'");
                }
                if (value.StartsWith("#")) {
                    throw new ArgumentException("Key cannot start with '#'");
                }
                if (value.Contains("\r") || value.Contains("\n")) {
                    throw new ArgumentException("Value cannot contain linebreaks");
                }

                data[key] = value.Trim();
                if (AutoSave) Save();

            }
        }

        public void Save() {
            lock (path) {
                var sb = new StringBuilder();
                foreach (var kvp in data) {
                    sb.Append(kvp.Key);
                    sb.Append(':');
                    sb.AppendLine(kvp.Value);
                }
                File.WriteAllText(path, "## Config generated " + DateTime.Now + " ##\r\n" + sb, Encoding.Unicode);
            }
        }

        public void Load() {
            try {
                using (var sr = new StreamReader(File.OpenRead(path))) {
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        if (line.Trim().StartsWith("#")) continue;
                        var sp = line.Split(new[] { ':' }, 2);
                        if (sp.Length < 2) continue;
                        if (string.IsNullOrWhiteSpace(sp[0])) continue;
                        data[sp[0].Trim().ToLower()] = sp[1].Trim();
                    }
                }
            } catch (Exception ex) {
                Logger.Log("Exception loading config\r\n" + ex);
            }
        }

        public static class Defaults {
            public static bool AutoSave = true;
        }

        #region Extra Accessors
        public void Set(string key, object o) {
            key = key.Trim().ToLower();
            this[key] = o.ToString();
        }

        public bool GetBool(string key) {
            key = key.Trim().ToLower();
            return data.ContainsKey(key) && data[key].Equals("True", StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetInt32(string key, int default_ = 0) {
            key = key.Trim().ToLower();
            int p;
            if (data.ContainsKey(key) && int.TryParse(data[key], out p))
                return p;
            return default_;
        }

        public uint GetUInt32(string key, uint default_ = 0) {
            key = key.Trim().ToLower();
            uint p;
            if (data.ContainsKey(key) && uint.TryParse(data[key], out p))
                return p;
            return default_;
        }

        public short GetInt16(string key, short default_ = 0) {
            key = key.Trim().ToLower();
            short p;
            if (data.ContainsKey(key) && short.TryParse(data[key], out p))
                return p;
            return default_;
        }

        public ushort GetUInt16(string key, ushort default_ = 0) {
            key = key.Trim().ToLower();
            ushort p;
            if (data.ContainsKey(key) && ushort.TryParse(data[key], out p))
                return p;
            return default_;
        }

        public long GetInt64(string key, long default_ = 0) {
            key = key.Trim().ToLower();
            long p;
            if (data.ContainsKey(key) && long.TryParse(data[key], out p))
                return p;
            return default_;
        }

        public ulong GetUInt64(string key, ulong default_ = 0) {
            ulong p;
            if (data.ContainsKey(key) && ulong.TryParse(data[key], out p))
                return p;
            return default_;
        }

        public DateTime GetDateTime(string key) {
            DateTime p;
            if (data.ContainsKey(key) && DateTime.TryParse(data[key], out p))
                return p;
            return new DateTime(0);
        }
        public DateTime GetDateTime(string key, DateTime default_) {
            DateTime p;
            if (data.ContainsKey(key) && DateTime.TryParse(data[key], out p))
                return p;
            return default_;
        }
        #endregion
    }
}
