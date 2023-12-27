using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EDHM_UI_mk2.Forms
{
	public partial class ShipyardReborn : DevExpress.XtraEditors.XtraForm
	{
		#region Properties & Declarations

		public List<ui_preset_new> UI_Themes { get; set; }
		public game_instance ActiveInstance { get; set; }
		public ShipyardEx Shipyard { get; set; }
		public List<ship> ShipList { get; set; }

		public List<ui_translation> Translations { get; set; }
		public bool IsDirty { get; set; } = false;		
		public string LangShort = "en"; //<- Idioma x Defecto

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private bool IsLoading = false;

		#endregion

		#region Constructors

		public ShipyardReborn()
		{
			InitializeComponent();
		}

		private void ShipyardReborn_Load(object sender, EventArgs e)
		{

		}
		private void ShipyardReborn_Shown(object sender, EventArgs e)
		{
			IsLoading = true;
			LoadShipyardData();
			IsLoading = false;
		}
		private void ShipyardReborn_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (IsDirty)
			{
				if (Mensajero.ShowMessage("Save Changes?",
					"There are un-saved changes!\r\nDo you want to save before leaving?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Stop) == DialogResult.Yes)
				{
					SaveChanges();
					this.DialogResult = DialogResult.OK;
				}
			}
		}

		#endregion

		#region Methods

		private void LoadShipyardData()
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				// Load and Prepare the List of Available Themes:
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

					this.repCboAvailableThemes.ValueMember = "name";
					this.repCboAvailableThemes.DisplayMember = "name";
					this.repCboAvailableThemes.DataSource = this.UI_Themes;
				}

				if (this.Shipyard != null) EnableControls(true);
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
			finally { this.Cursor = Cursors.Default; }
		}
		public void LoadTestData()
		{
			try
			{
				this.Shipyard = new ShipyardEx()
				{
					enabled = true,
					player_name = "Blue Mystic",
					active_instance = ActiveInstance.key
				};
				this.Shipyard.ships = new List<ship_loadout_ex>();

				foreach (var ship in ShipList)
				{
					this.Shipyard.ships.Add(new ship_loadout_ex(ShipList, ship.ed_short)
					{
						ship_name = "Custom Ship Name",
						ship_plate = "Plate ID: 0000-0",
						theme = null
					});
				}

				this.gridControl1.DataSource = this.Shipyard.ships;
				this.txtPlayerName_Rep.Text = this.Shipyard.player_name;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public bool ScanPlayerJournal()
		{
			bool _ret = false;
			try
			{
				string EDJournalDir = Util.WinReg_ReadKey("EDHM", "PlayerJournal").NVL("");
				if (Directory.Exists(EDJournalDir))
				{
					this.Cursor = Cursors.WaitCursor;
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						DirectoryInfo di = new DirectoryInfo(EDJournalDir);

						//Busca el Archivo de Log mas reciente:
						FileInfo JournalFile = di.GetFiles("Journal.*.log")
									.OrderByDescending(f => f.LastWriteTime).First();

						if (JournalFile != null)
						{
							//El archivo se abre en modo Compartido:
							var fs = new FileStream(JournalFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
							using (var sr = new StreamReader(fs))
							{
								string Line = string.Empty;
								while ((Line = sr.ReadLine()) != null) //<- Lee Linea x Linea
								{
									// Process line
									PlayerJournal_DetectEvents(Line); //<- Analiza la Linea Buscando los Eventos deseados 	
								}
							}
							_ret = (Shipyard.ships != null && Shipyard.ships.Count > 0);

							Invoke(new Action(() =>
							{
								EnableControls(_ret);
							}));
						};
					});
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		private void PlayerJournal_DetectEvents(string JsonLine)
		{
			/* AQUI SE LEEN LAS LINEAS NUEVAS DEL LOG Y SE DETECTAN LOS EVENTOS DESEADOS   */
			// Esto sigue ejecutandose dentro del proceso iniciado x 'ReadPlayerJournal()'
			try
			{
				/* Cada linea del Log es un Objeto JSON completo
				 * https://elite-journal.readthedocs.io/en/latest/
				 * */

				//1. Buscar el Nombre del Jugador:
				int index = 0;
				index = JsonLine.IndexOf("\"event\":\"Commander\"", index);
				if (index != -1)
				{
					//Evento Detectado!
					Debug.WriteLine("Commander event.");
					dynamic data = Newtonsoft.Json.Linq.JObject.Parse(JsonLine);
					if (data != null)
					{
						Shipyard.player_name = Convert.ToString(data.Name);						
					}
				}

				//2. Detectar cuando se Cambia la Nave:
				index = 0;
				index = JsonLine.IndexOf("\"event\":\"Loadout\"", index);
				if (index != -1)
				{
					//Evento Detectado!
					Debug.WriteLine("Loadout event.");
					dynamic data = Newtonsoft.Json.Linq.JObject.Parse(JsonLine);
					{
						/*	when loading from main menu, or when switching ships, or after changing the ship in Outfitting, or when docking SRV back in mothership
						 *	
						 *	"timestamp": "2021-08-06T01:56:55Z", "event": "Loadout", 
						 *	"Ship": "cutter", "ShipID": 7, "ShipName": "NORMANDY", "ShipIdent": "SR-04", "HullValue": 180435872,
							"ModulesValue": 128249820, "HullHealth": 1.0, "UnladenMass": 1803.399902, "CargoCapacity": 320, "MaxJumpRange": 31.563942, .... */
						if (data != null)
						{
							if (ShipList.IsNotEmpty() && !string.IsNullOrEmpty(Convert.ToString(data.Ship)))
							{
								string eventShipID = Convert.ToString(data.Ship).ToLowerInvariant();
								PlayerJournal_ShipChanged(new ship_loadout_ex(ShipList, eventShipID)
								{
									ship_name = data.ShipName,
									ship_plate = data.ShipIdent,
									theme = null
								});
							}
						}
					}
				}

				//3. Detectar cuando se lanza el SRV:
				index = 0;
				index = JsonLine.IndexOf("\"event\":\"LaunchSRV\"", index);
				if (index != -1 )
				{
					/* When deploying the SRV from a ship onto planet surface
					 * 
					 *{ "timestamp":"2023-07-30T18:28:01Z", "event":"LaunchSRV", 
					 * "SRVType":"combat_multicrew_srv_01", 
					 * "SRVType_Localised":"SRV Scorpion", 
					 * "Loadout":"default", "ID":10, "PlayerControlled":true } */

					Debug.WriteLine("LaunchSRV event.");
					dynamic data = Newtonsoft.Json.Linq.JObject.Parse(JsonLine);
					{
						string eventShipID = Convert.ToString(data.SRVType).ToLowerInvariant();
						PlayerJournal_ShipChanged(new ship_loadout_ex(ShipList, eventShipID)
						{
							ship_name = data.SRVType_Localised,
							ship_plate = string.Empty,
							theme = null
						});
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void PlayerJournal_ShipChanged(ship_loadout_ex CurrentShip)
		{
			try
			{
				if (Shipyard != null)
				{
					ship_loadout_ex MyShip = null;

					if (Shipyard.ships == null) Shipyard.ships = new List<ship_loadout_ex>();

					//Revisar si la Nave ya existe en el Shipyard:
					bool Existe = false;
					if (Shipyard.ships.IsNotEmpty())
					{
						MyShip = Shipyard.ships.Find(x => x.Ship.ed_short == CurrentShip.Ship.ed_short.Trim() &&
															x.ship_name == CurrentShip.ship_name.Trim() &&
															x.ship_plate == CurrentShip.ship_plate.Trim());
						if (MyShip != null)
						{
							Existe = true;
						}
					}

					//Si la Nave No existe la Agregamos al Historial:
					if (!Existe)
					{
						Shipyard.ships.Add(CurrentShip);
						this.IsDirty = true;
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		public void EnableControls(bool Enable)
		{
			try
			{
				cmdRemoveShip.Enabled = Enable;
				cmdReset.Enabled = Enable;

				this.chkEnableThemeChange_Rep.EditValue = this.Shipyard.enabled;
				this.Shipyard.active_instance = ActiveInstance.key;

				this.gridControl1.DataSource = !Enable ? null : this.Shipyard.ships;
				this.txtPlayerName_Rep.EditValue = !Enable ? null : this.Shipyard.player_name;

				this.Cursor = Cursors.Default;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void SaveChanges()
		{
			try
			{
				Util.WinReg_WriteKey("EDHM", "WatchMe", this.Shipyard.enabled);
				Util.Serialize_ToJSON(Path.Combine(this.AppExePath, @"Data\Shipyard.json"), this.Shipyard);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion

		#region Control Events

		private void itemsView1_ElementMouseClick(object sender, DevExpress.XtraGrid.Views.Items.ItemsViewHtmlElementMouseEventArgs e)
		{
			//click on a ship's item
		}
		private void tileView1_EditFormHidden(object sender, DevExpress.XtraGrid.Views.Grid.EditFormHiddenEventArgs e)
		{
			IsDirty = true;
		}

		private void chkEnableThemeChange_Rep_Toggled(object sender, EventArgs e)
		{
			if (!IsLoading)
			{
				this.Shipyard.enabled = Convert.ToBoolean(this.chkEnableThemeChange_Rep.EditValue);
				if (this.Shipyard.enabled)
				{
					Util.WinReg_WriteKey("EDHM", "WatchMe", this.Shipyard.enabled);
				}
				var ConsentText = Translations.Find(TR => TR.id == "Consent").lang.Find(lang => lang.key == LangShort);

				if (this.Shipyard.enabled &&
					Mensajero.ShowMessage("Do you Agree?",
						string.Format("{0}\r\n\r\n{1}\r\n\r\n{2}", ConsentText.value, ConsentText.category, ConsentText.description),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					ScanPlayerJournal();
				}
				else
				{
					this.Shipyard.enabled = false;
					this.chkEnableThemeChange_Rep.EditValue = false;
					this.gridControl1.DataSource = null;
				}
				IsDirty = true;
			}			
		}

		private void cmdRemoveShip_Click(object sender, EventArgs e)
		{
			ship_loadout_ex mSeleccionada = (ship_loadout_ex)this.tileView1.GetFocusedRow();
			if (mSeleccionada != null)
			{
				var _Text = Translations.Find(TR => TR.id == "ShipRemoval").lang.Find(lang => lang.key == LangShort);

				if (Mensajero.ShowMessage(_Text.category, 
					string.Format(_Text.value, 
					mSeleccionada.Ship.ship_full_name, mSeleccionada.ship_name), 
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					this.Shipyard.ships.Remove(mSeleccionada);
					this.gridControl1.RefreshDataSource();
					IsDirty = true;
				}
			}
		}

		private void cmdReset_Click(object sender, EventArgs e)
		{
			var _Text = Translations.Find(TR => TR.id == "Reset").lang.Find(lang => lang.key == LangShort);

			if (Mensajero.ShowMessage("Reset Shipyard?",
				string.Format("{0}{1}{2}", _Text.category, _Text.value, _Text.description),
					MessageBoxButtons.YesNo, MessageBoxIcon.Stop) == DialogResult.Yes)
			{
				ScanPlayerJournal();
				IsDirty = true;
				string DoneTitle = _Text.description;
				_Text = Translations.Find(TR => TR.id == "ResetDone").lang.Find(lang => lang.key == LangShort);

				Mensajero.ShowMessage("Done!",
					string.Format("{0}{1}{2}", _Text.category, _Text.value, _Text.description),
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void cmdInfo_Click(object sender, EventArgs e)
		{
			var _Text = Translations.Find(TR => TR.id == "Info").lang.Find(lang => lang.key == LangShort);

			Mensajero.ShowMessage("What is this?",
					string.Format("{0}{1}{2}", _Text.category, _Text.value, _Text.description),
					MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		private void cmdSaveChanges_Click(object sender, EventArgs e)
		{
			SaveChanges();
			IsLoading = true;
			LoadShipyardData();
			IsLoading = false;
		}

		#endregion


	}
}