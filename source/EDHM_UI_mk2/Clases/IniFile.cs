using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EDHM_UI_mk2
{
	public class IniFile
	{
		private string Path = string.Empty;
		private string EXE = Assembly.GetExecutingAssembly().GetName().Name;

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

		[DllImport("kernel32")]
		static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);

		[DllImport("kernel32.dll")]
		private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

		public IniFile(string IniPath = null)
		{
			this.Path = new FileInfo(IniPath ?? this.EXE + ".ini").FullName;
			if (!File.Exists(this.Path))
			{
				throw new System.Exception(string.Format("ERROR_404: NOT FOUND\r\nThe file: '{0}' does NOT exists!", this.Path));
			}
		}

		public string ReadKey(string Key, string Section = null)
		{
			string _ret = string.Empty;
			try
			{
				StringBuilder RetVal = new StringBuilder(255);
				int i = GetPrivateProfileString(Section, Key, "", RetVal, 255, this.Path);
				_ret =  RetVal.ToString();
			}
			catch { }
			return _ret;
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

		public List<string> GetAllSectionNames()
		{
			List<string> _ret = new List<string>();

			byte[] buffer = new byte[1024];
			GetPrivateProfileSectionNames(buffer, buffer.Length, this.Path);

			string allSections = System.Text.Encoding.Default.GetString(buffer);
			string[] sectionNames = allSections.Split('\0');
			foreach (string sectionName in sectionNames)
			{
				if (sectionName != string.Empty)
					_ret.Add(sectionName);
			}

			return _ret;
		}

		public List<string> GetAllKeys(string Section = null)
		{
			List<string> _ret = new List<string>();

			byte[] buffer = new byte[2048];
			GetPrivateProfileSection(Section, buffer, 2048, this.Path);

			string[] tmp = System.Text.Encoding.Default.GetString(buffer).Trim('\0').Split('\0');
			foreach (string entry in tmp)
			{
				if (!entry.EmptyOrNull()) _ret.Add(entry.Substring(0, entry.IndexOf("=")));
			}

			return _ret;
		}
	}
}