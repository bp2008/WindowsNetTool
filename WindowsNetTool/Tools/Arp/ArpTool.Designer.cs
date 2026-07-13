namespace WindowsNetTool.Tools.Arp
{
	partial class ArpTool
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
			this.lblFilterIp = new System.Windows.Forms.Label();
			this.txtFilterIp = new System.Windows.Forms.TextBox();
			this.lblFilterMac = new System.Windows.Forms.Label();
			this.txtFilterMac = new System.Windows.Forms.TextBox();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.listArp = new System.Windows.Forms.ListView();
			this.colIp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colMac = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colInterface = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnPing = new System.Windows.Forms.Button();
			this.lblCount = new System.Windows.Forms.Label();
			this.lblHint = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblFilterIp
			// 
			this.lblFilterIp.AutoSize = true;
			this.lblFilterIp.Location = new System.Drawing.Point(9, 16);
			this.lblFilterIp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblFilterIp.Name = "lblFilterIp";
			this.lblFilterIp.Size = new System.Drawing.Size(49, 16);
			this.lblFilterIp.TabIndex = 0;
			this.lblFilterIp.Text = "IP filter:";
			// 
			// txtFilterIp
			// 
			this.txtFilterIp.Location = new System.Drawing.Point(76, 12);
			this.txtFilterIp.Margin = new System.Windows.Forms.Padding(4);
			this.txtFilterIp.Name = "txtFilterIp";
			this.txtFilterIp.Size = new System.Drawing.Size(140, 22);
			this.txtFilterIp.TabIndex = 1;
			this.txtFilterIp.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
			// 
			// lblFilterMac
			// 
			this.lblFilterMac.AutoSize = true;
			this.lblFilterMac.Location = new System.Drawing.Point(235, 16);
			this.lblFilterMac.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblFilterMac.Name = "lblFilterMac";
			this.lblFilterMac.Size = new System.Drawing.Size(66, 16);
			this.lblFilterMac.TabIndex = 2;
			this.lblFilterMac.Text = "MAC filter:";
			// 
			// txtFilterMac
			// 
			this.txtFilterMac.Location = new System.Drawing.Point(318, 12);
			this.txtFilterMac.Margin = new System.Windows.Forms.Padding(4);
			this.txtFilterMac.Name = "txtFilterMac";
			this.txtFilterMac.Size = new System.Drawing.Size(140, 22);
			this.txtFilterMac.TabIndex = 3;
			this.txtFilterMac.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
			// 
			// btnRefresh
			// 
			this.btnRefresh.Location = new System.Drawing.Point(475, 8);
			this.btnRefresh.Margin = new System.Windows.Forms.Padding(4);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(110, 30);
			this.btnRefresh.TabIndex = 4;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// listArp
			// 
			this.listArp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listArp.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIp,
            this.colMac,
            this.colInterface,
            this.colState});
			this.listArp.FullRowSelect = true;
			this.listArp.HideSelection = false;
			this.listArp.Location = new System.Drawing.Point(9, 46);
			this.listArp.Margin = new System.Windows.Forms.Padding(4);
			this.listArp.MultiSelect = false;
			this.listArp.Name = "listArp";
			this.listArp.Size = new System.Drawing.Size(649, 470);
			this.listArp.TabIndex = 5;
			this.listArp.UseCompatibleStateImageBehavior = false;
			this.listArp.View = System.Windows.Forms.View.Details;
			this.listArp.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listArp_ColumnClick);
			this.listArp.ItemActivate += new System.EventHandler(this.listArp_ItemActivate);
			// 
			// colIp
			// 
			this.colIp.Text = "IP Address";
			this.colIp.Width = 135;
			// 
			// colMac
			// 
			this.colMac.Text = "MAC Address";
			this.colMac.Width = 150;
			// 
			// colInterface
			// 
			this.colInterface.Text = "Interface";
			this.colInterface.Width = 205;
			// 
			// colState
			// 
			this.colState.Text = "State";
			this.colState.Width = 110;
			// 
			// btnPing
			// 
			this.btnPing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnPing.Location = new System.Drawing.Point(9, 524);
			this.btnPing.Margin = new System.Windows.Forms.Padding(4);
			this.btnPing.Name = "btnPing";
			this.btnPing.Size = new System.Drawing.Size(140, 30);
			this.btnPing.TabIndex = 6;
			this.btnPing.Text = "Ping Selected";
			this.btnPing.UseVisualStyleBackColor = true;
			this.btnPing.Click += new System.EventHandler(this.btnPing_Click);
			//
			// lblCount
			// 
			this.lblCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblCount.AutoSize = true;
			this.lblCount.Location = new System.Drawing.Point(157, 531);
			this.lblCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblCount.Name = "lblCount";
			this.lblCount.Size = new System.Drawing.Size(0, 16);
			this.lblCount.TabIndex = 7;
			// 
			// lblHint
			// 
			this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblHint.Location = new System.Drawing.Point(6, 562);
			this.lblHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHint.Name = "lblHint";
			this.lblHint.Size = new System.Drawing.Size(652, 32);
			this.lblHint.TabIndex = 8;
			this.lblHint.Text = "Filters match any part of an address; the MAC filter ignores separators.  Double-" +
    "click an entry to begin monitoring it in the Ping tool.";
			// 
			// ArpTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblFilterIp);
			this.Controls.Add(this.txtFilterIp);
			this.Controls.Add(this.lblFilterMac);
			this.Controls.Add(this.txtFilterMac);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.listArp);
			this.Controls.Add(this.btnPing);
			this.Controls.Add(this.lblCount);
			this.Controls.Add(this.lblHint);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(600, 340);
			this.Name = "ArpTool";
			this.Size = new System.Drawing.Size(665, 611);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblFilterIp;
		private System.Windows.Forms.TextBox txtFilterIp;
		private System.Windows.Forms.Label lblFilterMac;
		private System.Windows.Forms.TextBox txtFilterMac;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.ListView listArp;
		private System.Windows.Forms.ColumnHeader colIp;
		private System.Windows.Forms.ColumnHeader colMac;
		private System.Windows.Forms.ColumnHeader colInterface;
		private System.Windows.Forms.ColumnHeader colState;
		private System.Windows.Forms.Button btnPing;
		private System.Windows.Forms.Label lblCount;
		private System.Windows.Forms.Label lblHint;
	}
}
