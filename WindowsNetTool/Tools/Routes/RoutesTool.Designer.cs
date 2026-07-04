namespace WindowsNetTool.Tools.Routes
{
	partial class RoutesTool
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
			this.listRoutes = new System.Windows.Forms.ListView();
			this.colPrefix = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colInterface = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colNextHop = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colMetric = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lblRoutePrefix = new System.Windows.Forms.Label();
			this.txtRoutePrefix = new System.Windows.Forms.TextBox();
			this.lblRouteInterface = new System.Windows.Forms.Label();
			this.comboRouteInterface = new System.Windows.Forms.ComboBox();
			this.lblRouteNextHop = new System.Windows.Forms.Label();
			this.txtRouteNextHop = new System.Windows.Forms.TextBox();
			this.lblRouteMetric = new System.Windows.Forms.Label();
			this.txtRouteMetric = new System.Windows.Forms.TextBox();
			this.btnAddRoute = new System.Windows.Forms.Button();
			this.btnRefreshRoutes = new System.Windows.Forms.Button();
			this.lblRouteHint = new System.Windows.Forms.Label();
			this.btnDeleteRoute = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// listRoutes
			// 
			this.listRoutes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listRoutes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPrefix,
            this.colInterface,
            this.colNextHop,
            this.colMetric});
			this.listRoutes.FullRowSelect = true;
			this.listRoutes.HideSelection = false;
			this.listRoutes.Location = new System.Drawing.Point(9, 9);
			this.listRoutes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.listRoutes.MultiSelect = false;
			this.listRoutes.Name = "listRoutes";
			this.listRoutes.Size = new System.Drawing.Size(649, 478);
			this.listRoutes.TabIndex = 0;
			this.listRoutes.UseCompatibleStateImageBehavior = false;
			this.listRoutes.View = System.Windows.Forms.View.Details;
			// 
			// colPrefix
			// 
			this.colPrefix.Text = "Destination Prefix";
			this.colPrefix.Width = 170;
			// 
			// colInterface
			// 
			this.colInterface.Text = "Interface";
			this.colInterface.Width = 230;
			// 
			// colNextHop
			// 
			this.colNextHop.Text = "Next Hop";
			this.colNextHop.Width = 140;
			// 
			// colMetric
			// 
			this.colMetric.Text = "Metric";
			this.colMetric.Width = 90;
			// 
			// lblRoutePrefix
			// 
			this.lblRoutePrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblRoutePrefix.AutoSize = true;
			this.lblRoutePrefix.Location = new System.Drawing.Point(4, 500);
			this.lblRoutePrefix.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRoutePrefix.Name = "lblRoutePrefix";
			this.lblRoutePrefix.Size = new System.Drawing.Size(43, 16);
			this.lblRoutePrefix.TabIndex = 1;
			this.lblRoutePrefix.Text = "Prefix:";
			// 
			// txtRoutePrefix
			// 
			this.txtRoutePrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtRoutePrefix.Location = new System.Drawing.Point(63, 495);
			this.txtRoutePrefix.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtRoutePrefix.Name = "txtRoutePrefix";
			this.txtRoutePrefix.Size = new System.Drawing.Size(172, 22);
			this.txtRoutePrefix.TabIndex = 2;
			// 
			// lblRouteInterface
			// 
			this.lblRouteInterface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblRouteInterface.AutoSize = true;
			this.lblRouteInterface.Location = new System.Drawing.Point(243, 500);
			this.lblRouteInterface.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRouteInterface.Name = "lblRouteInterface";
			this.lblRouteInterface.Size = new System.Drawing.Size(61, 16);
			this.lblRouteInterface.TabIndex = 3;
			this.lblRouteInterface.Text = "Interface:";
			// 
			// comboRouteInterface
			// 
			this.comboRouteInterface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.comboRouteInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboRouteInterface.FormattingEnabled = true;
			this.comboRouteInterface.Location = new System.Drawing.Point(315, 495);
			this.comboRouteInterface.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.comboRouteInterface.Name = "comboRouteInterface";
			this.comboRouteInterface.Size = new System.Drawing.Size(239, 24);
			this.comboRouteInterface.TabIndex = 4;
			// 
			// lblRouteNextHop
			// 
			this.lblRouteNextHop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblRouteNextHop.AutoSize = true;
			this.lblRouteNextHop.Location = new System.Drawing.Point(6, 530);
			this.lblRouteNextHop.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRouteNextHop.Name = "lblRouteNextHop";
			this.lblRouteNextHop.Size = new System.Drawing.Size(63, 16);
			this.lblRouteNextHop.TabIndex = 5;
			this.lblRouteNextHop.Text = "Next hop:";
			// 
			// txtRouteNextHop
			// 
			this.txtRouteNextHop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtRouteNextHop.Location = new System.Drawing.Point(81, 525);
			this.txtRouteNextHop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtRouteNextHop.Name = "txtRouteNextHop";
			this.txtRouteNextHop.Size = new System.Drawing.Size(145, 22);
			this.txtRouteNextHop.TabIndex = 6;
			// 
			// lblRouteMetric
			// 
			this.lblRouteMetric.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblRouteMetric.AutoSize = true;
			this.lblRouteMetric.Location = new System.Drawing.Point(234, 530);
			this.lblRouteMetric.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRouteMetric.Name = "lblRouteMetric";
			this.lblRouteMetric.Size = new System.Drawing.Size(46, 16);
			this.lblRouteMetric.TabIndex = 7;
			this.lblRouteMetric.Text = "Metric:";
			// 
			// txtRouteMetric
			// 
			this.txtRouteMetric.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtRouteMetric.Location = new System.Drawing.Point(293, 525);
			this.txtRouteMetric.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtRouteMetric.Name = "txtRouteMetric";
			this.txtRouteMetric.Size = new System.Drawing.Size(65, 22);
			this.txtRouteMetric.TabIndex = 8;
			// 
			// btnAddRoute
			// 
			this.btnAddRoute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAddRoute.Location = new System.Drawing.Point(5, 555);
			this.btnAddRoute.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnAddRoute.Name = "btnAddRoute";
			this.btnAddRoute.Size = new System.Drawing.Size(160, 30);
			this.btnAddRoute.TabIndex = 9;
			this.btnAddRoute.Text = "Add Route";
			this.btnAddRoute.UseVisualStyleBackColor = true;
			this.btnAddRoute.Click += new System.EventHandler(this.btnAddRoute_Click);
			// 
			// btnRefreshRoutes
			// 
			this.btnRefreshRoutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRefreshRoutes.Location = new System.Drawing.Point(173, 555);
			this.btnRefreshRoutes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnRefreshRoutes.Name = "btnRefreshRoutes";
			this.btnRefreshRoutes.Size = new System.Drawing.Size(120, 30);
			this.btnRefreshRoutes.TabIndex = 10;
			this.btnRefreshRoutes.Text = "Refresh";
			this.btnRefreshRoutes.UseVisualStyleBackColor = true;
			this.btnRefreshRoutes.Click += new System.EventHandler(this.btnRefreshRoutes_Click);
			// 
			// lblRouteHint
			// 
			this.lblRouteHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblRouteHint.AutoSize = true;
			this.lblRouteHint.Location = new System.Drawing.Point(6, 589);
			this.lblRouteHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRouteHint.Name = "lblRouteHint";
			this.lblRouteHint.Size = new System.Drawing.Size(541, 16);
			this.lblRouteHint.TabIndex = 11;
			this.lblRouteHint.Text = "Blank next hop = on-link.  Blank metric = automatic.  Routes are persistent (surv" +
    "ive reboots).";
			// 
			// btnDeleteRoute
			// 
			this.btnDeleteRoute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnDeleteRoute.Location = new System.Drawing.Point(367, 555);
			this.btnDeleteRoute.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnDeleteRoute.Name = "btnDeleteRoute";
			this.btnDeleteRoute.Size = new System.Drawing.Size(187, 30);
			this.btnDeleteRoute.TabIndex = 12;
			this.btnDeleteRoute.Text = "Delete Selected";
			this.btnDeleteRoute.UseVisualStyleBackColor = true;
			this.btnDeleteRoute.Click += new System.EventHandler(this.btnDeleteRoute_Click);
			// 
			// RoutesTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listRoutes);
			this.Controls.Add(this.lblRoutePrefix);
			this.Controls.Add(this.txtRoutePrefix);
			this.Controls.Add(this.lblRouteInterface);
			this.Controls.Add(this.comboRouteInterface);
			this.Controls.Add(this.lblRouteNextHop);
			this.Controls.Add(this.txtRouteNextHop);
			this.Controls.Add(this.lblRouteMetric);
			this.Controls.Add(this.txtRouteMetric);
			this.Controls.Add(this.btnAddRoute);
			this.Controls.Add(this.btnRefreshRoutes);
			this.Controls.Add(this.lblRouteHint);
			this.Controls.Add(this.btnDeleteRoute);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "RoutesTool";
			this.Size = new System.Drawing.Size(665, 611);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView listRoutes;
		private System.Windows.Forms.ColumnHeader colPrefix;
		private System.Windows.Forms.ColumnHeader colInterface;
		private System.Windows.Forms.ColumnHeader colNextHop;
		private System.Windows.Forms.ColumnHeader colMetric;
		private System.Windows.Forms.Label lblRoutePrefix;
		private System.Windows.Forms.TextBox txtRoutePrefix;
		private System.Windows.Forms.Label lblRouteInterface;
		private System.Windows.Forms.ComboBox comboRouteInterface;
		private System.Windows.Forms.Label lblRouteNextHop;
		private System.Windows.Forms.TextBox txtRouteNextHop;
		private System.Windows.Forms.Label lblRouteMetric;
		private System.Windows.Forms.TextBox txtRouteMetric;
		private System.Windows.Forms.Button btnAddRoute;
		private System.Windows.Forms.Button btnRefreshRoutes;
		private System.Windows.Forms.Label lblRouteHint;
		private System.Windows.Forms.Button btnDeleteRoute;
	}
}
