using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace EDHM_UI_mk2.Forms
{
	public partial class ThemeParametersForm : DevExpress.XtraEditors.XtraForm
	{
		public ThemeParametersForm()
		{
			InitializeComponent();
		}

		public string ModName { get; set; }
		public string ThemeName { get; set; }
		public string Author { get; set; }
		public string Description { get; set; }
		public Image Thumbnail { get; set; }

		public bool ThisIsAMod { get; set; } = false;


		private void ThemeParametersForm_Load(object sender, EventArgs e)
		{

		}
		private void ThemeParametersForm_Shown(object sender, EventArgs e)
		{
			//txtModName.Visible = this.ThisIsAMod; <href=https://inara.cz/elite/cmdr-ranks/262694/>Blue Mystic</href>

			if (this.ModName != null && this.ModName != string.Empty)
			{
				this.txtModName.EditValue = this.ModName;
			}
			if (this.ThemeName != null && this.ThemeName != string.Empty)
			{
				this.txtName.EditValue = this.ThemeName;
			}
			if (this.Author != null && this.Author != string.Empty)
			{
				this.txtAuthor.EditValue = this.Author;
			}
			if (this.ThisIsAMod)
			{
				this.txtAuthor.Properties.MaskSettings.Configure<DevExpress.XtraEditors.Mask.MaskSettings.RegExp>(settings =>
				{
					settings.MaskExpression = "[0-9a-zA-Z ()-]+";
				});
			}

			if (this.Description != null && this.Description != string.Empty)
			{
				this.txtDescription.EditValue = this.Description;
			}
			if (this.Thumbnail != null)
			{
				this.picThumb.Image = this.Thumbnail;
			}
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			bool Continuar = true;
			if (this.txtAuthor.EditValue == null || this.txtAuthor.EditValue.ToString().Trim() == string.Empty)
			{
				this.dxErrorProvider1.SetError(this.txtAuthor, "Required Field!"); Continuar = false;
			}
			if (this.txtName.EditValue == null || this.txtName.EditValue.ToString().Trim() == string.Empty)
			{
				this.dxErrorProvider1.SetError(this.txtName, "Required Field!"); Continuar = false;
			}

			if (Continuar)
			{
				this.dxErrorProvider1.SetError(this.txtAuthor, string.Empty);
				this.dxErrorProvider1.SetError(this.txtName, string.Empty);

				this.ModName = this.txtModName.EditValue.ToString().Trim();
				this.ThemeName = this.txtName.EditValue.ToString().Trim();
				this.Author = this.txtAuthor.EditValue.ToString().Trim();
				this.Description = Util.NVL(this.txtDescription.EditValue, "").Trim();
				this.Thumbnail = this.picThumb.Image;

				this.DialogResult = DialogResult.OK;
			}
			
		}

		private void defaultToolTipController1_DefaultController_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			Process process = new Process();
			process.StartInfo.FileName = (e.Link);
			process.StartInfo.Verb = "open";
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			try
			{
				process.Start();
			}
			catch { }
		}
	}
}