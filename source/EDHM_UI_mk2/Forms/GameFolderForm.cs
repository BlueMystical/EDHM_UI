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

		public bool WatchMe { get; set; } //<- Determina si Registra las Naves del Jugador
		public bool GreetMe { get; set; } //<- Determina si Saluda al Jugador al Inicio
		public bool HideToTray { get; set; } //<- Determina si se Oculta en el Tray al Cerrar la ventana

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

		private void FindGameEx()
		{
			try
			{

			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
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

					string GameInstances_JSON = Util.Serialize_ToJSON(this.GameInstancesEx);
					Util.WinReg_WriteKey("EDHM", "GameInstances", GameInstances_JSON);

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

					Util.WinReg_WriteKey("EDHM", "HideToTray", this.HideToTray);
					Util.WinReg_WriteKey("EDHM", "GreetMe", this.GreetMe);
					Util.WinReg_WriteKey("EDHM", "WatchMe", this.WatchMe);
					Util.WinReg_WriteKey("EDHM", "Language", this.cboLanguages.EditValue.ToString());

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
			/*  ASISTENTE PARA LOCALIZAR EL JUEGO */
			try
			{
				GameInstance _Selected = (GameInstance)this.gridView1.GetFocusedRow();
				if (_Selected != null)
				{
					if (XtraMessageBox.Show(string.Format("Do you Need Help Finding the Locations for Elite Dangerous Game?\r\nFor the '{0}' Instance.", _Selected.instance),
										"Need a Hand?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						if (XtraMessageBox.Show("First go and Start the Game Client.\r\n\r\nClick Yes when the Game is Running.",
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

								#region Close the Game

								try
								{
									//const uint VK_F1 = 0x70;
									//const uint VK_F2 = 0x71;
									//const uint VK_F3 = 0x72;
									//const uint VK_F4 = 0x73;
									//const uint VK_F5 = 0x74;
									//const uint VK_F6 = 0x75;
									//const uint VK_F7 = 0x76;
									//const uint VK_F8 = 0x77;
									//const uint VK_F9 = 0x78;
									//const uint VK_F10 = 0x79;
									//const uint VK_F11 = 0x7A;
									//const uint VK_F12 = 0x7B;
									//const uint VK_F13 = 0x7C;
									//const uint VK_F14 = 0x7D;
									//const uint VK_F15 = 0x7E;
									//const uint VK_F16 = 0x7F;
									//const uint VK_F17 = 0x80;
									//const uint VK_F18 = 0x81;
									//const uint VK_F19 = 0x82;
									//const uint VK_F20 = 0x83;
									//const uint VK_F21 = 0x84;
									//const uint VK_F22 = 0x85;
									//const uint VK_F23 = 0x86;
									//const uint VK_F24 = 0x87;

									//IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, GameTitle);
									//int result3 = SendMessage(hwnd, (int)VK_F11, (int)VK_F11, "0");
									//SendMessage(hwnd, 0x7A, (int)VK_F11, "{F11}");

									//Aqui se cierra la ventana del juego
									GameProcess.CloseMainWindow();
									GameProcess.Kill();
								}
								catch { }
								finally { GameProcess.Dispose(); }

								#endregion

								if (Directory.Exists(Path.Combine(ProductsFolder, "elite-dangerous-64")))
								{
									HORI_PATH = Path.Combine(ProductsFolder, "elite-dangerous-64");
								}
								if (Directory.Exists(Path.Combine(ProductsFolder, "elite-dangerous-odyssey-64")))
								{
									ODYS_PATH = Path.Combine(ProductsFolder, "elite-dangerous-odyssey-64");
								}
								if (Directory.Exists(Path.Combine(ProductsFolder, "FORC-FDEV-DO-1000")))
								{
									ODYS_PATH = Path.Combine(ProductsFolder, "FORC-FDEV-DO-1000");
								}

								string Msg1 = !HORI_PATH.EmptyOrNull() ? "Horizons had been Detected" : string.Empty;
								string Msg2 = !ODYS_PATH.EmptyOrNull() ? "Odyssey had been Detected" : string.Empty;

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
	}
}