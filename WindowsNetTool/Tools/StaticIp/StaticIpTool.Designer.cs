namespace WindowsNetTool.Tools.StaticIp
{
	partial class StaticIpTool
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
			this.btnRefresh = new System.Windows.Forms.Button();
			this.lblInterfaceInfo = new System.Windows.Forms.Label();
			this.listAddresses = new System.Windows.Forms.ListView();
			this.colAddress = new System.Windows.Forms.ColumnHeader();
			this.colMask = new System.Windows.Forms.ColumnHeader();
			this.colSource = new System.Windows.Forms.ColumnHeader();
			this.lblNewAddress = new System.Windows.Forms.Label();
			this.txtNewAddress = new System.Windows.Forms.TextBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
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
			this.comboInterfaces.Size = new System.Drawing.Size(587, 21);
			this.comboInterfaces.TabIndex = 1;
			this.comboInterfaces.SelectedIndexChanged += new System.EventHandler(this.comboInterfaces_SelectedIndexChanged);
			//
			// btnRefresh
			//
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRefresh.Location = new System.Drawing.Point(655, 4);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(101, 25);
			this.btnRefresh.TabIndex = 2;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			//
			// lblInterfaceInfo
			//
			this.lblInterfaceInfo.AutoSize = true;
			this.lblInterfaceInfo.Location = new System.Drawing.Point(4, 38);
			this.lblInterfaceInfo.Name = "lblInterfaceInfo";
			this.lblInterfaceInfo.Size = new System.Drawing.Size(0, 13);
			this.lblInterfaceInfo.TabIndex = 3;
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
			this.listAddresses.Location = new System.Drawing.Point(7, 58);
			this.listAddresses.MultiSelect = false;
			this.listAddresses.Name = "listAddresses";
			this.listAddresses.Size = new System.Drawing.Size(749, 395);
			this.listAddresses.TabIndex = 4;
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
			this.lblNewAddress.Location = new System.Drawing.Point(4, 460);
			this.lblNewAddress.Name = "lblNewAddress";
			this.lblNewAddress.Size = new System.Drawing.Size(216, 13);
			this.lblNewAddress.TabIndex = 5;
			this.lblNewAddress.Text = "Add static address (e.g. 192.168.1.2/24):";
			//
			// txtNewAddress
			//
			this.txtNewAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtNewAddress.Location = new System.Drawing.Point(7, 478);
			this.txtNewAddress.Name = "txtNewAddress";
			this.txtNewAddress.Size = new System.Drawing.Size(226, 20);
			this.txtNewAddress.TabIndex = 6;
			//
			// btnAdd
			//
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAdd.Location = new System.Drawing.Point(239, 476);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(110, 24);
			this.btnAdd.TabIndex = 7;
			this.btnAdd.Text = "Add Static IP";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			//
			// btnDelete
			//
			this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDelete.Location = new System.Drawing.Point(616, 476);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(140, 24);
			this.btnDelete.TabIndex = 8;
			this.btnDelete.Text = "Delete Selected IP";
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			//
			// StaticIpTool
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblInterface);
			this.Controls.Add(this.comboInterfaces);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.lblInterfaceInfo);
			this.Controls.Add(this.listAddresses);
			this.Controls.Add(this.lblNewAddress);
			this.Controls.Add(this.txtNewAddress);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.btnDelete);
			this.Name = "StaticIpTool";
			this.Size = new System.Drawing.Size(760, 508);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblInterface;
		private System.Windows.Forms.ComboBox comboInterfaces;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Label lblInterfaceInfo;
		private System.Windows.Forms.ListView listAddresses;
		private System.Windows.Forms.ColumnHeader colAddress;
		private System.Windows.Forms.ColumnHeader colMask;
		private System.Windows.Forms.ColumnHeader colSource;
		private System.Windows.Forms.Label lblNewAddress;
		private System.Windows.Forms.TextBox txtNewAddress;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnDelete;
	}
}
