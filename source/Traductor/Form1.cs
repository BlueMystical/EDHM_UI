using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using EDHM_UI_mk2;

namespace Traductor
{
	public partial class Form1 : Form
	{
		private ui_setting Settings = new ui_setting();
		private List<key_value_ex> _ControlsJSON = null;

		private string _path = string.Empty;

		public Form1()
		{
			InitializeComponent();
		}


		private void mnuLoadSourceFile_Click(object sender, EventArgs e)
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
						if (_Grupo.Elements.IsNotEmpty())
						{
							foreach (var _Elemento in _Grupo.Elements)
							{
								_Texto.AppendLine(string.Format("{0}|{1}|{2}", _Elemento.Category, _Elemento.Title,
									_Elemento.Description.Replace("\r\n", "<p>")));
								i++;
							}
						}
					}
					this.textBox1.Text = _Texto.ToString() + "|" + i;
				}
			}
		}
		private void mnuSaveDestinationFile_Click(object sender, EventArgs e)
		{
			if (this.textBox1.Text != string.Empty)
			{
				string _Lang = "en";
				Util.ShowInputDialog("Language?", ref _Lang);
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
					List<ui_translation> _Translations = Util.DeSerialize_FromJSON<List<ui_translation>>(SFDialog.FileName);

					string[] Lineas = this.textBox1.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
					if (Lineas != null && Lineas.Length > 0 && this.Settings != null)
					{
						int i = 0;
						foreach (var _Grupo in this.Settings.ui_groups)
						{
							string[] Palabras;
							_Grupo.Title = Lineas[i]; i++;

							var _GRP = _Translations.Find(x => x.group == "TileGroup" && x.id == _Grupo.Name);
							if (_GRP != null)
							{
								var _EN = _GRP.lang.Find(x => x.key == _Lang);
								_EN.value = _Grupo.Title;
							}

							if (_Grupo.Elements.IsNotEmpty())
							{
								foreach (var _Elemento in _Grupo.Elements)
								{
									Palabras = Lineas[i].Split(new char[] { '|' });

									_Elemento.Category = Palabras[0].Trim();
									_Elemento.Title = Palabras[1].Trim();
									_Elemento.Description = Palabras[2].Replace("<p>", "\r\n").Trim();

									i++;

									var _ELE = _Translations.Find(x => x.group == "Element" && x.id == _Elemento.Key);
									if (_ELE != null)
									{
										var _EN = _ELE.lang.Find(x => x.key == _Lang);
										if (_EN != null)
										{
											_EN.value = _Elemento.Title;
											_EN.category =  _Elemento.Category;
											_EN.description = _Elemento.Description;
										}
										else
										{
											if (_ELE.lang is null) _ELE.lang = new List<language>();
											_ELE.lang.Add(new language
											{
												key = _Lang,
												value = _Elemento.Title,
												category = _Elemento.Category,
												description = _Elemento.Description
											});
										}
									}
									else
									{
										ui_translation _TR = new ui_translation
										{
											id = _Elemento.Key,
											group = "Element",
											lang = new List<language>()
										};
										_TR.lang.Add(new language
										{
											key = _Lang,
											value = _Elemento.Title,
											category = _Elemento.Category,
											description = _Elemento.Description
										});
										_Translations.Add(_TR);
									}
								}
							}
						}
					}

					//Util.Serialize_ToJSON(SFDialog.FileName, this.Settings);
					Util.Serialize_ToJSON(SFDialog.FileName, _Translations);

					MessageBox.Show("Saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}

				
			}
		}

		private void mnuMenus_LoadFile_Click(object sender, EventArgs e)
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
				this._ControlsJSON = Util.DeSerialize_FromJSON<List<key_value_ex>>(OFDialog.FileName);
				if (this._ControlsJSON.IsNotEmpty())
				{
					StringBuilder _Texto = new StringBuilder();
					foreach (var _Menu in this._ControlsJSON)
					{
						_Texto.AppendLine(string.Format("{0}|{1}",_Menu.value, _Menu.extra.NVL("")));
					}
					this.textBox1.Text = _Texto.ToString();
				}
			}
		}
		private void mnuMenus_SaveFile_Click(object sender, EventArgs e)
		{
			if (this.textBox1.Text != string.Empty)
			{
				string[] Lineas = this.textBox1.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				if (Lineas != null && Lineas.Length > 0 && this.Settings != null)
				{
					int i = 0;
					foreach (var _Menu in this._ControlsJSON)
					{
						string[] Palabras = Lineas[i].Split(new char[] { '|' });

						_Menu.value = Palabras[0];
						_Menu.extra = Palabras[1];
						i++;
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
						InitialDirectory = @"D:\Soft TEST\ED UI mk2\EDHM_UI_mk2\Data"
					};
					if (SFDialog.ShowDialog() == DialogResult.OK)
					{
						Util.Serialize_ToJSON(SFDialog.FileName, this._ControlsJSON);
						MessageBox.Show("Saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					
				}
			}
		}
	}
}
