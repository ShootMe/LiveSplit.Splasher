namespace LiveSplit.Splasher {
	partial class SplasherSettings {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.flowMain = new System.Windows.Forms.FlowLayoutPanel();
			this.flowOptions = new System.Windows.Forms.FlowLayoutPanel();
			this.chkAutoReset = new System.Windows.Forms.CheckBox();
			this.chkCheckpoints = new System.Windows.Forms.CheckBox();
			this.toolTips = new System.Windows.Forms.ToolTip(this.components);
			this.flowMain.SuspendLayout();
			this.flowOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowMain
			// 
			this.flowMain.AutoSize = true;
			this.flowMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowMain.Controls.Add(this.flowOptions);
			this.flowMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowMain.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowMain.Location = new System.Drawing.Point(0, 0);
			this.flowMain.Margin = new System.Windows.Forms.Padding(0);
			this.flowMain.Name = "flowMain";
			this.flowMain.Size = new System.Drawing.Size(279, 53);
			this.flowMain.TabIndex = 0;
			this.flowMain.WrapContents = false;
			// 
			// flowOptions
			// 
			this.flowOptions.Controls.Add(this.chkAutoReset);
			this.flowOptions.Controls.Add(this.chkCheckpoints);
			this.flowOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowOptions.Location = new System.Drawing.Point(0, 0);
			this.flowOptions.Margin = new System.Windows.Forms.Padding(0);
			this.flowOptions.Name = "flowOptions";
			this.flowOptions.Size = new System.Drawing.Size(279, 53);
			this.flowOptions.TabIndex = 0;
			// 
			// chkAutoReset
			// 
			this.chkAutoReset.AutoSize = true;
			this.chkAutoReset.Location = new System.Drawing.Point(3, 3);
			this.chkAutoReset.Name = "chkAutoReset";
			this.chkAutoReset.Size = new System.Drawing.Size(79, 17);
			this.chkAutoReset.TabIndex = 0;
			this.chkAutoReset.Text = "Auto Reset";
			this.toolTips.SetToolTip(this.chkAutoReset, "Will automatically reset your splits if you are on the main menu");
			this.chkAutoReset.UseVisualStyleBackColor = true;
			this.chkAutoReset.CheckedChanged += new System.EventHandler(this.chkBox_CheckedChanged);
			// 
			// chkCheckpoints
			// 
			this.chkCheckpoints.AutoSize = true;
			this.chkCheckpoints.Location = new System.Drawing.Point(3, 26);
			this.chkCheckpoints.Name = "chkCheckpoints";
			this.chkCheckpoints.Size = new System.Drawing.Size(85, 17);
			this.chkCheckpoints.TabIndex = 1;
			this.chkCheckpoints.Text = "Checkpoints";
			this.toolTips.SetToolTip(this.chkCheckpoints, "Splits after each checkpoint has been reached");
			this.chkCheckpoints.UseVisualStyleBackColor = true;
			this.chkCheckpoints.CheckedChanged += new System.EventHandler(this.chkBox_CheckedChanged);
			// 
			// SplasherSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.flowMain);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SplasherSettings";
			this.Size = new System.Drawing.Size(279, 53);
			this.Load += new System.EventHandler(this.Settings_Load);
			this.flowMain.ResumeLayout(false);
			this.flowOptions.ResumeLayout(false);
			this.flowOptions.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.FlowLayoutPanel flowMain;
		private System.Windows.Forms.FlowLayoutPanel flowOptions;
		private System.Windows.Forms.ToolTip toolTips;
		private System.Windows.Forms.CheckBox chkCheckpoints;
		private System.Windows.Forms.CheckBox chkAutoReset;
	}
}
