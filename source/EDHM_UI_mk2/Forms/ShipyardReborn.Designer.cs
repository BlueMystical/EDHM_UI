namespace EDHM_UI_mk2.Forms
{
	partial class ShipyardReborn
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
			DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition1 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
			DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition2 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
			DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition3 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
			DevExpress.XtraEditors.TableLayout.TableRowDefinition tableRowDefinition1 = new DevExpress.XtraEditors.TableLayout.TableRowDefinition();
			DevExpress.XtraEditors.TableLayout.TableRowDefinition tableRowDefinition2 = new DevExpress.XtraEditors.TableLayout.TableRowDefinition();
			DevExpress.XtraEditors.TableLayout.TableRowDefinition tableRowDefinition3 = new DevExpress.XtraEditors.TableLayout.TableRowDefinition();
			DevExpress.XtraEditors.TableLayout.TableRowDefinition tableRowDefinition4 = new DevExpress.XtraEditors.TableLayout.TableRowDefinition();
			DevExpress.XtraEditors.TableLayout.TableSpan tableSpan1 = new DevExpress.XtraEditors.TableLayout.TableSpan();
			DevExpress.XtraGrid.Views.Tile.TileViewItemElement tileViewItemElement1 = new DevExpress.XtraGrid.Views.Tile.TileViewItemElement();
			DevExpress.XtraGrid.Views.Tile.TileViewItemElement tileViewItemElement2 = new DevExpress.XtraGrid.Views.Tile.TileViewItemElement();
			DevExpress.XtraGrid.Views.Tile.TileViewItemElement tileViewItemElement3 = new DevExpress.XtraGrid.Views.Tile.TileViewItemElement();
			DevExpress.XtraGrid.Views.Tile.TileViewItemElement tileViewItemElement4 = new DevExpress.XtraGrid.Views.Tile.TileViewItemElement();
			DevExpress.XtraGrid.Views.Tile.TileViewItemElement tileViewItemElement5 = new DevExpress.XtraGrid.Views.Tile.TileViewItemElement();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShipyardReborn));
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip3 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipItem toolTipItem3 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip4 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipItem toolTipItem4 = new DevExpress.Utils.ToolTipItem();
			this.gridColumn1 = new DevExpress.XtraGrid.Columns.TileViewColumn();
			this.gridColumn2 = new DevExpress.XtraGrid.Columns.TileViewColumn();
			this.gridColumn4 = new DevExpress.XtraGrid.Columns.TileViewColumn();
			this.repCboAvailableThemes = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
			this.gridColumn3 = new DevExpress.XtraGrid.Columns.TileViewColumn();
			this.gridColumn5 = new DevExpress.XtraGrid.Columns.TileViewColumn();
			this.gridControl1 = new DevExpress.XtraGrid.GridControl();
			this.tileView1 = new DevExpress.XtraGrid.Views.Tile.TileView();
			this.chkEnableThemeChange_Rep = new DevExpress.XtraEditors.ToggleSwitch();
			this.txtPlayerName_Rep = new DevExpress.XtraEditors.TextEdit();
			this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
			this.cmdSaveChanges = new DevExpress.XtraEditors.SimpleButton();
			this.cmdInfo = new DevExpress.XtraEditors.SimpleButton();
			this.cmdReset = new DevExpress.XtraEditors.SimpleButton();
			this.cmdRemoveShip = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.repCboAvailableThemes)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tileView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chkEnableThemeChange_Rep.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtPlayerName_Rep.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
			this.panelControl2.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridColumn1
			// 
			this.gridColumn1.Caption = "Type";
			this.gridColumn1.FieldName = "Ship.ship_full_name";
			this.gridColumn1.Name = "gridColumn1";
			this.gridColumn1.OptionsColumn.AllowEdit = false;
			this.gridColumn1.OptionsColumn.ReadOnly = true;
			this.gridColumn1.Visible = true;
			this.gridColumn1.VisibleIndex = 0;
			// 
			// gridColumn2
			// 
			this.gridColumn2.Caption = "Name";
			this.gridColumn2.FieldName = "ship_name";
			this.gridColumn2.Name = "gridColumn2";
			this.gridColumn2.OptionsColumn.AllowEdit = false;
			this.gridColumn2.OptionsColumn.ReadOnly = true;
			this.gridColumn2.Visible = true;
			this.gridColumn2.VisibleIndex = 1;
			// 
			// gridColumn4
			// 
			this.gridColumn4.AppearanceCell.BackColor = System.Drawing.Color.DarkOrange;
			this.gridColumn4.AppearanceCell.Options.UseBackColor = true;
			this.gridColumn4.Caption = "Theme";
			this.gridColumn4.ColumnEdit = this.repCboAvailableThemes;
			this.gridColumn4.FieldName = "theme";
			this.gridColumn4.Name = "gridColumn4";
			this.gridColumn4.Visible = true;
			this.gridColumn4.VisibleIndex = 3;
			// 
			// repCboAvailableThemes
			// 
			this.repCboAvailableThemes.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
			this.repCboAvailableThemes.AutoHeight = false;
			this.repCboAvailableThemes.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.repCboAvailableThemes.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("name", "Name", 200, DevExpress.Utils.FormatType.None, "", true, DevExpress.Utils.HorzAlignment.Default, DevExpress.Data.ColumnSortOrder.None, DevExpress.Utils.DefaultBoolean.Default),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Preview", "Preview", 200, DevExpress.Utils.FormatType.None, "", true, DevExpress.Utils.HorzAlignment.Default, DevExpress.Data.ColumnSortOrder.None, DevExpress.Utils.DefaultBoolean.Default)});
			this.repCboAvailableThemes.Name = "repCboAvailableThemes";
			this.repCboAvailableThemes.NullText = "[No Theme]";
			this.repCboAvailableThemes.PopupWidth = 400;
			this.repCboAvailableThemes.PopupWidthMode = DevExpress.XtraEditors.PopupWidthMode.ContentWidth;
			this.repCboAvailableThemes.ShowPopupButtons = DevExpress.Utils.DefaultBoolean.True;
			// 
			// gridColumn3
			// 
			this.gridColumn3.Caption = "ID";
			this.gridColumn3.FieldName = "ship_plate";
			this.gridColumn3.Name = "gridColumn3";
			this.gridColumn3.OptionsColumn.AllowEdit = false;
			this.gridColumn3.OptionsColumn.ReadOnly = true;
			this.gridColumn3.Visible = true;
			this.gridColumn3.VisibleIndex = 2;
			// 
			// gridColumn5
			// 
			this.gridColumn5.Caption = "Imagen";
			this.gridColumn5.FieldName = "Preview";
			this.gridColumn5.Name = "gridColumn5";
			this.gridColumn5.OptionsColumn.AllowEdit = false;
			this.gridColumn5.OptionsEditForm.RowSpan = 2;
			this.gridColumn5.Visible = true;
			this.gridColumn5.VisibleIndex = 4;
			// 
			// gridControl1
			// 
			this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControl1.Location = new System.Drawing.Point(0, 47);
			this.gridControl1.MainView = this.tileView1;
			this.gridControl1.Name = "gridControl1";
			this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repCboAvailableThemes});
			this.gridControl1.Size = new System.Drawing.Size(987, 557);
			this.gridControl1.TabIndex = 0;
			this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.tileView1});
			// 
			// tileView1
			// 
			this.tileView1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.tileView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.gridColumn2,
            this.gridColumn3,
            this.gridColumn4,
            this.gridColumn5});
			this.tileView1.GridControl = this.gridControl1;
			this.tileView1.Name = "tileView1";
			this.tileView1.OptionsBehavior.EditingMode = DevExpress.XtraGrid.Views.Tile.TileViewEditingMode.EditForm;
			this.tileView1.OptionsEditForm.EditFormColumnCount = 1;
			this.tileView1.OptionsEditForm.FormCaptionFormat = "Choose a Theme to Apply on ship embark:";
			this.tileView1.OptionsEditForm.PopupEditFormWidth = 400;
			this.tileView1.OptionsEditForm.ShowOnDoubleClick = DevExpress.Utils.DefaultBoolean.True;
			this.tileView1.OptionsEditForm.ShowOnEnterKey = DevExpress.Utils.DefaultBoolean.True;
			this.tileView1.OptionsFind.AlwaysVisible = true;
			this.tileView1.OptionsTiles.RowCount = 0;
			this.tileView1.OptionsView.ShowViewCaption = true;
			tableColumnDefinition1.Length.Value = 120D;
			tableColumnDefinition2.Length.Value = 49D;
			tableColumnDefinition3.Length.Value = 191D;
			this.tileView1.TileColumns.Add(tableColumnDefinition1);
			this.tileView1.TileColumns.Add(tableColumnDefinition2);
			this.tileView1.TileColumns.Add(tableColumnDefinition3);
			tableRowDefinition1.Length.Value = 20D;
			tableRowDefinition2.Length.Value = 17D;
			tableRowDefinition3.Length.Value = 17D;
			tableRowDefinition4.Length.Value = 50D;
			this.tileView1.TileRows.Add(tableRowDefinition1);
			this.tileView1.TileRows.Add(tableRowDefinition2);
			this.tileView1.TileRows.Add(tableRowDefinition3);
			this.tileView1.TileRows.Add(tableRowDefinition4);
			tableSpan1.ColumnSpan = 2;
			tableSpan1.RowSpan = 4;
			this.tileView1.TileSpans.Add(tableSpan1);
			tileViewItemElement1.Appearance.Normal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			tileViewItemElement1.Appearance.Normal.Options.UseFont = true;
			tileViewItemElement1.Column = this.gridColumn1;
			tileViewItemElement1.ColumnIndex = 2;
			tileViewItemElement1.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement1.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.Squeeze;
			tileViewItemElement1.Text = "gridColumn1";
			tileViewItemElement1.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement2.Column = this.gridColumn2;
			tileViewItemElement2.ColumnIndex = 2;
			tileViewItemElement2.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement2.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.Squeeze;
			tileViewItemElement2.RowIndex = 1;
			tileViewItemElement2.Text = "gridColumn2";
			tileViewItemElement2.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement3.Column = this.gridColumn4;
			tileViewItemElement3.ColumnIndex = 2;
			tileViewItemElement3.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement3.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.Squeeze;
			tileViewItemElement3.RowIndex = 3;
			tileViewItemElement3.Text = "gridColumn4";
			tileViewItemElement3.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement4.Column = this.gridColumn3;
			tileViewItemElement4.ColumnIndex = 2;
			tileViewItemElement4.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement4.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.Squeeze;
			tileViewItemElement4.RowIndex = 2;
			tileViewItemElement4.Text = "gridColumn3";
			tileViewItemElement4.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement5.Column = this.gridColumn5;
			tileViewItemElement5.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			tileViewItemElement5.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.Squeeze;
			tileViewItemElement5.Text = "gridColumn5";
			tileViewItemElement5.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
			this.tileView1.TileTemplate.Add(tileViewItemElement1);
			this.tileView1.TileTemplate.Add(tileViewItemElement2);
			this.tileView1.TileTemplate.Add(tileViewItemElement3);
			this.tileView1.TileTemplate.Add(tileViewItemElement4);
			this.tileView1.TileTemplate.Add(tileViewItemElement5);
			this.tileView1.ViewCaption = "Your Ships";
			this.tileView1.EditFormHidden += new DevExpress.XtraGrid.Views.Grid.EditFormHiddenEventHandler(this.tileView1_EditFormHidden);
			// 
			// chkEnableThemeChange_Rep
			// 
			this.chkEnableThemeChange_Rep.Location = new System.Drawing.Point(114, 6);
			this.chkEnableThemeChange_Rep.Name = "chkEnableThemeChange_Rep";
			this.chkEnableThemeChange_Rep.Properties.ContentAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.chkEnableThemeChange_Rep.Properties.OffText = "Off";
			this.chkEnableThemeChange_Rep.Properties.OnText = "On";
			this.chkEnableThemeChange_Rep.Size = new System.Drawing.Size(112, 36);
			this.chkEnableThemeChange_Rep.TabIndex = 5;
			this.chkEnableThemeChange_Rep.Toggled += new System.EventHandler(this.chkEnableThemeChange_Rep_Toggled);
			// 
			// txtPlayerName_Rep
			// 
			this.txtPlayerName_Rep.Location = new System.Drawing.Point(313, 6);
			this.txtPlayerName_Rep.Name = "txtPlayerName_Rep";
			this.txtPlayerName_Rep.Properties.ReadOnly = true;
			this.txtPlayerName_Rep.Size = new System.Drawing.Size(221, 36);
			this.txtPlayerName_Rep.TabIndex = 4;
			// 
			// panelControl2
			// 
			this.panelControl2.Controls.Add(this.cmdSaveChanges);
			this.panelControl2.Controls.Add(this.cmdInfo);
			this.panelControl2.Controls.Add(this.cmdReset);
			this.panelControl2.Controls.Add(this.cmdRemoveShip);
			this.panelControl2.Controls.Add(this.labelControl2);
			this.panelControl2.Controls.Add(this.labelControl1);
			this.panelControl2.Controls.Add(this.chkEnableThemeChange_Rep);
			this.panelControl2.Controls.Add(this.txtPlayerName_Rep);
			this.panelControl2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelControl2.Location = new System.Drawing.Point(0, 0);
			this.panelControl2.Name = "panelControl2";
			this.panelControl2.Size = new System.Drawing.Size(987, 47);
			this.panelControl2.TabIndex = 2;
			// 
			// cmdSaveChanges
			// 
			this.cmdSaveChanges.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("cmdSaveChanges.ImageOptions.SvgImage")));
			this.cmdSaveChanges.Location = new System.Drawing.Point(904, 7);
			this.cmdSaveChanges.Name = "cmdSaveChanges";
			this.cmdSaveChanges.Size = new System.Drawing.Size(72, 35);
			toolTipItem1.Text = "Save Changes";
			superToolTip1.Items.Add(toolTipItem1);
			this.cmdSaveChanges.SuperTip = superToolTip1;
			this.cmdSaveChanges.TabIndex = 11;
			this.cmdSaveChanges.Text = "Save Changes";
			this.cmdSaveChanges.Click += new System.EventHandler(this.cmdSaveChanges_Click);
			// 
			// cmdInfo
			// 
			this.cmdInfo.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("cmdInfo.ImageOptions.SvgImage")));
			this.cmdInfo.Location = new System.Drawing.Point(826, 7);
			this.cmdInfo.Name = "cmdInfo";
			this.cmdInfo.Size = new System.Drawing.Size(72, 35);
			toolTipItem2.Text = "What is the Shipyard?";
			superToolTip2.Items.Add(toolTipItem2);
			this.cmdInfo.SuperTip = superToolTip2;
			this.cmdInfo.TabIndex = 10;
			this.cmdInfo.Text = "Info";
			this.cmdInfo.Click += new System.EventHandler(this.cmdInfo_Click);
			// 
			// cmdReset
			// 
			this.cmdReset.Enabled = false;
			this.cmdReset.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("cmdReset.ImageOptions.SvgImage")));
			this.cmdReset.Location = new System.Drawing.Point(708, 7);
			this.cmdReset.Name = "cmdReset";
			this.cmdReset.Size = new System.Drawing.Size(113, 35);
			toolTipItem3.Text = "Delete all ships and reload them from your latest Journal.";
			superToolTip3.Items.Add(toolTipItem3);
			this.cmdReset.SuperTip = superToolTip3;
			this.cmdReset.TabIndex = 9;
			this.cmdReset.Text = "Reset";
			this.cmdReset.Click += new System.EventHandler(this.cmdReset_Click);
			// 
			// cmdRemoveShip
			// 
			this.cmdRemoveShip.Enabled = false;
			this.cmdRemoveShip.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("cmdRemoveShip.ImageOptions.SvgImage")));
			this.cmdRemoveShip.Location = new System.Drawing.Point(549, 7);
			this.cmdRemoveShip.Name = "cmdRemoveShip";
			this.cmdRemoveShip.Size = new System.Drawing.Size(153, 35);
			toolTipItem4.Text = "Delete the Selected Ship";
			superToolTip4.Items.Add(toolTipItem4);
			this.cmdRemoveShip.SuperTip = superToolTip4;
			this.cmdRemoveShip.TabIndex = 8;
			this.cmdRemoveShip.Text = "Remove Ship";
			this.cmdRemoveShip.Click += new System.EventHandler(this.cmdRemoveShip_Click);
			// 
			// labelControl2
			// 
			this.labelControl2.Location = new System.Drawing.Point(242, 18);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(65, 13);
			this.labelControl2.TabIndex = 7;
			this.labelControl2.Text = "Player Name:";
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(13, 18);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(93, 13);
			this.labelControl1.TabIndex = 6;
			this.labelControl1.Text = "Shipyard Enabled:";
			// 
			// ShipyardReborn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(987, 604);
			this.Controls.Add(this.gridControl1);
			this.Controls.Add(this.panelControl2);
			this.Name = "ShipyardReborn";
			this.Text = "Shipyard Reborn";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShipyardReborn_FormClosing);
			this.Load += new System.EventHandler(this.ShipyardReborn_Load);
			this.Shown += new System.EventHandler(this.ShipyardReborn_Shown);
			((System.ComponentModel.ISupportInitialize)(this.repCboAvailableThemes)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tileView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chkEnableThemeChange_Rep.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtPlayerName_Rep.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
			this.panelControl2.ResumeLayout(false);
			this.panelControl2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.GridControl gridControl1;
		private DevExpress.XtraEditors.TextEdit txtPlayerName_Rep;
		private DevExpress.XtraEditors.ToggleSwitch chkEnableThemeChange_Rep;
		private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit repCboAvailableThemes;
		private DevExpress.XtraGrid.Views.Tile.TileView tileView1;
		private DevExpress.XtraGrid.Columns.TileViewColumn gridColumn1;
		private DevExpress.XtraGrid.Columns.TileViewColumn gridColumn2;
		private DevExpress.XtraGrid.Columns.TileViewColumn gridColumn3;
		private DevExpress.XtraGrid.Columns.TileViewColumn gridColumn4;
		private DevExpress.XtraGrid.Columns.TileViewColumn gridColumn5;
		private DevExpress.XtraEditors.PanelControl panelControl2;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.SimpleButton cmdReset;
		private DevExpress.XtraEditors.SimpleButton cmdRemoveShip;
		private DevExpress.XtraEditors.SimpleButton cmdInfo;
		private DevExpress.XtraEditors.SimpleButton cmdSaveChanges;
	}
}