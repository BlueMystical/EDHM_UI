using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;

namespace EDHM_UI_mk2.Forms
{
	public partial class ShipyardForm : DevExpress.XtraEditors.XtraForm
	{
		public List<ui_preset_new> UI_Themes { get; set; }
		public game_instance ActiveInstance { get; set; }
		public player_loadout Shipyard { get; set; }

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;

		public ShipyardForm()
		{
			InitializeComponent();
		}
		private void ShipyardForm_Load(object sender, EventArgs e)
		{

		}
		private void ShipyardForm_Shown(object sender, EventArgs e)
		{
			LoadShipyardData();
		}

		private void LoadShipyardData()
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if (this.UI_Themes.IsNotEmpty())
				{
					var _CurrentSettings = this.UI_Themes.Find(x => x.name == "Current Settings");
					this.UI_Themes.Remove(_CurrentSettings);

					ui_preset_new _RandomTheme = new ui_preset_new("@Random Theme", Path.Combine(this.ActiveInstance.path, @"EDHM-ini"), "User")
					{
						Preview = Util.GetElementImage(Path.Combine(this.AppExePath, "Images", "PREVIEW.PNG"))
					};
					this.UI_Themes.Add(_RandomTheme);
					this.UI_Themes = UI_Themes.OrderBy(x => x.name).ToList();

					this.cboThemes_Rep.ValueMember = "name";
					this.cboThemes_Rep.DisplayMember = "name";
					this.cboThemes_Rep.DataSource = this.UI_Themes;
				}

				if (this.Shipyard != null)
				{
					this.gridControl1.DataSource = this.Shipyard.ships;

					this.txtPlayerName_Rep.Text = this.Shipyard.player_name;
					this.txtActiveInstance_Rep.Text = this.Shipyard.active_instance;
					this.chkEnableThemeChange_Rep.EditValue = this.Shipyard.theme_swaping;

					ship_loadout mSeleccionada = (ship_loadout)this.gridView1.GetFocusedRow();
					if (mSeleccionada != null)
					{
						var _Theme = this.UI_Themes.Find(x => x.name == mSeleccionada.theme);
						if (_Theme != null)
						{
							this.pictureEdit2.Image = _Theme.Preview;
						}
						else
						{
							this.pictureEdit2.Image = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
		{
			GridView view = sender as GridView;
			ship_loadout mSeleccionada = (ship_loadout)view.GetFocusedRow();
			if (mSeleccionada != null)
			{
				var _Theme = this.UI_Themes.Find(x => x.name == mSeleccionada.theme);
				if (_Theme != null)
				{
					this.pictureEdit2.Image = _Theme.Preview;
				}
				else
				{
					this.pictureEdit2.Image = null;
				}
			}
		}

		private void cboThemes_Rep_EditValueChanged(object sender, EventArgs e)
		{
			LookUpEdit Control = sender as LookUpEdit;
			ui_preset_new _Theme = (ui_preset_new)Control.GetSelectedDataRow();
			if (_Theme != null)
			{
				this.pictureEdit2.Image = _Theme.Preview;
			}
			else
			{
				this.pictureEdit2.Image = null;
			}
		}
		private void chkEnableThemeChange_Rep_Toggled(object sender, EventArgs e)
		{
			this.Shipyard.theme_swaping = Convert.ToBoolean(this.chkEnableThemeChange_Rep.EditValue);
			if (this.Shipyard.theme_swaping)
			{
				Util.WinReg_WriteKey("EDHM", "WatchMe", this.Shipyard.theme_swaping);
			}
		}

		private void cmdSaveChanges_Click(object sender, EventArgs e)
		{
			if (this.Shipyard != null)
			{
				Util.Serialize_ToJSON(Path.Combine(this.AppExePath, @"Data\PlayerLoadout.json"), 
					this.Shipyard);

				this.DialogResult = DialogResult.OK;
			}
		}
		private void cmdRefreshList_Click(object sender, EventArgs e)
		{
			try
			{
				string JsonShipyardPath = Path.Combine(this.AppExePath, @"Data\PlayerLoadout.json");
				if (File.Exists(JsonShipyardPath))
				{
					this.Shipyard = Util.DeSerialize_FromJSON<player_loadout>(JsonShipyardPath);
					LoadShipyardData();
				}
				else
				{
					this.Shipyard = new player_loadout();
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdRemoveShip_Click(object sender, EventArgs e)
		{
			ship_loadout _Row = (ship_loadout)this.gridView1.GetFocusedRow();
			if (_Row != null)
			{
				if (XtraMessageBox.Show("Are you sure to Delete the Selected Ship from the Shipyard?\r\nIt may come back again next time you use it in game.", 
					"Confirm Ship Removal:", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes)
				{
					this.Shipyard.ships.Remove(_Row);
					this.gridView1.RefreshData();
				}
			}
		}
		private void cmdHowTo_Click(object sender, EventArgs e)
		{
			StringBuilder _Message = new StringBuilder();
			_Message.AppendLine("- The ships in the list gets updated once you board any ship in the game.");			
			_Message.AppendLine("- Once your Ship is in the List, you can asign it a Theme.");
			_Message.AppendLine("- Whenever you swap ships in game, the UI will check if it has a theme asigned, if so, the UI will Apply it's theme and refresh changes in game.");
			_Message.AppendLine("- The 'Swap themes on ship change' Switch must be ON, it is OFF by default.");
			_Message.AppendLine();
			_Message.AppendLine("- The UI Program (but not this window) needs to be runing for the Detection of your Ships.");

			XtraMessageBox.Show(_Message.ToString(),
					"How To:", MessageBoxButtons.OK, MessageBoxIcon.Information); 
		}
	}
}