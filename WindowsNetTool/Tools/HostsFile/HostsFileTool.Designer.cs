namespace WindowsNetTool.Tools.HostsFile
{
	partial class HostsFileTool
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblHostsPath = new System.Windows.Forms.Label();
			this.txtHosts = new System.Windows.Forms.TextBox();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnReload = new System.Windows.Forms.Button();
			this.btnFlushDns = new System.Windows.Forms.Button();
			this.lblHostsHint = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblHostsPath
			// 
			this.lblHostsPath.AutoSize = true;
			this.lblHostsPath.Location = new System.Drawing.Point(9, 11);
			this.lblHostsPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHostsPath.Name = "lblHostsPath";
			this.lblHostsPath.Size = new System.Drawing.Size(59, 16);
			this.lblHostsPath.TabIndex = 0;
			this.lblHostsPath.Text = "hosts file";
			// 
			// txtHosts
			// 
			this.txtHosts.AcceptsTab = true;
			this.txtHosts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtHosts.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtHosts.HideSelection = false;
			this.txtHosts.Location = new System.Drawing.Point(9, 34);
			this.txtHosts.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtHosts.MaxLength = 0;
			this.txtHosts.Multiline = true;
			this.txtHosts.Name = "txtHosts";
			this.txtHosts.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtHosts.Size = new System.Drawing.Size(644, 520);
			this.txtHosts.TabIndex = 1;
			this.txtHosts.WordWrap = false;
			this.txtHosts.TextChanged += new System.EventHandler(this.txtHosts_TextChanged);
			this.txtHosts.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtHosts_KeyDown);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnSave.Location = new System.Drawing.Point(9, 562);
			this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(120, 30);
			this.btnSave.TabIndex = 2;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnReload
			// 
			this.btnReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnReload.Location = new System.Drawing.Point(137, 562);
			this.btnReload.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnReload.Name = "btnReload";
			this.btnReload.Size = new System.Drawing.Size(120, 30);
			this.btnReload.TabIndex = 3;
			this.btnReload.Text = "Reload";
			this.btnReload.UseVisualStyleBackColor = true;
			this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
			// 
			// btnFlushDns
			// 
			this.btnFlushDns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnFlushDns.Location = new System.Drawing.Point(265, 562);
			this.btnFlushDns.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnFlushDns.Name = "btnFlushDns";
			this.btnFlushDns.Size = new System.Drawing.Size(160, 30);
			this.btnFlushDns.TabIndex = 4;
			this.btnFlushDns.Text = "Flush DNS Cache";
			this.btnFlushDns.UseVisualStyleBackColor = true;
			this.btnFlushDns.Click += new System.EventHandler(this.btnFlushDns_Click);
			// 
			// lblHostsHint
			// 
			this.lblHostsHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblHostsHint.AutoSize = true;
			this.lblHostsHint.Location = new System.Drawing.Point(9, 596);
			this.lblHostsHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHostsHint.Name = "lblHostsHint";
			this.lblHostsHint.Size = new System.Drawing.Size(358, 16);
			this.lblHostsHint.TabIndex = 5;
			this.lblHostsHint.Text = "Flush the DNS cache after saving so old entries don\'t linger.";
			// 
			// HostsFileTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblHostsPath);
			this.Controls.Add(this.txtHosts);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.btnReload);
			this.Controls.Add(this.btnFlushDns);
			this.Controls.Add(this.lblHostsHint);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "HostsFileTool";
			this.Size = new System.Drawing.Size(660, 619);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblHostsPath;
		private System.Windows.Forms.TextBox txtHosts;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnReload;
		private System.Windows.Forms.Button btnFlushDns;
		private System.Windows.Forms.Label lblHostsHint;
	}
}
