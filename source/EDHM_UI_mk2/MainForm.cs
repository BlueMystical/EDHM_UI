using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
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
using DevExpress.Data.Filtering;

namespace EDHM_UI_mk2
{
	/*  EDHM UI - Autor: Ing. Jhollman Chacon R. (Blue Mystic) @ 2021 */
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

		private DateTime LastCheckDate = DateTime.MinValue;

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private string CommanderName = string.Empty;
		private string ShipIDName = string.Empty;
		private string WatchingFile = string.Empty;
		private string LangShort = "en"; //<- Idioma x Defecto


		private List<ui_preset_new> UI_Themes = null;
		private ui_setting Settings = new ui_setting();
		private HashSet<Color> _RecentColors = new HashSet<Color>();            //<- Guarda los Colores del Tema, no admite repetidos
		private List<combo_item> _ElementPresets = new List<combo_item>();

		[Flags]
		public enum StatusFlags
		{
			Docked_Landing_pad = 1,
			Landed_Planet_surface = 2,
			Landing_Gear_Down = 4,
			Shields_Up = 8,
			Supercruise = 16,
			FlightAssist_Off = 32,
			Hardpoints_Deployed = 64,
			In_Wing = 128,
			LightsOn = 256,
			Cargo_Scoop_Deployed = 512,
			Silent_Running = 1024,
			Scooping_Fuel = 2048,
			Srv_Handbrake = 4096,
			Srv_Turret_View = 8192,
			Srv_Turret_Retracted = 16384,
			Srv_DriveAssist = 32768,
			Fsd_MassLocked = 65536,
			Fsd_Charging = 131072,
			Fsd_Cooldown = 262144,
			Low_Fuel = 524288,
			Over_Heating = 1048576,
			Has_Lat_Long = 2097152,
			IsInDanger = 4194304,
			Being_Interdicted = 8388608,
			In_MainShip = 16777216,
			In_Fighter = 33554432,
			In_SRV = 67108864,
			Hud_Analysis_Mode = 134217728,
			Night_Vision_ON = 268435456,
			Altitude_Average_Radius = 536870912
			//fsd_jump = 1073741824‬‬
		};

		//Srv_HighBeam = 2147483648
		//https://elite-journal.readthedocs.io/en/latest/Status%20File/
		//https://stackoverflow.com/questions/27294690/decoding-a-bitmask-from-a-value-in-c-sharp

		#endregion

		#region Constructores y Eventos de la Ventana

