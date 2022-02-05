﻿using System;
using System.Configuration;
using DevExpress.XtraSplashScreen;
using EDHM_DX;
using EDHM_UI_mk2;

namespace EDHM_UserInterface
{
	public partial class AboutForm : SplashScreen
	{
		public AboutForm()
		{
			InitializeComponent();
		}

		#region Overrides

		public override void ProcessCommand(Enum cmd, object arg)
		{
			base.ProcessCommand(cmd, arg);
		}

		#endregion

		public enum SplashScreenCommand
		{
		}

		private void AboutForm_Load(object sender, EventArgs e)
		{
			string _AppVersion= ConfigurationManager.AppSettings["AppVersion"].ToString();
			string _ModVersion = Util.WinReg_ReadKey("EDHM", "ModVersion").NVL(string.Empty);

			linkUIversion.Text = string.Format("UI Version <href=https://github.com/BlueMystical/EDHM_UI/releases/latest>{0}</href>", _AppVersion);
			linkModVersion.Text = string.Format("Mod Version <href=https://github.com/psychicEgg/EDHM/releases/latest>{0}</href>", _ModVersion);
		}

		private void simpleButton1_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void hyperlinkLabelControl1_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link);
			//https://www.reddit.com/user/psychicEgg/
		}

		private void hyperlinkLabelControl2_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link);
			//https://www.reddit.com/user/xiba2k4
		}

		private void hyperlinkLabelControl3_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link);
			//https://www.inara.cz/cmdr/262694/
		}

		private void hyperlinkLabelControl5_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			//readme
			System.Diagnostics.Process.Start(e.Link);
			//string _ModVersion = Util.WinReg_ReadKey("EDHM", "ModVersion").NVL(string.Empty);
			//string ED_X64_PATH = Util.WinReg_ReadKey("EDHM", "EDx64_Path").NVL(string.Empty);
			//string ReadMe_path = System.IO.Path.Combine(ED_X64_PATH, string.Format("EDHM-Readme-{0}.txt", _ModVersion));
			//if (System.IO.File.Exists(ReadMe_path))
			//{
			//    //ShaderViewer _TextViewer = new ShaderViewer(ReadMe_path);
			//    //_TextViewer.LineNumber = 19;
			//    //_TextViewer.Show();
			//}
		}

		private void hyperlinkLabelControl4_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link);
			//https://github.com/bo3b/3Dmigoto/
		}

		private void linkUIversion_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link);
		}

		private void linkModVersion_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link);
		}
	}
}