namespace WindowsNetTool.Tools.Traceroute
{
	partial class TracerouteTool
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
			this.lblTarget = new System.Windows.Forms.Label();
			this.txtTarget = new System.Windows.Forms.TextBox();
			this.chkPreferIpv4 = new System.Windows.Forms.CheckBox();
			this.btnStartStop = new System.Windows.Forms.Button();
			this.listHops = new WindowsNetTool.DoubleBufferedListView();
			this.colHop = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colRtt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colHostname = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblHint = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblTarget
			// 
			this.lblTarget.AutoSize = true;
			this.lblTarget.Location = new System.Drawing.Point(9, 16);
			this.lblTarget.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblTarget.Name = "lblTarget";
			this.lblTarget.Size = new System.Drawing.Size(50, 16);
			this.lblTarget.TabIndex = 0;
			this.lblTarget.Text = "Target:";
			// 
			// txtTarget
			// 
			this.txtTarget.Location = new System.Drawing.Point(70, 12);
			this.txtTarget.Margin = new System.Windows.Forms.Padding(4);
			this.txtTarget.Name = "txtTarget";
			this.txtTarget.Size = new System.Drawing.Size(230, 22);
			this.txtTarget.TabIndex = 1;
			this.txtTarget.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtTarget_KeyDown);
			// 
			// chkPreferIpv4
			// 
			this.chkPreferIpv4.AutoSize = true;
			this.chkPreferIpv4.Checked = true;
			this.chkPreferIpv4.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkPreferIpv4.Location = new System.Drawing.Point(312, 14);
			this.chkPreferIpv4.Margin = new System.Windows.Forms.Padding(4);
			this.chkPreferIpv4.Name = "chkPreferIpv4";
			this.chkPreferIpv4.Size = new System.Drawing.Size(94, 20);
			this.chkPreferIpv4.TabIndex = 2;
			this.chkPreferIpv4.Text = "Prefer IPv4";
			this.chkPreferIpv4.UseVisualStyleBackColor = true;
			// 
			// btnStartStop
			// 
			this.btnStartStop.Location = new System.Drawing.Point(424, 8);
			this.btnStartStop.Margin = new System.Windows.Forms.Padding(4);
			this.btnStartStop.Name = "btnStartStop";
			this.btnStartStop.Size = new System.Drawing.Size(110, 30);
			this.btnStartStop.TabIndex = 3;
			this.btnStartStop.Text = "Start";
			this.btnStartStop.UseVisualStyleBackColor = true;
			this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
			//
			// listHops
			// 
			this.listHops.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listHops.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHop,
            this.colRtt,
            this.colAddress,
            this.colHostname});
			this.listHops.FullRowSelect = true;
			this.listHops.HideSelection = false;
			this.listHops.Location = new System.Drawing.Point(9, 46);
			this.listHops.Margin = new System.Windows.Forms.Padding(4);
			this.listHops.MultiSelect = false;
			this.listHops.Name = "listHops";
			this.listHops.Size = new System.Drawing.Size(649, 498);
			this.listHops.TabIndex = 4;
			this.listHops.UseCompatibleStateImageBehavior = false;
			this.listHops.View = System.Windows.Forms.View.Details;
			this.listHops.ItemActivate += new System.EventHandler(this.listHops_ItemActivate);
			// 
			// colHop
			// 
			this.colHop.Text = "Hop";
			this.colHop.Width = 45;
			// 
			// colRtt
			// 
			this.colRtt.Text = "RTT";
			this.colRtt.Width = 70;
			// 
			// colAddress
			// 
			this.colAddress.Text = "IP Address";
			this.colAddress.Width = 160;
			// 
			// colHostname
			// 
			this.colHostname.Text = "Host Name";
			this.colHostname.Width = 330;
			// 
			// lblStatus
			// 
			this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblStatus.Location = new System.Drawing.Point(9, 554);
			this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(649, 16);
			this.lblStatus.TabIndex = 5;
			// 
			// lblHint
			// 
			this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblHint.Location = new System.Drawing.Point(9, 577);
			this.lblHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHint.Name = "lblHint";
			this.lblHint.Size = new System.Drawing.Size(652, 32);
			this.lblHint.TabIndex = 6;
			this.lblHint.Text = "Every hop is probed concurrently, so the full route is discovered rapidly instead" +
    " of gradually hop-by-hop.  Double-click a hop to begin monitoring it in the Ping" +
    " tool.";
			// 
			// TracerouteTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblTarget);
			this.Controls.Add(this.txtTarget);
			this.Controls.Add(this.chkPreferIpv4);
			this.Controls.Add(this.btnStartStop);
			this.Controls.Add(this.listHops);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.lblHint);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(560, 340);
			this.Name = "TracerouteTool";
			this.Size = new System.Drawing.Size(665, 616);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblTarget;
		private System.Windows.Forms.TextBox txtTarget;
		private System.Windows.Forms.CheckBox chkPreferIpv4;
		private System.Windows.Forms.Button btnStartStop;
		private WindowsNetTool.DoubleBufferedListView listHops;
		private System.Windows.Forms.ColumnHeader colHop;
		private System.Windows.Forms.ColumnHeader colRtt;
		private System.Windows.Forms.ColumnHeader colAddress;
		private System.Windows.Forms.ColumnHeader colHostname;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label lblHint;
	}
}
