using System;
using System.Drawing;
using System.Windows.Forms;

namespace EDHM_UI_Thumbnail_Maker
{
	public partial class Form1 : Form
	{
		private UserRect SelectionBox = null;   //<- Caja de Seleccion sobre la Imagen
		private Image _LoadedImage = null;      //<- Imagen Cargada
		public string ThemeFolder { get; set; } //<- Carpeta donde está el Thema

		public Form1()
		{
			InitializeComponent();
		}
		public Form1(string[] args)
		{
			InitializeComponent();
			//Leer los argumentos pasados por linea de comandos:
			if (args != null && args.Length > 0)
			{
				this.ThemeFolder = args[0];
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			try
			{
				this.Text = string.Format("EDHM_UI - Thumbnail Maker [{0}]", this.ThemeFolder);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void Form1_Shown(object sender, EventArgs e)
		{
			//Esta es la Caja de Seleccion que se dibuja
			//MoveSelectionBox(0, 0, 40, 40);
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			// CTRL+V:
			if (e.KeyCode == Keys.V && (e.Control))
			{
				if (Clipboard.ContainsImage())
				{
					picImagen.SizeMode = PictureBoxSizeMode.Zoom;
					_LoadedImage = Clipboard.GetImage();
					picImagen.Image = _LoadedImage;

					MoveSelectionBox(40, this.picImagen.Height - 221, 1000, 198);
				}
				else
				{
					MessageBox.Show("Clipboard is empty. Please Copy an Image.");
				}
			}
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = string.Format("{0}x{1}", this.Width, this.Height);

			MoveSelectionBox(40, this.picImagen.Height - 221, 1000, 198);
		}


		private void MoveSelectionBox(int _left, int _Top, int _Width, int Height)
		{
			try
			{
				//Esta es la Caja de Seleccion que se dibuja
				Rectangle _REC = new Rectangle(_left, _Top, _Width, Height);

				picImagen.Image = _LoadedImage;

				//Dibuja la Caja de Seleccion
				if (this.SelectionBox is null) this.SelectionBox = new UserRect();
				this.SelectionBox.SetRectangle(_REC);
				this.SelectionBox.SetPictureBox(this.picImagen);
				this.SelectionBox.BorderSize = 1;
				this.SelectionBox.OnPositionUpdated += (object _Sender, EventArgs _E) =>
				{
					//Esto Ocurre cuando se Mueve o cambia de tamaño la Caja de Seleccion
					if (_Sender != null && this.SelectionBox != null)
					{
						_REC = (Rectangle)this.SelectionBox.rect;
						if (_REC != null)
						{
							toolStripStatusLabel1.Text = string.Format("{0}x{1}", _REC.Width, _REC.Height);
						}
					}
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void CreateAndSaveThumbnailImage()
		{
			try
			{
				if (this.SelectionBox != null && this.picImagen.Image != null)
				{
					//1. Obtiene el Area Seleccionada del Screenshot
					Rectangle _REC = this.SelectionBox.rect;

					//2. Ajusta la Imagen a la pantalla:
					Image _DisplayPIC = Util.ScaleImage(this.picImagen.Image, this.picImagen.Height);

					//3. Recorta el Area Seleccionada:
					Image _ScaledImage_A = Util.CropImage(_DisplayPIC, _REC.X, _REC.Y, _REC.Width, _REC.Height);

					//4. Ajusta la Imagen obtenida hasta 71px de alto
					Image _ScaledImage_B = Util.ScaleImage(_ScaledImage_A, 71);

					if (_ScaledImage_B != null)
					{
						//5. Muestra la Imagen Obtenida en la pantalla
						this.picImagen.Image = _ScaledImage_B;
						this.picImagen.SizeMode = PictureBoxSizeMode.CenterImage;

						//La Imagen se guarda en la Carpeta del Tema:
						SaveFileDialog SFDialog = new SaveFileDialog()
						{
							Filter = "Image JPG|*.jpg",
							FilterIndex = 0,
							DefaultExt = "jpg",
							AddExtension = true,
							CheckPathExists = true,
							OverwritePrompt = true,
							FileName = "PREVIEW",
							InitialDirectory = !string.IsNullOrEmpty(this.ThemeFolder) ? ThemeFolder : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
						};
						if (SFDialog.ShowDialog() == DialogResult.OK)
						{
							//6. Guarda la Imagen en JPG al 100% de Calidad:
							Util.SaveJpegWithCompression(_ScaledImage_B, SFDialog.FileName, 100);
						}
					}
				}				
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void pasteImageFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Clipboard.ContainsImage())
			{
				picImagen.SizeMode = PictureBoxSizeMode.Zoom;
				picImagen.Image = Clipboard.GetImage();

				MoveSelectionBox(40, this.picImagen.Height - 221, 1000, 198);
			}
			else
			{
				MessageBox.Show("Clipboard is empty. Please Copy an Image.");
			}
		}

		private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
		{
			MoveSelectionBox(90, this.picImagen.Height - 221, 1000, 198);
		}

		private void toolStripDropDownButton1_Click(object sender, EventArgs e)
		{
			CreateAndSaveThumbnailImage();
		}

		private void openImageFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog OFDialog = new OpenFileDialog()
			{
				Filter = "Image Files|*.jpg;*.png;*.bmp",
				FilterIndex = 0,
				DefaultExt = "jpg",
				AddExtension = true,
				CheckPathExists = true,
				CheckFileExists = true,
				InitialDirectory = (this.ThemeFolder != null && ThemeFolder != string.Empty) ? ThemeFolder : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};
			if (OFDialog.ShowDialog() == DialogResult.OK)
			{
				picImagen.SizeMode = PictureBoxSizeMode.Zoom;
				this._LoadedImage = Image.FromFile(OFDialog.FileName);
				picImagen.Image = this._LoadedImage;

				MoveSelectionBox(40, this.picImagen.Height - 221, 1000, 198);
			}
		}

		private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CreateAndSaveThumbnailImage();
		}
	}
}
