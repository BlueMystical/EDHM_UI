using DevExpress.Data.Filtering;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraVerticalGrid.Rows;
using EDHM_DX;
using EDHM_UI_mk2.Clases;
using EDHM_UI_mk2.Forms;
using EDHM_UserInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Util_Test;

namespace EDHM_UI_mk2
{
	/*  EDHM UI - Autor: Ing. Jhollman Chacon R. (Blue Mystic) @ 2021  */
	public partial class MainForm : DevExpress.XtraEditors.XtraForm
	{
		#region Propiedades Publicas

		public List<GameInstance> GameInstancesEx { get; set; }
		public List<game_instance> GameInstances { get; set; }
		public game_instance ActiveInstance { get; set; }

		public ui_setting DefaultSettings { get; set; }
		public ui_preset_new SelectedTheme { get; set; }
		public player_loadout Shipyard { get; set; }
		public ui_group GlobalSettings { get; set; }

		#endregion

		#region Declaraciones Privadas

		[DllImport("User32.dll")]
		private static extern int SetForegroundWindow(IntPtr point);

		private PreviewForm PreviewForm = null;
		private PreviewOdysseyForm PreviewFormODY = null;

		//private AccordionControlElement SelectedThemeElement = null;
		private System.Diagnostics.Process GameWindow = null; //<- La ventana del Juego (si está corriendo)

		private bool ThemeListLoaded = false;
		private bool SilentUpdate = true;
		private bool GameIsRunning = false;
		private bool mCloseAutorized = false;
		private bool DoUpdate = false;
		private bool RunWatcher = true;
		private bool ShowTips = true;

		private bool WatchMe = true; //<- Determina si Registra las Naves del Jugador
		private bool GreetMe = true; //<- Determina si Saluda al Jugador al Inicio
		private bool HideToTray = false; //<- Determina si se Oculta en el Tray al Cerrar la ventana
		private bool StartHidden = false; //<- Se recibe x linea de Comandos, si es true el programa inicia oculto en el Tray.
		private bool SavingTheme = false;
		private bool AutoApplyTheme = false; //<- If true, tries to send the F11 key to the game
		private int SavesToRemember = 10; //<- Cantidad de Guardados a mostrar en el combo

		private DateTime LastCheckDate = DateTime.MinValue;

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private string CommanderName = string.Empty;
		private string ShipIDName = string.Empty;
		private ship CurrentShip = null;
		private string WatchingFile = string.Empty;
		private string LangShort = "en"; //<- Idioma x Defecto
		private string ThemeWord = "Themes"; //<- para una Etiqueta que muestra la cantidad de temas, se traduce a diferentes idiomas

		private string UI_DOCUMENTS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Elite Dangerous\EDHM_UI");



		private event EventHandler OnThemeApply;//<- Ocurre al Aplicar un tema

		private List<ui_preset_new> UI_Themes = null;
		private ui_setting Settings = new ui_setting();
		private List<ui_translation> _Translations = null;
		private List<ship> ED_Ships = null;

		private HashSet<Color> _RecentColors = new HashSet<Color>();            //<- Guarda los Colores del Tema, no admite repetidos
		private List<combo_item> _ElementPresets = new List<combo_item>();



		//Srv_HighBeam = 2147483648
		//https://elite-journal.readthedocs.io/en/latest/Status%20File/
		//https://stackoverflow.com/questions/27294690/decoding-a-bitmask-from-a-value-in-c-sharp

		#endregion

		#region Constructores y Eventos de la Ventana

