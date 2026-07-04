namespace WindowsNetTool.Tools.IpScanner
{
	partial class IpScannerTool
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
			this.lblInFlight = new System.Windows.Forms.Label();
			this.numInFlight = new System.Windows.Forms.NumericUpDown();
			this.btnStartStop = new System.Windows.Forms.Button();
			this.listResults = new WindowsNetTool.DoubleBufferedListView();
			this.colIp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPing = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colHostname = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colMac = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colLastReply = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnPing = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblHint = new System.Windows.Forms.Label();
			this.timerFlush = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.numInFlight)).BeginInit();
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
			this.comboSubnet.Size = new System.Drawing.Size(238, 24);
			this.comboSubnet.TabIndex = 1;
			this.comboSubnet.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboSubnet_KeyDown);
			// 
			// lblInFlight
			// 
			this.lblInFlight.AutoSize = true;
			this.lblInFlight.Location = new System.Drawing.Point(326, 16);
			this.lblInFlight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblInFlight.Name = "lblInFlight";
			this.lblInFlight.Size = new System.Drawing.Size(77, 16);
			this.lblInFlight.TabIndex = 2;
			this.lblInFlight.Text = "In-flight limit:";
			// 
			// numInFlight
			// 
			this.numInFlight.Increment = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numInFlight.Location = new System.Drawing.Point(430, 13);
			this.numInFlight.Margin = new System.Windows.Forms.Padding(4);
			this.numInFlight.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.numInFlight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numInFlight.Name = "numInFlight";
			this.numInFlight.Size = new System.Drawing.Size(64, 22);
			this.numInFlight.TabIndex = 3;
			this.numInFlight.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.numInFlight.ValueChanged += new System.EventHandler(this.numInFlight_ValueChanged);
			// 
			// btnStartStop
			// 
			this.btnStartStop.Location = new System.Drawing.Point(510, 8);
			this.btnStartStop.Margin = new System.Windows.Forms.Padding(4);
			this.btnStartStop.Name = "btnStartStop";
			this.btnStartStop.Size = new System.Drawing.Size(110, 30);
			this.btnStartStop.TabIndex = 4;
			this.btnStartStop.Text = "Start";
			this.btnStartStop.UseVisualStyleBackColor = true;
			this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
			// 
			// listResults
			// 
			this.listResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIp,
            this.colPing,
            this.colHostname,
            this.colMac,
            this.colLastReply});
			this.listResults.FullRowSelect = true;
			this.listResults.HideSelection = false;
			this.listResults.Location = new System.Drawing.Point(9, 46);
			this.listResults.Margin = new System.Windows.Forms.Padding(4);
			this.listResults.MultiSelect = false;
			this.listResults.Name = "listResults";
			this.listResults.Size = new System.Drawing.Size(649, 470);
			this.listResults.TabIndex = 5;
			this.listResults.UseCompatibleStateImageBehavior = false;
			this.listResults.View = System.Windows.Forms.View.Details;
			this.listResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listResults_ColumnClick);
			this.listResults.ItemActivate += new System.EventHandler(this.listResults_ItemActivate);
			// 
			// colIp
			// 
			this.colIp.Text = "IP Address";
			this.colIp.Width = 120;
			// 
			// colPing
			// 
			this.colPing.Text = "Ping Time";
			this.colPing.Width = 80;
			// 
			// colHostname
			// 
			this.colHostname.Text = "Host Name";
			this.colHostname.Width = 205;
			// 
			// colMac
			// 
			this.colMac.Text = "MAC Address";
			this.colMac.Width = 140;
			// 
			// colLastReply
			// 
			this.colLastReply.Text = "Last Reply";
			this.colLastReply.Width = 92;
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
			this.lblHint.Location = new System.Drawing.Point(6, 562);
			this.lblHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHint.Name = "lblHint";
			this.lblHint.Size = new System.Drawing.Size(652, 32);
			this.lblHint.TabIndex = 8;
			this.lblHint.Text = "Every address is pinged repeatedly, so hosts that miss one wave are caught by a l" +
    "ater one.  Double-click a host to begin monitoring it in the Ping tool.\r\n";
			// 
			// timerFlush
			// 
			this.timerFlush.Interval = 250;
			this.timerFlush.Tick += new System.EventHandler(this.timerFlush_Tick);
			// 
			// IpScannerTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblSubnet);
			this.Controls.Add(this.comboSubnet);
			this.Controls.Add(this.lblInFlight);
			this.Controls.Add(this.numInFlight);
			this.Controls.Add(this.btnStartStop);
			this.Controls.Add(this.listResults);
			this.Controls.Add(this.btnPing);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.lblHint);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(640, 340);
			this.Name = "IpScannerTool";
			this.Size = new System.Drawing.Size(665, 611);
			((System.ComponentModel.ISupportInitialize)(this.numInFlight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblSubnet;
		private System.Windows.Forms.ComboBox comboSubnet;
		private System.Windows.Forms.Label lblInFlight;
		private System.Windows.Forms.NumericUpDown numInFlight;
		private System.Windows.Forms.Button btnStartStop;
		private WindowsNetTool.DoubleBufferedListView listResults;
		private System.Windows.Forms.ColumnHeader colIp;
		private System.Windows.Forms.ColumnHeader colPing;
		private System.Windows.Forms.ColumnHeader colHostname;
		private System.Windows.Forms.ColumnHeader colMac;
		private System.Windows.Forms.ColumnHeader colLastReply;
		private System.Windows.Forms.Button btnPing;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label lblHint;
		private System.Windows.Forms.Timer timerFlush;
	}
}
