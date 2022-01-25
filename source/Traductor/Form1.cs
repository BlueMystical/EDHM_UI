using System;
using System.Text;
using System.Windows.Forms;
using EDHM_UI_mk2;

namespace Traductor
{
	public partial class Form1 : Form
	{
		private ui_setting Settings = new ui_setting();

		private string _path = string.Empty;

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog OFDialog = new OpenFileDialog()
			{
				Filter = "JSON Data|*.json",
				FilterIndex = 0,
				DefaultExt = "json",
				AddExtension = true,
				CheckPathExists = true,
				CheckFileExists = true,
				InitialDirectory = @"D:\Soft TEST\ED UI mk2\EDHM_UI_mk2\Data"
			};

			if (OFDialog.ShowDialog() == DialogResult.OK)
			{
				this.Settings = Util.DeSerialize_FromJSON<ui_setting>(OFDialog.FileName);
				if (this.Settings != null)
				{
					int i = 0;
					this._path = OFDialog.FileName;
					StringBuilder _Texto = new StringBuilder();
					foreach (var _Grupo in this.Settings.ui_groups)
					{
						_Texto.AppendLine(_Grupo.Title); i++;
						foreach (var _Elemento in _Grupo.Elements)
						{
							_Texto.AppendLine(string.Format("{0}|{1}|{2}", _Elemento.Category, _Elemento.Title, 
								_Elemento.Description.Replace("\r\n", "<p>")));
							i++;
						}
					}
					this.textBox1.Text = _Texto.ToString() + "|" + i;
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (this.textBox1.Text != string.Empty)
			{
				string[] Lineas = this.textBox1.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				if (Lineas != null && Lineas.Length > 0 && this.Settings != null)
				{
					int i = 0;
					foreach (var _Grupo in this.Settings.ui_groups)
					{
						string[] Palabras;
						_Grupo.Title = Lineas[i]; i++;
						foreach (var _Elemento in _Grupo.Elements)
						{
							Palabras = Lineas[i].Split(new char[] { '|' });

							_Elemento.Category = Palabras[0].Trim(); 
							_Elemento.Title = Palabras[1].Trim(); 
							_Elemento.Description = Palabras[2].Replace("<p>","\r\n").Trim();

							i++;
						}
					}

					SaveFileDialog SFDialog = new SaveFileDialog()
					{
						Filter = "JSON Data|*.json",
						FilterIndex = 0,
						DefaultExt = "json",
						AddExtension = true,
						CheckPathExists = true,
						OverwritePrompt = true,
						FileName = this._path,
						InitialDirectory = System.IO.Path.GetDirectoryName(this._path)
					};
					if (SFDialog.ShowDialog() == DialogResult.OK)
					{
						Util.Serialize_ToJSON(SFDialog.FileName, this.Settings);
						MessageBox.Show("Saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
		}
	}
}
