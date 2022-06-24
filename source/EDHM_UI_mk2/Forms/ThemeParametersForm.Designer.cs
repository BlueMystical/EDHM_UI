namespace EDHM_UI_mk2.Forms
{
	partial class ThemeParametersForm
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
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
			this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
			this.picThumb = new DevExpress.XtraEditors.PictureEdit();
			this.cmdCancel = new DevExpress.XtraEditors.SimpleButton();
			this.cmdOK = new DevExpress.XtraEditors.SimpleButton();
			this.separatorControl1 = new DevExpress.XtraEditors.SeparatorControl();
			this.txtDescription = new DevExpress.XtraEditors.TextEdit();
			this.txtAuthor = new DevExpress.XtraEditors.TextEdit();
			this.txtName = new DevExpress.XtraEditors.TextEdit();
			this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
			this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
			this.txtModName = new DevExpress.XtraEditors.TextEdit();
			this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
			((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
			this.layoutControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picThumb.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtAuthor.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtModName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
			this.SuspendLayout();
			// 
			// layoutControl1
			// 
			this.layoutControl1.Controls.Add(this.txtModName);
			this.layoutControl1.Controls.Add(this.picThumb);
			this.layoutControl1.Controls.Add(this.cmdCancel);
			this.layoutControl1.Controls.Add(this.cmdOK);
			this.layoutControl1.Controls.Add(this.separatorControl1);
			this.layoutControl1.Controls.Add(this.txtDescription);
			this.layoutControl1.Controls.Add(this.txtAuthor);
			this.layoutControl1.Controls.Add(this.txtName);
			this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutControl1.Location = new System.Drawing.Point(0, 0);
			this.layoutControl1.Name = "layoutControl1";
			this.layoutControl1.Root = this.Root;
			this.layoutControl1.Size = new System.Drawing.Size(362, 380);
			this.layoutControl1.TabIndex = 0;
			this.layoutControl1.Text = "layoutControl1";
			// 
			// picThumb
			// 
			this.picThumb.EditValue = global::EDHM_UI_mk2.Properties.Resources._3PM_Default_2;
			this.picThumb.Location = new System.Drawing.Point(12, 188);
			this.picThumb.Name = "picThumb";
			this.picThumb.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
			this.picThumb.Size = new System.Drawing.Size(338, 94);
			this.picThumb.StyleController = this.layoutControl1;
			toolTipTitleItem1.Text = "Instructions:";
			toolTipItem1.Text = "- Right click and save this image to open it with your favorite editor.";
			toolTipItem2.Text = "- Then right click and Open the edited image.";
			superToolTip1.Items.Add(toolTipTitleItem1);
			superToolTip1.Items.Add(toolTipItem1);
			superToolTip1.Items.Add(toolTipItem2);
			this.picThumb.SuperTip = superToolTip1;
			this.picThumb.TabIndex = 10;
			// 
			// cmdCancel
			// 
			this.cmdCancel.Location = new System.Drawing.Point(12, 326);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(158, 42);
			this.cmdCancel.StyleController = this.layoutControl1;
			this.cmdCancel.TabIndex = 9;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// cmdOK
			// 
			this.cmdOK.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.cmdOK.Appearance.ForeColor = System.Drawing.Color.Black;
			this.cmdOK.Appearance.Options.UseBackColor = true;
			this.cmdOK.Appearance.Options.UseForeColor = true;
			this.cmdOK.Location = new System.Drawing.Point(174, 326);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(176, 42);
			this.cmdOK.StyleController = this.layoutControl1;
			this.cmdOK.TabIndex = 8;
			this.cmdOK.Text = "&OK";
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// separatorControl1
			// 
			this.separatorControl1.Location = new System.Drawing.Point(12, 286);
			this.separatorControl1.Name = "separatorControl1";
			this.separatorControl1.Size = new System.Drawing.Size(338, 36);
			this.separatorControl1.TabIndex = 7;
			// 
			// txtDescription
			// 
			this.txtDescription.EditValue = "";
			this.txtDescription.Location = new System.Drawing.Point(93, 132);
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.Size = new System.Drawing.Size(257, 36);
			this.txtDescription.StyleController = this.layoutControl1;
			this.txtDescription.TabIndex = 6;
			// 
			// txtAuthor
			// 
			this.txtAuthor.Location = new System.Drawing.Point(93, 92);
			this.txtAuthor.Name = "txtAuthor";
			this.txtAuthor.Size = new System.Drawing.Size(257, 36);
			this.txtAuthor.StyleController = this.layoutControl1;
			this.txtAuthor.TabIndex = 5;
			// 
			// txtName
			// 
			this.txtName.Location = new System.Drawing.Point(93, 52);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(257, 36);
			this.txtName.StyleController = this.layoutControl1;
			this.txtName.TabIndex = 4;
			// 
			// Root
			// 
			this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
			this.Root.GroupBordersVisible = false;
			this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem6,
            this.layoutControlItem7,
            this.layoutControlItem8});
			this.Root.Name = "Root";
			this.Root.Size = new System.Drawing.Size(362, 380);
			this.Root.TextVisible = false;
			// 
			// layoutControlItem1
			// 
			this.layoutControlItem1.Control = this.txtName;
			this.layoutControlItem1.Location = new System.Drawing.Point(0, 40);
			this.layoutControlItem1.Name = "layoutControlItem1";
			this.layoutControlItem1.Size = new System.Drawing.Size(342, 40);
			this.layoutControlItem1.Text = "Theme Name:";
			this.layoutControlItem1.TextSize = new System.Drawing.Size(69, 13);
			// 
			// layoutControlItem2
			// 
			this.layoutControlItem2.Control = this.txtAuthor;
			this.layoutControlItem2.Location = new System.Drawing.Point(0, 80);
			this.layoutControlItem2.Name = "layoutControlItem2";
			this.layoutControlItem2.Size = new System.Drawing.Size(342, 40);
			this.layoutControlItem2.Text = "Author:";
			this.layoutControlItem2.TextSize = new System.Drawing.Size(69, 13);
			// 
			// layoutControlItem3
			// 
			this.layoutControlItem3.Control = this.txtDescription;
			this.layoutControlItem3.Location = new System.Drawing.Point(0, 120);
			this.layoutControlItem3.Name = "layoutControlItem3";
			this.layoutControlItem3.Size = new System.Drawing.Size(342, 40);
			this.layoutControlItem3.Text = "Description:";
			this.layoutControlItem3.TextSize = new System.Drawing.Size(69, 13);
			// 
			// layoutControlItem4
			// 
			this.layoutControlItem4.Control = this.separatorControl1;
			this.layoutControlItem4.Location = new System.Drawing.Point(0, 274);
			this.layoutControlItem4.Name = "layoutControlItem4";
			this.layoutControlItem4.Size = new System.Drawing.Size(342, 40);
			this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
			this.layoutControlItem4.TextVisible = false;
			// 
			// layoutControlItem5
			// 
			this.layoutControlItem5.Control = this.cmdOK;
			this.layoutControlItem5.Location = new System.Drawing.Point(162, 314);
			this.layoutControlItem5.Name = "layoutControlItem5";
			this.layoutControlItem5.Size = new System.Drawing.Size(180, 46);
			this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
			this.layoutControlItem5.TextVisible = false;
			// 
			// layoutControlItem6
			// 
			this.layoutControlItem6.Control = this.cmdCancel;
			this.layoutControlItem6.Location = new System.Drawing.Point(0, 314);
			this.layoutControlItem6.Name = "layoutControlItem6";
			this.layoutControlItem6.Size = new System.Drawing.Size(162, 46);
			this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
			this.layoutControlItem6.TextVisible = false;
			// 
			// layoutControlItem7
			// 
			this.layoutControlItem7.Control = this.picThumb;
			this.layoutControlItem7.Location = new System.Drawing.Point(0, 160);
			this.layoutControlItem7.Name = "layoutControlItem7";
			this.layoutControlItem7.Size = new System.Drawing.Size(342, 114);
			this.layoutControlItem7.Text = "Thumbnail:";
			this.layoutControlItem7.TextLocation = DevExpress.Utils.Locations.Top;
			this.layoutControlItem7.TextSize = new System.Drawing.Size(69, 13);
			// 
			// txtModName
			// 
			this.txtModName.Enabled = false;
			this.txtModName.Location = new System.Drawing.Point(93, 12);
			this.txtModName.Name = "txtModName";
			this.txtModName.Properties.ReadOnly = true;
			this.txtModName.Size = new System.Drawing.Size(257, 36);
			this.txtModName.StyleController = this.layoutControl1;
			this.txtModName.TabIndex = 11;
			// 
			// layoutControlItem8
			// 
			this.layoutControlItem8.Control = this.txtModName;
			this.layoutControlItem8.Location = new System.Drawing.Point(0, 0);
			this.layoutControlItem8.Name = "layoutControlItem8";
			this.layoutControlItem8.Size = new System.Drawing.Size(342, 40);
			this.layoutControlItem8.Text = "Mod Name:";
			this.layoutControlItem8.TextSize = new System.Drawing.Size(69, 13);
			// 
			// ThemeParametersForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(362, 380);
			this.Controls.Add(this.layoutControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ThemeParametersForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Theme Parameters:";
			this.Load += new System.EventHandler(this.ThemeParametersForm_Load);
			this.Shown += new System.EventHandler(this.ThemeParametersForm_Shown);
			((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
			this.layoutControl1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.picThumb.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtAuthor.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtModName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraLayout.LayoutControl layoutControl1;
		private DevExpress.XtraLayout.LayoutControlGroup Root;
		private DevExpress.XtraEditors.SimpleButton cmdCancel;
		private DevExpress.XtraEditors.SimpleButton cmdOK;
		private DevExpress.XtraEditors.SeparatorControl separatorControl1;
		private DevExpress.XtraEditors.TextEdit txtDescription;
		private DevExpress.XtraEditors.TextEdit txtAuthor;
		private DevExpress.XtraEditors.TextEdit txtName;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
		private DevExpress.XtraEditors.PictureEdit picThumb;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
		private DevExpress.XtraEditors.TextEdit txtModName;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
	}
}