namespace WindowsNetTool.Tools.DeviceList
{
	partial class DeviceListTool
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
			this.components = new System.ComponentModel.Container();
			this.lblSubnet = new System.Windows.Forms.Label();
			this.comboSubnet = new System.Windows.Forms.ComboBox();
			this.btnStartStop = new System.Windows.Forms.Button();
			this.lblFilter = new System.Windows.Forms.Label();
			this.txtFilter = new System.Windows.Forms.TextBox();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.listDevices = new WindowsNetTool.DoubleBufferedListView();
			this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colIpv4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colIpv6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colMac = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPing = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colLastReply = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.txtDetails = new System.Windows.Forms.TextBox();
			this.btnPing = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblHint = new System.Windows.Forms.Label();
			this.timerFlush = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblSubnet
			// 
			this.lblSubnet.AutoSize = true;
			this.lblSubnet.Location = new System.Drawing.Point(9, 16);
			this.lblSubnet.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSubnet.Name = "lblSubnet";
			this.lblSubnet.Size = new System.Drawing.Size(52, 16);
			this.lblSubnet.TabIndex = 0;
			this.lblSubnet.Text = "Subnet:";
			// 
			// comboSubnet
			// 
			this.comboSubnet.FormattingEnabled = true;
			this.comboSubnet.Location = new System.Drawing.Point(76, 12);
			this.comboSubnet.Margin = new System.Windows.Forms.Padding(4);
			this.comboSubnet.Name = "comboSubnet";
			this.comboSubnet.Size = new System.Drawing.Size(210, 24);
			this.comboSubnet.TabIndex = 1;
			this.comboSubnet.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboSubnet_KeyDown);
			// 
			// btnStartStop
			// 
			this.btnStartStop.Location = new System.Drawing.Point(298, 8);
			this.btnStartStop.Margin = new System.Windows.Forms.Padding(4);
			this.btnStartStop.Name = "btnStartStop";
			this.btnStartStop.Size = new System.Drawing.Size(90, 30);
			this.btnStartStop.TabIndex = 2;
			this.btnStartStop.Text = "Start";
			this.btnStartStop.UseVisualStyleBackColor = true;
			this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
			// 
			// lblFilter
			// 
			this.lblFilter.AutoSize = true;
			this.lblFilter.Location = new System.Drawing.Point(404, 16);
			this.lblFilter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblFilter.Name = "lblFilter";
			this.lblFilter.Size = new System.Drawing.Size(39, 16);
			this.lblFilter.TabIndex = 3;
			this.lblFilter.Text = "Filter:";
			// 
			// txtFilter
			// 
			this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilter.Location = new System.Drawing.Point(452, 12);
			this.txtFilter.Margin = new System.Windows.Forms.Padding(4);
			this.txtFilter.Name = "txtFilter";
			this.txtFilter.Size = new System.Drawing.Size(206, 22);
			this.txtFilter.TabIndex = 4;
			this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer.Location = new System.Drawing.Point(9, 46);
			this.splitContainer.Margin = new System.Windows.Forms.Padding(4);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.listDevices);
			this.splitContainer.Panel1MinSize = 80;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.txtDetails);
			this.splitContainer.Panel2MinSize = 60;
			this.splitContainer.Size = new System.Drawing.Size(649, 481);
			this.splitContainer.SplitterDistance = 325;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.TabIndex = 5;
			// 
			// listDevices
			// 
			this.listDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colIpv4,
            this.colIpv6,
            this.colMac,
            this.colPing,
            this.colLastReply,
            this.colInfo});
			this.listDevices.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listDevices.FullRowSelect = true;
			this.listDevices.HideSelection = false;
			this.listDevices.Location = new System.Drawing.Point(0, 0);
			this.listDevices.Margin = new System.Windows.Forms.Padding(4);
			this.listDevices.MultiSelect = false;
			this.listDevices.Name = "listDevices";
			this.listDevices.Size = new System.Drawing.Size(649, 325);
			this.listDevices.TabIndex = 0;
			this.listDevices.UseCompatibleStateImageBehavior = false;
			this.listDevices.View = System.Windows.Forms.View.Details;
			this.listDevices.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listDevices_ColumnClick);
			this.listDevices.ItemActivate += new System.EventHandler(this.listDevices_ItemActivate);
			this.listDevices.SelectedIndexChanged += new System.EventHandler(this.listDevices_SelectedIndexChanged);
			// 
			// colName
			// 
			this.colName.Text = "Name";
			this.colName.Width = 160;
			// 
			// colIpv4
			// 
			this.colIpv4.Text = "IPv4 Address";
			this.colIpv4.Width = 125;
			// 
			// colIpv6
			// 
			this.colIpv6.Text = "IPv6 Address";
			this.colIpv6.Width = 165;
			// 
			// colMac
			// 
			this.colMac.Text = "MAC Address";
			this.colMac.Width = 130;
			// 
			// colPing
			// 
			this.colPing.Text = "Ping";
			this.colPing.Width = 65;
			// 
			// colLastReply
			// 
			this.colLastReply.Text = "Last Reply";
			this.colLastReply.Width = 85;
			// 
			// colInfo
			// 
			this.colInfo.Text = "Info";
			this.colInfo.Width = 105;
			// 
			// txtDetails
			// 
			this.txtDetails.BackColor = System.Drawing.SystemColors.Window;
			this.txtDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtDetails.Font = new System.Drawing.Font("Consolas", 9F);
			this.txtDetails.Location = new System.Drawing.Point(0, 0);
			this.txtDetails.Margin = new System.Windows.Forms.Padding(4);
			this.txtDetails.Multiline = true;
			this.txtDetails.Name = "txtDetails";
			this.txtDetails.ReadOnly = true;
			this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtDetails.Size = new System.Drawing.Size(649, 151);
			this.txtDetails.TabIndex = 0;
			this.txtDetails.WordWrap = false;
			// 
			// btnPing
			// 
			this.btnPing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnPing.Location = new System.Drawing.Point(9, 535);
			this.btnPing.Margin = new System.Windows.Forms.Padding(4);
			this.btnPing.Name = "btnPing";
			this.btnPing.Size = new System.Drawing.Size(140, 30);
			this.btnPing.TabIndex = 6;
			this.btnPing.Text = "Ping Selected";
			this.btnPing.UseVisualStyleBackColor = true;
			this.btnPing.Click += new System.EventHandler(this.btnPing_Click);
			// 
			// lblStatus
			// 
			this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(157, 531);
			this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(0, 16);
			this.lblStatus.TabIndex = 7;
			// 
			// lblHint
			// 
			this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblHint.Location = new System.Drawing.Point(6, 572);
			this.lblHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHint.Name = "lblHint";
			this.lblHint.Size = new System.Drawing.Size(652, 32);
			this.lblHint.TabIndex = 8;
			this.lblHint.Text = "Combines ping scanning with the ARP and NDP tables to build one row per device.  " +
    "Select a device to see all of its addresses; double-click to ping it.";
			// 
			// timerFlush
			// 
			this.timerFlush.Interval = 250;
			this.timerFlush.Tick += new System.EventHandler(this.timerFlush_Tick);
			// 
			// DeviceListTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblSubnet);
			this.Controls.Add(this.comboSubnet);
			this.Controls.Add(this.btnStartStop);
			this.Controls.Add(this.lblFilter);
			this.Controls.Add(this.txtFilter);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.btnPing);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.lblHint);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(640, 400);
			this.Name = "DeviceListTool";
			this.Size = new System.Drawing.Size(665, 611);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblSubnet;
		private System.Windows.Forms.ComboBox comboSubnet;
		private System.Windows.Forms.Button btnStartStop;
		private System.Windows.Forms.Label lblFilter;
		private System.Windows.Forms.TextBox txtFilter;
		private System.Windows.Forms.SplitContainer splitContainer;
		private WindowsNetTool.DoubleBufferedListView listDevices;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colIpv4;
		private System.Windows.Forms.ColumnHeader colIpv6;
		private System.Windows.Forms.ColumnHeader colMac;
		private System.Windows.Forms.ColumnHeader colPing;
		private System.Windows.Forms.ColumnHeader colLastReply;
		private System.Windows.Forms.ColumnHeader colInfo;
		private System.Windows.Forms.TextBox txtDetails;
		private System.Windows.Forms.Button btnPing;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label lblHint;
		private System.Windows.Forms.Timer timerFlush;
	}
}
