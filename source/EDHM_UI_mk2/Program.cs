using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace EDHM_UI_mk2
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				//Matar Otras Instancias de esta Aplicacion:
				Process[] tbPro = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
				foreach (Process p in tbPro)
				{
					if (p.Id != Process.GetCurrentProcess().Id)
					{
						p.Kill();
					}
				}

				DevExpress.UserSkins.BonusSkins.Register();

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm(args));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
