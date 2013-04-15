using System;
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

        private void KickAllClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Warning, this will remove all players from the server! Do you want to continue?", "Yes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Connection.Write("/js fap('event.player.kickPlayer(\"\xA7dYou have beek kicked!\")')");
                MessageBox.Show("All players have been kicked!");
            } else {
                MessageBox.Show("Players have not been kicked");
            }
        }

        private void ServerLockdownEnable(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Warning, this will kick, and lock all non-staff members out of the server. Do you want to continue?", "Yes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Connection.Write("/js lockdown.lock();");
                Connection.Write("/js fap('event.player.kickPlayer(\"\xA7cSERVER ENTERING LOCK DOWN!\")')");
                MessageBox.Show("Server Entering Lockdown Mode");
            } else {
                MessageBox.Show("Server will not enter lockdown mode.");
            }
        }
        private void ServerLockdownDisable(object sender, RoutedEventArgs e) {
            Connection.Write("/js lockdown.unlock();");
            MessageBox.Show("Lockdown mode has been disabled.");
        }

        private void DisableReg(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Warning, this will disable all promotion on the server. Do you want to continue?", "Yes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Connection.Write("/js persistence.put('lockoutprom', true);");
                MessageBox.Show("Promotion has been disabled!");
            } else {
                MessageBox.Show("Promotion has not been disabled");
            }
        }

        private void EnableReg(object sender, RoutedEventArgs e) {
            Connection.Write("/js persistence.put('lockoutprom', false);");
            MessageBox.Show("Promotion has been re enabled!");
        }

        private void BroadcastButton(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrWhiteSpace(BroadCastText.Text)) {
                Connection.Write("/broadcast §d§lSERVER BROADCAST:§r§b " + BroadCastText.Text.Trim());
                BroadCastText.Text = "";
            } else {
                MessageBox.Show("Invalid input!");
            }
        }

        private void LwcClean(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("This will attempt to remove entries from the database that no longer exist. This is a safe function and should not cause any loss of protections.", "Ok?", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                Connection.Write("/lwc admin cleanup");
                MessageBox.Show("Database has been cleaned up!");
            } else {
                MessageBox.Show("Database not cleaned");
            }
        }

        //private void LockOut(object sender, RoutedEventArgs e) {
        //    if (UnlockFunctions1.Text == "cmcs17106") {
        //        UnlockFunctions1.Text = "";
        //        KickAll.IsEnabled = true;
        //        LockDownOn.IsEnabled = true;
        //        LockDownOff.IsEnabled = true;
        //        PromOff.IsEnabled = true;
        //        PromOn.IsEnabled = true;
        //        BroadcastB.IsEnabled = true;
        //        BroadCastText.IsEnabled = true;
        //        LwcCleaner.IsEnabled = true;
        //    }

        //}

        private void GbanButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(GbanUsr.Text) ||
                string.IsNullOrWhiteSpace(GbanReason.Text) || GbanUsr.Text == "Username" || GbanReason.Text == "Reason For Ban") {
                MessageBox.Show("Oops! Check to make sure you have a player AND a reason specified!");
                return;
            }

            if (MessageBox.Show("Clicking YES will permanently GLOBAL ban " + GbanUsr.Text + " from the server for " + GbanReason.Text +
                ". Do you wish to continue?", "Yes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Connection.Write("/ban g " + GbanUsr.Text + " " + GbanReason.Text);
                MessageBox.Show(GbanUsr.Text + " has been GLOBAL banned!");
                GbanUsr.Text = "";
                GbanReason.Text = "";
            }
        }

        private void GbanReason_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (GbanReason.Text == "Reason For Ban") {
                GbanReason.Text = "";
            }
        }

        private void GbanUsr_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (GbanUsr.Text == "Username") {
                GbanUsr.Text = "";
            }
        }

        private void LbanUser_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (LbanUser.Text == "Username") {
                LbanUser.Text = "";
            }
        }

        private void LbanReason_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (LbanReason.Text == "Reason For Ban") {
                LbanReason.Text = "";
            }
        }

        private void LbanButton(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(LbanUser.Text) || string.IsNullOrWhiteSpace(LbanReason.Text) || LbanUser.Text == "Username" || LbanReason.Text == "Reason For Ban") {
                MessageBox.Show("Oops! Check to make sure you have a player AND a reason specified!");
                return;
            }

            if (MessageBox.Show("Clicking YES will permanently LOCAL ban " + LbanUser.Text + " from the server for " + LbanReason.Text + ". Do you wish to continue?",
                "Yes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Connection.Write("/ban " + LbanUser.Text + LbanReason.Text);
                MessageBox.Show(LbanUser.Text + " has been LOCAL banned!");
                LbanUser.Text = "";
                LbanReason.Text = "";
            }
        }

        private void MuteName_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (MuteName.Text == "Username") {
                MuteName.Text = "";
            }
        }

        private void MuteButton(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(MuteName.Text) || MuteName.Text == "Username") {
                MessageBox.Show("Did you specify a player?");
            } else {
                Connection.Write("/mute " + MuteName.Text);
                MessageBox.Show(MuteName.Text + " has been muted!");
            }
        }

        private void PlayButton(object sender, RoutedEventArgs e) {
            if (PlayUsr.Text == "Username" || PlayCmd.Text == "Command" || string.IsNullOrWhiteSpace(PlayUsr.Text) ||
                string.IsNullOrWhiteSpace(PlayCmd.Text)) {
                MessageBox.Show("Please specify a player and a command or text!");
            } else {
                Connection.Write("/js gplr('" + PlayUsr.Text + "').chat('" + PlayCmd.Text + "');");
                PlayCmd.Text = "";
                PlayUsr.Text = "";
            }
        }

        private void AllCommandClick(object sender, RoutedEventArgs e) {
            if (AllCmd.Text == "Command" || string.IsNullOrWhiteSpace(AllCmd.Text)) {
                MessageBox.Show("Please Specify a command or text!");
            } else {
                Connection.Write("/js fap('event.player.chat(\"" + AllCmd.Text + "\")');");
                AllCmd.Text = "";
            }
        }

        private void GbanUsr_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(GbanUsr.Text)) {
                GbanUsr.Text = "Username";
            }
        }

        private void LbanUser_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(LbanUser.Text)) {
                LbanUser.Text = "Username";
            }
        }

        private void MuteName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(MuteName.Text)) {
                MuteName.Text = "Username";
            }
        }

        private void GbanReason_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(GbanReason.Text)) {
                GbanReason.Text = "Reason For Ban";
            }
        }

        private void LbanReason_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(LbanReason.Text)) {
                LbanReason.Text = "Reason For Ban";
            }
        }

        private void PlayUsr_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (PlayUsr.Text == "Username") {
                PlayUsr.Text = "";
            }
        }

        private void PlayUsr_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(PlayUsr.Text)) {
                PlayUsr.Text = "Username";
            }
        }

        private void PlayCmd_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (PlayUsr.Text == "Command") {
                PlayUsr.Text = "";
            }
        }

        private void PlayCmd_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(PlayCmd.Text)) {
                PlayCmd.Text = "Command";
            }
        }

        private void AllCmd_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (AllCmd.Text == "Command") {
                AllCmd.Text = "";
            }
        }

        private void AllCmd_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(AllCmd.Text)) {
                AllCmd.Text = "Command";
            }
        }

        private void KickButtonClick1(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrWhiteSpace(KickUsr.Text) || !string.IsNullOrWhiteSpace(KickRsn.Text)) {
                Connection.Write("/kick " + KickUsr.Text + " " + KickRsn.Text);
            } else {
                MessageBox.Show("Please specify a user and a reason.");
            }
        }

        private void KickUsr_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (KickUsr.Text == "Username") {
                KickUsr.Text = "";
            }
        }

        private void KickUsr_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(KickUsr.Text)) {
                KickUsr.Text = "Username";
            }
        }

        private void KickRsn_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (KickRsn.Text == "Reason For Kick") {
                KickRsn.Text = "";
            }
        }

        private void KickRsn_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(KickRsn.Text)) {
                KickRsn.Text = "Reason For Kick";
            }
        }

        private void TpUsr_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (TpUsr.Text == "User to teleport") {
                TpUsr.Text = "";
            }
        }

        private void TpUsr_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(TpUsr.Text)) {
                TpUsr.Text = "User to teleport";
            }
        }

        private void TpTarget_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (TpTarget.Text == "Destination") {
                TpTarget.Text = "";
            }
        }

        private void TpTarget_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(TpTarget.Text)) {
                TpTarget.Text = "Destination";
            }
        }

        private void TpButtonClick1(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrWhiteSpace(TpUsr.Text) || !string.IsNullOrWhiteSpace(TpTarget.Text)) {
                Connection.Write("/tp -s " + TpUsr.Text + " " + TpTarget.Text);
                TpUsr.Text = "Username";
                TpTarget.Text = "Destination";
            } else {
                MessageBox.Show("Please specify a target and a destination!");
            }
        }

        private void PexAddGroup(object sender, RoutedEventArgs e) {
            string group = GpermGrp.Text.ToString();
            if (!string.IsNullOrWhiteSpace(Gperm.Text)) {
                if (GpermGrp.SelectedIndex > 0) {
                    Connection.Write("./pex group " + group + " add " + Gperm.Text);
                    GpermGrp.SelectedIndex = 0;
                    Gperm.Text = "Permission Node";
                } else {
                    MessageBox.Show("Please Specify all Information!");
                }
            }
        }

        private void Gperm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (Gperm.Text == "Permission Node") {
                Gperm.Text = "";
            }
        }

        private void Gperm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(Gperm.Text)) {
                Gperm.Text = "Permission Node";
            }
        }

        private void ConPerm_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (ConPerm.Text == "Command") {
                ConPerm.Text = "";
            }
        }

        private void ConPerm_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(ConPerm.Text)) {
                ConPerm.Text = "Command";
            }
        }

        private void ConVal_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (ConVal.Text == "Permission Level (Numbers Only!)") {
                ConVal.Text = "";
            }
        }

        private void ConVal_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace(ConVal.Text)) {
                ConVal.Text = "Permission Level (Numbers Only!)";
            }
        }

        private void ConsolePermButtonClick1(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrWhiteSpace(ConPerm.Text) || !string.IsNullOrWhiteSpace(ConVal.Text)) {
                Connection.Write("/config perm nodevals." + ConPerm.Text + " " + ConVal.Text);
                MessageBox.Show("Successfully added the permission: " + ConPerm.Text + ", to the permission list with a value of: " + ConVal.Text + "!");
                ConPerm.Text = "Command";
                ConVal.Text = "Permission Level (Numbers Only!)";
            } else {
                MessageBox.Show("Error! Please make sure to specify a command, and a value!");
            }
        }
    }
}