		public MainForm()
		{
			InitializeComponent();
			DoubleBuffered = true;
		}
		public MainForm(string[] args)
		{
			InitializeComponent();
			DoubleBuffered = true;

			//Leer los argumentos pasados por linea de comandos:
			if (args != null && args.Length > 0)
			{
				StartHidden = args[0] == "-hidden" ? true : false;
				if (args[0] == "-update")
				{
					DoUpdate = true;
				}
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			#region Opciones Regionales y de Idioma

			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			customCulture.NumberFormat.NumberGroupSeparator = ",";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
			customCulture.NumberFormat.CurrencyGroupSeparator = ",";
			customCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
			customCulture.DateTimeFormat.LongDatePattern = "dddd, MMMM d, yyyy";

			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			//Carga el Idioma del Usuario:
			LangShort = Util.WinReg_ReadKey("EDHM", "Language").NVL("en");
			string _AvailableLanguages = Util.AppConfig_GetValue("Languages");
			if (!_AvailableLanguages.Contains(LangShort))
			{
				LangShort = "en";
			}

			WatchMe = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "WatchMe").NVL("true"));
			GreetMe = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "GreetMe").NVL("true"));
			HideToTray = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "HideToTray").NVL("false"));
			ShowTips = Convert.ToBoolean(Util.AppConfig_GetValue("ShowTips").NVL("true"));
			chkTips_NoShow.Checked = !ShowTips;
			cmdThemes_ShowFavorites.Checked = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "FavToggle").NVL("false"));
			SavesToRemember = Convert.ToInt32(Util.WinReg_ReadKey("EDHM", "SavesToRemember").NVL("10"));
			AutoApplyTheme = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "AutoApplyTheme").NVL("false"));

			var EDJournalDir = Util.WinReg_ReadKey("EDHM", "PlayerJournal");
			if (EDJournalDir is null)
			{
				Util.WinReg_WriteKey("EDHM", "PlayerJournal",
					Path.Combine(Environment.GetEnvironmentVariable("UserProfile"), @"Saved Games\Frontier Developments\Elite Dangerous"));
			}

			#endregion

			#region Cargar las Instancias del Juego

			string _RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");
			string GameInstances_JSON = Util.WinReg_ReadKey("EDHM", "GameInstances").NVL(string.Empty);

			if (!GameInstances_JSON.EmptyOrNull())
			{
				GameInstancesEx = Util.DeSerialize_FromJSON_String<List<GameInstance>>(GameInstances_JSON);
				if (GameInstancesEx != null && GameInstancesEx.Count > 0)
				{
					bool HayCambios = false;
					foreach (var _instance in GameInstancesEx)
					{
						foreach (var _game in _instance.games)
						{
							if (_game.game_id.EmptyOrNull())
							{
								HayCambios = true;
								_game.game_id = string.Format("{0}|{1}", _instance.instance, _game.key);
							}
						}
					}
					if (HayCambios)
					{
						GameInstances_JSON = Util.Serialize_ToJSON(GameInstancesEx);
						Util.WinReg_WriteKey("EDHM", "GameInstances", GameInstances_JSON);
					}					
				}
			}
			else
			{
				//Crea Las Instancias x Defecto:
				GameInstancesEx = new List<GameInstance>();
				GameInstancesEx.Add(new GameInstance() { instance = "Default", games = new List<game_instance>() });
				GameInstancesEx[0].games.Add(new game_instance()
				{
					key = "ED_Horizons",
					name = "Horizons",
					instance = "Default (Horizons)",
					game_id = "Default|ED_Horizons",
					themes_folder = Path.Combine(UI_DOCUMENTS, "HORIZ", "Themes"),
					path = Util.WinReg_ReadKey("EDHM", "ED_Horizons").NVL(string.Empty),
					is_active = Util.IIf(_RegActiveInstance == "ED_Horizons", true, false)
				});
				GameInstancesEx[0].games.Add(new game_instance()
				{
					key = "ED_Odissey",
					name = "Odyssey",
					instance = "Default (Odyssey)",
					game_id = "Default|ED_Odissey",
					themes_folder = Path.Combine(UI_DOCUMENTS, "ODYSS", "Themes"),
					path = Util.WinReg_ReadKey("EDHM", "ED_Odissey").NVL(string.Empty),
					is_active = Util.IIf(_RegActiveInstance == "ED_Odissey", true, false)
				});

				GameInstances_JSON = Util.Serialize_ToJSON(GameInstancesEx);
				Util.WinReg_WriteKey("EDHM", "GameInstances", GameInstances_JSON);
			}

			//Carga los valores que se muestran en el Combo:
			GameInstances = new List<game_instance>();
			foreach (var _Instance in GameInstancesEx)
			{
				foreach (var _Game in _Instance.games)
				{
					GameInstances.Add(new game_instance()
					{
						instance = _Game.instance,
						game_id = _Game.game_id,
						key = _Game.key,
						name = _Game.name,
						path = _Game.path,
						is_active = _Game.is_active,
						themes_folder = _Game.themes_folder
					});
				}
			}

			repCboGameInstances.ValueMember = "game_id";
			repCboGameInstances.DisplayMember = "instance";
			repCboGameInstances.DataSource = GameInstances;

			#endregion

			#region Carga el Historial de Naves

			string JsonShipyardPath = Path.Combine(AppExePath, @"Data\PlayerLoadout.json");
			if (File.Exists(JsonShipyardPath))
			{
				Shipyard = Util.DeSerialize_FromJSON<player_loadout>(JsonShipyardPath);
			}
			else
			{
				Shipyard = new player_loadout();
			}

			#endregion

			#region Seleccionar la Instancia Activa

			string[] _ActiveGames = _RegActiveInstance.Split(new char[] { '|' });
			if (_ActiveGames != null && _ActiveGames.Length > 1)
			{
				ActiveInstance = GameInstances.Find(x => x.game_id == _RegActiveInstance);
			}
			else
			{
				_RegActiveInstance = string.Format("{0}|{1}", GameInstancesEx[0].instance, _RegActiveInstance.NVL("ED_Horizons"));
				ActiveInstance = GameInstances.Find(x => x.game_id == _RegActiveInstance);
				Util.WinReg_WriteKey("EDHM", "ActiveInstance", _RegActiveInstance);
			}

			if (ActiveInstance != null)
			{
				CboGameInstances.EditValue = ActiveInstance.game_id;

				string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";

				lblVersion_App.Caption = string.Format("App Version: {0}", System.Configuration.ConfigurationManager.AppSettings["AppVersion"].ToString());
				lblVersion_MOD.Caption = string.Format("Mod Version: {0}", Util.WinReg_ReadKey("EDHM", string.Format("Version_{0}", search)).NVL("v1.51"));
			}

			#endregion

			//this.Location = new Point(0, 0);
		}
		private void MainForm_Shown(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				progressPanel1.Dock = DockStyle.Fill;
				progressPanel1.Caption = "Loading..";
				progressPanel1.Description = "please wait";
				progressPanel1.Visible = true;
				progressPanel1.BringToFront();

				if (StartHidden)
				{
					Hide();
					notifyIcon1.Visible = true;
				}

				bool FirstRun = Convert.ToBoolean(Util.AppConfig_GetValue("FirstRun"));
				if (FirstRun || DoUpdate)
				{
					//Si es la primera vez, se instala el MOD
					SilentUpdate = true;
					Cursor = Cursors.WaitCursor;
					MainMenu_ApplyTheme.Enabled = false;
					gridControl1.Cursor = Cursors.WaitCursor;

					if (GameInstances != null)
					{
						foreach (var _Instance in GameInstances)
						{
							if (!_Instance.path.EmptyOrNull())
							{
								InstallGameInstance(_Instance);
							}
						}
					}
					Util.AppConfig_SetValue("FirstRun", "false");
					Task.Factory.StartNew(() => { Run_HotFix(); });
				}

				CheckForModUpdates();
				LoadGameInstance(ActiveInstance, LangShort);  //<- Carga La Instancia Activa, tambien verifica si el MOD esta instalado	
				LoadMenus(LangShort);
				LoadThemeList_EX(); //<- Cargar la Lista de Temas disponibles
				LoadGlobalSettings(ActiveInstance);
				LoadShipList();


				string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
				lblVersion_App.Caption = string.Format("App Version: {0}", System.Configuration.ConfigurationManager.AppSettings["AppVersion"].ToString());
				lblVersion_MOD.Caption = string.Format("Mod Version: {0}", Util.WinReg_ReadKey("EDHM", string.Format("Version_{0}", search)).NVL("v1.51"));

				Load_UITips();
				History_LoadElements(SavesToRemember);

				PlayerJournal_GetPlayerInfo();
				ReadPlayerJournal();

				progressPanel1.Visible = false;

			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				Cursor = Cursors.Default;
				MainMenu_ApplyTheme.Enabled = true;
				gridControl1.Cursor = Cursors.Default;
			}
		}
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			//Al intentar cerrar la ventana se minimiza en la bandeja 'SysTray'
			if (e.CloseReason == CloseReason.UserClosing)
			{
				if (!mCloseAutorized && HideToTray)
				{
					e.Cancel = true;
					Hide();
					notifyIcon1.Visible = true;

					ShowSystemNotificacion("EDHM - UI", "Will be running in the background.");
				}
				else
				{
					try
					{
						//Guarda los Global Settings:
						if (GlobalSettings != null)
						{
							string _TemplatesPath = Path.Combine(AppExePath, "Data");
							string _File = Path.Combine(_TemplatesPath, ActiveInstance.key + "_Global_Settings.json");

							Util.Serialize_ToJSON(_File, GlobalSettings);
						}

						//Aqui se cierra normalmente:
						if (notifyIcon1 != null)
						{
							notifyIcon1.Visible = false;
							notifyIcon1.Icon.Dispose();
							notifyIcon1.Dispose();
						}
						if (GameWindow != null)
						{
							GameWindow.Dispose();
						}
					}
					catch { }
				}
			}
		}
		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			//Al Cerrar definitivamente el Formulario elimina el icono del SysTray
			if (notifyIcon1 != null)
			{
				notifyIcon1.Visible = false;
				if (notifyIcon1.Icon != null)
				{
					notifyIcon1.Icon.Dispose();
				}

				notifyIcon1.Dispose();
			}
			if (GameWindow != null)
			{
				GameWindow.Dispose();
			}
		}
		private void MainForm_Resize(object sender, EventArgs e)
		{
			//Al Minimizar la Ventana, se oculta y se minimiza en la bandeja 'SysTray'
			if (WindowState == FormWindowState.Minimized)
			{
				//Hide();

				////Muestra una Notificacion en la Bandeja del Sistema:
				//this.notifyIcon1.Visible = true;

				//this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
				//this.notifyIcon1.BalloonTipTitle = "EDHM - UI";
				//this.notifyIcon1.BalloonTipText = "This program would be running in the background.";
				//this.notifyIcon1.ShowBalloonTip(3000); //<- Ocultar tras 5 segundos.
			}
		}

		#endregion

		#region Metodos

		bool LoadingThemes = false; //<- Previene que se llame este metodo varias veces a la vez.
		bool LoadingTheme = false; //<- Previene que se llame este metodo varias veces a la vez.
		Stopwatch _Stopwatch = new Stopwatch();

		private bool LoadingGameInstance = false;
		private void LoadGameInstance(game_instance pGameInstance, string pLang = "en")
		{
			/* AQUI SE CARGA LA CONFIGURACION ACTUAL */
			try
			{
				if (LoadingGameInstance)
				{
					return;
				}

				LoadingGameInstance = true;

				//Verificar si La instancia está Seteada:
				if (ActiveInstance == null || ActiveInstance.path.EmptyOrNull() || ActiveInstance.path == "[NOT_FOUND]")
				{
					//La Ruta del Juego NO ha sido Establecida!
					GameFolderForm _Form = new GameFolderForm(GameInstancesEx);
					if (_Form.ShowDialog() == DialogResult.OK)
					{
						GameInstancesEx = _Form.GameInstancesEx;
						ActiveInstance = GameInstancesEx[0].games[0]; //<- Horizons de la Primera instancia
						if (ActiveInstance != null)
						{
							CboGameInstances.EditValue = ActiveInstance.key;
						}

						if (XtraMessageBox.Show("Would you like to Update your EDHM?", "UPDATE?",
							MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							InstallGameInstance(ActiveInstance);
							LoadThemeList_EX();
						}
					}
				}

				//Verificar si el MOD está Instalado:
				if (!ActiveInstance.path.EmptyOrNull())
				{
					string gamekey = Path.Combine(ActiveInstance.path, "d3dx.ini");
					if (!File.Exists(gamekey))
					{
						//El MOD no está Instalado.
						InstallGameInstance(ActiveInstance);
						LoadThemeList_EX();
					}
				}

			//Cargar Los Valores Base de La Instancia
			Inicio:
				string JsonSettings_path = Path.Combine(AppExePath, "Data", pGameInstance.key + string.Format("_Settings_EN.json"));
				if (File.Exists(JsonSettings_path))
				{
					Settings = Util.DeSerialize_FromJSON<ui_setting>(JsonSettings_path);

					//Carga la Lista de Presets disponibles:
					if (Settings != null && Settings.Presets.IsNotEmpty())
					{
						_ElementPresets = Settings.Presets;
					}

					DefaultSettings = Load_DefaultTheme(pGameInstance, pLang);

					//LoadMenus(pLang);
				}
				else
				{
					XtraMessageBox.Show(string.Format("The language file '{0}' could not be found!\r\nDefault EN will be loaded.", JsonSettings_path),
						"ERROR 404: NOT FOUND", MessageBoxButtons.OK, MessageBoxIcon.Error);

					pLang = "en";
					LangShort = "en";
					goto Inicio;
				}

				if (ActiveInstance.key == "ED_Odissey" && PreviewForm != null && PreviewForm.Visible)
				{
					PreviewForm.Close();
				}

				LoadingGameInstance = false;

				string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
				lblVersion_MOD.Caption = string.Format("Mod Version: {0}",
					Util.WinReg_ReadKey("EDHM", string.Format("Version_{0}", search)).NVL("v1.51"));
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private bool InstallGameInstance(game_instance pGameInstance)
		{
			bool _ret = false;
			try
			{
				string update_path = Path.Combine(AppExePath, "Updates");
				string search = pGameInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";

				KillGameProcces();

				if (Directory.Exists(update_path))
				{
					bool Backup = true; //<- Determina si se Respaldan los 'Current Settings' del Usuario
					DirectoryInfo di = new DirectoryInfo(update_path);

					//Busca el Archivo ZIP mas reciente:
					var UpdateFile = di.GetFiles(string.Format("{0}_EDHM-*.zip", search))
						.OrderByDescending(f => f.LastWriteTime).First();

					if (UpdateFile != null)
					{
						//Get File's Version:
						string[] _Parts = UpdateFile.Name.Split(new char[] { '-' });
						string _Version = _Parts[1].Remove(_Parts[1].Length - 4); //<- 'v1.51'

						if (Backup)
						{
							//Backup Current Settings:
							if (!Directory.Exists(@"C:\Temp"))
							{
								Directory.CreateDirectory(@"C:\Temp");
							}

							if (File.Exists(Path.Combine(pGameInstance.path, "EDHM-ini", "Startup-Profile.ini")))
							{
								File.Copy(Path.Combine(pGameInstance.path, "EDHM-ini", "Startup-Profile.ini"),
											   Path.Combine(@"C:\Temp", pGameInstance.key + "_Startup-Profile.ini"), true);

								File.Copy(Path.Combine(pGameInstance.path, "EDHM-ini", "XML-Profile.ini"),
											   Path.Combine(@"C:\Temp", pGameInstance.key + "_XML-Profile.ini"), true);
							}
							if (File.Exists(Path.Combine(pGameInstance.path, "EDHM-ini", "Custom.ini")))
							{
								File.Copy(Path.Combine(pGameInstance.path, "EDHM-ini", "Custom.ini"),
											   Path.Combine(@"C:\Temp", pGameInstance.key + "_Custom.ini"), true);
							}
							if (pGameInstance.key == "ED_Odissey" && File.Exists(Path.Combine(pGameInstance.path, "EDHM-ini", "Advanced.ini")))
							{
								File.Copy(Path.Combine(pGameInstance.path, "EDHM-ini", "Advanced.ini"),
											   Path.Combine(@"C:\Temp", pGameInstance.key + "_Advanced.ini"), true);
							}
						}

						//Descomprime el ZIP de EDHM en el Directorio del Juego:
						if (Util.DoNetZIP_UnCompressFile(UpdateFile.FullName, pGameInstance.path))
						{
							_ret = true;

							//Store Imported Version:
							Util.WinReg_WriteKey("EDHM", string.Format("Version_{0}", search), _Version);

							SetGraphicSettings();
						}

						// Unzip the Themes: D:\Documentos\Elite Dangerous\EDHM_UI\ODYSS\Themes
						string origen = Path.Combine(update_path, string.Format("Themes_EDHM_{0}.zip", search));
						if (File.Exists(origen))
						{
							string destino = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Elite Dangerous\EDHM_UI\", search);
							if (!Directory.Exists(destino))
							{
								Directory.CreateDirectory(destino);
							}

							Util.DoNetZIP_UnCompressFile(origen, destino);

							pGameInstance.themes_folder = Path.Combine(destino, "Themes");
						}

						if (Backup) //<- Restore User's Settings:
						{
							try
							{
								if (File.Exists(Path.Combine(@"C:\Temp", pGameInstance.key + "_Startup-Profile.ini")))
								{
									File.Copy(Path.Combine(@"C:\Temp", pGameInstance.key + "_Startup-Profile.ini"),
										Path.Combine(pGameInstance.path, "EDHM-ini", "Startup-Profile.ini"), true);
								}
								if (File.Exists(Path.Combine(@"C:\Temp", pGameInstance.key + "_XML-Profile.ini")))
								{
									File.Copy(Path.Combine(@"C:\Temp", pGameInstance.key + "_XML-Profile.ini"),
										Path.Combine(pGameInstance.path, "EDHM-ini", "XML-Profile.ini"), true);
								}
								if (pGameInstance.key == "ED_Horizons" && File.Exists(Path.Combine(@"C:\Temp", pGameInstance.key + "_Custom.ini")))
								{
									File.Copy(Path.Combine(@"C:\Temp", pGameInstance.key + "_Custom.ini"),
										Path.Combine(pGameInstance.path, "EDHM-ini", "Custom.ini"), true);
								}
								if (pGameInstance.key == "ED_Odissey" && File.Exists(Path.Combine(@"C:\Temp", pGameInstance.key + "_Advanced.ini")))
								{
									File.Copy(Path.Combine(@"C:\Temp", pGameInstance.key + "_Advanced.ini"),
										Path.Combine(pGameInstance.path, "EDHM-ini", "Advanced.ini"), true);
								}
							}
							catch { }
						}

						XtraMessageBoxArgs args = new XtraMessageBoxArgs()
						{
							Caption = "Success!",
							Text = string.Format("EDHM Version '{0}' had been Installed!\r\n{1}", _Version, pGameInstance.instance),
							Buttons = new DialogResult[] { DialogResult.OK }
						};
						args.AutoCloseOptions.Delay = 2000;
						args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
						XtraMessageBox.Show(args).ToString();
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private void SetGraphicSettings()
		{
			/*  CAMBIA LOS VALORES DE ALGUNOS SETTINGS PARA QUE SEAN COMPATIBLES CON EDHM */
			try
			{
				string LocalAppData = Environment.GetEnvironmentVariable("LocalAppData");
				string FilePath = string.Empty;
				XDocument xmlFile;

				#region Set the 'DisableGuiEffects'

				FilePath = Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Settings.xml");
				if (File.Exists(FilePath))
				{
					xmlFile = System.Xml.Linq.XDocument.Load(FilePath);

					var query = from c in xmlFile.Elements("GraphicsOptions").Elements("DisableGuiEffects")
								select c;

					//update Atribute Value
					//foreach (System.Xml.Linq.XElement book in query)
					//{
					//	book.Attribute("attr1").Value = "MyNewValue";
					//}

					//update elements value
					foreach (XElement element in query)
					{
						element.Value = "false";
					}

					xmlFile.Save(FilePath);
				}

				#endregion

				#region MaterialQuality

				//FilePath = Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Custom.4.0.fxcfg");
				//if (File.Exists(FilePath))
				//{
				//	xmlFile = System.Xml.Linq.XDocument.Load(FilePath);

				//	var query2 = from c in xmlFile.Root.Elements("MaterialQuality")
				//				 select c;
				//	foreach (XElement element in query2)
				//	{
				//		element.Value = "3";
				//	}
				//	xmlFile.Save(FilePath);
				//}

				#endregion

				#region Set the XML Colors to the Default Identity Matrix

				FilePath = Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\GraphicsConfigurationOverride.xml");
				if (File.Exists(FilePath))
				{
					try
					{
						var _XmlReader = new System.Xml.XmlDocument();
						_XmlReader.Load(FilePath);
						Util.SetXMLValue(_XmlReader, @"/GraphicsConfig/GUIColour/Default/MatrixRed", " 1, 0, 0 ");
						Util.SetXMLValue(_XmlReader, @"/GraphicsConfig/GUIColour/Default/MatrixGreen", " 0, 1, 0 ");
						Util.SetXMLValue(_XmlReader, @"/GraphicsConfig/GUIColour/Default/MatrixBlue", " 0, 0, 1 ");
						_XmlReader.Save(FilePath);
					}
					catch 
					{
						StringBuilder _GP = new StringBuilder();
						_GP.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
						_GP.AppendLine("<GraphicsConfig>");
						_GP.AppendLine("  <GUIColour>");
						_GP.AppendLine("    <Default>");
						_GP.AppendLine("      <LocalisationName>Standard</LocalisationName>");
						_GP.AppendLine("      <MatrixRed>1, 0, 0</MatrixRed>");
						_GP.AppendLine("      <MatrixGreen>0, 1, 0</MatrixGreen>");
						_GP.AppendLine("      <MatrixBlue>0, 0, 1</MatrixBlue>");
						_GP.AppendLine("    </Default>");
						_GP.AppendLine("  </GUIColour>");
						_GP.AppendLine("</GraphicsConfig>");

						Util.SaveTextFile(FilePath, _GP.ToString(), Util.TextEncoding.UTF8);
					}

					/* try
					{
						xmlFile = System.Xml.Linq.XDocument.Load(FilePath);

						var query = from c in xmlFile.Elements("GraphicsConfig").Elements("GUIColour").Elements("Default") select c;
						if (query != null)
						{
							foreach (XElement element in query.Elements())
							{
								if (element.Name == "MatrixRed")
								{
									element.Value = "1, 0, 0";
								}

								if (element.Name == "MatrixGreen")
								{
									element.Value = "0, 1, 0";
								}

								if (element.Name == "MatrixBlue")
								{
									element.Value = "0, 0, 1";
								}
							}
							xmlFile.Save(FilePath);
						}
					}
					catch
					{

						StringBuilder _GP = new StringBuilder();
						_GP.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
						_GP.AppendLine("<GraphicsConfig>");
						_GP.AppendLine("  <GUIColour>");
						_GP.AppendLine("    <Default>");
						_GP.AppendLine("      <LocalisationName>Standard</LocalisationName>");
						_GP.AppendLine("      <MatrixRed>1, 0, 0</MatrixRed>");
						_GP.AppendLine("      <MatrixGreen>0, 1, 0</MatrixGreen>");
						_GP.AppendLine("      <MatrixBlue>0, 0, 1</MatrixBlue>");
						_GP.AppendLine("    </Default>");
						_GP.AppendLine("  </GUIColour>");
						_GP.AppendLine("</GraphicsConfig>");

						Util.SaveTextFile(FilePath, _GP.ToString(), Util.TextEncoding.UTF8);
					}*/
				}

				#endregion
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void LoadMenus(string pLang = "en")
		{
			try
			{
				#region Load the Translated Menu Elements

				string JsonSettings_path = Path.Combine(AppExePath, string.Format("Data\\{0}_Translations.json", ActiveInstance.key));
				if (File.Exists(JsonSettings_path))
				{
					_Translations = Util.DeSerialize_FromJSON<List<ui_translation>>(JsonSettings_path);
					if (_Translations != null)
					{
						#region Load the Menu Translations

						List<ui_translation> _MenuTranslation = _Translations.FindAll(x => x.group == "BarMenus");

						var _Controls = barManager1.Items.ToList();
						foreach (var item in _MenuTranslation)
						{
							var _Trans = item.lang.FirstOrDefault(x => x.key == LangShort);

							//Obtiene la traduccion de la palabra 'Temas':
							if (item.id == "MainMenu_Themes")
							{
								ThemeWord = _Trans.value.Split(new char[] { ' ' }).FirstOrDefault();
							}

							var _Menu = _Controls.Find(x => x.Name == item.id);
							if (_Menu != null)
							{
								_Menu.Caption = _Trans.value;

								//Revisa si tiene datos para el 'ToolTip':
								if (!_Trans.description.EmptyOrNull())
								{
									string[] _Tip = _Trans.description.Split(new char[] { '/' });

									SuperToolTip sTooltip1 = new SuperToolTip();
									SuperToolTipSetupArgs args = new SuperToolTipSetupArgs();
									args.Title.Text = _Tip[0];
									args.Contents.Text = _Tip[1];
									sTooltip1.Setup(args);

									_Menu.SuperTip = sTooltip1;
								}
							}
						}

						#endregion

						#region Load the Group Tiles

						List<ui_translation> _TileGroupTranslations = _Translations.FindAll(x => x.group == "TileGroup");
						if (_TileGroupTranslations.IsNotEmpty())
						{
							foreach (var _TileTrans in _TileGroupTranslations)
							{
								try
								{
									var _Translation = _TileTrans.lang.FirstOrDefault(x => x.key == LangShort);
									Settings.ui_groups.FirstOrDefault(x => x.Name == _TileTrans.id).Title = _Translation.value;

									if (_TileTrans.id == "Panel_UP")
									{
										tilePanel_UP.Text = _Translation.value;
									}

									if (_TileTrans.id == "Settings")
									{
										tileSettings.Text = _Translation.value;
									}

									if (_TileTrans.id == "SpecialFX")
									{
										tileSpecialFX.Text = _Translation.value;
									}

									if (_TileTrans.id == "Panel_Lower")
									{
										tilePanel_Lower.Text = _Translation.value;
									}

									if (_TileTrans.id == "Lighting")
									{
										tileLighting.Text = _Translation.value;
									}

									if (_TileTrans.id == "Reticle")
									{
										tileReticle.Text = _Translation.value;
									}

									if (_TileTrans.id == "RadarHUD")
									{
										tileRadarHUD.Text = _Translation.value;
									}

									if (_TileTrans.id == "CHUD_AHUD")
									{
										tileCHUD_AHUD.Text = _Translation.value;
									}

									if (_TileTrans.id == "ShieldsHolo")
									{
										tileShieldsHolo.Text = _Translation.value;
									}

									if (_TileTrans.id == "StationsPanels")
									{
										tileStationsPanels.Text = _Translation.value;
									}

									if (_TileTrans.id == "Power_Distributor")
									{
										tileIPower_Distributor.Text = _Translation.value;
									}

									if (_TileTrans.id == "FuelBars")
									{
										tileFuelBars.Text = _Translation.value;
									}

									if (_TileTrans.id == "Foot_HUD")
									{
										tileFoot_HUD.Text = _Translation.value;
									}
								}
								catch { }
							}
						}

						#endregion
					}
				}

				#endregion
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadShipList()
		{
			string _FilePath = Path.Combine(AppExePath, @"Data\Ship_List.json");
			if (File.Exists(_FilePath))
			{
				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

					ED_Ships = Util.DeSerialize_FromJSON<List<ship>>(_FilePath);
				});
			}
		}

		private void LoadElementPreviews()
		{
			/* CARGA LAS IMAGENES QUE SE USAN EN LOS TOOLTIPS DE CADA ELEMENTO  */
			try
			{
				string ElementsFolder = Path.Combine(AppExePath, @"Images\Elements_ODY");
				if (Directory.Exists(ElementsFolder))
				{
					ElementsImgCollection.Images.Clear();

					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

						foreach (string _ImageFile in Directory.GetFiles(ElementsFolder, "*.png"))
						{
							//Las imagenes se pueden acceder usando el nombre del archivo
							//El nombre del archivo se corresponde con la 'key' del Elemento
							ElementsImgCollection.AddImage(
								Util.GetElementImage(_ImageFile), Path.GetFileNameWithoutExtension(_ImageFile));
						}
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadThemeList_EX()
		{
			/* AQUI SE CARGA LA LISTA DE TEMAS DISPONIBLES DESDE LA CARPETA 'DEMO-PROFILES'  */
			Cursor = Cursors.WaitCursor;
			try
			{
				if (!ActiveInstance.path.EmptyOrNull() && !LoadingThemes)
				{
					LoadElementPreviews();

					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						int theme_count = 0;
						LoadingThemes = true;
						ThemeListLoaded = false;
						UI_Themes = new List<ui_preset_new>();

						string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
						ActiveInstance.themes_folder = Path.Combine(UI_DOCUMENTS, search, "Themes");

						if (Directory.Exists(ActiveInstance.themes_folder))
						{
							#region Agregar Primero el Thema Actual:

							ui_preset_new _mytheme = new ui_preset_new("Current Settings", Path.Combine(ActiveInstance.path, @"EDHM-ini"), "User")
							{
								Preview = Util.GetElementImage(Path.Combine(AppExePath, "Images", "PREVIEW.PNG")),
								IsFavorite = true
							};
							UI_Themes.Add(_mytheme);

							#endregion

							#region Cargar Todos los Temas Disponibles

							foreach (var d in System.IO.Directory.GetDirectories(ActiveInstance.themes_folder))
							{
								DirectoryInfo dir = new DirectoryInfo(d);
								ui_preset_new _theme = new ui_preset_new(dir.Name, dir.FullName);
								theme_count++;

								//Buscar el Archivo que identifica al Autor del Tema:
								string CreditsFile = dir.GetFiles("*.credits").Select(fi => fi.Name).FirstOrDefault().NVL("Unknown.credits");
								if (!CreditsFile.EmptyOrNull())
								{
									_theme.author = Path.GetFileNameWithoutExtension(CreditsFile);
									theme_details ThemeDetails = null;

									try
									{
										ThemeDetails = Util.DeSerialize_FromJSON<theme_details>(Path.Combine(_theme.folder, CreditsFile));
										if (ThemeDetails != null)
										{
											_theme.name = ThemeDetails.theme;
											_theme.author = ThemeDetails.author;
											_theme.description = ThemeDetails.description;
											_theme.BigPreview = ThemeDetails.preview;
										}
									}
									catch { }

									//La descripcion del Tema esta dentro del archivo, exepto la primera linea.
									if (ThemeDetails == null)
									{
										if (File.Exists(Path.Combine(_theme.folder, CreditsFile)))
										{
											string _description = Util.ReadTextFile(Path.Combine(_theme.folder, CreditsFile));
											_theme.description = _description.RemoveLine(0);
										}
									}
								}

								//Buscar el archivo que identifica al tema como Favorito
								if (File.Exists(Path.Combine(_theme.folder, "IsFavorite.fav")))
								{
									_theme.IsFavorite = true;
								}

								//Buscar el archivo del Thumbnail:
								if (File.Exists(Path.Combine(_theme.folder, "PREVIEW.jpg")))
								{
									//Carga la Imagen sin dejara 'en uso':
									using (Stream stream = File.OpenRead(Path.Combine(_theme.folder, "PREVIEW.jpg")))
									{
										_theme.Preview = System.Drawing.Image.FromStream(stream);
										_theme.HasPreview = true;
									}
									_theme.Preview = Util.ResizeImage((Bitmap)_theme.Preview, 360, 71, false);
								}
								else
								{
									//sI EL TEMA NO TIENE PREVIEW, USA UNA IMAGEN X DEFECTO;
									using (Stream stream = File.OpenRead(Path.Combine(AppExePath, "Images", "PREVIEW_DEFAULT.PNG")))
									{
										_theme.Preview = System.Drawing.Image.FromStream(stream);
										_theme.HasPreview = false;
									}
								}

								#region Write Theme's Name

								//Escribe el Nombre del Mod Sobre la Imagen Thumbnail:
								if (_theme.Preview != null)
								{
									Bitmap bm = new Bitmap(_theme.Preview.Width, _theme.Preview.Height);
									using (Graphics e = Graphics.FromImage(bm))
									{
										e.DrawImage(_theme.Preview,
											new Rectangle(new Point(0, 0),
											new Size(_theme.Preview.Width, _theme.Preview.Height)));
										e.DrawString(_theme.name, new System.Drawing.Font("Tahoma", 9, FontStyle.Bold),
												Brushes.White, 10, _theme.Preview.Height - 20);
									}
									_theme.Preview = bm;
								}

								#endregion

								UI_Themes.Add(_theme);
							}

							#endregion
						}

						//3. Load and Show the presets in the Combo control:
						Invoke((MethodInvoker)(() =>
						{
							try
							{
								if (UI_Themes.IsNotEmpty())
								{
									gridControl1.DataSource = UI_Themes;
								}
								ThemeListLoaded = true;

								// Carga los Settings Actuales:
								if (UI_Themes.IsNotEmpty())
								{
									ui_preset_new _theme = gridView1.GetFocusedRow() as ui_preset_new;
									SelectedTheme = UI_Themes[0];
									if (SelectedTheme != null)
									{
										LoadTheme(SelectedTheme);
									}

									dockThemes.Text = string.Format("{0} {1}", theme_count, ThemeWord);
								}
							}
							catch { }
						}));
						LoadingThemes = false;
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { Cursor = Cursors.Default; }
		}

		private ui_setting Load_DefaultTheme(game_instance pGameInstance, string pLang = "en")
		{
			ui_setting _ret = null;
			try
			{
				string _TemplatesPath = Path.Combine(AppExePath, "Data");

				IniFile _StartupReader = new IniFile(Path.Combine(_TemplatesPath, pGameInstance.key + "_Startup-Profile.ini"));
				IniFile _ReaderAdvanced = null;
				IniFile _ReaderSuitHud = null;
				IniFile _ReaderCustom = null;

				if (pGameInstance.key == "ED_Odissey" && File.Exists(Path.Combine(_TemplatesPath, pGameInstance.key + "_Advanced.ini")))
				{
					_ReaderAdvanced = new IniFile(Path.Combine(_TemplatesPath, pGameInstance.key + "_Advanced.ini"));
					_ReaderSuitHud = new IniFile(Path.Combine(_TemplatesPath, pGameInstance.key + "_SuitHud.ini"));
				}
				else
				{
					_ReaderCustom = new IniFile(Path.Combine(_TemplatesPath, pGameInstance.key + "_Custom.ini"));
				}

				//string JsonSettings_path = Path.Combine(_TemplatesPath, string.Format("{0}_Settings_{1}.json", pGameInstance.key, pLang.ToUpper()));
				string JsonSettings_path = Path.Combine(_TemplatesPath, string.Format("{0}_Settings_EN.json", pGameInstance.key));
				if (File.Exists(JsonSettings_path))
				{
					_ret = Util.DeSerialize_FromJSON<ui_setting>(JsonSettings_path);
				}

				if (_ret != null && _StartupReader != null && _ret.ui_groups.IsNotEmpty())
				{
					foreach (var _group in _ret.ui_groups)
					{
						if (_group.Elements.IsNotEmpty())
						{
							foreach (var _Element in _group.Elements)
							{
								switch (_Element.ValueType)
								{
									case "Color":
										#region Es un Color

										//Key = 'x176|y176|z176'
										string[] keys = _Element.Key.Split(new char[] { '|' });
										if (keys != null && keys.Length > 0)
										{
											List<double> _RGBA = new List<double>();
											foreach (string _Key in keys)
											{
												switch (_Element.File)
												{
													case "Startup-Profile":
														_RGBA.Add(Convert.ToDouble(_StartupReader.ReadKey(_Key, _Element.Section).NVL("-1")));
														break;

													case "Advanced":
														_RGBA.Add(Convert.ToDouble(_ReaderAdvanced.ReadKey(_Key, _Element.Section).NVL("-1")));
														break;

													case "Custom":
														_RGBA.Add(Convert.ToDouble(_ReaderCustom.ReadKey(_Key, _Element.Section).NVL("-1")));
														break;

													case "SuitHud":
														_RGBA.Add(Convert.ToDouble(_ReaderSuitHud.ReadKey(_Key, _Element.Section).NVL("-1")));
														break;

													default:
														_RGBA.Add(Convert.ToDouble(_StartupReader.ReadKey(_Key, _Element.Section).NVL("-1")));
														break;
												}
											}

											//convertir de GammaCorrected -> sRGB -> RGB
											if (IsColor(_RGBA))
											{
												_Element.Value = ReverseGammaCorrected(_RGBA).ToArgb();
											}
											else
											{
												//Si la Clave no existe en el Tema elejido, se carga el valor del tema plantilla:
												_Element.Value = _Element.Value;
											}
										}

										#endregion
										break;

									//case "Preset": break;
									//case "Brightness": break;
									//case "ONOFF": break;
									//case "": break;
									default:
										decimal _defaultValue = _Element.Value;
										switch (_Element.File)
										{
											case "Startup-Profile":
												_Element.Value = Convert.ToDecimal(_StartupReader.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
												break;

											case "Advanced":
												_Element.Value = Convert.ToDecimal(_ReaderAdvanced.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
												break;

											case "Custom":
												_Element.Value = Convert.ToDecimal(_ReaderCustom.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
												break;

											case "SuitHud":
												_Element.Value = Convert.ToDecimal(_ReaderSuitHud.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
												break;

											default:
												_Element.Value = Convert.ToDecimal(_ReaderAdvanced.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
												break;
										}
										if (_Element.Value < 0)
										{
											_Element.Value = _defaultValue;
										}
										break;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private void LoadTheme(ui_preset_new _Theme)
		{
			try
			{
				//if (_Stopwatch.IsRunning) _Stopwatch.Stop();
				_Stopwatch.Start();
				switch (ActiveInstance.key)
				{
					case "ED_Horizons": LoadTheme_Horizons(_Theme); break;
					case "ED_Odissey": LoadTheme_Odissey(_Theme); break;
					default: break;
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void LoadTheme_Horizons(ui_preset_new _Theme)
		{
			Cursor = Cursors.WaitCursor;

			try
			{
				if (_Theme != null && !LoadingTheme)
				{
					if (!_Theme.folder.EmptyOrNull() && Directory.Exists(_Theme.folder))
					{
						LoadingTheme = true;

						//Si el archivo no existe lo copia desde las Templates:
						if (!File.Exists(Path.Combine(_Theme.folder, "Custom.ini")))
						{
							string template_path = Path.Combine(AppExePath, "Data", string.Format("{0}_Custom.ini", ActiveInstance.key));
							if (File.Exists(template_path))
							{
								File.Copy(template_path, Path.Combine(_Theme.folder, "Custom.ini"), true);
							}
						}

						//3. Abrir el Archivo INI desde su Ubicacion:
						IniFile _Reader = new IniFile(Path.Combine(_Theme.folder, @"Startup-Profile.ini"));
						IniFile _Custom = new IniFile(Path.Combine(_Theme.folder, @"Custom.ini")); //<- Open and Read the INI file
						IniFile _ReaderXML = new IniFile(Path.Combine(_Theme.folder, @"XML-Profile.ini"));

						if (_Reader != null)
						{
							if (Settings.ui_groups.IsNotEmpty())
							{
								Settings.name = _Theme.name;
								Settings.author = _Theme.author;

								_RecentColors = new HashSet<Color>();

								foreach (var _group in Settings.ui_groups)
								{
									if (_group.Elements.IsNotEmpty())
									{
										foreach (var _Element in _group.Elements)
										{
											if (_Element.ValueType == "Color")
											{
												#region Es un Color

												List<double> _GammaColors = new List<double>();

												string[] keys = _Element.Key.Split(new char[] { '|' });
												if (keys != null && keys.Length > 0)
												{
													foreach (string _Key in keys)
													{
														switch (_Element.File)
														{
															case "Startup-Profile":
																_GammaColors.Add(Convert.ToDouble(_Reader.ReadKey(_Key, _Element.Section).NVL("-1")));
																break;

															case "Custom":
																_GammaColors.Add(Convert.ToDouble(_Custom.ReadKey(_Key, _Element.Section).NVL("-1")));
																break;

															default:
																_GammaColors.Add(Convert.ToDouble(_Reader.ReadKey(_Key, _Element.Section).NVL("-1")));
																break;
														}
													}
												}

												//convertir de GammaCorrected -> sRGB -> RGB
												if (IsColor(_GammaColors))
												{
													if (_Element.Key == "x72|y72|z72")
													{

													}
													_Element.Value = ReverseGammaCorrected(_GammaColors).ToArgb();
												}
												else
												{
													//Si la Clave no existe en el Tema elejido, se carga el valor del tema plantilla:
													var _DefaultElement = DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
														Elements.Find(x => x.Key == _Element.Key);
													_Element.Value = _DefaultElement.Value;
												}

												//Guarda los colores usados en el Tema:
												_RecentColors.Add(Color.FromArgb(Util.ValidarNulo(_Element.Value, 0)));

												#endregion
											}
											else //<- ValueType = 'Preset', 'Decimal'
											{
												switch (_Element.File)
												{
													case "Custom":
														_Element.Value = Convert.ToDecimal(_Custom.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
														break;

													default:
														_Element.Value = Convert.ToDecimal(_Reader.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
														break;
												}
											}
											if (_Element.ValueType != "Color" && _Element.Value < 0)
											{
												//Si la Clave no existe en el Tema elejido, se carga el valor del tema plantilla:
												var _DefaultElement = DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
													Elements.Find(x => x.Key == _Element.Key);
												_Element.Value = _DefaultElement.Value;
											}
										}
									}
								}

								if (_ReaderXML != null)
								{
									foreach (var _key in Settings.xml_profile)
									{
										_key.value = Convert.ToDecimal(_ReaderXML.ReadKey(_key.key, "constants").NVL("-1"));
									}
								}

								LoadingTheme = false;
								PreviewTheme(true);
							}
						}

						if (_Reader != null)
						{
							_Reader = null;
						}

						if (_Custom != null)
						{
							_Custom = null;
						}

						if (_ReaderXML != null)
						{
							_ReaderXML = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { Cursor = Cursors.Default; }
		}
		private void LoadTheme_Odissey(ui_preset_new _Theme)
		{
			/* LEE LOS VALORES DEL TEMA ACTUAL Y LOS GUARDA EN EL JSON  'this.Settings'  */
			try
			{
				if (_Stopwatch.ElapsedMilliseconds >= 5000)
				{
					LoadingTheme = false;
					_Stopwatch.Stop();
				}
				if (_Theme != null && !LoadingTheme)
				{
					if (!_Theme.folder.EmptyOrNull() && Directory.Exists(_Theme.folder))
					{
						LoadingTheme = true;
						MainMenu_ApplyTheme.Enabled = false;
						Cursor = Cursors.WaitCursor;

						#region LOAD INI FILES

						IniFile _Reader = new IniFile(Path.Combine(_Theme.folder, @"Startup-Profile.ini"));
						IniFile _ReaderXML = new IniFile(Path.Combine(_Theme.folder, @"XML-Profile.ini"));
						IniFile _ReaderAdvanced = null;
						IniFile _ReaderOnfoot = null;

						if (ActiveInstance.key == "ED_Odissey")
						{
							if (File.Exists(Path.Combine(_Theme.folder, "Advanced.ini")))
							{
								_ReaderAdvanced = new IniFile(Path.Combine(_Theme.folder, @"Advanced.ini"));
							}
							if (!File.Exists(Path.Combine(_Theme.folder, "SuitHud.ini")))
							{
								//Si el archivo no existe lo copia desde las Templates:
								string template_path = Path.Combine(AppExePath, "Data", string.Format("{0}_SuitHud.ini", ActiveInstance.key));
								if (File.Exists(template_path))
								{
									File.Copy(template_path, Path.Combine(_Theme.folder, "SuitHud.ini"), true);
								}
							}
							_ReaderOnfoot = new IniFile(Path.Combine(_Theme.folder, @"SuitHud.ini"));
						}

						#endregion

						if (_Reader != null && _ReaderAdvanced != null)
						{
							Settings.name = _Theme.name;
							Settings.author = _Theme.author;

							if (Settings.ui_groups.IsNotEmpty())
							{
								_RecentColors = new HashSet<Color>();

								//Load HUD Elements:
								foreach (var _group in Settings.ui_groups)
								{
									if (_group.Elements.IsNotEmpty())
									{
										foreach (var _Element in _group.Elements)
										{
											if (_Element.ValueType == "Color")
											{
												#region Es un Color

												if (_Element.Key == "x170|y170|z170")
												{

												}

												List<double> _GammaColors = new List<double>();

												string[] keys = _Element.Key.Split(new char[] { '|' });
												if (keys != null && keys.Length > 0)
												{
													foreach (string _Key in keys)
													{
														switch (_Element.File)
														{
															case "Startup-Profile":
																_GammaColors.Add(Convert.ToDouble(_Reader.ReadKey(_Key, _Element.Section).NVL("-1")));
																break;

															case "Advanced":
																_GammaColors.Add(Convert.ToDouble(_ReaderAdvanced.ReadKey(_Key, _Element.Section).NVL("-1")));
																break;

															case "SuitHud":
																_GammaColors.Add(Convert.ToDouble(_ReaderOnfoot.ReadKey(_Key, _Element.Section).NVL("-1")));
																break;

															default:
																_GammaColors.Add(Convert.ToDouble(_ReaderAdvanced.ReadKey(_Key, _Element.Section).NVL("-1")));
																break;
														}
													}
												}

												//convertir de GammaCorrected -> sRGB -> RGB
												if (IsColor(_GammaColors))
												{
													_Element.Value = ReverseGammaCorrected(_GammaColors).ToArgb();
												}
												else
												{
													//Si la Clave no existe en el Tema elejido, se carga el valor del tema plantilla:
													var _DefaultElement = DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
														Elements.Find(x => x.Key == _Element.Key);
													_Element.Value = _DefaultElement.Value;
												}

												//Guarda los colores usados en el Tema:
												_RecentColors.Add(Color.FromArgb(Util.ValidarNulo(_Element.Value, 0)));

												#endregion
											}
											else  //<- ValueType = 'Preset', 'Decimal'
											{
												switch (_Element.File)
												{
													case "Startup-Profile":
														_Element.Value = Convert.ToDecimal(_Reader.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
														break;

													case "Advanced":
														_Element.Value = Convert.ToDecimal(_ReaderAdvanced.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
														break;

													case "SuitHud":
														_Element.Value = Convert.ToDecimal(_ReaderOnfoot.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
														break;

													default:
														_Element.Value = Convert.ToDecimal(_ReaderAdvanced.ReadKey(_Element.Key, _Element.Section).NVL("-1"));
														break;
												}
												if (_Element.Key == "w120" && _Element.Value == 1.0m)
												{
													_Element.Value = -1; //<- lo pongo negativo para buscarlo en la plantilla aqui abajo
												}
												if (_Element.ValueType != "Color" && _Element.Value < 0)
												{
													if (_Element.Key == "w248")
													{

													}
													//Si la Clave no existe en el Tema elejido, se carga el valor del tema plantilla:
													var _DefaultElement = DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
														Elements.Find(x => x.Key == _Element.Key);
													_Element.Value = _DefaultElement.Value;
												}
											}
										}
									}
								}

								//Load XML Settings:
								if (_ReaderXML != null && Settings.xml_profile != null)
								{
									foreach (var _key in Settings.xml_profile)
									{
										_key.value = Convert.ToDecimal(_ReaderXML.ReadKey(_key.key, "constants").NVL("-1"));
									}
								}

								PreviewTheme(true);
							}
						}
						else
						{
							throw new Exception("ERROR 404: This doesn't seems like an Odyssey theme!");
						}

						if (_Reader != null)
						{
							_Reader = null;
						}

						if (_ReaderAdvanced != null)
						{
							_ReaderAdvanced = null;
						}

						if (_ReaderXML != null)
						{
							_ReaderXML = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				Cursor = Cursors.Default;
				LoadingTheme = false;
				MainMenu_ApplyTheme.Enabled = true;
				_Stopwatch.Stop();
			}
		}

		private void LoadGroupSettings(string pUIGroupName, string SelectRowName = "")
		{
			/* AQUI CARGAMOS TODOS LOS ELEMENTOS DEL GRUPO INDICADO Y LOS MOSTRAMOS EN LA VENTANA DE PROPIEDADES  */
			try
			{
				if (Settings != null && Settings.ui_groups.IsNotEmpty())
				{
					Cursor = Cursors.WaitCursor;
					var t = Task.Factory.StartNew(delegate
					{
						List<ui_translation> _ElementTranslations = _Translations.FindAll(x => x.group == "Element");
						ui_group _UIGroup = Settings.ui_groups.Find(x => x.Name == pUIGroupName);
						if (_UIGroup != null)
						{
							//Cargar las propiedades
							try
							{
								vGridDetalles.BeginUpdate();
								vGridDetalles.Rows.Clear();

								foreach (element _Element in _UIGroup.Elements)
								{
									//Aqui Busca la Traduccion para el Elemento indicado (x Clave) y el Idioma Seleccionado:
									var _TileTrans = _ElementTranslations.FirstOrDefault(x => x.id == _Element.Key);
									if (_TileTrans != null)
									{
										var _Translation = _TileTrans.lang.FirstOrDefault(x => x.key == LangShort);

										_Element.Category = _Translation.category;
										_Element.Title = _Translation.value;
										_Element.Description = _Translation.description;
									}

									EditorRow _Fila = new DevExpress.XtraVerticalGrid.Rows.EditorRow(_Element.Title);
									//Muestra en el ToolTip los detalles del Elemento:
									_Fila.Properties.ToolTip = string.Format("<u><b>{0}</b></u><br><p>{1}<p><br><image={2}>",
										_Element.Title, _Element.Description,
										_Element.Key.NVL("").Replace('|', '_')); //<- La imagen se obiene de 'this.ElementsImgCollection' usando la 'key' del elemento
									_Fila.Properties.Caption = _Element.Title;
									_Fila.Properties.FieldName = "Value";
									_Fila.Tag = _Element;

									//Valor x defecto para el brillo del texto en horizons:
									if (ActiveInstance.key == "ED_Horizons" && _Element.Key == "w151")
									{
										if (_Element.Value <= 0)
										{
											_Element.Value = 1; //
										}
									}
									//Valor x defecto para el brillo de Orbit Lines en horizons:
									if (ActiveInstance.key == "ED_Horizons" && _Element.Key == "y117")
									{
										if (_Element.Value <= 0)
										{
											_Element.Value = 1;
										}
									}

									switch (_Element.ValueType)
									{
										case "Brightness":
											#region Mostrar una TrackBar

											//"ValueType": "Brightness",
											//"Type": "2X", <- 1X, 2X, 4X, 4X_Min

											Invoke((MethodInvoker)(() =>
											{
												RepositoryItemTrackBar _ComboValue = new RepositoryItemTrackBar
												{
													EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
													Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title),
													EditValueChangedDelay = 500,
													ShowValueToolTip = true,
													Tag = _Element
												};
												_ComboValue.ValueChanged += _ComboValue_EditValueChanged;

												switch (_Element.Type)
												{
													case "1X":
														_ComboValue.Maximum = 10;
														_ComboValue.Minimum = 0;
														break;

													case "2X":
														_ComboValue.Maximum = 20;
														_ComboValue.Minimum = 0;
														break;

													case "4X": //eXPANDED
														_ComboValue.Maximum = 40;
														_ComboValue.Minimum = 0;
														break;

													case "4X_Min": //sHADED
														_ComboValue.Maximum = 40;
														_ComboValue.Minimum = -40;
														break;

													default:
														_ComboValue.Maximum = 20;
														_ComboValue.Minimum = 0;
														break;
												}

												_Fila.Properties.RowEdit = _ComboValue;
												_Fila.Properties.Value = _Element.Value * 10;
											}));

											#endregion
											break;

										case "Preset":
											#region Mostrar un Combo con los Presets

											//"ValueType": "Preset",
											//"Type": "AdvancedMode", <- "{Preset_Name}"

											if (_ElementPresets.IsNotEmpty())
											{
												List<combo_item> _Presets = _ElementPresets.FindAll(x => x.Type == _Element.Type);
												if (_Presets.IsNotEmpty())
												{
													Invoke((MethodInvoker)(() =>
													{
														RepositoryItemLookUpEdit _ComboPreset = new RepositoryItemLookUpEdit()
														{
															Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title),
															DisplayMember = "Name",
															ValueMember = "Index",
															DataSource = _Presets,
															AllowFocused = true,
															Tag = _Element
														};
														_ComboPreset.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name"));
														_ComboPreset.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Index"));
														_ComboPreset.EditValueChanged += _ComboValue_EditValueChanged;
														_ComboPreset.AppearanceFocused.ForeColor = Color.Orange;
														_ComboPreset.BestFit();

														_Fila.Properties.RowEdit = _ComboPreset;
														_Fila.Properties.Value = _Element.Value;
													}));
												}
											}

											#endregion
											break;

										case "Color":
											#region Muestra un selector de Color

											//"ValueType": "Color",
											//"Type": "CustomColor",

											Invoke((MethodInvoker)(() =>
											{
												//RepositoryItemColorEdit _ComboColor = new RepositoryItemColorEdit()
												//{
												//	Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title),
												//	ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced,
												//	ShowMoreColorsButton = true,
												//	ShowCustomColors = true,
												//	ShowSystemColors = false,
												//	Tag = _Element
												//};
												RepositoryItemMyColorPickEdit _ComboColor = new RepositoryItemMyColorPickEdit
												{
													Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title),
													ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Simple,
													AutomaticColor = Color.Orange,
													ShowMoreColorsButton = true,
													ShowColorDialog = true,
													ShowWebColors = false,
													ShowCustomColors = true,
													ShowSystemColors = false,
													ShowWebSafeColors = false,
													ShowMyCustomColors = true,
													ShowMyPastelColors = false,
													Tag = _Element
												};
												//_ComboColor.EditValueChanged += _ComboValue_EditValueChanged;
												List<Color> _GColors = null;
												var _StandardColors = _ComboColor.MyStandardColors;

												_ComboColor.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
												_ComboColor.EditValueChangedDelay = 500;
												_ComboColor.ColorDialogOptions.ShowTabs = ShowTabs.RGBModel;
												_ComboColor.ColorDialogOptions.AllowTransparency = true;
												_ComboColor.ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced;
												_ComboColor.EditValueChanged += (object Sender, EventArgs E) =>
												{
													//Crea un Gradiente a Blanco usando el color seleccionado, lo pone en la ultima columna
													Color _E = (Sender as Util_Test.MyColorPickEdit).Color;
													var _CustomColors = _ComboColor.MyStandardColors;

													_GColors = Util.GetColorGradients(_E, Color.Black, 7).ToList();

													_CustomColors[0, 9] = _GColors[0];
													_CustomColors[0, 19] = _GColors[1];
													_CustomColors[0, 29] = _GColors[2];
													_CustomColors[0, 39] = _GColors[3];
													_CustomColors[0, 49] = _GColors[4];
													_CustomColors[0, 59] = _GColors[5];

													_GColors = Util.GetColorGradients(_E, Color.White, 7).ToList();

													_CustomColors[0, 8] = _GColors[0];
													_CustomColors[0, 18] = _GColors[1];
													_CustomColors[0, 28] = _GColors[2];
													_CustomColors[0, 38] = _GColors[3];
													_CustomColors[0, 48] = _GColors[4];
													_CustomColors[0, 58] = _GColors[5];

													_ComboValue_EditValueChanged(Sender, E);
												};

												//-------- LOAD THE THEME'S COLOR PALETTE 
												if (_RecentColors != null)
												{
													List<Color> _RecColors = _RecentColors.ToList();
													_RecColors.Sort(new XColorComparer());

													var themeColors = _ComboColor.ThemeColors;
													try
													{
														int i = 0;
														for (int _RowIndex = 0; _RowIndex < themeColors.RowCount; _RowIndex++) //<-Rows
														{
															for (int _ColumnIndex = 0; _ColumnIndex < themeColors.ColumnCount; _ColumnIndex++) //<-Columns
															{
																themeColors[_RowIndex, _ColumnIndex] = _RecColors[i]; // Color.FromArgb(0, 255, 0);
																i++;
																//green = green - greenDelta;
															}
														}
													}
													catch { }
												}

												//---- CAMBIA LA LISTA DE COLORES STANDARD --------------------
												//var _StandardColors = _ComboColor.StandardColors;
												//_StandardColors[0, 0] = Color.Black; 
												//_StandardColors[0, 1] = Color.White; 
												//_StandardColors[0, 2] = Color.Gray;
												//_StandardColors[0, 3] = Color.Red;
												//_StandardColors[0, 4] = Color.Orange; // Color.FromArgb(255, 85, 0);     //<- Orange
												//_StandardColors[0, 5] = Color.Yellow;
												//_StandardColors[0, 6] = Color.Green;
												//_StandardColors[0, 7] = Color.Cyan;
												//_StandardColors[0, 8] = Color.Blue;
												//_StandardColors[0, 9] = Color.Purple;


												//----- ASIGNA EL VALOR DEL CONTROL
												if (DefaultSettings != null && _Element.Value == 0)
												{
													//SI NO ENCUENTRA LA CLAVE EN EL TEMA, CARGA VALOR X DEFECTO
													var _Grupo = DefaultSettings.ui_groups.Find(x => x.Name == _UIGroup.Name);
													if (_Grupo != null)
													{
														var _Default = _Grupo.Elements.Find(x => x.Key == _Element.Key);
														if (_Default != null)
														{
															_Fila.Properties.Value = Color.FromArgb((int)_Default.Value);
														}
													}
												}
												else
												{
													//- SET THE VALUE FROM THE THEME
													_Fila.Properties.Value = Color.FromArgb(Util.ValidarNulo(_Element.Value, 0));
												}
												_ComboColor.AutomaticColor = (Color)_Fila.Properties.Value;

												//----Crea un Gradiente a Blanco usando el color seleccionado, lo pone en la ultima columna												
												_GColors = Util.GetColorGradients((Color)_Fila.Properties.Value, Color.Black, 7).ToList();

												_StandardColors[0, 9] = _GColors[0];
												_StandardColors[0, 19] = _GColors[1];
												_StandardColors[0, 29] = _GColors[2];
												_StandardColors[0, 39] = _GColors[3];
												_StandardColors[0, 49] = _GColors[4];
												_StandardColors[0, 59] = _GColors[5];

												_GColors = Util.GetColorGradients((Color)_Fila.Properties.Value, Color.White, 7).ToList();

												_StandardColors[0, 8] = _GColors[0];
												_StandardColors[0, 18] = _GColors[1];
												_StandardColors[0, 28] = _GColors[2];
												_StandardColors[0, 38] = _GColors[3];
												_StandardColors[0, 48] = _GColors[4];
												_StandardColors[0, 58] = _GColors[5];

												//--- SET THE COLORPICKER INTO THE PROPERTY GRID
												_Fila.Properties.RowEdit = _ComboColor;
											}));

											#endregion
											break;

										case "ONOFF":
											#region Muestra un Toggle Switch:

											//"ValueType": "ONOFF",
											//"Type": "ONOFF",
											//"Value": 0,  <- 0=Off, 1=On

											Invoke((MethodInvoker)(() =>
											{
												RepositoryItemToggleSwitch _ToggleControl = new RepositoryItemToggleSwitch();
												_ToggleControl.Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title);
												_ToggleControl.EditValueChanged += _ComboValue_EditValueChanged;
												_ToggleControl.EditValueChangedDelay = 500;
												_ToggleControl.Tag = _Element;

												_Fila.Properties.RowEdit = _ToggleControl;
												_Fila.Properties.Value = Util.IntegerToBool(Util.ValidarNulo(_Element.Value, 0));
											}));

											#endregion
											break;

										case "Number":
											#region Muestra un SpinEdit Numerico

											//"ValueType": "Number",
											//"Type": "INT|-10|1000|n0",   <- Type|Min|Max|Mask
											//"Type": "DEC|0.0|1.0|n2",

											RepositoryItemSpinEdit _NumberItem = new RepositoryItemSpinEdit()
											{
												Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title),
												EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
												EditValueChangedDelay = 500,
												AllowMouseWheel = true,
												AllowFocused = true,
												Tag = _Element
											};
											_NumberItem.EditValueChanged += _ComboValue_EditValueChanged;
											_NumberItem.AppearanceFocused.ForeColor = Color.Orange;
											if (!_Element.Type.EmptyOrNull())
											{
												string[] keys = _Element.Type.Split(new char[] { '|' });
												if (keys != null && keys.Length > 0)
												{
													_NumberItem.IsFloatValue = (keys[0] == "INT" ? false : true);
													_NumberItem.MinValue = Convert.ToDecimal(keys[1]);
													_NumberItem.MaxValue = Convert.ToDecimal(keys[2]);
													_NumberItem.Increment = (keys[0] == "INT" ? 1.0m : 0.01m);
													_NumberItem.EditMask = keys[3];
													_NumberItem.UseMaskAsDisplayFormat = true;
												}
											}

											_Fila.Properties.RowEdit = _NumberItem;
											_Fila.Properties.Value = _Element.Value;

											#endregion
											break;

										default:
											#region Muestra un TextBox

											RepositoryItemTextEdit _TextItem = new RepositoryItemTextEdit()
											{
												Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title),
												EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
												EditValueChangedDelay = 500,
												AllowFocused = true,
												Tag = _Element
											};
											_TextItem.EditValueChanged += _ComboValue_EditValueChanged;
											_TextItem.AppearanceFocused.ForeColor = Color.Orange;

											_Fila.Properties.RowEdit = _TextItem;
											_Fila.Properties.Value = _Element.Value;

											#endregion
											break;
									}

									#region Categorias

									//Agrega la Categoria del Elemento, si ya existe, usa esa, sino, la crea nueva:
									if (!_Element.Category.EmptyOrNull())
									{
										//_Translation.value = _Element.Category

										CategoryRow Categoria = vGridDetalles.Rows[_Element.Category] as CategoryRow;
										if (Categoria == null)
										{
											Categoria = new CategoryRow(_Element.Category)
											{
												Name = _Element.Category
											};
											vGridDetalles.Rows.Add(Categoria);
										}
										Categoria.ChildRows.Add(_Fila);
									}
									else
									{
										vGridDetalles.Rows.Add(_Fila);
									}

									if (_Fila != null && _Element.Title == SelectRowName)
									{
										Invoke((MethodInvoker)(() =>
										{
											vGridDetalles.FocusedRow = _Fila;
										}));
									}

									#endregion
								}
							}
							catch (Exception ex)
							{
								XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}

							Invoke((MethodInvoker)(() =>
							{
								vGridDetalles.EndUpdate();
								//this.vGridDetalles.BestFit();

								dockDetalles.Text = _UIGroup.Title;
								dockManager1.ActivePanel = dockDetalles;
								Cursor = Cursors.Default;
							}));
						}
						else
						{
							Invoke((MethodInvoker)(() =>
							{
								vGridDetalles.Rows.Clear();

								dockDetalles.Text = string.Empty;
								dockManager1.ActivePanel = dockDetalles;
								Cursor = Cursors.Default;
							}));

						}
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void PreviewTheme(bool XMLchanged = false)
		{
			//TODO: De momento solo Horizons tiene Preview
			if (ActiveInstance.key == "ED_Horizons")
			{
				if (PreviewForm == null || !PreviewForm.Visible)
				{
					XMLchanged = true;
					PreviewForm = new PreviewForm(ActiveInstance);
					PreviewForm.Show();
					PreviewForm.OnPreviewLoaded += PreviewForm_OnPreviewLoaded;
				}
				PreviewForm.Settings = Settings;
				PreviewForm.PreviewTheme(XMLchanged);
			}
			else
			{
				//if (this.PreviewFormODY == null || !this.PreviewFormODY.Visible)
				//{
				//	XMLchanged = true;
				//	this.PreviewFormODY = new PreviewOdysseyForm(this.ActiveInstance);
				//	this.PreviewFormODY.Show();
				//	this.PreviewFormODY.OnPreviewLoaded += PreviewForm_OnPreviewLoaded;
				//}
				//this.PreviewFormODY.Settings = this.Settings;
				//this.PreviewFormODY.PreviewTheme(XMLchanged);
			}
		}
		private void PreviewForm_OnPreviewLoaded(object sender, EventArgs e)
		{
			//sI EL TEMA NO TIENE PREVIEW, le creamos una usando el Preview;
			if (PreviewForm != null && SelectedTheme != null && !SelectedTheme.HasPreview)
			{
				SelectedTheme.Preview = PreviewForm.GetPreviewThumbnail();
				SelectedTheme.HasPreview = true;
				SelectedTheme.Preview.Save(Path.Combine(SelectedTheme.folder, "PREVIEW.jpg"));
			}
		}

		private string CreateNewTheme()
		{
			string _ret = string.Empty;
			try
			{
				if (XtraMessageBox.Show("This will Create a New Profile using the Current Settings, would you like to Continue?",
					"Create New Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					Cursor = Cursors.WaitCursor;
					if (SelectedTheme.name == "Current Settings")
					{
						SelectedTheme.name = "MyTheme";
					}

					ThemeParametersForm _Form = new ThemeParametersForm
					{
						ThisIsAMod = false,
						ModName = SelectedTheme.name,
						ThemeName = SelectedTheme.name,
						Author = SelectedTheme.author,
						Description = SelectedTheme.description,
						Thumbnail = SelectedTheme.Preview
					};
					if (_Form.ShowDialog() == DialogResult.OK)
					{
						SelectedTheme.name = _Form.ThemeName;
						SelectedTheme.author = _Form.Author;
						SelectedTheme.description = _Form.Description;
						SelectedTheme.Preview = _Form.Thumbnail;

						string GameFolder = Path.Combine(ActiveInstance.path, @"EDHM-ini");
						string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
						string NewProfileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
												   @"Elite Dangerous\EDHM_UI", search, "Themes", SelectedTheme.name);
						SelectedTheme.folder = NewProfileFolder;

						//Agregar el Identificador del Autor:
						theme_details ThemeDetails = new theme_details
						{
							author = SelectedTheme.author,
							theme = SelectedTheme.name,
							description = string.Format("** THIS THEME WAS MADE BY {0} **", _Form.Author.ToUpper()),
							preview = string.Empty
						};

						//1. Crear la Carpeta para el Nuevo Perfil, si ya Existe, se Sobreescribe:
						var _ProfileDir = System.IO.Directory.CreateDirectory(NewProfileFolder);
						if (_ProfileDir != null)
						{
							//Cuando se termina de Aplicar el tema se produce este evento:
							OnThemeApply += (object _Sender, EventArgs _E) =>
							{
								//Copiar los Archivos del Tema Actual: // existing files will be overwritten
								Util.Serialize_ToJSON(Path.Combine(NewProfileFolder, string.Format("{0}.credits", _Form.Author)), ThemeDetails);

								File.Copy(Path.Combine(GameFolder, @"Startup-Profile.ini"),
									Path.Combine(NewProfileFolder, @"Startup-Profile.ini"), true);

								File.Copy(Path.Combine(GameFolder, "XML-Profile.ini"),
									Path.Combine(NewProfileFolder, @"XML-Profile.ini"), true);

								if (ActiveInstance.key == "ED_Odissey")
								{
									if (File.Exists(Path.Combine(GameFolder, @"Advanced.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"Advanced.ini"),
										Path.Combine(NewProfileFolder, @"Advanced.ini"), true);
									}
									if (File.Exists(Path.Combine(GameFolder, @"SuitHud.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"SuitHud.ini"),
										Path.Combine(NewProfileFolder, @"SuitHud.ini"), true);
									}
								}

								// Agregar una Imagen de Thumbnail :
								if (SelectedTheme.Preview != null)
								{
									//Save the Image as JPG:
									SelectedTheme.Preview.Save(Path.Combine(NewProfileFolder, "PREVIEW.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
								}

								LoadThemeList_EX();
								_ret = NewProfileFolder; //<- Devuelve la Ruta del Nuevo tema

								//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 2 SEGUNDOS:
								XtraMessageBoxArgs args = new XtraMessageBoxArgs()
								{
									Caption = "Done",
									Text = string.Format("The theme '{0}' has successfully been Created.", SelectedTheme.name),
									Buttons = new DialogResult[] { DialogResult.OK }
								};
								args.AutoCloseOptions.Delay = 3000;
								args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
								XtraMessageBox.Show(args).ToString();
							};
							ApplyTheme(false, true); //<- Aplica los cambios Actuales  
						}
					}

					//Comentar todo aqui abajo
					/*
					string _ProfileName = XtraInputBox.Show("Type a Name for your Custom Theme:\r\n** If Exists, it will be Overwritten!",
						"Create New Theme", SelectedTheme.name);

					string _AuthorName = XtraInputBox.Show("Who's the Author of this theme?",
						"Create New Theme", SelectedTheme.author);

					if (!_ProfileName.EmptyOrNull())
					{
						SelectedTheme.name = _ProfileName;
						SelectedTheme.author = _AuthorName;

						string GameFolder = Path.Combine(ActiveInstance.path, @"EDHM-ini");
						string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
						string NewProfileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
												   @"Elite Dangerous\EDHM_UI", search, "Themes", _ProfileName);
						SelectedTheme.folder = NewProfileFolder;

						//1. Crear la Carpeta para el Nuevo Perfil, si ya Existe, se Sobreescribe:
						var _ProfileDir = System.IO.Directory.CreateDirectory(NewProfileFolder);
						if (_ProfileDir != null)
						{
							//Cuando se termina de Aplicar el tema se produce este evento:
							OnThemeApply += (object _Sender, EventArgs _E) =>
							{
								//Copiar los Archivos del Tema Actual: // existing files will be overwritten
								File.Copy(Path.Combine(GameFolder, @"Startup-Profile.ini"),
									Path.Combine(NewProfileFolder, @"Startup-Profile.ini"), true);

								File.Copy(Path.Combine(GameFolder, "XML-Profile.ini"),
									Path.Combine(NewProfileFolder, @"XML-Profile.ini"), true);

								if (ActiveInstance.key == "ED_Odissey")
								{
									if (File.Exists(Path.Combine(GameFolder, @"Advanced.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"Advanced.ini"),
										Path.Combine(NewProfileFolder, @"Advanced.ini"), true);
									}
									if (File.Exists(Path.Combine(GameFolder, @"SuitHud.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"SuitHud.ini"),
										Path.Combine(NewProfileFolder, @"SuitHud.ini"), true);
									}
								}

								//Agregar el Identificador del Autor:
								theme_details ThemeDetails = new theme_details
								{
									author = _AuthorName,
									theme = _ProfileName,
									description = string.Format("** THIS THEME WAS MADE BY {0} **", _AuthorName.ToUpper()),
									preview = string.Empty
								};
								Util.Serialize_ToJSON(Path.Combine(NewProfileFolder, string.Format("{0}.credits", _AuthorName)), ThemeDetails);
								//Util.SaveTextFile(Path.Combine(NewProfileFolder, string.Format("{0}.credits", _AuthorName)),
								//	string.Format("** THIS THEME WAS MADE BY {0} **", _AuthorName.ToUpper()),
								//	Util.TextEncoding.UTF8);

								// Agregar una Imagen de Preview :
								if (ActiveInstance.key == "ED_Horizons")
								{
									Image _Preview = PreviewForm.GetPreviewThumbnail();
									Bitmap bitmapImage = new Bitmap(_Preview);
									bitmapImage.Save(Path.Combine(NewProfileFolder, "PREVIEW.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
								}

								LoadThemeList_EX();
								_ret = NewProfileFolder; //<- Devuelve la Ruta del Nuevo tema

								//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 2 SEGUNDOS:
								XtraMessageBoxArgs args = new XtraMessageBoxArgs()
								{
									Caption = "Done",
									Text = string.Format("The theme '{0}' has successfully been Created.", SelectedTheme.name),
									Buttons = new DialogResult[] { DialogResult.OK }
								};
								args.AutoCloseOptions.Delay = 3000;
								args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
								XtraMessageBox.Show(args).ToString();
							};
							ApplyTheme(false, true); //<- Aplica los cambios Actuales  
						}
					}*/
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { Cursor = Cursors.Default; }
			return _ret;
		}
		private string CreateNewThemeSync()
		{
			string _ret = string.Empty;
			try
			{
				if (XtraMessageBox.Show("This will Create a New Profile using the Current Settings, would you like to Continue?",
					"Create New Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					Cursor = Cursors.WaitCursor;
					if (SelectedTheme.name == "Current Settings")
					{
						SelectedTheme.name = "MyTheme";
					}

					ThemeParametersForm _Form = new ThemeParametersForm
					{
						ThisIsAMod = false,
						ModName = SelectedTheme.name,
						ThemeName = SelectedTheme.name,
						Author = SelectedTheme.author,
						Description = SelectedTheme.description,
						Thumbnail = SelectedTheme.Preview
					};
					if (_Form.ShowDialog() == DialogResult.OK)
					{
						SelectedTheme.name = _Form.ThemeName;
						SelectedTheme.author = _Form.Author;
						SelectedTheme.description = _Form.Description;
						SelectedTheme.Preview = _Form.Thumbnail;

						string GameFolder = Path.Combine(ActiveInstance.path, @"EDHM-ini");
						string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
						string NewProfileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
												   @"Elite Dangerous\EDHM_UI", search, "Themes", SelectedTheme.name);

						SelectedTheme.folder = NewProfileFolder;
						_ret = NewProfileFolder; //<- Devuelve la Ruta del Nuevo tema

						//Agregar el Identificador del Autor:
						theme_details ThemeDetails = new theme_details
						{
							author = SelectedTheme.author,
							theme = SelectedTheme.name,
							description = string.Format("** THIS THEME WAS MADE BY {0} **", _Form.Author.ToUpper()),
							preview = string.Empty
						};

						//1. Crear la Carpeta para el Nuevo Perfil, si ya Existe, se Sobreescribe:
						var _ProfileDir = System.IO.Directory.CreateDirectory(NewProfileFolder);
						if (_ProfileDir != null)
						{
							//Cuando se termina de Aplicar el tema se produce este evento:
							OnThemeApply += (object _Sender, EventArgs _E) =>
							{
								//Copiar los Archivos del Tema Actual: // existing files will be overwritten
								Util.Serialize_ToJSON(Path.Combine(NewProfileFolder, string.Format("{0}.credits", _Form.Author)), ThemeDetails);

								File.Copy(Path.Combine(GameFolder, @"Startup-Profile.ini"),
									Path.Combine(NewProfileFolder, @"Startup-Profile.ini"), true);

								File.Copy(Path.Combine(GameFolder, "XML-Profile.ini"),
									Path.Combine(NewProfileFolder, @"XML-Profile.ini"), true);

								if (ActiveInstance.key == "ED_Odissey")
								{
									if (File.Exists(Path.Combine(GameFolder, @"Advanced.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"Advanced.ini"),
										Path.Combine(NewProfileFolder, @"Advanced.ini"), true);
									}
									if (File.Exists(Path.Combine(GameFolder, @"SuitHud.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"SuitHud.ini"),
										Path.Combine(NewProfileFolder, @"SuitHud.ini"), true);
									}
								}

								// Agregar una Imagen de Thumbnail :
								if (SelectedTheme.Preview != null)
								{
									//Save the Image as JPG:
									SelectedTheme.Preview.Save(Path.Combine(NewProfileFolder, "PREVIEW.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
								}

								LoadThemeList_EX();
								_ret = NewProfileFolder; //<- Devuelve la Ruta del Nuevo tema

								//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 2 SEGUNDOS:
								XtraMessageBoxArgs args = new XtraMessageBoxArgs()
								{
									Caption = "Done",
									Text = string.Format("The theme '{0}' has successfully been Created.", SelectedTheme.name),
									Buttons = new DialogResult[] { DialogResult.OK }
								};
								args.AutoCloseOptions.Delay = 3000;
								args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
								XtraMessageBox.Show(args).ToString();
							};
							ApplyTheme(false, true); //<- Aplica los cambios Actuales  
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { Cursor = Cursors.Default; }
			return _ret;
		}

		private void CreateThemeKeyBinding(string _ProfileName)
		{
			try
			{
				KeyBindingSimpleForm _KeyForm = new KeyBindingSimpleForm(_ProfileName);
				if (_KeyForm.ShowDialog() == DialogResult.OK)
				{
					string GameFolder = Path.Combine(ActiveInstance.path, "EDHM-ini");
					string MyProfilesINI = Util.ReadTextFile(Path.Combine(GameFolder, @"MyProfiles\MyProfiles.ini"), Util.TextEncoding.UTF8);

					IniFile _Reader = new IniFile(Path.Combine(ActiveInstance.path, "XML-Profile.ini"));

					StringBuilder _Profile = new StringBuilder();
					_Profile.AppendLine(string.Format("[keyToggle-{0}]", _ProfileName.Replace(' ', '-')));
					_Profile.AppendLine(string.Format("Key = {0}", _KeyForm.KeyBinding));

					if (Settings.ui_groups.IsNotEmpty())
					{
						//Aqui Grabamos todos los Elelementos:
						foreach (var _group in Settings.ui_groups)
						{
							if (_group.Elements.IsNotEmpty())
							{
								foreach (var _Element in _group.Elements)
								{
									_Profile.AppendLine(string.Format(";{0}", _Element.Title));
									_Profile.AppendLine(string.Format("{0} = {1}", _Element.Key, _Element.Value.ToString()));
								}
							}
						}
					}

					//Grabar tambien el XML con la Matriz de color:
					if (_Reader != null)
					{
						_Profile.AppendLine("; == Station XML ==");
						_Profile.AppendLine("; Top Row<Matrix Red>");
						_Profile.AppendLine(string.Format("x158 = {0}", _Reader.ReadKey("x158", "constants")));
						_Profile.AppendLine(string.Format("y158 = {0}", _Reader.ReadKey("y158", "constants")));
						_Profile.AppendLine(string.Format("z158 = {0}", _Reader.ReadKey("z158", "constants")));

						_Profile.AppendLine("; Middle Row <Matrix Green>");
						_Profile.AppendLine(string.Format("x159 = {0}", _Reader.ReadKey("x159", "constants")));
						_Profile.AppendLine(string.Format("y159 = {0}", _Reader.ReadKey("y159", "constants")));
						_Profile.AppendLine(string.Format("z159 = {0}", _Reader.ReadKey("z159", "constants")));

						_Profile.AppendLine("; Bottom Row <Matrix Blue>");
						_Profile.AppendLine(string.Format("x160 = {0}", _Reader.ReadKey("x160", "constants")));
						_Profile.AppendLine(string.Format("y160 = {0}", _Reader.ReadKey("y160", "constants")));
						_Profile.AppendLine(string.Format("z160 = {0}", _Reader.ReadKey("z160", "constants")));

						_Profile.AppendLine("; == Portrait Fix ==");
						_Profile.AppendLine("; Top Row <Matrix Red>");
						_Profile.AppendLine(string.Format("x153 = {0}", _Reader.ReadKey("x153", "constants")));
						_Profile.AppendLine(string.Format("y153 = {0}", _Reader.ReadKey("y153", "constants")));
						_Profile.AppendLine(string.Format("z153 = {0}", _Reader.ReadKey("z153", "constants")));

						_Profile.AppendLine("; Middle Row <Matrix Green>");
						_Profile.AppendLine(string.Format("x154 = {0}", _Reader.ReadKey("x154", "constants")));
						_Profile.AppendLine(string.Format("y154 = {0}", _Reader.ReadKey("y154", "constants")));
						_Profile.AppendLine(string.Format("z154 = {0}", _Reader.ReadKey("z154", "constants")));

						_Profile.AppendLine("; Bottom Row <Matrix Blue>");
						_Profile.AppendLine(string.Format("x155 = {0}", _Reader.ReadKey("x155", "constants")));
						_Profile.AppendLine(string.Format("y155 = {0}", _Reader.ReadKey("y155", "constants")));
						_Profile.AppendLine(string.Format("z155 = {0}", _Reader.ReadKey("z155", "constants")));

						_Profile.AppendLine("; == Ship XML ==");
						_Profile.AppendLine("; Top Row <Matrix Red>");
						_Profile.AppendLine(string.Format("x150 = {0}", _Reader.ReadKey("x150", "constants")));
						_Profile.AppendLine(string.Format("y150 = {0}", _Reader.ReadKey("y150", "constants")));
						_Profile.AppendLine(string.Format("z150 = {0}", _Reader.ReadKey("z150", "constants")));

						_Profile.AppendLine("; Middle Row <Matrix Green>");
						_Profile.AppendLine(string.Format("x151 = {0}", _Reader.ReadKey("x151", "constants")));
						_Profile.AppendLine(string.Format("y151 = {0}", _Reader.ReadKey("y151", "constants")));
						_Profile.AppendLine(string.Format("z151 = {0}", _Reader.ReadKey("z151", "constants")));

						_Profile.AppendLine("; Bottom Row <Matrix Blue>");
						_Profile.AppendLine(string.Format("x152 = {0}", _Reader.ReadKey("x152", "constants")));
						_Profile.AppendLine(string.Format("y152 = {0}", _Reader.ReadKey("y152", "constants")));
						_Profile.AppendLine(string.Format("z152 = {0}", _Reader.ReadKey("z152", "constants")));
					}

					//Guardo los cambios en el Archivo:
					string NewProfileFolder = Path.Combine(ActiveInstance.path, ActiveInstance.themes_folder, _ProfileName);
					Util.SaveTextFile(Path.Combine(NewProfileFolder, "Hotkey-Profile.txt"), _Profile.ToString(), Util.TextEncoding.UTF8);

					//Añado los datos del nuevo perfil al final del archivo:
					MyProfilesINI = string.Format("{0}\r\n{1}", MyProfilesINI, _Profile.ToString());
					Util.SaveTextFile(Path.Combine(GameFolder, @"MyProfiles\MyProfiles.ini"), MyProfilesINI, Util.TextEncoding.UTF8);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ApplyTheme(bool SaveIt = true, bool KeepItQuiet = false, bool Async = true)
		{
			Cursor = Cursors.WaitCursor;
			MainMenu_ApplyTheme.Enabled = false;
			gridControl1.Cursor = Cursors.WaitCursor;

			if (Async)
			{
				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
					ApplyThemeSync(SaveIt, KeepItQuiet);
				});
			}
			else
			{
				ApplyThemeSync(SaveIt, KeepItQuiet);
			}
		}
		private void ApplyThemeSync(bool SaveIt = true, bool KeepItQuiet = false)
		{
			#region COPY THE TEMPLATE FILES

			if (File.Exists(Path.Combine(AppExePath, string.Format(@"Data\{0}_Startup-Profile.ini", ActiveInstance.key))))
			{
				File.Copy(Path.Combine(AppExePath, string.Format(@"Data\{0}_Startup-Profile.ini", ActiveInstance.key)),
					Path.Combine(ActiveInstance.path, @"EDHM-ini\Startup-Profile.ini"), true);
			}
			if (File.Exists(Path.Combine(AppExePath, string.Format(@"Data\{0}_XML-Profile.ini", ActiveInstance.key))))
			{
				File.Copy(Path.Combine(AppExePath, string.Format(@"Data\{0}_XML-Profile.ini", ActiveInstance.key)),
					Path.Combine(ActiveInstance.path, @"EDHM-ini\XML-Profile.ini"), true);
			}
			if (ActiveInstance.key == "ED_Odissey")
			{
				if (File.Exists(Path.Combine(AppExePath, string.Format(@"Data\{0}_Advanced.ini", ActiveInstance.key))))
				{
					File.Copy(Path.Combine(AppExePath, string.Format(@"Data\{0}_Advanced.ini", ActiveInstance.key)),
						Path.Combine(ActiveInstance.path, @"EDHM-ini\Advanced.ini"), true);
				}
				if (File.Exists(Path.Combine(AppExePath, string.Format(@"Data\{0}_SuitHud.ini", ActiveInstance.key))))
				{
					File.Copy(Path.Combine(AppExePath, string.Format(@"Data\{0}_SuitHud.ini", ActiveInstance.key)),
						Path.Combine(ActiveInstance.path, @"EDHM-ini\SuitHud.ini"), true);
				}
			}

			#endregion

			/*   APPLY THE THEME INTO THE GAME FILES      */
			ApplyTheme_Files(KeepItQuiet);

			if (SaveIt)
			{
				SaveTheme(true); //<- Save the changes in the JSON
			}

			//Save the Settings into the History:
			History_AddSettings(Settings);
			History_LoadElements(SavesToRemember);

			if (SelectedTheme != null)
			{
				//Select the Applied theme into the Theme List
				int rowHandle = gridView1.LocateByValue("name", SelectedTheme.name);
				if (rowHandle != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
				{
					gridView1.FocusedRowHandle = rowHandle;
				}
			}
			if (AutoApplyTheme)
			{
				//If set and Game is running, attepms to send the F11 key to refresh the colors in game:
				Process[] targetProcess = Process.GetProcessesByName("EliteDangerous64");
				if (targetProcess.Length > 0)
				{
					IntPtr h = targetProcess[0].MainWindowHandle;
					SetForegroundWindow(h);
					SendKeys.SendWait("{F11}");
				}
			}
		}
		private void ApplyTheme_Files(bool KeepItQuiet = false)
		{
			//Save theme changes into the game files
			try
			{
				if (!LoadingTheme)
				{
					if (!ActiveInstance.path.EmptyOrNull())
					{
						LoadingTheme = true;

						IniFile _Reader = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\Startup-Profile.ini")); //<- Open and Read the INI file			
						IniFile _ReaderXML = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\XML-Profile.ini"));
						IniFile _ReaderAdvanced = null;
						IniFile _ReaderOnfoot = null;

						IniFile _Custom = null;

						if (ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(ActiveInstance.path, @"EDHM-ini\Advanced.ini")))
						{
							_ReaderAdvanced = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\Advanced.ini"));
						}

						if (ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(ActiveInstance.path, @"EDHM-ini\SuitHud.ini")))
						{
							_ReaderOnfoot = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\SuitHud.ini"));
						}

						if (ActiveInstance.key == "ED_Horizons" && File.Exists(Path.Combine(ActiveInstance.path, @"EDHM-ini\Custom.ini")))
						{
							_Custom = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\Custom.ini"));
						}

						IniFile _CPM_Interior = null;
						if (CurrentShip != null && ActiveInstance.key == "ED_Odissey" &&
							File.Exists(Path.Combine(ActiveInstance.path, @"EDHM-ini\3rdPartyMods\CPM-@Cockpit-Paint-Mod.ini")))
						{
							_CPM_Interior = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\3rdPartyMods\CPM-@Cockpit-Paint-Mod.ini"));
							_CPM_Interior.WriteKey("w158", CurrentShip.ship_id.ToString(), "constants");
						}

						if (Settings.ui_groups.IsNotEmpty())
						{
							foreach (var _group in Settings.ui_groups)
							{
								if (_group.Elements.IsNotEmpty())
								{
									foreach (var _Element in _group.Elements)
									{
										//Ahora se guarda el Valor en los INIs correspondientes:
										if (_Element.ValueType == "Color")
										{
											string[] keys = _Element.Key.Split(new char[] { '|' });
											if (keys != null && keys.Length > 0)
											{
												//Convertir Color RGB -> sRGB -> GammaCorrected
												Color _Color = Color.FromArgb(Convert.ToInt32(_Element.Value));
												List<double> _GammaColors = GetGammaCorrected_RGBA(_Color);

												int i = 0;
												foreach (string _Key in keys)
												{
													switch (_Element.File)
													{
														case "Advanced": _ReaderAdvanced.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;
														case "SuitHud": _ReaderOnfoot.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;
														case "Custom": _Custom.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;
														default: _Reader.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;
													}
													i++;
												}
											}
										}
										else //<- No es un Color <- ValueType = 'Preset', 'Decimal'
										{
											switch (_Element.File)
											{
												case "Advanced": _ReaderAdvanced.WriteKey(_Element.Key, _Element.Value.ToString(), _Element.Section); break;
												case "SuitHud": _ReaderOnfoot.WriteKey(_Element.Key, _Element.Value.ToString(), _Element.Section); break;
												case "Custom": _Custom.WriteKey(_Element.Key, _Element.Value.ToString(), _Element.Section); break;
												default: _Reader.WriteKey(_Element.Key, _Element.Value.NVL("0"), _Element.Section); break;
											}
										}
									}
								}
							}
						}

						//Now we Apply the XML:
						if (Settings.xml_profile.IsNotEmpty())
						{
							if (_ReaderXML != null)
							{
								foreach (var _key in Settings.xml_profile)
								{
									_ReaderXML.WriteKey(_key.key, _key.value.ToString(), "constants");
								}
							}
						}

						ApplyGlobalSettings();

						//if (SaveIt) SaveTheme(true);

						//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 2 SEGUNDOS:
						if (KeepItQuiet == false)
						{
							XtraMessageBoxArgs args = new XtraMessageBoxArgs()
							{
								Caption = "Visual Theme Applied Succesfully!.",
								Text = string.Format("{0} by {1}\r\n{2}", Settings.name, Settings.author, ActiveInstance.instance),
								Buttons = new DialogResult[] { DialogResult.OK }
							};
							args.AutoCloseOptions.Delay = 2000;
							args.AutoCloseOptions.ShowTimerOnDefaultButton = true;

							XtraMessageBox.Show(args).ToString();
						}
					}
					else
					{
						XtraMessageBox.Show("the Game' Instance Path is not set.", "ERROR_404",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				Invoke((MethodInvoker)(() =>
				{
					LoadingTheme = false;
					Cursor = Cursors.Default;
					MainMenu_ApplyTheme.Enabled = true;
					gridControl1.Cursor = Cursors.Default;

					if (OnThemeApply != null)
					{
						OnThemeApply(null, null); //<-Se ha Aplicado un Tema
					}
				}));
			}
		}

		private void History_AddSettings(ui_setting _Theme)
		{
			/* ADD THE THEME TO THE HISTORY */
			try
			{
				if (Settings != null)
				{
					//Get the History folder for the Current Instance
					string GameInstanceID = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
					string HistoryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
											   @"Elite Dangerous\EDHM_UI", GameInstanceID, "History");

					if (!Directory.Exists(HistoryFolder))
					{
						Directory.CreateDirectory(HistoryFolder);
					}

					//File has a timestamp in the name:
					string FilePath = Path.Combine(HistoryFolder, string.Format("{0}.json", DateTime.Now.ToString("yyyyMMddHHmmss")));

					//Saves the data in JSON format
					Util.Serialize_ToJSON(FilePath, Settings);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void History_LoadElements(int ItemsToLoad = 10)
		{
			try
			{
				//Obtiene la ruta de la carpeta con el historial, para la instancia actual
				string GameInstanceID = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
				string HistoryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
										   @"Elite Dangerous\EDHM_UI", GameInstanceID, "History");

				if (Directory.Exists(HistoryFolder))
				{
					DirectoryInfo di = new DirectoryInfo(HistoryFolder);

					//Busca todos los JSON en la carpeta, ordenaos descentemente x fecha:
					var JsonFiles = di.GetFiles("*.json").OrderByDescending(f => f.LastWriteTime);
					if (JsonFiles != null)
					{
						System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
						MainMenu_History.ClearLinks(); //<- Borra el menu del Historial
						var _Files = JsonFiles.ToList();    //<- Lista de archivos

						//Carga los archivos hasta el tope o hasta que se acaben
						for (int i = 0; i < ItemsToLoad && i < _Files.Count; i++)
						{
							FileInfo _File = _Files[i];
							string FileName = System.IO.Path.GetFileNameWithoutExtension(_File.FullName); //<- Nombre sin Extension ni Path
							DateTime _Date = DateTime.ParseExact(FileName, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);

							//Crea el menu, el nombre se obtiene de la fecha del archivo
							//El archivo en sí se guarda en el 'tag' del menu
							BarButtonItem _Menu = new BarButtonItem
							{
								Name = _File.Name,
								Caption = Util.GetPrettyDate(_Date),
								Tag = _File
							};
							_Menu.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("MainMenu_InstallMod.ImageOptions.SvgImage")));
							_Menu.ItemClick += (object _Sender, ItemClickEventArgs _E) =>
							{
								//Ocurre cuando el menu es clickeado								
								FileInfo _SelectedFile = _E.Item.Tag as FileInfo;
								if (_SelectedFile != null)
								{
									//De-Serialize the json file:
									Settings = Util.DeSerialize_FromJSON<ui_setting>(_SelectedFile.FullName);
									if (Settings != null && Settings.Presets.IsNotEmpty())
									{
										//Carga la Lista de Presets disponibles:
										_ElementPresets = Settings.Presets;
									}
									ApplyTheme(true);
								}
							};
							MainMenu_History.AddItem(_Menu);
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void SaveTheme(bool Silent = false)
		{
			try
			{
				// THIS SAVES THE CHANGES INTO THE JSON FILE */
				string JsonSettings_path = Path.Combine(AppExePath, "Data", ActiveInstance.key + string.Format("_Settings_EN.json"));
				if (System.IO.File.Exists(JsonSettings_path))
				{
					if (Settings != null)
					{
						Util.Serialize_ToJSON(JsonSettings_path, Settings);
						if (!Silent)
						{
							XtraMessageBox.Show("Theme Saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}
		private void SaveThemeChanges()
		{
			/*  SAVES THE CHANGES INTO THE SELECTED THEME */
			try
			{
				if (!SavingTheme)
				{
					SavingTheme = true;
					if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
					{
						string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
						string GameFolder = Path.Combine(ActiveInstance.path, @"EDHM-ini");
						string NewProfileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
												   @"Elite Dangerous\EDHM_UI", search, "Themes", SelectedTheme.name);

						//1. Crear la Carpeta para el Nuevo Perfil, si ya Existe, se Sobreescribe:
						DirectoryInfo _ProfileDir = System.IO.Directory.CreateDirectory(NewProfileFolder);
						if (_ProfileDir != null)
						{
							//Cuando se termina de Aplicar el tema se produce este evento:
							OnThemeApply += (object _Sender, EventArgs _E) =>
							{
								//Copiar los Archivos del Tema Actual: // existing files will be overwritten
								File.Copy(Path.Combine(GameFolder, @"Startup-Profile.ini"),
										Path.Combine(NewProfileFolder, @"Startup-Profile.ini"), true);

								File.Copy(Path.Combine(GameFolder, "XML-Profile.ini"),
									Path.Combine(NewProfileFolder, @"XML-Profile.ini"), true);

								if (ActiveInstance.key == "ED_Odissey")
								{
									if (File.Exists(Path.Combine(GameFolder, @"Advanced.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"Advanced.ini"),
										Path.Combine(NewProfileFolder, @"Advanced.ini"), true);
									}
									if (File.Exists(Path.Combine(GameFolder, @"SuitHud.ini")))
									{
										File.Copy(Path.Combine(GameFolder, @"SuitHud.ini"),
										Path.Combine(NewProfileFolder, @"SuitHud.ini"), true);
									}
								}

								//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 2 SEGUNDOS:
								XtraMessageBoxArgs args = new XtraMessageBoxArgs()
								{
									Caption = "Done",
									Text = string.Format("The profile '{0}' has successfully been Saved.", SelectedTheme.name),
									Buttons = new DialogResult[] { DialogResult.OK }
								};
								args.AutoCloseOptions.Delay = 2000;
								args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
								XtraMessageBox.Show(args).ToString();

								SavingTheme = false;
								OnThemeApply = null;

								if (SelectedTheme != null)
								{
									int rowHandle = gridView1.LocateByValue("name", SelectedTheme.name);
									if (rowHandle != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
									{
										gridView1.FocusedRowHandle = rowHandle;
									}
								}
							};
							ApplyTheme(true, true); //<- Aplica los cambios Actuales 
						}
					}
					else
					{
						CreateNewTheme();
						SavingTheme = false;
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		private bool ImportTheme()
		{
			bool _ret = false;

			/* IMPORTA UN ZIP DESCARGADO CON UN TEMA NUEVO Y LO INSTALA EN LA CARPETA DE LOS TEMAS */
			try
			{
				/* SOLO FUNCIONA CON ARCHIVOS .ZIP  */
				OpenFileDialog XOFD = new OpenFileDialog()
				{
					Filter = "ZIP files|*.zip",
					FilterIndex = 0,
					DefaultExt = "zip",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = true,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};
				if (XOFD.ShowDialog() == DialogResult.OK)
				{
					Cursor = Cursors.WaitCursor;

					string FileName = System.IO.Path.GetFileNameWithoutExtension(XOFD.FileName); //<- Nombre sin Extension ni Path
					string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
					string ThemesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
											   @"Elite Dangerous\EDHM_UI", search, "Themes");

					if (Directory.Exists(ThemesFolder))
					{
						Util.DoNetZIP_UnCompressFile(XOFD.FileName, ThemesFolder);

						_ret = true;
						XtraMessageBox.Show(string.Format("The theme file '{0}' had been Installed!", FileName), "Success!",
									MessageBoxButtons.OK, MessageBoxIcon.Information);

						LoadThemeList_EX();
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { Cursor = Cursors.Default; }
			return _ret;
		}
		private bool ExportTheme()
		{
			/* Convierte la configuracion actual en un tema nuevo y lo empaca en un ZIP  */
			bool _ret = false;
			try
			{
				string NewThemePath = CreateNewThemeSync();

				if (!NewThemePath.EmptyOrNull() && Directory.Exists(NewThemePath))
				{
					string ThemeName = new DirectoryInfo(NewThemePath).Name;

					SaveFileDialog XSFD = new SaveFileDialog()
					{
						Filter = "ZIP file|*.zip",
						FilterIndex = 0,
						DefaultExt = "zip",
						AddExtension = true,
						CheckPathExists = true,
						OverwritePrompt = true,
						FileName = string.Format(ThemeName),
						InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
					};

					if (XSFD.ShowDialog() == DialogResult.OK)
					{
						Cursor = Cursors.WaitCursor;

						bool _IsFavorite = File.Exists(Path.Combine(NewThemePath, "IsFavorite.fav"));
						if (_IsFavorite)
						{
							//No queremos incluir el 'Favorito' con el tema exportado
							File.Delete(Path.Combine(NewThemePath, "IsFavorite.fav"));
						}

						Util.DoNetZIP_CompressFolder(NewThemePath, XSFD.FileName);

						if (_IsFavorite)
						{
							//Volvemos a dejar la marca de favorito
							Util.SaveTextFile(Path.Combine(NewThemePath, "IsFavorite.fav"),
															"** THIS THEME IS A FAVORITE **",
															Util.TextEncoding.UTF8);
						}

						if (File.Exists(XSFD.FileName))
						{
							_ret = true;
							string FileName = System.IO.Path.GetFileName(XSFD.FileName);

							XtraMessageBox.Show(string.Format("The theme file '{0}' had been Exported!", FileName), "Success!",
										MessageBoxButtons.OK, MessageBoxIcon.Information);

							LoadThemeList_EX();

							//Abrir una carpeta en el Explorador de Windows y Seleccionar un Archivo:
							string argument = string.Format("/select, \"{0}\"", XSFD.FileName);
							System.Diagnostics.Process.Start("explorer.exe", argument);
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { Cursor = Cursors.Default; }
			return _ret;
		}

		private void UninstallEDHMmod(bool Silent = false)
		{
			try
			{
				if (!CheckGameRunning())
				{
					if (Directory.Exists(ActiveInstance.path))
					{
						string _KEY = ActiveInstance.key == "ED_Horizons" ? "HORIZONS" : "ODYSSEY";

						progressPanel1.Dock = DockStyle.Fill;
						progressPanel1.Caption = String.Format("Un-Installing EDHM ({0})", _KEY);
						progressPanel1.Description = "";
						progressPanel1.Visible = true;
						progressPanel1.BringToFront();

						Application.DoEvents();
						bool Continuar = true;

						if (!Silent)
						{
							if (XtraMessageBox.Show("This will remove EDHM from your Game.\r\nDo you want to Continue?", "Un-Install EDHM?",
									MessageBoxButtons.YesNo, MessageBoxIcon.Stop) != DialogResult.Yes)
							{
								Continuar = false;
							}
						}

						if (Continuar)
						{
							Cursor = Cursors.WaitCursor;

							System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
							var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
							{
								System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

								/* COMMON FILES AND FOLDERS */
								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing d3dx.ini"; }));
								string File_Path = Path.Combine(ActiveInstance.path, @"d3dx.ini");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing d3d11.dll"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"d3d11.dll");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing d3d11_log.txt"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"d3d11_log.txt");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing d3d11_profile_log.txt"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"d3d11_profile_log.txt");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing d3dcompiler_46.dll"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"d3dcompiler_46.dll");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing d3dcompiler_46_log.txt"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"d3dcompiler_46_log.txt");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing nvapi64.dll"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"nvapi64.dll");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing nvapi_log.txt"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"nvapi_log.txt");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing ShaderUsage.txt"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"ShaderUsage.txt");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing ShaderFixes"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"ShaderFixes");
								if (Directory.Exists(File_Path))
								{
									Directory.Delete(File_Path, true);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-ini"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"EDHM-ini");
								if (Directory.Exists(File_Path))
								{
									Directory.Delete(File_Path, true);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing ShaderCache"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"ShaderCache");
								if (Directory.Exists(File_Path))
								{
									Directory.Delete(File_Path, true);
								}

								Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-Uninstall.bat"; }));
								File_Path = Path.Combine(ActiveInstance.path, @"EDHM-Uninstall.bat");
								if (File.Exists(File_Path))
								{
									File.Delete(File_Path);
								}

								/* ODYSSEY EXCLUSIVE FILES */
								if (_KEY == "ODYSSEY")
								{

								}

								/* HORIZONS EXCLUSIVE FILES */
								if (_KEY == "HORIZONS")
								{
									Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-v1.5-Catalogue.pdf"; }));
									File_Path = Path.Combine(ActiveInstance.path, @"EDHM-v1.5-Catalogue.pdf");
									if (File.Exists(File_Path))
									{
										File.Delete(File_Path);
									}

									Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-v1.51-Manual.pdf"; }));
									File_Path = Path.Combine(ActiveInstance.path, @"EDHM-v1.51-Manual.pdf");
									if (File.Exists(File_Path))
									{
										File.Delete(File_Path);
									}

									Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-v1.51-Profile-Guide.pdf"; }));
									File_Path = Path.Combine(ActiveInstance.path, @"EDHM-v1.51-Profile-Guide.pdf");
									if (File.Exists(File_Path))
									{
										File.Delete(File_Path);
									}

									Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-v1.52-Manual.pdf"; }));
									File_Path = Path.Combine(ActiveInstance.path, @"EDHM-v1.52-Manual.pdf");
									if (File.Exists(File_Path))
									{
										File.Delete(File_Path);
									}

									Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-Keybinds-Essential.bat"; }));
									File_Path = Path.Combine(ActiveInstance.path, @"EDHM-Keybinds-Essential.bat");
									if (File.Exists(File_Path))
									{
										File.Delete(File_Path);
									}

									Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-Keybinds-Full.bat"; }));
									File_Path = Path.Combine(ActiveInstance.path, @"EDHM-Keybinds-Full.bat");
									if (File.Exists(File_Path))
									{
										File.Delete(File_Path);
									}

									Invoke((MethodInvoker)(() => { progressPanel1.Description = "Removing EDHM-RemoveDemos.bat"; }));
									File_Path = Path.Combine(ActiveInstance.path, @"EDHM-RemoveDemos.bat");
									if (File.Exists(File_Path))
									{
										File.Delete(File_Path);
									}
								}

								Invoke((MethodInvoker)(() =>
								{
									progressPanel1.Description = "Done.";
									progressPanel1.Visible = false;
									Cursor = Cursors.Default;

									if (!Silent)
									{
										XtraMessageBox.Show("EDHM had been completely removed from your Game!\r\nThank you for trying us ^_^", "Process Complete",
															MessageBoxButtons.OK, MessageBoxIcon.Information);
									}
								}));
							});
						}
						else
						{
							progressPanel1.Description = "Done.";
							progressPanel1.Visible = false;
							Cursor = Cursors.Default;
						}
					}
				}
				else
				{
					XtraMessageBox.Show("Please Close the Game and try again.", "Game Most Be Closed!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
				progressPanel1.Visible = false;
				Cursor = Cursors.Default;
			}
		}
		private void ExecuteBAT(string FilePath, string _Arguments = "")
		{
			if (!FilePath.EmptyOrNull() && File.Exists(FilePath))
			{
				var _DOS = new System.Diagnostics.ProcessStartInfo
				{
					CreateNoWindow = true, //This hides the dos-style black window that the command prompt usually shows
					FileName = @"cmd.exe",
					Verb = "runas", //<-   'open', 'runas', 'runasuser' | This is what actually runs the command as administrator
					WorkingDirectory = System.IO.Path.GetDirectoryName(FilePath),
					Arguments = string.Format("/C \"{0}\"", FilePath) //<- Comillas para las rutas con espacios
				};
				if (!_Arguments.EmptyOrNull())
				{
					_DOS.Arguments = string.Format("/C \"{0}\" {1}", FilePath, _Arguments);
				}
				try
				{
					var _BAT = new System.Diagnostics.Process();
					_BAT.StartInfo = _DOS;
					_BAT.Start();
					_BAT.WaitForExit();

					Console.WriteLine("ExitCode: {0}", _BAT.ExitCode);
					if (_BAT.ExitCode >= 0)
					{
						//ExitCode: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
						//SUCCESS= 0, INVALID_FUNCTION = 1, FILE_NOT_FOUND = 2, PATH_NOT_FOUND = 3, ACCESS_DENIED = 5

						//MessageBox.Show(string.Format("Process Complete. \r\nExitCode: {0}", _BAT.ExitCode), "Done",
						//	MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					_BAT.Close();
				}
				catch (Exception)
				{
					//If you are here the user clicked decline to grant admin privileges (or he's not administrator)
					MessageBox.Show("You have to Aprove the program execution!", "Canceled!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}

		private bool Run_HotFix()
		{
			bool _ret = false;
			try
			{
				if (File.Exists(Path.Combine(AppExePath, "EDHM_HOTFIX.json")))
				{
					List<file_job> _Jobs = Util.DeSerialize_FromJSON<List<file_job>>(Path.Combine(AppExePath, "EDHM_HOTFIX.json"));
					if (_Jobs != null)
					{
						/* AHORA ES MULTI-INSTANCIA !!!!!******  */
						List<GameInstance> GameInstancesEx = null;

						string _RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");
						string GameInstances_JSON = Util.WinReg_ReadKey("EDHM", "GameInstances").NVL(string.Empty);

						#region Load the Game Instances from Windows Registry

						if (!GameInstances_JSON.EmptyOrNull())
						{
							GameInstancesEx = Util.DeSerialize_FromJSON_String<List<GameInstance>>(GameInstances_JSON);
							if (GameInstancesEx != null && GameInstancesEx.Count > 0)
							{
								GameInstances_JSON = Util.Serialize_ToJSON(GameInstancesEx);
							}
						}
						else
						{
							//Crea Las Instancias x Defecto:
							GameInstancesEx = new List<GameInstance>();
							GameInstancesEx.Add(new GameInstance() { instance = "Default", games = new List<game_instance>() });
							GameInstancesEx[0].games.Add(new game_instance()
							{
								key = "ED_Horizons",
								name = "Horizons",
								instance = "Horizons (Default)",
								game_id = "Default|ED_Horizons",
								themes_folder = @"EDHM-ini\DemoProfiles",
								path = Util.WinReg_ReadKey("EDHM", "ED_Horizons").NVL(string.Empty),
								is_active = (_RegActiveInstance == "ED_Horizons" ? true : false)
							});
							GameInstancesEx[0].games.Add(new game_instance()
							{
								key = "ED_Odissey",
								name = "Odyssey",
								instance = "Odyssey (Default)",
								game_id = "Default|ED_Odissey",
								themes_folder = @"EDHM-ini\MyProfiles",
								path = Util.WinReg_ReadKey("EDHM", "ED_Odissey").NVL(string.Empty),
								is_active = (_RegActiveInstance == "ED_Horizons" ? true : false)
							});

							GameInstances_JSON = Util.Serialize_ToJSON(GameInstancesEx);
						}

						#endregion

						if (GameInstancesEx != null && GameInstancesEx.Count > 0)
						{
							// Aplicar el HotFix a cada instancia
							foreach (var _Instance in GameInstancesEx)
							{
								string HORI_Path = _Instance.games.Find(x => x.key == "ED_Horizons").path.NVL(string.Empty);
								string ODYS_Path = _Instance.games.Find(x => x.key == "ED_Odissey").path.NVL(string.Empty);
								string UI_Documents = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Elite Dangerous\EDHM_UI");

								foreach (file_job _job in _Jobs)
								{
									string GamePath = _job.game == "ODYSSEY" ? ODYS_Path : HORI_Path;
									string GameInstance = _job.game == "ODYSSEY" ? "ODYSS" : "HORIZ";

									_job.file_path = _job.file_path.Replace("%GAME_PATH%", GamePath);
									_job.file_path = _job.file_path.Replace("%UI_PATH%", AppExePath);
									_job.file_path = _job.file_path.Replace("%UI_DOCS%", UI_Documents);

									if (_job.destination != null && _job.destination != string.Empty)
									{
										_job.destination = _job.destination.Replace("%GAME_PATH%", GamePath);
										_job.destination = _job.destination.Replace("%UI_PATH%", AppExePath);
									}

									try
									{
										switch (_job.action)
										{
											case "COPY": //Copia un Archivo o Directorio de un lugar a otro, acepta comodines
												if (!Directory.Exists(Path.GetDirectoryName(_job.destination)))
												{
													Directory.CreateDirectory(Path.GetDirectoryName(_job.destination));
												}

												if (File.Exists(_job.file_path))
												{
													File.Copy(_job.file_path, _job.destination, true);
												}

												break;

											case "MOVE": //Mueve un Archivo de un lugar a otro, acepta comodines
												if (File.Exists(_job.file_path))
												{
													if (!Directory.Exists(Path.GetDirectoryName(_job.destination)))
													{
														Directory.CreateDirectory(Path.GetDirectoryName(_job.destination));
													}

													File.Copy(_job.file_path, _job.destination, true);
													File.Delete(_job.file_path);
												}
												break;

											case "REPLACE": //Copia el Archivo sólo si existe previamente
												if (File.Exists(_job.file_path) && File.Exists(_job.destination))
												{
													File.Copy(_job.file_path, _job.destination, true);
												}
												break;

											case "DEL": //Borra un Archivo
												if (File.Exists(_job.file_path))
												{
													File.Delete(_job.file_path);
												}

												break;

											case "RMDIR": //Borra un Directorio y todo su contenido
												if (Directory.Exists(_job.file_path))
												{
													Directory.Delete(_job.file_path, true);
												}

												break;

											case "MVDIR": //Mueve un Directorio de un lugar a otro
												if (Directory.Exists(_job.file_path))
												{
													Directory.Move(_job.file_path, _job.destination);
												}

												break;

											case "RMDIR-EX": //Borra las Carpetas de un Directorio salvo las Execpciones
															 //El nombre del directorio Raiz va en 'file_path', ej: "file_path":"%UI_DOCS%\\ODYSS",
															 //Las Excepciones van en 'destination', solo los nombres separados x comas. ej:  "destination":"Themes,History"
												if (Directory.Exists(_job.file_path))
												{
													var Exepciones = _job.destination.Split(new char[] { ',' }).ToList();
													var directories = Directory.GetDirectories(_job.file_path);
													if (directories != null)
													{
														foreach (string _DIR in directories)
														{
															string _DirName = Path.GetFileNameWithoutExtension(_DIR);
															var _EsExcepcion = Exepciones.Find(x => x == _DirName);
															if (_EsExcepcion is null)
															{
																Directory.Delete(_DIR, true);
															}
														}
													}
												}
												break;
										}
									}
									catch { }
								}
							}
							_ret = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace);
			}
			return _ret;
		}

		private bool CheckGameRunning(bool ForceCheck = false)
		{
			bool _ret = false;
			try
			{
				//Busca la Ventana del Juego:
				if (GameWindow == null || ForceCheck == true)
				{
					string GameTitle = Util.AppConfig_GetValue("GameProcessID");
					System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
					foreach (System.Diagnostics.Process process in processlist)
					{
						if (!String.IsNullOrEmpty(process.MainWindowTitle))
						{
							if (process.MainWindowTitle == GameTitle)
							{
								GameWindow = process;
								GameWindow.EnableRaisingEvents = true;
								GameWindow.Exited += (sender, eventArgs) =>
								{
									//Ocurre cuando el Juego se Cierra:
									GameIsRunning = false;
									GameWindow = null;
									_ret = false;
								};
								GameIsRunning = true;
								_ret = true;
								break;
							}
						}
					}
					processlist = null;
				}
				else
				{
					if (GameWindow.HasExited == false)
					{
						GameIsRunning = true;
						_ret = true;
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		private bool CheckGameRunning()
		{
			//Busca un Proceso x Nombre de Ventana:
			string GameTitle = Util.AppConfig_GetValue("GameProcessID");
			System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
			System.Diagnostics.Process UI_Proc = null;

			foreach (System.Diagnostics.Process process in processlist)
			{
				if (!String.IsNullOrEmpty(process.ProcessName))
				{
					if (process.MainWindowTitle == GameTitle)
					{
						UI_Proc = process;
						break;
					}
				}
			}
			return UI_Proc != null ? true : false;
		}
		private bool KillGameProcces()
		{
			//Busca un Proceso x Nombre de Ventana:
			string GameTitle = Util.AppConfig_GetValue("GameProcessID");
			System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
			System.Diagnostics.Process Game_Proc = null;

			foreach (System.Diagnostics.Process process in processlist)
			{
				if (!String.IsNullOrEmpty(process.MainWindowTitle))
				{
					if (process.MainWindowTitle == GameTitle)
					{
						Game_Proc = process;
						break;
					}
				}
			}
			if (Game_Proc != null)
			{
				//Game_Proc.CloseMainWindow();
				//Game_Proc.WaitForExit(5000);
				//if (Game_Proc != null && Game_Proc.HasExited == false) Game_Proc.Kill();

				XtraMessageBoxArgs args = new XtraMessageBoxArgs()
				{
					Caption = "Game is Running!",
					Text = "Game needs to be closed in order to install any MODS\r\nClosing it in..",
					Buttons = new DialogResult[] { DialogResult.OK }
					//,Icon = new Icon(MessageBoxIcon.Information)
				};
				args.AutoCloseOptions.Delay = 5000;
				args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
				if (XtraMessageBox.Show(args) == DialogResult.OK)
				{
					if (Game_Proc != null && Game_Proc.HasExited == false)
					{
						Game_Proc.Kill();
					}
				}
			}
			return Game_Proc != null ? true : false;
		}


		/* HERE CHECKS FOR UPDATES  */
		bool CheckingUpdates = false; //<- Previene que se llame este metodo varias veces a la vez.
		private void CheckForModUpdates(bool QuietMode = true)
		{
			/* AQUI SE BUSCAN ACTUALIZACIONDES DEL PROGRAMA, DEL MOD Y ARCHIVOS VARIOS */
			try
			{
				if (!CheckingUpdates)
				{
					CheckingUpdates = true;
					string _FileName = "EDHM_Version.json";
					string TempFilePath = Path.Combine(Path.GetTempPath(), "EDHM_UI", _FileName); //<- %LOCALAPPDATA%\Temp\EDHM_UI\EDHM_Version.json

					if (File.Exists(TempFilePath))
					{
						File.Delete(TempFilePath);
					}

					FileDownloader FD = new FileDownloader(Util.AppConfig_GetValue("UpdatesURL"), TempFilePath);
					FD.OnDownload_Complete += (sender, eventArgs) =>
					{
						//System.ComponentModel.AsyncCompletedEventArgs Ev = eventArgs as System.ComponentModel.AsyncCompletedEventArgs;
						long[] _Data = sender as long[];
						if (_Data != null)
						{
							CheckingUpdates = false;
							if (File.Exists(TempFilePath))
							{
								VersionInfo VI = Util.DeSerialize_FromJSON<VersionInfo>(TempFilePath);
								if (VI != null)
								{
									bool HayActualizacion = false;

									//Obtiene las Versiones Actuales:
									Version App_Version = new Version(Util.AppConfig_GetValue("Version"));
									Version HoriVersion = new Version(Util.AppConfig_GetValue("HORIZ_EDHM_Version"));
									Version OddyVersion = new Version(Util.AppConfig_GetValue("ODYSS_EDHM_Version"));

									//Compara las Versiones Actuales con la Descargada:
									if (App_Version < new Version(VI.app_version))
									{
										HayActualizacion = true;
									}
									//if (HoriVersion < new Version(VI.ED_Horizons)) HayActualizacion = true;
									//if (OddyVersion < new Version(VI.ED_Odissey)) HayActualizacion = true;

									VI.cur_version = App_Version.ToString();
									Util.Serialize_ToJSON(TempFilePath, VI);

									if (HayActualizacion)
									{
										//Hay Actualizacion!
										if (XtraMessageBox.Show(string.Format("There is an Update Available v{0}: \r\nChange Log:\r\n{1}", VI.app_version, VI.changelog),
												"Download Update?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
										{
											if (PreviewForm != null && PreviewForm.Visible)
											{
												PreviewForm.Close();
											}

											System.Diagnostics.ProcessStartInfo StartInfo = new System.Diagnostics.ProcessStartInfo
											{
												UseShellExecute = true,
												Verb = "runas",  //<- Ejecuta el Programa con Permisos Elevados
												WorkingDirectory = Environment.CurrentDirectory,
												FileName = "EDHM_UI_Patcher.exe",
												Arguments = string.Format("\"{0}\"", TempFilePath)
											};
											System.Diagnostics.Process p = System.Diagnostics.Process.Start(StartInfo);
										}
									}
									else
									{
										//No hay Actualizacion!
										if (!QuietMode)
										{
											XtraMessageBox.Show("Program is Up to date.", "All good!", MessageBoxButtons.OK, MessageBoxIcon.Information);
										}
										Console.WriteLine("EDHM mod is up to date.");
									}
								}
							}
						}
						else
						{
							//MessageBox.Show(Ev.Error.Message + Ev.Error.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					};
					FD.StartDownload(); //<- Aqui se Inicia la Descarga
				}
			}
			catch (System.ComponentModel.Win32Exception exception)
			{
				XtraMessageBox.Show(exception.Message);
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		public void GetUserIPData()
		{
			try
			{
				string URL = "ipinfo.io/200.58.144.171?token=d811bd45b5fcf5";

				System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URL);
				request.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

				using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
				using (Stream stream = response.GetResponseStream())
				using (StreamReader reader = new StreamReader(stream))
				{
					Console.WriteLine(reader.ReadToEnd());
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		private void Load_UITips(bool ForceShow = false)
		{
			try
			{
				if (true)
				{
					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

						List<UI_Tips> _Tips = null;
						string JsonSettings_path = Path.Combine(AppExePath, "Data", "UI_Tips.json");

						if (File.Exists(JsonSettings_path))
						{
							_Tips = Util.DeSerialize_FromJSON<List<UI_Tips>>(JsonSettings_path);
						}

						if (_Tips.IsNotEmpty())
						{
							//Carga el Idioma del Usuario:
							UI_Tips _TipsLang = _Tips.Find(x => x.Name == "Tips" && x.Language == LangShort);
							if (_TipsLang == null)
							{
								_TipsLang = _Tips.Find(x => x.Name == "Tips" && x.Language == "en");
							}
							if (_TipsLang != null)
							{
								//Actualiza los Controles fuera de este Proceso:	
								Invoke((MethodInvoker)(() =>
								{
									listTips.DataSource = _TipsLang.Elements;

									dockTips.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
									dockTips.HideImmediately();
									dockTips.Text = _TipsLang.Title;

									if (ShowTips || ForceShow)
									{
										dockTips.ShowSliding();
									}
								}));
							}
						}
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		#endregion

		#region Utilities

		/// <summary>Devuelve 'true' si el color contiene valores RGB validos.</summary>
		/// <param name="_RGBA">Valores RGBA, (A es opcional)</param>
		private bool IsColor(List<double> _RGBA)
		{
			bool _ret = true;
			try
			{
				if (_RGBA != null && _RGBA.Count > 0)
				{
					foreach (var item in _RGBA)
					{
						if (item < 0)
						{
							_ret = false;
						}
					}
				}
			}
			catch { }
			return _ret;
		}

		private double Convert_sRGB_FromLinear(double theLinearValue, double _GammaValue = 2.4)
		{ return theLinearValue <= 0.0031308f ? theLinearValue * 12.92f : Math.Pow(theLinearValue, 1.0 / _GammaValue) * 1.055 - 0.055; }
		private double Convert_sRGB_ToLinear(double thesRGBValue, double _GammaValue = 2.4)
		{ return thesRGBValue <= 0.04045f ? thesRGBValue / 12.92f : Math.Pow((thesRGBValue + 0.055f) / 1.055f, _GammaValue); }

		private List<double> GetGammaCorrected_RGBA(System.Drawing.Color _Color, double _GammaValue = 2.4)
		{
			List<double> _ret = null; //<- valores en formato RGBA
			try
			{
				//El proceso de conversion es asi:  RGB -> sRGB (no lineal) -> Gamma Corrected

				ColorManagment.ColorConverter.Init();

				//Convertir al espacio sRGB no lineal: 
				ColorManagment.ColorRGB sRGBcolor = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, _Color.R, _Color.G, _Color.B);

				//Get Gamma Corrected Values:
				_ret = new List<double>();
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.R, _GammaValue), 4));
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.G, _GammaValue), 4));
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.B, _GammaValue), 4));
				_ret.Add(Math.Round(Convert.ToDouble(Util.NormalizeNumber(_Color.A, 0m, 255m, 0m, 1m)), 4)); //alpha remains linear!
			}
			catch (Exception ex) { MessageBox.Show(ex.Message + ex.StackTrace); }
			return _ret;
		}
		private Color ReverseGammaCorrected(double _Gamma_R, double _Gamma_G, double _Gamma_B, double _Gamma_A = 1.0, double _GammaValue = 2.4)
		{
			System.Drawing.Color _ret = System.Drawing.Color.White;
			try
			{
				//El proceso de de conversion es asi:   GammaCorrectedValue -> sRGB -> Linear sRGB -> RGB

				//Undo Gamma Correction: Produce Valores en sRGB
				var inv_R = Convert_sRGB_FromLinear(_Gamma_R, _GammaValue);
				var inv_G = Convert_sRGB_FromLinear(_Gamma_G, _GammaValue);
				var inv_B = Convert_sRGB_FromLinear(_Gamma_B, _GammaValue);

				//Linearizar el sRGB:
				var colorB = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, inv_R, inv_G, inv_B, true);

				//Convertir a RGB normal:
				int R = (int)Math.Round(colorB.R * 255, MidpointRounding.AwayFromZero);
				int G = (int)Math.Round(colorB.G * 255, MidpointRounding.AwayFromZero);
				int B = (int)Math.Round(colorB.B * 255, MidpointRounding.AwayFromZero);
				int A = (int)Util.NormalizeNumber(Convert.ToDecimal(_Gamma_A), 0.0m, 1m, 0m, 255m);

				_ret = System.Drawing.Color.FromArgb(A, R, G, B);
			}
			catch (Exception ex) { MessageBox.Show(ex.Message + ex.StackTrace); }
			return _ret;
		}
		private Color ReverseGammaCorrected(List<double> _GammaComponents, double _GammaValue = 2.4)
		{
			System.Drawing.Color _ret = System.Drawing.Color.White;
			try
			{
				//El proceso de de conversion es asi:   GammaCorrectedValue -> sRGB -> Linear sRGB -> RGB

				//Undo Gamma Correction: Produce Valores en sRGB
				double inv_R = Convert_sRGB_FromLinear(_GammaComponents[0], _GammaValue);
				double inv_G = Convert_sRGB_FromLinear(_GammaComponents[1], _GammaValue);
				double inv_B = Convert_sRGB_FromLinear(_GammaComponents[2], _GammaValue);
				double alpha = 1;
				if (_GammaComponents.Count > 3)
				{
					alpha = _GammaComponents[3];
				}

				//Linearizar el sRGB:
				var colorB = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, inv_R, inv_G, inv_B, true);

				//Convertir a RGB normal:
				int R = (int)Math.Round(colorB.R * 255, MidpointRounding.AwayFromZero);
				int G = (int)Math.Round(colorB.G * 255, MidpointRounding.AwayFromZero);
				int B = (int)Math.Round(colorB.B * 255, MidpointRounding.AwayFromZero);
				int A = (int)Util.NormalizeNumber(Convert.ToDecimal(alpha), 0.0m, 1m, 0m, 255m);

				_ret = System.Drawing.Color.FromArgb(A, R, G, B);
			}
			catch (Exception ex) { MessageBox.Show(ex.Message + ex.StackTrace); }
			return _ret;
		}

		private void ShowSystemNotificacion(string pTitle, string pMessage, int pTimeOut = 3000)
		{
			/* MUESTRA UNA NOTIFICACION DEL TIPO BALLOONTIP de Windows */
			try
			{
				var notification = new System.Windows.Forms.NotifyIcon()
				{
					Icon = System.Drawing.SystemIcons.Information,
					BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info,
					BalloonTipTitle = pTitle,
					BalloonTipText = pMessage,
					Visible = true
				};
				//Quitar el Icono cuando se cierra la Notificacion:
				notification.BalloonTipClosed += (sender, e) =>
				{
					var thisIcon = (NotifyIcon)sender;
					thisIcon.Visible = false;
					thisIcon.Dispose();
				};
				notification.BalloonTipClicked += (sender, e) =>
				{
					var thisIcon = (NotifyIcon)sender;
					thisIcon.Visible = false;
					thisIcon.Dispose();
				};
				// Display for 3 seconds.
				notification.ShowBalloonTip(pTimeOut);
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		#endregion

		#region Shipyard

		private bool ApplyingShip = false;

		private void ReadPlayerJournal()
		{
			try
			{
				if (WatchMe)
				{
					// Player Journal default location:  %UserProfile%\Saved Games\Frontier Developments\Elite Dangerous\

					//Esto Lee las Flags usadas en el archivo 'Status.json' https://elite-journal.readthedocs.io/en/latest/Status%20File/
					/*var mask = (StatusFlags)16842765;
					var result =
						Enum.GetFlafValues(typeof(StatusFlags))
							.Cast<StatusFlags>()
							.Where(value => mask.HasFlag(value))
							.ToList();  
					*/

					string EDJournalDir = Util.WinReg_ReadKey("EDHM", "PlayerJournal").NVL("");
					if (Directory.Exists(EDJournalDir))
					{
						System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
						var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
						{
							System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
							DirectoryInfo di = new DirectoryInfo(EDJournalDir);

							//Busca el Archivo de Log mas reciente:
							FileInfo JournalFile = di.GetFiles("Journal.*.log")
										.OrderByDescending(f => f.LastWriteTime).First();

							if (JournalFile != null)
							{
								//Abre el archivo en modo compartido y 'Escucha' si ha sido modificado:
								//Se queda con este archivo si el juego ya estaba corriendo cuando se abrio la UI
								PlayerJournal_WatchFile(JournalFile.FullName);
							}

							//Queda vigilando el Directoro x si se crea un nuevo archivo de log:
							//pasa cuando se abre el juego, despues de haber abierto la UI
							PlayerJournal_WatchDirectory(EDJournalDir);
						});
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void PlayerJournal_WatchDirectory(string pDirectoryPath)
		{
			FileSystemWatcher watcher = new FileSystemWatcher
			{
				Path = pDirectoryPath,
				EnableRaisingEvents = true,
				Filter = "Journal.*.log"
			};
			watcher.Created += (sender, e) =>
			{
				RunWatcher = false; //<- Deja de Leer el archivo actual (si hay)

				DirectoryInfo di = new DirectoryInfo(pDirectoryPath);

				//Busca el Archivo de Log mas reciente:
				FileInfo JournalFile = di.GetFiles("Journal.*.log")
					.OrderByDescending(f => f.LastWriteTime).First();

				if (JournalFile != null)
				{
					//Abre el archivo en modo compartido y 'Escucha' si ha sido modificado:
					RunWatcher = true;
					PlayerJournal_WatchFile(JournalFile.FullName);
				}
			};

		}
		private void PlayerJournal_WatchFile(string pFilePath)
		{
			/* LEE EL ARCHIVO DE LOG DEL JORNAL Y LO MANTIENE ABIERTO REACCIONANDO A SUS CAMBIOS  */
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				if (File.Exists(pFilePath) && WatchingFile != pFilePath)
				{
					WatchingFile = pFilePath;
					//ShowSystemNotificacion("EDHM - UI", "New Journal Log Detected!\r\n" + pFilePath);

					//TODO: Aqui deberia leer todo el archivo
					//Hay que leer todas las lineas del archivo y registrar solo la ultima nave que encuentre
					//

					var wh = new System.Threading.AutoResetEvent(false);
					var LogWatcher = new FileSystemWatcher(".")
					{
						Path = System.IO.Path.GetDirectoryName(pFilePath),  //<- Obtiene el Path: (Sin archivo ni extension)
						Filter = System.IO.Path.GetFileName(pFilePath),     //<- Nombre del Archivo con Extension (Sin Ruta)
						NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite,
						EnableRaisingEvents = true
					};
					LogWatcher.Changed += (sender, eventArgs) =>
					{
						wh.Set(); //<- Avisa que hay Cambios en el Archivo
					};

					//El archivo se abre en modo Compartido:
					var fs = new FileStream(pFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					using (var sr = new StreamReader(fs))
					{
						string Line = string.Empty;
						while (RunWatcher) //<- Poner en False para dejar de Leer el Archivo
						{
							Line = sr.ReadLine(); //<- Lee Linea x Linea
							if (Line != null)
							{
								//Analiza la Linea Buscando los Eventos deseados:
								PlayerJournal_DetectEvents(Line);
							}
							else
							{
								wh.WaitOne(1000); //<- Cuando ya no hay más lineas, pausa el proceso de lectura y Espera a que FileSystemWatcher notifique algun cambio
							}
						}
						wh.Close();
						LogWatcher.Dispose();
					}
				}
			});
		}
		private void PlayerJournal_DetectEvents(string JsonLine)
		{
			/* AQUI SE LEEN LAS LINEAS NUEVAS DEL LOG Y SE DETECTAN LOS EVENTOS DESEADOS   */
			// Esto sigue ejecutandose dentro del proceso iniciado x 'ReadPlayerJournal()'
			try
			{
				/* Cada linea del Log es un Objeto JSON completo
				 * Eventos a Detectar:
				 * 
				 * "timestamp":"2021-12-09T18:49:00Z",
				 "event":"Commander", "Name":"Blue mystic"
				 "event":"LoadGame", "Commander":"Blue mystic", "Horizons":true, "Odyssey":true, "Ship":"Python", "ShipID":6, "ShipName":"NORMANDY", "ShipIdent":"SR-03", "GameMode":"Solo", "gameversion":"4.0.0.700"
				 "event":"Loadout", "Ship":"cutter", "ShipName":"NORMANDY",  "ShipIdent":"SR-04"
				 "event":"Rank", "Combat":3, "Trade":6, "Explore":5, "Soldier":0, "Exobiologist":0, "Empire":12, "Federation":3, "CQC":0 }
				 "event":"Shutdown" 

				"event": "interdicted", "Submitted": false, "Interdictor": "Dread Pirate Roberts", "IsPlayer": false, "Faction": "Timocani Purple Posse"
				 "event":"ShipTargeted", "TargetLocked":true, "Ship":"anaconda", "ScanStage":3, "PilotName":"$cmdr_decorate:#name=ChloeDB;", "PilotName_Localised":"CMDR ChloeDB", "PilotRank":"Deadly", "SquadronID":"NERC", "ShieldHealth":100.000000, "HullHealth":100.000000, "LegalStatus":"Clean", "Power":"Aisling Duval" }
				 "event":"ShipTargeted", "TargetLocked":false }
				 "event":"UnderAttack" ...

				 "event":"FSDTarget", "Name":"Pleiades Sector GW-W c1-13", "StarClass":"K", "RemainingJumpsInRoute":1 }
				 "event":"StartJump", "JumpType":"Hyperspace", "StarSystem":"Pleiades Sector GW-W c1-13", "SystemAddress":3657130971786, "StarClass":"K" }
				 "event":"FSDJump", "StarSystem":"Pleiades Sector GW-W c1-13", "SystemAllegiance":"Independent", "SystemEconomy_Localised":"Military", "SystemSecondEconomy_Localised":"None", 
									"SystemGovernment_Localised":"Cooperative", "SystemSecurity_Localised":"Medium Security", "Population":1200000, 
									"Body":"Pleiades Sector GW-W c1-13 A", "BodyID":1, "BodyType":"Star", "JumpDist":14.956, "FuelUsed":0.154522, "FuelLevel":15.845478, 
									"Factions":[ { "Name":"Anti Xeno Initiative", "FactionState":"None", "Government":"Patronage", "Influence":0.075000, "Allegiance":"Independent", 
															"Happiness_Localised":"Happy", "MyReputation":100.000000 }, 
												 { "Name":"The Hive", "FactionState":"Boom", "Government":"Cooperative", "Influence":0.645000, "Allegiance":"Independent",  
															"Happiness_Localised":"Happy", "MyReputation":96.040001, 
															"RecoveringStates":[ { "State":"PublicHoliday", "Trend":0 } ], "ActiveStates":[ { "State":"Boom" } ] }
				 */

				//1. Buscar el Nombre del Jugador:
				int index = 0;
				index = JsonLine.IndexOf("\"event\":\"Commander\"", index);
				if (index != -1)
				{
					//Evento Detectado!
					dynamic data = Newtonsoft.Json.Linq.JObject.Parse(JsonLine);
					if (data != null)
					{
						if (CommanderName != Convert.ToString(data.Name))
						{
							CommanderName = data.Name;
							if (GreetMe)
							{
								ShowSystemNotificacion(string.Format("Welcome Commander {0}", CommanderName),
									"TIP: You can Refresh Color Changes Ingame by pressing F11 key.");
							}
						}
					}
				}

				//2. Detectar cuando se Cambia la Nave:
				index = 0;
				index = JsonLine.IndexOf("\"event\":\"Loadout\"", index);
				if (index != -1)
				{
					//Evento Detectado!
					dynamic data = Newtonsoft.Json.Linq.JObject.Parse(JsonLine);
					{
						/*	"timestamp": "2021-08-06T01:56:55Z", "event": "Loadout", "Ship": "cutter", "ShipID": 7, "ShipName": "NORMANDY", "ShipIdent": "SR-04", "HullValue": 180435872,
							"ModulesValue": 128249820, "HullHealth": 1.0, "UnladenMass": 1803.399902, "CargoCapacity": 320, "MaxJumpRange": 31.563942,   */
						if (data != null)
						{
							if (ED_Ships.IsNotEmpty())
							{
								if (CurrentShip is null)
								{
									//this.ShipIDName = string.Format("{0} ({1} {2})", Util.NVL(data.Ship, string.Empty), data.ShipName, data.ShipIdent);
									CurrentShip = ED_Ships.Find(x => x.ed_short == Convert.ToString(data.Ship));
									PlayerJournal_ShipChanged(new ship_loadout
									{
										ship_short_type = data.Ship,
										ship_name = data.ShipName,
										ship_id = data.ShipIdent
									});
								}
								else
								{
									if (this.CurrentShip.NaveCambiada(Convert.ToString(data.Ship)))
									{
										CurrentShip = ED_Ships.Find(x => x.ed_short == Convert.ToString(data.Ship));
										ShipIDName = string.Format("{0} ({1} {2})", Util.NVL(CurrentShip.ship_full_name, string.Empty), data.ShipName, data.ShipIdent);

										PlayerJournal_ShipChanged(new ship_loadout
										{
											ship_short_type = data.Ship,
											ship_name = data.ShipName,
											ship_id = data.ShipIdent
										});
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void PlayerJournal_ShipChanged(ship_loadout CurrentShip)
		{
			/* OCURRE CUANDO SE CAMBIA LA NAVE 
			    - Guarda la Nave en el Historial de Naves   
				- Si el Juego está abierto, Aplica el Tema seleccionado para la nave  
			*/
			// Esto sigue ejecutandose dentro del proceso iniciado x 'ReadPlayerJournal()'
			try
			{
				if (this.CurrentShip != null)
				{
					Invoke((MethodInvoker)(() =>
					{
						lblShipStatus.Caption = string.Format("Cmdr. {0}, Ship: {1}", CommanderName.NVL("Unknown"), this.CurrentShip.ship_full_name);
					}));


					//Registrar el ID de la nave para el CPM:
					if (ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(ActiveInstance.path, @"EDHM-ini\3rdPartyMods\CPM-@Cockpit-Paint-Mod.ini")))
					{
						IniFile _CPM_Interior = null;
						_CPM_Interior = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\3rdPartyMods\CPM-@Cockpit-Paint-Mod.ini"));
						_CPM_Interior.WriteKey("w158", this.CurrentShip.ship_id.ToString(), "constants");
					}
				}

				if (Shipyard != null)
				{
					ship_loadout MyShip = null;
					Shipyard.player_name = CommanderName;
					Shipyard.active_instance = ActiveInstance.key;

					if (Shipyard.ships == null)
					{
						Shipyard.ships = new List<ship_loadout>();
					}

					//Revisar si la Nave ya existe en el Shipyard:
					bool Existe = false;
					if (Shipyard.ships.IsNotEmpty())
					{
						MyShip = Shipyard.ships.Find(x => x.ship_short_type == CurrentShip.ship_short_type.Trim() &&
																	 x.ship_name == CurrentShip.ship_name.Trim() &&
																	   x.ship_id == CurrentShip.ship_id.Trim());
						if (MyShip != null)
						{
							Existe = true;
						}
					}

					//Si la Nave No existe la Agregamos al Historial:
					if (!Existe && WatchMe)
					{
						//ShowSystemNotificacion("EDHM - UI", "New Ship Detected!: " + CurrentShip.ship_short_type);
						Shipyard.ships.Add(CurrentShip);

						//Guarda los camnbios en el JSON:
						Util.Serialize_ToJSON(Path.Combine(AppExePath, @"Data\PlayerLoadout.json"), Shipyard);
						return;
					}

					//Verificar si el Juego esta Corriendo:						
					string GameTitle = Util.AppConfig_GetValue("GameProcessID");
					System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
					foreach (System.Diagnostics.Process process in processlist)
					{
						if (!String.IsNullOrEmpty(process.ProcessName))
						{
							if (process.MainWindowTitle == GameTitle)
							{
								GameWindow = process;
								GameIsRunning = true;
								break;
							}
						}
					}

					if (GameIsRunning && Existe) //<- Determinar si el juego está Corriendo
					{
						ApplyingShip = true;

						//Si la Nave esta registrada en el Historial y está habilitado el Cambio de Tema:
						if (Shipyard != null && Shipyard.ships.IsNotEmpty() && Shipyard.theme_swaping == true)
						{
							//La nave debe tener un tema asignado:
							if (MyShip != null && !MyShip.theme.EmptyOrNull())
							{
								//ShowSystemNotificacion("EDHM - UI", "Ship Embark Detected!: " + CurrentShip.ship_short_type);

								//Busca el Tema indicado para la Nave:
								ui_preset_new _Theme = null;
								if (MyShip.theme == "@Random Theme")
								{
									int index = 0; //Elije un tema aleatorio
									index = index.RandomBetween(1, UI_Themes.Count - 1);
									_Theme = UI_Themes[index];
								}
								else
								{
									//Elije el tema indicado:
									_Theme = UI_Themes.Find(x => x.name == MyShip.theme);
								}

								if (_Theme != null)
								{
									//Carga el Tema Indicado:
									Invoke((MethodInvoker)(() =>
									{
										SelectedTheme = _Theme;
										LoadTheme(_Theme);
										lblShipStatus.Caption = string.Format("Cmdr. {0}, Ship: {1}",
													CommanderName, ShipIDName);
										//Aplica el Tema:
										ApplyTheme(true, true);

										//Envia F11 al Juego, para Refrescar los Colores:
										if (GameWindow != null && !GameWindow.HasExited)
										{
											IntPtr h = GameWindow.MainWindowHandle;
											SetForegroundWindow(h);

											System.Threading.Thread.Sleep(3000);
											SendKeys.SendWait("{F11}");

											ApplyingShip = false;

											System.Threading.Thread.Sleep(3000); //<- Espera 3 segundos
											SendKeys.SendWait("{F11}"); //Envia la Tecla x segunda vez

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
										}

									}));
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void PlayerJournal_GetPlayerInfo()
		{
			try
			{
				string EDJournalDir = Util.WinReg_ReadKey("EDHM", "PlayerJournal").NVL("");
				if (Directory.Exists(EDJournalDir))
				{
					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
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
								string JsonLine = string.Empty;
								bool KeepReading = true;
								while (KeepReading) //<- Poner en False para dejar de Leer el Archivo
								{
									JsonLine = sr.ReadLine(); //<- Lee Linea x Linea
									if (JsonLine != null)
									{
										//Analiza la Linea Buscando los Eventos deseados:
										int index = 0;
										index = JsonLine.IndexOf("\"event\":\"LoadGame\"", index);
										/* 	"event": "LoadGame", "FID": "F5553303", "Commander": "Blue mystic", "Horizons": true, "Odyssey": true, "Ship": "Cutter", "Ship_Localised": "Imperial Cutter",
											"ShipID": 7, "ShipName": "NORMANDY", "ShipIdent": "SR-04", "FuelLevel": 64.0, "FuelCapacity": 64.0, "GameMode": "Solo", "Credits": 17540607, "Loan": 0,
											"language": "English/UK", "gameversion": "4.0.0.701", "build": "r273365/r0 " */

										if (index != -1)
										{
											//Evento Detectado!
											dynamic data = Newtonsoft.Json.Linq.JObject.Parse(JsonLine);
											if (data != null)
											{
												ShipIDName = string.Format("{0} ({1} {2})", Util.NVL(data.Ship_Localised, string.Empty), data.ShipName, data.ShipIdent);

												if (ED_Ships.IsNotEmpty())
												{
													CurrentShip = ED_Ships.Find(x => x.ed_short.ToUpper() == Convert.ToString(data.Ship.ToString().ToUpper()));
												}

												if (CommanderName != Convert.ToString(data.Commander))
												{
													CommanderName = data.Commander;
													if (GreetMe)
													{
														ShowSystemNotificacion(string.Format("Welcome Commander {0}", CommanderName),
															"TIP: You can Refresh Color Changes Ingame by pressing F11 key.");
													}
												}

												//PlayerJournal_SetUserInfo(data);
												Invoke((MethodInvoker)(() =>
												{
													lblShipStatus.Caption = string.Format("Cmdr. {0}, Ship: {1}", CommanderName.NVL("Unknown"), ShipIDName.NVL("Unknown"));
												}));
											}
											KeepReading = false;
										}
									}
								}
							}
						}
					});
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void PlayerJournal_SetUserInfo(dynamic PlayerData)
		{
			/* PlayerData:
			 * 	"event": "LoadGame", "FID": "F5553303", "Commander": "Blue Mystic", "Horizons": true, "Odyssey": true, "Ship": "Cutter", "Ship_Localised": "Imperial Cutter",
				"ShipID": 7, "ShipName": "NORMANDY", "ShipIdent": "SR-04", "FuelLevel": 64.0, "FuelCapacity": 64.0, "GameMode": "Solo", "Credits": 17540607, "Loan": 0,
				"language": "English/UK", "gameversion": "4.0.0.701", "build": "r273365/r0 " */
			try
			{
				// Enviar esta solicitud Sólo si no se ha hecho antes o si algo importante cambia
				bool ForcedUpdate = true;
				var PlayerInfo = new
				{
					CommanderName = PlayerData.Commander,
					Horizons = PlayerData.Horizons,
					Odyssey = PlayerData.Odyssey,
					Language = PlayerData.language,
					ForcedUpdate
				};
				string PlayerInfo_JSON = Newtonsoft.Json.JsonConvert.SerializeObject(PlayerInfo, Newtonsoft.Json.Formatting.None);
				string PlayerInfo_WREG = Util.WinReg_ReadKey("EDHM", "PlayerInfo").NVL(string.Empty);

				if (PlayerInfo_WREG.EmptyOrNull() || PlayerInfo_WREG != PlayerInfo_JSON || ForcedUpdate)
				{
					Util.WinReg_WriteKey("EDHM", "PlayerInfo", PlayerInfo_JSON);

					//Solicitar la Ubicacion usando la IP:
					string _Response = Util.WebRequest_GET("https://ipinfo.io/?token=d811bd45b5fcf5");
					if (_Response != null && _Response != string.Empty)
					{
						/* _Response:
						  "ip": "200.58.144.171", "hostname": "ns.cutcsa.com.uy", "city": "Montevideo", "region": "Montevideo", "country": "UY", "loc": "-34.9033,-56.1882",
						  "org": "AS19422 Telefonica Moviles del Uruguay SA", "postal": "11100", "timezone": "America/Montevideo" */

						dynamic MyIP = Newtonsoft.Json.JsonConvert.DeserializeObject(_Response);
						if (MyIP != null)
						{
							var DataToSave = new
							{
								IP = MyIP.ip,
								Country = MyIP.country,
								City = MyIP.city,
								Location = MyIP.loc,
								TimeZone = MyIP.timezone,
								Language = PlayerData.language,
								CommanderName = PlayerData.Commander,
								Horizons = PlayerData.Horizons.ToString().ToLower(),
								Odyssey = PlayerData.Odyssey.ToString().ToLower(),
								GameMode = PlayerData.GameMode,
								Date = DateTime.Today.ToString("yyyy-MM-dd")
							};
							string JSONStr = Newtonsoft.Json.JsonConvert.SerializeObject(DataToSave, Newtonsoft.Json.Formatting.None);
							//{"IP":"200.58.144.171","Country":"UY","City":"Montevideo","Location":"-34.9033,-56.1882","TimeZone":"America/Montevideo","Language":"English/UK","CommanderName":"Blue mystic","Horizons":true,"Odyssey":true,"GameMode":"Solo"}

							// Enviar los Datos mediante POST:
							string url_desa = @"http://localhost:3000/users/add";
							string url_prod = @"https://edhm-ui.herokuapp.com/users/add";
							string _Res = Util.WebRequest_POST(url_prod, JSONStr, "application/json");
							Console.WriteLine(_Res);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion

		#region Eventos Principales

		/* AQUI SE Establece la Instancia Activa:  */
		private void CboGameInstances_EditValueChanged(object sender, EventArgs e)
		{

		}
		private void CboGameInstances_HiddenEditor(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			if (CboGameInstances.EditValue != null)
			{
				var _Selected = (game_instance)(CboGameInstances.Edit as RepositoryItemLookUpEdit)
					.GetDataSourceRowByKeyValue(CboGameInstances.EditValue);

				ActiveInstance = GameInstances.Find(x => x.game_id == _Selected.game_id);

				vGridDetalles.Rows.Clear();
				//this.accordionControl1.Elements.Clear();
				Application.DoEvents();

				Util.WinReg_WriteKey("EDHM", "ActiveInstance", ActiveInstance.game_id);

				LoadGameInstance(ActiveInstance, LangShort);  //<- Carga La Instancia Activa	
				LoadMenus(LangShort);
				LoadGlobalSettings(ActiveInstance); //<- Carga los Valores Globales
				LoadThemeList_EX();

				dockManager1.ActivePanel = dockThemes;
			}
		}

		/* AQUI SE SELECCIONA UN THEMA PARA CARGAR  */
		private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
		{
			/* AL SELECCIONAR UN TEMA DE LA LISTA  */
			DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
			if (view.IsRowSelected(e.RowHandle) && e.Column.FieldName == "Preview")
			{
				ui_preset_new _Theme = view.GetRow(e.RowHandle) as ui_preset_new;
				if (_Theme != null)
				{
					SelectedTheme = _Theme;
					LoadTheme(_Theme);
				}
			}
		}

		/*  AQUI SE CARGAN LAS PROPIEDADES o elementos DEL GRUPO SELECCIONADO */
		private void tileControl1_ItemClick(object sender, TileItemEventArgs e)
		{
			e.Item.Checked = true;
			if (e.Item.Tag != null)
			{
				string _GroupName = e.Item.Tag.ToString();
				if (_GroupName == "StationsPanels")
				{
					/*	AQUI SE CARGA EL EDITOR XML  */
					if (ActiveInstance.key == "ED_Horizons")
					{
						XML_Form _Form = new XML_Form(ActiveInstance, Settings.xml_profile);
						if (_Form.ShowDialog() == DialogResult.OK)
						{
							Settings.xml_profile = _Form.xml_profile;
							PreviewTheme(true);
						}
					}
					else
					{
						XML_Form_Odyssey _Form = new XML_Form_Odyssey(ActiveInstance, Settings.xml_profile);
						if (_Form.ShowDialog() == DialogResult.OK)
						{
							Settings.xml_profile = _Form.xml_profile;
							PreviewTheme(true);
						}
					}
				}
				else /* AQUI SE CARGAN TODOS LOS DEMAS GRUPOS */
				{
					LoadGroupSettings(_GroupName);
				}
			}
		}

		/* AQUI SE CAPTURAN LOS CAMBIOS HECHOS EN LOS CONTROLES del cuadro de Propiedades */
		private void _ComboValue_EditValueChanged(object sender, EventArgs e)
		{
			try
			{
				element _SelectedElement = null;
				string _GroupName = string.Empty;

				var T = sender.GetType();
				switch (T.Name)
				{
					case "ToggleSwitch":
						ToggleSwitch _ToggleControl = sender as ToggleSwitch;
						_GroupName = _ToggleControl.Properties.Name.Split(new char[] { '|' })[0];
						if (_ToggleControl.Properties.Tag != null)
						{
							_SelectedElement = _ToggleControl.Properties.Tag as element;
							_SelectedElement.Value = Util.BoolToInteger(Util.ValidarNulo(_ToggleControl.EditValue, false));
						}
						break;

					case "LookUpEdit":
						LookUpEdit _Combo = sender as LookUpEdit;
						_GroupName = _Combo.Properties.Name.Split(new char[] { '|' })[0];
						if (_Combo.Properties.Tag != null)
						{
							_SelectedElement = _Combo.Properties.Tag as element;
							_SelectedElement.Value = Util.ValidarNulo(_Combo.EditValue, 0m);
						}
						break;

					case "TrackBarControl":
						TrackBarControl _TrackBar = sender as TrackBarControl;
						_GroupName = _TrackBar.Properties.Name.Split(new char[] { '|' })[0];
						if (_TrackBar.Properties.Tag != null)
						{
							_SelectedElement = _TrackBar.Properties.Tag as element;
							_SelectedElement.Value = Util.ValidarNulo(_TrackBar.EditValue, 0m) / 10m;
						}
						break;

					case "ColorPickEdit": // "ColorEdit":
						ColorPickEdit _ColorEd = sender as ColorPickEdit;
						_GroupName = _ColorEd.Properties.Name.Split(new char[] { '|' })[0];
						if (_ColorEd.Properties.Tag != null)
						{
							_SelectedElement = _ColorEd.Properties.Tag as element;
							if (_ColorEd.EditValue != null)
							{
								_SelectedElement.Value = _ColorEd.Color.ToArgb();
							}
						}
						break;

					case "MyColorPickEdit": // "CustomColorEdit":
						Util_Test.MyColorPickEdit _ColorEd2 = sender as Util_Test.MyColorPickEdit;
						_GroupName = _ColorEd2.Properties.Name.Split(new char[] { '|' })[0];
						if (_ColorEd2.Properties.Tag != null)
						{
							_SelectedElement = _ColorEd2.Properties.Tag as element;
							if (_ColorEd2.EditValue != null)
							{
								_SelectedElement.Value = _ColorEd2.Color.ToArgb();
							}
						}
						break;

					case "SpinEdit":
						SpinEdit _SpinEdit = sender as SpinEdit;
						_GroupName = _SpinEdit.Properties.Name.Split(new char[] { '|' })[0];
						if (_SpinEdit.Properties.Tag != null)
						{
							_SelectedElement = _SpinEdit.Properties.Tag as element;
							_SelectedElement.Value = Util.ValidarNulo(_SpinEdit.EditValue, 0m);
						}
						break;

					case "TextEdit":
						TextEdit _TextEdit = sender as TextEdit;
						_GroupName = _TextEdit.Properties.Name.Split(new char[] { '|' })[0];
						if (_TextEdit.Properties.Tag != null)
						{
							_SelectedElement = _TextEdit.Properties.Tag as element;
							_SelectedElement.Value = Util.ValidarNulo(_TextEdit.EditValue, 0m);
						}
						break;

					default:
						break;
				}

				PreviewTheme();

				/* No se nececita asignar los cambios en la variable general porque c# ya los aplica de manera implicita.
				 * Todo queda Guardado en la variable 'this.Settings' */
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/* MUESTRA LA DESCRIPCION DEL ELEMENTO SELECCIONADO en las propiedades  */
		private void vGridDetalles_FocusedRowChanged(object sender, DevExpress.XtraVerticalGrid.Events.FocusedRowChangedEventArgs e)
		{
			if (e.Row != null && e.Row.Tag != null)
			{
				try
				{
					element _Element = e.Row.Tag as element;
					if (_Element != null)
					{
						lblDescription_Caption.Text = _Element.Title;
						lblDescription_Description.Text = _Element.Description;
					}
					else
					{
						lblDescription_Caption.Text = string.Empty;
						lblDescription_Description.Text = string.Empty;
					}
				}
				catch { }
			}
		}

		#endregion

		#region Eventos de Controles


		private void mnuTray_Exit_Click(object sender, EventArgs e)
		{
			mCloseAutorized = true;
			Close();
		}
		private void mnuTray_Open_Click(object sender, EventArgs e)
		{
			Show();
			base.WindowState = FormWindowState.Normal;
			BringToFront();
		}
		private void notifyIcon1_DoubleClick(object sender, EventArgs e)
		{
			Show(); //Doble Click en el Icono del SysTray para Mostrar la Ventana
			base.WindowState = FormWindowState.Normal;
			BringToFront();
		}


		private void accordionControl1_CustomDrawElement(object sender, CustomDrawElementEventArgs e)
		{/*
			//Dibuja Borde sobre el tema seleccionado
			try
			{
				if (this.ThemeListLoaded)
				{
					//Si el elemento está seleccionado, Dibuja un borde Amarillo alrededor
					if (e.ObjectInfo.Element == this.SelectedThemeElement || this.SelectedThemeElement.OwnerElement == e.ObjectInfo.Element)
					{
						if (e.Element.Tag != null)
						{
							ui_preset_new _Theme = e.Element.Tag as ui_preset_new;
							Rectangle _Box = e.ObjectInfo.HeaderBounds; _Box.Width = 360;

							//Aqui dibuja la imagen normalmente:
							e.Cache.DrawImage(_Theme.Preview, _Box);

							//Aqui escribo el Nombre del Tema
							e.Cache.DrawString(_Theme.name, new System.Drawing.Font("Tahoma", 12, FontStyle.Bold),
								Brushes.White, _Box.X + 10, _Box.Y + 45);

							//Aqui dibuja un borde alrededor de todo el objeto:
							e.Cache.DrawRectangle(new Pen(Color.Orange), _Box);
						}
						e.Handled = true;
					}
					else
					{
						//Al pasar el Mouse, Dibuja un borde Amarillo en el lado izquierdo
						if (e.ObjectInfo.State == DevExpress.Utils.Drawing.ObjectState.Hot)
						{
							if (e.Element.Tag != null)
							{
								ui_preset_new _Theme = e.Element.Tag as ui_preset_new;
								Rectangle _Box = e.ObjectInfo.HeaderBounds; _Box.Width = 360;

								//Aqui dibuja la imagen normalmente:
								e.Cache.DrawImage(_Theme.Preview, _Box);

								//Aqui escribo el Nombre del Tema
								e.Cache.DrawString(_Theme.name, new System.Drawing.Font("Tahoma", 12, FontStyle.Bold),
									Brushes.White, _Box.X + 10, _Box.Y + 45);
							}

							//Aqui le dibuja un Borde Amarillo a la izquierda
							e.Cache.FillRectangle(Color.Orange, new Rectangle(e.ObjectInfo.HeaderBounds.Location, new Size(this.ScaleHelper.ScaleHorizontal(3),
									e.ObjectInfo.HeaderBounds.Height)));

							e.Handled = true;
						}
					}
				}
			}
			catch { }*/
		}
		private void dockTips_ClosingPanel(object sender, DevExpress.XtraBars.Docking.DockPanelCancelEventArgs e)
		{
			e.Cancel = true;
			e.Panel.HideSliding();
		}
		private void chkTips_NoShow_CheckedChanged(object sender, EventArgs e)
		{
			Util.AppConfig_SetValue("ShowTips", (!chkTips_NoShow.Checked).ToString());
		}

		/* TIPS MOSTRADOS SOBRE LOS TEMAS **/
		private void toolTipController1_GetActiveObjectInfo(object sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
		{
			//Muestra un tooltip sobre el tema al pasar el mosue
			if (e.Info == null && e.SelectedControl == gridControl1)
			{
				DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView;
				DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitInfo info = view.CalcHitInfo(e.ControlMousePosition);
				if (info.InRowCell)
				{
					ui_preset_new _theme = view.GetRow(info.RowHandle) as ui_preset_new;
					if (_theme != null)
					{
						string cellKey = info.RowHandle.ToString() + " - " + info.Column.ToString();
						e.Info = new ToolTipControlInfo(cellKey,
							string.Format("By {0}\r\n{1}", _theme.author, _theme.description),
							_theme.name);
					}
				}
			}
		}
		private void toolTipController1_HyperlinkClick(object sender, HyperlinkClickEventArgs e)
		{
			//Al dar click en un link dentro del tooltip, se abre en el Navegador x defecto:
			if (e.Link != string.Empty)
			{
				System.Diagnostics.Process.Start(e.Link);
			}
		}

		private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
		{
			//Dibuja sobre el tema seleccionaod
			DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;

			if (view.IsRowSelected(e.RowHandle) && e.Column.FieldName == "Preview")
			{
				ui_preset_new _theme = view.GetRow(e.RowHandle) as ui_preset_new;
				if (_theme != null && _theme.Preview != null)
				{
					//Aqui dibuja la imagen normalmente:
					e.Graphics.DrawImage(_theme.Preview, e.Bounds);

					//Aqui escribo el Nombre del Tema
					e.Cache.DrawString(_theme.name, new System.Drawing.Font("Tahoma", 12, FontStyle.Bold),
						Brushes.White, e.Bounds);

					e.Handled = true;
				}
			}
		}
		private void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
		{
			/* Cuando se hace un tema favorito, se crea un archivo para identificarlo  */
			ui_preset_new _theme = e.Row as ui_preset_new;
			if (_theme.IsFavorite)
			{
				Util.SaveTextFile(Path.Combine(_theme.folder, "IsFavorite.fav"),
								"** THIS THEME IS A FAVORITE **",
								Util.TextEncoding.UTF8);
			}
			else
			{
				if (File.Exists(Path.Combine(_theme.folder, "IsFavorite.fav")))
				{
					File.Delete(Path.Combine(_theme.folder, "IsFavorite.fav"));
				}
			}
		}
		private void gridView1_BeforeLeaveRow(object sender, DevExpress.XtraGrid.Views.Base.RowAllowEventArgs e)
		{
			//Previene que se cambie la fila activa hasta que se termine de cargar el tema
			if (LoadingTheme)
			{
				e.Allow = false;
			}
		}

		private void cmdThemes_ShowFavorites_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			//Filtra mostrando solo los temas favoritos
			BarToggleSwitchItem _Toggle = sender as BarToggleSwitchItem;
			if (_Toggle.Checked)
			{
				//CriteriaOperator expr1 = new BinaryOperator("IsFavorite", true);
				gridView1.ActiveFilterCriteria = new BinaryOperator("IsFavorite", true);
			}
			else
			{
				gridView1.ActiveFilterCriteria = null;
			}
			Util.WinReg_WriteKey("EDHM", "FavToggle", _Toggle.Checked.ToString());
		}

		private void repGridThemes_Favorite_EditValueChanged(object sender, EventArgs e)
		{
			gridView1.PostEditor(); //<- Actualiza el DataSource de la Grilla inmediatamente
			gridView1.UpdateCurrentRow();
		}

		#endregion

		#region Botonera

		private void cmdReloadThemes_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			LoadThemeList_EX();
		}
		private void cmdMakeNewTheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			CreateNewTheme();
		}
		private void cmdSaveTheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			SaveThemeChanges();
		}
		private void cmdShowPreview_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			/* Huevo de Pascua, sólo en Odissey  */
			if (ActiveInstance.key == "ED_Odissey")
			{
				//Rickroll:
				System.Diagnostics.Process.Start(@"https://www.youtube.com/watch?v=dQw4w9WgXcQ");

				//string PacMan = Path.Combine(this.AppExePath, @"Data\TicTacToe\TicTacToe.exe");
				//if (File.Exists(PacMan))
				//{
				//	System.Diagnostics.Process.Start(PacMan);
				//}
			}
			PreviewTheme(true);
		}
		private void cmdImportTheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			ImportTheme();
		}
		private void cmdExportTheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			ExportTheme();
		}
		private void cmdDeletetheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
			{
				if (XtraMessageBox.Show(string.Format("Are you Sure of Deleting the theme '{0}'?", SelectedTheme.name), "Confirm?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					if (Directory.Exists(SelectedTheme.folder))
					{
						DirectoryInfo dir = new DirectoryInfo(SelectedTheme.folder);
						dir.Delete(true);

						LoadThemeList_EX();
					}
				}
			}

		}
		private void cmdOpenThemeFolder_ItemClick(object sender, ItemClickEventArgs e)
		{
			try
			{
				if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
				{
					//Abrir una carpeta en el Explorador de Windows:
					System.Diagnostics.Process.Start(SelectedTheme.folder);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		#endregion

		#region Search

		private void SearchElement(string _NameFilter)
		{
			try
			{
				if (!_NameFilter.EmptyOrNull() && Settings != null && Settings.ui_groups.IsNotEmpty())
				{
					Cursor = Cursors.WaitCursor;
					var t = Task.Factory.StartNew(delegate
					{
						List<element> _Results = new List<element>();

						try
						{
							foreach (ui_group _UIGroup in Settings.ui_groups)
							{
								//Busca en los nombres de los grupos (Categorias)
								if (_UIGroup.Elements != null)
								{
									//Busca en los nombres de los elementos:
									List<element> _Elements = _UIGroup.Elements.FindAll(obj => obj.Title.ToUpper().Contains(_NameFilter.ToUpper()) ||
																							   obj.Category.ToUpper().Contains(_NameFilter.ToUpper()) ||
																							   obj.Description.ToUpper().Contains(_NameFilter.ToUpper()));
									if (_Elements.IsNotEmpty())
									{
										foreach (var _Element in _Elements)
										{
											_Element.Parent = _UIGroup.Name;
										}
										_Results.AddRange(_Elements);
									}
								}
							}

							//Busca tambien en la lista de temas:
							if (UI_Themes.IsNotEmpty())
							{
								var _ret = UI_Themes.FindAll(x => x.name.ToUpper().Contains(_NameFilter.ToUpper()) ||
																	   x.author.ToUpper().Contains(_NameFilter.ToUpper()));
								if (_ret != null)
								{
									foreach (var item in _ret)
									{
										_Results.Add(new element()
										{
											Parent = "Themes",
											Category = "Theme",
											Title = String.Format("{0} by {1}", item.name, item.author),
											File = item.name
										});
									}
								}
							}
						}
						catch (Exception ex)
						{
							XtraMessageBox.Show(ex.Message + ex.StackTrace);
						}

						//Actualiza los Controles fuera de este Proceso:	
						Invoke((MethodInvoker)(() =>
						{
							if (_Results.IsNotEmpty())
							{
								gridSearch.DataSource = _Results;
								gridView_Search.ExpandAllGroups();
							}
							else
							{
								gridSearch.DataSource = null;
							}
							Cursor = Cursors.Default;
						}));
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void repMainMenu_SearchBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				//Se Presionó la Tecla ENTER
				ButtonEdit _Edit = sender as ButtonEdit;

				txtSeach.Text = Util.NVL(_Edit.EditValue, "");
				if (panelSearch.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
				{
					panelSearch.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
				}

				panelSearch.ShowSliding();

				SearchElement(txtSeach.Text);
			}
		}
		private void repMainMenu_SearchBox_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			txtSeach.Text = Util.NVL(MainMenu_SearchBox.EditValue, "");
			if (panelSearch.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
			{
				panelSearch.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
			}

			panelSearch.ShowSliding();

			SearchElement(txtSeach.Text);
		}

		private void txtSeach_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			SearchElement(txtSeach.Text);
		}
		private void txtSeach_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				//Se Presionó la Tecla ENTER
				SearchElement(txtSeach.Text);
			}
		}

		private void gridView_Search_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
		{
			//Al seleccionar uno de los resulados, lo muestra en la ventana de Propiedades
			try
			{
				DevExpress.XtraGrid.Views.Grid.GridView View = sender as DevExpress.XtraGrid.Views.Grid.GridView;
				if (View != null)
				{
					element _Element = (element)View.GetFocusedRow();
					if (_Element != null)
					{
						if (_Element.Category == "Theme")
						{
							int rowHandle = gridView1.LocateByValue("name", _Element.File);
							if (rowHandle != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
							{
								gridView1.FocusedRowHandle = rowHandle;
								dockManager1.ActivePanel = dockThemes;
							}
						}
						else
						{
							LoadGroupSettings(_Element.Parent, _Element.Title);
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion

		#region Global Settings

		//los Global Settings se guardan automaticamnete cuando el Programa se Cierra

		private void LoadGlobalSettings(game_instance pGameInstance)
		{
			try
			{
				if (pGameInstance != null)
				{
					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

						GlobalSettings = null;
						string _File = Path.Combine(AppExePath, "Data", pGameInstance.key + "_Global_Settings.json");

						if (File.Exists(_File))
						{
							GlobalSettings = Util.DeSerialize_FromJSON<ui_group>(_File);
						}

						if (GlobalSettings != null)
						{
							//Cargar las propiedades
							try
							{
								vGridGlobalSettings.BeginUpdate();
								vGridGlobalSettings.Rows.Clear();

								foreach (element _Element in GlobalSettings.Elements)
								{
									EditorRow _Fila = new DevExpress.XtraVerticalGrid.Rows.EditorRow(_Element.Title);
									_Fila.Properties.ToolTip = _Element.Description;
									_Fila.Properties.Caption = _Element.Title;
									_Fila.Properties.FieldName = "Value";
									_Fila.Tag = _Element;

									//Valor x defecto para el brillo del texto en horizons:
									if (ActiveInstance.key == "ED_Horizons" && _Element.Key == "w151")
									{
										if (_Element.Value <= 0)
										{
											_Element.Value = 1;
										}
									}

									switch (_Element.ValueType)
									{
										case "Brightness":
											#region Mostrar una TrackBar

											//"ValueType": "Brightness",
											//"Type": "2X", <- 1X, 2X, 4X, 4X_Min

											Invoke((MethodInvoker)(() =>
											{
												RepositoryItemTrackBar _ComboValue = new RepositoryItemTrackBar
												{
													EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
													Name = string.Format("{0}|{1}", GlobalSettings.Name, _Element.Title),
													EditValueChangedDelay = 500,
													ShowValueToolTip = true,
													Tag = _Element
												};
												_ComboValue.ValueChanged += _ComboValue_EditValueChanged;

												switch (_Element.Type)
												{
													case "1X":
														_ComboValue.Maximum = 10;
														_ComboValue.Minimum = 0;
														break;

													case "2X":
														_ComboValue.Maximum = 20;
														_ComboValue.Minimum = 0;
														break;

													case "4X": //eXPANDED
														_ComboValue.Maximum = 40;
														_ComboValue.Minimum = 0;
														break;

													case "4X_Min": //sHADED
														_ComboValue.Maximum = 40;
														_ComboValue.Minimum = -40;
														break;

													default:
														_ComboValue.Maximum = 20;
														_ComboValue.Minimum = 0;
														break;
												}

												_Fila.Properties.RowEdit = _ComboValue;
												_Fila.Properties.Value = _Element.Value * 10;
											}));

											#endregion
											break;

										case "Preset":
											#region Mostrar un Combo con los Presets

											//"ValueType": "Preset",
											//"Type": "AdvancedMode", <- "{Preset_Name}"

											if (_ElementPresets.IsNotEmpty())
											{
												List<combo_item> _Presets = _ElementPresets.FindAll(x => x.Type == _Element.Type);
												if (_Presets.IsNotEmpty())
												{
													Invoke((MethodInvoker)(() =>
													{
														RepositoryItemLookUpEdit _ComboPreset = new RepositoryItemLookUpEdit()
														{
															Name = string.Format("{0}|{1}", GlobalSettings.Name, _Element.Title),
															DisplayMember = "Name",
															ValueMember = "Index",
															DataSource = _Presets,
															AllowFocused = true,
															Tag = _Element
														};
														_ComboPreset.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name"));
														_ComboPreset.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Index"));
														_ComboPreset.EditValueChanged += _ComboValue_EditValueChanged;
														_ComboPreset.AppearanceFocused.ForeColor = Color.Orange;
														_ComboPreset.BestFit();

														_Fila.Properties.RowEdit = _ComboPreset;
														_Fila.Properties.Value = _Element.Value;
													}));
												}
											}

											#endregion
											break;

										case "Color":
											#region Muestra un selector de Color

											//"ValueType": "Color",
											//"Type": "CustomColor",

											Invoke((MethodInvoker)(() =>
											{
												//RepositoryItemColorEdit _ComboColor = new RepositoryItemColorEdit()
												//{
												//	Name = string.Format("{0}|{1}", this.GlobalSettings.Name, _Element.Title),
												//	ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced,
												//	ShowMoreColorsButton = true,
												//	ShowCustomColors = true,
												//	ShowSystemColors = false,
												//	Tag = _Element
												//};
												RepositoryItemColorPickEdit _ComboColor = new RepositoryItemColorPickEdit
												{
													Name = string.Format("{0}|{1}", GlobalSettings.Name, _Element.Title),
													ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Simple,
													AutomaticColor = Color.Orange,
													ShowMoreColorsButton = true,
													ShowCustomColors = true,
													ShowSystemColors = false,
													ShowWebSafeColors = true,
													ShowWebColors = true,
													Tag = _Element
												};
												//_ComboColor.StandardColors
												_ComboColor.EditValueChanged += _ComboValue_EditValueChanged;
												_ComboColor.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
												_ComboColor.EditValueChangedDelay = 500;
												_ComboColor.ColorDialogOptions.ShowTabs = ShowTabs.RGBModel;
												_ComboColor.ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced;

												if (_RecentColors != null)
												{
													//Carga los colores usados en el tema en el cuadro 'colores Recientes'
													foreach (Color _Colour in _RecentColors)
													{
														_ComboColor.RecentColors.InsertColor(_Colour);
													}
												}

												_Fila.Properties.RowEdit = _ComboColor;
												_Fila.Properties.Value = Color.FromArgb(Util.ValidarNulo(_Element.Value, 0));
											}));

											#endregion
											break;

										case "ONOFF":
											#region Muestra un Toggle Switch:

											//"ValueType": "ONOFF",
											//"Type": "ONOFF",
											//"Value": 0,  <- 0=Off, 1=On

											Invoke((MethodInvoker)(() =>
											{
												RepositoryItemToggleSwitch _ToggleControl = new RepositoryItemToggleSwitch();
												_ToggleControl.Name = string.Format("{0}|{1}", GlobalSettings.Name, _Element.Title);
												_ToggleControl.EditValueChanged += _ComboValue_EditValueChanged;
												_ToggleControl.EditValueChangedDelay = 500;
												_ToggleControl.Tag = _Element;

												_Fila.Properties.RowEdit = _ToggleControl;
												_Fila.Properties.Value = Util.IntegerToBool(Util.ValidarNulo(_Element.Value, 0));
											}));

											#endregion
											break;

										case "Number":

											#region Muestra un SpinEdit Numerico

											//"ValueType": "Number",
											//"Type": "INT|-10|1000|n0",   <- Type|Min|Max|Mask
											//"Type": "DEC|0.0|1.0|n2",

											RepositoryItemSpinEdit _NumberItem = new RepositoryItemSpinEdit()
											{
												Name = string.Format("{0}|{1}", GlobalSettings.Name, _Element.Title),
												EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
												EditValueChangedDelay = 500,
												AllowMouseWheel = true,
												AllowFocused = true,
												Tag = _Element
											};
											_NumberItem.EditValueChanged += _ComboValue_EditValueChanged;
											_NumberItem.AppearanceFocused.ForeColor = Color.Orange;
											if (!_Element.Type.EmptyOrNull())
											{
												string[] keys = _Element.Type.Split(new char[] { '|' });
												if (keys != null && keys.Length > 0)
												{
													_NumberItem.IsFloatValue = (keys[0] == "INT" ? false : true);
													_NumberItem.MinValue = Convert.ToDecimal(keys[1]);
													_NumberItem.MaxValue = Convert.ToDecimal(keys[2]);
													_NumberItem.Increment = (keys[0] == "INT" ? 1.0m : 0.01m);
													_NumberItem.EditMask = keys[3];
													_NumberItem.UseMaskAsDisplayFormat = true;
												}
											}

											_Fila.Properties.RowEdit = _NumberItem;
											_Fila.Properties.Value = _Element.Value;

											#endregion
											break;

										default:
											#region Muestra un TextBox

											RepositoryItemTextEdit _TextItem = new RepositoryItemTextEdit()
											{
												Name = string.Format("{0}|{1}", GlobalSettings.Name, _Element.Title),
												EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
												EditValueChangedDelay = 500,
												AllowFocused = true,
												Tag = _Element
											};
											_TextItem.EditValueChanged += _ComboValue_EditValueChanged;
											_TextItem.AppearanceFocused.ForeColor = Color.Orange;

											_Fila.Properties.RowEdit = _TextItem;
											_Fila.Properties.Value = _Element.Value;

											#endregion
											break;
									}

									#region Categorias

									//Agrega la Categoria del Elemento, si ya existe, usa esa, sino, la crea nueva:
									if (!_Element.Category.EmptyOrNull())
									{
										CategoryRow Categoria = vGridGlobalSettings.Rows[_Element.Category] as CategoryRow;
										if (Categoria == null)
										{
											Categoria = new CategoryRow(_Element.Category)
											{
												Name = _Element.Category
											};
											vGridGlobalSettings.Rows.Add(Categoria);
										}
										Categoria.ChildRows.Add(_Fila);
									}
									else
									{
										vGridGlobalSettings.Rows.Add(_Fila);
									}

									//if (_Fila != null && _Element.Title == SelectRowName)
									//{
									//	Invoke((MethodInvoker)(() =>
									//	{
									//		this.vGridGlobalSettings.FocusedRow = _Fila;
									//	}));
									//}

									#endregion
								}
							}
							catch (Exception ex)
							{
								XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}
							Invoke((MethodInvoker)(() =>
							{
								vGridGlobalSettings.EndUpdate();
								panelGlobalSettings.Text = GlobalSettings.Title;
							}));
						}
						else
						{
							Invoke((MethodInvoker)(() =>
							{
								vGridGlobalSettings.Rows.Clear();
							}));
						}

						Invoke((MethodInvoker)(() =>
						{
							GlobalSettings_Title.Text = "Global Settings";
							GlobalSettings_Description.Text = "Elements in this List will have priority over the same from themes, so you can 'force' this settings to be applied no mather what theme you choose.";
						}));
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		private void ApplyGlobalSettings()
		{
			try
			{
				if (GlobalSettings != null && GlobalSettings.Elements.IsNotEmpty())
				{
					IniFile _StartupReader = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\Startup-Profile.ini"));
					IniFile _ReaderAdvanced = null;
					IniFile _ReaderOnfoot = null;

					if (ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(ActiveInstance.path, @"EDHM-ini\Advanced.ini")))
					{
						_ReaderAdvanced = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\Advanced.ini"));
					}
					if (ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(ActiveInstance.path, @"EDHM-ini\SuitHud.ini")))
					{
						_ReaderOnfoot = new IniFile(Path.Combine(ActiveInstance.path, @"EDHM-ini\SuitHud.ini"));
					}

					foreach (var _Element in GlobalSettings.Elements)
					{
						if (_Element.ValueType == "Color")
						{
							string[] keys = _Element.Key.Split(new char[] { '|' });
							if (keys != null && keys.Length > 0)
							{
								//Convertir Color RGB -> sRGB -> GammaCorrected
								Color _Color = Color.FromArgb(Convert.ToInt32(_Element.Value));
								List<double> _GammaColors = GetGammaCorrected_RGBA(_Color);

								int i = 0;
								foreach (string _Key in keys)
								{
									switch (_Element.File)
									{
										case "Advanced": _ReaderAdvanced.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;
										case "SuitHud": _ReaderOnfoot.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;

										default: _StartupReader.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;
									}
									i++;
								}
							}
						}
						else //<- ValueType = 'Preset', 'Decimal', etc
						{
							switch (_Element.File)
							{
								case "Advanced": _ReaderAdvanced.WriteKey(_Element.Key, _Element.Value.ToString(), _Element.Section); break;
								case "SuitHud": _ReaderOnfoot.WriteKey(_Element.Key, _Element.Value.ToString(), _Element.Section); break;

								default: _StartupReader.WriteKey(_Element.Key, _Element.Value.NVL("0"), _Element.Section); break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		private void vGridDetalles_Click(object sender, EventArgs e)
		{

		}
		private void vGridDetalles_MouseDown(object sender, MouseEventArgs e)
		{
			//Selecciona la Fila al dar Click Derecho
			if (e.Button == MouseButtons.Right)
			{
				DevExpress.XtraVerticalGrid.VGridHitInfo hInfo = vGridDetalles.CalcHitInfo(new Point(e.X, e.Y));
				if (hInfo != null && hInfo.HitInfoType == DevExpress.XtraVerticalGrid.HitInfoTypeEnum.HeaderCell)
				{
					vGridDetalles.FocusedRow = hInfo.Row;
					vGridDetalles.FocusedRecordCellIndex = hInfo.CellIndex;
					vGridDetalles.FocusedRecord = hInfo.RecordIndex;
					vGridDetalles.ShowEditor();
				}
			}
		}

		private void mnuGlobalSettings_Add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				var _Selected = vGridDetalles.FocusedRow;
				if (_Selected != null)
				{
					element _Element = (element)_Selected.Tag;
					if (_Element != null)
					{
						if (XtraMessageBox.Show(string.Format("You want to add '{0}' to the Global Settings List?", _Element.Title),
							"Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							if (GlobalSettings is null)
							{
								GlobalSettings = new ui_group("GlobalSettings", "Global Settings");
							}

							element _Existe = null;
							if (GlobalSettings.Elements.IsNotEmpty())
							{
								_Existe = GlobalSettings.Elements.Find(x => x.Key == _Element.Key);
							}

							if (_Existe is null)
							{
								if (GlobalSettings.Elements is null)
								{
									GlobalSettings.Elements = new List<element>();
								}

								GlobalSettings.Elements.Add(_Element);

								//Guarda los datos en el JSON:
								if (GlobalSettings != null)
								{
									Util.Serialize_ToJSON(Path.Combine(AppExePath, "Data", ActiveInstance.key + "_Global_Settings.json"), GlobalSettings);

									LoadGlobalSettings(ActiveInstance);
									dockManager1.ActivePanel = dockGlobalSettings;
								}
							}
							else
							{
								XtraMessageBox.Show("That Element is already on the Global Settings List", "Nope", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
		private void mnuGlobalSettings_Remove_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				var _Selected = vGridGlobalSettings.FocusedRow;
				if (_Selected != null)
				{
					element _Element = (element)_Selected.Tag;
					if (_Element != null)
					{
						if (XtraMessageBox.Show(string.Format("You want to Remove '{0}' from the Global Settings List?", _Element.Title),
							"Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							if (GlobalSettings != null)
							{
								GlobalSettings.Elements.Remove(_Element);

								Util.Serialize_ToJSON(Path.Combine(AppExePath, "Data", ActiveInstance.key + "_Global_Settings.json"), GlobalSettings);

								LoadGlobalSettings(ActiveInstance);
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

		private void vGridGlobalSettings_FocusedRowChanged(object sender, DevExpress.XtraVerticalGrid.Events.FocusedRowChangedEventArgs e)
		{
			/* MUESTRA LA DESCRIPCION DEL ELEMENTO SELECCIONADO en las propiedades  */
			if (e.Row != null && e.Row.Tag != null)
			{
				element _Element = e.Row.Tag as element;
				if (_Element != null)
				{
					GlobalSettings_Title.Text = _Element.Title;
					GlobalSettings_Description.Text = _Element.Description;
				}
				else
				{
					GlobalSettings_Title.Text = string.Empty;
					GlobalSettings_Description.Text = string.Empty;
				}
			}
		}
		private void vGridGlobalSettings_MouseDown(object sender, MouseEventArgs e)
		{
			//Selecciona la Fila al dar Click Derecho
			if (e.Button == MouseButtons.Right)
			{
				DevExpress.XtraVerticalGrid.VGridHitInfo hInfo = vGridGlobalSettings.CalcHitInfo(new Point(e.X, e.Y));
				if (hInfo != null && hInfo.HitInfoType == DevExpress.XtraVerticalGrid.HitInfoTypeEnum.HeaderCell)
				{
					vGridGlobalSettings.FocusedRow = hInfo.Row;
					vGridGlobalSettings.FocusedRecordCellIndex = hInfo.CellIndex;
					vGridGlobalSettings.FocusedRecord = hInfo.RecordIndex;
					vGridGlobalSettings.ShowEditor();
				}
			}
		}


		#endregion

		#region Theme Context Menus

		private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
		{
			DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
			DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitInfo hitInfo = view.CalcHitInfo(e.Point);
			if (hitInfo.InRowCell)
			{
				view.FocusedRowHandle = hitInfo.RowHandle;
				view.SelectRow(hitInfo.RowHandle);

				if (view.IsRowSelected(hitInfo.RowHandle) && hitInfo.Column.FieldName == "Preview")
				{
					ui_preset_new _Theme = view.GetRow(hitInfo.RowHandle) as ui_preset_new;
					if (_Theme != null)
					{
						SelectedTheme = _Theme;
						LoadTheme(_Theme);

						//Desactiva el menu si no tiene Preview:
						mnuTheme_Preview.Enabled = !_Theme.BigPreview.EmptyOrNull();
					}
				}

				var column = hitInfo.Column;
				popupMenu_Themes.ShowPopup(barManager1, view.GridControl.PointToScreen(e.Point));
			}
		}
		private void mnuTheme_Apply_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
			{
				ApplyTheme(true);
			}
		}
		private void mnuTheme_Rename_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
			{
				string _ThemeName = XtraInputBox.Show("Type a Name for your Custom Theme:\r\n** If Exists, it will be Overwritten!",
										"Create New Theme", SelectedTheme.name);
				if (!_ThemeName.EmptyOrNull())
				{
					Directory.Move(SelectedTheme.folder,
										Path.Combine(System.IO.Directory.GetParent(SelectedTheme.folder).FullName, _ThemeName)
									);
					LoadThemeList_EX();
				}
			}
		}
		private void mnuTheme_Delete_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
			{
				if (XtraMessageBox.Show(string.Format("Are you Sure of Deleting the theme '{0}'?", SelectedTheme.name), "Confirm?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					if (Directory.Exists(SelectedTheme.folder))
					{
						DirectoryInfo dir = new DirectoryInfo(SelectedTheme.folder);
						dir.Delete(true);

						LoadThemeList_EX();
					}
				}
			}
		}
		private void mnuTheme_Export_ItemClick(object sender, ItemClickEventArgs e)
		{
			ExportTheme();
		}
		private void mnuTheme_MakeNew_ItemClick(object sender, ItemClickEventArgs e)
		{
			CreateNewTheme();
		}
		private void mnuTheme_OpenFolder_ItemClick(object sender, ItemClickEventArgs e)
		{
			try
			{
				if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
				{
					//Abrir una carpeta en el Explorador de Windows:
					System.Diagnostics.Process.Start(SelectedTheme.folder);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void mnuTheme_Save_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
			{
				SaveThemeChanges();
			}
		}
		private void mnuTheme_Thumbnail_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
			{
				System.Diagnostics.Process.Start(
															Path.Combine(AppExePath, "EDHM_UI_Thumbnail_Maker.exe"),
															"\"" + SelectedTheme.folder + "\"");
			}
		}
		private void mnuTheme_Preview_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (SelectedTheme != null && SelectedTheme.name != "Current Settings")
			{
				if (!SelectedTheme.BigPreview.EmptyOrNull())
				{
					System.Diagnostics.Process.Start(SelectedTheme.BigPreview);
				}
			}
		}

		#endregion

		#region Main Menu

		private void MainMenu_Settings_ItemClick(object sender, ItemClickEventArgs e)
		{
			//Opens the Settings Window:
			GameFolderForm _Form = new GameFolderForm(GameInstancesEx);
			if (_Form.ShowDialog() == DialogResult.OK)
			{
				this.GameInstancesEx = _Form.GameInstancesEx;

				string _RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");
				string GameInstances_JSON = Util.Serialize_ToJSON(_Form.GameInstancesEx); // Util.WinReg_ReadKey("EDHM", "GameInstances").NVL(string.Empty);

				if (GameInstancesEx != null && GameInstancesEx.Count > 0)
				{
					foreach (var _instance in GameInstancesEx)
					{
						foreach (var _game in _instance.games)
						{
							if (_game.game_id.EmptyOrNull())
							{
								_game.game_id = string.Format("{0}|{1}", _instance.instance, _game.key);
							}
						}
					}
				}

				//Carga los valores que se muestran en el Combo:
				GameInstances = new List<game_instance>();
				foreach (var _Instance in GameInstancesEx)
				{
					foreach (var _Game in _Instance.games)
					{
						GameInstances.Add(new game_instance()
						{
							instance = _Game.instance,
							game_id = _Game.game_id,
							key = _Game.key,
							name = _Game.name,
							path = _Game.path,
							is_active = _Game.is_active,
							themes_folder = _Game.themes_folder
						});
					}
				}

				string[] _ActiveGames = _RegActiveInstance.Split(new char[] { '|' });
				if (_ActiveGames != null && _ActiveGames.Length > 1)
				{
					ActiveInstance = GameInstances.Find(x => x.game_id == _RegActiveInstance);
				}
				else
				{
					_RegActiveInstance = string.Format("{0}|{1}", GameInstancesEx[0].instance, _RegActiveInstance.NVL("ED_Horizons"));
					ActiveInstance = GameInstances.Find(x => x.game_id == _RegActiveInstance);
					Util.WinReg_WriteKey("EDHM", "ActiveInstance", _RegActiveInstance);
				}

				repCboGameInstances.ValueMember = "game_id";
				repCboGameInstances.DisplayMember = "instance";
				repCboGameInstances.DataSource = GameInstances;


				//this.ActiveInstance = this.GameInstances.Find(x => x.key == _RegActiveInstance);
				if (ActiveInstance != null)
				{
					CboGameInstances.EditValue = ActiveInstance.game_id;

					//Carga el Idioma del Usuario:
					LangShort = Util.WinReg_ReadKey("EDHM", "Language").NVL("en");

					LoadGameInstance(ActiveInstance, LangShort);  //<- Carga La Instancia Activa	
					LoadThemeList_EX(); //<- Cargar la Lista de Temas disponibles
										//ReadPlayerJournal();
				}

				HideToTray = _Form.HideToTray;
				GreetMe = _Form.GreetMe;
				WatchMe = _Form.WatchMe;
				SavesToRemember = _Form.SavesToRemember;
				AutoApplyTheme = _Form.AutoApplyTheme;

				LoadMenus(LangShort);

				History_LoadElements(SavesToRemember);
			}
		}
		private void MainMenu_GameFolder_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (ActiveInstance != null && !ActiveInstance.path.EmptyOrNull())
			{
				//Abrir una carpeta en el Explorador de Windows:
				System.Diagnostics.Process.Start(ActiveInstance.path);
			}
		}
		private void MainMenu_Sipyard_ItemClick(object sender, ItemClickEventArgs e)
		{
			/*  MUESTRA EL HISTORIAL DE NAVES Y SUS TEMAS ASOCIADOS  */
			ShipyardForm _Form = new ShipyardForm
			{
				ActiveInstance = ActiveInstance,
				UI_Themes = (List<ui_preset_new>)UI_Themes.Clone(),
				Shipyard = Shipyard
			};
			if (_Form.ShowDialog() == DialogResult.OK)
			{
				Shipyard = _Form.Shipyard;
			}
		}

		private void MainMenu_InstallMod_ItemClick(object sender, ItemClickEventArgs e)
		{
			SilentUpdate = false;

			KillGameProcces();
			InstallGameInstance(ActiveInstance);

			//Cargar Los Valores Base de La Instancia:
			string JsonSettings_path = Path.Combine(AppExePath, "Data", ActiveInstance.key + string.Format("_Settings_{0}.json", LangShort.ToUpper()));
			if (File.Exists(JsonSettings_path))
			{
				Settings = Util.DeSerialize_FromJSON<ui_setting>(JsonSettings_path);
			}

			//Carga la Lista de Presets disponibles:
			if (Settings != null && Settings.Presets.IsNotEmpty())
			{
				_ElementPresets = Settings.Presets;
			}
			DefaultSettings = Load_DefaultTheme(ActiveInstance, LangShort);

			//Carga la Lista de Temas
			LoadThemeList_EX();

			string search = ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
			lblVersion_App.Caption = string.Format("App Version: {0}", System.Configuration.ConfigurationManager.AppSettings["AppVersion"].ToString());
			lblVersion_MOD.Caption = string.Format("Mod Version: {0}", Util.WinReg_ReadKey("EDHM", string.Format("Version_{0}", search)).NVL("v1.51"));
		}
		private void MainMenu_UninstallMod_ItemClick(object sender, ItemClickEventArgs e)
		{
			UninstallEDHMmod();
		}
		private void MainMenu_UpdateCheck_ItemClick(object sender, ItemClickEventArgs e)
		{
			CheckForModUpdates(false);
		}

		private void MainMenu_Plugins_ItemClick(object sender, ItemClickEventArgs e)
		{
			try
			{
				TPModsManager _Form = new TPModsManager(ActiveInstance);
				_Form.Show();
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void MainMenu_Help_ItemClick(object sender, ItemClickEventArgs e)
		{
			//abre con el programa predeterminado: 
			/*
			if (File.Exists(Path.Combine(AppExePath, @"Data\EDHM_UI_Guide.pdf")))
			{
				System.Diagnostics.Process.Start(Path.Combine(AppExePath, @"Data\EDHM_UI_Guide.pdf"));
			}*/
			System.Diagnostics.Process.Start("https://edhm-ui.herokuapp.com/");
			//Load_UITips(true);
		}
		private void MainMenu_About_ItemClick(object sender, ItemClickEventArgs e)
		{
			AboutForm _form = new AboutForm();
			_form.ShowDialog();
		}

		private void MainMenu_Themes_ItemClick(object sender, ItemClickEventArgs e)
		{
			dockManager1.ActivePanel = dockThemes;
		}
		private void MainMenu_ApplyTheme_ItemClick(object sender, ItemClickEventArgs e)
		{
			ApplyTheme(true);
		}

		#endregion

		private void progressPanel1_Click(object sender, EventArgs e)
		{

		}


	}

	public class RedColorComparer : IComparer<Color>
	{
		public int Compare(Color a, Color b)
		{
			if (a.R < b.R)
			{
				return 1;
			}
			else if (a.R == b.R)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
	}
	public class XColorComparer : IComparer<Color>
	{
		public int Compare(Color x, Color y)
		{
			// local variables
			Color cx, cy;
			float hx, hy, sx, sy, bx, by;

			// get Color values
			cx = (new SolidBrush(x)).Color;
			cy = (new SolidBrush(y)).Color;
			// get saturation values
			sx = cx.GetSaturation();
			sy = cy.GetSaturation();
			// get hue values
			hx = cx.GetHue();
			hy = cy.GetHue();
			// get brightness values
			bx = cx.GetBrightness();
			by = cy.GetBrightness();

			// determine order
			// 1 : hue       
			if (hx < hy)
			{
				return -1;
			}
			else if (hx > hy)
			{
				return 1;
			}
			else
			{
				// 2 : saturation
				if (sx < sy)
				{
					return -1;
				}
				else if (sx > sy)
				{
					return 1;
				}
				else
				{
					// 3 : brightness
					if (bx < by)
					{
						return -1;
					}
					else if (bx > by)
					{
						return 1;
					}
					else
					{
						return 0;
					}
				}
			}
		}
	}
}
