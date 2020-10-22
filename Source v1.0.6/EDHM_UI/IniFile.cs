using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


namespace EDHM_UI
{
	public class IniFile
	{
		private string Path = string.Empty;
		private string EXE = Assembly.GetExecutingAssembly().GetName().Name;

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

		public IniFile(string IniPath = null)
		{
			this.Path = new FileInfo(IniPath ?? this.EXE + ".ini").FullName;
		}

		public string ReadKey(string Key, string Section = null)
		{
			StringBuilder RetVal = new StringBuilder(255);
			int i = GetPrivateProfileString(Section, Key, "", RetVal, 255, this.Path);
			return RetVal.ToString();
		}

		public void WriteKey(string Key, string Value, string Section = null)
		{
			WritePrivateProfileString(Section ?? this.EXE, Key, Value, this.Path);
		}

		public void DeleteKey(string Key, string Section = null)
		{
			WriteKey(Key, null, Section ?? this.EXE);
		}

		public void DeleteSection(string Section = null)
		{
			WriteKey(null, null, Section ?? this.EXE);
		}

		public bool KeyExists(string Key, string Section = null)
		{
			return ReadKey(Key, Section).Length > 0;
		}
	}

	public static class Util
	{
		/// <summary>Constantes para los Codigos de Pagina al leer o guardar archivos de texto.</summary>
		public enum TextEncoding
		{
			/// <summary>CodePage:1252; windows-1252 ANSI Latin 1; Western European (Windows)</summary>
			ANSI = 1252,
			/// <summary>CodePage:850; ibm850; ASCII Multilingual Latin 1; Western European (DOS)</summary>
			DOS_850 = 850,
			/// <summary>CodePage:1200; utf-16; Unicode UTF-16, little endian byte order (BMP of ISO 10646);</summary>
			Unicode = 1200,
			/// <summary>CodePage:65001; utf-8; Unicode (UTF-8)</summary>
			UTF8 = 65001
		}

