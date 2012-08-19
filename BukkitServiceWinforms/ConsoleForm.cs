using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BukkitServiceWinforms {
    
    public partial class ConsoleForm : Form {
        static readonly Regex SevereRegex = new Regex("^\\d\\d\\:\\d\\d\\:\\d\\d [SEVERE]");

        public ConsoleForm() {
            InitializeComponent();
            LoginForm.Stream.BeginRead((sender, message) => {
                if (message == null) {
                    LoginForm.Stream.EndRead();
                    Invoke(new Action(() => MessageBox.Show("Connection lost")));
                    Close();
                    return;
                }
                if (SevereRegex.IsMatch(message)) {
                    BeginInvoke(new Action(() => Append(errorout, message)));
                    return;
                }
                BeginInvoke(new Action(() => Append(outputbox, message)));
            });
        }

        void Append(RichTextBox rtb, string message) {
            rtb.AppendText(message);
            if (rtb.TextLength > 2048) {
                rtb.Text = rtb.Text.Substring(rtb.TextLength - 2048);
            }
            rtb.SelectionStart = rtb.TextLength;
            rtb.ScrollToCaret();
        }

        private void ConsoleForm_FormClosing(object sender, FormClosingEventArgs e) {
            LoginForm.Stream.EndRead();
        }

        private void inputbox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode != Keys.Enter) return;

            var msg = inputbox.Text;
            inputbox.Text = "";

            if (!inputbox.AutoCompleteCustomSource.Contains(msg))
                inputbox.AutoCompleteCustomSource.Insert(0, msg);
            else {
                inputbox.AutoCompleteCustomSource.Remove(msg);
                inputbox.AutoCompleteCustomSource.Insert(0, msg);
            }

            LoginForm.Stream.WriteAsync(msg);
        }
    }
}
