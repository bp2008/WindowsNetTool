namespace WindowsNetTool.Tools.Ping
{
	partial class PingTool
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
			this.pingTimer = new System.Windows.Forms.Timer(this.components);
			this.lblTarget = new System.Windows.Forms.Label();
			this.txtTarget = new System.Windows.Forms.TextBox();
			this.btnStartStop = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.lblRateCaption = new System.Windows.Forms.Label();
			this.trackRate = new System.Windows.Forms.TrackBar();
			this.lblRate = new System.Windows.Forms.Label();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.lblStats = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.trackRate)).BeginInit();
			this.SuspendLayout();
			//
			// pingTimer
			//
			this.pingTimer.Tick += new System.EventHandler(this.pingTimer_Tick);
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
			this.txtTarget.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtTarget.Name = "txtTarget";
			this.txtTarget.Size = new System.Drawing.Size(230, 22);
			this.txtTarget.TabIndex = 1;
			this.txtTarget.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtTarget_KeyDown);
			//
			// btnStartStop
			//
			this.btnStartStop.Location = new System.Drawing.Point(312, 8);
			this.btnStartStop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnStartStop.Name = "btnStartStop";
			this.btnStartStop.Size = new System.Drawing.Size(110, 30);
			this.btnStartStop.TabIndex = 2;
			this.btnStartStop.Text = "Start";
			this.btnStartStop.UseVisualStyleBackColor = true;
			this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
			//
			// btnClear
			//
			this.btnClear.Location = new System.Drawing.Point(430, 8);
			this.btnClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(110, 30);
			this.btnClear.TabIndex = 3;
			this.btnClear.Text = "Clear Log";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			//
			// lblRateCaption
			//
			this.lblRateCaption.AutoSize = true;
			this.lblRateCaption.Location = new System.Drawing.Point(9, 59);
			this.lblRateCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRateCaption.Name = "lblRateCaption";
			this.lblRateCaption.Size = new System.Drawing.Size(66, 16);
			this.lblRateCaption.TabIndex = 4;
			this.lblRateCaption.Text = "Ping rate:";
			//
			// trackRate
			//
			this.trackRate.LargeChange = 2;
			this.trackRate.Location = new System.Drawing.Point(80, 46);
			this.trackRate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.trackRate.Maximum = 16;
			this.trackRate.Name = "trackRate";
			this.trackRate.Size = new System.Drawing.Size(300, 45);
			this.trackRate.TabIndex = 5;
			this.trackRate.Value = 8;
			this.trackRate.ValueChanged += new System.EventHandler(this.trackRate_ValueChanged);
			//
			// lblRate
			//
			this.lblRate.AutoSize = true;
			this.lblRate.Location = new System.Drawing.Point(388, 59);
			this.lblRate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRate.Name = "lblRate";
			this.lblRate.Size = new System.Drawing.Size(120, 16);
			this.lblRate.TabIndex = 6;
			this.lblRate.Text = "1 ping per second";
			//
			// txtLog
			//
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.BackColor = System.Drawing.SystemColors.Window;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLog.HideSelection = false;
			this.txtLog.Location = new System.Drawing.Point(9, 100);
			this.txtLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtLog.MaxLength = 0;
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(644, 468);
			this.txtLog.TabIndex = 7;
			this.txtLog.TabStop = false;
			this.txtLog.WordWrap = false;
			//
			// lblStats
			//
			this.lblStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblStats.AutoSize = true;
			this.lblStats.Location = new System.Drawing.Point(9, 576);
			this.lblStats.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStats.Name = "lblStats";
			this.lblStats.Size = new System.Drawing.Size(0, 16);
			this.lblStats.TabIndex = 8;
			//
			// PingTool
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblTarget);
			this.Controls.Add(this.txtTarget);
			this.Controls.Add(this.btnStartStop);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.lblRateCaption);
			this.Controls.Add(this.trackRate);
			this.Controls.Add(this.lblRate);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.lblStats);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MinimumSize = new System.Drawing.Size(560, 340);
			this.Name = "PingTool";
			this.Size = new System.Drawing.Size(660, 619);
			((System.ComponentModel.ISupportInitialize)(this.trackRate)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Timer pingTimer;
		private System.Windows.Forms.Label lblTarget;
		private System.Windows.Forms.TextBox txtTarget;
		private System.Windows.Forms.Button btnStartStop;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Label lblRateCaption;
		private System.Windows.Forms.TrackBar trackRate;
		private System.Windows.Forms.Label lblRate;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.Label lblStats;
	}
}
