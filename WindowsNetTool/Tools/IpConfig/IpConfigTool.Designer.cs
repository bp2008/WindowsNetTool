namespace WindowsNetTool.Tools.IpConfig
{
	partial class IpConfigTool
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
			this.lblInterface = new System.Windows.Forms.Label();
			this.comboInterfaces = new System.Windows.Forms.ComboBox();
			this.btnRenameInterface = new System.Windows.Forms.Button();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.lblInterfaceInfo = new System.Windows.Forms.Label();
			this.btnDhcpToggle = new System.Windows.Forms.Button();
			this.btnStatusWindow = new System.Windows.Forms.Button();
			this.groupAddresses = new System.Windows.Forms.GroupBox();
			this.listAddresses = new System.Windows.Forms.ListView();
			this.colAddress = new System.Windows.Forms.ColumnHeader();
			this.colMask = new System.Windows.Forms.ColumnHeader();
			this.colSource = new System.Windows.Forms.ColumnHeader();
			this.lblNewAddress = new System.Windows.Forms.Label();
			this.txtNewAddress = new System.Windows.Forms.TextBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.groupGateways = new System.Windows.Forms.GroupBox();
			this.listGateways = new System.Windows.Forms.ListView();
			this.colGateway = new System.Windows.Forms.ColumnHeader();
			this.colGwMetric = new System.Windows.Forms.ColumnHeader();
			this.colGwSource = new System.Windows.Forms.ColumnHeader();
			this.btnGwMoveUp = new System.Windows.Forms.Button();
			this.btnGwMoveDown = new System.Windows.Forms.Button();
			this.btnGwRemove = new System.Windows.Forms.Button();
			this.txtNewGateway = new System.Windows.Forms.TextBox();
			this.lblGwMetric = new System.Windows.Forms.Label();
			this.txtNewGwMetric = new System.Windows.Forms.TextBox();
			this.btnGwAdd = new System.Windows.Forms.Button();
			this.lblGwHint = new System.Windows.Forms.Label();
			this.groupDns = new System.Windows.Forms.GroupBox();
			this.lblDnsSource = new System.Windows.Forms.Label();
			this.listDns = new System.Windows.Forms.ListBox();
			this.btnDnsMoveUp = new System.Windows.Forms.Button();
			this.btnDnsMoveDown = new System.Windows.Forms.Button();
			this.btnDnsRemove = new System.Windows.Forms.Button();
			this.btnDnsUseDhcp = new System.Windows.Forms.Button();
			this.txtNewDns = new System.Windows.Forms.TextBox();
			this.btnDnsAdd = new System.Windows.Forms.Button();
			this.groupAddresses.SuspendLayout();
			this.groupGateways.SuspendLayout();
			this.groupDns.SuspendLayout();
			this.SuspendLayout();
			//
			// lblInterface
			//
			this.lblInterface.AutoSize = true;
			this.lblInterface.Location = new System.Drawing.Point(4, 10);
			this.lblInterface.Name = "lblInterface";
			this.lblInterface.Size = new System.Drawing.Size(52, 13);
			this.lblInterface.TabIndex = 0;
			this.lblInterface.Text = "Interface:";
			//
			// comboInterfaces
			//
			this.comboInterfaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.comboInterfaces.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboInterfaces.FormattingEnabled = true;
			this.comboInterfaces.Location = new System.Drawing.Point(62, 6);
			this.comboInterfaces.Name = "comboInterfaces";
			this.comboInterfaces.Size = new System.Drawing.Size(491, 21);
			this.comboInterfaces.TabIndex = 1;
			this.comboInterfaces.SelectedIndexChanged += new System.EventHandler(this.comboInterfaces_SelectedIndexChanged);
			//
			// btnRenameInterface
			//
			this.btnRenameInterface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRenameInterface.Location = new System.Drawing.Point(559, 4);
			this.btnRenameInterface.Name = "btnRenameInterface";
			this.btnRenameInterface.Size = new System.Drawing.Size(90, 25);
			this.btnRenameInterface.TabIndex = 2;
			this.btnRenameInterface.Text = "Rename...";
			this.btnRenameInterface.UseVisualStyleBackColor = true;
			this.btnRenameInterface.Click += new System.EventHandler(this.btnRenameInterface_Click);
			//
			// btnRefresh
			//
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRefresh.Location = new System.Drawing.Point(655, 4);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(101, 25);
			this.btnRefresh.TabIndex = 3;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			//
			// lblInterfaceInfo
			//
			this.lblInterfaceInfo.AutoSize = true;
			this.lblInterfaceInfo.Location = new System.Drawing.Point(4, 36);
			this.lblInterfaceInfo.Name = "lblInterfaceInfo";
			this.lblInterfaceInfo.Size = new System.Drawing.Size(0, 13);
			this.lblInterfaceInfo.TabIndex = 4;
			//
			// btnDhcpToggle
			//
			this.btnDhcpToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDhcpToggle.Location = new System.Drawing.Point(655, 33);
			this.btnDhcpToggle.Name = "btnDhcpToggle";
			this.btnDhcpToggle.Size = new System.Drawing.Size(101, 24);
			this.btnDhcpToggle.TabIndex = 5;
			this.btnDhcpToggle.Text = "Enable DHCP";
			this.btnDhcpToggle.UseVisualStyleBackColor = true;
			this.btnDhcpToggle.Click += new System.EventHandler(this.btnDhcpToggle_Click);
			//
			// btnStatusWindow
			//
			this.btnStatusWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStatusWindow.Location = new System.Drawing.Point(655, 61);
			this.btnStatusWindow.Name = "btnStatusWindow";
			this.btnStatusWindow.Size = new System.Drawing.Size(101, 24);
			this.btnStatusWindow.TabIndex = 6;
			this.btnStatusWindow.Text = "Status Window";
			this.btnStatusWindow.UseVisualStyleBackColor = true;
			this.btnStatusWindow.Click += new System.EventHandler(this.btnStatusWindow_Click);
			//
			// groupAddresses
			//
			this.groupAddresses.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.groupAddresses.Controls.Add(this.listAddresses);
			this.groupAddresses.Controls.Add(this.lblNewAddress);
			this.groupAddresses.Controls.Add(this.txtNewAddress);
			this.groupAddresses.Controls.Add(this.btnAdd);
			this.groupAddresses.Controls.Add(this.btnDelete);
			this.groupAddresses.Location = new System.Drawing.Point(7, 100);
			this.groupAddresses.Name = "groupAddresses";
			this.groupAddresses.Size = new System.Drawing.Size(749, 220);
			this.groupAddresses.TabIndex = 7;
			this.groupAddresses.TabStop = false;
			this.groupAddresses.Text = "IPv4 Addresses";
			//
			// listAddresses
			//
			this.listAddresses.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.listAddresses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.colAddress,
			this.colMask,
			this.colSource});
			this.listAddresses.FullRowSelect = true;
			this.listAddresses.HideSelection = false;
			this.listAddresses.Location = new System.Drawing.Point(6, 19);
			this.listAddresses.MultiSelect = false;
			this.listAddresses.Name = "listAddresses";
			this.listAddresses.Size = new System.Drawing.Size(737, 151);
			this.listAddresses.TabIndex = 0;
			this.listAddresses.UseCompatibleStateImageBehavior = false;
			this.listAddresses.View = System.Windows.Forms.View.Details;
			//
			// colAddress
			//
			this.colAddress.Text = "Address";
			this.colAddress.Width = 180;
			//
			// colMask
			//
			this.colMask.Text = "Subnet Mask";
			this.colMask.Width = 200;
			//
			// colSource
			//
			this.colSource.Text = "Source";
			this.colSource.Width = 120;
			//
			// lblNewAddress
			//
			this.lblNewAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblNewAddress.AutoSize = true;
			this.lblNewAddress.Location = new System.Drawing.Point(6, 175);
			this.lblNewAddress.Name = "lblNewAddress";
			this.lblNewAddress.Size = new System.Drawing.Size(213, 13);
			this.lblNewAddress.TabIndex = 1;
			this.lblNewAddress.Text = "New address (e.g. 192.168.1.2/24):";
			//
			// txtNewAddress
			//
			this.txtNewAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtNewAddress.Location = new System.Drawing.Point(6, 193);
			this.txtNewAddress.Name = "txtNewAddress";
			this.txtNewAddress.Size = new System.Drawing.Size(200, 20);
			this.txtNewAddress.TabIndex = 2;
			//
			// btnAdd
			//
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAdd.Location = new System.Drawing.Point(212, 191);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(100, 24);
			this.btnAdd.TabIndex = 3;
			this.btnAdd.Text = "Add Static IP";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			//
			// btnDelete
			//
			this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDelete.Location = new System.Drawing.Point(603, 191);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(140, 24);
			this.btnDelete.TabIndex = 4;
			this.btnDelete.Text = "Delete Selected IP";
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			//
			// groupGateways
			//
			this.groupGateways.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.groupGateways.Controls.Add(this.listGateways);
			this.groupGateways.Controls.Add(this.btnGwMoveUp);
			this.groupGateways.Controls.Add(this.btnGwMoveDown);
			this.groupGateways.Controls.Add(this.btnGwRemove);
			this.groupGateways.Controls.Add(this.txtNewGateway);
			this.groupGateways.Controls.Add(this.lblGwMetric);
			this.groupGateways.Controls.Add(this.txtNewGwMetric);
			this.groupGateways.Controls.Add(this.btnGwAdd);
			this.groupGateways.Controls.Add(this.lblGwHint);
			this.groupGateways.Location = new System.Drawing.Point(7, 326);
			this.groupGateways.Name = "groupGateways";
			this.groupGateways.Size = new System.Drawing.Size(749, 140);
			this.groupGateways.TabIndex = 8;
			this.groupGateways.TabStop = false;
			this.groupGateways.Text = "Default Gateways (Windows prefers lower metrics)";
			//
			// listGateways
			//
			this.listGateways.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.colGateway,
			this.colGwMetric,
			this.colGwSource});
			this.listGateways.FullRowSelect = true;
			this.listGateways.HideSelection = false;
			this.listGateways.Location = new System.Drawing.Point(6, 19);
			this.listGateways.MultiSelect = false;
			this.listGateways.Name = "listGateways";
			this.listGateways.Size = new System.Drawing.Size(600, 82);
			this.listGateways.TabIndex = 0;
			this.listGateways.UseCompatibleStateImageBehavior = false;
			this.listGateways.View = System.Windows.Forms.View.Details;
			//
			// colGateway
			//
			this.colGateway.Text = "Gateway";
			this.colGateway.Width = 180;
			//
			// colGwMetric
			//
			this.colGwMetric.Text = "Metric";
			this.colGwMetric.Width = 120;
			//
			// colGwSource
			//
			this.colGwSource.Text = "Source";
			this.colGwSource.Width = 100;
			//
			// btnGwMoveUp
			//
			this.btnGwMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGwMoveUp.Location = new System.Drawing.Point(612, 19);
			this.btnGwMoveUp.Name = "btnGwMoveUp";
			this.btnGwMoveUp.Size = new System.Drawing.Size(131, 24);
			this.btnGwMoveUp.TabIndex = 1;
			this.btnGwMoveUp.Text = "Move Up";
			this.btnGwMoveUp.UseVisualStyleBackColor = true;
			this.btnGwMoveUp.Click += new System.EventHandler(this.btnGwMoveUp_Click);
			//
			// btnGwMoveDown
			//
			this.btnGwMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGwMoveDown.Location = new System.Drawing.Point(612, 47);
			this.btnGwMoveDown.Name = "btnGwMoveDown";
			this.btnGwMoveDown.Size = new System.Drawing.Size(131, 24);
			this.btnGwMoveDown.TabIndex = 2;
			this.btnGwMoveDown.Text = "Move Down";
			this.btnGwMoveDown.UseVisualStyleBackColor = true;
			this.btnGwMoveDown.Click += new System.EventHandler(this.btnGwMoveDown_Click);
			//
			// btnGwRemove
			//
			this.btnGwRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGwRemove.Location = new System.Drawing.Point(612, 75);
			this.btnGwRemove.Name = "btnGwRemove";
			this.btnGwRemove.Size = new System.Drawing.Size(131, 24);
			this.btnGwRemove.TabIndex = 3;
			this.btnGwRemove.Text = "Remove";
			this.btnGwRemove.UseVisualStyleBackColor = true;
			this.btnGwRemove.Click += new System.EventHandler(this.btnGwRemove_Click);
			//
			// txtNewGateway
			//
			this.txtNewGateway.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtNewGateway.Location = new System.Drawing.Point(6, 110);
			this.txtNewGateway.Name = "txtNewGateway";
			this.txtNewGateway.Size = new System.Drawing.Size(140, 20);
			this.txtNewGateway.TabIndex = 4;
			//
			// lblGwMetric
			//
			this.lblGwMetric.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblGwMetric.AutoSize = true;
			this.lblGwMetric.Location = new System.Drawing.Point(152, 113);
			this.lblGwMetric.Name = "lblGwMetric";
			this.lblGwMetric.Size = new System.Drawing.Size(40, 13);
			this.lblGwMetric.TabIndex = 5;
			this.lblGwMetric.Text = "Metric:";
			//
			// txtNewGwMetric
			//
			this.txtNewGwMetric.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtNewGwMetric.Location = new System.Drawing.Point(196, 110);
			this.txtNewGwMetric.Name = "txtNewGwMetric";
			this.txtNewGwMetric.Size = new System.Drawing.Size(50, 20);
			this.txtNewGwMetric.TabIndex = 6;
			//
			// btnGwAdd
			//
			this.btnGwAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnGwAdd.Location = new System.Drawing.Point(252, 108);
			this.btnGwAdd.Name = "btnGwAdd";
			this.btnGwAdd.Size = new System.Drawing.Size(100, 24);
			this.btnGwAdd.TabIndex = 7;
			this.btnGwAdd.Text = "Add Gateway";
			this.btnGwAdd.UseVisualStyleBackColor = true;
			this.btnGwAdd.Click += new System.EventHandler(this.btnGwAdd_Click);
			//
			// lblGwHint
			//
			this.lblGwHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblGwHint.AutoSize = true;
			this.lblGwHint.Location = new System.Drawing.Point(358, 113);
			this.lblGwHint.Name = "lblGwHint";
			this.lblGwHint.Size = new System.Drawing.Size(140, 13);
			this.lblGwHint.TabIndex = 8;
			this.lblGwHint.Text = "(blank metric = automatic)";
			//
			// groupDns
			//
			this.groupDns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.groupDns.Controls.Add(this.lblDnsSource);
			this.groupDns.Controls.Add(this.listDns);
			this.groupDns.Controls.Add(this.btnDnsMoveUp);
			this.groupDns.Controls.Add(this.btnDnsMoveDown);
			this.groupDns.Controls.Add(this.btnDnsRemove);
			this.groupDns.Controls.Add(this.btnDnsUseDhcp);
			this.groupDns.Controls.Add(this.txtNewDns);
			this.groupDns.Controls.Add(this.btnDnsAdd);
			this.groupDns.Location = new System.Drawing.Point(7, 472);
			this.groupDns.Name = "groupDns";
			this.groupDns.Size = new System.Drawing.Size(749, 164);
			this.groupDns.TabIndex = 9;
			this.groupDns.TabStop = false;
			this.groupDns.Text = "IPv4 DNS Servers (in resolution order)";
			//
			// lblDnsSource
			//
			this.lblDnsSource.AutoSize = true;
			this.lblDnsSource.Location = new System.Drawing.Point(6, 17);
			this.lblDnsSource.Name = "lblDnsSource";
			this.lblDnsSource.Size = new System.Drawing.Size(44, 13);
			this.lblDnsSource.TabIndex = 0;
			this.lblDnsSource.Text = "Source:";
			//
			// listDns
			//
			this.listDns.FormattingEnabled = true;
			this.listDns.IntegralHeight = false;
			this.listDns.Location = new System.Drawing.Point(6, 36);
			this.listDns.Name = "listDns";
			this.listDns.Size = new System.Drawing.Size(600, 92);
			this.listDns.TabIndex = 1;
			//
			// btnDnsMoveUp
			//
			this.btnDnsMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDnsMoveUp.Location = new System.Drawing.Point(612, 36);
			this.btnDnsMoveUp.Name = "btnDnsMoveUp";
			this.btnDnsMoveUp.Size = new System.Drawing.Size(131, 24);
			this.btnDnsMoveUp.TabIndex = 2;
			this.btnDnsMoveUp.Text = "Move Up";
			this.btnDnsMoveUp.UseVisualStyleBackColor = true;
			this.btnDnsMoveUp.Click += new System.EventHandler(this.btnDnsMoveUp_Click);
			//
			// btnDnsMoveDown
			//
			this.btnDnsMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDnsMoveDown.Location = new System.Drawing.Point(612, 64);
			this.btnDnsMoveDown.Name = "btnDnsMoveDown";
			this.btnDnsMoveDown.Size = new System.Drawing.Size(131, 24);
			this.btnDnsMoveDown.TabIndex = 3;
			this.btnDnsMoveDown.Text = "Move Down";
			this.btnDnsMoveDown.UseVisualStyleBackColor = true;
			this.btnDnsMoveDown.Click += new System.EventHandler(this.btnDnsMoveDown_Click);
			//
			// btnDnsRemove
			//
			this.btnDnsRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDnsRemove.Location = new System.Drawing.Point(612, 92);
			this.btnDnsRemove.Name = "btnDnsRemove";
			this.btnDnsRemove.Size = new System.Drawing.Size(131, 24);
			this.btnDnsRemove.TabIndex = 4;
			this.btnDnsRemove.Text = "Remove";
			this.btnDnsRemove.UseVisualStyleBackColor = true;
			this.btnDnsRemove.Click += new System.EventHandler(this.btnDnsRemove_Click);
			//
			// btnDnsUseDhcp
			//
			this.btnDnsUseDhcp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDnsUseDhcp.Location = new System.Drawing.Point(612, 120);
			this.btnDnsUseDhcp.Name = "btnDnsUseDhcp";
			this.btnDnsUseDhcp.Size = new System.Drawing.Size(131, 24);
			this.btnDnsUseDhcp.TabIndex = 5;
			this.btnDnsUseDhcp.Text = "Use DHCP DNS";
			this.btnDnsUseDhcp.UseVisualStyleBackColor = true;
			this.btnDnsUseDhcp.Click += new System.EventHandler(this.btnDnsUseDhcp_Click);
			//
			// txtNewDns
			//
			this.txtNewDns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtNewDns.Location = new System.Drawing.Point(6, 134);
			this.txtNewDns.Name = "txtNewDns";
			this.txtNewDns.Size = new System.Drawing.Size(200, 20);
			this.txtNewDns.TabIndex = 6;
			//
			// btnDnsAdd
			//
			this.btnDnsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnDnsAdd.Location = new System.Drawing.Point(212, 132);
			this.btnDnsAdd.Name = "btnDnsAdd";
			this.btnDnsAdd.Size = new System.Drawing.Size(100, 24);
			this.btnDnsAdd.TabIndex = 7;
			this.btnDnsAdd.Text = "Add DNS";
			this.btnDnsAdd.UseVisualStyleBackColor = true;
			this.btnDnsAdd.Click += new System.EventHandler(this.btnDnsAdd_Click);
			//
			// IpConfigTool
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblInterface);
			this.Controls.Add(this.comboInterfaces);
			this.Controls.Add(this.btnRenameInterface);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.lblInterfaceInfo);
			this.Controls.Add(this.btnDhcpToggle);
			this.Controls.Add(this.btnStatusWindow);
			this.Controls.Add(this.groupAddresses);
			this.Controls.Add(this.groupGateways);
			this.Controls.Add(this.groupDns);
			this.Name = "IpConfigTool";
			this.Size = new System.Drawing.Size(760, 640);
			this.groupAddresses.ResumeLayout(false);
			this.groupAddresses.PerformLayout();
			this.groupGateways.ResumeLayout(false);
			this.groupGateways.PerformLayout();
			this.groupDns.ResumeLayout(false);
			this.groupDns.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblInterface;
		private System.Windows.Forms.ComboBox comboInterfaces;
		private System.Windows.Forms.Button btnRenameInterface;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Label lblInterfaceInfo;
		private System.Windows.Forms.Button btnDhcpToggle;
		private System.Windows.Forms.Button btnStatusWindow;
		private System.Windows.Forms.GroupBox groupAddresses;
		private System.Windows.Forms.ListView listAddresses;
		private System.Windows.Forms.ColumnHeader colAddress;
		private System.Windows.Forms.ColumnHeader colMask;
		private System.Windows.Forms.ColumnHeader colSource;
		private System.Windows.Forms.Label lblNewAddress;
		private System.Windows.Forms.TextBox txtNewAddress;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.GroupBox groupGateways;
		private System.Windows.Forms.ListView listGateways;
		private System.Windows.Forms.ColumnHeader colGateway;
		private System.Windows.Forms.ColumnHeader colGwMetric;
		private System.Windows.Forms.ColumnHeader colGwSource;
		private System.Windows.Forms.Button btnGwMoveUp;
		private System.Windows.Forms.Button btnGwMoveDown;
		private System.Windows.Forms.Button btnGwRemove;
		private System.Windows.Forms.TextBox txtNewGateway;
		private System.Windows.Forms.Label lblGwMetric;
		private System.Windows.Forms.TextBox txtNewGwMetric;
		private System.Windows.Forms.Button btnGwAdd;
		private System.Windows.Forms.Label lblGwHint;
		private System.Windows.Forms.GroupBox groupDns;
		private System.Windows.Forms.Label lblDnsSource;
		private System.Windows.Forms.ListBox listDns;
		private System.Windows.Forms.Button btnDnsMoveUp;
		private System.Windows.Forms.Button btnDnsMoveDown;
		private System.Windows.Forms.Button btnDnsRemove;
		private System.Windows.Forms.Button btnDnsUseDhcp;
		private System.Windows.Forms.TextBox txtNewDns;
		private System.Windows.Forms.Button btnDnsAdd;
	}
}
