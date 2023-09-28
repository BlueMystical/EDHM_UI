using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
		public string PreviewURL { get; set; }
		public Image Thumbnail { get; set; }
		public string ThemeFolder { get; set; } 

		public bool ThisIsAMod { get; set; } = false;
		public bool ThumbnailAdded = false;


		private void ThemeParametersForm_Load(object sender, EventArgs e)
		{

		}
		private void ThemeParametersForm_Shown(object sender, EventArgs e)
		{
			//txtModName.Visible = this.ThisIsAMod; <href='https://inara.cz/elite/cmdr-ranks/262694/>Blue Mystic'</href>

			txtModName.EditValue = ModName.NVL(string.Empty);
			txtName.EditValue = ThemeName.NVL(string.Empty);
			txtAuthor.EditValue = Author.NVL(string.Empty);
			txtDescription.EditValue = Description.NVL(string.Empty);
			txtPreviewURL.EditValue = PreviewURL.NVL(string.Empty);

			if (ThisIsAMod)
			{
				txtAuthor.Properties.MaskSettings.Configure<DevExpress.XtraEditors.Mask.MaskSettings.RegExp>(settings =>
				{
					settings.MaskExpression = "[0-9a-zA-Z ()-]+";
				});
			}

			if (Thumbnail != null)
			{
				picThumb.Image = Thumbnail;
			}
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			bool Continuar = true;
			if (txtAuthor.EditValue == null || txtAuthor.EditValue.ToString().Trim() == string.Empty)
			{
				dxErrorProvider1.SetError(txtAuthor, "Required Field!"); Continuar = false;
			}
			if (txtName.EditValue == null || txtName.EditValue.ToString().Trim() == string.Empty)
			{
				dxErrorProvider1.SetError(txtName, "Required Field!"); Continuar = false;
			}
			/*
			if (!ThumbnailAdded)
			{
				dxErrorProvider1.SetError(picThumb, "Required Field!\r\nDouble Click on it to add an Screenshot!"); // Continuar = false;
				if (Mensajero.ShowMessage("Thumbnail Missing",
					"The Thumbnail Image is Required to publish your Theme!\r\nDoubleClick the Image to set it.\r\nDo you want to Continue without it?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, true) == DialogResult.No)
				{
					Continuar = false;
				}
			}*/

			if (Continuar)
			{
				dxErrorProvider1.SetError(txtAuthor, string.Empty);
				dxErrorProvider1.SetError(txtName, string.Empty);

				ModName = txtModName.EditValue.ToString().Trim();
				ThemeName = txtName.EditValue.ToString().Trim();
				Author = txtAuthor.EditValue.ToString().Trim();
				Description = Util.NVL(txtDescription.EditValue, "").Trim();
				PreviewURL = Util.NVL(txtPreviewURL.EditValue, "").Trim();

				ThemeFolder = ThemeFolder.NVL(@"C:\Temp");

				//Thumbnail = ThumbnailAdded ? Image.FromFile(@"C:\Temp\PREVIEW.jpg")  : picThumb.Image; // Util.GetElementImage(@"C:\Temp\PREVIEW.jpg")

				if (File.Exists(Path.Combine(ThemeFolder, "PREVIEW.jpg")))
				{
					//Carga la Imagen sin dejara 'en uso':
					using (Stream stream = File.OpenRead(Path.Combine(ThemeFolder, "PREVIEW.jpg")))
					{
						Thumbnail = System.Drawing.Image.FromStream(stream);
					}
				}

				DialogResult = DialogResult.OK;
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

		private void picThumb_DoubleClick(object sender, EventArgs e)
		{
			string ThumbPath = !string.IsNullOrEmpty(ThemeFolder) ? ThemeFolder : @"C:\Temp";
			
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = "EDHM_UI_Thumbnail_Maker.exe";
			process.StartInfo.Arguments = "/AC /P:" + "\"" + ThumbPath + "\""; //<- /AC[Autoclose] /P:"[ImageSavingDirectory]"
			process.StartInfo.ErrorDialog = true;
			process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
			process.EnableRaisingEvents = true;
			process.Exited += (Sender, eventArgs) =>
			{
				if (File.Exists(Path.Combine(ThumbPath, "PREVIEW.jpg")))
				{
					//Invoke necesario xq esto ocurre en otro porceso:
					Invoke((MethodInvoker)(() =>
					{
						if (File.Exists(Path.Combine(ThumbPath, "PREVIEW.jpg")))
						{
							//Carga la Imagen sin dejara 'en uso':
							using (Stream stream = File.OpenRead(Path.Combine(ThumbPath, "PREVIEW.jpg")))
							{
								picThumb.Image = System.Drawing.Image.FromStream(stream);
							}
						}
						
						dxErrorProvider1.SetError(picThumb, "");
						ThumbnailAdded = true;
					}));
				}
			};
			process.Start();
			//process.WaitForExit(1000 * 60 * 5);    // Wait up to five minutes.

		}
	}
}