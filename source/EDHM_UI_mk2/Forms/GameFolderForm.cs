﻿using System;
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
						key = "ED_Odyssey",
						name = "Odyssey",
						instance = "Odyssey (Default)",
						themes_folder = @"EDHM-ini\MyProfiles",
						path = Util.WinReg_ReadKey("EDHM", "ED_Odyssey").NVL(string.Empty),
						is_active = Util.IIf(_RegActiveInstance == "ED_Odyssey", true, false)
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

						//Buscar Ruta de Odyssey: \Products\elite-dangerous-odyssey-64
						if (Directory.Exists(Path.Combine(GameDir, @"Products\elite-dangerous-odyssey-64")))
						{
							_ret.Add(Path.Combine(GameDir, @"Products\elite-dangerous-odyssey-64"));
						}
					}
				}
				else
				{
					XtraMessageBox.Show("Program tried to Detect your Game Install paths but failed to do so.\r\nPlease set the Game Paths manually.\r\n'elite-dangerous-64' for Horizons and 'elite-dangerous-Odyssey-64' for Odyssey.",
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
					if (this.GameInstancesEx[0].games[0].path.EmptyOrNull())
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
								case "elite-dangerous-64":			_Game.name = "Horizons (3.8)";	_Game.key = "ED_Horizons"; break;       //<- Horizons 3.8
								case "FORC-FDEV-DO-38-IN-40":		_Game.name = "Horizons (4.0)";	_Game.key = "ED_Odyssey"; break;        //<- Horizons 4.0
								case "elite-dangerous-odyssey-64":	_Game.name = "Odyssey (4.0)";	_Game.key = "ED_Odyssey"; break;        //<- Odyssey 4.0
								case "FORC-FDEV-DO-1000":			_Game.name = "Odyssey (4.0)";	_Game.key = "ED_Odyssey"; break;       //<- Odyssey 4.0 alt
								default: break;
							}

							_Game.instance = string.Format("{0} ({1})", _Instance.instance, _Game.name);
							_Game.game_id = string.Format("{0}|{1}", _Instance.instance, _Game.key);
							_Game.themes_folder = (_Game.key == "ED_Horizons") ?
								Path.Combine(UI_DOCUMENTS, "HORIZ", "Themes") :
								Path.Combine(UI_DOCUMENTS, "ODYSS", "Themes");
						}
					}

					Util.WinReg_WriteKey("EDHM", "GameInstances", Util.Serialize_ToJSON(this.GameInstancesEx));

					if (this.IsFirstRun)
					{
						//si Odyssey tiene path entonces esa es la instancia Activa:
						if (!this.GameInstancesEx[0].games[1].path.EmptyOrNull())
						{
							this.ActiveInstance = this.GameInstancesEx[0].games[1];
						}
						else
						{
							this.ActiveInstance = this.GameInstancesEx[0].games[0];
						}
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
				GameInstance _Selected = (GameInstance)this.gridView1.GetFocusedRow();
				if (_Selected != null)
				{
					if (XtraMessageBox.Show(string.Format("Do you Need Help Finding the Locations for Elite Dangerous Game?\r\nFor the '{0}' Instance.", _Selected.instance),
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
								string ODYS_PATH = string.Empty;
								string HORI_PATH = string.Empty;

								string GameFolder = System.IO.Path.GetDirectoryName(GameProcess.MainModule.FileName); //Obtiene el Path: (Sin archivo ni extension:							
								string ProductsFolder = System.IO.Directory.GetParent(GameFolder).FullName; //<- Obtiene la Carpeta Anterior 
								string RootFolder = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(GameProcess.MainModule.FileName)).Name; //<- Nombre de la Ultima Carpeta en la Ruta

								#region Close the Game

								try
								{
									GameProcess.CloseMainWindow();
									GameProcess.Kill();
								}
								catch { }
								finally { GameProcess.Dispose(); }

								#endregion

								switch (RootFolder)
								{
									case "elite-dangerous-64":			HORI_PATH = GameFolder; _Selected.games[0].name = "Horizons (3.8)";	_Selected.games[0].key = "ED_Horizons"; break;		//<- Horizons 3.8
									case "FORC-FDEV-DO-38-IN-40":		ODYS_PATH = GameFolder; _Selected.games[1].name = "Horizons (4.0)";	_Selected.games[1].key = "ED_Odyssey"; break;		//<- Horizons 4.0
									case "elite-dangerous-odyssey-64":	ODYS_PATH = GameFolder; _Selected.games[1].name = "Odyssey (4.0)";	_Selected.games[1].key = "ED_Odyssey"; break;		//<- Odyssey 4.0
									case "FORC-FDEV-DO-1000":			ODYS_PATH = GameFolder; _Selected.games[1].name = "Odyssey";		_Selected.games[1].key = "ED_Odyssey"; break;		//<- Odyssey 4.0 alt								
									default: break;
								}

								if (HORI_PATH.EmptyOrNull() && Directory.Exists(Path.Combine(ProductsFolder, "elite-dangerous-64")))
								{
									HORI_PATH = Path.Combine(ProductsFolder, "elite-dangerous-64");
									_Selected.games[0].instance = "Horizons (3.8)";
								}
								if (ODYS_PATH.EmptyOrNull() && Directory.Exists(Path.Combine(ProductsFolder, "FORC-FDEV-DO-38-IN-40")))
								{
									ODYS_PATH = Path.Combine(ProductsFolder, "FORC-FDEV-DO-38-IN-40");
									_Selected.games[1].instance = "Horizons (4.0)";
								}
								if (ODYS_PATH.EmptyOrNull() && Directory.Exists(Path.Combine(ProductsFolder, "elite-dangerous-odyssey-64")))
								{
									ODYS_PATH = Path.Combine(ProductsFolder, "elite-dangerous-odyssey-64");
									_Selected.games[1].instance = "Odyssey (4.0)";
								}
								if (ODYS_PATH.EmptyOrNull() && Directory.Exists(Path.Combine(ProductsFolder, "FORC-FDEV-DO-1000")))
								{
									ODYS_PATH = Path.Combine(ProductsFolder, "FORC-FDEV-DO-1000");
									_Selected.games[1].instance = "Odyssey";
								}

								string Msg1 = !HORI_PATH.EmptyOrNull() ? string.Format("{0} had been Detected", _Selected.games[0].name) : string.Empty;
								string Msg2 = !ODYS_PATH.EmptyOrNull() ? string.Format("{0} had been Detected", _Selected.games[1].name) : string.Empty;

								if (!HORI_PATH.EmptyOrNull())
								{
									//Establece los juegos encontrados en la Instancia Seleccionada
									if (_Selected != null)
									{
										if (XtraMessageBox.Show(string.Format("{0}\r\n{1}\r\n\r\nWould you like to Apply the Found Locations to the Instance '{2}'?", Msg1, Msg2, _Selected.instance),
																	"GAME HAD BEEN FOUND!",
																	MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
										{
											_Selected.games[0].path = HORI_PATH;
											_Selected.games[1].path = ODYS_PATH;

											if (HORI_PATH.Contains("steamapps"))	_Selected.instance = "Steam";
											if (HORI_PATH.Contains("Epic Games"))	_Selected.instance = "Epic Games";
											if (HORI_PATH.Contains("Frontier"))		_Selected.instance = "Frontier";

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
							key = "ED_Odyssey",
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
	}
}