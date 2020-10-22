namespace EDHM_UI
{
	partial class ShadersEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShadersEditor));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.cboProfiles = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.cmdCopyProfileAs = new System.Windows.Forms.ToolStripButton();
			this.cmdSaveProfile = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.cmdImportProfile = new System.Windows.Forms.ToolStripButton();
			this.cmdExportProfile = new System.Windows.Forms.ToolStripButton();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.cmdAddNewShader = new System.Windows.Forms.ToolStripButton();
			this.cmdRemoveShader = new System.Windows.Forms.ToolStripButton();
			this.cmdSaveShaderChanges = new System.Windows.Forms.ToolStripButton();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.toolStrip1.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.cboProfiles,
            this.toolStripSeparator2,
            this.cmdCopyProfileAs,
            this.cmdSaveProfile,
            this.toolStripSeparator1,
            this.cmdImportProfile,
            this.cmdExportProfile});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(384, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(47, 22);
			this.toolStripLabel1.Text = "Presets:";
			// 
			// cboProfiles
			// 
			this.cboProfiles.Items.AddRange(new object[] {
            "No Profiles Yet"});
			this.cboProfiles.Name = "cboProfiles";
			this.cboProfiles.Size = new System.Drawing.Size(150, 25);
			this.cboProfiles.DropDownClosed += new System.EventHandler(this.cboProfiles_DropDownClosed);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// cmdCopyProfileAs
			// 
			this.cmdCopyProfileAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdCopyProfileAs.Image = global::EDHM_UI.Properties.Resources._1352900428_page_2_copy;
			this.cmdCopyProfileAs.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdCopyProfileAs.Name = "cmdCopyProfileAs";
			this.cmdCopyProfileAs.Size = new System.Drawing.Size(23, 22);
			this.cmdCopyProfileAs.Text = "Clone Preset..";
			this.cmdCopyProfileAs.ToolTipText = "Clone Preset..";
			this.cmdCopyProfileAs.Click += new System.EventHandler(this.cmdCopyProfileAs_Click);
			// 
			// cmdSaveProfile
			// 
			this.cmdSaveProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdSaveProfile.Image = global::EDHM_UI.Properties.Resources._1352899444_disk;
			this.cmdSaveProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdSaveProfile.Name = "cmdSaveProfile";
			this.cmdSaveProfile.Size = new System.Drawing.Size(23, 22);
			this.cmdSaveProfile.Text = "Save Preset";
			this.cmdSaveProfile.ToolTipText = "Save Preset";
			this.cmdSaveProfile.Click += new System.EventHandler(this.cmdSaveProfile_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// cmdImportProfile
			// 
			this.cmdImportProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdImportProfile.Image = global::EDHM_UI.Properties.Resources.folder_table;
			this.cmdImportProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdImportProfile.Name = "cmdImportProfile";
			this.cmdImportProfile.Size = new System.Drawing.Size(23, 22);
			this.cmdImportProfile.Text = "toolStripButton2";
			this.cmdImportProfile.ToolTipText = "Import Preset";
			this.cmdImportProfile.Click += new System.EventHandler(this.cmdImportProfile_Click);
			// 
			// cmdExportProfile
			// 
			this.cmdExportProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdExportProfile.Image = global::EDHM_UI.Properties.Resources.lightning_go;
			this.cmdExportProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdExportProfile.Name = "cmdExportProfile";
			this.cmdExportProfile.Size = new System.Drawing.Size(23, 22);
			this.cmdExportProfile.Text = "toolStripButton2";
			this.cmdExportProfile.ToolTipText = "Export Preset";
			this.cmdExportProfile.Click += new System.EventHandler(this.cmdExportProfile_Click);
			// 
			// toolStrip2
			// 
			this.toolStrip2.Dock = System.Windows.Forms.DockStyle.Right;
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdAddNewShader,
            this.cmdRemoveShader,
            this.cmdSaveShaderChanges});
			this.toolStrip2.Location = new System.Drawing.Point(352, 25);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(32, 439);
			this.toolStrip2.TabIndex = 2;
			this.toolStrip2.Text = "toolStrip2";
			// 
			// cmdAddNewShader
			// 
			this.cmdAddNewShader.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdAddNewShader.Image = ((System.Drawing.Image)(resources.GetObject("cmdAddNewShader.Image")));
			this.cmdAddNewShader.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdAddNewShader.Name = "cmdAddNewShader";
			this.cmdAddNewShader.Size = new System.Drawing.Size(29, 20);
			this.cmdAddNewShader.Text = "toolStripButton1";
			this.cmdAddNewShader.ToolTipText = "Add New Shader";
			this.cmdAddNewShader.Click += new System.EventHandler(this.cmdAddNewShader_Click);
			// 
			// cmdRemoveShader
			// 
			this.cmdRemoveShader.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdRemoveShader.Image = global::EDHM_UI.Properties.Resources.delete;
			this.cmdRemoveShader.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdRemoveShader.Name = "cmdRemoveShader";
			this.cmdRemoveShader.Size = new System.Drawing.Size(29, 20);
			this.cmdRemoveShader.Text = "Remove Shader";
			this.cmdRemoveShader.ToolTipText = "Remove Selected Shader";
			this.cmdRemoveShader.Click += new System.EventHandler(this.cmdRemoveShader_Click);
			// 
			// cmdSaveShaderChanges
			// 
			this.cmdSaveShaderChanges.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cmdSaveShaderChanges.Image = global::EDHM_UI.Properties.Resources.disk_multiple;
			this.cmdSaveShaderChanges.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cmdSaveShaderChanges.Name = "cmdSaveShaderChanges";
			this.cmdSaveShaderChanges.Size = new System.Drawing.Size(29, 20);
			this.cmdSaveShaderChanges.Text = "Apply Shaders";
			this.cmdSaveShaderChanges.Click += new System.EventHandler(this.cmdSaveShaderChanges_Click);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoScroll = true;
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 25);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(352, 439);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// ShadersEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 464);
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.toolStrip2);
			this.Controls.Add(this.toolStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ShadersEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "EDHM UI Editor";
			this.Load += new System.EventHandler(this.ShadersEditor_Load);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripComboBox cboProfiles;
		private System.Windows.Forms.ToolStripButton cmdCopyProfileAs;
		private System.Windows.Forms.ToolStripButton cmdSaveProfile;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripButton cmdAddNewShader;
		private System.Windows.Forms.ToolStripButton cmdRemoveShader;
		private System.Windows.Forms.ToolStripButton cmdImportProfile;
		private System.Windows.Forms.ToolStripButton cmdExportProfile;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton cmdSaveShaderChanges;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
	}
}