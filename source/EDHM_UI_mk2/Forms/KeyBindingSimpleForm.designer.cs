namespace EDHM_DX
{
	partial class KeyBindingSimpleForm
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
			this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.cboKeys = new DevExpress.XtraEditors.LookUpEdit();
			this.cboModifKeys = new DevExpress.XtraEditors.LookUpEdit();
			this.txtProfileName = new DevExpress.XtraEditors.TextEdit();
			this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
			this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
			this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
			this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
			this.dxErrorProvider1 = new DevExpress.XtraEditors.DXErrorProvider.DXErrorProvider(this.components);
			this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
			this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
			this.bar3 = new DevExpress.XtraBars.Bar();
			this.cmdAceptar = new DevExpress.XtraBars.BarButtonItem();
			this.cmdCancelar = new DevExpress.XtraBars.BarButtonItem();
			((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
			this.layoutControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cboKeys.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cboModifKeys.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtProfileName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
			this.SuspendLayout();
			// 
			// layoutControl1
			// 
			this.layoutControl1.Controls.Add(this.labelControl1);
			this.layoutControl1.Controls.Add(this.cboKeys);
			this.layoutControl1.Controls.Add(this.cboModifKeys);
			this.layoutControl1.Controls.Add(this.txtProfileName);
			this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutControl1.Location = new System.Drawing.Point(0, 0);
			this.layoutControl1.Name = "layoutControl1";
			this.layoutControl1.Root = this.layoutControlGroup1;
			this.layoutControl1.Size = new System.Drawing.Size(393, 148);
			this.layoutControl1.TabIndex = 0;
			this.layoutControl1.Text = "layoutControl1";
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(12, 123);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(255, 13);
			this.labelControl1.StyleController = this.layoutControl1;
			this.labelControl1.TabIndex = 9;
			this.labelControl1.Text = "* If the Profile already Exists, it will be Overwrited.";
			// 
			// cboKeys
			// 
			this.cboKeys.Location = new System.Drawing.Point(191, 68);
			this.cboKeys.Name = "cboKeys";
			this.cboKeys.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.cboKeys.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("code", "code", 20, DevExpress.Utils.FormatType.None, "", false, DevExpress.Utils.HorzAlignment.Default, DevExpress.Data.ColumnSortOrder.None, DevExpress.Utils.DefaultBoolean.Default),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("description", "Key")});
			this.cboKeys.Size = new System.Drawing.Size(190, 36);
			this.cboKeys.StyleController = this.layoutControl1;
			this.cboKeys.TabIndex = 6;
			// 
			// cboModifKeys
			// 
			this.cboModifKeys.Location = new System.Drawing.Point(12, 68);
			this.cboModifKeys.Name = "cboModifKeys";
			this.cboModifKeys.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.cboModifKeys.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("code", "code", 20, DevExpress.Utils.FormatType.None, "", false, DevExpress.Utils.HorzAlignment.Default, DevExpress.Data.ColumnSortOrder.None, DevExpress.Utils.DefaultBoolean.Default),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("description", "Key Modif")});
			this.cboModifKeys.Size = new System.Drawing.Size(175, 36);
			this.cboModifKeys.StyleController = this.layoutControl1;
			this.cboModifKeys.TabIndex = 5;
			// 
			// txtProfileName
			// 
			this.txtProfileName.Location = new System.Drawing.Point(83, 12);
			this.txtProfileName.Name = "txtProfileName";
			this.txtProfileName.Properties.ReadOnly = true;
			this.txtProfileName.Size = new System.Drawing.Size(298, 36);
			this.txtProfileName.StyleController = this.layoutControl1;
			this.txtProfileName.TabIndex = 4;
			// 
			// layoutControlGroup1
			// 
			this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
			this.layoutControlGroup1.GroupBordersVisible = false;
			this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.emptySpaceItem1,
            this.layoutControlItem6});
			this.layoutControlGroup1.Name = "layoutControlGroup1";
			this.layoutControlGroup1.Size = new System.Drawing.Size(393, 148);
			this.layoutControlGroup1.TextVisible = false;
			// 
			// layoutControlItem1
			// 
			this.layoutControlItem1.Control = this.txtProfileName;
			this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
			this.layoutControlItem1.Name = "layoutControlItem1";
			this.layoutControlItem1.Size = new System.Drawing.Size(373, 40);
			this.layoutControlItem1.Text = "Profile Name:";
			this.layoutControlItem1.TextSize = new System.Drawing.Size(68, 13);
			// 
			// layoutControlItem2
			// 
			this.layoutControlItem2.Control = this.cboModifKeys;
			this.layoutControlItem2.Location = new System.Drawing.Point(0, 40);
			this.layoutControlItem2.Name = "layoutControlItem2";
			this.layoutControlItem2.Size = new System.Drawing.Size(179, 56);
			this.layoutControlItem2.Text = "Modificator:";
			this.layoutControlItem2.TextLocation = DevExpress.Utils.Locations.Top;
			this.layoutControlItem2.TextSize = new System.Drawing.Size(68, 13);
			// 
			// layoutControlItem3
			// 
			this.layoutControlItem3.Control = this.cboKeys;
			this.layoutControlItem3.Location = new System.Drawing.Point(179, 40);
			this.layoutControlItem3.Name = "layoutControlItem3";
			this.layoutControlItem3.Size = new System.Drawing.Size(194, 56);
			this.layoutControlItem3.Text = "Key Binding:";
			this.layoutControlItem3.TextLocation = DevExpress.Utils.Locations.Top;
			this.layoutControlItem3.TextSize = new System.Drawing.Size(68, 13);
			// 
			// emptySpaceItem1
			// 
			this.emptySpaceItem1.AllowHotTrack = false;
			this.emptySpaceItem1.Location = new System.Drawing.Point(0, 96);
			this.emptySpaceItem1.Name = "emptySpaceItem1";
			this.emptySpaceItem1.Size = new System.Drawing.Size(373, 15);
			this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
			// 
			// layoutControlItem6
			// 
			this.layoutControlItem6.Control = this.labelControl1;
			this.layoutControlItem6.Location = new System.Drawing.Point(0, 111);
			this.layoutControlItem6.Name = "layoutControlItem6";
			this.layoutControlItem6.Size = new System.Drawing.Size(373, 17);
			this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
			this.layoutControlItem6.TextVisible = false;
			// 
			// dxErrorProvider1
			// 
			this.dxErrorProvider1.ContainerControl = this;
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
            this.cmdAceptar,
            this.cmdCancelar});
			this.barManager1.MaxItemId = 2;
			this.barManager1.StatusBar = this.bar3;
			// 
			// barDockControlTop
			// 
			this.barDockControlTop.CausesValidation = false;
			this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
			this.barDockControlTop.Manager = this.barManager1;
			this.barDockControlTop.Size = new System.Drawing.Size(393, 0);
			// 
			// barDockControlBottom
			// 
			this.barDockControlBottom.CausesValidation = false;
			this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.barDockControlBottom.Location = new System.Drawing.Point(0, 148);
			this.barDockControlBottom.Manager = this.barManager1;
			this.barDockControlBottom.Size = new System.Drawing.Size(393, 32);
			// 
			// barDockControlLeft
			// 
			this.barDockControlLeft.CausesValidation = false;
			this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
			this.barDockControlLeft.Manager = this.barManager1;
			this.barDockControlLeft.Size = new System.Drawing.Size(0, 148);
			// 
			// barDockControlRight
			// 
			this.barDockControlRight.CausesValidation = false;
			this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.barDockControlRight.Location = new System.Drawing.Point(393, 0);
			this.barDockControlRight.Manager = this.barManager1;
			this.barDockControlRight.Size = new System.Drawing.Size(0, 148);
			// 
			// bar3
			// 
			this.bar3.BarName = "Barra de estado";
			this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
			this.bar3.DockCol = 0;
			this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
			this.bar3.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.cmdAceptar),
            new DevExpress.XtraBars.LinkPersistInfo(this.cmdCancelar)});
			this.bar3.OptionsBar.AllowQuickCustomization = false;
			this.bar3.OptionsBar.DrawDragBorder = false;
			this.bar3.OptionsBar.UseWholeRow = true;
			this.bar3.Text = "Barra de estado";
			// 
			// cmdAceptar
			// 
			this.cmdAceptar.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
			this.cmdAceptar.Caption = "&OK";
			this.cmdAceptar.Id = 0;
			this.cmdAceptar.ItemAppearance.Normal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.cmdAceptar.ItemAppearance.Normal.Options.UseBackColor = true;
			this.cmdAceptar.Name = "cmdAceptar";
			this.cmdAceptar.Size = new System.Drawing.Size(100, 0);
			this.cmdAceptar.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.cmdAceptar_ItemClick);
			// 
			// cmdCancelar
			// 
			this.cmdCancelar.Caption = "&Cancel";
			this.cmdCancelar.Id = 1;
			this.cmdCancelar.Name = "cmdCancelar";
			this.cmdCancelar.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.cmdCancelar_ItemClick);
			// 
			// KeyBindingSimpleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(393, 180);
			this.Controls.Add(this.layoutControl1);
			this.Controls.Add(this.barDockControlLeft);
			this.Controls.Add(this.barDockControlRight);
			this.Controls.Add(this.barDockControlBottom);
			this.Controls.Add(this.barDockControlTop);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "KeyBindingSimpleForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "New Profile & Key Binding:";
			this.Load += new System.EventHandler(this.KeyBindingSimpleForm_Load);
			this.Shown += new System.EventHandler(this.KeyBindingSimpleForm_Shown);
			((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
			this.layoutControl1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.cboKeys.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cboModifKeys.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtProfileName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraLayout.LayoutControl layoutControl1;
		private DevExpress.XtraEditors.LookUpEdit cboKeys;
		private DevExpress.XtraEditors.LookUpEdit cboModifKeys;
		private DevExpress.XtraEditors.TextEdit txtProfileName;
		private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
		private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
		private DevExpress.XtraEditors.DXErrorProvider.DXErrorProvider dxErrorProvider1;
		private DevExpress.XtraBars.BarDockControl barDockControlLeft;
		private DevExpress.XtraBars.BarManager barManager1;
		private DevExpress.XtraBars.Bar bar3;
		private DevExpress.XtraBars.BarButtonItem cmdAceptar;
		private DevExpress.XtraBars.BarButtonItem cmdCancelar;
		private DevExpress.XtraBars.BarDockControl barDockControlTop;
		private DevExpress.XtraBars.BarDockControl barDockControlBottom;
		private DevExpress.XtraBars.BarDockControl barDockControlRight;
	}
}