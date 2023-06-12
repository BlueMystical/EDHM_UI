using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace EDHM_UI_mk2.Clases
{
	/// <summary>Clase para manejar Vinculos Simbolicos entre Directorios.</summary>
	public static class SymLink
	{
		#region Declaraciones API Win32

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);	
		private enum SymbolicLink
		{
			File = 0,
			Directory = 1
		}

		#endregion

		/// <summary>Determina si el Directorio especificado es un 'Symbolic Link'
		/// <para>Devuelve 'false' si el Directorio NO existe.</para>
		/// <para>Devuelve 'true' si el Directorio es cualquiera de estos: [MountPoint, SymbolicLink, JunctionPoint]</para></summary>
		/// <param name="DirectoryPath">Ruta del Directorio a probar</param>
		public static bool IsSymbolicDirectory(string DirectoryPath)
		{
			bool _ret = false;
			try
			{
				if (Directory.Exists(DirectoryPath))
				{
					var Link = new ReparsePoint(DirectoryPath);
					if (Link != null && Link.Tag != ReparsePoint.TagType.None)
					{
						_ret = true;
					}
				}
			}
			catch (Exception ex) { throw ex; }
			return _ret;
		}

		/// <summary>Determina si el Directorio especificado es un 'Symbolic Link' y devuelve la ruta 'real' a donde apunta.</summary>
		/// <param name="DirectoryPath">Ruta del Directorio a probar</param>
		public static string GetTargetDirectory(string DirectoryPath)
		{
			string _ret = string.Empty;
			try
			{
				if (Directory.Exists(DirectoryPath))
				{
					var Link = new ReparsePoint(DirectoryPath);
					if (Link != null && Link.Tag != ReparsePoint.TagType.None)
					{
						_ret = Link.Target;
					}
				}
			}
			catch (Exception ex) { throw ex; }
			return _ret;
		}

		/// <summary>Crea un 'Symbolic Link' apuntando al Directorio especificado.</summary>
		/// <param name="linkPath">Ruta donde se Crea el Link.</param>
		/// <param name="targetPath">Ruta a donde apunta el Link.</param>
		public static bool CreateLinkToDirectory(string linkPath, string targetPath)
		{
			bool _ret = false;
			try
			{
				_ret = CreateSymbolicLink(linkPath, targetPath, SymbolicLink.Directory);
			}
			catch (Exception ex) { throw ex; }
			return _ret;
		}
	}

	/// <summary>Determina si un Directorio es un SymLink y si lo es ('Tag' > 0), devuelve la ruta a donde apunta ('Target').</summary>
	public class ReparsePoint
	{
		//Source:	https://www.codeproject.com/Articles/21202/Reparse-Points-in-Vista
		//			https://learn.microsoft.com/en-us/windows/win32/fileio/determining-whether-a-directory-is-a-volume-mount-point

		#region Declaraciones API Win32

		private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;     // Moiunt point or junction, see winnt.h
		private const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;         // SYMLINK or SYMLINKD (see http://wesnerm.blogs.com/net_undocumented/2006/10/index.html)
		private const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
		private const string SE_BACKUP_NAME = "SeBackupPrivilege";
		private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
		private const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
		private const uint FILE_DEVICE_FILE_SYSTEM = 9;
		private const uint FILE_ANY_ACCESS = 0;
		private const uint METHOD_BUFFERED = 0;
		private const int MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16 * 1024;
		private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
		private const int FSCTL_GET_REPARSE_POINT = 42;

		// This is the official version of the data buffer, see http://msdn2.microsoft.com/en-us/library/ms791514.aspx
		// not the one used at http://www.flexhex.com/docs/articles/hard-links.phtml
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct REPARSE_DATA_BUFFER
		{
			public uint ReparseTag;
			public short ReparseDataLength;
			public short Reserved;
			public short SubsNameOffset;
			public short SubsNameLength;
			public short PrintNameOffset;
			public short PrintNameLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXIMUM_REPARSE_DATA_BUFFER_SIZE)]
			public char[] ReparseTarget;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LUID
		{
			public UInt32 LowPart;
			public Int32 HighPart;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LUID_AND_ATTRIBUTES
		{
			public LUID Luid;
			public UInt32 Attributes;
		}

		private struct TOKEN_PRIVILEGES
		{
			public UInt32 PrivilegeCount;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]        // !! think we only need one
			public LUID_AND_ATTRIBUTES[] Privileges;
		}

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, out REPARSE_DATA_BUFFER outBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern IntPtr CreateFile(string fileName, [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess, [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
			int securityAttributes,
			[MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
			uint flags,
			IntPtr template);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

		[DllImport("kernel32.dll")]
		static extern IntPtr GetCurrentProcess();

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
			[MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
			ref TOKEN_PRIVILEGES NewState,
			Int32 BufferLength,
			//ref TOKEN_PRIVILEGES PreviousState,					!! for some reason this won't accept null
			IntPtr PreviousState,
			IntPtr ReturnLength);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);

		#endregion

		#region Declaraciones Privadas

		private string normalisedTarget;
		private string actualTarget;
		private TagType tag;

		public enum TagType
		{
			None = 0,
			MountPoint = 1,
			SymbolicLink = 2,
			JunctionPoint = 3
		}

		#endregion

		#region Constructor

		/// <summary>Takes a full path to a reparse point and finds the target. </summary>
		/// <param name="path">Full path of the reparse point</param>
		public ReparsePoint(string path)
		{
			Debug.Assert(!string.IsNullOrEmpty(path) && path.Length > 2 && path[1] == ':' && path[2] == '\\');
			normalisedTarget = "";
			tag = TagType.None;
			bool success;
			int lastError;
			// Apparently we need to have backup privileges
			IntPtr token;
			TOKEN_PRIVILEGES tokenPrivileges = new TOKEN_PRIVILEGES();
			tokenPrivileges.Privileges = new LUID_AND_ATTRIBUTES[1];
			success = OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES, out token);
			lastError = Marshal.GetLastWin32Error();
			if (success)
			{
				success = LookupPrivilegeValue(null, SE_BACKUP_NAME, out tokenPrivileges.Privileges[0].Luid);           // null for local system
				lastError = Marshal.GetLastWin32Error();
				if (success)
				{
					tokenPrivileges.PrivilegeCount = 1;
					tokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
					success = AdjustTokenPrivileges(token, false, ref tokenPrivileges, Marshal.SizeOf(tokenPrivileges), IntPtr.Zero, IntPtr.Zero);
					lastError = Marshal.GetLastWin32Error();
				}
				CloseHandle(token);
			}

			if (success)
			{
				// Open the file and get its handle
				IntPtr handle = CreateFile(path, FileAccess.Read, FileShare.None, 0, FileMode.Open, FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
				lastError = Marshal.GetLastWin32Error();
				if (handle.ToInt32() >= 0)
				{
					REPARSE_DATA_BUFFER buffer = new REPARSE_DATA_BUFFER();
					// Make up the control code - see CTL_CODE on ntddk.h
					uint controlCode = (FILE_DEVICE_FILE_SYSTEM << 16) | (FILE_ANY_ACCESS << 14) | (FSCTL_GET_REPARSE_POINT << 2) | METHOD_BUFFERED;
					uint bytesReturned;
					success = DeviceIoControl(handle, controlCode, IntPtr.Zero, 0, out buffer, MAXIMUM_REPARSE_DATA_BUFFER_SIZE, out bytesReturned, IntPtr.Zero);
					lastError = Marshal.GetLastWin32Error();
					if (success)
					{
						string subsString = "";
						string printString = "";
						// Note that according to http://wesnerm.blogs.com/net_undocumented/2006/10/symbolic_links_.html
						// Symbolic links store relative paths, while junctions use absolute paths
						// however, they can in fact be either, and may or may not have a leading \.
						Debug.Assert(buffer.ReparseTag == IO_REPARSE_TAG_SYMLINK || buffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT,
							"Unrecognised reparse tag");                        // We only recognise these two
						if (buffer.ReparseTag == IO_REPARSE_TAG_SYMLINK)
						{
							// for some reason symlinks seem to have an extra two characters on the front
							subsString = new string(buffer.ReparseTarget, (buffer.SubsNameOffset / 2 + 2), buffer.SubsNameLength / 2);
							printString = new string(buffer.ReparseTarget, (buffer.PrintNameOffset / 2 + 2), buffer.PrintNameLength / 2);
							tag = TagType.SymbolicLink;
						}
						else if (buffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
						{
							// This could be a junction or a mounted drive - a mounted drive starts with "\\??\\Volume"
							subsString = new string(buffer.ReparseTarget, buffer.SubsNameOffset / 2, buffer.SubsNameLength / 2);
							printString = new string(buffer.ReparseTarget, buffer.PrintNameOffset / 2, buffer.PrintNameLength / 2);
							tag = subsString.StartsWith(@"\??\Volume") ? TagType.MountPoint : TagType.JunctionPoint;
						}
						Debug.Assert(!(string.IsNullOrEmpty(subsString) && string.IsNullOrEmpty(printString)), "Failed to retrieve parse point");
						// the printstring should give us what we want
						if (!string.IsNullOrEmpty(printString))
						{
							normalisedTarget = printString;
						}
						else
						{
							// if not we can use the substring with a bit of tweaking
							normalisedTarget = subsString;
							Debug.Assert(normalisedTarget.Length > 2, "Target string too short");
							Debug.Assert(
								(normalisedTarget.StartsWith(@"\??\") && (normalisedTarget[5] == ':' || normalisedTarget.StartsWith(@"\??\Volume")) ||
								(!normalisedTarget.StartsWith(@"\??\") && normalisedTarget[1] != ':')),
								"Malformed subsString");
							// Junction points must be absolute
							Debug.Assert(
									buffer.ReparseTag == IO_REPARSE_TAG_SYMLINK ||
									normalisedTarget.StartsWith(@"\??\Volume") ||
									normalisedTarget[1] == ':',
								"Relative junction point");
							if (normalisedTarget.StartsWith(@"\??\"))
							{
								normalisedTarget = normalisedTarget.Substring(4);
							}
						}
						actualTarget = normalisedTarget;
						// Symlinks can be relative.
						if (buffer.ReparseTag == IO_REPARSE_TAG_SYMLINK && (normalisedTarget.Length < 2 || normalisedTarget[1] != ':'))
						{
							// it's relative, we need to tack it onto the path
							if (normalisedTarget[0] == '\\')
							{
								normalisedTarget = normalisedTarget.Substring(1);
							}
							if (path.EndsWith(@"\"))
							{
								path = path.Substring(0, path.Length - 1);
							}
							// Need to take the symlink name off the path
							normalisedTarget = path.Substring(0, path.LastIndexOf('\\')) + @"\" + normalisedTarget;
							// Note that if the symlink target path contains any ..s these are not normalised but returned as is.
						}
						// Remove any final slash for consistency
						if (normalisedTarget.EndsWith("\\"))
						{
							normalisedTarget = normalisedTarget.Substring(0, normalisedTarget.Length - 1);
						}
					}
					CloseHandle(handle);
				}
				else
				{
					success = false;
				}
			}
		}

		#endregion

		#region Metodos Publicos

		/// <summary>
		/// This returns the normalised target, ie. if the actual target is relative it has been made absolute
		/// Note that it is not fully normalised in that .s and ..s may still be included.
		/// </summary>
		/// <returns>The normalised path</returns>
		public override string ToString()
		{
			return normalisedTarget;
		}

		#endregion

		#region Propiedades Publicas

		/// <summary>
		/// Gets the actual target string, before normalising
		/// </summary>
		public string Target
		{
			get
			{
				return actualTarget;
			}
		}

		/// <summary>
		/// Gets the tag
		/// </summary>
		public TagType Tag
		{
			get
			{
				return tag;
			}
		}

		#endregion
	}
}
