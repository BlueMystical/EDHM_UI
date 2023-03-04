using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;

namespace EDHM_UI_mk2
{
	public partial class GameFolderForm : DevExpress.XtraEditors.XtraForm
	{
		#region Private Declarations

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private string RegActiveInstance = string.Empty;
		private bool IsFirstRun = false;

		[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
		[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
		private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
		[DllImport("User32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

		#endregion

		#region Public Properties

		public List<GameInstance> GameInstancesEx { get; set; }
		public game_instance ActiveInstance { get; set; }
		public List<key> Languages { get; set; }

		public bool WatchMe { get; set; }		//<- Determina si Registra las Naves del Jugador
		public bool GreetMe { get; set; }		//<- Determina si Saluda al Jugador al Inicio
		public bool HideToTray { get; set; }	//<- Determina si se Oculta en el Tray al Cerrar la ventana
		public int SavesToRemember { get; set; } //<- Cantidad de Guardados a recordar
		public bool AutoApplyTheme { get; set; } //<- If set, and game is running, it will send the F11 key to the game

		#endregion

		public GameFolderForm()
		{

		}
		public GameFolderForm(List<GameInstance> _GameInstances)
		{
			InitializeComponent();
			this.GameInstancesEx = _GameInstances;
		}

		private void GameFolderForm_Load(object sender, EventArgs e)
		{
			this.RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");

			this.WatchMe = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "WatchMe").NVL("true"));
			this.GreetMe = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "GreetMe").NVL("true"));
			this.HideToTray = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "HideToTray").NVL("false"));
			this.SavesToRemember = Convert.ToInt32(Util.WinReg_ReadKey("EDHM", "SavesToRemember").NVL("10"));
			this.AutoApplyTheme = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "AutoApplyTheme").NVL("false"));

			this.txtPlayerJournal.EditValue = Util.WinReg_ReadKey("EDHM", "PlayerJournal").NVL(string.Empty);
			this.txtSavesToRememberRep.Value = this.SavesToRemember;

			#region Carga los Idiomas Disponibles

			string CurrentLanguage = Util.WinReg_ReadKey("EDHM", "Language").NVL("en");
			string AvailableLanguages = Util.AppConfig_GetValue("Languages");
			if (!AvailableLanguages.EmptyOrNull())
			{
				this.Languages = new List<key>();
				string[] Palabras = AvailableLanguages.Split(new char[] { ',' });
				foreach (string lang in Palabras)
				{
					System.Globalization.CultureInfo LangCulture = new System.Globalization.CultureInfo(lang);
					if (LangCulture != null)
					{
						key _Lang = new key
						{
							Name = (lang == "jp" ? "日本語" : LangCulture.NativeName.ToUpperInvariant()),
							Value = lang
						};
						this.Languages.Add(_Lang);
					}
				}
				if (this.Languages.IsNotEmpty())
				{
					this.cboLanguages.Properties.DataSource = this.Languages;
					this.cboLanguages.Properties.DisplayMember = "Name";
					this.cboLanguages.Properties.ValueMember = "Value";
					this.cboLanguages.EditValue = CurrentLanguage;
				}
			}

			#endregion

			#region Cargar las Instancias del Juego

			if (this.GameInstancesEx is null)
			{
				string _RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");
				string GameInstances_JSON = Util.WinReg_ReadKey("EDHM", "GameInstances").NVL(string.Empty);

				if (!GameInstances_JSON.EmptyOrNull())
				{
					this.GameInstancesEx = Util.DeSerialize_FromJSON_String<List<GameInstance>>(GameInstances_JSON);
				}
				else
				{
					//Crea Las Instancias x Defecto:
					this.GameInstancesEx = new List<GameInstance>();
					this.GameInstancesEx.Add(new GameInstance() { instance = "Default", games = new List<game_instance>() });
					this.GameInstancesEx[0].games.Add(new game_instance()
					{
						key = "ED_Horizons",
						name = "Horizons",
						instance = "Horizons (Default)",
						themes_folder = @"EDHM-ini\DemoProfiles",
						path = Util.WinReg_ReadKey("EDHM", "ED_Horizons").NVL(string.Empty),
						is_active = Util.IIf(_RegActiveInstance == "ED_Horizons", true, false)
					});
					this.GameInstancesEx[0].games.Add(new game_instance()
					{
						key = "ED_Odissey",
						name = "Odyssey",
						instance = "Odyssey (Default)",
						themes_folder = @"EDHM-ini\MyProfiles",
						path = Util.WinReg_ReadKey("EDHM", "ED_Odissey").NVL(string.Empty),
						is_active = Util.IIf(_RegActiveInstance == "ED_Odissey", true, false)
					});

					GameInstances_JSON = Util.Serialize_ToJSON(this.GameInstancesEx);
					Util.WinReg_WriteKey("EDHM", "GameInstances", GameInstances_JSON);
				}
			}
			#endregion

			this.chkSettings_WatchMe.Checked = this.WatchMe;
			this.chkSettings_GreetMe.Checked = this.GreetMe;
			this.chkSettings_HideToTray.Checked = this.HideToTray;
			this.chkAutoApplyTheme.Checked = this.AutoApplyTheme;
		}
		private void GameFolderForm_Shown(object sender, EventArgs e)
		{
			if (this.GameInstancesEx.IsNotEmpty())
			{
				this.gridControl1.DataSource = this.GameInstancesEx;
				ExpandAllRows(this.gridView1);
				this.gridView2.BestFitColumns();
			}
			if (this.IsFirstRun)
			{
				XtraMessageBox.Show("Here you are Required to set the Game Paths, click the [...] buttons to set them Manually.\r\nOr you can use the 'Game Locator Assistant'.", "Welcome!",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		private void GameFolderForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			//if (this.IsFirstRun)
			//{
			//	if (this.GameInstancesEx.IsNotEmpty())
			//	{
			//		if (XtraMessageBox.Show("There are Un-Saved Changes!\r\nWould you like to Save them now?",
			//			"Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			//		{
			//			SaveChanges();
			//		}
			//	}
			//}
		}

		public List<string> FindGameFolder()
		{
			List<string> _ret = null;
			try
			{
				string FindElite = Util.InstalledProgram_GetPath("Elite Dangerous");
				if (!FindElite.EmptyOrNull())
				{
					string GameDir = FindElite.Split(new char[] { '|' })[1];
					if (Directory.Exists(GameDir))
					{
						_ret = new List<string>();

						//Buscar Ruta de Horizons: \Products\elite-dangerous-64
						if (Directory.Exists(Path.Combine(GameDir, @"Products\elite-dangerous-64")))
						{
							_ret.Add(Path.Combine(GameDir, @"Products\elite-dangerous-64"));
						}

						//Buscar Ruta de Odissey: \Products\elite-dangerous-odyssey-64
						if (Directory.Exists(Path.Combine(GameDir, @"Products\elite-dangerous-odyssey-64")))
						{
							_ret.Add(Path.Combine(GameDir, @"Products\elite-dangerous-odyssey-64"));
						}
					}
				}
				else
				{
					XtraMessageBox.Show("Program tried to Detect your Game Install paths but failed to do so.\r\nPlease set the Game Paths manually.\r\n'elite-dangerous-64' for Horizons and 'elite-dangerous-odissey-64' for Odyssey.",
						"ERROR", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private void SaveChanges()
		{
			try
			{
				if (this.GameInstancesEx.IsNotEmpty())
				{
					if (this.GameInstancesEx[0].games[0].path.EmptyOrNull() && this.GameInstancesEx[0].games[1].path.EmptyOrNull())
					{
						XtraMessageBox.Show("Folder Path had not been set!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}

					string UI_DOCUMENTS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Elite Dangerous\EDHM_UI");

					//Fixing the Instance Parameters:
					foreach (var _Instance in this.GameInstancesEx)
					{
						foreach (var _Game in _Instance.games)
						{
							if (_Game.path.Contains("steamapps"))	_Instance.instance = "Steam";
							if (_Game.path.Contains("Epic Games"))	_Instance.instance = "Epic Games";
							if (_Game.path.Contains("Frontier"))	_Instance.instance = "Frontier";

							switch (Path.GetFileNameWithoutExtension(_Game.path))
							{
								case "elite-dangerous-64":			_Game.name = "Horizons (Legacy)";	_Game.key = "ED_Horizons"; break;       //<- Horizons 3.8
								case "FORC-FDEV-DO-38-IN-40":		_Game.name = "Horizons (Live)";	_Game.key = "ED_Odissey"; break;        //<- Horizons 4.0
								case "elite-dangerous-odyssey-64":	_Game.name = "Odyssey (Live)";	_Game.key = "ED_Odissey"; break;        //<- Odyssey 4.0
								case "FORC-FDEV-DO-1000":			_Game.name = "Odyssey (Live)";	_Game.key = "ED_Odissey"; break;       //<- Odyssey 4.0 alt
								default: _Game.name = "Odyssey (Live)"; _Game.key = "ED_Odissey"; break;   
							}

							_Game.instance = string.Format("{0} ({1})", _Instance.instance, _Game.name);
							_Game.game_id =  string.Format("{0}|{1}", _Instance.instance, _Game.key);
							_Game.themes_folder = (_Game.key == "ED_Horizons") ?
								Path.Combine(UI_DOCUMENTS, "HORIZ", "Themes") :
								Path.Combine(UI_DOCUMENTS, "ODYSS", "Themes");
						}
					}

					Util.WinReg_WriteKey("EDHM", "GameInstances", Util.Serialize_ToJSON(this.GameInstancesEx));

					if (this.IsFirstRun)
					{
						//si Odyssey tiene path entonces esa es la instancia Activa:
						this.ActiveInstance = (this.GameInstancesEx[0].games[1].path.EmptyOrNull()) ? 
							this.GameInstancesEx[0].games[0] : 
							this.GameInstancesEx[0].games[1];

						Util.WinReg_WriteKey("EDHM", "ActiveInstance", this.ActiveInstance.key);
					}
					else
					{
						this.ActiveInstance = null;
					}

					this.IsFirstRun = false;
					this.HideToTray = this.chkSettings_HideToTray.Checked;
					this.GreetMe = this.chkSettings_GreetMe.Checked;
					this.WatchMe = this.chkSettings_WatchMe.Checked;
					this.AutoApplyTheme = this.chkAutoApplyTheme.Checked;
					this.SavesToRemember = Convert.ToInt32(this.txtSavesToRememberRep.Value);

					Util.WinReg_WriteKey("EDHM", "HideToTray", this.HideToTray);
					Util.WinReg_WriteKey("EDHM", "GreetMe", this.GreetMe);
					Util.WinReg_WriteKey("EDHM", "WatchMe", this.WatchMe);
					Util.WinReg_WriteKey("EDHM", "Language", this.cboLanguages.EditValue.ToString());
					Util.WinReg_WriteKey("EDHM", "PlayerJournal", this.txtPlayerJournal.EditValue.ToString());
					Util.WinReg_WriteKey("EDHM", "SavesToRemember", this.SavesToRemember);
					Util.WinReg_WriteKey("EDHM", "AutoApplyTheme", this.AutoApplyTheme);

					XtraMessageBox.Show("Settings Saved.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
					this.DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					XtraMessageBox.Show("At least One game instance must be created!",
						"ERROR 404 - NOT FOUND", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void GameLocatorAssistant()
		{
			/*  ASISTENTE PARA LOCALIZAR EL JUEGO */
			try
			{
				GameInstance Instancia = (GameInstance)this.gridView1.GetFocusedRow();
				if (Instancia != null)
				{
					if (XtraMessageBox.Show(string.Format("Do you Need Help Finding the Locations for Elite Dangerous Game?\r\nFor the '{0}' Instance.", Instancia.instance),
										"Need a Hand?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						if (XtraMessageBox.Show("First go and Start the Game Client.\r\nLeave this window open while you go,\r\n or open it again after game is running.\r\n\r\nClick Yes when the Game is Running.",
						"STEP 1:", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							#region Detecting the Game Process

							string GameTitle = Util.AppConfig_GetValue("GameProcessID");
							System.Diagnostics.Process GameProcess = null;

							System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
							foreach (System.Diagnostics.Process process in processlist)
							{
								if (!String.IsNullOrEmpty(process.ProcessName))
								{
									if (process.MainWindowTitle == GameTitle)
									{
										GameProcess = process;
										break;
									}
								}
							}

							#endregion

							if (GameProcess != null)
							{
								string GameProcess_Folder = System.IO.Path.GetDirectoryName(GameProcess.MainModule.FileName); //Obtiene el Path: (Sin archivo ni extension:							
								string ProductsFolder = System.IO.Directory.GetParent(GameProcess_Folder).FullName; //<- Obtiene la Carpeta Anterior 
								
								#region Close the Game

								try
								{
									GameProcess.CloseMainWindow();
									GameProcess.Kill();
								}
								catch { }
								finally { GameProcess.Dispose(); }

								#endregion

								Instancia.games = new List<game_instance>();

								foreach (string d in System.IO.Directory.GetDirectories(ProductsFolder))
								{
									game_instance Game = new game_instance() { path = d };

									switch (new System.IO.DirectoryInfo(d).Name) 
									{
										case "elite-dangerous-64":			//<- Horizons 3.8											
											Game.instance = "Horizons (3.8)";
											Game.name = "Horizons (Legacy)";
											Game.key = "ED_Horizons";											
											break;      

										case "FORC-FDEV-DO-38-IN-40":       //<- Horizons 4.0
											Game.instance = "Horizons (4.0)";
											Game.name = "Horizons (Live)";
											Game.key = "ED_Odissey";
											break;     
											
										case "elite-dangerous-odyssey-64": //<- Odyssey 4.0;
											Game.instance = "Odyssey (4.0)";
											Game.name = "Odyssey (Live)";
											Game.key = "ED_Odissey";
											break;       

										case "FORC-FDEV-DO-1000":       //<- Odyssey 4.0 alt	
											Game.instance = "Odyssey (4.0)";
											Game.name = "Odyssey (Live)";
											Game.key = "ED_Odissey";
											break;    
											
										default:
											Game.instance = "Odyssey (4.0)";
											Game.name = "Odyssey (Live)";
											Game.key = "ED_Odissey";
											break;
									}

									if (File.Exists(Path.Combine(Game.path, "EliteDangerous64.exe")))
									{
										Instancia.games.Add(Game);
									}									
								}

								string Mensaje = string.Empty;
								foreach (var game in Instancia.games)
								{
									Mensaje += string.Format("{0} had been Detected\r\n", game.name);
								}							

								if (!Mensaje.EmptyOrNull())
								{
									//Establece los juegos encontrados en la Instancia Seleccionada
									if (Instancia != null)
									{
										if (XtraMessageBox.Show(string.Format("{0}\r\n\r\nWould you like to Apply the Found Locations to the Instance '{1}'?", Mensaje, Instancia.instance),
																	"GAME HAD BEEN FOUND!",
																	MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
										{
											if (Instancia.games[0].path.Contains("steamapps"))	Instancia.instance = "Steam";
											if (Instancia.games[0].path.Contains("Epic Games"))	Instancia.instance = "Epic Games";
											if (Instancia.games[0].path.Contains("Frontier"))	Instancia.instance = "Frontier";

											this.gridControl1.RefreshDataSource();
											ExpandAllRows(this.gridView1);
										}
									}
								}
								else
								{
									XtraMessageBox.Show("Sorry, could not find the Game Locations, try again.", "ERROR 404 - NOT FOUND",
											MessageBoxButtons.OK, MessageBoxIcon.Error);
								}
							}
							else
							{
								XtraMessageBox.Show("Sorry, could not find the Game Running, try again.", "ERROR 404 - NOT FOUND",
									MessageBoxButtons.OK, MessageBoxIcon.Error);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		//Expandir todas las Filas Maestras:
		public void ExpandAllRows(GridView View)
		{
			//GridView View = sender as GridView;
			View.BeginUpdate();
			try
			{
				int dataRowCount = View.DataRowCount;
				for (int rHandle = 0; rHandle < dataRowCount; rHandle++)
					View.SetMasterRowExpanded(rHandle, true);
			}
			finally
			{
				View.EndUpdate();
			}
		}



		private void cmdSaveChanges_Click(object sender, EventArgs e)
		{
			SaveChanges();
		}

		private void cmdHelp_Click(object sender, EventArgs e)
		{
			GameLocatorAssistant();
		}

		private void repGameFolderSelector_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			/* Este es el boton [...] dentro de la Grilla | Busca El Ejecutable del Juego en la Ruta indicada  */
			try
			{
				ButtonEdit _Selector = sender as ButtonEdit;
				string _path = _Selector.EditValue.NVL("");
				int instance_index = -1;
				int game_index = -1;
				

				if (this.gridControl1.FocusedView != null)
				{
					GameInstance Instance = this.gridView1.GetFocusedRow() as GameInstance;
					if (Instance != null)
					{
						for (int i = 0; i <= this.GameInstancesEx.Count -1; i++)
						{
							if (this.GameInstancesEx[i].instance == Instance.instance)
							{
								instance_index = i; break;
							}							
						}
					}
					var detailView = this.gridControl1.FocusedView as GridView;
					game_instance FR = detailView.GetFocusedRow() as game_instance;
					if (FR != null)
					{
						for (int i = 0; i <= this.GameInstancesEx[instance_index].games.Count - 1; i++)
						{
							if (this.GameInstancesEx[instance_index].games[i].game_id == FR.game_id)
							{
								game_index = i; break;
							}
						}
					}
				}

				OpenFileDialog OFDialog = new OpenFileDialog()
				{
					Filter = "Game Executable|*.exe",
					FilterIndex = 0,
					DefaultExt = "exe",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = true,
					FileName = "EliteDangerous64.exe",
					InitialDirectory = (!_path.EmptyOrNull()) ? _path.ToString() : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};

				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					_Selector.EditValue = System.IO.Path.GetDirectoryName(OFDialog.FileName);

					this.GameInstancesEx[instance_index].games[game_index].path = System.IO.Path.GetDirectoryName(OFDialog.FileName);

					this.gridView2.PostEditor(); //<- Actualiza el DataSource de la Grilla inmediatamente
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}


		private void cmdRemoveInstance_Click(object sender, EventArgs e)
		{
			/* Elimina la Instancia Elejida  */
			GameInstance _Selected = (GameInstance)this.gridView1.GetFocusedRow();
			if (_Selected != null)
			{
				if (XtraMessageBox.Show(string.Format("Do you want to Delete the '{0}' instance?", _Selected.instance), "Are you Sure?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					this.GameInstancesEx.Remove(_Selected);
					this.gridControl1.RefreshDataSource();
				}
			}
		}
		private void cmdAddGameInstance_Click(object sender, EventArgs e)
		{
			/*  Agrega una Nueva Instancia */
			string _GameInstanceName = XtraInputBox.Show("Name for the new Instance:", "Creating a New Game Instance", "");
			if (!_GameInstanceName.EmptyOrNull())
			{
				if (this.GameInstancesEx != null)
				{
					var _Existe = this.GameInstancesEx.Find(x => x.instance == _GameInstanceName.Trim());
					if (_Existe is null)
					{
						GameInstance _NewGame = new GameInstance() { instance = _GameInstanceName.Trim(), games = new List<game_instance>() };
						_NewGame.games.Add(new game_instance()
						{
							key = "ED_Horizons",
							name = "Horizons",
							instance = _GameInstanceName.Trim(),
							themes_folder = @"EDHM-ini\DemoProfiles",
							path = string.Empty,
							is_active = false
						});
						_NewGame.games.Add(new game_instance()
						{
							key = "ED_Odissey",
							name = "Odyssey",
							instance = _GameInstanceName.Trim(),
							themes_folder = @"EDHM-ini\MyProfiles",
							path = string.Empty,
							is_active = false
						});
						this.GameInstancesEx.Add(_NewGame);
						this.gridControl1.RefreshDataSource();

						ExpandAllRows(this.gridView1);

						//Seleccionar una fila segun el valor indicado
						int rowHandle = gridView1.LocateByValue("instance", _GameInstanceName.Trim());
						if (rowHandle != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
							gridView1.FocusedRowHandle = rowHandle;
						

						XtraMessageBox.Show("Instance Created!\r\n\r\nYou now need to set the Game Paths, click the [...] buttons.\r\nYou can also use the 'Game Locator Assistant'.", "Success!",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						XtraMessageBox.Show("There is already an Instance with that name!", "Error",
							MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
			}
		}

		private void txtPlayerJournal_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			try
			{
				ButtonEdit _Selector = sender as ButtonEdit;
				string _path = _Selector.EditValue.NVL("");

				OpenFileDialog OFDialog = new OpenFileDialog()
				{
					Filter = "Log Files|*.log",
					FilterIndex = 0,
					DefaultExt = "log",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = false,
					InitialDirectory = (!_path.EmptyOrNull()) ? _path.ToString() : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};

				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					_Selector.EditValue = System.IO.Path.GetDirectoryName(OFDialog.FileName);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void chkAutoApplyTheme_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void chkSettings_HideToTray_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void lblPi_MouseDown(object sender, MouseEventArgs e)
		{			
			if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
			{
				this.gridView2.Columns[0].OptionsColumn.AllowFocus = false;
				this.gridView2.Columns[1].OptionsColumn.AllowFocus = true;
			}
		}

		private void gridView1_MasterRowExpanded(object sender, CustomMasterRowEventArgs e)
		{
			GridView masterView = sender as GridView;
			GridView detailView = masterView.GetDetailView(e.RowHandle, e.RelationIndex) as GridView;
			detailView.Focus();
		}
	}
}