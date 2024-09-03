namespace MariaDB_Backup_and_Verifications
{
    partial class Form1
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
            this.btnLocalBackup = new System.Windows.Forms.Button();
            this.btnRemoteBackup = new System.Windows.Forms.Button();
            this.btnVerifyBackups = new System.Windows.Forms.Button();
            this.btnRestoreBackups = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLocalBackup
            // 
            this.btnLocalBackup.Location = new System.Drawing.Point(51, 92);
            this.btnLocalBackup.Name = "btnLocalBackup";
            this.btnLocalBackup.Size = new System.Drawing.Size(130, 58);
            this.btnLocalBackup.TabIndex = 0;
            this.btnLocalBackup.Text = "Local Backup";
            this.btnLocalBackup.UseVisualStyleBackColor = true;
            this.btnLocalBackup.Click += new System.EventHandler(this.btnLocalBackup_Click);
            // 
            // btnRemoteBackup
            // 
            this.btnRemoteBackup.Location = new System.Drawing.Point(383, 92);
            this.btnRemoteBackup.Name = "btnRemoteBackup";
            this.btnRemoteBackup.Size = new System.Drawing.Size(130, 58);
            this.btnRemoteBackup.TabIndex = 1;
            this.btnRemoteBackup.Text = "Remote Backup";
            this.btnRemoteBackup.UseVisualStyleBackColor = true;
            this.btnRemoteBackup.Click += new System.EventHandler(this.btnRemoteBackup_Click);
            // 
            // btnVerifyBackups
            // 
            this.btnVerifyBackups.Location = new System.Drawing.Point(227, 23);
            this.btnVerifyBackups.Name = "btnVerifyBackups";
            this.btnVerifyBackups.Size = new System.Drawing.Size(130, 58);
            this.btnVerifyBackups.TabIndex = 2;
            this.btnVerifyBackups.Text = "Verify Backups";
            this.btnVerifyBackups.UseVisualStyleBackColor = true;
            this.btnVerifyBackups.Click += new System.EventHandler(this.btnVerifyBackups_Click);
            // 
            // btnRestoreBackups
            // 
            this.btnRestoreBackups.Enabled = false;
            this.btnRestoreBackups.Location = new System.Drawing.Point(227, 120);
            this.btnRestoreBackups.Name = "btnRestoreBackups";
            this.btnRestoreBackups.Size = new System.Drawing.Size(130, 58);
            this.btnRestoreBackups.TabIndex = 3;
            this.btnRestoreBackups.Text = "Restore Backups";
            this.btnRestoreBackups.UseVisualStyleBackColor = true;
            this.btnRestoreBackups.Click += new System.EventHandler(this.btnRestoreBackups_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 218);
            this.Controls.Add(this.btnRestoreBackups);
            this.Controls.Add(this.btnVerifyBackups);
            this.Controls.Add(this.btnRemoteBackup);
            this.Controls.Add(this.btnLocalBackup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Maria DB Backup and Verifications";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLocalBackup;
        private System.Windows.Forms.Button btnRemoteBackup;
        private System.Windows.Forms.Button btnVerifyBackups;
        private System.Windows.Forms.Button btnRestoreBackups;
    }
}

