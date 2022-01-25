using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EDHM_UI_mk2;

namespace EDHM_DX
{
	public partial class KeyBindingSimpleForm : DevExpress.XtraEditors.XtraForm
	{
		public string ProfileName { get; set; }
		public string KeyBinding { get; set; }

		private List<Codiguera> ModifKeys = null;
		private List<Codiguera> AvailableKeys = null;

		public KeyBindingSimpleForm(string _ProfileName)
		{
			InitializeComponent();
			this.ProfileName = _ProfileName;
		}

		private void KeyBindingSimpleForm_Load(object sender, EventArgs e)
		{
			int Index = 0;
			this.AvailableKeys = new List<Codiguera>();
			for (Index = 1; Index <= 12; Index++)
			{
				AvailableKeys.Add(new Codiguera(Index, string.Format("F{0}", Index)));
			}
			for (int i = 0; i <= 9; i++)
			{
				AvailableKeys.Add(new Codiguera(12 + i, string.Format("{0}", i)));
			}
			char[] alphabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
			foreach (var _Alpha in alphabeto)
			{
				AvailableKeys.Add(new Codiguera(Index, _Alpha.ToString()));
			}
			this.cboKeys.Properties.DisplayMember = "description";
			this.cboKeys.Properties.ValueMember = "description";
			this.cboKeys.Properties.DataSource = AvailableKeys;

			this.ModifKeys = new List<Codiguera>();
			ModifKeys.Add(new Codiguera(1, "CTRL"));
			ModifKeys.Add(new Codiguera(2, "ALT"));
			ModifKeys.Add(new Codiguera(3, "SHIFT"));
			ModifKeys.Add(new Codiguera(4, "SPACE"));
			ModifKeys.Add(new Codiguera(5, "CTRL"));

			this.cboModifKeys.Properties.DisplayMember = "description";
			this.cboModifKeys.Properties.ValueMember = "description";
			this.cboModifKeys.Properties.DataSource = ModifKeys;
		}
		private void KeyBindingSimpleForm_Shown(object sender, EventArgs e)
		{
			this.txtProfileName.EditValue = this.ProfileName;
		}


		private void cmdAceptar_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			bool Continuar = true;

			if (this.cboModifKeys.EditValue == null)
			{
				dxErrorProvider1.SetError(this.cboModifKeys, "You must Select a Key!");
				Continuar = false;
			}
			else
			{
				dxErrorProvider1.SetError(this.cboModifKeys, string.Empty);
			}
			if (this.cboKeys.EditValue == null)
			{
				dxErrorProvider1.SetError(this.cboKeys, "You must Select a Key!");
				Continuar = false;
			}
			else
			{
				dxErrorProvider1.SetError(this.cboKeys, string.Empty);
			}

			if (Continuar)
			{
				this.KeyBinding = string.Format("{0} {1}", this.cboModifKeys.EditValue, this.cboKeys.EditValue);
				this.DialogResult = DialogResult.OK;
			}
		}

		private void cmdCancelar_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}