namespace WindowsNetTool
{
	partial class MainForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.listBoxTools = new System.Windows.Forms.ListBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Margin = new System.Windows.Forms.Padding(4);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.listBoxTools);
			this.splitContainer.Panel1MinSize = 120;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.AutoScroll = true;
			this.splitContainer.Size = new System.Drawing.Size(802, 838);
			this.splitContainer.SplitterDistance = 120;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.TabIndex = 0;
			// 
			// listBoxTools
			// 
			this.listBoxTools.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxTools.FormattingEnabled = true;
			this.listBoxTools.IntegralHeight = false;
			this.listBoxTools.ItemHeight = 16;
			this.listBoxTools.Location = new System.Drawing.Point(0, 0);
			this.listBoxTools.Margin = new System.Windows.Forms.Padding(4);
			this.listBoxTools.Name = "listBoxTools";
			this.listBoxTools.Size = new System.Drawing.Size(120, 838);
			this.listBoxTools.TabIndex = 0;
			this.listBoxTools.SelectedIndexChanged += new System.EventHandler(this.listBoxTools_SelectedIndexChanged);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(802, 838);
			this.Controls.Add(this.splitContainer);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(780, 540);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "WindowsNetTool";
			this.splitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.ListBox listBoxTools;
	}
}
