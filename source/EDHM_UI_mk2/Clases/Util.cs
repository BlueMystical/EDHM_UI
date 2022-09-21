using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace EDHM_UI_mk2
{
	public static class Util
	{
		[DllImport("msi.dll", CharSet = CharSet.Unicode)]
		private static extern Int32 MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf, ref Int32 len);

		[DllImport("msi.dll", SetLastError = true)]
		private static extern int MsiEnumProducts(int iProductIndex, StringBuilder lpProductBuf);

		[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

		[DllImport("User32.dll")]
		static extern int SetForegroundWindow(IntPtr point);

		private enum MSIINSTALLCONTEXT
		{
			MSIINSTALLCONTEXT_NONE = 0,
			MSIINSTALLCONTEXT_USERMANAGED = 1,
			MSIINSTALLCONTEXT_USERUNMANAGED = 2,
			MSIINSTALLCONTEXT_MACHINE = 4,
			MSIINSTALLCONTEXT_ALL = (MSIINSTALLCONTEXT_USERMANAGED | MSIINSTALLCONTEXT_USERUNMANAGED | MSIINSTALLCONTEXT_MACHINE),
		}

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

		public static string MSI_InstalledProgram_GetPath(string _ProgramName)
		{
			string _ret = string.Empty;
			try
			{
				StringBuilder sbProductCode = new StringBuilder(39);
				int iIdx = 0;
				while (
					0 == MsiEnumProducts(iIdx++, sbProductCode))
				{
					Int32 productNameLen = 512;
					StringBuilder sbProductName = new StringBuilder(productNameLen);

					MsiGetProductInfo(sbProductCode.ToString(),
						"ProductName", sbProductName, ref productNameLen);

					if (sbProductName.ToString().Contains(_ProgramName))
					{
						Int32 installDirLen = 1024;
						StringBuilder sbInstallDir = new StringBuilder(installDirLen);

						MsiGetProductInfo(sbProductCode.ToString(), "InstallLocation", sbInstallDir, ref installDirLen);

						_ret = string.Format("{0}|{1}", sbProductName, sbInstallDir);
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			return _ret;
		}
		public static string InstalledProgram_GetPath(string findByName)
		{
			string _ret = string.Empty;
			try
			{
				string displayName;
				string InstallPath;
				string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

				//64 bits computer
				RegistryKey key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				RegistryKey key = key64.OpenSubKey(registryKey);

				if (key != null)
				{
					foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
					{
						displayName = subkey.GetValue("DisplayName") as string;
						if (displayName != null && displayName.Contains(findByName))
						{
							InstallPath = subkey.GetValue("InstallLocation").ToString();
							_ret = string.Format("{0}|{1}", displayName, InstallPath);
							break;
						}
					}
					key.Close();
				}
			}
			catch { }
			return _ret;
		}

		public static bool NullBool(object Value, bool Default = false)
		{
			bool _ret = Default;
			if (Value != null)
			{
				_ret = (bool)Value;
			}
			return _ret;
		}

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

		/// <summary>Permite Copiar (por Valor) una Lista de Objetos.
		/// <para>El Objeto debe Implementar la Interface 'ICloneable'.</para></summary>
		/// <typeparam name="T">Tipo del Objeto a Clonar</typeparam>
		/// <param name="listToClone">Objetos a Clonar</param>
		public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
		{
			//MODO DE USO: UI_Themes = (List<ui_preset_new>)this.UI_Themes.Clone();

			return listToClone.Select(item => (T)item.Clone()).ToList();
		}
		
		/// <summary>Devuelve 'true' si la lista de elementos NO está Vacia.</summary>
		/// <param name="elements">Lista de Elementos</param>
		public static bool IsNotEmpty(this System.Collections.ICollection elements)
		{
			return elements != null && elements.Count > 0;
		}

		public static void SplitOnce(this string value, string separator, out string part1, out string part2)
		{
			if (value != null)
			{
				int idx = value.IndexOf(separator);
				if (idx >= 0)
				{
					part1 = value.Substring(0, idx);
					part2 = value.Substring(idx + separator.Length);
				}
				else
				{
					part1 = value;
					part2 = null;
				}
			}
			else
			{
				part1 = "";
				part2 = null;
			}
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

			var _ret = default(T);
			try
			{
				if (!filePath.EmptyOrNull())
				{
					if (System.IO.File.Exists(filePath))
					{
						//Carga el JSON sin dejar el archivo 'en uso':
						using (TextReader reader = new StreamReader(filePath))
						{
							var fileContents = reader.ReadToEnd(); reader.Close();
							_ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContents);
						}
					}
				}
			}
			finally { }
			return _ret;
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




		/// <summary>Lee un Archivo de Texto usando la Codificacion especificada.</summary>
		/// <param name="FilePath">Ruta de acceso al Archivo. Si no existe se produce un Error.</param>
		/// <param name="CodePage">[Opcional] Pagina de Codigos con la que se Leerá el archivo. Por defecto se usa UTF-8.</param>
		public static string ReadTextFile(string FilePath, TextEncoding CodePage = TextEncoding.UTF8)
		{
			string _ret = string.Empty;
			try
			{
				if (FilePath != null && FilePath != string.Empty)
				{
					if (System.IO.File.Exists(FilePath))
					{
						//Abre el archivo sin Bloquearlo:
						using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						{
							using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding((int)CodePage)))
							{
								_ret = sr.ReadToEnd();
							}
						}
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
		/// <param name="CodePage">[Opcional] Pagina de Codigos con la que se guarda el archivo. Por defecto se usa UTF8.</param>
		public static bool SaveTextFile(string FilePath, string Data, TextEncoding CodePage = TextEncoding.UTF8)
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
						if (CodePage == TextEncoding.UTF8) ENCODING = new UTF8Encoding(false); //<- UTF8 sin BOM

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

		/// <summary>En una Cadena con multiples lineas, Borra la linea indicada.</summary>
		/// <param name="_Text">Cadena de Texto</param>
		/// <param name="Line">Indice basado en Cero que indica el numero de linea a Borrar</param>
		public static string RemoveLine(this String _Text, int Line)
		{
			string _ret = _Text;

			if (_Text != null && _Text != string.Empty)
			{
				int index = 0;
				StringBuilder Texto = new StringBuilder();
				string[] Lineas = System.Text.RegularExpressions.Regex.Split(_Text, "\r\n|\r|\n");
				foreach (string _line in Lineas)
				{
					if (index != Line)
					{
						Texto.AppendLine(_line);
					}
					index++;
				}
				_ret = Texto.ToString();
			}

			return _ret;
		}
		

		/// <summary>Devuelve la 'key' de la Instancia Activa.</summary>
		/// <param name="elements"></param>
		public static string ActiveInstance(this List<game_instance> elements)
		{
			string _ret = string.Empty;
			if (elements != null && elements.Count > 0)
			{
				// Devuelve el primer elemento que encuentre y detiene la busqueda:
				var ActiveOne = elements.Find(x => x.is_active == true);
				if (ActiveOne != null)
				{
					_ret = ActiveOne.key;
				}
				else
				{
					_ret = elements[0].key;
				}
			}
			return _ret;
		}


		/// <summary>Evalua si un determinado valor se encuentra entre una lista de valores.</summary>
		/// <param name="pVariable">Valor a Buscar.</param>
		/// <param name="pValores">Lista de Valores de Referencia. Ignora Mayusculas.</param>
		/// <returns>Devuelve 'True' si el valor existe en la lista al menos una vez.</returns>
		public static bool In(this String text, params string[] pValores)
		{
			bool retorno = false;
			try
			{
				foreach (string val in pValores)
				{
					if (text.Equals(val, StringComparison.InvariantCultureIgnoreCase))
					{ retorno = true; break; }
				}
			}
			catch { }
			return retorno;
		}
		/// <summary>Evalua si un determinado valor se encuentra entre una lista de valores.</summary>
		/// <param name="pVariable">Valor a Buscar.</param>
		/// <param name="pValores">Lista de Valores de Referencia.</param>
		/// <returns>Devuelve 'True' si el valor existe en la lista al menos una vez.</returns>
		public static bool In(this Int32 valor, params int[] pValores)
		{
			bool retorno = false;
			try
			{
				foreach (int val in pValores)
				{
					if (val == valor) { retorno = true; break; }
				}
			}
			catch { }
			return retorno;
		}
		public static bool In<T>(this T source, params T[] list)
		{
			if (null == source) throw new ArgumentNullException("source");
			return list.Contains(source);
		}

		/// <summary>Determina si el valor de la Variable se encuentra dentro del Rango especificado.</summary>
		/// <typeparam name="T">Tipo de Datos del  Objeto</typeparam>
		/// <param name="Valor">Valor (numerico) a comparar.</param>
		/// <param name="Desde">Rango Inicial</param>
		/// <param name="Hasta">Rango final</param>
		public static bool Between<T>(this T Valor, T Desde, T Hasta) where T : IComparable<T>
		{
			return Valor.CompareTo(Desde) >= 0 && Valor.CompareTo(Hasta) < 0;
		}


		/// <summary>Si el Valor es Nulo, devuelve el valor por defecto.</summary>
		/// <param name="pValor">Valor a Verificar.</param>
		/// <param name="pDefault">Valor por defecto.</param>
		public static string ValidarNulo(object pValor, string pDefault = "")
		{
			string _ret = pDefault;
			try
			{
				if (pValor != null && pValor.ToString() != string.Empty && !pValor.ToString().Trim().Equals(""))
					_ret = Convert.ToString(pValor);
			}
			catch { }
			return _ret;
		}
		public static int ValidarNulo(object pValor, int pDefault = int.MinValue)
		{
			int _ret = pDefault;
			try
			{
				if (pValor != null) _ret = Convert.ToInt32(pValor);
				if (_ret == int.MinValue) _ret = pDefault;
			}
			catch { }
			return _ret;
		}
		public static long ValidarNulo(object pValor, long pDefault = long.MinValue)
		{
			long _ret = pDefault;
			try
			{
				if (pValor != null) _ret = Convert.ToInt64(pValor);
				if (_ret == int.MinValue) _ret = pDefault;
			}
			catch { }
			return _ret;
		}
		public static decimal ValidarNulo(object pValor, decimal pDefault = decimal.MinValue)
		{
			decimal _ret = pDefault;
			try
			{
				if (pValor != null) _ret = Convert.ToDecimal(pValor);
				if (_ret == decimal.MinValue) _ret = pDefault;
			}
			catch { }
			return _ret;
		}
		public static DateTime ValidarNulo(object pValor, DateTime? pDefault)
		{
			DateTime _ret = pDefault ?? DateTime.MinValue;
			try
			{
				if (pValor != null) _ret = Convert.ToDateTime(pValor);
			}
			catch { }
			return _ret;
		}
		public static bool ValidarNulo(object pValor, bool pDefault = false)
		{
			bool _ret = pDefault;
			try
			{
				if (pValor != null) _ret = Convert.ToBoolean(pValor);
			}
			catch { }
			return _ret;
		}

		public static bool IntegerToBool(int pValor, bool pDefault = false)
		{
			bool _ret = pDefault;
			try
			{
				if (pValor == 1) _ret = true;
			}
			catch { }
			return _ret;
		}
		public static int BoolToInteger(bool pValor, int pDefault = 0)
		{
			int _ret = pDefault;
			try
			{
				if (pValor) _ret = 1;
			}
			catch { }
			return _ret;
		}

		/// <summary>
		/// Encodes integers into a byte array. Due to padding if 
		/// encoding integers of less than 8 bits you could reserve 0 
		/// as a special case to allow for detecting surplus results
		/// (or explicitly externally keep track of the expected length
		/// and truncate the excess 0 value integers returned by this 
		/// method).
		/// </summary>
		/// <param name="ints">integer arrays, value of each must be >=0
		///                    and below the maximum storable in an 
		///                    unsigned int of bitsPerInt bits</param>
		/// <param name="bitsPerInt"># bits to use to encode each
		///                          integer</param>
		/// <returns></returns>
		public static byte[] GetBytesFromInts(int[] ints, int bitsPerInt)
		{
			if (bitsPerInt < 1 || bitsPerInt > 31)
			{
				throw new ArgumentOutOfRangeException("bitsPerInt",
					"1..31 bit unsigned integers supported only.");
			}
			foreach (var i in ints)
			{
				if (i < 0 || i > (Math.Pow(2, bitsPerInt) - 1))
				{
					throw new ArgumentOutOfRangeException("ints",
								  "Integer is <0 or >((2^bitsPerInt)-1)");
				}
			}

			var totalBits = (bitsPerInt * ints.Length);
			var result = new byte[(totalBits / 8) + (totalBits % 8 == 0 ? 0 : 1)];

			for (var i = 0; i < ints.Length; i++)
			{
				var startBit = i * bitsPerInt;
				var startByte = (startBit / 8);
				var endBit = (startBit + bitsPerInt);
				var endByte = endBit / 8 + (endBit % 8 == 0 ? 0 : 1);
				var byteRange = endByte - startByte;

				for (var b = 0; b < byteRange; b++)
				{
					var bi = startByte + b;
					Int64 asi64 = ints[i];
					var shiftedForMerge = b == 0 ?
				(asi64 << (startBit % 8)) :
				(asi64 >> ((b * 8) - (startBit % 8)));
					result[bi] |= (byte)shiftedForMerge;
				}
			}

			return result;
		}

		/// <summary>
		/// May return more ints than the number encoded due to padding.
		/// You should reserve the value of 0 to cater for detecting this.
		/// </summary>
		/// <param name="data">byte array encoded by GetBytesFromInts</param>
		/// <param name="bitsPerInt"># used per integer when encoding with
		///                          GetBytesFromInts</param>
		/// <returns></returns>
		public static int[] GetIntsFromBits(byte[] data, int bitsPerInt)
		{
			if (bitsPerInt < 1 || bitsPerInt > 31)
			{
				throw new ArgumentOutOfRangeException("bitsPerInt",
					"1..31 bit unsigned integers supported only.");
			}

			var result = new int[(data.Length * 8) / bitsPerInt];

			for (var i = 0; i < result.Length; i++)
			{
				var startBit = i * bitsPerInt;
				var startByte = (startBit / 8);
				var endBit = (startBit + bitsPerInt);
				var endByte = (endBit - 1) / 8;
				var byteRange = endByte - startByte;

				for (var b = 0; b <= byteRange; b++)
				{
					var bi = startByte + b;
					Int64 byteAsI64 = data[bi];
					var amountToShift = (b * 8) - (startBit % 8);
					var shiftedForMerge = amountToShift < 0 ?
				byteAsI64 >> (0 - amountToShift) :
				byteAsI64 << amountToShift;
					result[i] |= (int)shiftedForMerge;
				}

				result[i] &= (1 << bitsPerInt) - 1;
			}

			return result;
		}

		/// <summary>Obtiene los Valores de las Flags enmascaradas en un nunmero Entero.</summary>
		/// <param name="maskValue">Numero desde 0 a 1073741824</param>
		/// <returns></returns>
		public static IEnumerable<int> GetFlafValues(int maskValue)
		{
			//https://elite-journal.readthedocs.io/en/latest/Status%20File/
			/*var mask = (StatusFlags)16842765;
			var result =
				Enum.GetValues(typeof(StatusFlags))
					.Cast<StatusFlags>()
					.Where(value => mask.HasFlag(value))
					.ToList();*/

			int max = 1073741824;

			for (int i = max; i > 0; i /= 2)
			{
				int x = i & maskValue;
				if (x > 0)
				{
					yield return x;
				}
			}
		}

		/// <summary>Devuelve uno de dos objetos, dependiendo de la evaluación de una expresión.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression">Expresión que se desea evaluar.</param>
		/// <param name="truePart">Se devuelve si Expression se evalúa como True.</param>
		/// <param name="falsePart">Se devuelve si Expression se evalúa como False.</param>
		public static object IIf(bool expression, object truePart, object falsePart)
		{ return expression ? truePart : falsePart; }
		public static T IIf<T>(bool expression, T truePart, T falsePart)
		{ return expression ? truePart : falsePart; }



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
		
		#region Random Numbers

		/// <summary>Generador de numeros aleatorios.
		/// Al ser declarado a nivel de clase, mejora la calidad de los numeros aleatorios generados.</summary>
		private static Random RND = new Random();

		/// <summary>Obtiene un Número aleatorio.
		/// Si el Numero Base es Diferente a Cero se usará como Semilla.</summary>
		/// <param name="Numero">(Seed) Un número usado para calcular un valor inicial para la secuencia numérica pseudoaleatoria. Si se especifica un número negativo, se usa el valor absoluto del número.</param>
		public static int RandomNumber(this Int32 Numero)
		{
			int _ret = 0;
			try
			{
				if (Numero != 0)
				{
					_ret = new Random(Numero).Next();
				}
				else
				{
					_ret = RND.Next();
				}
			}
			catch { }
			return _ret;
		}

		/// <summary>Obtiene un Número aleatorio entre 0.0 y 1.0
		/// Si el Numero Base es Diferente a Cero se usará como Semilla.
		/// Ejem: double DD = new double().RandomNumber();</summary>
		/// <param name="Numero">(Seed) Un número usado para calcular un valor inicial para la secuencia numérica pseudoaleatoria. Si se especifica un número negativo, se usa el valor absoluto del número.</param>
		public static double RandomNumber(this Double Numero)
		{
			double _ret = 0;
			try
			{
				if (Numero > 0)
				{
					_ret = new Random(Convert.ToInt32(Numero)).NextDouble();
				}
				else
				{
					_ret = RND.NextDouble();
				}
			}
			catch { }
			return _ret;
		}

		/// <summary>Obtiene un Número aleatorio entre el Rango especificado.
		/// Si el Numero Base es Diferente a Cero se usará como Semilla.
		/// Ejem: int G = new Int32().RandomBetween(0, 10);</summary>
		/// <param name="Numero">(Seed) Un número usado para calcular un valor inicial para la secuencia numérica pseudoaleatoria. Si se especifica un número negativo, se usa el valor absoluto del número.</param>
		/// <param name="MinValue">Valor Minimo del Rango</param>
		/// <param name="MaxValue">Valor Maximo del Rango</param>
		public static int RandomBetween(this Int32 Numero, int MinValue, int MaxValue)
		{
			int _ret = 0;
			try
			{
				if (Numero != 0)
				{
					_ret = new Random(Numero).Next(MinValue, MaxValue + 1);
				}
				else
				{
					_ret = RND.Next(MinValue, MaxValue + 1); //<- MaxValue No es Inclusivo
				}
			}
			catch { }
			return _ret;
		}

		/// <summary>Obtiene un Número aleatorio entre el Rango especificado.
		/// Si el Numero Base es Diferente a Cero se usará como Semilla.
		/// Ejem: double D = new double().RandomBetween(1, 10);</summary>
		/// <param name="Numero">(Seed) Un número usado para calcular un valor inicial para la secuencia numérica pseudoaleatoria. Si se especifica un número negativo, se usa el valor absoluto del número.</param>
		/// <param name="MinValue">Valor Minimo del Rango</param>
		/// <param name="MaxValue">Valor Maximo del Rango</param>
		public static double RandomBetween(this Double Numero, int MinValue, int MaxValue)
		{
			double _ret = 0;
			try
			{
				if (Numero != 0)
				{
					_ret = new Random(Convert.ToInt32(Numero)).NextDouble() * MaxValue;
				}
				else
				{
					_ret = RND.NextDouble() * MaxValue; //<- MaxValue No es Inclusivo
				}
			}
			catch { }
			return _ret;
		}

		/// <summary>Reordena al Azar la lista de elementos.</summary>
		/// <typeparam name="T">Cualquier tipo de Objeto.</typeparam>
		/// <param name="list">Lista de elementos a reordenar.</param>
		public static void RandomizeOrder<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = RND.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		#endregion

		public static System.Windows.Forms.DialogResult ShowInputDialog(string _Title, ref string input)
		{
			System.Drawing.Size size = new System.Drawing.Size(200, 70);

			System.Windows.Forms.Form inputBox = new System.Windows.Forms.Form()
			{
				FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
				ClientSize = size,
				Text = _Title,
				MaximizeBox = false,
				MinimizeBox = false
			};

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

		#region Imagen

		public static String Number_To_RGBA_Normalized(decimal _Value,
			decimal A_MinValue = 0, decimal A_MaxValue = 1,
			decimal B_MinValue = 0, decimal B_MaxValue = 1,
			int DecimalPlaces = 6)
		{
			string _Ret = string.Empty;
			decimal _RedDec = NormalizeNumber(A_MinValue, A_MaxValue, B_MinValue, B_MaxValue);

			_Ret = string.Format("{0}", Math.Round(_RedDec, DecimalPlaces).ToString().Replace(',', '.').PadRight(DecimalPlaces + 2, '0'));
			return _Ret;
		}

		public static decimal NormalizeNumber(decimal _Value,
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

		

		/// <summary>Convierte un color RGBA (0 -> 255) a la escala indicada, por defecto 0.0 -> 1.0</summary>
		/// <param name="_Color"></param>
		/// <param name="MinValue"></param>
		/// <param name="MaxValue"></param>
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

			_Ret = string.Format("{0}, {1}, {2}, {3}",
				Math.Round(_RedDec, 6).ToString(),
				Math.Round(_GreenDec, 6).ToString(),
				Math.Round(_BlueDec, 6).ToString(),
				Math.Round(_AlfaDec, 6).ToString()
			);

			return _Ret;
		}
		public static Color Color_From_RGBA_Normalized(string _Red, string _Green, string _Blue, string _Alpha)
		{
			Color _ret = Color.White;
			decimal MinValue = 0;
			decimal MaxValue = 255;

			decimal _R = Convert.ToDecimal(_Red);
			decimal _G = Convert.ToDecimal(_Green);
			decimal _B = Convert.ToDecimal(_Blue);
			decimal _A = Convert.ToDecimal(_Alpha);

			decimal _RedDec = (MaxValue - MinValue) / (1 - 0) * (_R - 1) + MaxValue;
			decimal _GreenDec = (MaxValue - MinValue) / (1 - 0) * (_G - 1) + MaxValue;
			decimal _BlueDec = (MaxValue - MinValue) / (1 - 0) * (_B - 1) + MaxValue;
			decimal _AlfaDec = (MaxValue - MinValue) / (1 - 0) * (_A - 1) + MaxValue;

			_ret = Color.FromArgb(
				Convert.ToInt32(_AlfaDec),
				Convert.ToInt32(_RedDec),
				Convert.ToInt32(_GreenDec),
				Convert.ToInt32(_BlueDec));

			return _ret;
		}
		public static String Color_To_RGBA_Normalized_EX(System.Drawing.Color _Color, List<key_value> ColorFixes, decimal MinValue = 0, decimal MaxValue = 1)
		{
			string _Ret = string.Empty;

			decimal _R = Convert.ToDecimal(_Color.R.ToString());
			decimal _RedDec = 0;

			decimal _G = Convert.ToDecimal(_Color.G.ToString());
			decimal _GreenDec = 0;

			decimal _B = Convert.ToDecimal(_Color.B.ToString());
			decimal _BlueDec = 0;

			decimal _A = Convert.ToDecimal(_Color.A.ToString());
			decimal _AlfaDec = 0;

			//If there is a Calibration Table for the Element:
			if (ColorFixes.IsNotEmpty())
			{
				//Find the right Value in the Calibration Table:
				_RedDec = ColorFixes.Find(x => x.key == _R).value;
				_GreenDec = ColorFixes.Find(x => x.key == _G).value;
				_BlueDec = ColorFixes.Find(x => x.key == _B).value;
				_AlfaDec = 0;
			}
			else
			{
				/* Fomula for Normalize Ranges:
				 * newvalue = (max2 - min2)/ (max1 - min1) * (value - max1) + max2 */
				_RedDec = (MaxValue - MinValue) / (255 - 0) * (_R - 255) + MaxValue;
				_GreenDec = (MaxValue - MinValue) / (255 - 0) * (_G - 255) + MaxValue;
				_BlueDec = (MaxValue - MinValue) / (255 - 0) * (_B - 255) + MaxValue;
				_AlfaDec = 0;
			}

			//Color Format is ARGB:
			_Ret = string.Format("({0}, {1}, {2}, {3})",
					Math.Round(_AlfaDec, 6).ToString().Replace(',', '.').PadRight(8, '0'),
					Math.Round(_RedDec, 6).ToString().Replace(',', '.').PadRight(8, '0'),
					Math.Round(_GreenDec, 6).ToString().Replace(',', '.').PadRight(8, '0'),
					Math.Round(_BlueDec, 6).ToString().Replace(',', '.').PadRight(8, '0'));

			return _Ret;
		}


		/// <summary>Abre la Imagen indicada (si existe) sin dejarla 'en uso'.</summary>
		/// <param name="_ImagePath">Ruta Completa al Archivo</param>
		public static Image GetElementImage(string _ImagePath, string _DefaultImage = "")
		{
			//Abre una Imagen sin dejarla 'en uso':
			Image _ret = null;
			try
			{
				if (File.Exists(_ImagePath))
				{
					using (var s = new System.IO.FileStream(_ImagePath, System.IO.FileMode.Open))
					{
						_ret = Image.FromStream(s);
					}
				}
				else
				{
					if (_DefaultImage != string.Empty && File.Exists(_DefaultImage))
					{
						using (var s = new System.IO.FileStream(_DefaultImage, System.IO.FileMode.Open))
						{
							_ret = Image.FromStream(s);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		/// <summary>Abre la Imagen indicada (si existe) sin dejarla 'en uso'.</summary>
		/// <param name="_ImagePath">Ruta Completa al Archivo</param>
		public static Image GetElementBitmap(string _ImagePath, string _DefaultImage = "")
		{
			//Abre una Imagen sin dejarla 'en uso':
			Image _ret = null;
			try
			{
				if (File.Exists(_ImagePath))
				{
					_ret = Bitmap.FromFile(_ImagePath);
					//using (var s = new System.IO.FileStream(_ImagePath, System.IO.FileMode.Open))
					//{
					//	_ret = Bitmap.FromStream(s);
					//}
				}
				else
				{
					if (_DefaultImage != string.Empty && File.Exists(_DefaultImage))
					{
						_ret = Bitmap.FromFile(_DefaultImage);
						//using (var s = new System.IO.FileStream(_DefaultImage, System.IO.FileMode.Open))
						//{
						//	_ret = Bitmap.FromStream(s);
						//}
					}
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


		/// <summary>Crea una Matrix de Identidad de Color.</summary>
		public static float[][] CreateIdentityMatrix()
		{
			float[][] _ret;
			try
			{
				/* R=Red, G=Green, B=Blue, A=Alpha, I=Intensity */
				//--------------------------------------------------------
				//| RR (0,0) | RG (0,1) | RB (0,2) | RA (0,3) | RI (0,4) |  <- 0,4 Debe ser Siempre 0
				//| GR (1,0) | GG (1,1) | GB (1,2) | GA (1,3) | GI (1,4) |  <- 1,4 Debe ser Siempre 0
				//| BR (2,0) | BG (2,1) | BB (2,2) | BA (2,3) | BI (2,4) |  <- 2,4 Debe ser Siempre 0
				//| AR (3,0) | AG (3,1) | AB (3,2) | AA (3,3) | AI (3,4) |  <- 3,4 Debe ser Siempre 0
				//| IR (4,0) | IG (4,1) | IB (4,2) | IA (4,3) | II (4,4) |  <- 4,4 Debe ser siempre 1
				//--------------------------------------------------------
				// 3,3 Determina la Transparencia Global de la Imagen

				float[][] colorMatrixElements = {
				   new float[] { 1,  0,  0,  0,  0 },        // red scaling 
				   new float[] { 0,  1,  0,  0,  0 },        // green scaling 
				   new float[] { 0,  0,  1,  0,  0 },        // blue scaling 
				   new float[] { 0,  0,  0,  1,  0 },		// alpha scaling 
				   new float[] { 0,  0,  0,  0,  1 }
				};
				_ret = colorMatrixElements;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		/// <summary>Aplica una Matrix de Color sobre una Imagen</summary>
		/// <param name="_Image">Imagen Original</param>
		/// <param name="colorMatrix">Matrix de Color 5x5</param>
		public static Image ApplyColorMatrix(Image _Image, float[][] colorMatrix)
		{
			Image _ret = null;
			try
			{
				System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
				int width = _Image.Width;
				int height = _Image.Height;

				System.Drawing.Imaging.ColorMatrix ImageMatrix = new System.Drawing.Imaging.ColorMatrix(colorMatrix);
				imageAttributes.SetColorMatrix(ImageMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

				using (System.Drawing.Graphics G = Graphics.FromImage(_Image))
				{
					G.DrawImage(
					   _Image,
					   new Rectangle(0, 0, width, height),  // destination rectangle 
					   0, 0,        // upper-left corner of source rectangle 
					   width,       // width of source rectangle
					   height,      // height of source rectangle
					   GraphicsUnit.Pixel,
					   imageAttributes);
				}
				_ret = _Image;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return _ret;
		}

		/// <summary>Reemplaza el Color Indicado en la Imagen con Otro color a eleccion.</summary>
		/// <param name="inputImage">Imagen Original</param>
		/// <param name="tolerance">Tolerancia del Color (0-255)</param>
		/// <param name="oldColor">Color Original a Cambiar</param>
		/// <param name="NewColor">Color Nuevo</param>
		public static Image ColorReplace(Image inputImage, int tolerance, Color oldColor, Color NewColor)
		{
			Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height);
			Graphics G = Graphics.FromImage(outputImage);
			G.DrawImage(inputImage, 0, 0);
			for (Int32 y = 0; y < outputImage.Height; y++)
				for (Int32 x = 0; x < outputImage.Width; x++)
				{
					Color PixelColor = outputImage.GetPixel(x, y);
					if (PixelColor.R > oldColor.R - tolerance && PixelColor.R < oldColor.R + tolerance && PixelColor.G > oldColor.G - tolerance && PixelColor.G < oldColor.G + tolerance && PixelColor.B > oldColor.B - tolerance && PixelColor.B < oldColor.B + tolerance)
					{
						int RColorDiff = oldColor.R - PixelColor.R;
						int GColorDiff = oldColor.G - PixelColor.G;
						int BColorDiff = oldColor.B - PixelColor.B;

						if (PixelColor.R > oldColor.R) RColorDiff = NewColor.R + RColorDiff;
						else RColorDiff = NewColor.R - RColorDiff;
						if (RColorDiff > 255) RColorDiff = 255;
						if (RColorDiff < 0) RColorDiff = 0;
						if (PixelColor.G > oldColor.G) GColorDiff = NewColor.G + GColorDiff;
						else GColorDiff = NewColor.G - GColorDiff;
						if (GColorDiff > 255) GColorDiff = 255;
						if (GColorDiff < 0) GColorDiff = 0;
						if (PixelColor.B > oldColor.B) BColorDiff = NewColor.B + BColorDiff;
						else BColorDiff = NewColor.B - BColorDiff;
						if (BColorDiff > 255) BColorDiff = 255;
						if (BColorDiff < 0) BColorDiff = 0;

						outputImage.SetPixel(x, y, Color.FromArgb(RColorDiff, GColorDiff, BColorDiff));
					}
				}
			return outputImage;
		}

		public static Image ColorRise(Image _Image, Color pOldColor, Color pNewColor)
		{
			Bitmap _ret = new Bitmap(_Image.Width, _Image.Height);

			using (System.Drawing.Graphics G = Graphics.FromImage(_Image))
			{
				// Set the image attribute's color mappings
				ColorMap[] colorMap = new ColorMap[1];
				colorMap[0] = new ColorMap
				{
					OldColor = pOldColor,
					NewColor = pNewColor
				};

				ImageAttributes attr = new ImageAttributes();
				attr.SetRemapTable(colorMap);

				// Draw using the color map
				Rectangle rect = new Rectangle(0, 0, _ret.Width, _ret.Height);
				G.DrawImage(_ret, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, attr);
			}
			return _ret;
		}

		/// <summary>Colorea una imagen usando una Matrix de Color.</summary>
		/// <param name="bmp">Imagen a Colorear</param>
		/// <param name="c">Color a Utilizar</param>
		public static Bitmap ChangeToColor(Bitmap bmp, Color c)
		{
			Bitmap bmp2 = new Bitmap(bmp.Width, bmp.Height);
			using (Graphics g = Graphics.FromImage(bmp2))
			{
				float tR = c.R / 255f;
				float tG = c.G / 255f;
				float tB = c.B / 255f;

				ColorMatrix colorMatrix = new ColorMatrix(new float[][]
				{
					new float[] { 0,    0,  0,  0,  0 },
					new float[] { 0,    0,  0,  0,  0 },
					new float[] { 0,    0,  0,  0,  0 },
					new float[] { 0,    0,  0,  1,  0 },  //<- not changing alpha
					new float[] { tR,   tG, tB, 0,  1 }
				});

				ImageAttributes attributes = new ImageAttributes();
				attributes.SetColorMatrix(colorMatrix);

				g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height),
					0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
			}
			return bmp2;
		}

		public static Bitmap GradientImage(int width, int height, Color color1, Color color2, float angle)
		{
			var r = new Rectangle(0, 0, width, height);
			var bmp = new Bitmap(width, height);
			using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(r, color1, color2, angle, true))
			using (var g = Graphics.FromImage(bmp))
				g.FillRectangle(brush, r);
			return bmp;
		}

		//public enum ChannelARGB
		//{
		//	Blue = 0,
		//	Green = 1,
		//	Red = 2,
		//	Alpha = 3
		//}
		//public static void transferOneARGBChannelFromOneBitmapToAnother(
		//	Bitmap source, Bitmap dest, ChannelARGB sourceChannel, ChannelARGB destChannel)
		//{
		//	if (source.Size != dest.Size)
		//		throw new ArgumentException();
		//	Rectangle r = new Rectangle(Point.Empty, source.Size);
		//	System.Drawing.Imaging.BitmapData bdSrc = source.LockBits(r, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		//	BitmapData bdDst = dest.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
		//	unsafe
		//	{
		//		byte* bpSrc = (byte*)bdSrc.Scan0.ToPointer();
		//		byte* bpDst = (byte*)bdDst.Scan0.ToPointer();
		//		bpSrc += (int)sourceChannel;
		//		bpDst += (int)destChannel;
		//		for (int i = r.Height * r.Width; i > 0; i--)
		//		{
		//			*bpDst = *bpSrc;
		//			bpSrc += 4;
		//			bpDst += 4;
		//		}
		//	}
		//	source.UnlockBits(bdSrc);
		//	dest.UnlockBits(bdDst);
		//}


		/// <summary>Dibuja una imagen sobre otra (capas)</summary>
		/// <param name="BottomLayer">Imagen de la Capa más Baja</param>
		/// <param name="TopLayer">Imagen de la Capa Superior</param>
		/// <param name="margin">Margen o Padding aplicado</param>
		public static Bitmap Superimpose(Bitmap BottomLayer, Bitmap TopLayer, int margin = 0)
		{
			using (Graphics g = Graphics.FromImage(BottomLayer))
			{
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
				TopLayer.MakeTransparent();
				int x = BottomLayer.Width - TopLayer.Width - margin;
				int y = BottomLayer.Height - TopLayer.Height - margin;
				g.DrawImage(TopLayer, new Point(x, y));
			}
			return BottomLayer;
		}

		/// <summary>Hace la imagen mas o menos Transparente</summary>
		/// <param name="image">Imagen Original</param>
		/// <param name="opacity">Nivel de Opacidad (0.0 - 1.0)</param>
		public static Image SetOpacity(this Image image, float opacity)
		{
			var colorMatrix = new System.Drawing.Imaging.ColorMatrix();
			colorMatrix.Matrix33 = opacity;

			var imageAttributes = new System.Drawing.Imaging.ImageAttributes();
			imageAttributes.SetColorMatrix(
				colorMatrix,
				System.Drawing.Imaging.ColorMatrixFlag.Default,
				System.Drawing.Imaging.ColorAdjustType.Bitmap);

			var output = new Bitmap(image.Width, image.Height);

			using (Graphics gfx = Graphics.FromImage(output))
			{
				gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				gfx.DrawImage(
					image,
					new Rectangle(0, 0, image.Width, image.Height),
					0,
					0,
					image.Width,
					image.Height,
					GraphicsUnit.Pixel,
					imageAttributes);
			}
			return output;
		}

		/// <summary>Establece el Brillo y el Contraste de la Imagen</summary>
		/// <param name="image">Imagen Original</param>
		/// <param name="brightness">(0.0 - 1.0)</param>
		/// <param name="contrast">(0.0 - 1.0)</param>
		/// <param name="gamma">(0.0 - 1.0)</param>
		public static Image SetBrightness(this Image image, float brightness, float contrast = 1.0f, float gamma = 1.0f)
		{
			Bitmap output = new Bitmap(image.Width, image.Height);

			/* R=Red, G=Green, B=Blue, A=Alpha, I=Intensity */
			//--------------------------------------------------------
			//| RR (0,0) | RG (0,1) | RB (0,2) | RA (0,3) | RI (0,4) |  <- 0,4 Debe ser Siempre 0
			//| GR (1,0) | GG (1,1) | GB (1,2) | GA (1,3) | GI (1,4) |  <- 1,4 Debe ser Siempre 0
			//| BR (2,0) | BG (2,1) | BB (2,2) | BA (2,3) | BI (2,4) |  <- 2,4 Debe ser Siempre 0
			//| AR (3,0) | AG (3,1) | AB (3,2) | AA (3,3) | AI (3,4) |  <- 3,4 Debe ser Siempre 0
			//| IR (4,0) | IG (4,1) | IB (4,2) | IA (4,3) | II (4,4) |  <- 4,4 Debe ser siempre 1
			//--------------------------------------------------------
			// 3,3 Determina la Transparencia Global de la Imagen

			float _Bright = brightness - 1.0f;
			float[][] ptsArray ={
				new float[] { contrast, 0,          0,          0,      0 }, // scale red
				new float[] { 0,        contrast,   0,          0,      0 }, // scale green
				new float[] { 0,        0,          contrast,   0,      0 }, // scale blue
				new float[] { 0,        0,          0,          1.0f,   0 }, // don't scale alpha
				new float[] { _Bright, _Bright,     _Bright,    0,      1 }
			};

			System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
			imageAttributes.ClearColorMatrix();
			imageAttributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(ptsArray),
				System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
			imageAttributes.SetGamma(gamma, System.Drawing.Imaging.ColorAdjustType.Bitmap);

			using (Graphics g = Graphics.FromImage(output))
			{
				g.DrawImage(image, new Rectangle(0, 0, output.Width, output.Height),
					0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			}
			return output;
		}

		// Perform gamma correction on the image.
		public static Bitmap AdjustGamma(Image image, float gamma = 2.2f)
		{
			// Set the ImageAttributes object's gamma value.
			System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
			attributes.SetGamma(gamma);

			// Draw the image onto the new bitmap
			// while applying the new gamma value.
			Point[] points =
			{
				new Point(0, 0),
				new Point(image.Width, 0),
				new Point(0, image.Height),
			};
			Rectangle rect =
				new Rectangle(0, 0, image.Width, image.Height);

			// Make the result bitmap.
			Bitmap bm = new Bitmap(image.Width, image.Height);
			using (Graphics gr = Graphics.FromImage(bm))
			{
				gr.DrawImage(image, points, rect,
					GraphicsUnit.Pixel, attributes);
			}

			// Return the result.
			return bm;
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

								zip.AddFile(_File, Relative);
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

		#region Algebra - Matrices

		public static double[][] MatrixCreate(int rows, int cols)
		{
			double[][] result = new double[rows][];
			for (int i = 0; i < rows; ++i)
				result[i] = new double[cols];
			return result;
		}
		public static double[][] MatrixIdentity(int n)
		{
			// return an n x n Identity matrix
			double[][] result = MatrixCreate(n, n);
			for (int i = 0; i < n; ++i)
				result[i][i] = 1.0;

			return result;
		}
		public static double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
		{
			int aRows = matrixA.Length; int aCols = matrixA[0].Length;
			int bRows = matrixB.Length; int bCols = matrixB[0].Length;
			if (aCols != bRows)
				throw new Exception("Non-conformable matrices in MatrixProduct");

			double[][] result = MatrixCreate(aRows, bCols);

			for (int i = 0; i < aRows; ++i) // each row of A
				for (int j = 0; j < bCols; ++j) // each col of B
					for (int k = 0; k < aCols; ++k) // could use k less-than bRows
						result[i][j] += matrixA[i][k] * matrixB[k][j];

			return result;
		}
		public static double[][] MatrixInverse(double[][] matrix)
		{
			int n = matrix.Length;
			double[][] result = MatrixDuplicate(matrix);

			int[] perm;
			int toggle;
			double[][] lum = MatrixDecompose(matrix, out perm,
			  out toggle);
			if (lum == null)
				throw new Exception("Unable to compute inverse");

			double[] b = new double[n];
			for (int i = 0; i < n; ++i)
			{
				for (int j = 0; j < n; ++j)
				{
					if (i == perm[j])
						b[j] = 1.0;
					else
						b[j] = 0.0;
				}

				double[] x = HelperSolve(lum, b);

				for (int j = 0; j < n; ++j)
					result[j][i] = x[j];
			}
			return result;
		}
		public static double[][] MatrixDuplicate(double[][] matrix)
		{
			// allocates/creates a duplicate of a matrix.
			double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
			for (int i = 0; i < matrix.Length; ++i) // copy the values
				for (int j = 0; j < matrix[i].Length; ++j)
					result[i][j] = matrix[i][j];
			return result;
		}
		public static double[] HelperSolve(double[][] luMatrix, double[] b)
		{
			// before calling this helper, permute b using the perm array
			// from MatrixDecompose that generated luMatrix
			int n = luMatrix.Length;
			double[] x = new double[n];
			b.CopyTo(x, 0);

			for (int i = 1; i < n; ++i)
			{
				double sum = x[i];
				for (int j = 0; j < i; ++j)
					sum -= luMatrix[i][j] * x[j];
				x[i] = sum;
			}

			x[n - 1] /= luMatrix[n - 1][n - 1];
			for (int i = n - 2; i >= 0; --i)
			{
				double sum = x[i];
				for (int j = i + 1; j < n; ++j)
					sum -= luMatrix[i][j] * x[j];
				x[i] = sum / luMatrix[i][i];
			}

			return x;
		}
		public static double[][] MatrixDecompose(double[][] matrix, out int[] perm, out int toggle)
		{
			// Doolittle LUP decomposition with partial pivoting.
			// rerturns: result is L (with 1s on diagonal) and U;
			// perm holds row permutations; toggle is +1 or -1 (even or odd)
			int rows = matrix.Length;
			int cols = matrix[0].Length; // assume square
			if (rows != cols)
				throw new Exception("Attempt to decompose a non-square m");

			int n = rows; // convenience

			double[][] result = MatrixDuplicate(matrix);

			perm = new int[n]; // set up row permutation result
			for (int i = 0; i < n; ++i) { perm[i] = i; }

			toggle = 1; // toggle tracks row swaps.
						// +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

			for (int j = 0; j < n - 1; ++j) // each column
			{
				double colMax = Math.Abs(result[j][j]); // find largest val in col
				int pRow = j;
				//for (int i = j + 1; i less-than n; ++i)
				//{
				//  if (result[i][j] greater-than colMax)
				//  {
				//    colMax = result[i][j];
				//    pRow = i;
				//  }
				//}

				// reader Matt V needed this:
				for (int i = j + 1; i < n; ++i)
				{
					if (Math.Abs(result[i][j]) > colMax)
					{
						colMax = Math.Abs(result[i][j]);
						pRow = i;
					}
				}
				// Not sure if this approach is needed always, or not.

				if (pRow != j) // if largest value not on pivot, swap rows
				{
					double[] rowPtr = result[pRow];
					result[pRow] = result[j];
					result[j] = rowPtr;

					int tmp = perm[pRow]; // and swap perm info
					perm[pRow] = perm[j];
					perm[j] = tmp;

					toggle = -toggle; // adjust the row-swap toggle
				}

				// --------------------------------------------------
				// This part added later (not in original)
				// and replaces the 'return null' below.
				// if there is a 0 on the diagonal, find a good row
				// from i = j+1 down that doesn't have
				// a 0 in column j, and swap that good row with row j
				// --------------------------------------------------

				if (result[j][j] == 0.0)
				{
					// find a good row to swap
					int goodRow = -1;
					for (int row = j + 1; row < n; ++row)
					{
						if (result[row][j] != 0.0)
							goodRow = row;
					}

					if (goodRow == -1)
						throw new Exception("Cannot use Doolittle's method");

					// swap rows so 0.0 no longer on diagonal
					double[] rowPtr = result[goodRow];
					result[goodRow] = result[j];
					result[j] = rowPtr;

					int tmp = perm[goodRow]; // and swap perm info
					perm[goodRow] = perm[j];
					perm[j] = tmp;

					toggle = -toggle; // adjust the row-swap toggle
				}
				// --------------------------------------------------
				// if diagonal after swap is zero . .
				//if (Math.Abs(result[j][j]) less-than 1.0E-20) 
				//  return null; // consider a throw

				for (int i = j + 1; i < n; ++i)
				{
					result[i][j] /= result[j][j];
					for (int k = j + 1; k < n; ++k)
					{
						result[i][k] -= result[i][j] * result[j][k];
					}
				}


			} // main j column loop

			return result;
		}

		#endregion

		public static IEnumerable<Color> GetGradients(Color start, Color end, int steps)
		{
			int stepA = ((end.A - start.A) / (steps - 1));
			int stepR = ((end.R - start.R) / (steps - 1));
			int stepG = ((end.G - start.G) / (steps - 1));
			int stepB = ((end.B - start.B) / (steps - 1));

			for (int i = 0; i < steps; i++)
			{
				yield return Color.FromArgb(start.A + (stepA * i),
											start.R + (stepR * i),
											start.G + (stepG * i),
											start.B + (stepB * i));
			}
		}

		public static string GetPrettyDate(DateTime d)
		{
			// 1.
			// Get time span elapsed since the date.
			TimeSpan s = DateTime.Now.Subtract(d);

			// 2.
			// Get total number of days elapsed.
			int dayDiff = (int)s.TotalDays;

			// 3.
			// Get total number of seconds elapsed.
			int secDiff = (int)s.TotalSeconds;

			// 4.
			// Don't allow out of range values.
			if (dayDiff < 0 || dayDiff >= 31)
			{
				return null;
			}

			// 5.
			// Handle same-day times.
			if (dayDiff == 0)
			{
				// A.
				// Less than one minute ago.
				if (secDiff < 60)
				{
					return "just now";
				}
				// B.
				// Less than 2 minutes ago.
				if (secDiff < 120)
				{
					return string.Format("{0:D2}:{1:D2} minutes ago",
						s.Minutes, s.Seconds);
				}
				// C.
				// Less than one hour ago.
				if (secDiff < 3600)
				{
					return string.Format("{0:D2}:{1:D2} minutes ago",
						s.Minutes, s.Seconds);
				}
				// D.
				// Less than 2 hours ago.
				if (secDiff < 7200)
				{
					return string.Format("1 hour ago ({0})", d.ToShortTimeString());
				}
				// E.
				// Less than one day ago.
				if (secDiff < 86400)
				{
					return string.Format("{0:D2}:{1:D2} hours ago",
						s.Hours, s.Seconds);
				}
			}
			// 6.
			// Handle previous days.
			if (dayDiff == 1)
			{
				return string.Format("Yesterday at {0}", d.ToShortTimeString());
			}
			if (dayDiff < 7)
			{
				return string.Format("{0} days ago ({1})",
					dayDiff, d.ToString());
			}
			if (dayDiff < 31)
			{
				return string.Format("{0} weeks ago ({1})",
					Math.Ceiling((double)dayDiff / 7), d.ToString());
			}
			if (dayDiff > 31)
			{
				return d.ToString();
			}
			return null;
		}

		/// <summary>Envia una solicitud a un sitio we usando GET.</summary>
		/// <param name="uri">Direccion URL dek sitio</param>
		/// <returns></returns>
		public static string WebRequest_GET(string uri)
		{
			System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
			request.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

			using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
		public static async Task<string> WebRequest_GetAsync(string uri)
		{
			System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
			request.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

			using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)await request.GetResponseAsync())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				return await reader.ReadToEndAsync();
			}
		}

		public static string WebRequest_POST(string uri, string data, string contentType, string method = "POST")
		{
			byte[] dataBytes = Encoding.UTF8.GetBytes(data);

			System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
			request.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
			request.ContentLength = dataBytes.Length;
			request.ContentType = contentType;
			request.Method = method;

			using (Stream requestBody = request.GetRequestStream())
			{
				requestBody.Write(dataBytes, 0, dataBytes.Length);
			}

			using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
		public static async Task<string> WebRequest_PostAsync(string uri, string data, string contentType, string method = "POST")
		{
			byte[] dataBytes = Encoding.UTF8.GetBytes(data);

			System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
			request.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
			request.ContentLength = dataBytes.Length;
			request.ContentType = contentType;
			request.Method = method;

			using (Stream requestBody = request.GetRequestStream())
			{
				await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
			}

			using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)await request.GetResponseAsync())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				return await reader.ReadToEndAsync();
			}
		}
	}

	[Serializable]
	public class key_value : ICloneable
	{
		public key_value() { }
		public key_value(int _Key, decimal _Value)
		{
			this.key = _Key;
			this.value = _Value;
		}

		public int key { get; set; }
		public decimal value { get; set; }

		public object Clone()
		{
			return (key_value)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", this.key, this.value);
		}
	}

	[Serializable]
	public class key_value_ex : ICloneable
	{
		public key_value_ex() { }
		public key_value_ex(string _Key, string _Value)
		{
			this.key = _Key;
			this.value = _Value;
		}

		public string key { get; set; }
		public string value { get; set; }
		public string description { get; set; }
		public string extra { get; set; }

		public object Clone()
		{
			return (key_value_ex)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", this.key, this.value);
		}
	}

	[Serializable]
	public class value_key : ICloneable
	{
		public value_key() { }
		public value_key(string _Key, decimal _Value)
		{
			this.key = _Key;
			this.value = _Value;
		}

		public string key { get; set; }
		public decimal value { get; set; }

		public object Clone()
		{
			return (value_key)MemberwiseClone();
		}
		public override string ToString()
		{
			return string.Format("{0}: {1}", this.key, this.value);
		}
	}


	[Serializable]
	public class Codiguera
	{
		public Codiguera() { }
		public Codiguera(int _code, string _description)
		{
			this.code = _code;
			this.description = _description;
		}

		public int code { get; set; }
		public string description { get; set; }
	}

	public static class ImageHelper
	{
		public static byte[] CropImage(byte[] content, int x, int y, int width, int height)
		{
			using (MemoryStream stream = new MemoryStream(content))
			{
				return CropImage(stream, x, y, width, height);
			}
		}

		public static byte[] CropImage(Stream content, int x, int y, int width, int height)
		{
			//Parsing stream to bitmap
			using (Bitmap sourceBitmap = new Bitmap(content))
			{
				//Get new dimensions
				double sourceWidth = Convert.ToDouble(sourceBitmap.Size.Width);
				double sourceHeight = Convert.ToDouble(sourceBitmap.Size.Height);
				Rectangle cropRect = new Rectangle(x, y, width, height);

				//Creating new bitmap with valid dimensions
				using (Bitmap newBitMap = new Bitmap(cropRect.Width, cropRect.Height))
				{
					using (Graphics g = Graphics.FromImage(newBitMap))
					{
						g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
						g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
						g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
						g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

						g.DrawImage(sourceBitmap, new Rectangle(0, 0, newBitMap.Width, newBitMap.Height), cropRect, GraphicsUnit.Pixel);

						return GetBitmapBytes(newBitMap);
					}
				}
			}
		}

		public static byte[] GetBitmapBytes(Bitmap source)
		{
			//Settings to increase quality of the image
			System.Drawing.Imaging.ImageCodecInfo codec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()[4];
			System.Drawing.Imaging.EncoderParameters parameters = new System.Drawing.Imaging.EncoderParameters(1);
			parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

			//Temporary stream to save the bitmap
			using (MemoryStream tmpStream = new MemoryStream())
			{
				source.Save(tmpStream, codec, parameters);

				//Get image bytes from temporary stream
				byte[] result = new byte[tmpStream.Length];
				tmpStream.Seek(0, SeekOrigin.Begin);
				tmpStream.Read(result, 0, (int)tmpStream.Length);

				return result;
			}
		}
		public static Image Resize(Image current, int maxWidth, int maxHeight)
		{
			int width, height;
			#region reckon size 
			if (current.Width > current.Height)
			{
				width = maxWidth;
				height = Convert.ToInt32(current.Height * maxHeight / (double)current.Width);
			}
			else
			{
				width = Convert.ToInt32(current.Width * maxWidth / (double)current.Height);
				height = maxHeight;
			}
			#endregion

			#region get resized bitmap 
			var canvas = new Bitmap(width, height);

			using (var graphics = Graphics.FromImage(canvas))
			{
				graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				graphics.DrawImage(current, 0, 0, width, height);
			}

			return canvas;
			#endregion
		}

		/// <summary>Convierte una Imagen en un Array de Bits para su almacenamiento en
		/// un campo BLOB de la Base de Datos.</summary>
		/// <param name="imageIn">Imagen extraida del Control.</param>
		/// <returns>Array de Bits con los datos de la Imagen.</returns>
		public static byte[] imageToByteArray(System.Drawing.Image imageIn)
		{
			byte[] retorno = null;
			MemoryStream ms = null;
			try
			{
				ms = new MemoryStream();
				imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
				retorno = ms.ToArray();
			}
			catch (Exception ex) { throw ex; }
			finally { if (ms != null) { ms.Close(); } }
			return retorno;
		}

		/// <summary>Convierte una Imagen almacenada en un campo BLOB de la Base de Datos 
		/// en un Objeto Image para mostrar en los Controles.</summary>
		/// <param name="byteArrayIn">Array de Bits que contiene la Imagen.</param>
		/// <returns>Imagen lista para Mostrar.</returns>
		public static Image byteArrayToImage(byte[] byteArrayIn)
		{
			Image retorno = null;
			MemoryStream ms = null;
			try
			{
				ms = new MemoryStream(byteArrayIn);
				retorno = Image.FromStream(ms);
			}
			catch (Exception ex) { throw ex; }
			finally { if (ms != null) { ms.Close(); } }
			return retorno;
		}

		public static Image ScaleImage(Image image, int height)
		{
			double ratio = (double)height / image.Height;
			int newWidth = (int)(image.Width * ratio);
			int newHeight = (int)(image.Height * ratio);
			Bitmap newImage = new Bitmap(newWidth, newHeight);
			using (Graphics g = Graphics.FromImage(newImage))
			{
				g.DrawImage(image, 0, 0, newWidth, newHeight);
			}
			image.Dispose();
			return newImage;
		}
	}

	public static class AsyncHelpers
	{
		/*   USO:
		 *  customerList = AsyncHelpers.RunSync<List<Customer>>(() => GetCustomers());
		 *  
		 */


		/// <summary>
		/// Execute's an async Task<T> method which has a void return value synchronously
		/// </summary>
		/// <param name="task">Task<T> method to execute</param>
		public static void RunSync(Func<Task> task)
		{
			var oldContext = SynchronizationContext.Current;
			var synch = new ExclusiveSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synch);
			synch.Post(async _ =>
			{
				try
				{
					await task();
				}
				catch (Exception e)
				{
					synch.InnerException = e;
					throw;
				}
				finally
				{
					synch.EndMessageLoop();
				}
			}, null);
			synch.BeginMessageLoop();

			SynchronizationContext.SetSynchronizationContext(oldContext);
		}

		/// <summary>
		/// Execute's an async Task<T> method which has a T return type synchronously
		/// </summary>
		/// <typeparam name="T">Return Type</typeparam>
		/// <param name="task">Task<T> method to execute</param>
		/// <returns></returns>
		public static T RunSync<T>(Func<Task<T>> task)
		{
			var oldContext = SynchronizationContext.Current;
			var synch = new ExclusiveSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synch);
			T ret = default(T);
			synch.Post(async _ =>
			{
				try
				{
					ret = await task();
				}
				catch (Exception e)
				{
					synch.InnerException = e;
					throw;
				}
				finally
				{
					synch.EndMessageLoop();
				}
			}, null);
			synch.BeginMessageLoop();
			SynchronizationContext.SetSynchronizationContext(oldContext);
			return ret;
		}

		private class ExclusiveSynchronizationContext : SynchronizationContext
		{
			private bool done;
			public Exception InnerException { get; set; }
			readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
			readonly Queue<Tuple<SendOrPostCallback, object>> items =
				new Queue<Tuple<SendOrPostCallback, object>>();

			public override void Send(SendOrPostCallback d, object state)
			{
				throw new NotSupportedException("We cannot send to our same thread");
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				lock (items)
				{
					items.Enqueue(Tuple.Create(d, state));
				}
				workItemsWaiting.Set();
			}

			public void EndMessageLoop()
			{
				Post(_ => done = true, null);
			}

			public void BeginMessageLoop()
			{
				while (!done)
				{
					Tuple<SendOrPostCallback, object> task = null;
					lock (items)
					{
						if (items.Count > 0)
						{
							task = items.Dequeue();
						}
					}
					if (task != null)
					{
						task.Item1(task.Item2);
						if (InnerException != null) // the method threw an exeption
						{
							throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
						}
					}
					else
					{
						workItemsWaiting.WaitOne();
					}
				}
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}
		}
	}
}
