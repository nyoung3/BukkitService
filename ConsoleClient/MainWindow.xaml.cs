﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ConsoleClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private static readonly byte[] Handshake = new byte[] { 0xFE, 0xFD, 0x09, 0x00, 0x00, 0x00, 0x01 };
        private static readonly byte[] Stat = new byte[] { 0xFE, 0xFD, 0x00, 0x00, 0x00, 0x00, 0x01 };
        private ServerStats serverStats;

        public MainWindow() {
            InitializeComponent();
            consoleOutput.Document.Blocks.Remove(consoleOutput.Document.Blocks.FirstBlock);
            errorOutput.Document.Blocks.Remove(errorOutput.Document.Blocks.FirstBlock);
            infopage.Navigate(new Uri("UserInfo.xaml", UriKind.Relative), "Connorcpu");
        }

        private void StartClick(object sender, RoutedEventArgs e) {
            Connection.Write("/start");
        }

        private void StopClick(object sender, RoutedEventArgs e) {
            Connection.Write("/stop");
        }

        private void RestartClick(object sender, RoutedEventArgs e) {
            Connection.Write("/restart");
        }

        private static readonly Regex SevereRegex = new Regex("^([0-9])([0-9]):([0-9])([0-9]):([0-9])([0-9]) \\[SEVERE\\]|^at\\b|^java\\.");
        private static readonly Regex WarningRegex = new Regex("^([0-9])([0-9]):([0-9])([0-9]):([0-9])([0-9]) \\[WARNING\\]");
        private static readonly Regex ChestshopRegex = new Regex("^([0-9])([0-9]):([0-9])([0-9]):([0-9])([0-9]) \\[INFO\\] \\[ChestShop\\]");
        private static readonly AnsiParser StdParser = new AnsiParser();
        private static readonly AnsiParser ErrParser = new AnsiParser();
        private static readonly AnsiParser ShpParser = new AnsiParser();

        private void WindowLoaded1(object sender, RoutedEventArgs e) {
            var lw = new LoginWindow();
            if (lw.ShowDialog() != true) {
                Environment.Exit(0);
            }
            Title = lw.Hostbox.Text;
            Connection.Callback = message => {
                if (string.IsNullOrEmpty(message)) {
                    return;
                }
                if (SevereRegex.IsMatch(message)) {
                    var d = ErrParser.ParseText("\u001b[31m" + message);
                    Dispatcher.Invoke(() => errorOutput.Document.Blocks.Add(d), DispatcherPriority.Background);
                    Dispatcher.Invoke(() => errorOutput.ScrollToEnd(), DispatcherPriority.Background);
                } else if (WarningRegex.IsMatch(message)) {
                    var d = ErrParser.ParseText("\u001b[33m" + message);
                    Dispatcher.Invoke(() => errorOutput.Document.Blocks.Add(d), DispatcherPriority.Background);
                    Dispatcher.Invoke(() => errorOutput.ScrollToEnd(), DispatcherPriority.Background);
                } else if (ChestshopRegex.IsMatch(message)) {
                    var d = ShpParser.ParseText(message);
                    Dispatcher.Invoke(() => chestshopOutput.Document.Blocks.Add(d), DispatcherPriority.Background);
                    Dispatcher.Invoke(() => chestshopOutput.ScrollToEnd(), DispatcherPriority.Background);
                } else {
                    var d = StdParser.ParseText(message);
                    Dispatcher.Invoke(() => consoleOutput.Document.Blocks.Add(d), DispatcherPriority.Background);
                    Dispatcher.Invoke(() => consoleOutput.ScrollToEnd(), DispatcherPriority.Background);
                }
            };
            new Thread(() => {
                Thread.CurrentThread.Name = "ServerStatsThread";
                try {
                    serverStats = ServerStats.ParseResponse(new WebClient().DownloadString(Connection.ServerIp));
                    Dispatcher.Invoke(PopulateServerInfo);
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                }
            }).Start();
        }

        private void CertificateMenuItemClick(object sender, RoutedEventArgs e) {
            new CertificateInfo().ShowDialog();
        }

        private void WindowClosed1(object sender, EventArgs e) {
            Environment.Exit(0);
        }

        private async void InputBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.S && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
                InputBox.AppendText("§");
            } else if (e.Key == Key.Enter) {
                if (!await Connection.Write(InputBox.Text)) {
                    MessageBox.Show("Connection lost!");
                    Environment.Exit(0);
                }
                InputBox.Clear();
            }
        }

        void PopulateServerInfo() {
            OnlinePlayerList.Items.Clear();
            PluginList.Items.Clear();

            foreach (var p in serverStats.Players) {
                OnlinePlayerList.Items.Add(p);
            }

            foreach (var p in serverStats.Plugins) {
                PluginList.Items.Add(p);
            }
        }

        public class ServerStats {
            public ServerStats(
                string motd,
                string version,
                string[] plugins,
                string map,
                string online,
                string max,
                string port,
                string[] players) {
                MOTD = motd;
                Version = version;
                Plugins = plugins;
                Map = map;
                PlayersOnline = online;
                MaxPlayers = max;
                ServerPort = port;
                Players = players;
            }

            public static ServerStats ParseResponse(string response) {
                var parts = response.Split(new[] { "\x00\x01player_\x00\x00" }, 2, StringSplitOptions.None);
                var players = parts[1].Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

                var normalparts = parts[0].Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                var keys = new Dictionary<string, string>();

                for (var i = 0; i < normalparts.Length - 1; i += 2) {
                    keys[normalparts[i]] = normalparts[i + 1];
                }
                return new ServerStats(
                    keys["hostname"],
                    keys["version"],
                    keys["plugins"].Split(';').Select(s => s.Trim()).ToArray(),
                    keys["map"],
                    keys["numplayers"],
                    keys["maxplayers"],
                    keys["hostport"],
                    players);
            }

            public static ReadOnlyDictionary<String, String> ParseKeys(string response) {
                var keys = new Dictionary<string, string>();

                var parts = response.Split(new[] { "\x00\x01player_\x00\x00" }, 2, StringSplitOptions.None);
                keys["players"] = parts[1].Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries).Aggregate("", (s, s1) => s += s1 + ",");

                var normalparts = parts[0].Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < normalparts.Length - 1; i += 2) {
                    keys[normalparts[i]] = normalparts[1];
                }
                return new ReadOnlyDictionary<string, string>(keys);
            }

            public readonly string MOTD;
            public readonly string Version;
            public readonly string[] Plugins;
            public readonly string Map;
            public readonly string PlayersOnline;
            public readonly string MaxPlayers;
            public readonly string ServerPort;
            public readonly string[] Players;
        }

        private void RefreshServerInfo(object sender, RoutedEventArgs e) {
            new Thread(async () => {
                while (await Connection.Write("###pong")) {
                    Thread.Sleep(60000);
                }
            }).Start();
            new Thread(() => {
                Thread.CurrentThread.Name = "ServerStatsThread";
                try {
                    serverStats = ServerStats.ParseResponse(new WebClient().DownloadString(Connection.ServerIp));
                    Dispatcher.Invoke(PopulateServerInfo);
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                }
            }).Start();
        }
    }
}