namespace MusicSorter2
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.StartBut = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.FolderBox = new System.Windows.Forms.TextBox();
            this.FolderLabel = new System.Windows.Forms.Label();
            this.BrowseBut = new System.Windows.Forms.Button();
            this.ModeComboBox = new System.Windows.Forms.ComboBox();
            this.ModeLabel = new System.Windows.Forms.Label();
            this.UpdatesLabel = new System.Windows.Forms.Label();
            this.RenameCheck = new System.Windows.Forms.CheckBox();
            this.CreatedCheck = new System.Windows.Forms.CheckBox();
            this.MovedCheck = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.FormatComboBox = new System.Windows.Forms.ComboBox();
            this.HelpLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // StartBut
            // 
            this.StartBut.Location = new System.Drawing.Point(69, 191);
            this.StartBut.Name = "StartBut";
            this.StartBut.Size = new System.Drawing.Size(231, 49);
            this.StartBut.TabIndex = 0;
            this.StartBut.Text = "Start";
            this.StartBut.UseVisualStyleBackColor = true;
            this.StartBut.Click += new System.EventHandler(this.StartBut_Click);
            // 
            // FolderBox
            // 
            this.FolderBox.Location = new System.Drawing.Point(122, 13);
            this.FolderBox.Name = "FolderBox";
            this.FolderBox.Size = new System.Drawing.Size(234, 20);
            this.FolderBox.TabIndex = 1;
            this.FolderBox.Text = "C:\\Music\\";
            // 
            // FolderLabel
            // 
            this.FolderLabel.AutoSize = true;
            this.FolderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FolderLabel.Location = new System.Drawing.Point(66, 15);
            this.FolderLabel.Name = "FolderLabel";
            this.FolderLabel.Size = new System.Drawing.Size(50, 16);
            this.FolderLabel.TabIndex = 2;
            this.FolderLabel.Text = "Folder:";
            // 
            // BrowseBut
            // 
            this.BrowseBut.Location = new System.Drawing.Point(304, 39);
            this.BrowseBut.Name = "BrowseBut";
            this.BrowseBut.Size = new System.Drawing.Size(52, 21);
            this.BrowseBut.TabIndex = 3;
            this.BrowseBut.Text = "Browse";
            this.BrowseBut.UseVisualStyleBackColor = true;
            this.BrowseBut.Click += new System.EventHandler(this.BrowseBut_Click);
            // 
            // ModeComboBox
            // 
            this.ModeComboBox.FormattingEnabled = true;
            this.ModeComboBox.Location = new System.Drawing.Point(122, 49);
            this.ModeComboBox.Name = "ModeComboBox";
            this.ModeComboBox.Size = new System.Drawing.Size(63, 21);
            this.ModeComboBox.TabIndex = 4;
            // 
            // ModeLabel
            // 
            this.ModeLabel.AutoSize = true;
            this.ModeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModeLabel.Location = new System.Drawing.Point(34, 50);
            this.ModeLabel.Name = "ModeLabel";
            this.ModeLabel.Size = new System.Drawing.Size(82, 16);
            this.ModeLabel.TabIndex = 5;
            this.ModeLabel.Text = "Type of sort:";
            // 
            // UpdatesLabel
            // 
            this.UpdatesLabel.AutoSize = true;
            this.UpdatesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpdatesLabel.Location = new System.Drawing.Point(20, 121);
            this.UpdatesLabel.Name = "UpdatesLabel";
            this.UpdatesLabel.Size = new System.Drawing.Size(96, 16);
            this.UpdatesLabel.TabIndex = 6;
            this.UpdatesLabel.Text = "Show updates:";
            // 
            // RenameCheck
            // 
            this.RenameCheck.AutoSize = true;
            this.RenameCheck.Location = new System.Drawing.Point(122, 122);
            this.RenameCheck.Name = "RenameCheck";
            this.RenameCheck.Size = new System.Drawing.Size(86, 17);
            this.RenameCheck.TabIndex = 7;
            this.RenameCheck.Text = "File renamed";
            this.RenameCheck.UseVisualStyleBackColor = true;
            // 
            // CreatedCheck
            // 
            this.CreatedCheck.AutoSize = true;
            this.CreatedCheck.Location = new System.Drawing.Point(122, 145);
            this.CreatedCheck.Name = "CreatedCheck";
            this.CreatedCheck.Size = new System.Drawing.Size(94, 17);
            this.CreatedCheck.TabIndex = 7;
            this.CreatedCheck.Text = "Folder created";
            this.CreatedCheck.UseVisualStyleBackColor = true;
            // 
            // MovedCheck
            // 
            this.MovedCheck.AutoSize = true;
            this.MovedCheck.Location = new System.Drawing.Point(122, 168);
            this.MovedCheck.Name = "MovedCheck";
            this.MovedCheck.Size = new System.Drawing.Size(77, 17);
            this.MovedCheck.TabIndex = 7;
            this.MovedCheck.Text = "File moved";
            this.MovedCheck.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "Rename format:";
            // 
            // FormatComboBox
            // 
            this.FormatComboBox.FormattingEnabled = true;
            this.FormatComboBox.Items.AddRange(new object[] {
            "{#}. {T}",
            "{T}",
            "{AR} {AL} {#} {T}"});
            this.FormatComboBox.Location = new System.Drawing.Point(122, 85);
            this.FormatComboBox.Name = "FormatComboBox";
            this.FormatComboBox.Size = new System.Drawing.Size(111, 21);
            this.FormatComboBox.TabIndex = 8;
            // 
            // HelpLabel
            // 
            this.HelpLabel.AutoSize = true;
            this.HelpLabel.Location = new System.Drawing.Point(239, 89);
            this.HelpLabel.Name = "HelpLabel";
            this.HelpLabel.Size = new System.Drawing.Size(32, 13);
            this.HelpLabel.TabIndex = 9;
            this.HelpLabel.TabStop = true;
            this.HelpLabel.Text = "Help!";
            this.HelpLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HelpLabel_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 249);
            this.Controls.Add(this.HelpLabel);
            this.Controls.Add(this.FormatComboBox);
            this.Controls.Add(this.CreatedCheck);
            this.Controls.Add(this.MovedCheck);
            this.Controls.Add(this.RenameCheck);
            this.Controls.Add(this.UpdatesLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ModeLabel);
            this.Controls.Add(this.ModeComboBox);
            this.Controls.Add(this.BrowseBut);
            this.Controls.Add(this.FolderLabel);
            this.Controls.Add(this.FolderBox);
            this.Controls.Add(this.StartBut);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Music Sorter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartBut;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TextBox FolderBox;
        private System.Windows.Forms.Label FolderLabel;
        private System.Windows.Forms.Button BrowseBut;
        private System.Windows.Forms.ComboBox ModeComboBox;
        private System.Windows.Forms.Label ModeLabel;
        private System.Windows.Forms.Label UpdatesLabel;
        private System.Windows.Forms.CheckBox RenameCheck;
        private System.Windows.Forms.CheckBox CreatedCheck;
        private System.Windows.Forms.CheckBox MovedCheck;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FormatComboBox;
        private System.Windows.Forms.LinkLabel HelpLabel;
    }
}

