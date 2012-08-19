namespace BukkitServiceWinforms {
    partial class LoginForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.hostbox = new System.Windows.Forms.TextBox();
            this.userbox = new System.Windows.Forms.TextBox();
            this.passbox = new System.Windows.Forms.TextBox();
            this.connect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.loginToolTips = new System.Windows.Forms.ToolTip(this.components);
            this.rempass = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // hostbox
            // 
            this.hostbox.Location = new System.Drawing.Point(67, 9);
            this.hostbox.Name = "hostbox";
            this.hostbox.Size = new System.Drawing.Size(193, 20);
            this.hostbox.TabIndex = 0;
            // 
            // userbox
            // 
            this.userbox.Location = new System.Drawing.Point(67, 36);
            this.userbox.Name = "userbox";
            this.userbox.Size = new System.Drawing.Size(193, 20);
            this.userbox.TabIndex = 1;
            // 
            // passbox
            // 
            this.passbox.Location = new System.Drawing.Point(67, 63);
            this.passbox.Name = "passbox";
            this.passbox.Size = new System.Drawing.Size(193, 20);
            this.passbox.TabIndex = 2;
            this.passbox.UseSystemPasswordChar = true;
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(185, 89);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(75, 23);
            this.connect.TabIndex = 3;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.ConnectClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Host";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password";
            // 
            // rempass
            // 
            this.rempass.AutoSize = true;
            this.rempass.Location = new System.Drawing.Point(54, 93);
            this.rempass.Name = "rempass";
            this.rempass.Size = new System.Drawing.Size(125, 17);
            this.rempass.TabIndex = 7;
            this.rempass.Text = "Remember password";
            this.rempass.UseVisualStyleBackColor = true;
            // 
            // LoginForm
            // 
            this.AcceptButton = this.connect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 124);
            this.Controls.Add(this.rempass);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connect);
            this.Controls.Add(this.passbox);
            this.Controls.Add(this.userbox);
            this.Controls.Add(this.hostbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.LoginFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox hostbox;
        private System.Windows.Forms.TextBox userbox;
        private System.Windows.Forms.TextBox passbox;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolTip loginToolTips;
        private System.Windows.Forms.CheckBox rempass;
    }
}

