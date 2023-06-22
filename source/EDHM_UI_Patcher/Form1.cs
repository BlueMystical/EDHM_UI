using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace EDHM_UI_Patcher
{
    public partial class Form1 : Form
    {
        private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
        private string InfoJsonPath = string.Empty;
		private string LangShort = string.Empty;

		private const string ProcessName = "EDHM_UI_mk2";
		private const string WindowTitle = "Elite - Dangerous (CLIENT)";

		private VersionInfo _VersionInfo = null;    //<- Version Publicada
        private Version Curr_Version = null;        //<- Version en la PC local
        private Version Last_Version = null;        //<- Ultima Version Disponible	
        private Version Inst_Version = null;        //<- Ultimo Instalador

        private bool _Instalar = false;             //<- Hay que bajar y correr el Instalador
        private bool _Parchear = false;             //<- Hay que bajar y Correr el Parche
        private bool _LastIsInstaller = false;      //<- La Ultima version es un Instalador

        private bool InstallerIsDownloading = false; //<- The Installer is Downloading
        private bool PatcherIsDownloading = false;   //<- The Patch Update is Downloading

        public Form1()
        {
            InitializeComponent();
        }
        public Form1(string _InfoJsonPath)
        {
            InitializeComponent();

            if (!_InfoJsonPath.EmptyOrNull())
            {
                if (File.Exists(_InfoJsonPath))
                {
                    this.InfoJsonPath = _InfoJsonPath;
                    this._VersionInfo = Util.DeSerialize_FromJSON<VersionInfo>(_InfoJsonPath);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Obligar a usar los puntos y las comas;
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            customCulture.NumberFormat.NumberGroupSeparator = ",";
            customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            customCulture.NumberFormat.CurrencyGroupSeparator = ",";
            customCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            customCulture.DateTimeFormat.LongDatePattern = "dddd, MMMM d, yyyy";
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (this._VersionInfo != null)
            {
                this.label1.Text = string.Format("v{0}", this._VersionInfo.app_version);
                this.textBox1.Text = this._VersionInfo.changelog;

                this.Curr_Version = new Version(this._VersionInfo.cur_version);         //<- Version en la PC local
                this.Last_Version = new Version(this._VersionInfo.app_version);         //<- Ultima Version Disponible		
                this.Inst_Version = new Version(this._VersionInfo.last_installer);      //<- Ultimo Instalador

                this._Instalar = this.Inst_Version > this.Curr_Version ? true : false;            //<- Hay que bajar y correr el Instalador
                this._Parchear = this.Last_Version > this.Inst_Version ? true : false;            //<- Hay que bajar y Correr el Parche
                this._LastIsInstaller = this.Last_Version == this.Inst_Version ? true : false;    //<- La Ultima version es un Instalador
                this._Parchear = !this._LastIsInstaller;

                //this.textBox1.Text = string.Format("Local Version: {0}\r\nLast Installer: {1}\r\nLast Update: {2}",
                //							Curr_Version.ToString(), Inst_Version.ToString(), Last_Version.ToString() );
            }
            //Revisar si el programa corre como Administrador
            if (new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).
                                                IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                this.Text = "EDHM Live Patcher [ADMIN]";
            }
        }

        private void DescargarActualizacion_MOD(VersionInfo vErsion)
        {
            try
            {
                //Ruta donde se descarga el ZIP con la actualizacion:

                if (vErsion != null && !vErsion.download_url.EmptyOrNull())
                {
                    //this.Cursor = Cursors.WaitCursor;
                    this.lblInfo.Text = "Downloading, Please Wait.."; Application.DoEvents();
                    this.circularProgressBar1.Visible = true;
                    this.circularProgressBar1.Maximum = 100;
                    this.circularProgressBar1.Value_Inner = 0;
                    this.circularProgressBar1.Value = 0;

                    if (this._Instalar)
                    {
                        this.InstallerIsDownloading = true;
                        string FileToDownload = vErsion.install_url + "?" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string TempFilePath = Path.Combine(Path.GetTempPath(), "EDHM_UI", "EDHM_UI_Setup.msi");
						DateTime _startedAt = DateTime.MinValue; //<- Inicio de la Descarga

						FileDownloader FD = new FileDownloader(FileToDownload, TempFilePath);
                        FD.OnDownload_Progress += (sender, eventArgs) =>
                        {					

							//Muestra el Progreso de la Descarga:
							Invoke((MethodInvoker)(() =>
                            {
                                if (sender is double[] _Data) //<- Data[] = [0]:ProgressPercentage, [1]:BytesReceived, [2]:BytesTotal, [3]:Speed kb/s
								{
                                    if (this._Instalar && this._Parchear)
                                    {
                                        this.circularProgressBar1.Value_Inner = Convert.ToInt32(_Data[0]);
                                    }
                                    else
                                    {
                                        this.circularProgressBar1.Value = Convert.ToInt32(_Data[0]);
                                    }

									//Aqui se calcula la Velocidad de la Descarga:
									double bytesPerSecond = 0;
									if (_startedAt == default(DateTime))
									{
										_startedAt = DateTime.Now;
									}
									else
									{
										var timeSpan = DateTime.Now - _startedAt;										
										if (timeSpan.TotalSeconds > 0)
										{
											bytesPerSecond = _Data[1] / timeSpan.TotalSeconds;
										}
									}

									//Convierte los Bytes a la unidad más adecuada:
									string DSpeedReadable = Util.GetFileSize(Convert.ToInt64(bytesPerSecond), out double DSpeed);
									//Muestra la Velocidad de descarga en la Grafica:
									performanceChart1.AddValue(Convert.ToDecimal(DSpeed));
									//Muestra el Progreso de la descarga en la Etiqueta:
									this.lblInfo.Text = string.Format("Downloading {0} of {1} | {2}/s",
                                                        Util.GetFileSize(Convert.ToInt64(_Data[1])), //<- BytesReceived
														Util.GetFileSize(Convert.ToInt64(_Data[2])), //<- BytesTotal
														DSpeedReadable //_Data[3]    //<- Speed
														);
                                }
                            }));
                        };
                        FD.OnDownload_Error += (sender, eventArgs) =>
                        {
                            //Cuando Ocurre un Error
                            Invoke((MethodInvoker)(() =>
                                {
                                    if (sender is Exception _Data)
                                    {
                                        this.lblInfo.Text = _Data.Message;
                                    }
                                }));
                        };
                        FD.OnDownload_Complete += (sender, eventArgs) =>
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                this.InstallerIsDownloading = false;
                                this.lblInfo.Text = "Checking Game State.."; Application.DoEvents();

                                //Cuando termina la Descarga, Descomprimir el Archivo en la Ruta del Programa:
                                if (sender is long[] _Data)
                                {
                                    if (File.Exists(TempFilePath))
                                    {
										#region Update is Ready to Install, need to Exit the UI program now.

										//Busca un Proceso x Nombre de Ventana:
										System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
                                        System.Diagnostics.Process UI_Proc = null;
                                        System.Diagnostics.Process Game_Proc = null;

                                        foreach (System.Diagnostics.Process process in processlist)
                                        {
                                            if (!String.IsNullOrEmpty(process.ProcessName))
                                            {
												//Busca el proceso del UI app
                                                if (process.ProcessName == ProcessName)
                                                {
                                                    UI_Proc = process;
                                                }
												//Busca el proceso del Juego
                                                if (process.MainWindowTitle == WindowTitle)
                                                {
                                                    Game_Proc = process;
                                                }
                                                if (UI_Proc != null && Game_Proc != null) break;
                                            }
                                        }
										//Si la UI está abierta:
                                        if (UI_Proc != null)
                                        {
                                            this.lblInfo.Text = "Closing UI Program.."; Application.DoEvents();

                                            UI_Proc.EnableRaisingEvents = true;
                                            UI_Proc.Exited += (Sender, e) => //Ocurre cuando el Juego se Cierra:
                                            {
                                                //DoPatch(vErsion, TempFilePath);
                                            };
                                            UI_Proc.CloseMainWindow();
                                        }

										//Si el Juego esta Corriendo:
                                        if (Game_Proc != null)
                                        {
                                            this.lblInfo.Text = "Closing Game Client.."; Application.DoEvents();

                                            Game_Proc.EnableRaisingEvents = true;
                                            Game_Proc.Exited += (Sender, e) => //Ocurre cuando el Juego se Cierra:
                                            {
                                                DoPatch(vErsion, TempFilePath);
                                            };
                                            Game_Proc.CloseMainWindow();
                                        }
                                        else  //<- GAME IS NOT RUNNING
                                        {
                                            DoPatch(vErsion, TempFilePath);
                                        }
                                        processlist = null;

                                        #endregion
                                    }
                                }
                                else
                                {
                                    //MessageBox.Show(Ev.Error.Message + Ev.Error.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                                this.lblInfo.Text = "Done.";
                                this.Cursor = Cursors.Default;
                            }));
                        };
                        FD.StartDownload(); //<- Aqui se Inicia la Descarga

						performanceChart1.Visible = true;

					}

                    Application.DoEvents();

                    if (this._Parchear)
                    {
                        this.PatcherIsDownloading = true;

                        string FileToDownload = vErsion.download_url + "?" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string TempFilePath = Path.Combine(Path.GetTempPath(), @"EDHM_UI\EDHM_UI_Patch.zip");

                        FileDownloader FD2 = new FileDownloader(FileToDownload, TempFilePath);
                        FD2.OnDownload_Progress += (sender, eventArgs) =>
                        {
                            //Muestra el Progreso de la Descarga:
                            Invoke((MethodInvoker)(() =>
                            {
                                if (sender is double[] _Data) //<- ProgressPercentage, BytesReceived, BytesTotal, Speed
                                {
                                    this.circularProgressBar1.Value = Convert.ToInt32(_Data[0]);

                                    if (!this.InstallerIsDownloading)
                                    {
                                        this.lblInfo.Text = string.Format("Downloading {0} of {1} | {2:n0} kb/s",
                                                            Util.GetFileSize(Convert.ToInt64(_Data[1])),
                                                            Util.GetFileSize(Convert.ToInt64(_Data[2])),
                                                            _Data[3]);
                                    }
                                }
                            }));
                        };
                        FD2.OnDownload_Error += (sender, eventArgs) =>
                        {
                            //Cuando Ocurre un Error
                            Invoke((MethodInvoker)(() =>
                            {
                                if (sender is Exception _Data)
                                {
                                    this.lblInfo.Text = _Data.Message;
                                }
                            }));
                        };
                        FD2.OnDownload_Complete += (sender, eventArgs) =>
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                this.PatcherIsDownloading = false;

                                //Cuando termina la Descarga, Descomprimir el Archivo en la Ruta del Programa:
                                long[] _Data = sender as long[];
                                if (_Data != null)
                                {
                                    if (File.Exists(TempFilePath))
                                    {
                                        #region Update is Ready to Install, need to Exit the UI program now.

                                        //Busca un Proceso x Nombre de Ventana:
                                        string ProcessName = "EDHM_UI_mk2";
                                        string WindowTitle = "Elite - Dangerous (CLIENT)";

                                        System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
                                        System.Diagnostics.Process UI_Proc = null;
                                        System.Diagnostics.Process Game_Proc = null;

                                        foreach (System.Diagnostics.Process process in processlist)
                                        {
                                            if (!String.IsNullOrEmpty(process.ProcessName))
                                            {
                                                if (process.ProcessName == ProcessName)
                                                {
                                                    UI_Proc = process;
                                                }
                                                if (process.MainWindowTitle == WindowTitle)
                                                {
                                                    Game_Proc = process;
                                                }
                                                if (UI_Proc != null && Game_Proc != null) break;
                                            }
                                        }
                                        if (UI_Proc != null)
                                        {
                                            this.lblInfo.Text = "Closing UI Program.."; Application.DoEvents();

                                            UI_Proc.EnableRaisingEvents = true;
                                            UI_Proc.Exited += (Sender, e) => //Ocurre cuando el Juego se Cierra:
                                            {
                                                //DoPatch(vErsion, TempFilePath);
                                            };
                                            UI_Proc.CloseMainWindow();
                                        }
                                        else  //<- UI IS NOT RUNNING
                                        {
                                            //DoPatch(vErsion, TempFilePath);
                                        }

                                        if (Game_Proc != null)
                                        {
                                            this.lblInfo.Text = "Closing Game Client.."; Application.DoEvents();

                                            Game_Proc.EnableRaisingEvents = true;
                                            Game_Proc.Exited += (Sender, e) => //Ocurre cuando el Juego se Cierra:
                                            {
                                                DoPatch(vErsion, TempFilePath);
                                            };
                                            Game_Proc.CloseMainWindow();
                                        }
                                        else  //<- GAME IS NOT RUNNING
                                        {
                                            DoPatch(vErsion, TempFilePath);
                                        }
                                        processlist = null;

                                        #endregion
                                    }
                                }
                                else
                                {
                                    //MessageBox.Show(Ev.Error.Message + Ev.Error.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                                this.lblInfo.Text = "Done.";
                                this.Cursor = Cursors.Default;
                            }));
                        };
                        FD2.StartDownload(); //<- Aqui se Inicia la Descarga
                    }

					if (!this._Instalar && !this._Parchear)
					{
						this.lblInfo.Text = @"Everything is Updated, nothing to do here ¯\_(ツ)_/¯.";
						this.Cursor = Cursors.Default;
						//this.cmdUpdate.Enabled = true;
					}

					//TODO: Este descarga en multiples procesos:
					//Downloader D = new Downloader();
					//var result = D.Download(FileToDownload, Path.Combine(Path.GetTempPath(), "EDHM_UI"), 2);
					//if (result != null)
					//{
					//	double Speed = Math.Round((result.Size / 1024) / result.TimeTaken.TotalSeconds, 2);
					//	Console.WriteLine(string.Format("Time: {0:n0}s | Speed: {1:n2} kb/s", result.TimeTaken.TotalSeconds, Speed));
					//}
				}
            }
            catch (ThreadAbortException tex)
            {
                //System.Threading.Thread.Join();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private void DoPatch(VersionInfo vErsion, string TempFilePath)
        {
            try
            {
                if (this._Instalar)
                {
                    Invoke((MethodInvoker)(() => { this.lblInfo.Text = "Installing Update.."; }));

                    //Launch the MSI Installer and waits for it:
                    var Installer_PROC = System.Diagnostics.Process.Start(TempFilePath);
                    Installer_PROC.EnableRaisingEvents = true;
                    if (this._Parchear) Installer_PROC.WaitForExit();
                }              

                if (this._Parchear)
                {
                    #region Patch is Ready to Install, need to Exit the UI program now.

                    //Busca un Proceso x Nombre de Ventana:
                    string ProcessName = "EDHM_UI_mk2";
                    string WindowTitle = "Elite - Dangerous (CLIENT)";

                    System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
                    System.Diagnostics.Process UI_Proc = null;
                    System.Diagnostics.Process Game_Proc = null;

                    foreach (System.Diagnostics.Process process in processlist)
                    {
                        if (!String.IsNullOrEmpty(process.ProcessName))
                        {
                            if (process.ProcessName == ProcessName)
                            {
                                UI_Proc = process;
                            }
                            if (process.MainWindowTitle == WindowTitle)
                            {
                                Game_Proc = process;
                            }
                            if (UI_Proc != null && Game_Proc != null) break;
                        }
                    }
                    if (UI_Proc != null)
                    {
                        this.lblInfo.Text = "Closing UI Program.."; Application.DoEvents();
                        UI_Proc.CloseMainWindow();
                        UI_Proc.WaitForExit();
                        if (UI_Proc != null && UI_Proc.HasExited == false) UI_Proc.Kill();
                    }
                    else  //<- UI IS NOT RUNNING
                    {
                        //DoPatch(vErsion, TempFilePath);
                    }

                    if (Game_Proc != null)
                    {
                        this.lblInfo.Text = "Closing Game Client.."; Application.DoEvents();
                        Game_Proc.CloseMainWindow();
                        Game_Proc.WaitForExit();
                        if (Game_Proc != null && Game_Proc.HasExited == false) Game_Proc.Kill();
                    }
                    else  //<- GAME IS NOT RUNNING
                    {
                        //DoPatch(vErsion, TempFilePath);
                    }
                    processlist = null;

                    #endregion

                    Invoke((MethodInvoker)(() => { this.lblInfo.Text = "Patching Files.."; }));

                    //Descomprime el archivo descargado, reemplazando TODOS los archivos:
                    Util.DoNetZIP_UnCompressFile(TempFilePath,
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EDHM_UI"));

                    // Re-Launch the UI program telling it there is an Update:
                    Invoke((MethodInvoker)(() => { this.lblInfo.Text = "Re-starting UI Program.."; }));
                    System.Diagnostics.Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"EDHM_UI\EDHM_UI_mk2.exe"),
                                                                                    "-update " + vErsion.app_version);
                }

                //*** Gonna Move the HotFix into the UI program  ***/
                //if (vErsion.run_hotfix)
                //{
                //    Invoke((MethodInvoker)(() => { this.lblInfo.Text = "Running HotFix.."; }));
                //    Run_HotFix();
                //}

                Invoke((MethodInvoker)(() => { this.lblInfo.Text = "Done."; }));
                System.Threading.Thread.Sleep(3000); //<- Espera 3 segundos y Cierra este Programa
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
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
                        /* AHORA ES MULTI-INSTANCIA !!!!!******  */
                        List<GameInstance> GameInstancesEx = null;

                        string _RegActiveInstance = Util.WinReg_ReadKey("EDHM", "ActiveInstance").NVL("ED_Horizons");
                        string GameInstances_JSON = Util.WinReg_ReadKey("EDHM", "GameInstances").NVL(string.Empty);
						string UI_Documents = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Elite Dangerous\EDHM_UI");

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

                                foreach (file_job _job in _Jobs)
                                {
                                    string GamePath = _job.game == "ODYSSEY" ? ODYS_Path : HORI_Path;
									string GameInstance = _job.game == "ODYSSEY" ? "ODYSS" : "HORIZ";									

									_job.file_path = _job.file_path.Replace("%GAME_PATH%", GamePath);
                                    _job.file_path = _job.file_path.Replace("%UI_PATH%", this.AppExePath);
									_job.file_path = _job.file_path.Replace("%UI_DOCS%", UI_Documents);

									if (_job.destination != null && _job.destination != string.Empty)
                                    {
                                        _job.destination = _job.destination.Replace("%GAME_PATH%", GamePath);
                                        _job.destination = _job.destination.Replace("%UI_PATH%", this.AppExePath);
                                    }

                                    try
                                    {
										switch (_job.action)
										{
											case "COPY":
												if (!Directory.Exists(Path.GetDirectoryName(_job.destination)))
													Directory.CreateDirectory(Path.GetDirectoryName(_job.destination));

												if (File.Exists(_job.file_path))
													File.Copy(_job.file_path, _job.destination, true);
												break;

											case "MOVE":
												if (File.Exists(_job.file_path))
												{
													if (!Directory.Exists(Path.GetDirectoryName(_job.destination)))
														Directory.CreateDirectory(Path.GetDirectoryName(_job.destination));

													File.Copy(_job.file_path, _job.destination, true);
													File.Delete(_job.file_path);
												}
												break;

											case "REPLACE":
												if (File.Exists(_job.file_path) && File.Exists(_job.destination))
												{
													File.Copy(_job.file_path, _job.destination, true);
												}
												break;

											case "DEL":
												if (File.Exists(_job.file_path))
													File.Delete(_job.file_path);
												break;

											case "RMDIR":
												if (Directory.Exists(_job.file_path))
													Directory.Delete(_job.file_path, true);
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

		/// <summary>Detecta si el juego esta Corriendo y Pregunta si lo debe Cerrar automaticamente.
		/// <para>Devuelve 'true' si el Juego No estaba corriendo o si fue Cerrado correctamente.</para>
		/// </summary>
		private bool KillGameProcces()
		{
			bool _ret = true;

			//Busca un Proceso x Nombre de Ventana:
			//string GameTitle = Util.AppConfig_GetValue("GameProcessID");
			System.Diagnostics.Process[] processlist = System.Diagnostics.Process.GetProcesses();
			System.Diagnostics.Process Game_Proc = null;

			foreach (System.Diagnostics.Process process in processlist)
			{
				if (!String.IsNullOrEmpty(process.MainWindowTitle))
				{
					if (process.MainWindowTitle == WindowTitle)
					{
						Game_Proc = process; //<- El juego está corriendo
						_ret = false;
						break;
					}
				}
			}
			if (Game_Proc != null)
			{
				string MSG_Title = string.Empty;
				string MSG_Body = string.Empty;

				LangShort = Util.WinReg_ReadKey("EDHM", "Language").NVL("en");

				switch (LangShort)
				{
					case "en":
						MSG_Title = "The Game is Running!";
						MSG_Body = "The Game needs to be closed in order to install any MODS.\r\nClick OK to close it now.";
						break;
					case "de":
						MSG_Title = "Das Spiel läuft!";
						MSG_Body = "Das Spiel muss geschlossen werden, um MODS zu installieren.\r\nKlicken Sie auf „Akzeptieren“, um es jetzt zu schließen.";
						break;
					case "fr":
						MSG_Title = "Le jeu tourne !";
						MSG_Body = "Le jeu doit être fermé pour pouvoir installer des MODS.\r\nCliquez sur Accepter pour le fermer maintenant.";
						break;
					case "ru":
						MSG_Title = "Игра запущена!";
						MSG_Body = "Игру необходимо закрыть, чтобы установить любые МОДЫ.\r\nНажмите «Принять», чтобы закрыть ее сейчас.";
						break;
					case "pt":
						MSG_Title = "O jogo está rodando!";
						MSG_Body = "O Jogo precisa ser fechado para instalar qualquer MODS.\r\nClique em Aceitar para fechá-lo agora.";
						break;
					case "it":
						MSG_Title = "Il gioco è in esecuzione!";
						MSG_Body = "Il gioco deve essere chiuso per poter installare eventuali MODS.\r\nFai clic su Accetta per chiuderlo ora.";
						break;
					case "es":
						MSG_Title = "¡El juego está en marcha!";
						MSG_Body = "El juego debe estar cerrado para poder instalar cualquier MOD.\r\nHaz clic en Aceptar para cerrarlo ahora.";
						break;
					default:
						MSG_Title = "Game is Running!";
						MSG_Body = "Game needs to be closed in order to install any MODS";
						break;
				}

				if (Mensajero.ShowDialogDark(MSG_Title, MSG_Body,
					MessageBoxButtons.OKCancel, MessageBoxIcon.Information, Language: this.LangShort) == DialogResult.OK)
				{
					if (Game_Proc != null && Game_Proc.HasExited == false)
					{
						//Game_Proc.CloseMainWindow();
						//Game_Proc.WaitForExit(5000);
						//if (Game_Proc != null && Game_Proc.HasExited == false) Game_Proc.Kill();
						Game_Proc.Kill();
					}
					System.Threading.Thread.Sleep(3000); //<- Espera 3 segundos
					_ret = (Game_Proc != null && Game_Proc.HasExited) ? true : false;
				}
			}
			return _ret;
		}


		private void cmdUpdate_Click(object sender, EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;
				Application.DoEvents();

				if (KillGameProcces())
				{
					var Boton = sender as Button;
					Boton.Enabled = false;
					DescargarActualizacion_MOD(this._VersionInfo);
				}
			}
			catch { }
			finally { this.Cursor = Cursors.Default; }			           
        }
    }

    [Serializable]
    public class VersionInfo
    {
        public VersionInfo() { }

        public string app_version { get; set; }
        public string cur_version { get; set; }
        public string last_installer { get; set; }

        public string ED_Horizons { get; set; }
        public string ED_Odissey { get; set; }

        public string download_url { get; set; }
        public string install_url { get; set; }

        public string changelog { get; set; }

        public bool re_install { get; set; }
        public bool run_hotfix { get; set; }
    }

    [Serializable]
    public class file_job
    {
        public file_job() { }

        public string game { get; set; }
        public string file_path { get; set; }
        public string action { get; set; }

        public string destination { get; set; }
    }

}
