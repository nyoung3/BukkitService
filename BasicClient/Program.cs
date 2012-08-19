using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using ConnorsNetworkingSuite;
using System.Linq;
using SslStream = ConnorsNetworkingSuite.SslStream;

namespace BasicClient {
    public static class Program {

        public static void Main() {
            try {
                DoWork();
            } catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("An error has occured in the client");
                Console.WriteLine(e);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void DoWork() {
            #region Initial Formatting

            // ReSharper disable ImplicitlyCapturedClosure
            // ReSharper disable AccessToModifiedClosure
            // ReSharper disable UnusedAnonymousMethodSignature
            try {
                Console.BufferWidth = 120;
                Console.WindowWidth = 120;
                Console.WindowHeight = 40;
            } catch {
                Console.WriteLine("Can't change width D:");
            }

            #endregion

            #region Connect

        gethost:
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Host: ");
            Console.ForegroundColor = ConsoleColor.White;
            var host = Console.ReadLine();

        getport:
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Port: ");
            Console.ForegroundColor = ConsoleColor.White;
            var portstr = Console.ReadLine();
            ushort port;
            if (!ushort.TryParse(portstr, out port)) {
                Console.WriteLine("That is not a valid port.");
                goto getport;
            }
            var tc = false;
        connect:
            NetStream stream;
            try {
                stream = SimpleStream.ConnectToHost(host, port);
            } catch {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Could not connect to host.");
                goto gethost;
            }

            var ssl = new SslStream(stream);
            try {
                ssl.AuthenticateAsClient(host,
                                     (sender, certificate, chain, errors) => {
                                         if (errors != SslPolicyErrors.None) {
                                             Console.ForegroundColor = ConsoleColor.Red;
                                             Console.WriteLine("There are problems with security certificate");
                                             Console.ForegroundColor = ConsoleColor.Yellow;
                                             if (errors == SslPolicyErrors.RemoteCertificateNameMismatch) {
                                                 Console.WriteLine("The name on the certificate (" + GetCN(certificate.Subject) +
                                                                   ") does not match the hostname entered (" + host + ")");
                                             } else {
                                                 Console.WriteLine(errors);
                                             }
                                         doproceed:
                                             Console.ForegroundColor = ConsoleColor.Cyan;
                                             Console.Write("Proceed? [y/n] ");
                                             Console.ForegroundColor = ConsoleColor.White;
                                             var key = Console.ReadKey();
                                             Console.WriteLine();
                                             switch (key.Key) {
                                                 case ConsoleKey.Y:
                                                     return true;
                                                 case ConsoleKey.N:
                                                     return false;
                                                 default:
                                                     Console.WriteLine("Please enter Y or N");
                                                     goto doproceed;
                                             }
                                         }
                                         return true;
                                     });
            } catch (AuthenticationException e) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Could not authenticate. {0}", e.Message);
                return;
            }
            stream = ssl;

            // ReSharper disable HeuristicUnreachableCode
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (!tc) {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("-----SSL Security info-----");
                Console.ForegroundColor = ConsoleColor.Yellow;
                DisplaySecurityLevel((System.Net.Security.SslStream)stream.UnderlyingStream);
                DisplaySecurityServices((System.Net.Security.SslStream)stream.UnderlyingStream);
            }
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            tc = true;

            stream.Encoding = Encoding.ASCII;
            stream.Write("utf-8");
            stream.Encoding = Encoding.UTF8;
            stream.Read();

            bool[] mancon = { true };

            #endregion

            #region Enter User Info

            var streamclosemre = new ManualResetEvent(false);
            Thread readthread;
            Thread writethread = null;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("User: ");
            Console.ForegroundColor = ConsoleColor.White;
            var user = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Pass: ");
            Console.ForegroundColor = ConsoleColor.White;
            var pass = ReadPassword();
            if (!stream.Write(user + ':' + pass)) {
                goto cleanup;
            }
            var cred_result = stream.Read();
            if (cred_result == null) {
                goto cleanup;
            }
            if (cred_result.StartsWith("ERR")) {
                if (cred_result == "ERR_AUTH_INVALID_CREDENTIALS") {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Incorrect username or password.");
                    goto connect;
                }
                if (cred_result == "ERR_ACCOUNT_NOT_AUTHORIZED") {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("That account is not authorized to connect.");
                    goto connect;
                }

                goto cleanup;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Login successful!");
            Console.ForegroundColor = ConsoleColor.Gray;

            #endregion

            #region Loop

            (readthread = new Thread(
                              delegate() {
                                  while (mancon[0] && stream.Connected) {
                                      var msg = stream.Read();
                                      if (msg == null) {
                                          mancon[0] = false;
                                          streamclosemre.Set();
                                          if (writethread != null && writethread.IsAlive) {
                                              writethread.Abort();
                                          }
                                          return;
                                      }
                                      if (msg.StartsWith("$")) msg = msg.Substring(1);
                                      WriteAnsiString(msg);
                                  }
                              })).Start();
            (writethread = new Thread(
                               delegate() {
                                   Console.TreatControlCAsInput = false;
                                   Console.CancelKeyPress +=
                                       (sender, args) => {
                                           mancon[0] = false;
                                           streamclosemre.Set();
                                           if (readthread != null && readthread.IsAlive) {
                                               readthread.Abort();
                                           }
                                           writethread.Abort();
                                           args.Cancel = true;
                                       };
                                   while (stream.Connected && mancon[0]) {
                                       var m2S = Console.ReadLine();
                                       if (string.IsNullOrWhiteSpace(m2S)) continue;
                                       if (stream.Write(m2S)) continue;
                                       mancon[0] = false;
                                       streamclosemre.Set();
                                       if (readthread != null && readthread.IsAlive) {
                                           readthread.Abort();
                                       }
                                   }
                               })).Start();
            // ReSharper restore ImplicitlyCapturedClosure
            // ReSharper restore AccessToModifiedClosure
            // ReSharper restore UnusedAnonymousMethodSignature

            #endregion

            #region Cleanup

            streamclosemre.WaitOne();

        cleanup:
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\r\nCONNECTION LOST\r\n");

            #endregion
        }

        #region Read a password while displaying hidden characters

        private static string ReadPassword() {
            var characters = new Stack<char>();
            for (var cki = Console.ReadKey(true); cki.Key != ConsoleKey.Enter; cki = Console.ReadKey(true)) {
                if (cki.Key == ConsoleKey.Backspace) {
                    if (characters.Count < 1) continue;
                    Console.Write("\b \b");
                    characters.Pop();
                } else {
                    Console.Write('*');
                    characters.Push(cki.KeyChar);
                }
            }
            Console.WriteLine("");
            var array = characters.ToArray();
            Array.Reverse(array);
            return new string(array);
        }

        #endregion

        #region Write to console with ANSI parsing

        private static void WriteAnsiString(string input) {
            var chars = input.ToCharArray();
            Array.Reverse(chars);
            var stack = new Stack<char>(chars);
            while (stack.Count > 0) {
                var c = stack.Pop();
                if (c == '\u001B') {
                    var num = "";
                    if (stack.Count > 0) stack.Pop();
                    while (stack.Count > 0 && char.IsNumber(c = stack.Pop())) {
                        num += c;
                    }
                    int nac;
                    if (int.TryParse(num, out nac)) {
                        GetCc(nac);
                    }
                } else {
                    Console.Write(c);
                }
            }
        }

        private static void GetCc(int code) {
            switch (code) {
                case 0:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case 30:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case 31:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case 32:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case 33:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case 34:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case 35:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case 36:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case 37:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 39:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }

        #endregion

        #region Display Certificate info


        static void DisplaySecurityLevel(System.Net.Security.SslStream stream) {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(System.Net.Security.SslStream stream) {
            Console.WriteLine("Is authenticated: {0}", stream.IsAuthenticated);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }

        static string GetCN(string CertSubject) {
            return CertSubject.Split(',').Select(s => s.Trim()).First(s => s.Split('=')[0].Equals("CN")).Split('=')[1].Trim();
        }

        #endregion
    }
}