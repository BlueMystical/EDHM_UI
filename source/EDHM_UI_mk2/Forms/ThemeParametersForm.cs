using System;
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


		private void ThemeParametersForm_Load(object sender, EventArgs e)
		{

		}
		private void ThemeParametersForm_Shown(object sender, EventArgs e)
		{
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
			this.ModName = this.txtModName.EditValue.ToString();
			this.ThemeName = this.txtName.EditValue.ToString();
			this.Author = this.txtAuthor.EditValue.ToString();
			this.Description = this.txtDescription.EditValue.ToString();
			this.Thumbnail = this.picThumb.Image;

			this.DialogResult = DialogResult.OK;
		}
	}
}