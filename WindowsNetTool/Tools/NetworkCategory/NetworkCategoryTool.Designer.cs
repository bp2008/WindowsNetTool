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
			this.colNetwork = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colCategory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colInternet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnCheckAll = new System.Windows.Forms.Button();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.btnRenameNetwork = new System.Windows.Forms.Button();
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
			this.listNetworks.Location = new System.Drawing.Point(9, 9);
			this.listNetworks.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.listNetworks.Name = "listNetworks";
			this.listNetworks.Size = new System.Drawing.Size(937, 542);
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
			this.btnCheckAll.Location = new System.Drawing.Point(9, 559);
			this.btnCheckAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnCheckAll.Name = "btnCheckAll";
			this.btnCheckAll.Size = new System.Drawing.Size(133, 30);
			this.btnCheckAll.TabIndex = 1;
			this.btnCheckAll.Text = "Check All";
			this.btnCheckAll.UseVisualStyleBackColor = true;
			this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
			// 
			// btnRefresh
			// 
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRefresh.Location = new System.Drawing.Point(151, 559);
			this.btnRefresh.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(133, 30);
			this.btnRefresh.TabIndex = 2;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// btnRenameNetwork
			// 
			this.btnRenameNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRenameNetwork.Location = new System.Drawing.Point(292, 559);
			this.btnRenameNetwork.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnRenameNetwork.Name = "btnRenameNetwork";
			this.btnRenameNetwork.Size = new System.Drawing.Size(133, 30);
			this.btnRenameNetwork.TabIndex = 7;
			this.btnRenameNetwork.Text = "Rename...";
			this.btnRenameNetwork.UseVisualStyleBackColor = true;
			this.btnRenameNetwork.Click += new System.EventHandler(this.btnRenameNetwork_Click);
			// 
			// lblSetChecked
			// 
			this.lblSetChecked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblSetChecked.AutoSize = true;
			this.lblSetChecked.Location = new System.Drawing.Point(12, 603);
			this.lblSetChecked.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSetChecked.Name = "lblSetChecked";
			this.lblSetChecked.Size = new System.Drawing.Size(99, 16);
			this.lblSetChecked.TabIndex = 3;
			this.lblSetChecked.Text = "Set checked to:";
			// 
			// btnPublic
			// 
			this.btnPublic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnPublic.Location = new System.Drawing.Point(134, 597);
			this.btnPublic.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnPublic.Name = "btnPublic";
			this.btnPublic.Size = new System.Drawing.Size(107, 30);
			this.btnPublic.TabIndex = 4;
			this.btnPublic.Text = "Public";
			this.btnPublic.UseVisualStyleBackColor = true;
			this.btnPublic.Click += new System.EventHandler(this.btnPublic_Click);
			// 
			// btnPrivate
			// 
			this.btnPrivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnPrivate.Location = new System.Drawing.Point(249, 597);
			this.btnPrivate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnPrivate.Name = "btnPrivate";
			this.btnPrivate.Size = new System.Drawing.Size(107, 30);
			this.btnPrivate.TabIndex = 5;
			this.btnPrivate.Text = "Private";
			this.btnPrivate.UseVisualStyleBackColor = true;
			this.btnPrivate.Click += new System.EventHandler(this.btnPrivate_Click);
			// 
			// btnDomain
			// 
			this.btnDomain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnDomain.Location = new System.Drawing.Point(364, 597);
			this.btnDomain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnDomain.Name = "btnDomain";
			this.btnDomain.Size = new System.Drawing.Size(107, 30);
			this.btnDomain.TabIndex = 6;
			this.btnDomain.Text = "Domain";
			this.btnDomain.UseVisualStyleBackColor = true;
			this.btnDomain.Click += new System.EventHandler(this.btnDomain_Click);
			// 
			// NetworkCategoryTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listNetworks);
			this.Controls.Add(this.btnCheckAll);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.btnRenameNetwork);
			this.Controls.Add(this.lblSetChecked);
			this.Controls.Add(this.btnPublic);
			this.Controls.Add(this.btnPrivate);
			this.Controls.Add(this.btnDomain);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "NetworkCategoryTool";
			this.Size = new System.Drawing.Size(953, 631);
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
		private System.Windows.Forms.Button btnRenameNetwork;
		private System.Windows.Forms.Label lblSetChecked;
		private System.Windows.Forms.Button btnPublic;
		private System.Windows.Forms.Button btnPrivate;
		private System.Windows.Forms.Button btnDomain;
	}
}
