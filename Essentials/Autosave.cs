﻿using BukkitServiceAPI;
using System.Threading;

namespace Essentials {
    public class AutoSave : ServerPlugin {
        int interval;
        Timer timer;

        public override void OnLoad() {
            if (int.TryParse(Config["interval"], out interval)) {
                Log("Loaded [AutoSave] with an interval of " + interval);
            } else {
                Config["interval"] = "10";
                interval = 10;
                Log("Loaded [AutoSave] with the default interval of 10");
            }

            timer = new Timer(Save, null, interval * 1000 * 60, interval * 1000 * 60);
        }

        public override bool OnCommand(string cmd, Client sender) {
            if (sender.HasPermission("autosave.change")) {
                var split = cmd.Split(' ');
                if (split.Length < 2) {
                    sender.Stream.Write("Too few parameters\r\n");
                    return true;
                }
                int i;
                if (!int.TryParse(split[1], out i)) {
                    sender.Stream.Write("Number expected; string given\r\n");
                    return true;
                }
                interval = i;
                timer.Change(interval * 1000 * 60, interval * 1000 * 60);
                Log("AutoSave interval set to " + interval);
            } else {
                sender.Stream.Write("\u001B[31mYou do not have permission change the interval\u001B[0m\r\n");
            }
            return true;
        }

        public void Save(object state) {
            ConsoleCommand("save-all");
        }
    }
}