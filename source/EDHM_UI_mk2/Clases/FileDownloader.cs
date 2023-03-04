using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using EDHM_UI_Patcher;

namespace EDHM_UI_mk2.Clases
{
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
		/// long[] _Data = sender as long[];  [0]ProgressPercentage, [1]BytesReceived, [2]BytesTotal
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

		//private List<Range> GetRange(int Chunks = 3, int MinSize = 1024000)
		//{
		//	List<Range> _ret = null;
		//	try
		//	{
		//		//segment size = min((file size / number of segments), minimum allowed segment size )
		//		long size = 0;
		//		long segment_size = 0;

		//		if (this.TotalBytesToReceive > MinSize)
		//		{
		//			Chunks = 1;
		//			segment_size = this.TotalBytesToReceive;
		//		}
		//		else
		//		{
		//			segment_size = Math.Min((this.TotalBytesToReceive / Chunks), MinSize);
		//		}

		//		_ret = new List<Range>();
		//		for (int i = 0; i < Chunks; i++)
		//		{
		//			Range _R = new Range
		//			{
		//				Start = size,
		//				End = segment_size
		//			};
		//			_ret.Add(_R);
		//			size += segment_size + 1;
		//		}
		//	}
		//	catch { }
		//	return _ret;
		//}
		//private void GetFile(int _Start, int _End)
		//{
		//	try
		//	{
		//		HttpWebRequest _request = WebRequest.Create(this.URL) as HttpWebRequest;
		//		_request.Headers.Add("Cache-Control", "no-cache"); //<- para evitar la Caché
		//		_request.UserAgent = this.UserAgent;
		//		_request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
		//		_request.AddRange(_Start, _End);

		//		WebResponse _response = _request.GetResponse();
		//		//Stream ns = _response.GetResponseStream();
		//		int RequestContentLength = 0;
		//		try
		//		{
		//			using (Stream responseStream = _response.GetResponseStream())
		//			{
		//				using (FileStream localFileStream = new FileStream(this.SaveFileName, FileMode.Append))
		//				{
		//					var buffer = new byte[4096];
		//					int bytesRead;

		//					while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
		//					{
		//						//totalBytesRead += bytesRead;
		//						RequestContentLength += bytesRead;
		//						localFileStream.Write(buffer, 0, bytesRead);
		//					}

		//					Console.WriteLine("Got bytes: {0}", RequestContentLength);
		//				}

		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine("Got bytes: {0}", RequestContentLength);
		//		}
		//	}
		//	catch { throw; }
		//}

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
		public string notification { get; set; }

		public bool re_install { get; set; }
		public bool run_hotfix { get; set; }
	}
}