		public MainForm()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
		}
		public MainForm(string[] args)
		{
			InitializeComponent();
			this.DoubleBuffered = true;

			//Leer los argumentos pasados por linea de comandos:
			if (args != null && args.Length > 0)
			{
				this.StartHidden = args[0] == "-hidden" ? true : false;
				if (args[0] == "-update")
				{
					this.DoUpdate = true;
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
			this.LangShort = Util.WinReg_ReadKey("EDHM", "Language").NVL("en");
			this.WatchMe = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "WatchMe").NVL("true"));
			this.GreetMe = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "GreetMe").NVL("true"));
			this.HideToTray = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "HideToTray").NVL("false"));
			this.ShowTips = Convert.ToBoolean(Util.AppConfig_GetValue("ShowTips").NVL("true"));
			this.chkTips_NoShow.Checked = !this.ShowTips;
			this.cmdThemes_ShowFavorites.Checked = Convert.ToBoolean(Util.WinReg_ReadKey("EDHM", "FavToogle").NVL("false"));

			#endregion

			#region Cargar las Instancias del Juego

			string _RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");
			string GameInstances_JSON = Util.WinReg_ReadKey("EDHM", "GameInstances").NVL(string.Empty);

			if (!GameInstances_JSON.EmptyOrNull())
			{
				this.GameInstancesEx = Util.DeSerialize_FromJSON_String<List<GameInstance>>(GameInstances_JSON);
				if (this.GameInstancesEx != null && this.GameInstancesEx.Count > 0)
				{
					foreach (var _instance in this.GameInstancesEx)
					{
						foreach (var _game in _instance.games)
						{
							if (_game.game_id.EmptyOrNull())
							{
								_game.game_id = string.Format("{0}|{1}", _instance.instance, _game.key);
							}
						}
					}
					GameInstances_JSON = Util.Serialize_ToJSON(this.GameInstancesEx);
					Util.WinReg_WriteKey("EDHM", "GameInstances", GameInstances_JSON);
				}
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
					instance  = "Horizons (Default)",
					game_id = "Default|ED_Horizons",
					themes_folder = @"EDHM-ini\DemoProfiles",
					path = Util.WinReg_ReadKey("EDHM", "ED_Horizons").NVL(string.Empty),
					is_active = Util.IIf(_RegActiveInstance == "ED_Horizons", true, false)
				});
				this.GameInstancesEx[0].games.Add(new game_instance()
				{
					key = "ED_Odissey",
					name = "Odyssey",
					instance = "Odyssey (Default)",
					game_id = "Default|ED_Odissey",
					themes_folder = @"EDHM-ini\MyProfiles",
					path = Util.WinReg_ReadKey("EDHM", "ED_Odissey").NVL(string.Empty),
					is_active = Util.IIf(_RegActiveInstance == "ED_Odissey", true, false)
				});

				GameInstances_JSON = Util.Serialize_ToJSON(this.GameInstancesEx);
				Util.WinReg_WriteKey("EDHM", "GameInstances", GameInstances_JSON);
			}

			//Carga los valores que se muestran en el Combo:
			this.GameInstances = new List<game_instance>();
			foreach (var _Instance in this.GameInstancesEx)
			{
				foreach (var _Game in _Instance.games)
				{
					_Game.instance = string.Format("{0} ({1})", _Game.name, _Instance.instance);
					this.GameInstances.Add(_Game);
				}
			}

			this.repCboGameInstances.ValueMember = "game_id";
			this.repCboGameInstances.DisplayMember = "instance";
			this.repCboGameInstances.DataSource = this.GameInstances;			

			#endregion

			#region Carga el Historial de Naves

			string JsonShipyardPath = Path.Combine(this.AppExePath, @"Data\PlayerLoadout.json");
			if (File.Exists(JsonShipyardPath))
			{
				this.Shipyard = Util.DeSerialize_FromJSON<player_loadout>(JsonShipyardPath);
			}
			else
			{
				this.Shipyard = new player_loadout();
			}

			#endregion

			#region Seleccionar la Instancia Activa

			string[] _ActiveGames = _RegActiveInstance.Split(new char[] { '|' });
			if (_ActiveGames != null && _ActiveGames.Length > 1)
			{
				this.ActiveInstance = this.GameInstances.Find(x => x.game_id == _RegActiveInstance);
			}
			else
			{
				_RegActiveInstance = string.Format("{0}|{1}", this.GameInstancesEx[0].instance, _RegActiveInstance.NVL("ED_Horizons"));
				this.ActiveInstance = this.GameInstances.Find(x => x.game_id == _RegActiveInstance);
				Util.WinReg_WriteKey("EDHM", "ActiveInstance", _RegActiveInstance);
			}
			
			if (this.ActiveInstance != null)
			{
				this.CboGameInstances.EditValue = this.ActiveInstance.game_id;

				string search = this.ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";

				this.lblVersion_App.Caption = string.Format("App Version: {0}", System.Configuration.ConfigurationManager.AppSettings["AppVersion"].ToString());
				this.lblVersion_MOD.Caption = string.Format("Mod Version: {0}", Util.WinReg_ReadKey("EDHM", string.Format("Version_{0}", search)).NVL("v1.51"));
			}

			#endregion

			//this.Location = new Point(0, 0);
		}
		private void MainForm_Shown(object sender, EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if (this.StartHidden)
				{
					Hide();
					this.notifyIcon1.Visible = true;
				}

				bool FirstRun = Convert.ToBoolean(Util.AppConfig_GetValue("FirstRun"));
				if (FirstRun || this.DoUpdate)
				{
					//Si es la primera vez, se instala el MOD
					this.SilentUpdate = true;

					if (this.GameInstances != null)
					{
						foreach (var _Instance in this.GameInstances)
						{
							if (!_Instance.path.EmptyOrNull())
							{
								InstallGameInstance(_Instance);
							}
						}
					}
					
					Run_HotFix();

					Util.AppConfig_SetValue("FirstRun", "false");
				}

				CheckForModUpdates();
				LoadGameInstance(this.ActiveInstance, this.LangShort);  //<- Carga La Instancia Activa, tambien verifica si el MOD esta instalado	
				//LoadThemeList(); //<- Cargar la Lista de Temas disponibles	
				LoadThemeList_EX();
				LoadGlobalSettings(this.ActiveInstance);

				string search = this.ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
				this.lblVersion_App.Caption = string.Format("App Version: {0}", System.Configuration.ConfigurationManager.AppSettings["AppVersion"].ToString());
				this.lblVersion_MOD.Caption = string.Format("Mod Version: {0}", Util.WinReg_ReadKey("EDHM", string.Format("Version_{0}", search)).NVL("v1.51"));

				Load_UITips();

				ReadPlayerJournal();
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			//Al intentar cerrar la ventana se minimiza en la bandeja 'SysTray'
			if (e.CloseReason == CloseReason.UserClosing)
			{
				if (!this.mCloseAutorized && this.HideToTray)
				{
					e.Cancel = true;
					Hide();
					this.notifyIcon1.Visible = true;

					ShowSystemNotificacion("EDHM - UI", "Will be running in the background.");
				}
				else
				{
					try
					{
						//Guarda los Global Settings:
						//if (this.GlobalSettings != null)
						//{
						//	string _TemplatesPath = Path.Combine(this.AppExePath, "Data");
						//	string _File = Path.Combine(_TemplatesPath, this.ActiveInstance.key + "_Global_Settings.json");

						//	Util.Serialize_ToJSON(_File, this.GlobalSettings);
						//}

						//Aqui se cierra normalmente:
						if (this.notifyIcon1 != null)
						{
							this.notifyIcon1.Visible = false;
							this.notifyIcon1.Icon.Dispose();
							this.notifyIcon1.Dispose();
						}
						if (this.GameWindow != null) this.GameWindow.Dispose();
					}
					catch { }
				}
			}
		}
		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			//Al Cerrar definitivamente el Formulario elimina el icono del SysTray
			if (this.notifyIcon1 != null)
			{
				this.notifyIcon1.Visible = false;
				if (this.notifyIcon1.Icon != null) this.notifyIcon1.Icon.Dispose();
				this.notifyIcon1.Dispose();
			}
			if (this.GameWindow != null) this.GameWindow.Dispose();
		}
		private void MainForm_Resize(object sender, EventArgs e)
		{
			//Al Minimizar la Ventana, se oculta y se minimiza en la bandeja 'SysTray'
			if (this.WindowState == FormWindowState.Minimized)
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

		bool LoadingGameInstance = false;
		private void LoadGameInstance(game_instance pGameInstance, string pLang = "en")
		{
			/* AQUI SE CARGA LA CONFIGURACION ACTUAL */
			try
			{
				if (this.LoadingGameInstance) return;
				this.LoadingGameInstance = true;

				//Verificar si La instancia está Seteada:
				if (this.ActiveInstance == null || this.ActiveInstance.path.EmptyOrNull() || this.ActiveInstance.path == "[NOT_FOUND]")
				{
					//La Ruta del Juego NO ha sido Establecida!
					GameFolderForm _Form = new GameFolderForm(this.GameInstancesEx);
					if (_Form.ShowDialog() == DialogResult.OK)
					{
						this.GameInstancesEx = _Form.GameInstancesEx;
						this.ActiveInstance = this.GameInstancesEx[0].games[0]; //<- Horizons de la Primera instancia
						if (this.ActiveInstance != null)
						{
							this.CboGameInstances.EditValue = this.ActiveInstance.key;
						}

						if (XtraMessageBox.Show("Would you like to Update your EDHM?", "UPDATE?",
							MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							InstallGameInstance(this.ActiveInstance);
							LoadThemeList_EX();
						}
					}
				}

				//Verificar si el MOD está Instalado:
				if (!this.ActiveInstance.path.EmptyOrNull())
				{
					string gamekey = Path.Combine(this.ActiveInstance.path, "d3dx.ini");
					if (!File.Exists(gamekey))
					{
						//El MOD no está Instalado.
						InstallGameInstance(this.ActiveInstance);
						LoadThemeList_EX();
					}
				}

				//Cargar Los Valores Base de La Instancia
				Inicio:
				string JsonSettings_path = Path.Combine(this.AppExePath, "Data", pGameInstance.key + string.Format("_Settings_{0}.json", pLang.ToUpper()));
				if (File.Exists(JsonSettings_path))
				{
					this.Settings = Util.DeSerialize_FromJSON<ui_setting>(JsonSettings_path);

					//Carga la Lista de Presets disponibles:
					if (this.Settings != null && this.Settings.Presets.IsNotEmpty())
					{
						this._ElementPresets = this.Settings.Presets;
					}

					this.DefaultSettings = Load_DefaultTheme(pGameInstance, pLang);

					//Carga los Titulos de cada Cuadro (Tile):
					if (this.Settings != null && this.Settings.ui_groups.IsNotEmpty())
					{
						ui_group _UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tilePanel_UP.Tag.ToString());
						if (_UIGroup != null) this.tilePanel_UP.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileSettings.Tag.ToString());
						if (_UIGroup != null) this.tileSettings.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileSpecialFX.Tag.ToString());
						if (_UIGroup != null) this.tileSpecialFX.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tilePanel_Lower.Tag.ToString());
						if (_UIGroup != null) this.tilePanel_Lower.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileLighting.Tag.ToString());
						if (_UIGroup != null) this.tileLighting.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileReticle.Tag.ToString());
						if (_UIGroup != null) this.tileReticle.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileRadarHUD.Tag.ToString());
						if (_UIGroup != null) this.tileRadarHUD.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileCHUD_AHUD.Tag.ToString());
						if (_UIGroup != null) this.tileCHUD_AHUD.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileShieldsHolo.Tag.ToString());
						if (_UIGroup != null) this.tileShieldsHolo.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileStationsPanels.Tag.ToString());
						if (_UIGroup != null) this.tileStationsPanels.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileIPower_Distributor.Tag.ToString());
						if (_UIGroup != null) this.tileIPower_Distributor.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileFuelBars.Tag.ToString());
						if (_UIGroup != null) this.tileFuelBars.Text = _UIGroup.Title;

						_UIGroup = this.Settings.ui_groups.Find(x => x.Name == this.tileFoot_HUD.Tag.ToString());
						if (_UIGroup != null) this.tileFoot_HUD.Text = _UIGroup.Title;
					}
				}
				else
				{
					XtraMessageBox.Show(string.Format("The language file '{0}' could not be found!\r\nDefault EN will be loaded.", JsonSettings_path),
						"ERROR 404: NOT FOUND", MessageBoxButtons.OK, MessageBoxIcon.Error);

					pLang = "en";
					goto Inicio;
				}

				if (this.ActiveInstance.key == "ED_Odissey" && this.PreviewForm != null && this.PreviewForm.Visible)
				{
					this.PreviewForm.Close();
				}

				this.LoadingGameInstance = false;

				string search = this.ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
				this.lblVersion_MOD.Caption = string.Format("Mod Version: {0}",
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
				string update_path = Path.Combine(this.AppExePath, "Updates");
				string search = pGameInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";

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
							if (!Directory.Exists(@"C:\Temp")) Directory.CreateDirectory(@"C:\Temp");
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

							XtraMessageBox.Show(string.Format("EDHM Version '{0}' had been Installed!\r\n{1}", 
										_Version, pGameInstance.instance), "Success!",
										MessageBoxButtons.OK, MessageBoxIcon.Information);

							//Abre el archivo de Ayuda:
							//if (File.Exists(Path.Combine(this.AppExePath, @"Data\EDHM_UI_Guide.pdf")))
							//{
							//	System.Diagnostics.Process.Start(Path.Combine(this.AppExePath, @"Data\EDHM_UI_Guide.pdf"));
							//}
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
				XDocument xmlFile;

				#region Set the 'DisableGuiEffects'

				if (File.Exists(Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Settings.xml")))
				{
					xmlFile = System.Xml.Linq.XDocument.Load(
							Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Settings.xml"));

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

					xmlFile.Save(Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Settings.xml"));
				}

				#endregion

				#region MaterialQuality

				//if (File.Exists(Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Custom.4.0.fxcfg")))
				//{
				//	xmlFile = System.Xml.Linq.XDocument.Load(
				//			Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Custom.4.0.fxcfg"));

				//	var query2 = from c in xmlFile.Root.Elements("MaterialQuality")
				//				 select c;
				//	foreach (XElement element in query2)
				//	{
				//		element.Value = "3";
				//	}
				//	xmlFile.Save(Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\Custom.4.0.fxcfg"));
				//}

				#endregion

				#region Set the XML Colors to the Default Identity Matrix

				if (File.Exists(Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\GraphicsConfigurationOverride.xml")))
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

					Util.SaveTextFile(Path.Combine(LocalAppData,
						@"Frontier Developments\Elite Dangerous\Options\Graphics\GraphicsConfigurationOverride.xml"),
						_GP.ToString(),
						Util.TextEncoding.UTF8);
				}

				//----------------------------------------------------------------

				/* esto es para guardar varias claves en un XML
				 * 
				 * xmlFile = System.Xml.Linq.XDocument.Load(
									Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\GraphicsConfigurationOverride.xml"));

				query = from c in xmlFile.Elements("GraphicsConfig").Elements("GUIColour").Elements("Default").Elements("MatrixRed")
						select c;
				foreach (XElement element in query) element.Value = "1, 0, 0";

				query = from c in xmlFile.Elements("GraphicsConfig").Elements("GUIColour").Elements("Default").Elements("MatrixGreen")
						select c;
				foreach (XElement element in query) element.Value = "0, 1, 0";

				query = from c in xmlFile.Elements("GraphicsConfig").Elements("GUIColour").Elements("Default").Elements("MatrixBlue")
						select c;
				foreach (XElement element in query) element.Value = "0, 0, 1";

				xmlFile.Save(Path.Combine(LocalAppData, @"Frontier Developments\Elite Dangerous\Options\Graphics\GraphicsConfigurationOverride.xml"));*/

				#endregion
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

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

						this.GlobalSettings = null;
						string _File = Path.Combine(this.AppExePath, "Data", pGameInstance.key + "_Global_Settings.json");

						if (File.Exists(_File))
						{
							 this.GlobalSettings = Util.DeSerialize_FromJSON<ui_group>(_File);
						}

						if (this.GlobalSettings != null)
						{
							//Cargar las propiedades
							try
							{
								this.vGridGlobalSettings.BeginUpdate();
								this.vGridGlobalSettings.Rows.Clear();

								foreach (element _Element in this.GlobalSettings.Elements)
								{
									EditorRow _Fila = new DevExpress.XtraVerticalGrid.Rows.EditorRow(_Element.Title);
									_Fila.Properties.ToolTip = _Element.Description;
									_Fila.Properties.Caption = _Element.Title;
									_Fila.Properties.FieldName = "Value";
									_Fila.Tag = _Element;

									//Valor x defecto para el brillo del texto en horizons:
									if (this.ActiveInstance.key == "ED_Horizons" && _Element.Key == "w151")
									{
										if (_Element.Value <= 0) _Element.Value = 1;
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
													Name = string.Format("{0}|{1}", this.GlobalSettings.Name, _Element.Title),
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

											if (this._ElementPresets.IsNotEmpty())
											{
												List<combo_item> _Presets = this._ElementPresets.FindAll(x => x.Type == _Element.Type);
												if (_Presets.IsNotEmpty())
												{
													Invoke((MethodInvoker)(() =>
													{
														RepositoryItemLookUpEdit _ComboPreset = new RepositoryItemLookUpEdit()
														{
															Name = string.Format("{0}|{1}", this.GlobalSettings.Name, _Element.Title),
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
													Name = string.Format("{0}|{1}", this.GlobalSettings.Name, _Element.Title),
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

												if (this._RecentColors != null)
												{
													//Carga los colores usados en el tema en el cuadro 'colores Recientes'
													foreach (Color _Colour in this._RecentColors)
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
											#region Muestra un Toogle Switch:

											//"ValueType": "ONOFF",
											//"Type": "ONOFF",
											//"Value": 0,  <- 0=Off, 1=On

											Invoke((MethodInvoker)(() =>
											{
												RepositoryItemToggleSwitch _ToogleControl = new RepositoryItemToggleSwitch();
												_ToogleControl.Name = string.Format("{0}|{1}", this.GlobalSettings.Name, _Element.Title);
												_ToogleControl.EditValueChanged += _ComboValue_EditValueChanged;
												_ToogleControl.EditValueChangedDelay = 500;
												_ToogleControl.Tag = _Element;

												_Fila.Properties.RowEdit = _ToogleControl;
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
												Name = string.Format("{0}|{1}", this.GlobalSettings.Name, _Element.Title),
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
												Name = string.Format("{0}|{1}", this.GlobalSettings.Name, _Element.Title),
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
										CategoryRow Categoria = this.vGridGlobalSettings.Rows[_Element.Category] as CategoryRow;
										if (Categoria == null)
										{
											Categoria = new CategoryRow(_Element.Category)
											{
												Name = _Element.Category
											};
											this.vGridGlobalSettings.Rows.Add(Categoria);
										}
										Categoria.ChildRows.Add(_Fila);
									}
									else
									{
										this.vGridGlobalSettings.Rows.Add(_Fila);
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
								this.vGridGlobalSettings.EndUpdate();
								this.panelGlobalSettings.Text = this.GlobalSettings.Title;
							}));
						}
						else
						{
							Invoke((MethodInvoker)(() =>
							{
								this.vGridGlobalSettings.Rows.Clear();
							}));							
						}

						Invoke((MethodInvoker)(() =>
						{
							this.GlobalSettings_Title.Text = "Global Settings";
							this.GlobalSettings_Description.Text = "Elements in this List will have priority over the same from themes, so you can 'force' this settings to be applied no mather what theme you choose.";
						}));						
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		bool LoadingThemes = false; //<- Previene que se llame este metodo varias veces a la vez.
		bool LoadingTheme = false; //<- Previene que se llame este metodo varias veces a la vez.

		private void LoadThemeList_EX()
		{
			/* AQUI SE CARGA LA LISTA DE TEMAS DISPONIBLES DESDE LA CARPETA 'DEMO-PROFILES'  */
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if (!this.ActiveInstance.path.EmptyOrNull() && !this.LoadingThemes)
				{
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						this.LoadingThemes = true;
						this.ThemeListLoaded = false;
						this.UI_Themes = new List<ui_preset_new>();

						string ProfilesFolder = Path.Combine(this.ActiveInstance.path, this.ActiveInstance.themes_folder);

						if (Directory.Exists(ProfilesFolder))
						{
							#region Agregar Primero el Thema Actual:

							ui_preset_new _mytheme = new ui_preset_new("Current Settings", Path.Combine(this.ActiveInstance.path, @"EDHM-ini"), "User")
							{
								Preview = Util.GetElementImage(Path.Combine(this.AppExePath, "Images", "PREVIEW.PNG")),
								IsFavorite = true
							};
							this.UI_Themes.Add(_mytheme);

							#endregion

							#region Cargar Todos los Temas Disponibles

							foreach (var d in System.IO.Directory.GetDirectories(ProfilesFolder))
							{
								DirectoryInfo dir = new DirectoryInfo(d);
								ui_preset_new _theme = new ui_preset_new(dir.Name, dir.FullName);

								//Buscar el Archivo que identifica al Autor del Tema:
								string CreditsFile = dir.GetFiles("*.credits").Select(fi => fi.Name).FirstOrDefault().NVL("Unknown.credits");
								if (!CreditsFile.EmptyOrNull())
								{
									_theme.author = Path.GetFileNameWithoutExtension(CreditsFile);
								}

								//Buscar el archivo que identifica al tema como Favorito
								if (File.Exists(Path.Combine(_theme.folder, "IsFavorite.fav")))
								{
									_theme.IsFavorite = true;
								}

								//Buscar el archivo de Preview:
								if (File.Exists(Path.Combine(_theme.folder, "PREVIEW.jpg")))
								{
									//Carga la Imagen sin dejara 'en uso':
									using (Stream stream = File.OpenRead(Path.Combine(_theme.folder, "PREVIEW.jpg")))
									{
										_theme.Preview = System.Drawing.Image.FromStream(stream);
										_theme.HasPreview = true;
									}
								}
								else
								{
									//sI EL TEMA NO TIENE PREVIEW, USA UNA IMAGEN X DEFECTO;
									using (Stream stream = File.OpenRead(Path.Combine(this.AppExePath, "Images", "PREVIEW_DEFAULT.PNG")))
									{
										_theme.Preview = System.Drawing.Image.FromStream(stream);
										_theme.HasPreview = false;
									}
								}

								this.UI_Themes.Add(_theme);
							}

							#endregion
						}

						//3. Load and Show the presets in the Combo control:
						Invoke((MethodInvoker)(() =>
						{
							if (this.UI_Themes.IsNotEmpty())
							{
								this.gridControl1.DataSource = this.UI_Themes;
							}
							this.ThemeListLoaded = true;

							// Carga los Settings Actuales:
							if (this.UI_Themes.IsNotEmpty())
							{
								ui_preset_new _theme = this.gridView1.GetFocusedRow() as ui_preset_new;
								this.SelectedTheme = this.UI_Themes[0];
								//this.SelectedThemeElement = this.accordionControl1.Elements[0];
								if (this.SelectedTheme != null)
								{
									switch (this.ActiveInstance.key)
									{
										case "ED_Horizons": LoadTheme_Horizons(this.SelectedTheme); break;
										case "ED_Odissey": LoadTheme_Odissey(this.SelectedTheme); break;
										default: break;
									}
								}
							}
						}));
						this.LoadingThemes = false;
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private ui_setting Load_DefaultTheme(game_instance pGameInstance, string pLang = "en")
		{
			ui_setting _ret = null;
			try
			{
				string _TemplatesPath = Path.Combine(this.AppExePath, "Data");

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

				string JsonSettings_path = Path.Combine(_TemplatesPath, string.Format("{0}_Settings_{1}.json", pGameInstance.key, pLang.ToUpper()));
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

		private void LoadTheme_Horizons(ui_preset_new _Theme)
		{
			this.Cursor = Cursors.WaitCursor;

			try
			{
				if (_Theme != null && !this.LoadingTheme)
				{
					if (!_Theme.folder.EmptyOrNull() && Directory.Exists(_Theme.folder))
					{
						this.LoadingTheme = true;

						//Si el archivo no existe lo copia desde las Templates:
						if ( !File.Exists(Path.Combine(_Theme.folder, "Custom.ini")))
						{
							string template_path = Path.Combine(this.AppExePath, "Data", string.Format("{0}_Custom.ini", this.ActiveInstance.key));
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
							if (this.Settings.ui_groups.IsNotEmpty())
							{
								this._RecentColors = new HashSet<Color>();

								foreach (var _group in this.Settings.ui_groups)
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
													var _DefaultElement = this.DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
														Elements.Find(x => x.Key == _Element.Key);
													_Element.Value = _DefaultElement.Value;
												}

												//Guarda los colores usados en el Tema:
												this._RecentColors.Add(Color.FromArgb(Util.ValidarNulo(_Element.Value, 0)));

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
												var _DefaultElement = this.DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
													Elements.Find(x => x.Key == _Element.Key);
												_Element.Value = _DefaultElement.Value;
											}
										}
									}
								}

								if (_ReaderXML != null)
								{
									foreach (var _key in this.Settings.xml_profile)
									{
										_key.value = Convert.ToDecimal(_ReaderXML.ReadKey(_key.key, "constants").NVL("-1"));
									}
								}

								this.LoadingTheme = false;
								PreviewTheme(true);
							}
						}

						if (_Reader != null) _Reader = null;
						if (_Custom != null) _Custom = null;
						if (_ReaderXML != null) _ReaderXML = null;
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}
		private void LoadTheme_Odissey(ui_preset_new _Theme)
		{
			/* LEE LOS VALORES DEL TEMA ACTUAL Y LOS GUARDA EN EL JSON  'this.Settings'  */
			this.Cursor = Cursors.WaitCursor;

			try
			{
				if (_Theme != null && !this.LoadingTheme)
				{
					if (!_Theme.folder.EmptyOrNull() && Directory.Exists(_Theme.folder))
					{
						this.LoadingTheme = true;

						//2. Abrir el Archivo INI desde su Ubicacion:
						IniFile _Reader = new IniFile(Path.Combine(_Theme.folder, @"Startup-Profile.ini"));
						IniFile _ReaderXML = new IniFile(Path.Combine(_Theme.folder, @"XML-Profile.ini"));
						IniFile _ReaderAdvanced = null;
						IniFile _ReaderOnfoot = null;

						if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(_Theme.folder, "Advanced.ini")))
						{
							_ReaderAdvanced = new IniFile(Path.Combine(_Theme.folder, @"Advanced.ini"));
						}
						//Si el archivo no existe lo copia desde las Templates:
						if (this.ActiveInstance.key == "ED_Odissey" && !File.Exists(Path.Combine(_Theme.folder, "SuitHud.ini")))
						{
							string template_path = Path.Combine(this.AppExePath, "Data", string.Format("{0}_SuitHud.ini", this.ActiveInstance.key));
							if (File.Exists(template_path))
							{
								File.Copy(template_path, Path.Combine(_Theme.folder, "SuitHud.ini"), true);
							}
						}
						if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(_Theme.folder, "SuitHud.ini")))
						{
							_ReaderOnfoot = new IniFile(Path.Combine(_Theme.folder, @"SuitHud.ini"));
						}

						if (_Reader != null && _ReaderAdvanced != null)
						{
							if (this.Settings.ui_groups.IsNotEmpty())
							{
								this._RecentColors = new HashSet<Color>();

								foreach (var _group in this.Settings.ui_groups)
								{
									if (_group.Elements.IsNotEmpty())
									{
										foreach (var _Element in _group.Elements)
										{
											if (_Element.ValueType == "Color")
											{
												if (_Element.Key == "x171|y171|z171")
												{

												}
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
													var _DefaultElement = this.DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
														Elements.Find(x => x.Key == _Element.Key);
													_Element.Value = _DefaultElement.Value;
												}

												//Guarda los colores usados en el Tema:
												this._RecentColors.Add(Color.FromArgb(Util.ValidarNulo(_Element.Value, 0)));

												#endregion
											}
											else  //<- ValueType = 'Preset', 'Decimal'
											{
												//Console.WriteLine(string.Format("{0};{1};{2}", _Element.Key, _Element.Title, _Element.File));
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
													//Si la Clave no existe en el Tema elejido, se carga el valor del tema plantilla:
													var _DefaultElement = this.DefaultSettings.ui_groups.Find(x => x.Name == _group.Name).
														Elements.Find(x => x.Key == _Element.Key);
													_Element.Value = _DefaultElement.Value;
												}
											}
										}
									}
								}

								if (_ReaderXML != null)
								{
									foreach (var _key in this.Settings.xml_profile)
									{
										_key.value = Convert.ToDecimal(_ReaderXML.ReadKey(_key.key, "constants").NVL("-1"));
									}
								}

								this.LoadingTheme = false;
								PreviewTheme(true);
							}
						}

						if (_Reader != null) _Reader = null;
						if (_ReaderAdvanced != null) _ReaderAdvanced = null;
						if (_ReaderXML != null) _ReaderXML = null;
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

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
						if (item < 0) _ret = false;
					}
				}
			}
			catch { }
			return _ret;
		}

		private void LoadGroupSettings(string pUIGroupName, string SelectRowName = "")
		{
			/* AQUI CARGAMOS TODOS LOS ELEMENTOS DEL GRUPO INDICADO Y LOS MOSTRAMOS EN LA VENTANA DE PROPIEDADES  */
			try
			{
				if (this.Settings != null && this.Settings.ui_groups.IsNotEmpty())
				{
					this.Cursor = Cursors.WaitCursor;
					var t = Task.Factory.StartNew(delegate
					{
						ui_group _UIGroup = this.Settings.ui_groups.Find(x => x.Name == pUIGroupName);
						if (_UIGroup != null)
						{
							//Cargar las propiedades
							try
							{
								this.vGridDetalles.BeginUpdate();
								this.vGridDetalles.Rows.Clear();

								foreach (element _Element in _UIGroup.Elements)
								{
									EditorRow _Fila = new DevExpress.XtraVerticalGrid.Rows.EditorRow(_Element.Title);
									_Fila.Properties.ToolTip = _Element.Description;
									_Fila.Properties.Caption = _Element.Title;
									_Fila.Properties.FieldName = "Value";
									_Fila.Tag = _Element;

									//Valor x defecto para el brillo del texto en horizons:
									if (this.ActiveInstance.key == "ED_Horizons" && _Element.Key == "w151")
									{
										if (_Element.Value <= 0) _Element.Value = 1; //
									}
									//Valor x defecto para el brillo de Orbit Lines en horizons:
									if (this.ActiveInstance.key == "ED_Horizons" && _Element.Key == "y117")
									{
										if (_Element.Value <= 0) _Element.Value = 1; 
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

											if (this._ElementPresets.IsNotEmpty())
											{
												List<combo_item> _Presets = this._ElementPresets.FindAll(x => x.Type == _Element.Type);
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
												RepositoryItemColorPickEdit _ComboColor = new RepositoryItemColorPickEdit
												{
													Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title),
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

												if (this._RecentColors != null)
												{
													//Carga los colores usados en el tema en el cuadro 'colores Recientes'
													foreach (Color _Colour in this._RecentColors)
													{
														_ComboColor.RecentColors.InsertColor(_Colour);
													}
												}

												if (this.DefaultSettings != null && _Element.Value == 0)
												{
													var _Grupo = this.DefaultSettings.ui_groups.Find(x => x.Name == _UIGroup.Name);
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
													_Fila.Properties.Value = Color.FromArgb(Util.ValidarNulo(_Element.Value, 0));
												}

												_Fila.Properties.RowEdit = _ComboColor;												
											}));

											#endregion
											break;

										case "ONOFF":
											#region Muestra un Toogle Switch:

											//"ValueType": "ONOFF",
											//"Type": "ONOFF",
											//"Value": 0,  <- 0=Off, 1=On

											Invoke((MethodInvoker)(() =>
											{
												RepositoryItemToggleSwitch _ToogleControl = new RepositoryItemToggleSwitch();
												_ToogleControl.Name = string.Format("{0}|{1}", _UIGroup.Name, _Element.Title);
												_ToogleControl.EditValueChanged += _ComboValue_EditValueChanged;
												_ToogleControl.EditValueChangedDelay = 500;
												_ToogleControl.Tag = _Element;

												_Fila.Properties.RowEdit = _ToogleControl;
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
										CategoryRow Categoria = this.vGridDetalles.Rows[_Element.Category] as CategoryRow;
										if (Categoria == null)
										{
											Categoria = new CategoryRow(_Element.Category)
											{
												Name = _Element.Category
											};
											this.vGridDetalles.Rows.Add(Categoria);
										}
										Categoria.ChildRows.Add(_Fila);
									}
									else
									{
										this.vGridDetalles.Rows.Add(_Fila);
									}

									if (_Fila != null && _Element.Title == SelectRowName)
									{
										Invoke((MethodInvoker)(() =>
										{
											this.vGridDetalles.FocusedRow = _Fila;
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
								this.vGridDetalles.EndUpdate();
								//this.vGridDetalles.BestFit();

								this.dockDetalles.Text = _UIGroup.Title;
								this.dockManager1.ActivePanel = this.dockDetalles;
								this.Cursor = Cursors.Default;
							}));
						}
						else
						{
							Invoke((MethodInvoker)(() =>
							{
								this.vGridDetalles.Rows.Clear();

								this.dockDetalles.Text = string.Empty;
								this.dockManager1.ActivePanel = this.dockDetalles;
								this.Cursor = Cursors.Default;
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
			if (this.ActiveInstance.key == "ED_Horizons")
			{
				if (this.PreviewForm == null || !this.PreviewForm.Visible)
				{
					XMLchanged = true;
					this.PreviewForm = new PreviewForm(this.ActiveInstance);
					this.PreviewForm.Show();
					this.PreviewForm.OnPreviewLoaded += PreviewForm_OnPreviewLoaded;
				}
				this.PreviewForm.Settings = this.Settings;
				this.PreviewForm.PreviewTheme(XMLchanged);
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
			if (this.PreviewForm != null && this.SelectedTheme != null && !this.SelectedTheme.HasPreview)
			{
				this.SelectedTheme.Preview = this.PreviewForm.GetPreviewThumbnail();
				this.SelectedTheme.HasPreview = true;
				this.SelectedTheme.Preview.Save(Path.Combine(this.SelectedTheme.folder, "PREVIEW.jpg"));
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
					this.Cursor = Cursors.WaitCursor;
					string _ProfileName = XtraInputBox.Show("Type a Name for your Custom Theme:\r\n** If Exists, it will be Overwritten!",
						"Create New Theme", this.SelectedTheme.name);

					string _AuthorName = XtraInputBox.Show("Who's the Author of this theme?",
						"Create New Theme", this.SelectedTheme.author);

					if (!_ProfileName.EmptyOrNull())
					{
						string GameFolder = Path.Combine(this.ActiveInstance.path, @"EDHM-ini");
						string NewProfileFolder = Path.Combine(this.ActiveInstance.path, this.ActiveInstance.themes_folder, _ProfileName);

						//1. Crear la Carpeta para el Nuevo Perfil, si ya Existe, se Sobreescribe:
						var _ProfileDir = System.IO.Directory.CreateDirectory(NewProfileFolder);
						if (_ProfileDir != null)
						{
							ApplyTheme(false, true); //<- Aplica los cambios Actuales

							if (this.ActiveInstance.key == "ED_Horizons")
							{
								//if (XtraMessageBox.Show("Would you like to Assign a Key Binding to this Profile?\r\n** This is enterely Optional, you should try to not overlfill your keybindings.",
								//							"Create Hot Key Binding?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
								//{
								//	CreateThemeKeyBinding(_ProfileName);
								//}
							}

							//Copiar los Archivos del Tema Actual: // existing files will be overwritten
							File.Copy(Path.Combine(GameFolder, @"Startup-Profile.ini"),
								Path.Combine(NewProfileFolder, @"Startup-Profile.ini"), true);

							File.Copy(Path.Combine(GameFolder, "XML-Profile.ini"),
								Path.Combine(NewProfileFolder, @"XML-Profile.ini"), true);

							if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(GameFolder, @"Advanced.ini")))
							{
								File.Copy(Path.Combine(GameFolder, @"Advanced.ini"),
								Path.Combine(NewProfileFolder, @"Advanced.ini"), true);
							}

							if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(GameFolder, @"SuitHud.ini")))
							{
								File.Copy(Path.Combine(GameFolder, @"SuitHud.ini"),
								Path.Combine(NewProfileFolder, @"SuitHud.ini"), true);
							}

							//Agregar el Identificador del Autor:
							Util.SaveTextFile(Path.Combine(NewProfileFolder, string.Format("{0}.credits", _AuthorName)),
								string.Format("** THIS THEME WAS MADE BY {0} **", _AuthorName.ToUpper()),
								Util.TextEncoding.UTF8);

							// Agregar una Imagen de Preview :
							if (this.ActiveInstance.key == "ED_Horizons")
							{
								Image _Preview = this.PreviewForm.GetPreviewThumbnail();
								Bitmap bitmapImage = new Bitmap(_Preview);
								bitmapImage.Save(Path.Combine(NewProfileFolder, "PREVIEW.PNG"), System.Drawing.Imaging.ImageFormat.Png);
							}

							LoadThemeList_EX();

							_ret = NewProfileFolder; //<- Devuelve la Ruta del Nuevo tema
							XtraMessageBox.Show(string.Format("The profile '{0}' has successfully been created.", _ProfileName),
								"Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
			return _ret;
		}
		private void CreateThemeKeyBinding(string _ProfileName)
		{
			try
			{
				KeyBindingSimpleForm _KeyForm = new KeyBindingSimpleForm(_ProfileName);
				if (_KeyForm.ShowDialog() == DialogResult.OK)
				{
					string GameFolder = Path.Combine(this.ActiveInstance.path, "EDHM-ini");
					string MyProfilesINI = Util.ReadTextFile(Path.Combine(GameFolder, @"MyProfiles\MyProfiles.ini"), Util.TextEncoding.UTF8);

					IniFile _Reader = new IniFile(Path.Combine(this.ActiveInstance.path, "XML-Profile.ini"));

					StringBuilder _Profile = new StringBuilder();
					_Profile.AppendLine(string.Format("[keyToggle-{0}]", _ProfileName.Replace(' ', '-')));
					_Profile.AppendLine(string.Format("Key = {0}", _KeyForm.KeyBinding));

					if (this.Settings.ui_groups.IsNotEmpty())
					{
						//Aqui Grabamos todos los Elelementos:
						foreach (var _group in this.Settings.ui_groups)
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
					string NewProfileFolder = Path.Combine(this.ActiveInstance.path, this.ActiveInstance.themes_folder, _ProfileName);
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

		private void ApplyTheme(bool SaveIt = true, bool KeepItQuiet = false)
		{
			// Copiar los Archivos Plantilla con la Ultima Version del MOD:
			if (File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Startup-Profile.ini", this.ActiveInstance.key))))
			{
				File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Startup-Profile.ini", this.ActiveInstance.key)),
					Path.Combine(this.ActiveInstance.path, @"EDHM-ini\Startup-Profile.ini"), true);
			}
			if (File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_XML-Profile.ini", this.ActiveInstance.key))))
			{
				File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_XML-Profile.ini", this.ActiveInstance.key)),
					Path.Combine(this.ActiveInstance.path, @"EDHM-ini\XML-Profile.ini"), true);
			}
			if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Advanced.ini", this.ActiveInstance.key))))
			{
				File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Advanced.ini", this.ActiveInstance.key)),
					Path.Combine(this.ActiveInstance.path, @"EDHM-ini\Advanced.ini"), true);
			}
			if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_SuitHud.ini", this.ActiveInstance.key))))
			{
				File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_SuitHud.ini", this.ActiveInstance.key)),
					Path.Combine(this.ActiveInstance.path, @"EDHM-ini\SuitHud.ini"), true);
			}			

			switch (this.ActiveInstance.key)
			{
				case "ED_Horizons":
					ApplyTheme_Horizons(SaveIt, KeepItQuiet);
					break;
				case "ED_Odissey":
					ApplyTheme_Odissey(SaveIt, KeepItQuiet);
					break;
			}
		}
		private void ApplyTheme_Horizons(bool SaveIt = true, bool KeepItQuiet = false)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if (!this.ActiveInstance.path.EmptyOrNull())
				{
					IniFile _Reader = new IniFile(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\Startup-Profile.ini")); //<- Open and Read the INI file
					IniFile _ReaderXML = new IniFile(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\XML-Profile.ini"));
					IniFile _Custom = new IniFile(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\Custom.ini")); //<- Open and Read the INI file

					if (this.Settings.ui_groups.IsNotEmpty())
					{
						foreach (var _group in this.Settings.ui_groups)
						{
							if (_group.Elements.IsNotEmpty())
							{
								foreach (var _Element in _group.Elements)
								{
									//1º Busca si hay un GlobalSettings para el Elemento
									if (this.GlobalSettings != null)
									{
										element _Existe = null;
										if (this.GlobalSettings.Elements.IsNotEmpty())
											_Existe = this.GlobalSettings.Elements.Find(x => x.Key == _Element.Key);
										if (_Existe != null)
										{
											//Si lo hay, este tiene prioridad y se aplica
											_Element.Value = _Existe.Value;
										}
									}

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
													case "Custom":
														_Custom.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section);
														break;

													default:
														_Reader.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section);
														break;
												}												
												i++;
											}
										}
									}
									else //<- No es un Color
									{
										switch (_Element.File)
										{
											case "Custom":
												_Custom.WriteKey(_Element.Key, _Element.Value.NVL("0"), _Element.Section);
												break;

											default:
												_Reader.WriteKey(_Element.Key, _Element.Value.NVL("0"), _Element.Section);
												break;
										}										
									}
								}
							}
						}
					}

					if (this.Settings.xml_profile.IsNotEmpty())
					{						
						if (_ReaderXML != null)
						{
							foreach (var _key in this.Settings.xml_profile)
							{
								_ReaderXML.WriteKey(_key.key, _key.value.ToString(), "constants");
							}
						}
					}

					if (SaveIt) SaveTheme(true);

					if (KeepItQuiet == false)
					{
						//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 3 SEGUNDOS:
						XtraMessageBoxArgs args = new XtraMessageBoxArgs();
						args.AutoCloseOptions.Delay = 2500;
						args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
						args.Caption = "Done";
						args.Text = "Visual Theme Applied Succesfully!.";
						args.Buttons = new DialogResult[] { DialogResult.OK };

						XtraMessageBox.Show(args).ToString();
					}
				}
				else
				{
					XtraMessageBox.Show("the Game' Instance Path is not set.", "ERROR_404",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}
		private void ApplyTheme_Odissey(bool SaveIt = true, bool KeepItQuiet = false)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if (!this.ActiveInstance.path.EmptyOrNull())
				{
					IniFile _Reader = new IniFile(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\Startup-Profile.ini")); //<- Open and Read the INI file			
					IniFile _ReaderXML = new IniFile(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\XML-Profile.ini"));
					IniFile _ReaderAdvanced = null;
					IniFile _ReaderOnfoot = null;

					if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\Advanced.ini")))
					{
						_ReaderAdvanced = new IniFile(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\Advanced.ini"));
					}

					if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\SuitHud.ini")))
					{
						_ReaderOnfoot = new IniFile(Path.Combine(this.ActiveInstance.path, @"EDHM-ini\SuitHud.ini"));
					}

					if (this.Settings.ui_groups.IsNotEmpty())
					{
						foreach (var _group in this.Settings.ui_groups)
						{
							if (_group.Elements.IsNotEmpty())
							{
								foreach (var _Element in _group.Elements)
								{
									//1º Busca si hay un GlobalSettings para el Elemento
									if (this.GlobalSettings != null)
									{
										element _Existe = null;
										if (this.GlobalSettings.Elements.IsNotEmpty())
											_Existe = this.GlobalSettings.Elements.Find(x => x.Key == _Element.Key);
										if (_Existe != null)
										{
											//Si lo hay, este tiene prioridad y se aplica
											_Element.Value = _Existe.Value;
										}
									}

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
													default: _Reader.WriteKey(_Key, _GammaColors[i].ToString(), _Element.Section); break;
												}
												i++;
											}
										}
									}
									else //<- ValueType = 'Preset', 'Decimal'
									{
										switch (_Element.File)
										{
											case "Advanced": _ReaderAdvanced.WriteKey(_Element.Key, _Element.Value.ToString(), _Element.Section); break;
											case "SuitHud": _ReaderOnfoot.WriteKey(_Element.Key, _Element.Value.ToString(), _Element.Section); break;
											default: _Reader.WriteKey(_Element.Key, _Element.Value.NVL("0"), _Element.Section); break;
										}
									}
								}
							}
						}
					}

					if (this.Settings.xml_profile.IsNotEmpty())
					{						
						if (_ReaderXML != null)
						{
							foreach (var _key in this.Settings.xml_profile)
							{
								_ReaderXML.WriteKey(_key.key, _key.value.ToString(), "constants");
							}
						}
					}

					if (SaveIt) SaveTheme(true);

					//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 2 SEGUNDOS:
					if (KeepItQuiet == false)
					{
						XtraMessageBoxArgs args = new XtraMessageBoxArgs()
						{
							Caption = "Done",
							Text = "Visual Theme Applied Succesfully!.",
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
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private void SaveTheme(bool Silent = false)
		{
			try
			{
				// THIS SAVES THE CHANGES IN THE PRESET INTO THE JSON FILE */
				string JsonSettings_path = Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_Settings.json");
				if (System.IO.File.Exists(JsonSettings_path))
				{
					if (this.Settings != null)
					{
						Util.Serialize_ToJSON(JsonSettings_path, this.Settings);
						if (!Silent) XtraMessageBox.Show("Theme Saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
					this.Cursor = Cursors.WaitCursor;

					string FileName = System.IO.Path.GetFileNameWithoutExtension(XOFD.FileName); //<- Nombre sin Extension ni Path
					string ThemesFolder = Path.Combine(this.ActiveInstance.path, this.ActiveInstance.themes_folder);

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
			finally { this.Cursor = Cursors.Default; }
			return _ret;
		}
		private bool ExportTheme()
		{
			/* Convierte la configuracion actual en un tema nuevo y lo empaca en un ZIP  */
			bool _ret = false;
			try
			{
				string NewThemePath = CreateNewTheme();

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
						this.Cursor = Cursors.WaitCursor;

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
			finally { this.Cursor = Cursors.Default; }
			return _ret;
		}

		private void UninstallEDHMmod(bool Silent = false)
		{
			try
			{
				if (!CheckGameRunning())
				{
					if (Directory.Exists(this.ActiveInstance.path))
					{
						string _KEY = this.ActiveInstance.key == "ED_Horizons" ? "HORIZONS" : "ODYSSEY";

						this.progressPanel1.Dock = DockStyle.Fill;
						this.progressPanel1.Caption = String.Format("Un-Installing EDHM ({0})", _KEY);
						this.progressPanel1.Description = "";
						this.progressPanel1.Visible = true;
						this.progressPanel1.BringToFront();

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
							this.Cursor = Cursors.WaitCursor;

							System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
							var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
							{
								System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

								/* COMMON FILES AND FOLDERS */
								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing d3dx.ini"; }));
								string File_Path = Path.Combine(this.ActiveInstance.path, @"d3dx.ini");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing d3d11.dll"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"d3d11.dll");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing d3d11_log.txt"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"d3d11_log.txt");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing d3d11_profile_log.txt"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"d3d11_profile_log.txt");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing d3dcompiler_46.dll"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"d3dcompiler_46.dll");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing d3dcompiler_46_log.txt"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"d3dcompiler_46_log.txt");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing nvapi64.dll"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"nvapi64.dll");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing nvapi_log.txt"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"nvapi_log.txt");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing ShaderUsage.txt"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"ShaderUsage.txt");
								if (File.Exists(File_Path)) File.Delete(File_Path);


								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing ShaderFixes"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"ShaderFixes");
								if (Directory.Exists(File_Path)) Directory.Delete(File_Path, true);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-ini"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-ini");
								if (Directory.Exists(File_Path)) Directory.Delete(File_Path, true);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing ShaderCache"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"ShaderCache");
								if (Directory.Exists(File_Path)) Directory.Delete(File_Path, true);

								Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-Uninstall.bat"; }));
								File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-Uninstall.bat");
								if (File.Exists(File_Path)) File.Delete(File_Path);

								/* ODYSSEY EXCLUSIVE FILES */
								if (_KEY == "ODYSSEY")
								{

								}

								/* HORIZONS EXCLUSIVE FILES */
								if (_KEY == "HORIZONS")
								{
									Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-v1.5-Catalogue.pdf"; }));
									File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-v1.5-Catalogue.pdf");
									if (File.Exists(File_Path)) File.Delete(File_Path);

									Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-v1.51-Manual.pdf"; }));
									File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-v1.51-Manual.pdf");
									if (File.Exists(File_Path)) File.Delete(File_Path);

									Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-v1.51-Profile-Guide.pdf"; }));
									File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-v1.51-Profile-Guide.pdf");
									if (File.Exists(File_Path)) File.Delete(File_Path); 

									Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-v1.52-Manual.pdf"; }));
									File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-v1.52-Manual.pdf");
									if (File.Exists(File_Path)) File.Delete(File_Path); 

									Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-Keybinds-Essential.bat"; }));
									File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-Keybinds-Essential.bat");
									if (File.Exists(File_Path)) File.Delete(File_Path);

									Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-Keybinds-Full.bat"; }));
									File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-Keybinds-Full.bat");
									if (File.Exists(File_Path)) File.Delete(File_Path);

									Invoke((MethodInvoker)(() => { this.progressPanel1.Description = "Removing EDHM-RemoveDemos.bat"; }));
									File_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-RemoveDemos.bat");
									if (File.Exists(File_Path)) File.Delete(File_Path);
								}

								Invoke((MethodInvoker)(() =>
								{
									this.progressPanel1.Description = "Done.";
									this.progressPanel1.Visible = false;
									this.Cursor = Cursors.Default;

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
							this.progressPanel1.Description = "Done.";
							this.progressPanel1.Visible = false;
							this.Cursor = Cursors.Default;
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
				this.progressPanel1.Visible = false;
				this.Cursor = Cursors.Default;
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
				if (File.Exists(Path.Combine(this.AppExePath, "EDHM_HOTFIX.json")))
				{
					List<file_job> _Jobs = Util.DeSerialize_FromJSON<List<file_job>>(Path.Combine(this.AppExePath, "EDHM_HOTFIX.json"));
					if (_Jobs != null)
					{
						string HORI_Path = Util.WinReg_ReadKey("EDHM", "ED_Horizons").NVL(string.Empty);
						string ODYS_Path = Util.WinReg_ReadKey("EDHM", "ED_Odissey").NVL(string.Empty);
						_ret = true;

						foreach (file_job _job in _Jobs)
						{
							string GamePath = _job.game == "ODYSSEY" ? ODYS_Path : HORI_Path;

							_job.file_path = _job.file_path.Replace("%GAME_PATH%", GamePath);
							_job.file_path = _job.file_path.Replace("%UI_PATH%", this.AppExePath);

							if (_job.destination != null && _job.destination != string.Empty)
							{
								_job.destination = _job.destination.Replace("%GAME_PATH%", GamePath);
								_job.destination = _job.destination.Replace("%UI_PATH%", this.AppExePath);
							}

							try
							{
								if (File.Exists(_job.file_path))
								{
									switch (_job.action)
									{
										case "COPY":
											File.Copy(_job.file_path, _job.destination, true);
											break;

										case "MOVE":
											File.Copy(_job.file_path, _job.destination, true);
											File.Delete(_job.file_path);
											break;

										case "DEL":
											File.Delete(_job.file_path);
											break;

										case "RMDIR":
											Directory.Delete(_job.file_path, true);
											break;
									}
								}
							}
							catch { }
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
			return _ret;
		}

		private bool CheckGameRunning(bool ForceCheck = false)
		{
			bool _ret = false;
			try
			{
				//Busca la Ventana del Juego:
				if (this.GameWindow == null || ForceCheck == true)
				{
					string GameTitle = Util.AppConfig_GetValue("GameProcessID");
					System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
					foreach (System.Diagnostics.Process process in processlist)
					{
						if (!String.IsNullOrEmpty(process.MainWindowTitle))
						{
							if (process.MainWindowTitle == GameTitle)
							{
								this.GameWindow = process;
								this.GameWindow.EnableRaisingEvents = true;
								this.GameWindow.Exited += (sender, eventArgs) =>
								{
									//Ocurre cuando el Juego se Cierra:
									this.GameIsRunning = false;
									this.GameWindow = null;
									_ret = false;
								};
								this.GameIsRunning = true;
								_ret = true;
								break;
							}
						}
					}
					processlist = null;
				}
				else
				{
					if (this.GameWindow.HasExited == false)
					{
						this.GameIsRunning = true;
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
					if (process.ProcessName == GameTitle)
					{
						UI_Proc = process;
						break;
					}
				}
			}
			return UI_Proc != null ? true : false;
		}

		/* HERE CHECKS FOR UPDATES  */
		bool CheckingUpdates = false; //<- Previene que se llame este metodo varias veces a la vez.
		private void CheckForModUpdates()
		{
			/* AQUI SE BUSCAN ACTUALIZACIONDES DEL PROGRAMA, DEL MOD Y ARCHIVOS VARIOS */
			try
			{
				if (!this.CheckingUpdates)
				{
					this.CheckingUpdates = true;
					string _FileName = "EDHM_Version.json";
					string TempFilePath = Path.Combine(Path.GetTempPath(), "EDHM_UI", _FileName); //<- %LOCALAPPDATA%\Temp\EDHM_UI\EDHM_Version.json

					if (File.Exists(TempFilePath)) File.Delete(TempFilePath);

					FileDownloader FD = new FileDownloader(Util.AppConfig_GetValue("UpdatesURL"), TempFilePath);
					FD.OnDownload_Complete += (sender, eventArgs) =>
					{
						//System.ComponentModel.AsyncCompletedEventArgs Ev = eventArgs as System.ComponentModel.AsyncCompletedEventArgs;
						long[] _Data = sender as long[];
						if (_Data != null)
						{
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
									if (App_Version < new Version(VI.app_version)) HayActualizacion = true;
									if (HoriVersion < new Version(VI.ED_Horizons)) HayActualizacion = true;
									if (OddyVersion < new Version(VI.ED_Odissey)) HayActualizacion = true;

									VI.cur_version = App_Version.ToString();
									Util.Serialize_ToJSON(TempFilePath, VI);

									if (HayActualizacion)
									{
										//Hay Actualizacion!
										if (XtraMessageBox.Show(string.Format("There is an Update Available v{0}: \r\nChange Log:\r\n{1}", VI.app_version, VI.changelog),
												"Download Update?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
										{
											if (this.PreviewForm != null && this.PreviewForm.Visible)
											{
												this.PreviewForm.Close();
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
				if (_GammaComponents.Count > 3) alpha = _GammaComponents[3];

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

		private bool ApplyingShip = false;
		private void ReadPlayerJournal()
		{
			try
			{
				if (this.WatchMe)
				{
					string UserProfile = Environment.GetEnvironmentVariable("UserProfile");
					string EDJournalDir = Path.Combine(UserProfile, @"Saved Games\Frontier Developments\Elite Dangerous");
					// %UserProfile%\Saved Games\Frontier Developments\Elite Dangerous

					//Esto Lee las Flags usadas en el archivo 'Status.json'
					//https://elite-journal.readthedocs.io/en/latest/Status%20File/
					/*var mask = (StatusFlags)16842765;
					var result =
						Enum.GetFlafValues(typeof(StatusFlags))
							.Cast<StatusFlags>()
							.Where(value => mask.HasFlag(value))
							.ToList();  
					*/

					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

						if (Directory.Exists(EDJournalDir))
						{
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
						}
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void PlayerJournal_WatchDirectory(string pDirectoryPath)
		{
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = pDirectoryPath;
			watcher.EnableRaisingEvents = true;
			watcher.Filter = "Journal.*.log";
			watcher.Created += (sender, e) =>
			{
				this.RunWatcher = false; //<- Deja de Leer el archivo actual (si hay)

				DirectoryInfo di = new DirectoryInfo(pDirectoryPath);

				//Busca el Archivo de Log mas reciente:
				FileInfo JournalFile = di.GetFiles("Journal.*.log")
					.OrderByDescending(f => f.LastWriteTime).First();

				if (JournalFile != null)
				{
					//Abre el archivo en modo compartido y 'Escucha' si ha sido modificado:
					this.RunWatcher = true;
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
				if (File.Exists(pFilePath) && this.WatchingFile != pFilePath)
				{
					this.WatchingFile = pFilePath;
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
						while (this.RunWatcher) //<- Poner en False para dejar de Leer el Archivo
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
				 "event":"Commander", "Name":"Blue mystic"
				 "event":"LoadGame", "Commander":"Blue mystic", "Horizons":true, "Odyssey":true, "Ship":"Python", "ShipID":6, "ShipName":"NORMANDY", "ShipIdent":"SR-03", "GameMode":"Solo", "gameversion":"4.0.0.700"
				 "event":"Loadout", "Ship":"cutter", "ShipName":"NORMANDY",  "ShipIdent":"SR-04"
				 "event":"Rank", "Combat":3, "Trade":6, "Explore":5, "Soldier":0, "Exobiologist":0, "Empire":12, "Federation":3, "CQC":0 }
				 "event":"Shutdown" */

				//1. Buscar el Nombre del Jugador:
				int index = 0;
				index = JsonLine.IndexOf("\"event\":\"Commander\"", index);
				if (index != -1)
				{
					//Evento Detectado!
					dynamic data = Newtonsoft.Json.Linq.JObject.Parse(JsonLine);
					if (data != null)
					{
						if (this.CommanderName != Convert.ToString(data.Name))
						{
							this.CommanderName = data.Name;
							if (this.GreetMe)
							{
								ShowSystemNotificacion(string.Format("Welcome Commander {0}", this.CommanderName),
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
					if (data != null)
					{
						ship_loadout SL = new ship_loadout
						{
							ship_short_type = data.Ship,
							ship_name = data.ShipName,
							ship_id = data.ShipIdent
						};

						this.ShipIDName = string.Format("{0} ({1} {2})", Util.NVL(data.Ship, string.Empty).ToUpper(), data.ShipName, data.ShipIdent);
						PlayerJournal_ShipChanged(SL);
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
				this.lblShipStatus.Caption = string.Format("Cmdr. {0}, Ship: {1}",
						this.CommanderName, this.ShipIDName);

				if (this.Shipyard != null)
				{
					ship_loadout MyShip = null;
					this.Shipyard.player_name = this.CommanderName;
					this.Shipyard.active_instance = this.ActiveInstance.key;

					if (this.Shipyard.ships == null) this.Shipyard.ships = new List<ship_loadout>();

					//Revisar si la Nave ya existe en el Shipyard:
					bool Existe = false;
					if (this.Shipyard.ships.IsNotEmpty())
					{
						MyShip = this.Shipyard.ships.Find(x => x.ship_short_type == CurrentShip.ship_short_type.Trim() &&
																	 x.ship_name == CurrentShip.ship_name.Trim() &&
																	   x.ship_id == CurrentShip.ship_id.Trim());
						if (MyShip != null) Existe = true;
					}

					//Si la Nave No existe la Agregamos al Historial:
					if (!Existe && this.WatchMe)
					{
						//ShowSystemNotificacion("EDHM - UI", "New Ship Detected!: " + CurrentShip.ship_short_type);
						this.Shipyard.ships.Add(CurrentShip);

						//Guarda los camnbios en el JSON:
						Util.Serialize_ToJSON(Path.Combine(this.AppExePath, @"Data\PlayerLoadout.json"), this.Shipyard);
						return;
					}

					//Verificar si el Juego esta Corriendo:		
					//if (!ApplyingShip)
					//{						
					string GameTitle = Util.AppConfig_GetValue("GameProcessID");
					System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
					foreach (System.Diagnostics.Process process in processlist)
					{
						if (!String.IsNullOrEmpty(process.ProcessName))
						{
							if (process.MainWindowTitle == GameTitle)
							{
								this.GameWindow = process;
								this.GameIsRunning = true;
								break;
							}
						}
					}
					//}
					//System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcessesByName(Util.AppConfig_GetValue("GameProcessID"));
					//if (processlist != null && processlist.Length > 0)
					//{
					//	this.GameWindow = processlist[0];
					//	this.GameIsRunning = true;
					//}
					//else
					//{
					//	this.GameWindow = null;
					//	this.GameIsRunning = false;
					//}

					if (this.GameIsRunning && Existe) //<- Determinar si el juego está Corriendo
					{
						this.ApplyingShip = true;

						//Si la Nave esta registrada en el Historial y está habilitado el Cambio de Tema:
						if (this.Shipyard != null && this.Shipyard.ships.IsNotEmpty() && this.Shipyard.theme_swaping == true)
						{
							//La nave debe tener un tema asignado:
							if (MyShip != null && !MyShip.theme.EmptyOrNull())
							{
								//ShowSystemNotificacion("EDHM - UI", "Ship Board Detected!: " + CurrentShip.ship_short_type);

								//Busca el Tema indicado para la Nave:
								ui_preset_new _Theme = null;
								if (MyShip.theme == "@Random Theme")
								{
									int index = 0; //Elije un tema aleatorio
									index = index.RandomBetween(1, this.UI_Themes.Count - 1);
									_Theme = this.UI_Themes[index];
								}
								else
								{
									//Elije el tema indicado:
									_Theme = this.UI_Themes.Find(x => x.name == MyShip.theme);
								}

								if (_Theme != null)
								{
									//Carga el Tema Indicado:
									Invoke((MethodInvoker)(() =>
									{
										this.SelectedTheme = _Theme;
										switch (this.ActiveInstance.key)
										{
											case "ED_Horizons": LoadTheme_Horizons(_Theme); break;
											case "ED_Odissey": LoadTheme_Odissey(_Theme); break;
											default: break;
										}
										this.lblShipStatus.Caption = string.Format("Cmdr. {0}, Ship: {1}",
													this.CommanderName, this.ShipIDName);
										//Aplica el Tema:
										ApplyTheme(false, true);

										//Envia F11 al Juego, para Refrescar los Colores:
										if (this.GameWindow != null && !this.GameWindow.HasExited)
										{
											IntPtr h = this.GameWindow.MainWindowHandle;
											SetForegroundWindow(h);

											System.Threading.Thread.Sleep(3000);
											SendKeys.SendWait("{F11}");

											//System.Threading.Thread.Sleep(3000); //<- Espera 3 segundos
											//SendKeys.SendWait("{F11}"); //Envia la Tecla x segunda vez

											this.ApplyingShip = false;
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
						string JsonSettings_path = Path.Combine(this.AppExePath, "Data", "UI_Tips.json");

						if (File.Exists(JsonSettings_path)) _Tips = Util.DeSerialize_FromJSON<List<UI_Tips>>(JsonSettings_path);
						if (_Tips.IsNotEmpty())
						{
							//Carga el Idioma del Usuario:
							UI_Tips _TipsLang = _Tips.Find(x => x.Name == "Tips" && x.Language == this.LangShort);
							if (_TipsLang == null)
							{
								_TipsLang = _Tips.Find(x => x.Name == "Tips" && x.Language == "en");
							}
							if (_TipsLang != null)
							{
								//Actualiza los Controles fuera de este Proceso:	
								Invoke((MethodInvoker)(() =>
								{
									this.listTips.DataSource = _TipsLang.Elements;

									this.dockTips.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
									this.dockTips.Text = _TipsLang.Title;

									if (this.ShowTips || ForceShow)
									{
										this.dockTips.ShowSliding();
									}
								}));
							}
						}
					});
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void SearchElement(string _NameFilter)
		{
			try
			{
				if (this.Settings != null && this.Settings.ui_groups.IsNotEmpty())
				{
					this.Cursor = Cursors.WaitCursor;
					var t = Task.Factory.StartNew(delegate
					{
						List<element> _Results = new List<element>();

						foreach (ui_group _UIGroup in this.Settings.ui_groups)
						{
							List<element> _Elements = _UIGroup.Elements.FindAll(obj => obj.Title.ToUpper().Contains(_NameFilter.ToUpper()) || 
																						obj.Category.ToUpper().Contains(_NameFilter.ToUpper()));
							if (_Elements.IsNotEmpty())
							{
								foreach (var _Element in _Elements)
								{
									_Element.Parent = _UIGroup.Name;
								}
								_Results.AddRange(_Elements);
							}							
						}

						if (_Results.IsNotEmpty())
						{
							//Actualiza los Controles fuera de este Proceso:	
							Invoke((MethodInvoker)(() =>
							{
								this.gridSearch.DataSource = _Results;
								this.gridView_Search.ExpandAllGroups();
							}));
						}
					});

				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
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
			if (this.CboGameInstances.EditValue != null)
			{
				this.ActiveInstance = (game_instance)(this.CboGameInstances.Edit as RepositoryItemLookUpEdit)
					.GetDataSourceRowByKeyValue(this.CboGameInstances.EditValue);

				this.vGridDetalles.Rows.Clear();
				//this.accordionControl1.Elements.Clear();
				Application.DoEvents();

				Util.WinReg_WriteKey("EDHM", "ActiveInstance", this.ActiveInstance.game_id);

				LoadGameInstance(this.ActiveInstance, this.LangShort);  //<- Carga La Instancia Activa	
				LoadGlobalSettings(this.ActiveInstance); //<- Carga los Valores Globales
				LoadThemeList_EX();

				this.dockManager1.ActivePanel = this.dockThemes;
			}
		}

		/* AQUI SE SELECCIONA UN THEMA PARA CARGAR  */
		private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
		{
			/* AL SELECCIONAR UN TEMA DE LA LISTA  */
			DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
			if (view.IsRowSelected(e.RowHandle) && e.Column.FieldName == "Preview")
			{
				ui_preset_new _theme = view.GetRow(e.RowHandle) as ui_preset_new;
				if (_theme != null)
				{
					this.SelectedTheme = _theme;
					switch (this.ActiveInstance.key)
					{
						case "ED_Horizons": LoadTheme_Horizons(_theme); break;
						case "ED_Odissey": LoadTheme_Odissey(_theme); break;
						default: break;
					}
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
					if (this.ActiveInstance.key == "ED_Horizons")
					{
						XML_Form _Form = new XML_Form(this.ActiveInstance, this.Settings.xml_profile);
						if (_Form.ShowDialog() == DialogResult.OK)
						{
							this.Settings.xml_profile = _Form.xml_profile;
							PreviewTheme(true);
						}
					}
					else
					{
						XML_Form_Odyssey _Form = new XML_Form_Odyssey(this.ActiveInstance, this.Settings.xml_profile);
						if (_Form.ShowDialog() == DialogResult.OK)
						{
							this.Settings.xml_profile = _Form.xml_profile;
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
						ToggleSwitch _ToogleControl = sender as ToggleSwitch;
						_GroupName = _ToogleControl.Properties.Name.Split(new char[] { '|' })[0];
						if (_ToogleControl.Properties.Tag != null)
						{
							_SelectedElement = _ToogleControl.Properties.Tag as element;
							_SelectedElement.Value = Util.BoolToInteger(Util.ValidarNulo(_ToogleControl.EditValue, false));
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
				element _Element = e.Row.Tag as element;
				if (_Element != null)
				{
					this.lblDescription_Caption.Text = _Element.Title;
					this.lblDescription_Description.Text = _Element.Description;
				}
				else
				{
					this.lblDescription_Caption.Text = string.Empty;
					this.lblDescription_Description.Text = string.Empty;
				}
			}
		}

		#endregion

		#region Eventos de Controles

		private void tileNav_SetGameFolder_TileClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
		{
			//Opens the Settings Window:
			GameFolderForm _Form = new GameFolderForm(this.GameInstancesEx);
			if (_Form.ShowDialog() == DialogResult.OK)
			{
				this.GameInstancesEx = _Form.GameInstancesEx;

				string _RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");
				string GameInstances_JSON = Util.WinReg_ReadKey("EDHM", "GameInstances").NVL(string.Empty);

				this.GameInstancesEx = Util.DeSerialize_FromJSON_String<List<GameInstance>>(GameInstances_JSON);
				if (this.GameInstancesEx != null && this.GameInstancesEx.Count > 0)
				{
					foreach (var _instance in this.GameInstancesEx)
					{
						foreach (var _game in _instance.games)
						{
							if (_game.game_id.EmptyOrNull())
							{
								_game.game_id = string.Format("{0}|{1}", _instance.instance, _game.key);
							}
						}
					}
					GameInstances_JSON = Util.Serialize_ToJSON(this.GameInstancesEx);
					Util.WinReg_WriteKey("EDHM", "GameInstances", GameInstances_JSON);
				}				

				//Carga los valores que se muestran en el Combo:
				this.GameInstances = new List<game_instance>();
				foreach (var _Instance in this.GameInstancesEx)
				{
					foreach (var _Game in _Instance.games)
					{
						_Game.instance = string.Format("{0} ({1})", _Game.name, _Instance.instance);
						this.GameInstances.Add(_Game);
					}
				}

				string[] _ActiveGames = _RegActiveInstance.Split(new char[] { '|' });
				if (_ActiveGames != null && _ActiveGames.Length > 1)
				{
					this.ActiveInstance = this.GameInstances.Find(x => x.game_id == _RegActiveInstance);
				}
				else
				{
					_RegActiveInstance = string.Format("{0}|{1}", this.GameInstancesEx[0].instance, _RegActiveInstance.NVL("ED_Horizons"));
					this.ActiveInstance = this.GameInstances.Find(x => x.game_id == _RegActiveInstance);
					Util.WinReg_WriteKey("EDHM", "ActiveInstance", _RegActiveInstance);
				}

				this.repCboGameInstances.ValueMember = "game_id";
				this.repCboGameInstances.DisplayMember = "instance";
				this.repCboGameInstances.DataSource = this.GameInstances;


				//this.ActiveInstance = this.GameInstances.Find(x => x.key == _RegActiveInstance);
				if (this.ActiveInstance != null)
				{
					this.CboGameInstances.EditValue = this.ActiveInstance.game_id;

					//Carga el Idioma del Usuario:
					this.LangShort = Util.WinReg_ReadKey("EDHM", "Language").NVL("en");

					LoadGameInstance(this.ActiveInstance, this.LangShort);  //<- Carga La Instancia Activa	
				}

				this.HideToTray = _Form.HideToTray;
				this.GreetMe = _Form.GreetMe;
				this.WatchMe = _Form.WatchMe;
			}
		}
		private void tileNav_OpenGameFolder_TileClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
		{
			if (this.ActiveInstance != null && !this.ActiveInstance.path.EmptyOrNull())
			{
				//Abrir una carpeta en el Explorador de Windows:
				System.Diagnostics.Process.Start(this.ActiveInstance.path);
			}
		}
		private void tileNav_About_TileClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
		{
			AboutForm _form = new AboutForm();
			_form.ShowDialog();
		}
		private void tileNav_KeyBindings_ElementClick(object sender, NavElementEventArgs e)
		{
			//XtraMessageBox.Show("HUD Dimmer = [F5]\r\nLighting Dimmer = [F6]\r\nAmbient Cabin Lights = [CTRL]+[F6]\r\nHUD Common Group = [CTRL]+[F5]\r\nDistributor = [ALT]+[F5]\r\nShield colour =[F2]\r\nPanel Lines (upper) = [CTRL]+[F4]\r\nPanel Lines (lower) = [ALT]+[F4]\r\nCombat HUD Colour = [CTRL]+[F2]\r\nCombat HUD Mode = [SHIFT]+[F2]\r\nAnalysis HUD Colour = [ALT]+[F2]\r\nRadar Colour = [F4]\r\nTargeting Reticle = [F7]\r\nSignature Bar Colour = [SHIFT]+[F4]\r\nEnvironment Target = [CTRL]+[F7]\r\nOwnShip Hologram = [F3]\r\nOwnShip Hologram Mode = [CTRL F3]\r\nReload Settings = [F11]\r\nKill Switch = [SHIFT]+[F1]",
			//	"EDHM KeyBindings:");

			/*  MUESTRA EL HISTORIAL DE NAVES Y SUS TEMAS ASOCIADOS  */
			ShipyardForm _Form = new ShipyardForm
			{
				ActiveInstance = this.ActiveInstance,
				UI_Themes = (List<ui_preset_new>)this.UI_Themes.Clone(),
				Shipyard = this.Shipyard
			};
			if (_Form.ShowDialog() == DialogResult.OK)
			{
				this.Shipyard = _Form.Shipyard;
			}
		}
		private void tileUpdateEDHM_ElementClick(object sender, NavElementEventArgs e)
		{
			this.SilentUpdate = false;

			//CheckForModUpdates();
			if (!CheckGameRunning())
			{
				InstallGameInstance(this.ActiveInstance);

				//Cargar Los Valores Base de La Instancia:
				string JsonSettings_path = Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_Settings.json");
				if (File.Exists(JsonSettings_path))
				{
					this.Settings = Util.DeSerialize_FromJSON<ui_setting>(JsonSettings_path);
				}

				//Carga la Lista de Presets disponibles:
				string JsonPresets_path = Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_Presets.json");
				if (File.Exists(JsonPresets_path))
				{
					this._ElementPresets = Util.DeSerialize_FromJSON<List<combo_item>>(JsonPresets_path);
				}

				//Carga la Lista de Temas
				LoadThemeList_EX();

				string search = this.ActiveInstance.key == "ED_Horizons" ? "HORIZ" : "ODYSS";
				this.lblVersion_App.Caption = string.Format("App Version: {0}", System.Configuration.ConfigurationManager.AppSettings["AppVersion"].ToString());
				this.lblVersion_MOD.Caption = string.Format("Mod Version: {0}", Util.WinReg_ReadKey("EDHM", string.Format("Version_{0}", search)).NVL("v1.51"));
			}
			else
			{
				XtraMessageBox.Show("Please Close the Game and try again.", "Game Most Be Closed!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}
		private void tileUninstallEDHM_TileClick(object sender, NavElementEventArgs e)
		{
			UninstallEDHMmod();

			//System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			//var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			//{
			//	System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;



			//});

			//string Uninstall_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-Uninstall.bat");
			//if (File.Exists(Uninstall_Path))
			//{
			//	if (XtraMessageBox.Show("This will Clean Un-Install EDHM Mod from your Elite Dangerous Game.\r\nDo you want to Continue?",
			//		"Un-Install EDHM?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			//	{
			//		ExecuteBAT(Uninstall_Path);
			//	}
			//}
			//else
			//{
			//	XtraMessageBox.Show("Un-Installer BAT could not be found!", "ERROR 404");
			//}
		}
		private void tile3PMods_TileClick(object sender, NavElementEventArgs e)
		{
			try
			{
				TPModsManager _Form = new TPModsManager(this.ActiveInstance);
				_Form.ShowDialog();
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void mnuTray_Exit_Click(object sender, EventArgs e)
		{
			this.mCloseAutorized = true;
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

		private void tileGetHelp_ElementClick(object sender, NavElementEventArgs e)
		{
			//abre con el programa predeterminado:
			if (File.Exists(Path.Combine(this.AppExePath, @"Data\EDHM_UI_Guide.pdf")))
			{
				System.Diagnostics.Process.Start(Path.Combine(this.AppExePath, @"Data\EDHM_UI_Guide.pdf"));
			}
			Load_UITips(true);
		}
		private void navButton_Themes_ElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
		{
			this.dockManager1.ActivePanel = this.dockThemes;
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
			Util.AppConfig_SetValue("ShowTips", (!this.chkTips_NoShow.Checked).ToString());
		}

		#endregion

		#region Botonera

		private void navApplyTheme_ElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
		{
			ApplyTheme(true);
		}
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
			try
			{
				if (this.SelectedTheme != null && this.SelectedTheme.name != "Current Settings")
				{
					string GameFolder = Path.Combine(this.ActiveInstance.path, @"EDHM-ini");
					string NewProfileFolder = Path.Combine(this.ActiveInstance.path, this.ActiveInstance.themes_folder, this.SelectedTheme.name);

					//1. Crear la Carpeta para el Nuevo Perfil, si ya Existe, se Sobreescribe:
					DirectoryInfo _ProfileDir = System.IO.Directory.CreateDirectory(NewProfileFolder);
					if (_ProfileDir != null)
					{
						//2. Copiar los Archivos Plantilla con la Ultima Version del MOD:
						if (File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Startup-Profile.ini", this.ActiveInstance.key))))
						{
							File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Startup-Profile.ini", this.ActiveInstance.key)),
								Path.Combine(GameFolder, @"Startup-Profile.ini"), true);
						}
						if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Advanced.ini", this.ActiveInstance.key))))
						{
							File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_Advanced.ini", this.ActiveInstance.key)),
								Path.Combine(GameFolder, @"Advanced.ini"), true);
						}
						if (this.ActiveInstance.key == "ED_Odissey" && File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_SuitHud.ini", this.ActiveInstance.key))))
						{
							File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_SuitHud.ini", this.ActiveInstance.key)),
								Path.Combine(GameFolder, @"SuitHud.ini"), true);
						}
						if (File.Exists(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_XML-Profile.ini", this.ActiveInstance.key))))
						{
							File.Copy(Path.Combine(this.AppExePath, string.Format(@"Data\{0}_XML-Profile.ini", this.ActiveInstance.key)),
								Path.Combine(GameFolder, @"XML-Profile.ini"), true);
						}

						ApplyTheme(false, true); //<- Aplica los cambios Actuales

						//Copiar los Archivos del Tema Actual: // existing files will be overwritten
						File.Copy(Path.Combine(GameFolder, @"Startup-Profile.ini"),
							Path.Combine(NewProfileFolder, @"Startup-Profile.ini"), true);

						File.Copy(Path.Combine(GameFolder, "XML-Profile.ini"),
							Path.Combine(NewProfileFolder, @"XML-Profile.ini"), true);

						if (this.ActiveInstance.key == "ED_Odissey")
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

						LoadThemeList_EX();

						//MUESTRA UN MENSAJE QUE SE CIERRA AUTOMATICAMENTE EN 2 SEGUNDOS:
						XtraMessageBoxArgs args = new XtraMessageBoxArgs()
						{
							Caption = "Done",
							Text = string.Format("The profile '{0}' has successfully been Saved.", this.SelectedTheme.name),
							Buttons = new DialogResult[] { DialogResult.OK }
						};
						args.AutoCloseOptions.Delay = 2000;
						args.AutoCloseOptions.ShowTimerOnDefaultButton = true;

						XtraMessageBox.Show(args).ToString();
					}
				}
				else
				{
					CreateNewTheme();
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}
		private void cmdSaveThemeChanges_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			SaveTheme();
		}
		private void cmdShowPreview_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			/* Huevo de Pascua, sólo en Odissey  */
			if (this.ActiveInstance.key == "ED_Odissey")
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
			if (this.SelectedTheme != null && this.SelectedTheme.name != "Current Settings")
			{
				if (XtraMessageBox.Show(string.Format("Are you Sure of Deleting the theme '{0}'?", this.SelectedTheme.name), "Confirm?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					if (Directory.Exists(this.SelectedTheme.folder))
					{
						DirectoryInfo dir = new DirectoryInfo(this.SelectedTheme.folder);
						dir.Delete(true);

						LoadThemeList_EX();
					}
				}
			}

		}

		#endregion

		#region Search

		private void txtSeach_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			SearchElement(this.txtSeach.Text);
		}
		private void txtSeach_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				//Se Presionó la Tecla ENTER
				SearchElement(this.txtSeach.Text);
			}
		}

		private void gridView_Search_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
		{
			//Al seleccionar uno de los resulados, lo muestra en la ventana de Propiedades
			DevExpress.XtraGrid.Views.Grid.GridView View = sender as DevExpress.XtraGrid.Views.Grid.GridView;
			if (View != null)
			{
				element _Element = (element)View.GetFocusedRow();
				if (_Element != null)
				{
					LoadGroupSettings(_Element.Parent, _Element.Title);
				}
			}
		}

		private void txtSearchBox_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			this.txtSeach.Text = this.txtSearchBox.Text;
			if (this.panelSearch.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible) this.panelSearch.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
			this.panelSearch.ShowSliding();
			SearchElement(this.txtSearchBox.Text);
		}
		private void txtSearchBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				//Se Presionó la Tecla ENTER
				this.txtSeach.Text = this.txtSearchBox.Text;
				if (this.panelSearch.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible) this.panelSearch.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
				this.panelSearch.ShowSliding();
				SearchElement(this.txtSearchBox.Text);
			}
		}

		#endregion

		#region Global Settings

		private void vGridDetalles_Click(object sender, EventArgs e)
		{

		}
		private void vGridDetalles_MouseDown(object sender, MouseEventArgs e)
		{
			//Selecciona la Fila al dar Click Derecho
			if (e.Button == MouseButtons.Right)
			{
				DevExpress.XtraVerticalGrid.VGridHitInfo hInfo = this.vGridDetalles.CalcHitInfo(new Point(e.X, e.Y));
				if (hInfo != null && hInfo.HitInfoType == DevExpress.XtraVerticalGrid.HitInfoTypeEnum.HeaderCell)
				{
					this.vGridDetalles.FocusedRow = hInfo.Row;
					this.vGridDetalles.FocusedRecordCellIndex = hInfo.CellIndex;
					this.vGridDetalles.FocusedRecord = hInfo.RecordIndex;
					this.vGridDetalles.ShowEditor();
				}
			}
		}

		private void mnuGlobalSettings_Add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				var _Selected = this.vGridDetalles.FocusedRow;
				if (_Selected != null)
				{
					element _Element = (element)_Selected.Tag;
					if (_Element != null)
					{
						if (XtraMessageBox.Show(string.Format("You want to add '{0}' to the Global Settings List?", _Element.Title),
							"Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							if (this.GlobalSettings is null) this.GlobalSettings = new ui_group("GlobalSettings", "Global Settings");							

							element _Existe = null;
							if (this.GlobalSettings.Elements.IsNotEmpty())
								_Existe = this.GlobalSettings.Elements.Find(x => x.Key == _Element.Key);

							if (_Existe is null)
							{
								if (this.GlobalSettings.Elements is null) this.GlobalSettings.Elements = new List<element>();
								this.GlobalSettings.Elements.Add(_Element);

								//Guarda los datos en el JSON:
								if (this.GlobalSettings != null)
								{
									Util.Serialize_ToJSON(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_Global_Settings.json"), this.GlobalSettings);

									LoadGlobalSettings(this.ActiveInstance);
									this.dockManager1.ActivePanel = this.dockGlobalSettings;
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
				var _Selected = this.vGridGlobalSettings.FocusedRow;
				if (_Selected != null)
				{
					element _Element = (element)_Selected.Tag;
					if (_Element != null)
					{
						if (XtraMessageBox.Show(string.Format("You want to Remove '{0}' from the Global Settings List?", _Element.Title),
							"Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{							
							if (this.GlobalSettings != null)
							{
								this.GlobalSettings.Elements.Remove(_Element);

								Util.Serialize_ToJSON(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_Global_Settings.json"), this.GlobalSettings);

								LoadGlobalSettings(this.ActiveInstance);
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
					this.GlobalSettings_Title.Text = _Element.Title;
					this.GlobalSettings_Description.Text = _Element.Description;
				}
				else
				{
					this.GlobalSettings_Title.Text = string.Empty;
					this.GlobalSettings_Description.Text = string.Empty;
				}
			}
		}
		private void vGridGlobalSettings_MouseDown(object sender, MouseEventArgs e)
		{
			//Selecciona la Fila al dar Click Derecho
			if (e.Button == MouseButtons.Right)
			{
				DevExpress.XtraVerticalGrid.VGridHitInfo hInfo = this.vGridGlobalSettings.CalcHitInfo(new Point(e.X, e.Y));
				if (hInfo != null && hInfo.HitInfoType == DevExpress.XtraVerticalGrid.HitInfoTypeEnum.HeaderCell)
				{
					this.vGridGlobalSettings.FocusedRow = hInfo.Row;
					this.vGridGlobalSettings.FocusedRecordCellIndex = hInfo.CellIndex;
					this.vGridGlobalSettings.FocusedRecord = hInfo.RecordIndex;
					this.vGridGlobalSettings.ShowEditor();
				}
			}
		}



		#endregion

		private void toolTipController1_GetActiveObjectInfo(object sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
		{
			//Muestra un tooltip sobre el tema al pasar el mosue
			if (e.Info == null && e.SelectedControl == this.gridControl1)
			{
				DevExpress.XtraGrid.Views.Grid.GridView view = this.gridControl1.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView;
				DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitInfo info = view.CalcHitInfo(e.ControlMousePosition);
				if (info.InRowCell)
				{
					ui_preset_new _theme = view.GetRow(info.RowHandle) as ui_preset_new;
					if (_theme != null)
					{
						string cellKey = info.RowHandle.ToString() + " - " + info.Column.ToString();
						e.Info = new ToolTipControlInfo(cellKey, string.Format("By {0}", _theme.author), _theme.name);
					}					
				}
			}
		}

		private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
		{
			//Dibuja sobre el tema seleccionaod
			DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;

			if (view.IsRowSelected(e.RowHandle) && e.Column.FieldName == "Preview")
			{
				ui_preset_new _theme = view.GetRow(e.RowHandle) as ui_preset_new;
				if (_theme != null)
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

		private void cmdThemes_ShowFavorites_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			//Filtra mostrando solo los temas favoritos
			BarToggleSwitchItem _Toogle = sender as BarToggleSwitchItem;
			if (_Toogle.Checked)
			{
				//CriteriaOperator expr1 = new BinaryOperator("IsFavorite", true);
				gridView1.ActiveFilterCriteria = new BinaryOperator("IsFavorite", true);
			}
			else
			{
				gridView1.ActiveFilterCriteria = null;
			}
			Util.WinReg_WriteKey("EDHM", "FavToogle", _Toogle.Checked.ToString());
		}

		private void repGridThemes_Favorite_EditValueChanged(object sender, EventArgs e)
		{
			this.gridView1.PostEditor(); //<- Actualiza el DataSource de la Grilla inmediatamente
			gridView1.UpdateCurrentRow();
		}
	}
}