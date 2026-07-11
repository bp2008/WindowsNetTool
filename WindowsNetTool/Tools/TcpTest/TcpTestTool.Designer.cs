namespace WindowsNetTool.Tools.TcpTest
{
	partial class TcpTestTool
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
			this.lblHost = new System.Windows.Forms.Label();
			this.txtHost = new System.Windows.Forms.TextBox();
			this.lblPort = new System.Windows.Forms.Label();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.btnTest = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.radioConnectOnly = new System.Windows.Forms.RadioButton();
			this.radioHttpGet = new System.Windows.Forms.RadioButton();
			this.lblUrl = new System.Windows.Forms.Label();
			this.txtUrl = new System.Windows.Forms.TextBox();
			this.lblHostHeader = new System.Windows.Forms.Label();
			this.txtHostHeader = new System.Windows.Forms.TextBox();
			this.lblHostHeaderNote = new System.Windows.Forms.Label();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.lblHint = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// lblHost
			//
			this.lblHost.AutoSize = true;
			this.lblHost.Location = new System.Drawing.Point(9, 16);
			this.lblHost.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHost.Name = "lblHost";
			this.lblHost.Size = new System.Drawing.Size(38, 16);
			this.lblHost.TabIndex = 0;
			this.lblHost.Text = "Host:";
			//
			// txtHost
			//
			this.txtHost.Location = new System.Drawing.Point(70, 12);
			this.txtHost.Margin = new System.Windows.Forms.Padding(4);
			this.txtHost.Name = "txtHost";
			this.txtHost.Size = new System.Drawing.Size(210, 22);
			this.txtHost.TabIndex = 1;
			this.txtHost.KeyDown += new System.Windows.Forms.KeyEventHandler(this.input_KeyDown);
			//
			// lblPort
			//
			this.lblPort.AutoSize = true;
			this.lblPort.Location = new System.Drawing.Point(292, 16);
			this.lblPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblPort.Name = "lblPort";
			this.lblPort.Size = new System.Drawing.Size(36, 16);
			this.lblPort.TabIndex = 2;
			this.lblPort.Text = "Port:";
			//
			// txtPort
			//
			this.txtPort.Location = new System.Drawing.Point(335, 12);
			this.txtPort.Margin = new System.Windows.Forms.Padding(4);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(55, 22);
			this.txtPort.TabIndex = 3;
			this.txtPort.KeyDown += new System.Windows.Forms.KeyEventHandler(this.input_KeyDown);
			//
			// btnTest
			//
			this.btnTest.Location = new System.Drawing.Point(404, 8);
			this.btnTest.Margin = new System.Windows.Forms.Padding(4);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(105, 30);
			this.btnTest.TabIndex = 4;
			this.btnTest.Text = "Test";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			//
			// btnClear
			//
			this.btnClear.Location = new System.Drawing.Point(517, 8);
			this.btnClear.Margin = new System.Windows.Forms.Padding(4);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(105, 30);
			this.btnClear.TabIndex = 5;
			this.btnClear.Text = "Clear Log";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			//
			// radioConnectOnly
			//
			this.radioConnectOnly.AutoSize = true;
			this.radioConnectOnly.Checked = true;
			this.radioConnectOnly.Location = new System.Drawing.Point(12, 46);
			this.radioConnectOnly.Margin = new System.Windows.Forms.Padding(4);
			this.radioConnectOnly.Name = "radioConnectOnly";
			this.radioConnectOnly.Size = new System.Drawing.Size(320, 20);
			this.radioConnectOnly.TabIndex = 6;
			this.radioConnectOnly.TabStop = true;
			this.radioConnectOnly.Text = "Connect only — test whether the port is open";
			this.radioConnectOnly.UseVisualStyleBackColor = true;
			//
			// radioHttpGet
			//
			this.radioHttpGet.AutoSize = true;
			this.radioHttpGet.Location = new System.Drawing.Point(12, 78);
			this.radioHttpGet.Margin = new System.Windows.Forms.Padding(4);
			this.radioHttpGet.Name = "radioHttpGet";
			this.radioHttpGet.Size = new System.Drawing.Size(140, 20);
			this.radioHttpGet.TabIndex = 7;
			this.radioHttpGet.Text = "HTTP GET request";
			this.radioHttpGet.UseVisualStyleBackColor = true;
			this.radioHttpGet.CheckedChanged += new System.EventHandler(this.radioHttpGet_CheckedChanged);
			//
			// lblUrl
			//
			this.lblUrl.AutoSize = true;
			this.lblUrl.Location = new System.Drawing.Point(200, 80);
			this.lblUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblUrl.Name = "lblUrl";
			this.lblUrl.Size = new System.Drawing.Size(37, 16);
			this.lblUrl.TabIndex = 8;
			this.lblUrl.Text = "URL:";
			//
			// txtUrl
			//
			this.txtUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtUrl.Location = new System.Drawing.Point(245, 76);
			this.txtUrl.Margin = new System.Windows.Forms.Padding(4);
			this.txtUrl.Name = "txtUrl";
			this.txtUrl.Size = new System.Drawing.Size(409, 22);
			this.txtUrl.TabIndex = 9;
			this.txtUrl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.input_KeyDown);
			//
			// lblHostHeader
			//
			this.lblHostHeader.AutoSize = true;
			this.lblHostHeader.Location = new System.Drawing.Point(200, 112);
			this.lblHostHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHostHeader.Name = "lblHostHeader";
			this.lblHostHeader.Size = new System.Drawing.Size(84, 16);
			this.lblHostHeader.TabIndex = 10;
			this.lblHostHeader.Text = "Host header:";
			//
			// txtHostHeader
			//
			this.txtHostHeader.Location = new System.Drawing.Point(295, 108);
			this.txtHostHeader.Margin = new System.Windows.Forms.Padding(4);
			this.txtHostHeader.Name = "txtHostHeader";
			this.txtHostHeader.Size = new System.Drawing.Size(200, 22);
			this.txtHostHeader.TabIndex = 11;
			this.txtHostHeader.KeyDown += new System.Windows.Forms.KeyEventHandler(this.input_KeyDown);
			//
			// lblHostHeaderNote
			//
			this.lblHostHeaderNote.AutoSize = true;
			this.lblHostHeaderNote.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lblHostHeaderNote.Location = new System.Drawing.Point(503, 112);
			this.lblHostHeaderNote.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHostHeaderNote.Name = "lblHostHeaderNote";
			this.lblHostHeaderNote.Size = new System.Drawing.Size(150, 16);
			this.lblHostHeaderNote.TabIndex = 12;
			this.lblHostHeaderNote.Text = "(blank = the URL\'s host)";
			//
			// txtLog
			//
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.BackColor = System.Drawing.SystemColors.Window;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLog.HideSelection = false;
			this.txtLog.Location = new System.Drawing.Point(9, 144);
			this.txtLog.Margin = new System.Windows.Forms.Padding(4);
			this.txtLog.MaxLength = 0;
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(647, 414);
			this.txtLog.TabIndex = 13;
			this.txtLog.TabStop = false;
			this.txtLog.WordWrap = false;
			//
			// lblHint
			//
			this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblHint.Location = new System.Drawing.Point(9, 564);
			this.lblHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHint.Name = "lblHint";
			this.lblHint.Size = new System.Drawing.Size(647, 48);
			this.lblHint.TabIndex = 14;
			this.lblHint.Text = "In HTTP GET mode the connection goes to Host : Port while the URL and Host heade" +
    "r say what to request, letting a site be tested on a specific server.  Blank Ho" +
    "st / Port are taken from the URL.  Certificate problems are reported but do not" +
    " stop the test.";
			//
			// TcpTestTool
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblHost);
			this.Controls.Add(this.txtHost);
			this.Controls.Add(this.lblPort);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.btnTest);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.radioConnectOnly);
			this.Controls.Add(this.radioHttpGet);
			this.Controls.Add(this.lblUrl);
			this.Controls.Add(this.txtUrl);
			this.Controls.Add(this.lblHostHeader);
			this.Controls.Add(this.txtHostHeader);
			this.Controls.Add(this.lblHostHeaderNote);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.lblHint);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(640, 380);
			this.Name = "TcpTestTool";
			this.Size = new System.Drawing.Size(665, 616);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblHost;
		private System.Windows.Forms.TextBox txtHost;
		private System.Windows.Forms.Label lblPort;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.Button btnTest;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.RadioButton radioConnectOnly;
		private System.Windows.Forms.RadioButton radioHttpGet;
		private System.Windows.Forms.Label lblUrl;
		private System.Windows.Forms.TextBox txtUrl;
		private System.Windows.Forms.Label lblHostHeader;
		private System.Windows.Forms.TextBox txtHostHeader;
		private System.Windows.Forms.Label lblHostHeaderNote;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.Label lblHint;
	}
}
