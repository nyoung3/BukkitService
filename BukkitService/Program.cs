using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BukkitService {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            if (Debugger.IsAttached || (args.Length > 0 && args[0].Contains("-console"))) {
                Console.Title = "Bukkit Service";
                RunAsConsole();
                Environment.Exit(0);
            } else {
                try {
                    Console.Title = "Bukkit Service";
                    RunAsConsole();
                    Environment.Exit(0);
                } catch {
                    ServiceBase.Run(new Bukkit());
                }
            }
        }

        static void RunAsConsole() {
            BukkitService.Main.Start(true);
            Console.Write("Service started.\r\n Starting client...\r\n");
            BasicClient.Program.Main();
            BukkitService.Main.Stop();
        }
    }
}
