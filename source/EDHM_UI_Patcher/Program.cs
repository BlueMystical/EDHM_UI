using System;
using System.Windows.Forms;

namespace EDHM_UI_Patcher
{
	static class Program
	{
		/// <summary>
		/// Punto de entrada principal para la aplicación.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (args != null)
			{
				Application.Run(new Form1(args[0]));				
			}
			else
			{
				MessageBox.Show("This Program requires 1 argument:  the path of the JSON file with the Update Information.");
			}
		}
	}
}
