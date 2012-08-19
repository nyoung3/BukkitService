using System;
using System.Diagnostics;
using System.Threading;
using BukkitService.Interactions;
using BukkitServiceAPI;

namespace BukkitService {
    class Server {
        internal DateTime LastConsoleMessage = DateTime.Now;
        internal DateTime LastStarted = DateTime.Now;
        private readonly object locker = new object();
        public static readonly Server Instance = new Server();
        internal Process Process { get; private set; }
        private bool procstarted;
        internal static bool restartinprogress;

        private readonly ManualResetEvent _spMre = new ManualResetEvent(true);
        private readonly ManualResetEvent _stMre = new ManualResetEvent(false);

        public event Action Stopped;

        private Server() {
        }

        public void Start() {
            if (restartinprogress) return;
            if (IsRunning) return;
            lock (locker) {
                if (IsRunning) return;
                Process = new Process {
                    StartInfo = new ProcessStartInfo(Main.config["javapath"]
                        ) {
                            Arguments = Main.config["javaargs"] + " -Djline.terminal=jline.UnsupportedTerminal -jar \"" + Main.config["jarpath"] + "\" " + Main.config["bukkitargs"],
                            WorkingDirectory = Util.ServerFilesDir,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            RedirectStandardInput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        },
                    EnableRaisingEvents = true
                };

                Process.Exited += OnStop;
                Process.OutputDataReceived += ConsoleOutput;
                Process.ErrorDataReceived += ConsoleOutput;

                Process.Start();
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();

                procstarted = true;
            }
        }

        private void ConsoleOutput(object sender, DataReceivedEventArgs args) {
            LastConsoleMessage = DateTime.Now;
            ClientLoop.ConsoleOutput(args.Data);
        }

        private void OnStop(object sender, EventArgs e) {
            procstarted = false;
            _spMre.Set();
            _stMre.Reset();
            ClientLoop.ConsoleOutput("\u001b[32mServer stopped\u001b[0m\r\n");
            try {
                Stopped();
            } catch (Exception ex) {
                Logger.Log("Exception occured invoking Stopped in Server.\r\n" + ex, false);
            }
        }

        public bool IsRunning {
            get { return Process != null && procstarted && !Process.HasExited; }
        }

        public bool Command(string cmd) {
            if (IsRunning) {
                if (cmd.Trim().Length < 1) return true;
                Process.StandardInput.Write(cmd.Trim() + "\n");
                return true;
            }
            return false;
        }

        public void WaitForStop() {
            _spMre.WaitOne();
        }

        public void WaitForStart() {
            _stMre.WaitOne();
        }
    }
}
