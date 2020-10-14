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
	}

	[Serializable]
	public class json_Lists
	{
		/// <summary>This Class Save/Retrieve the Lists shown in the Combo Controls.
		/// Its a Parser for the JSON file </summary>
		public json_Lists() { }

		public List<preset> Presets { get; set; }

		public List<string> AmbientCabinLights { get; set; }
		public List<string> TargetingReticle { get; set; }
		public List<string> ShieldColour { get; set; }
		public List<string> OwnShipHologram { get; set; }
		public List<string> Distributor { get; set; }
	}

	[Serializable]
	public class preset
	{
		public preset() { }

		public string name { get; set; }

		public int w1 { get; set; }
		public decimal w2 { get; set; }
		public int w3 { get; set; }
		public int w4 { get; set; }
		public int w5 { get; set; }
		public int w6 { get; set; }
		public int w7 { get; set; }
		public int w8 { get; set; }
		public int w9 { get; set; }
		public int w10 { get; set; }
		public int w11 { get; set; }
		public int w12 { get; set; }
	}
}
