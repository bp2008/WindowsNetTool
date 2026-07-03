namespace WindowsNetTool.Tools.NetworkCategory
{
	partial class NetworkCategoryTool
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
			this.listNetworks = new System.Windows.Forms.ListView();
			this.colNetwork = new System.Windows.Forms.ColumnHeader();
			this.colCategory = new System.Windows.Forms.ColumnHeader();
			this.colInternet = new System.Windows.Forms.ColumnHeader();
			this.btnCheckAll = new System.Windows.Forms.Button();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.lblSetChecked = new System.Windows.Forms.Label();
			this.btnPublic = new System.Windows.Forms.Button();
			this.btnPrivate = new System.Windows.Forms.Button();
			this.btnDomain = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// listNetworks
			//
			this.listNetworks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.listNetworks.CheckBoxes = true;
			this.listNetworks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.colNetwork,
			this.colCategory,
			this.colInternet});
			this.listNetworks.FullRowSelect = true;
			this.listNetworks.HideSelection = false;
			this.listNetworks.Location = new System.Drawing.Point(7, 7);
			this.listNetworks.Name = "listNetworks";
			this.listNetworks.Size = new System.Drawing.Size(749, 455);
			this.listNetworks.TabIndex = 0;
			this.listNetworks.UseCompatibleStateImageBehavior = false;
			this.listNetworks.View = System.Windows.Forms.View.Details;
			//
			// colNetwork
			//
			this.colNetwork.Text = "Network";
			this.colNetwork.Width = 320;
			//
			// colCategory
			//
			this.colCategory.Text = "Category";
			this.colCategory.Width = 140;
			//
			// colInternet
			//
			this.colInternet.Text = "Internet";
			this.colInternet.Width = 120;
			//
			// btnCheckAll
			//
			this.btnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnCheckAll.Location = new System.Drawing.Point(7, 476);
			this.btnCheckAll.Name = "btnCheckAll";
			this.btnCheckAll.Size = new System.Drawing.Size(100, 24);
			this.btnCheckAll.TabIndex = 1;
			this.btnCheckAll.Text = "Check All";
			this.btnCheckAll.UseVisualStyleBackColor = true;
			this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
			//
			// btnRefresh
			//
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRefresh.Location = new System.Drawing.Point(113, 476);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(100, 24);
			this.btnRefresh.TabIndex = 2;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			//
			// lblSetChecked
			//
			this.lblSetChecked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSetChecked.AutoSize = true;
			this.lblSetChecked.Location = new System.Drawing.Point(398, 481);
			this.lblSetChecked.Name = "lblSetChecked";
			this.lblSetChecked.Size = new System.Drawing.Size(85, 13);
			this.lblSetChecked.TabIndex = 3;
			this.lblSetChecked.Text = "Set checked to:";
			//
			// btnPublic
			//
			this.btnPublic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPublic.Location = new System.Drawing.Point(490, 476);
			this.btnPublic.Name = "btnPublic";
			this.btnPublic.Size = new System.Drawing.Size(80, 24);
			this.btnPublic.TabIndex = 4;
			this.btnPublic.Text = "Public";
			this.btnPublic.UseVisualStyleBackColor = true;
			this.btnPublic.Click += new System.EventHandler(this.btnPublic_Click);
			//
			// btnPrivate
			//
			this.btnPrivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPrivate.Location = new System.Drawing.Point(576, 476);
			this.btnPrivate.Name = "btnPrivate";
			this.btnPrivate.Size = new System.Drawing.Size(80, 24);
			this.btnPrivate.TabIndex = 5;
			this.btnPrivate.Text = "Private";
			this.btnPrivate.UseVisualStyleBackColor = true;
			this.btnPrivate.Click += new System.EventHandler(this.btnPrivate_Click);
			//
			// btnDomain
			//
			this.btnDomain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDomain.Location = new System.Drawing.Point(662, 476);
			this.btnDomain.Name = "btnDomain";
			this.btnDomain.Size = new System.Drawing.Size(80, 24);
			this.btnDomain.TabIndex = 6;
			this.btnDomain.Text = "Domain";
			this.btnDomain.UseVisualStyleBackColor = true;
			this.btnDomain.Click += new System.EventHandler(this.btnDomain_Click);
			//
			// NetworkCategoryTool
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listNetworks);
			this.Controls.Add(this.btnCheckAll);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.lblSetChecked);
			this.Controls.Add(this.btnPublic);
			this.Controls.Add(this.btnPrivate);
			this.Controls.Add(this.btnDomain);
			this.Name = "NetworkCategoryTool";
			this.Size = new System.Drawing.Size(760, 508);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView listNetworks;
		private System.Windows.Forms.ColumnHeader colNetwork;
		private System.Windows.Forms.ColumnHeader colCategory;
		private System.Windows.Forms.ColumnHeader colInternet;
		private System.Windows.Forms.Button btnCheckAll;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Label lblSetChecked;
		private System.Windows.Forms.Button btnPublic;
		private System.Windows.Forms.Button btnPrivate;
		private System.Windows.Forms.Button btnDomain;
	}
}
