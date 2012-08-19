namespace BukkitServiceWinforms {
    partial class ConsoleForm {
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
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.outputbox = new System.Windows.Forms.RichTextBox();
            this.inputbox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.errorout = new System.Windows.Forms.RichTextBox();
            this.tabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabPage1);
            this.tabs.Controls.Add(this.tabPage2);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(705, 543);
            this.tabs.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.outputbox);
            this.tabPage1.Controls.Add(this.inputbox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(697, 517);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Console";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // outputbox
            // 
            this.outputbox.BackColor = System.Drawing.Color.White;
            this.outputbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputbox.Location = new System.Drawing.Point(3, 3);
            this.outputbox.Name = "outputbox";
            this.outputbox.ReadOnly = true;
            this.outputbox.Size = new System.Drawing.Size(691, 491);
            this.outputbox.TabIndex = 1;
            this.outputbox.Text = "";
            // 
            // inputbox
            // 
            this.inputbox.AcceptsReturn = true;
            this.inputbox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.inputbox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.inputbox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputbox.Location = new System.Drawing.Point(3, 494);
            this.inputbox.Name = "inputbox";
            this.inputbox.Size = new System.Drawing.Size(691, 20);
            this.inputbox.TabIndex = 0;
            this.inputbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputbox_KeyDown);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.errorout);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(697, 517);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Errors";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // errorout
            // 
            this.errorout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorout.Location = new System.Drawing.Point(3, 3);
            this.errorout.Name = "errorout";
            this.errorout.ReadOnly = true;
            this.errorout.Size = new System.Drawing.Size(691, 511);
            this.errorout.TabIndex = 0;
            this.errorout.Text = "";
            // 
            // ConsoleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(705, 543);
            this.Controls.Add(this.tabs);
            this.Name = "ConsoleForm";
            this.Text = "ConsoleForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConsoleForm_FormClosing);
            this.tabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox inputbox;
        private System.Windows.Forms.RichTextBox outputbox;
        private System.Windows.Forms.RichTextBox errorout;
    }
}