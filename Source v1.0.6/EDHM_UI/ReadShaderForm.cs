using System;
using System.Drawing;
using System.Windows.Forms;

namespace EDHM_UI
{
	public partial class ReadShaderForm : Form
	{
		public shader Shader { get; set; }

		private string _TextFile = string.Empty;
		private string[] FileLines = null;

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private string ED_X64_PATH = string.Empty;

		public ReadShaderForm(shader _Shader, string _ED_X64_PATH)
		{
			InitializeComponent();
			this.Shader = _Shader;
			this.ED_X64_PATH = _ED_X64_PATH;
		}

		private void ReadShaderForm_Load(object sender, EventArgs e)
		{
			try
			{
				if (this.Shader != null)
				{
					this.Text = this.Shader.name;
					this.txtFileName.Text = this.Shader.file;
					this.txtLineNumber.Value = this.Shader.line;
					this.txtMaxValue.Value = this.Shader.max;
					this.txtMinValue.Value = this.Shader.min;
					this.cboKind.SelectedIndex = this.Shader.kind;

					if (this.Shader.file != string.Empty)
					{
						string FilePath = System.IO.Path.Combine(this.ED_X64_PATH, this.Shader.file);
						if (System.IO.File.Exists(FilePath))
						{
							//Open the Shader File and get all its Lines:
							this._TextFile = Util.ReadTextFile(FilePath, Util.TextEncoding.UTF8);
							this.FileLines = this._TextFile.Split(new char[] { '\n' });

							if (this.FileLines != null && this.FileLines.Length > 0)
							{
								this.txtLineText.Text = this.FileLines[(int)txtLineNumber.Value - 1];
								//Now we gotta Parse the Values:
								this.txtValue.Text = GetValueToSave(this.txtLineText.Text);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private string GetValueToSave(string TextLine)
		{
			string _ret = string.Empty;
			try
			{
				if (!TextLine.EmptyOrNull())
				{
					this.Shader.code = TextLine;

					int pos_open = this.Shader.code.IndexOf('(');
					int pos_close = this.Shader.code.IndexOf(')');
					string _Val = this.Shader.code.Substring(pos_open, (pos_close - pos_open) + 1);
					_Val = _Val.Replace("(", "").Replace(")", "");

					//0 = Single Value(Decimal), 1 = Color Value, 2 = Triple Value(Linked), 3 = Quadruple Value
					switch (this.Shader.kind)
					{
						case 0:							
							this.Shader.value = Convert.ToDecimal(_Val.Replace('.',','));
							_ret = string.Format("({0})", Math.Round(this.Shader.value, 6)
								.ToString().Replace(',', '.')
								.PadRight(8, '0'));
							break;

						case 1:
							{
								string[] _RGB = _Val.Split(new char[] { ',' });
								int _R = (int)Util.NormalizeNumber(Convert.ToDecimal(_RGB[0].Replace('.', ',')), this.Shader.min, this.Shader.max, 0, 255);
								int _G = (int)Util.NormalizeNumber(Convert.ToDecimal(_RGB[1].Replace('.', ',')), this.Shader.min, this.Shader.max, 0, 255);
								int _B = (int)Util.NormalizeNumber(Convert.ToDecimal(_RGB[2].Replace('.', ',')), this.Shader.min, this.Shader.max, 0, 255);
								int _A = (int)Util.NormalizeNumber(Convert.ToDecimal(_RGB[3].Replace('.', ',')), this.Shader.min, this.Shader.max, 0, 255);

								Color _Color = Color.FromArgb(_A, _R, _G, _B);
								this.Shader.value = _Color.ToArgb();
								_ret = string.Format("({0})", _Val);
							}
							break;

						case 2:
							{
								string[] _RGBA = _Val.Split(new char[] { ',' });
								this.Shader.value = Convert.ToDecimal(_RGBA[0].Replace('.', ','));

								_ret = string.Format("({0}, {1}, {2}, {3})",
										Math.Round(this.Shader.value, 6).ToString().Replace(',', '.').PadRight(8, '0'),
										Math.Round(this.Shader.value, 6).ToString().Replace(',', '.').PadRight(8, '0'),
										Math.Round(this.Shader.value, 6).ToString().Replace(',', '.').PadRight(8, '0'),
										0); //<- No Alpha
							}
							break;

						case 3:
							{
								string[] _RGB = _Val.Split(new char[] { ',' });
								decimal _R = Util.NormalizeNumber(Convert.ToDecimal(_RGB[0].Replace('.', ',')), 0, 255, this.Shader.min, this.Shader.max);
								decimal _G = Util.NormalizeNumber(Convert.ToDecimal(_RGB[1].Replace('.', ',')), this.Shader.min, this.Shader.max, 0, 255);
								decimal _B = Util.NormalizeNumber(Convert.ToDecimal(_RGB[2].Replace('.', ',')), this.Shader.min, this.Shader.max, 0, 255);
								decimal _A = Util.NormalizeNumber(Convert.ToDecimal(_RGB[3].Replace('.', ',')), this.Shader.min, this.Shader.max, 0, 255);

								this.Shader.value = _R;

								_ret = string.Format("({0}, {1}, {2}, {3})",
										Math.Round(_R, 6).ToString().Replace(',', '.').PadRight(8, '0'),
										Math.Round(_G, 6).ToString().Replace(',', '.').PadRight(8, '0'),
										Math.Round(_B, 6).ToString().Replace(',', '.').PadRight(8, '0'),
										Math.Round(_A, 6).ToString().Replace(',', '.').PadRight(8, '0'));
							}
							break;

						default:
							break;
					}
				}

				
			}
			catch (Exception ex)
			{
				//MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private void cmdFindFile_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialog OFDialog = new OpenFileDialog()
				{
					Filter = "Text File|*.txt|All Files|*.*",
					FilterIndex = 0,
					DefaultExt = "txt",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = true
				};

				if (this.Shader.file != null && this.Shader.file != string.Empty)
				{
					string FilePath = System.IO.Path.Combine(this.ED_X64_PATH, this.Shader.file);
					string path = System.IO.Path.GetDirectoryName(FilePath); //Obtiene el Path: (Sin archivo ni extension:
					string file_name = System.IO.Path.GetFileName(FilePath); //<- Nombre del Archivo con Extension (Sin Ruta)

					OFDialog.InitialDirectory = path;
					//OFDialog.InitialDirectory = this.ED_X64_PATH;
					OFDialog.FileName = file_name;
				}
				else
				{
					OFDialog.InitialDirectory = this.ED_X64_PATH; // AppDomain.CurrentDomain.BaseDirectory;
				}

				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					string[] Folders = OFDialog.FileName.Split(new char[] { '\\' });
					string LastFolder = Folders[Folders.Length - 2];
					string file_name = System.IO.Path.GetFileName(OFDialog.FileName); //<- Nombre del Archivo con Extension (Sin Ruta)

					this.txtFileName.Tag = OFDialog.FileName;
					this.txtFileName.Text = string.Format(@"{0}\{1}", LastFolder, file_name); 
					this._TextFile = Util.ReadTextFile(OFDialog.FileName, Util.TextEncoding.UTF8);

					this.FileLines = this._TextFile.Split(new char[] { '\n' });
					if (this.FileLines != null && this.FileLines.Length > 0)
					{
						this.txtLineText.Text = this.FileLines[(int)txtLineNumber.Value - 1];

						FileViewerForm Viewer = new FileViewerForm(this.FileLines, this.Shader.line);
						Viewer.Text = this.Shader.file;
						Viewer.Show();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void cmdShowFile_Click(object sender, EventArgs e)
		{
			try
			{
				if (this._TextFile != string.Empty && this.FileLines != null)
				{
					FileViewerForm Viewer = new FileViewerForm(this.FileLines, this.Shader.line);
					Viewer.Text = this.Shader.file;
					Viewer.Show();
				}				
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (Shader != null)
			{
				this.Shader.file = this.txtFileName.Text;
				this.Shader.line = (int)this.txtLineNumber.Value;
				this.Shader.min = this.txtMinValue.Value;
				this.Shader.max = this.txtMaxValue.Value;
				this.Shader.kind = this.cboKind.SelectedIndex;
			}
			this.DialogResult = DialogResult.OK;
		}

		private void txtLineNumber_ValueChanged(object sender, EventArgs e)
		{
			if (this.FileLines != null && this.FileLines.Length > 0)
			{
				this.txtLineText.Text = this.FileLines[(int)txtLineNumber.Value - 1];
				this.txtValue.Text = GetValueToSave(this.txtLineText.Text);
			}
		}

		private void cboKind_DropDownClosed(object sender, EventArgs e)
		{
			this.Shader.kind = cboKind.SelectedIndex;
			this.txtValue.Text = GetValueToSave(this.txtLineText.Text);
		}

		
	}
}
