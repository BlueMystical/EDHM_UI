namespace EDHM_UI_mk2.Forms
{
	partial class PreviewForm
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
			this.components = new System.ComponentModel.Container();
			this.picturePreview = new DevExpress.XtraEditors.PictureEdit();
			this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
			this.bar3 = new DevExpress.XtraBars.Bar();
			this.barToggleSwitchItem1 = new DevExpress.XtraBars.BarToggleSwitchItem();
			this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
			this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
			this.repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
			((System.ComponentModel.ISupportInitialize)(this.picturePreview.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).BeginInit();
			this.SuspendLayout();
			// 
			// picturePreview
			// 
			this.picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.picturePreview.Location = new System.Drawing.Point(0, 0);
			this.picturePreview.Name = "picturePreview";
			this.picturePreview.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
			this.picturePreview.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
			this.picturePreview.Size = new System.Drawing.Size(494, 281);
			this.picturePreview.TabIndex = 0;
			this.picturePreview.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picturePreview_MouseClick);
			// 
			// barManager1
			// 
			this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar3});
			this.barManager1.DockControls.Add(this.barDockControlTop);
			this.barManager1.DockControls.Add(this.barDockControlBottom);
			this.barManager1.DockControls.Add(this.barDockControlLeft);
			this.barManager1.DockControls.Add(this.barDockControlRight);
			this.barManager1.Form = this;
			this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barToggleSwitchItem1,
            this.barStaticItem1});
			this.barManager1.MaxItemId = 5;
			this.barManager1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemToggleSwitch1});
			this.barManager1.StatusBar = this.bar3;
			// 
			// bar3
			// 
			this.bar3.BarName = "Barra de estado";
			this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
			this.bar3.DockCol = 0;
			this.bar3.DockRow = 0;
			this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
			this.bar3.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barToggleSwitchItem1),
            new DevExpress.XtraBars.LinkPersistInfo(this.barStaticItem1)});
			this.bar3.OptionsBar.AllowQuickCustomization = false;
			this.bar3.OptionsBar.DrawDragBorder = false;
			this.bar3.OptionsBar.UseWholeRow = true;
			this.bar3.Text = "Barra de estado";
			// 
			// barToggleSwitchItem1
			// 
			this.barToggleSwitchItem1.Caption = "Combat Mode";
			this.barToggleSwitchItem1.Id = 1;
			this.barToggleSwitchItem1.Name = "barToggleSwitchItem1";
			this.barToggleSwitchItem1.CheckedChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.barToggleSwitchItem1_CheckedChanged);
			// 
			// barStaticItem1
			// 
			this.barStaticItem1.Caption = "* Some Elements may not look exactly as Preview.";
			this.barStaticItem1.Id = 4;
			this.barStaticItem1.Name = "barStaticItem1";
			// 
			// barDockControlTop
			// 
			this.barDockControlTop.CausesValidation = false;
			this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
			this.barDockControlTop.Manager = this.barManager1;
			this.barDockControlTop.Size = new System.Drawing.Size(494, 0);
			// 
			// barDockControlBottom
			// 
			this.barDockControlBottom.CausesValidation = false;
			this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.barDockControlBottom.Location = new System.Drawing.Point(0, 281);
			this.barDockControlBottom.Manager = this.barManager1;
			this.barDockControlBottom.Size = new System.Drawing.Size(494, 32);
			// 
			// barDockControlLeft
			// 
			this.barDockControlLeft.CausesValidation = false;
			this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
			this.barDockControlLeft.Manager = this.barManager1;
			this.barDockControlLeft.Size = new System.Drawing.Size(0, 281);
			// 
			// barDockControlRight
			// 
			this.barDockControlRight.CausesValidation = false;
			this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.barDockControlRight.Location = new System.Drawing.Point(494, 0);
			this.barDockControlRight.Manager = this.barManager1;
			this.barDockControlRight.Size = new System.Drawing.Size(0, 281);
			// 
			// repositoryItemToggleSwitch1
			// 
			this.repositoryItemToggleSwitch1.AutoHeight = false;
			this.repositoryItemToggleSwitch1.Name = "repositoryItemToggleSwitch1";
			this.repositoryItemToggleSwitch1.OffText = "Off";
			this.repositoryItemToggleSwitch1.OnText = "On";
			// 
			// PreviewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(494, 313);
			this.Controls.Add(this.picturePreview);
			this.Controls.Add(this.barDockControlLeft);
			this.Controls.Add(this.barDockControlRight);
			this.Controls.Add(this.barDockControlBottom);
			this.Controls.Add(this.barDockControlTop);
			this.LookAndFeel.TouchScaleFactor = 1F;
			this.LookAndFeel.TouchUIMode = DevExpress.Utils.DefaultBoolean.False;
			this.Name = "PreviewForm";
			this.Text = "PreviewForm";
			this.Load += new System.EventHandler(this.PreviewForm_Load);
			this.Shown += new System.EventHandler(this.PreviewForm_Shown);
			this.ResizeEnd += new System.EventHandler(this.PreviewForm_ResizeEnd);
			((System.ComponentModel.ISupportInitialize)(this.picturePreview.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.PictureEdit picturePreview;
		private DevExpress.XtraBars.BarManager barManager1;
		private DevExpress.XtraBars.Bar bar3;
		private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch1;
		private DevExpress.XtraBars.BarDockControl barDockControlTop;
		private DevExpress.XtraBars.BarDockControl barDockControlBottom;
		private DevExpress.XtraBars.BarDockControl barDockControlLeft;
		private DevExpress.XtraBars.BarDockControl barDockControlRight;
		private DevExpress.XtraBars.BarToggleSwitchItem barToggleSwitchItem1;
		private DevExpress.XtraBars.BarStaticItem barStaticItem1;
	}
}