		/// <summary>Devuelve 'true' si la Cadena es Nula o Vacia.</summary>
		/// <param name="pValor">Cadena de Texto</param>
		public static bool EmptyOrNull(this string pValor)
		{
			if (pValor != null && pValor != string.Empty)
			{
				return false;
			}
			else
			{
				return true;
			}
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


		/// <summary>Lee un Archivo de Texto usando la Codificacion especificada.</summary>
		/// <param name="FilePath">Ruta de acceso al Archivo. Si no existe se produce un Error.</param>
		/// <param name="CodePage">[Opcional] Pagina de Codigos con la que se Leerá el archivo. Por defecto se usa Unicode(UTF-16).</param>
		public static string ReadTextFile(string FilePath, TextEncoding CodePage = TextEncoding.Unicode)
		{
			string _ret = string.Empty;
			try
			{
				if (FilePath != null && FilePath != string.Empty)
				{
					if (System.IO.File.Exists(FilePath))
					{
						System.Text.Encoding ENCODING = System.Text.Encoding.GetEncoding((int)CodePage);
						_ret = System.IO.File.ReadAllText(FilePath, ENCODING);
					}
					else { throw new Exception(string.Format("ERROR 404: Archivo '{0}' NO Encontrado!", FilePath)); }
				}
				else { throw new Exception("No se ha Especificado la Ruta de acceso al Archivo!"); }
			}
			catch (Exception ex) { throw ex; }
			return _ret;
		}

		/// <summary>Guarda Datos en un Archivo de Texto usando la Codificacion especificada.</summary>
		/// <param name="FilePath">Ruta de acceso al Archivo. Si no existe, se Crea. Si existe, se Sobreescribe.</param>
		/// <param name="Data">Datos a Grabar en el Archivo.</param>
		/// <param name="CodePage">[Opcional] Pagina de Codigos con la que se guarda el archivo. Por defecto se usa Unicode(UTF-16).</param>
		public static bool SaveTextFile(string FilePath, string Data, TextEncoding CodePage = TextEncoding.Unicode)
		{
			bool _ret = false;
			try
			{
				if (FilePath != null && FilePath != string.Empty)
				{
					/* ANSI code pages, like windows-1252, can be different on different computers, 
					 * or can be changed for a single computer, leading to data corruption. 
					 * For the most consistent results, applications should use UNICODE, 
					 * such as UTF-8 or UTF-16, instead of a specific code page. 
					 https://docs.microsoft.com/es-es/windows/desktop/Intl/code-page-identifiers  */

					System.Text.Encoding ENCODING = System.Text.Encoding.GetEncoding((int)CodePage); //<- Unicode Garantiza Maxima compatibilidad
					using (System.IO.FileStream FILE = new System.IO.FileStream(FilePath, System.IO.FileMode.Create))
					{
						using (System.IO.StreamWriter WRITER = new System.IO.StreamWriter(FILE, ENCODING))
						{
							WRITER.Write(Data);
							WRITER.Close();
						}
					}
					if (System.IO.File.Exists(FilePath)) _ret = true;
				}
			}
			catch (Exception ex) { throw ex; }
			return _ret;
		}


		/// <summary>Lee una Clave del Registro de Windows para el Usuario Actual.
		/// Las Claves en este caso siempre se Leen desde 'HKEY_CURRENT_USER\Software\Cutcsa\DXCutcsa'.</summary>
		/// <param name="Sistema">Nombre del Sistema que guarda las Claves, ejem: RRHH, Contaduria, CutcsaPagos, etc.</param>
		/// <param name="KeyName">Nombre de la Clave a Leer</param>
		/// <returns>Devuelve NULL si la clave no existe</returns>
		public static object WinReg_ReadKey(string Sistema, string KeyName)
		{
			Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;
			Microsoft.Win32.RegistryKey sk1 = rk.OpenSubKey(@"Software\Cutcsa\DXCutcsa\" + Sistema);

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
				Microsoft.Win32.RegistryKey sk1 = rk.CreateSubKey(@"Software\Cutcsa\DXCutcsa\" + Sistema);
				sk1.SetValue(KeyName, Value);

				return true; //<-La Clave se Guardo Exitosamente!
			}
			catch { return false; }
		}

		public static void SearchSteam()
		{
			//steamGameDirs.Clear();
			//TODO: Open the correct File Location, or remember last opened folder?
			//D:\Juegos\SteamLibrary\steamapps\common\Elite Dangerous\Products\elite-dangerous-64
			//32 - bit: HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam
			//64 - bit: HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam
			//\Steam\steamapps\common\<game_folder>\

			string steam32 = "SOFTWARE\\VALVE\\";
			string steam64 = "SOFTWARE\\Wow6432Node\\Valve\\";
			string steam32path;
			string steam64path;
			string config32path;
			string config64path;
			Microsoft.Win32.RegistryKey key32 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(steam32);
			Microsoft.Win32.RegistryKey key64 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(steam64);
			if (key64.ToString() == null || key64.ToString() == "")
			{
				foreach (string k32subKey in key32.GetSubKeyNames())
				{
					using (Microsoft.Win32.RegistryKey subKey = key32.OpenSubKey(k32subKey))
					{
						steam32path = subKey.GetValue("InstallPath").ToString();
						config32path = steam32path + "/steamapps/libraryfolders.vdf";
						string driveRegex = @"[A-Z]:\\";
						if (File.Exists(config32path))
						{
							string[] configLines = File.ReadAllLines(config32path);
							foreach (var item in configLines)
							{
								Console.WriteLine("32:  " + item);
								System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(item, driveRegex);
								if (item != string.Empty && match.Success)
								{
									string matched = match.ToString();
									string item2 = item.Substring(item.IndexOf(matched));
									item2 = item2.Replace("\\\\", "\\");
									item2 = item2.Replace("\"", "\\steamapps\\common\\");
									//steamGameDirs.Add(item2);
								}
							}
							//steamGameDirs.Add(steam32path + "\\steamapps\\common\\");
						}
					}
				}
			}
			foreach (string k64subKey in key64.GetSubKeyNames())
			{
				using (Microsoft.Win32.RegistryKey subKey = key64.OpenSubKey(k64subKey))
				{
					steam64path = subKey.GetValue("InstallPath").ToString();
					config64path = steam64path + "/steamapps/libraryfolders.vdf";
					string driveRegex = @"[A-Z]:\\";
					if (File.Exists(config64path))
					{
						string[] configLines = File.ReadAllLines(config64path);
						foreach (var item in configLines)
						{
							Console.WriteLine("64:  " + item);
							System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(item, driveRegex);
							if (item != string.Empty && match.Success)
							{
								string matched = match.ToString();
								string item2 = item.Substring(item.IndexOf(matched));
								item2 = item2.Replace("\\\\", "\\");
								item2 = item2.Replace("\"", "\\steamapps\\common\\");
								//steamGameDirs.Add(item2);
							}
						}
						//steamGameDirs.Add(steam64path + "\\steamapps\\common\\");
					}
				}
			}
		}

		public static String HexConverter(System.Drawing.Color c)
		{
			return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}

		public static String RGBConverter(System.Drawing.Color c)
		{
			return "RGB(" + c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString() + ")";
		}

		public static String ARGBConverter(System.Drawing.Color c)
		{
			return string.Format("ARGB({0},{1},{2},{3})",
				c.A.ToString(),
				c.R.ToString(),
				c.G.ToString(),
				c.B.ToString() 
			);
		}

		public static String Color_To_RGBA_Normalized(System.Drawing.Color _Color, decimal MinValue = 0, decimal MaxValue = 1)
		{
			string _Ret = string.Empty;

			/* Fomula for Normalize Ranges:
			 * newvalue = (max2 - min2)/ (max1 - min1) * (value - max1) + max2 */

			decimal _R = Convert.ToDecimal(_Color.R.ToString());
			decimal _RedDec = (MaxValue - MinValue) / (255 - 0) * (_R - 255) + MaxValue;

			decimal _G = Convert.ToDecimal(_Color.G.ToString());
			decimal _GreenDec = (MaxValue - MinValue) / (255 - 0) * (_G - 255) + MaxValue;

			decimal _B = Convert.ToDecimal(_Color.B.ToString());
			decimal _BlueDec = (MaxValue - MinValue) / (255 - 0) * (_B - 255) + MaxValue;

			decimal _A = Convert.ToDecimal(_Color.A.ToString());
			decimal _AlfaDec = (MaxValue - MinValue) / (255 - 0) * (_A - 255) + MaxValue;

			_Ret = string.Format("({0}, {1}, {2}, {3})", 
				Math.Round(_RedDec, 6).ToString().Replace(',','.').PadRight(8, '0'),
				Math.Round(_GreenDec, 6).ToString().Replace(',', '.').PadRight(8, '0'),
				Math.Round(_BlueDec, 6).ToString().Replace(',', '.').PadRight(8, '0'),
				Math.Round(_AlfaDec, 6).ToString().Replace(',', '.').PadRight(8, '0')
			);

			return _Ret;
		}

		public static String Number_To_RGBA_Normalized(  decimal _Value, 
			decimal A_MinValue = 0, decimal A_MaxValue = 1,
			decimal B_MinValue = 0, decimal B_MaxValue = 1,
			int DecimalPlaces = 6)
		{
			string _Ret = string.Empty;
			decimal _RedDec = NormalizeNumber(A_MinValue, A_MaxValue, B_MinValue, B_MaxValue);

			_Ret = string.Format("{0}", Math.Round(_RedDec, DecimalPlaces).ToString().Replace(',', '.').PadRight(DecimalPlaces + 2, '0') );
			return _Ret;
		}

		public static decimal NormalizeNumber(  decimal _Value,
			decimal A_MinValue = 0, decimal A_MaxValue = 1,
			decimal B_MinValue = 0, decimal B_MaxValue = 10)
		{
			decimal _ret = 0;
			if (B_MaxValue > B_MinValue && A_MaxValue > A_MinValue)
			{
				_ret = (B_MaxValue - B_MinValue) / (A_MaxValue - A_MinValue) * (_Value - A_MaxValue) + B_MaxValue;
			}
			if (_ret < B_MinValue) _ret = B_MinValue;
			if (_ret > B_MaxValue) _ret = B_MaxValue;
			return _ret;
		}

		public static System.Windows.Forms.DialogResult ShowInputDialog(string _Title, ref string input)
		{
			System.Drawing.Size size = new System.Drawing.Size(200, 70);
			System.Windows.Forms.Form inputBox = new System.Windows.Forms.Form();

			inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			inputBox.ClientSize = size;
			inputBox.Text = _Title;

			System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox()
			{
				Size = new System.Drawing.Size(size.Width - 10, 23),
				Location = new System.Drawing.Point(5, 5),
				Text = input,
				TabIndex = 0
			};
			inputBox.Controls.Add(textBox);

			System.Windows.Forms.Button okButton = new System.Windows.Forms.Button()
			{				
				Name = "okButton",
				Text = "&OK",
				Size = new System.Drawing.Size(75, 23),
				Location = new System.Drawing.Point(size.Width - 80 - 80, 39),
				DialogResult = System.Windows.Forms.DialogResult.OK,
				TabIndex = 1
			};
			inputBox.Controls.Add(okButton);

			System.Windows.Forms.Button cancelButton = new System.Windows.Forms.Button()
			{
				Name = "cancelButton",
				Text = "&Cancel",
				DialogResult = System.Windows.Forms.DialogResult.Cancel,
				Size = new System.Drawing.Size(75, 23),
				Location = new System.Drawing.Point(size.Width - 80, 39),
				TabIndex = 2
			};
			inputBox.Controls.Add(cancelButton);

			inputBox.AcceptButton = okButton;
			inputBox.CancelButton = cancelButton;
			inputBox.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

			System.Windows.Forms.DialogResult result = inputBox.ShowDialog();
			input = textBox.Text;
			return result;
		}
	}

	/// <summary>This is what is saved in the JSON file.</summary>
	[Serializable]
	public class json_settings
	{
		public json_settings() { }

		public string current_preset { get; set; }

		public List<preset> Presets { get; set; }
	}

	/// <summary>Stores the Data of Each Preset.</summary>
	[Serializable]
	public class preset
	{
		public preset() { }

		public string name { get; set; }
		public List<shader> Shaders { get; set; }
	}

	/// <summary>Stores the Data of each Shader.</summary>
	[Serializable]
	public class shader
	{
		public shader()
		{
			min = 0;
			max = 1;
			value = 0;
			kind = 0;
			file = string.Empty;
			code = string.Empty;
			line = 1;
		}

		public string name { get; set; }
		public string file { get; set; }
		public string code { get; set; }
		public int line { get; set; }

		/// <summary>0=Single Value (Decimal), 1=Color Value, 2=Triple Value(Linked), 3=Quadruple Value</summary>
		public int kind { get; set; }

		public decimal value { get; set; }

		public decimal max { get; set; }
		public decimal min { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public bool is_selected { get; set; }
	}

	
}
