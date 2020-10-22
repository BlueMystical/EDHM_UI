using System;
using System.Drawing;
using System.Windows.Forms;

namespace EDHM_UI
{
	public partial class ShaderControl : UserControl
	{
		#region Private Declarations

		private bool Loaded = false;
		private string ED_X64_PATH = string.Empty;

		public delegate void ValueChangedHandler(object sender, EventArgs e);
		public event ValueChangedHandler ValueChanged;

		#endregion

		#region Public Properties

		public shader Shader { get; set; }
		public bool AutoSave { get; set; }
		public bool IsDirty { get; set; }
		public bool IsSelected { get; set; }

		#endregion

		#region Constructores

		public ShaderControl() { InitializeComponent(); }
		public ShaderControl(shader pShader, string _ED_X64_PATH)
		{
			InitializeComponent();
			this.Shader = pShader;
			this.ED_X64_PATH = _ED_X64_PATH;
		}

		private void ShaderControl_Load(object sender, EventArgs e)
		{
			LoadShader(this.Shader);
		}

		#endregion

		#region Metodos

		public void LoadShader(shader pShader)
		{
			try
			{
				this.Shader = pShader;
				if (this.Shader != null)
				{
					this.txtName.Text = this.Shader.name;
					int _Valor = 0;

					//this.Shader.kind = 0:Slider, 1:ColorPick; 2:Opacity
					switch (this.Shader.kind)
					{
						case 0:
							/* NUMERIC (DECIMAL) SLIDER */
							this.Slider.Visible = true;
							this.Slider.Minimum = 0;
							this.Slider.Maximum = 20;

							_Valor = Convert.ToInt32(Util.NormalizeNumber(this.Shader.value, this.Shader.min, this.Shader.max, 0, 20));
							if (_Valor > this.Slider.Maximum || _Valor < this.Slider.Minimum)
							{
								this.Slider.Value = 0;
							}
							else
							{
								this.Slider.Value = _Valor;
							}

							this.txtValue.DecimalPlaces = 6;
							this.txtValue.Minimum = this.Shader.min;
							this.txtValue.Maximum = this.Shader.max;
							this.txtValue.Value = this.Shader.value;
							this.txtValue.Increment = 0.000001M;

							this.cmdColorChange.Visible = false;
							this.lblColorARGB.Text = GetValueToSave();
							break;

						case 1:
							/* A COLOR VALUE */
							//mul r4.xyz, r4.xyzx, l(0.800000, 0.800000, 0.800000, 0.000000)
							this.Slider.Visible = false;

							Color _BackColor = Color.FromArgb(Convert.ToInt32(this.Shader.value));
							this.panelColor.BackColor = _BackColor;
							this.cmdColorChange.Visible = true;

							this.txtValue.DecimalPlaces = 0;
							this.txtValue.Minimum = Int32.MinValue;
							this.txtValue.Maximum = Int32.MaxValue;
							this.txtValue.Increment = 1;
							this.txtValue.Value = this.Shader.value;

							this.lblColorARGB.Text = GetValueToSave();
							break;

						case 2:
							/* NUMERIC (DECIMAL) 3 values simultaneous*/
							this.Slider.Visible = true;
							this.Slider.Minimum = 0;
							this.Slider.Maximum = 20;

							_Valor = Convert.ToInt32(Util.NormalizeNumber(this.Shader.value, this.Shader.min, this.Shader.max, 0, 20));
							if (_Valor > this.Slider.Maximum || _Valor < this.Slider.Minimum)
							{
								this.Slider.Value = 0;
							}
							else
							{
								this.Slider.Value = _Valor;
							}

							this.txtValue.DecimalPlaces = 6;
							this.txtValue.Minimum = this.Shader.min;
							this.txtValue.Maximum = this.Shader.max;
							this.txtValue.Value = this.Shader.value;
							this.txtValue.Increment = 0.000001M;

							this.cmdColorChange.Visible = false;

							this.lblColorARGB.Text = GetValueToSave();
							break;

						default:
							break;
					}
					Loaded = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public bool SaveShader()
		{
			/* THIS SAVES DATA DIRECTLY INTO THE SHADER FILE  */
			bool _ret = false;
			try
			{
				if (this.Shader != null)
				{
					if (this.Shader.file != string.Empty)
					{
						//0. File must be there beforehand:
						string FilePath = System.IO.Path.Combine(this.ED_X64_PATH, this.Shader.file);
						if (System.IO.File.Exists(FilePath))
						{
							//1. Read the whole file:
							string _TextFile = Util.ReadTextFile(FilePath, Util.TextEncoding.UTF8);
							
							//2. Split its Lines:
							string[] FileLines = _TextFile.Split(new string[] { "\r\n" }, StringSplitOptions.None);

							if (FileLines != null && FileLines.Length > 0)
							{
								//3. Modify the Line in Memory, replacing only the Value Part:
								string[] Partes = this.Shader.code.Split(new char[] { '(' });
								if (Partes != null && Partes.Length > 0)
								{
									this.Shader.code = string.Format("{0}{1}", Partes[0], GetValueToSave()).TrimEnd();
								}

								//4. Find the Line to be Written and Replace it whit the new one:
								FileLines[(int)this.Shader.line - 1] = this.Shader.code;

								//5. Re-Assembly the Lines into one single String:
								System.Text.StringBuilder _Writer = new System.Text.StringBuilder();
								foreach (string _Linea in FileLines) { if (_Linea != string.Empty) _Writer.AppendLine(_Linea); }

								//6. Write the String into a File:
								if (Util.SaveTextFile(FilePath, _Writer.ToString(), Util.TextEncoding.UTF8))
								{
									_ret = true;
									IsDirty = false;
									cmdSaveShader.Visible = false;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		protected void OnValueChanged(object sender, EventArgs e)
		{
			ValueChangedHandler handler = ValueChanged;
			if (handler != null)
			{
				if (this.IsDirty) cmdSaveShader.Visible = true;
				handler(this.Shader, e);
			}
		}

		private string GetValueToSave()
		{
			string _ret = string.Empty;
			try
			{
				switch (this.Shader.kind)
				{
					case 0:
						_ret = string.Format("({0})", Math.Round(this.Shader.value, 6)
							.ToString().Replace(',', '.')
							.PadRight(8, '0'));
						break;

					case 1:
						Color _BackColor = Color.FromArgb(Convert.ToInt32(this.Shader.value));
						_ret = Util.Color_To_RGBA_Normalized(_BackColor, this.Shader.min, this.Shader.max);
						break;

					case 2:
						_ret = string.Format("({0}, {1}, {2}, {3})",
								Math.Round(this.Shader.value, 6).ToString().Replace(',', '.').PadRight(8, '0'),
								Math.Round(this.Shader.value, 6).ToString().Replace(',', '.').PadRight(8, '0'),
								Math.Round(this.Shader.value, 6).ToString().Replace(',', '.').PadRight(8, '0'),
								0); //<- No Alpha
						break;

					default:
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		#endregion

		#region Eventos de Controles

		private void Slider_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				if (Loaded)
				{
					this.Shader.value = Util.NormalizeNumber(this.Slider.Value, 0, 20, this.Shader.min, this.Shader.max);
					this.txtValue.Value = this.Shader.value;
					IsDirty = true;

					this.lblColorARGB.Text = GetValueToSave();
					OnValueChanged(this.Shader, null); //<- Evento: She ha cambiado algo.
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void txtValue_ValueChanged(object sender, EventArgs e)
		{
			if (Loaded)
			{
				Loaded = false;
				this.Shader.value = this.txtValue.Value;
				this.lblColorARGB.Text = GetValueToSave();

				if (this.Shader.kind == 1)
				{
					this.panelColor.BackColor = Color.FromArgb( (int)this.Shader.value);
				}

				Loaded = true;
				IsDirty = true;

				OnValueChanged(this.Shader, null); //<- Evento: She ha cambiado algo.
			}
		}

		private void txtName_TextChanged(object sender, EventArgs e)
		{
			this.Shader.name = txtName.Text;
		}
		private void cmdDetails_Click(object sender, EventArgs e)
		{
			ReadShaderForm Dialog = new ReadShaderForm(this.Shader, this.ED_X64_PATH);
			if (Dialog.ShowDialog() == DialogResult.OK)
			{
				LoadShader(Dialog.Shader);
			}
		}

		private void cmdColorChange_Click(object sender, EventArgs e)
		{
			try
			{
				if (this.Shader.kind == 1)
				{
					Color _C = Color.FromArgb(Convert.ToInt32(this.Shader.value));
					ColorSlidersForm Dialog = new ColorSlidersForm(_C, this.Shader.min, this.Shader.max);
					Dialog.Title = this.Shader.name;
					if (Dialog.ShowDialog() == DialogResult.OK)
					{
						this.Shader.value = Dialog.Color.ToArgb();
						IsDirty = true;

						this.txtValue.Value = this.Shader.value;
						this.panelColor.BackColor = Dialog.Color;
						this.lblColorARGB.Text = Dialog.RGBA;

						OnValueChanged(this.Shader, null); //<- Evento: She ha cambiado algo.
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void panelColor_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				if (this.Shader.kind == 1)
				{
					Color _C = Color.FromArgb(Convert.ToInt32(this.Shader.value));
					ColorSlidersForm Dialog = new ColorSlidersForm(_C, this.Shader.min, this.Shader.max);
					Dialog.Title = this.Shader.name;
					if (Dialog.ShowDialog() == DialogResult.OK)
					{
						this.Shader.value = Dialog.Color.ToArgb();
						IsDirty = true;

						this.txtValue.Value = this.Shader.value;
						this.panelColor.BackColor = Dialog.Color;
						this.lblColorARGB.Text = Dialog.RGBA;

						OnValueChanged(this.Shader, null); //<- Evento: She ha cambiado algo.
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void checkSelected_CheckedChanged(object sender, EventArgs e)
		{
			this.Shader.is_selected = checkSelected.Checked;
		}

		private void cmdSaveShader_Click(object sender, EventArgs e)
		{
			if(SaveShader())
			{				
				MessageBox.Show("Shader Saved into File.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		#endregion
	}
}