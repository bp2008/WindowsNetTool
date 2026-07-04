namespace WindowsNetTool.Tools.DnsLookup
{
	partial class DnsLookupTool
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
			this.lblQuery = new System.Windows.Forms.Label();
			this.txtQuery = new System.Windows.Forms.TextBox();
			this.lblType = new System.Windows.Forms.Label();
			this.comboType = new System.Windows.Forms.ComboBox();
			this.lblServer = new System.Windows.Forms.Label();
			this.comboServer = new System.Windows.Forms.ComboBox();
			this.btnLookup = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// lblQuery
			//
			this.lblQuery.AutoSize = true;
			this.lblQuery.Location = new System.Drawing.Point(9, 16);
			this.lblQuery.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblQuery.Name = "lblQuery";
			this.lblQuery.Size = new System.Drawing.Size(47, 16);
			this.lblQuery.TabIndex = 0;
			this.lblQuery.Text = "Query:";
			//
			// txtQuery
			//
			this.txtQuery.Location = new System.Drawing.Point(100, 12);
			this.txtQuery.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtQuery.Name = "txtQuery";
			this.txtQuery.Size = new System.Drawing.Size(245, 22);
			this.txtQuery.TabIndex = 1;
			this.txtQuery.KeyDown += new System.Windows.Forms.KeyEventHandler(this.queryInput_KeyDown);
			//
			// lblType
			//
			this.lblType.AutoSize = true;
			this.lblType.Location = new System.Drawing.Point(357, 16);
			this.lblType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblType.Name = "lblType";
			this.lblType.Size = new System.Drawing.Size(40, 16);
			this.lblType.TabIndex = 2;
			this.lblType.Text = "Type:";
			//
			// comboType
			//
			this.comboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboType.Location = new System.Drawing.Point(403, 12);
			this.comboType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.comboType.Name = "comboType";
			this.comboType.Size = new System.Drawing.Size(147, 24);
			this.comboType.TabIndex = 3;
			//
			// lblServer
			//
			this.lblServer.AutoSize = true;
			this.lblServer.Location = new System.Drawing.Point(9, 50);
			this.lblServer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblServer.Name = "lblServer";
			this.lblServer.Size = new System.Drawing.Size(78, 16);
			this.lblServer.TabIndex = 4;
			this.lblServer.Text = "DNS server:";
			//
			// comboServer
			//
			this.comboServer.Location = new System.Drawing.Point(100, 46);
			this.comboServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.comboServer.Name = "comboServer";
			this.comboServer.Size = new System.Drawing.Size(245, 24);
			this.comboServer.TabIndex = 5;
			this.comboServer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.queryInput_KeyDown);
			//
			// btnLookup
			//
			this.btnLookup.Location = new System.Drawing.Point(357, 44);
			this.btnLookup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnLookup.Name = "btnLookup";
			this.btnLookup.Size = new System.Drawing.Size(110, 30);
			this.btnLookup.TabIndex = 6;
			this.btnLookup.Text = "Lookup";
			this.btnLookup.UseVisualStyleBackColor = true;
			this.btnLookup.Click += new System.EventHandler(this.btnLookup_Click);
			//
			// btnClear
			//
			this.btnClear.Location = new System.Drawing.Point(475, 44);
			this.btnClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(110, 30);
			this.btnClear.TabIndex = 7;
			this.btnClear.Text = "Clear Log";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			//
			// txtLog
			//
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.BackColor = System.Drawing.SystemColors.Window;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLog.HideSelection = false;
			this.txtLog.Location = new System.Drawing.Point(9, 88);
			this.txtLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtLog.MaxLength = 0;
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(644, 519);
			this.txtLog.TabIndex = 8;
			this.txtLog.TabStop = false;
			this.txtLog.WordWrap = false;
			//
			// DnsLookupTool
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblQuery);
			this.Controls.Add(this.txtQuery);
			this.Controls.Add(this.lblType);
			this.Controls.Add(this.comboType);
			this.Controls.Add(this.lblServer);
			this.Controls.Add(this.comboServer);
			this.Controls.Add(this.btnLookup);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.txtLog);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MinimumSize = new System.Drawing.Size(600, 340);
			this.Name = "DnsLookupTool";
			this.Size = new System.Drawing.Size(660, 619);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblQuery;
		private System.Windows.Forms.TextBox txtQuery;
		private System.Windows.Forms.Label lblType;
		private System.Windows.Forms.ComboBox comboType;
		private System.Windows.Forms.Label lblServer;
		private System.Windows.Forms.ComboBox comboServer;
		private System.Windows.Forms.Button btnLookup;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.TextBox txtLog;
	}
}
