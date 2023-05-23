using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EDHM_UI_Patcher
{
	public static class Util
	{
		/// <summary>Devuelve 'true' si la Cadena es Nula o Vacia.</summary>
		/// <param name="pValor">Cadena de Texto</param>
		public static bool EmptyOrNull(this string Value, bool Default = true)
		{
			bool _ret = Default;
			if (Value != null && Value != string.Empty)
			{
				_ret = false;
			}
			return _ret;
		}

		/// <summary>Devuelve un Valor por Defecto si la cadena es Null o Vacia.</summary>
		/// <param name="pTexto">Cadena de Texto</param>
		/// <param name="defaultValue">Valor x Defecto</param>
		/// <param name="considerWhiteSpaceIsEmpty">Los Espacios se consideran como Vacio?</param>
		public static string NVL(this object pTexto, string defaultValue, bool considerWhiteSpaceIsEmpty = true)
		{
			string _ret = defaultValue;
			if (pTexto != null)
			{
				if (considerWhiteSpaceIsEmpty)
				{
					if (pTexto.ToString().Trim() != string.Empty)
					{
						_ret = pTexto.ToString().Trim();
					}
				}
				else
				{
					_ret = pTexto.ToString();
				}
			}
			return _ret;
		}

        /// <summary>Serializa y escribe el objeto indicado en una cadena JSON.
        /// <para>El objeto (Clase) debe tener un Constructor sin Parametros definido.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">Tipo de Objeto al cual queremos convertir.</typeparam>
        /// <param name="objectToWrite">Instancia del Objeto que se va a Serializar.</param>
        public static string Serialize_ToJSON<T>(T objectToWrite) where T : new()
        {
            /* EJEMPLO:  string _JsonString = Util.Serialize_ToJSON(_Inventario);  */
            string _ret = string.Empty;
            try
            {
                _ret = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite);
            }
            catch { }
            return _ret;
        }

        public static T DeSerialize_FromJSON<T>(string filePath) where T : new()
        {
            /* EJEMPLO:  inventario _JSON = Util.DeSerialize_FromJSON_String<inventario>(Inventario_JSON);  */
            /* List<string> videogames = JsonConvert.DeserializeObject<List<string>>(json); */

            try
            {
                if (!filePath.EmptyOrNull())
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        using (TextReader reader = new StreamReader(filePath))
                        {
                            var fileContents = reader.ReadToEnd(); reader.Close();
                            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContents);
                        }
                    }
                    else
                    {
                        return default(T); //<- Si el Archivo NO Existe, les devuelvo un Objeto Vacio.
                    }
                }
                else
                {
                    return default(T); //<- Si me pasan un JSON vacio, les devuelvo un Objeto Vacio.
                }
            }
            finally { }
        }

        /// <summary>Serializa y escribe el objeto indicado en un archivo JSON.
        /// <para>La Clase a Serializar DEBE tener un Constructor sin parametros.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">El tipo de Objeto a guardar en el Archivo.</typeparam>
        /// <param name="filePath">Ruta completa al archivo donde se guardará el JSON.</param>
        /// <param name="objectToWrite">Instancia del Objeto a Serializar</param>
        /// <param name="append">'false'=Sobre-Escribe el Archivo, 'true'=Añade datos al final del archivo.</param>
        public static string Serialize_ToJSON<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            /* EJEMPLO:  string _JsonString = Util.Serialize_ToJSON(System.IO.Path.Combine(file_path,_file_name), _Inventario);  */

            string _ret = string.Empty;
            try
            {
                if (!filePath.EmptyOrNull())
                {
                    _ret = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite);
                    using (System.IO.TextWriter writer = new System.IO.StreamWriter(filePath, append))
                    {
                        writer.Write(_ret);
                        writer.Close();
                    };
                }
            }
            catch { }
            return _ret;
        }

        /// <summary>Crea una instancia de un Objeto leyendo sus datos desde una cadena JSON.
        /// <para>El objeto (Clase) debe tener un Constructor sin Parametros definido.</para></summary>
        /// <typeparam name="T">Tipo de Objeto al cual queremos convertir.</typeparam>
        /// <param name="JSONstring">Texto con formato JSON</param>
        public static T DeSerialize_FromJSON_String<T>(string JSONstring) where T : new()
        {
            /* EJEMPLO:  inventario _JSON = Util.DeSerialize_FromJSON_String<inventario>(Inventario_JSON);  */

            if (!JSONstring.EmptyOrNull())
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JSONstring);
            }
            else
            {
                return default(T); //<- Si me pasan un JSON vacio, les devuelvo un Objeto Vacio.
            }
        }


        /// <summary>Lee una Clave del Registro de Windows para el Usuario Actual.
        /// Las Claves en este caso siempre se Leen desde 'HKEY_CURRENT_USER\Software\Elte Dangerous\Mods\'.</summary>
        /// <param name="Sistema">Nombre del Sistema que guarda las Claves, ejem: RRHH, Contaduria, CutcsaPagos, etc.</param>
        /// <param name="KeyName">Nombre de la Clave a Leer</param>
        /// <returns>Devuelve NULL si la clave no existe</returns>
        public static object WinReg_ReadKey(string Sistema, string KeyName)
		{
			Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;
			Microsoft.Win32.RegistryKey sk1 = rk.OpenSubKey(@"Software\Elte Dangerous\Mods\" + Sistema);

			// Si la Clave no existe u ocurre un error al leerla, devuelve NULL
			if (sk1 == null)
			{
				return null;
			}
			else
			{
				try { return sk1.GetValue(KeyName); }
				catch { return null; }
			}
		}

		/// <summary>Escribe un Valor en una Clave del Registro de Windows para el Usuario Actual.
		/// Las Claves en este caso se Guardan siempre en 'HKEY_CURRENT_USER\Software\Cutcsa\DXCutcsa'.</summary>
		/// <param name="Sistema">Nombre del Sistema que guarda las Claves, ejem: RRHH, Contaduria, CutcsaPagos, etc.</param>
		/// <param name="KeyName">Nombre de la Clave a guardar, Si no existe se crea.</param>
		/// <param name="Value">Valor a Guardar</param>
		/// <returns>Devuelve TRUE si se guardo el valor Correctamente</returns>
		public static bool WinReg_WriteKey(string Sistema, string KeyName, object Value)
		{
			try
			{
				Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;
				Microsoft.Win32.RegistryKey sk1 = rk.CreateSubKey(@"Software\Elte Dangerous\Mods\" + Sistema);
				sk1.SetValue(KeyName, Value);

				return true; //<-La Clave se Guardo Exitosamente!
			}
			catch { return false; }
		}

		/// <summary>Convierte el tamaño de un archivo a la unidad más adecuada.</summary>
		/// <param name="pFileBytes">Tamaño del Archivo en Bytes</param>
		/// <returns>"0.### XB", ejem. "4.2 KB" or "1.434 GB"</returns>
		public static string GetFileSize(long pFileBytes)
		{
			// Get absolute value
			long absolute_i = (pFileBytes < 0 ? -pFileBytes : pFileBytes);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (pFileBytes >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (pFileBytes >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (pFileBytes >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (pFileBytes >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (pFileBytes >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = pFileBytes;
			}
			else
			{
				return pFileBytes.ToString("0 B"); // Byte
			}

			readable = System.Math.Round((readable / 1024), 2);
			return string.Format("{0:n1} {1}", readable, suffix);
		}
		public static string GetFileSize(long pFileBytes, out double ConvertedSize)
		{
			// Get absolute value
			long absolute_i = (pFileBytes < 0 ? -pFileBytes : pFileBytes);
			ConvertedSize = 0;

			// Determine the suffix and readable value
			string suffix;
			//double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				ConvertedSize = (pFileBytes >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				ConvertedSize = (pFileBytes >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				ConvertedSize = (pFileBytes >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				ConvertedSize = (pFileBytes >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				ConvertedSize = (pFileBytes >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				ConvertedSize = pFileBytes;
			}
			else
			{
				return pFileBytes.ToString("0 B"); // Byte
			}

			ConvertedSize = System.Math.Round((ConvertedSize / 1024), 2);
			return string.Format("{0:n2} {1}", ConvertedSize, suffix);
		}

		/// <summary>OBTIENE LA LISTA DE TODOS LOS ARCHIVOS Y SUB-DIRECTORIOS DENTRO DE LA RUTA ESPECIFICADA.
		/// Cada resultado incluye la ruta completa de cada archivo. </summary>
		/// <param name="path">Carpeta donde Buscar</param>
		/// <param name="searchPattern">puede ser '*.*' o '*.jpg'</param>
		/// <param name="searchOption"></param>
		public static IEnumerable<string> GetXFiles(string path, string searchPattern, SearchOption searchOption)
		{
			/* Sólo para .NET 4.5+  
			 * OBTIENE LA LISTA DE TODOS LOS ARCHIVOS Y SUB-DIRECTORIOS DENTRO DE LA RUTA ESPECIFICADA
			 */
			var foldersToProcess = new List<string>()
			{
				path
			};

			while (foldersToProcess.Count > 0)
			{
				string folder = foldersToProcess[0];
				foldersToProcess.RemoveAt(0);

				if (searchOption.HasFlag(SearchOption.AllDirectories))
				{
					//get subfolders
					try
					{
						var subfolders = Directory.GetDirectories(folder);
						foldersToProcess.AddRange(subfolders);
					}
					catch (Exception ex)
					{
						//log if you're interested
					}
				}

				//get files
				var files = new List<string>();
				try
				{
					files = Directory.GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly).ToList();
				}
				catch (Exception ex)
				{
					//log if you're interested
				}

				foreach (var file in files)
				{
					yield return file;
				}
			}
		}

		/// <summary>Obtiene la Ruta Relativa del Archivo. (Solo el nombre de las Carpetas)</summary>
		/// <param name="FilePath">Ruta Completa del Archivo</param>
		/// <param name="RootPath">Ruta del Directorio Padre</param>
		public static String GetRelativeFolder(string FilePath, string RootPath)
		{
			/* Si el archivo está en la Raiz, devuelve 'string.Empty', si hay varios niveles, devuelve 'Carpeta_1\Carpeta_2' */
			string _ret = string.Empty;
			try
			{
				string _PathOnly = System.IO.Path.GetDirectoryName(FilePath);
				_ret = _PathOnly.Replace(RootPath, string.Empty);
				if (_ret != string.Empty)
				{
					_ret = _ret.Remove(0, 1);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		public static Bitmap ResizeImage(Bitmap originalBitmap, int newWidth, int maxHeight, bool onlyResizeIfWider)
		{
			if (onlyResizeIfWider)
			{
				if (originalBitmap.Width <= newWidth)
				{
					newWidth = originalBitmap.Width;
				}
			}

			int newHeight = originalBitmap.Height * newWidth / originalBitmap.Width;
			if (newHeight > maxHeight)
			{
				// Resize with height instead
				newWidth = originalBitmap.Width * maxHeight / originalBitmap.Height;
				newHeight = maxHeight;
			}

			var alteredImage = new Bitmap(originalBitmap, new Size(newWidth, newHeight));
			alteredImage.SetResolution(72, 72);
			return alteredImage;
		}

		#region AppConfig

		/// <summary>Obtiene el Valor de una Clave guardada en el archivo 'App.config', en la Seccion 'appSettings'.</summary>
		/// <param name="Key">Nombre de la Clave que tiene el Valor</param>
		public static string AppConfig_GetValue(string Key)
		{
			string _ret = string.Empty;
			try
			{
				_ret = System.Configuration.ConfigurationManager.AppSettings[Key];
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		/// <summary>Guarda un Valor en una Clave de el archivo 'App.config', en la Seccion 'appSettings'.</summary>
		/// <param name="Key">Nombre de la Clave que tiene el Valor</param>
		/// <param name="Value">Valor a Guardar</param>
		public static bool AppConfig_SetValue(string Key, string Value)
		{
			bool _ret = false;
			try
			{
				System.Configuration.Configuration config =
					System.Configuration.ConfigurationManager.OpenExeConfiguration(
						System.Configuration.ConfigurationUserLevel.None);

				config.AppSettings.Settings[Key].Value = Value;
				config.Save(System.Configuration.ConfigurationSaveMode.Modified);

				System.Configuration.ConfigurationManager.RefreshSection("appSettings");
				_ret = true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		#endregion

		#region ZIP Files

		/// <summary>Comprime Una Carpeta con todos sus Sud-directorios y archivos.</summary>
		/// <param name="FolderPath">Ruta Completa de la Carpeta a Comprimir</param>
		/// <param name="ZIPfilePath">Ruta Completa del Archivo ZIP resultante</param>
		/// <param name="IncludeRootFolder">Indica se la carpeta raiz se incluye o no</param>
		public static void DoNetZIP_CompressFolder(string FolderPath, string ZIPfilePath, bool IncludeRootFolder = true)
		{
			try
			{
				if (Directory.Exists(FolderPath))
				{
					using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
					{
						//1. Obtener Todos los Sub-Directorios y su contenido
						List<string> _AllFiles = GetXFiles(FolderPath, "*.*", SearchOption.AllDirectories).ToList();
						if (_AllFiles != null && _AllFiles.Count > 0)
						{
							string RootFolder = new DirectoryInfo(FolderPath).Name;

							foreach (string _File in _AllFiles)
							{
								string Relative = GetRelativeFolder(_File, FolderPath);
								if (IncludeRootFolder)
								{
									Relative = Path.Combine(RootFolder, Relative);
								}

								if (Relative != string.Empty)
								{
									zip.AddFile(_File, Relative);
								}
								else
								{
									zip.AddFile(_File);
								}
							}

							zip.Save(ZIPfilePath);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Descomprime un ZIP en la Carpeta Indicada.
		/// <para>Si la Carpeta Destino no existe, la crea.</para>
		/// <para>Sobre-escribe los archivos existentes</para> </summary>
		/// <param name="ZIPfilePath">Ruta Completa del Archivo ZIP</param>
		/// <param name="DestinationFolder">Ruta Completa de la Carpeta Destino</param>
		public static bool DoNetZIP_UnCompressFile(string ZIPfilePath, string DestinationFolder)
		{
			bool _ret = false;
			try
			{
				if (File.Exists(ZIPfilePath))
				{
					if (!Directory.Exists(DestinationFolder)) Directory.CreateDirectory(DestinationFolder);

					using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(ZIPfilePath))
					{
						zip.ExtractAll(DestinationFolder, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
					}
					_ret = true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		#endregion
	}

	/// <summary>Clase para la Descarga Asincronica de Archivos.
	/// Autor: Jhollman Chacon R. 2021</summary>
	public class FileDownloader
	{
		#region Variables Privadas

		private bool _result = false;
		private readonly System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(0);
		private HttpWebRequest _request;
		private IAsyncResult _responseAsyncResult;
		private DateTime StartTime = DateTime.Now;

		#endregion

		#region Propiedades Publicas

		public string SaveFileName { get; set; } //<- Ruta Completa donde se guarda el archivo descargado.
		public string DownloadUrl { get; set; } //<- Direccion URL de Descarga
		public string UserAgent { get; set; }  //<- ID del gestor de Descarga, x defecto es Internet Explorer 11 sobre Windows 10 x64
		public byte[] DataDownloaded { get; set; } //<- Datos Descargados (en Binario)

		public bool DeleteExisting { get; set; } //<- TRUE: Si el archivo Existe, se borra. FALSE: Si el archivo existe, se le agregan datos.
		public bool AcceptRanges { get; set; }

		public bool UseProxy { get; set; }
		public string ProxyUser { get; set; }
		public string ProxyPass { get; set; }
		public string ProxyServer { get; set; }
		public int ProxyPort { get; set; }

		public long TotalBytesToReceive { get; set; }
		public long BytesReceived { get; set; }
		public Uri URL { get; set; }

		#endregion

		#region Eventos Publicos

		/// <summary>Progreso de la descarga:
		/// OnDownload_Progress(new long[] { percentage, bytesReceived, contentLength }, null); contentLength = Speed kb/s
		/// </summary>
		public event EventHandler OnDownload_Progress; //<- Progreso de la descarga

		/// <summary>Ocurre al Completarse la Descarga:
		/// long[] _Data = sender as long[];  [0]ProgressPercentage, [1]BytesReceived, [2]BytesTotal
		/// </summary>
		public event EventHandler OnDownload_Complete; //<- Descarga Terminada

		/// <summary>Muestra los Errores Ocurridos:
		/// Exception _Data = sender as Exception;
		/// </summary>
		public event EventHandler OnDownload_Error; //<- Error en la Descarga

		#endregion

		#region Constructor

		public FileDownloader(string url, string fullPathWhereToSave)
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
			if (string.IsNullOrEmpty(fullPathWhereToSave)) throw new ArgumentNullException("fullPathWhereToSave");

			this.DownloadUrl = url;
			this.SaveFileName = fullPathWhereToSave;
			this.DeleteExisting = true;
			this.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
		}

		#endregion

		#region Metodos Publicos

		/// <summary>Establece los Valores del Proxy</summary>
		/// <param name="pProxyServer"></param>
		/// <param name="Port"></param>
		/// <param name="UserName"></param>
		/// <param name="UserPassword"></param>
		public void SetProxy(string pProxyServer, int Port, string UserName = "", string UserPassword = "")
		{
			try
			{
				this.ProxyServer = pProxyServer;
				this.ProxyPort = Port;
				this.ProxyUser = UserName;
				this.ProxyPass = UserPassword;
			}
			catch { }
		}

		/// <summary>Metodo PAra Descarga Rapida y Silenciosa.</summary>
		/// <param name="url">Ruta completa donde Descargar el Archivo</param>
		/// <param name="fullPathWhereToSave">Ruta Completa donde Guardar el Archivo</param>
		/// <param name="timeoutInMilliSec">5 mins = 300k ms</param>
		public bool DownloadFile(string url, string fullPathWhereToSave, int timeoutInMilliSec = 300000)
		{
			return new FileDownloader(url, fullPathWhereToSave).StartDownload(timeoutInMilliSec);
		}

		/// <summary>Inicia la descarga Directa, si la conexion se interrumpe se pierden los datos.</summary>
		/// <param name="timeout"></param>
		public bool StartDownload(int timeout = 3000, int ReTryTimes = 3)
		{
			bool _ret = false;
			try
			{
				//Si no existe, crea la carpeta donde se guarda la descarga
				Directory.CreateDirectory(Path.GetDirectoryName(this.SaveFileName));

				//Si el archivo existe, lo borra					
				if (this.DeleteExisting && File.Exists(this.SaveFileName)) File.Delete(this.SaveFileName);

				using (WebClient client = new WebClient())
				{
					#region Establecer las propiedades de la Conexion

					//Agrega datos aleatorios a la solicitud para evitar la Caché
					this.URL = new Uri(this.DownloadUrl + "?random=" + DateTime.Now.Ticks);

					System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
					System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

					client.DownloadProgressChanged += WebClientDownloadProgressChanged;
					client.DownloadFileCompleted += WebClientDownloadCompleted;

					client.Headers.Add("user-agent", this.UserAgent);
					client.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Cache
					client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

					if (this.UseProxy)
					{
						client.Proxy = new WebProxy(this.ProxyServer + ":" + this.ProxyPort.ToString());
						if (this.ProxyUser.Length > 0) //<- El usuario es opcional
							client.Proxy.Credentials = new NetworkCredential(this.ProxyUser, this.ProxyPass);
					}

					#endregion

					#region Envia una Solicitud Probando la Conexion y Obteniendo Datos Basicos de la descarga

					this._request = WebRequest.Create(this.DownloadUrl) as HttpWebRequest;
					this._request.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Caché
					this._request.UserAgent = this.UserAgent;
					this._request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
					if (this.UseProxy)
					{
						this._request.Proxy = new WebProxy(this.ProxyServer + ":" + this.ProxyPort.ToString());
						if (this.ProxyUser.Length > 0)
							this._request.Proxy.Credentials = new NetworkCredential(this.ProxyUser, this.ProxyPass);
					}
					using (WebResponse _response = this._request.GetResponse())
					{
						//Determina si el Servidor Acepta Rangos de descarga, para poder descargar en partes simultaneamente.
						this.AcceptRanges = String.Compare(_response.Headers["Accept-Ranges"], "bytes", true) == 0;

						this.TotalBytesToReceive = Convert.ToInt64(_response.Headers["Content-Length"]);
					};
					#endregion

					//-----------------------------------------------------------------------------------;					
					this.StartTime = DateTime.Now;
					int count = 1;
					//Si ocurre un error, se Reintenta x veces:
					Retry.On<Exception>().For((uint)ReTryTimes).With(context =>
					{
						try
						{
							if (this.AcceptRanges)
							{
								//segment size = min((file size / number of segments), minimum allowed segment size )
								long segment_size = (this.TotalBytesToReceive / 3);
							}

							//Aqui se hace la Descarga:
							client.DownloadFileAsync(this.URL, this.SaveFileName);
						}
						catch (Exception ex)
						{
							OnDownload_Error(new Exception(string.Format("Download inturrpted retry({0}).. in 3 seconds", count++), ex), null);
							System.Threading.Thread.Sleep(timeout);
							throw;
						}
					});
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				this._semaphore.Dispose();
			}
			return _ret;
		}

		private List<Range> GetRange(int Chunks = 3, int MinSize = 1024000)
		{
			List<Range> _ret = null;
			try
			{
				//segment size = min((file size / number of segments), minimum allowed segment size )
				long size = 0;
				long segment_size = 0;

				if (this.TotalBytesToReceive > MinSize)
				{
					Chunks = 1;
					segment_size = this.TotalBytesToReceive;
				}
				else
				{
					segment_size = Math.Min((this.TotalBytesToReceive / Chunks), MinSize);
				}

				_ret = new List<Range>();
				for (int i = 0; i < Chunks; i++)
				{
					Range _R = new Range
					{
						Start = size,
						End = segment_size
					};
					_ret.Add(_R);
					size += segment_size + 1;
				}
			}
			catch { }
			return _ret;
		}
		private void GetFile(int _Start, int _End)
		{
			try
			{
				HttpWebRequest _request = WebRequest.Create(this.URL) as HttpWebRequest;
				_request.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Caché
				_request.UserAgent = this.UserAgent;
				_request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
				_request.AddRange(_Start, _End);

				WebResponse _response = _request.GetResponse();
				//Stream ns = _response.GetResponseStream();
				int RequestContentLength = 0;
				try
				{
					using (Stream responseStream = _response.GetResponseStream())
					{
						using (FileStream localFileStream = new FileStream(this.SaveFileName, FileMode.Append))
						{
							var buffer = new byte[4096];
							int bytesRead;

							while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
							{
								//totalBytesRead += bytesRead;
								RequestContentLength += bytesRead;
								localFileStream.Write(buffer, 0, bytesRead);
							}

							Console.WriteLine("Got bytes: {0}", RequestContentLength);
						}

					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Got bytes: {0}", RequestContentLength);
				}
			}
			catch { throw; }
		}

		/// <summary>Inicia la descarga, si la conexion se interrumpe se re-intenta x veces.</summary>
		/// <param name="ReTryTimes">Numero maximo de veces que se re-intenta la descarga tras un error.</param>
		public bool StartDownload_WithResume(int ReTryTimes = 3)
		{
			//Si no existe, crea la carpeta donde se guarda la descarga
			Directory.CreateDirectory(Path.GetDirectoryName(this.SaveFileName));

			//Si el archivo existe, lo borra					
			if (this.DeleteExisting && File.Exists(this.SaveFileName)) File.Delete(this.SaveFileName);

			var url = new Uri(this.DownloadUrl + "?random=" + DateTime.Now.Ticks); //<- para evitar la Caché

			using (WebClient client = new WebClient())
			{
				System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
				System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

				int count = 1;

				Retry.On<Exception>().For((uint)ReTryTimes).With(context =>
				{
					try
					{
						client.DownloadProgressChanged += WebClientDownloadProgressChanged;
						client.DownloadFileCompleted += WebClientDownloadCompleted;
						client.Headers.Add("user-agent", this.UserAgent);
						client.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Caché
						client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

						client.OpenRead(url);

						FileInfo finfo = null;
						if (this.DeleteExisting && File.Exists(this.SaveFileName))
						{
							finfo = new FileInfo(this.SaveFileName);

							if (client.ResponseHeaders != null &&
								finfo.Length >= Convert.ToInt64(client.ResponseHeaders["Content-Length"]))
							{
								File.Delete(this.SaveFileName);
							}
						}

						DownloadTheFile();
					}
					catch (Exception ex)
					{
						OnDownload_Error(new Exception(string.Format("Download inturrpted retry({0}).. in 5 seconds", count++), ex), null);
						System.Threading.Thread.Sleep(5000);
						throw;
					}
				});
			}
			return false;
		}

		#endregion

		#region Metodos Privados

		private void DownloadTheFile()
		{
			try
			{
				this._request = WebRequest.Create(this.DownloadUrl) as HttpWebRequest;
				this._request.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Caché
				this._request.Method = WebRequestMethods.Http.Get;
				this._request.UserAgent = this.UserAgent;
				this._request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

				WebResponse _response = this._request.GetResponse();
				bool acceptRanges = String.Compare(_response.Headers["Accept-Ranges"], "bytes", true) == 0;

				this._responseAsyncResult = this._request.BeginGetResponse(ResponseCallback, null);
			}
			catch { throw; }
		}
		private void ResponseCallback(object state)
		{
			try
			{
				var response = this._request.EndGetResponse(this._responseAsyncResult) as HttpWebResponse;
				long contentLength = response.ContentLength;
				if (contentLength == -1)
				{
					throw new Exception("ERROR 404 - NOT FOUND");
				}
				Stream responseStream = response.GetResponseStream();
				this.DataDownloaded = GetContentWithProgressReporting(responseStream, contentLength);
				response.Close();

				if (this.DataDownloaded != null)
				{
					//Aqui se guardan los datos descargados:
					long existLen = 0;
					System.IO.FileStream saveFileStream;

					//Si el archivo existe, se le agregan datos arriba:
					if (System.IO.File.Exists(this.SaveFileName))
					{
						System.IO.FileInfo fINfo = new System.IO.FileInfo(this.SaveFileName);
						existLen = fINfo.Length;
					}
					if (existLen > 0)
						saveFileStream = new System.IO.FileStream(this.SaveFileName,
																  System.IO.FileMode.Append, System.IO.FileAccess.Write,
																  System.IO.FileShare.ReadWrite);
					else
						saveFileStream = new System.IO.FileStream(this.SaveFileName,
																  System.IO.FileMode.Create, System.IO.FileAccess.Write,
																  System.IO.FileShare.ReadWrite);
					saveFileStream.Write(this.DataDownloaded, 0, this.DataDownloaded.Length);
					saveFileStream.Close();

					OnDownload_Complete(new long[] { 100, this.DataDownloaded.Length, contentLength }, null);  //<- Descarga Terminada
					try
					{
						this._semaphore.Release();
					}
					catch { }
				}
			}
			catch { throw; }
		}
		private byte[] GetContentWithProgressReporting(Stream responseStream, long contentLength)
		{
			try
			{
				UpdateProgressBar(0, 0, contentLength);

				// Allocate space for the content
				var data = new byte[contentLength];
				int currentIndex = 0;
				int bytesReceived = 0;
				var buffer = new byte[256];
				do
				{
					bytesReceived = responseStream.Read(buffer, 0, 256);
					Array.Copy(buffer, 0, data, currentIndex, bytesReceived);
					currentIndex += bytesReceived;

					// Report percentage
					double percentage = (double)currentIndex / contentLength;
					UpdateProgressBar((int)(percentage * 100), currentIndex, contentLength);
				} while (currentIndex < contentLength);

				UpdateProgressBar(100, bytesReceived, contentLength);
				return data;
			}
			catch { throw; }
		}
		private void UpdateProgressBar(int percentage, int bytesReceived, long contentLength)
		{
			OnDownload_Progress(new long[] { percentage, bytesReceived, contentLength }, null);
		}

		private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			if (OnDownload_Progress != null)
			{
				this.BytesReceived = e.BytesReceived;
				TimeSpan elapsedTime = DateTime.Now - this.StartTime;

				//To Calculate Speed in Kb Divide Value Of data by 1024 And Then by End Time Subtract Start Time To Know Download Per Second.
				double Speed = Math.Round((e.BytesReceived / 1024) / elapsedTime.TotalSeconds, 2);

				OnDownload_Progress(new double[] { e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive, Speed }, e);
			}
		}
		private void WebClientDownloadCompleted(object sender, AsyncCompletedEventArgs args)
		{
			if (OnDownload_Complete != null)
			{
				//OnDownload_Complete(sender, args); //<- Descarga Terminada
				OnDownload_Complete(new long[] { 100, this.BytesReceived, this.TotalBytesToReceive }, args);  //<- Descarga Terminada
			}

			try
			{
				this._semaphore.Release();
			}
			catch { }
		}

		#endregion
	}

	internal class Range
	{
		public long Start { get; set; }
		public long End { get; set; }
	}
	public class DownloadResult
	{
		public long Size { get; set; }
		public String FilePath { get; set; }
		public TimeSpan TimeTaken { get; set; }
		public int ParallelDownloads { get; set; }
	}

	public class Downloader
	{
		public string UserAgent { get; set; }  //<- ID del gestor de Descarga, x defecto es Internet Explorer 11 sobre Windows 10 x64
		public bool AcceptRanges { get; set; }
		public long TotalBytesToReceive { get; set; }
		public long BytesReceived { get; set; }

		public Downloader()
		{
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.DefaultConnectionLimit = 100;
			ServicePointManager.MaxServicePointIdleTime = 1000;
			this.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
		}
		public DownloadResult Download(String fileUrl, String destinationFolderPath, int numberOfParallelDownloads = 0, bool validateSSL = false)
		{
			if (!validateSSL)
			{
				ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			}

			Uri uri = new Uri(fileUrl);
			System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
			System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			//Calculate destination path  
			String destinationFilePath = Path.Combine(destinationFolderPath, uri.Segments.Last());

			DownloadResult result = new DownloadResult() { FilePath = destinationFilePath };

			//Handle number of parallel downloads  
			if (numberOfParallelDownloads <= 0)
			{
				numberOfParallelDownloads = Environment.ProcessorCount;
			}

			#region Get file size  

			HttpWebRequest webRequest = HttpWebRequest.Create(fileUrl) as HttpWebRequest;
			webRequest.Method = "HEAD";
			webRequest.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Caché
			webRequest.UserAgent = this.UserAgent;
			webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

			long responseLength;
			using (WebResponse webResponse = webRequest.GetResponse())
			{
				this.AcceptRanges = String.Compare(webResponse.Headers["Accept-Ranges"], "bytes", true) == 0;
				this.TotalBytesToReceive = Convert.ToInt64(webResponse.Headers["Content-Length"]);

				responseLength = long.Parse(webResponse.Headers.Get("Content-Length"));
				result.Size = responseLength;
			}
			#endregion

			if (File.Exists(destinationFilePath))
			{
				File.Delete(destinationFilePath);
			}

			using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Append))
			{
				ConcurrentDictionary<int, String> tempFilesDictionary = new ConcurrentDictionary<int, String>();

				#region Calculate ranges  

				List<Range> readRanges = new List<Range>();
				for (int chunk = 0; chunk < numberOfParallelDownloads - 1; chunk++)
				{
					var range = new Range()
					{
						Start = chunk * (responseLength / numberOfParallelDownloads),
						End = ((chunk + 1) * (responseLength / numberOfParallelDownloads)) - 1
					};
					readRanges.Add(range);
				}

				readRanges.Add(new Range()
				{
					Start = readRanges.Any() ? readRanges.Last().End + 1 : 0,
					End = responseLength - 1
				});

				#endregion

				DateTime startTime = DateTime.Now;

				#region Parallel download  

				int index = 0;
				Parallel.ForEach(readRanges, new ParallelOptions() { MaxDegreeOfParallelism = numberOfParallelDownloads }, readRange =>
				{
					HttpWebRequest _Request = HttpWebRequest.Create(fileUrl) as HttpWebRequest;
					_Request.Method = WebRequestMethods.Http.Get;
					_Request.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Caché
					_Request.UserAgent = this.UserAgent;
					_Request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
					_Request.AddRange(readRange.Start, readRange.End);
					
					using (HttpWebResponse _response = _Request.GetResponse() as HttpWebResponse)
					{
						String tempFilePath = Path.GetTempFileName();
						using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
						{
							_response.GetResponseStream().CopyTo(fileStream);
							tempFilesDictionary.TryAdd((int)index, tempFilePath);
						}
					}
					index++;
				});

				result.ParallelDownloads = index;

				#endregion

				result.TimeTaken = DateTime.Now.Subtract(startTime);

				#region Merge to single file  

				foreach (var tempFile in tempFilesDictionary.OrderBy(b => b.Key))
				{
					byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
					destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
					File.Delete(tempFile.Value);
				}
				#endregion

				return result;
			}
		}

		private void ReadStreamFromResponse(WebResponse response, ref ConcurrentDictionary<int, String> tempFilesDictionary, ref int index)
		{
			String tempFilePath = Path.GetTempFileName();
			using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				response.GetResponseStream().CopyTo(fileStream);
				tempFilesDictionary.TryAdd((int)index, tempFilePath);
			}

		}
	}

    [Serializable]
    public class game_instance
    {
        public game_instance() { }

        public string instance { get; set; } = "Default";
        public string game_id { get; set; }

        public string key { get; set; }
        public string name { get; set; }
        public string path { get; set; }

        public string themes_folder { get; set; }
        public bool is_active { get; set; } = false;

        public override string ToString()
        {
            return String.Format("{0} ({1})", name, instance);
        }
    }

    [Serializable]
    public class GameInstance
    {
        public GameInstance() { }

        public string instance { get; set; }
        public List<game_instance> games { get; set; }
    }
}
