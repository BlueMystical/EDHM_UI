using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace EDHM_UI_mk2.Forms
{
	public partial class XML_Form_Odyssey : DevExpress.XtraEditors.XtraForm
	{
		/// <summary>Devuelve la Matrix de Color Elejida.</summary>
		public float[][] ColorMatrix { get; set; }

		private Image StationOriginalImage = null;
		private Image StationPortraitImage = null;

		private Image ShipPanelOriginalImage = null;
		private Image ShipPanelPortraitImage = null;

		private Image PortraitMainImage = null;

		private IniFile _Reader = null;
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;

		public game_instance ActiveInstance { get; set; }
		public List<value_key> xml_profile { get; set; }

		public XML_Form_Odyssey(game_instance _ActiveInstance, List<value_key> _Xml_profile)
		{
			InitializeComponent();
			//this.LookAndFeel.TouchUIMode = DevExpress.Utils.DefaultBoolean.False;
			//this.LookAndFeel.TouchScaleFactor = 1;
			this.ActiveInstance = _ActiveInstance;
			this.xml_profile = _Xml_profile;
		}

		private void XML_Form_Odyssey_Load(object sender, EventArgs e)
		{
			//Obligar a usar los puntos y las comas;
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			customCulture.NumberFormat.NumberGroupSeparator = ",";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
			customCulture.NumberFormat.CurrencyGroupSeparator = ",";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			string ImagesPath = Path.Combine(this.AppExePath, "Images", this.ActiveInstance.key);

			this.StationOriginalImage = Image.FromFile(Path.Combine(ImagesPath, @"ColorMatrix_Preview_1a.png"));
			this.StationPortraitImage = Image.FromFile(Path.Combine(ImagesPath, @"ColorMatrix_Preview_1b.png"));

			this.ShipPanelOriginalImage = Image.FromFile(Path.Combine(ImagesPath, @"ColorMatrix_Preview_2a.png"));
			this.ShipPanelPortraitImage = Image.FromFile(Path.Combine(ImagesPath, @"ColorMatrix_Preview_2b.png"));

			//this.PortraitMainImage = Image.FromFile(Path.Combine(ImagesPath, @"ColorMatrix_Preview_3a.png"));

			this.picStationPanels.Image = this.StationOriginalImage;
		}
		private void XML_Form_Odyssey_Shown(object sender, EventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

				LoadINIProfile();

				DrawPicture_SHIP();
				DrawPicture_STATION();
			});
		}


		private void LoadINIProfile()
		{
			try
			{
				//Cuando se Carga un Tema, el XML se copia Localmente, usamos ese archivo 
				//this._Reader = new IniFile(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_XML-Profile.ini"));
				//this._Reader = new IniFile(Path.Combine(this.ActiveTheme.folder, "XML-Profile.ini"));

				if (true)
				{
					StringBuilder _XML_Matrix = new StringBuilder();

					//** CONFIGS:
					Invoke((MethodInvoker)(() =>
					{
						//this.cboPortrasitModel.EditValue = Convert.ToDecimal(this._Reader.ReadKey("x124", "constants"));
						//this.trackPortraitBrightness.Value = Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("w153", "constants")) * 100);
						//this.trackShipPanelsBrightness.Value = Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("z157", "constants")) * 100);
						//this.trackHiglightsBrightness.Value = Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("w157", "constants")) * 100);
					}));
					
					//-----------------------------------------------------------------------------------;
					//** --== XML Ship Profile ==--

					decimal bRR, bRG, bRB = 0;
					decimal bGR, bGG, bGB = 0;
					decimal bBR, bBG, bBB = 0;

					bRR = this.xml_profile.Find(X => X.key == "x150").value;  // Convert.ToDecimal(this._Reader.ReadKey("x150", "constants"));
					bRG = this.xml_profile.Find(X => X.key == "y150").value;  // Convert.ToDecimal(this._Reader.ReadKey("y150", "constants"));
					bRB = this.xml_profile.Find(X => X.key == "z150").value;  // Convert.ToDecimal(this._Reader.ReadKey("z150", "constants"));

					bGR = this.xml_profile.Find(X => X.key == "x151").value;  // Convert.ToDecimal(this._Reader.ReadKey("x151", "constants"));
					bGG = this.xml_profile.Find(X => X.key == "y151").value;  // Convert.ToDecimal(this._Reader.ReadKey("y151", "constants"));
					bGB = this.xml_profile.Find(X => X.key == "z151").value;  // Convert.ToDecimal(this._Reader.ReadKey("z151", "constants"));

					bBR = this.xml_profile.Find(X => X.key == "x152").value;  // Convert.ToDecimal(this._Reader.ReadKey("x152", "constants"));
					bBG = this.xml_profile.Find(X => X.key == "y152").value;  // Convert.ToDecimal(this._Reader.ReadKey("y152", "constants"));
					bBB = this.xml_profile.Find(X => X.key == "z152").value;  // Convert.ToDecimal(this._Reader.ReadKey("z152", "constants"));

					_XML_Matrix.Length = 0;
					_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", bRR, bRG, bRB));
					_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", bGR, bGG, bGB));
					_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", bBR, bBG, bBB));

					Invoke((MethodInvoker)(() =>
					{
						this.txtXML_LOW.Text = _XML_Matrix.ToString();

						this.sRR_B.Value = Convert.ToInt32(bRR * 100);
						this.sRG_B.Value = Convert.ToInt32(bRG * 100);
						this.sRB_B.Value = Convert.ToInt32(bRB * 100);

						this.sGR_B.Value = Convert.ToInt32(bGR * 100);
						this.sGG_B.Value = Convert.ToInt32(bGG * 100);
						this.sGB_B.Value = Convert.ToInt32(bGB * 100);

						this.sBR_B.Value = Convert.ToInt32(bBR * 100);
						this.sBG_B.Value = Convert.ToInt32(bBG * 100);
						this.sBB_B.Value = Convert.ToInt32(bBB * 100);
					}));
				}
			}
			catch (Exception ex)
			{
				Invoke((MethodInvoker)(() =>
				{
					XtraMessageBox.Show(ex.Message);
				}));
			}
		}

		private void DrawPicture_SHIP()
		{
			try
			{
				Invoke((MethodInvoker)(() =>
				{
					this.picShipPanels.Image = null;
					this.picShipPanels.Refresh();
				}));

				Image image = (Image)this.ShipPanelOriginalImage.Clone();

				ImageAttributes imageAttributes = new ImageAttributes();
				int width = image.Width;
				int height = image.Height;

				float[][] colorMatrixElements = {
				   new float[] { (float)this.sRR_B.Value / 100, (float)this.sRG_B.Value / 100, (float)this.sRB_B.Value / 100,  0, 0},        // red scaling factor of 2
				   new float[] { (float)this.sGR_B.Value / 100, (float)this.sGG_B.Value / 100, (float)this.sGB_B.Value / 100,  0, 0},        // green scaling factor of 1
				   new float[] { (float)this.sBR_B.Value / 100, (float)this.sBG_B.Value / 100, (float)this.sBB_B.Value / 100,  0, 0},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 1f}			// three translations of 0.2
				};
				this.ColorMatrix = colorMatrixElements;

				ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
				imageAttributes.SetColorMatrix(
				   colorMatrix,
				   ColorMatrixFlag.Default,
				   ColorAdjustType.Bitmap);

				using (System.Drawing.Graphics G = Graphics.FromImage(image))
				{
					G.DrawImage(
					   image,
					   new Rectangle(0, 0, width, height),  // destination rectangle 
					   0, 0,        // upper-left corner of source rectangle 
					   width,       // width of source rectangle
					   height,      // height of source rectangle
					   GraphicsUnit.Pixel,
					   imageAttributes);
				}

				//image = Util.AdjustGamma(image, 2.4f);

				Invoke((MethodInvoker)(() =>
				{
					this.picShipPanels.Image = image;
					this.picShipPanels.Refresh();

					DrawPortrait_SHIP();
				}));
			}
			catch (Exception ex)
			{
				Invoke((MethodInvoker)(() =>
				{
					XtraMessageBox.Show(ex.Message);
				}));
			}
		}
		private void DrawPortrait_SHIP()
		{
			Image _BaseLayer = null;
			Image _Layer_1 = null;

			_BaseLayer = this.picShipPanels.Image;

			_Layer_1 = (Image)this.ShipPanelPortraitImage.Clone();
			//_Layer_1 = Util.SetBrightness(_Layer_1, (float)_PortraitBrightness);
			_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1), 0);

			Invoke((MethodInvoker)(() =>
			{
				this.picShipPanels.Image = _Layer_1;
				this.picShipPanels.Refresh();
			}));
		}

		private void DrawPicture_STATION()
		{
			try
			{
				Invoke((MethodInvoker)(() =>
				{
					this.picStationPanels.Image = null;
					this.picStationPanels.Refresh();
				}));

				Image image = (Image)this.StationOriginalImage.Clone();

				ImageAttributes imageAttributes = new ImageAttributes();
				int width = image.Width;
				int height = image.Height;

				float[][] colorMatrixElements = {
				   new float[] { (float)this.sRR_B.Value / 100, (float)this.sRG_B.Value / 100, (float)this.sRB_B.Value / 100,  0,0},        // red scaling factor of 2
				   new float[] { (float)this.sGR_B.Value / 100, (float)this.sGG_B.Value / 100, (float)this.sGB_B.Value / 100,  0, 0},        // green scaling factor of 1
				   new float[] { (float)this.sBR_B.Value / 100, (float)this.sBG_B.Value / 100, (float)this.sBB_B.Value / 100,  0, 0},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 1f}			// three translations of 0.2
				};
				this.ColorMatrix = colorMatrixElements;

				ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
				imageAttributes.SetColorMatrix(
				   colorMatrix,
				   ColorMatrixFlag.Default,
				   ColorAdjustType.Bitmap);

				using (System.Drawing.Graphics G = Graphics.FromImage(image))
				{
					G.DrawImage(
					   image,
					   new Rectangle(0, 0, width, height),  // destination rectangle 
					   0, 0,        // upper-left corner of source rectangle 
					   width,       // width of source rectangle
					   height,      // height of source rectangle
					   GraphicsUnit.Pixel,
					   imageAttributes);
				}				

				Invoke((MethodInvoker)(() =>
				{
					this.picStationPanels.Image = image;
					this.picStationPanels.Refresh();

					DrawPortrait_STATION();
				}));
			}
			catch (Exception ex)
			{
				Invoke((MethodInvoker)(() =>
				{
					XtraMessageBox.Show(ex.Message);
				}));
			}
		}
		private void DrawPortrait_STATION()
		{
			Image _BaseLayer = null;
			Image _Layer_1 = null;

			_BaseLayer = this.picStationPanels.Image;

			_Layer_1 = (Image)this.StationPortraitImage.Clone();
			//_Layer_1 = Util.ApplyColorMatrix(_Layer_1, PanelColorMatrix);
			//_Layer_1 = Util.ApplyColorMatrix(_Layer_1, PortraitColorMatrix);

			//_Layer_1 = Util.SetBrightness(_Layer_1, (float)_PortraitBrightness);
			_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1), 0);

			Invoke((MethodInvoker)(() =>
			{
				this.picStationPanels.Image = _Layer_1;
				this.picStationPanels.Refresh();
			}));
		}

		private void MakeXML_LOW()
		{
			decimal tRR, tRG, tRB;
			decimal tGR, tGG, tGB;
			decimal tBR, tBG, tBB;

			tRR = (decimal)this.sRR_B.Value / 100;
			tRG = (decimal)this.sRG_B.Value / 100;
			tRB = (decimal)this.sRB_B.Value / 100;

			tGR = (decimal)this.sGR_B.Value / 100;
			tGG = (decimal)this.sGG_B.Value / 100;
			tGB = (decimal)this.sGB_B.Value / 100;

			tBR = (decimal)this.sBR_B.Value / 100;
			tBG = (decimal)this.sBG_B.Value / 100;
			tBB = (decimal)this.sBB_B.Value / 100;

			StringBuilder _XML_Matrix = new StringBuilder();
			_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", tRR, tRG, tRB));
			_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", tGR, tGG, tGB));
			_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", tBR, tBG, tBB));

			Invoke((MethodInvoker)(() =>
			{
				this.txtXML_LOW.Text = _XML_Matrix.ToString();
			}));
		}
		private void ImportXML_LOW(string _XML)
		{
			try
			{
				if (_XML != null && _XML != string.Empty)
				{
					decimal RR, RG, RB = 0;
					decimal GR, GG, GB = 0;
					decimal BR, BG, BB = 0;

					int _Start = _XML.IndexOf("<MatrixRed>");
					int _End = _XML.IndexOf("</MatrixRed>");
					string _RedM = _XML.Substring(_Start, _End - _Start);
					if (_RedM != string.Empty)
					{
						_RedM = _RedM.Replace("<MatrixRed>", string.Empty);
						string[] subs = _RedM.Split(',');
						RR = Convert.ToDecimal(subs[0]);
						RG = Convert.ToDecimal(subs[1]);
						RB = Convert.ToDecimal(subs[2]);

						Invoke((MethodInvoker)(() =>
						{
							this.sRR_B.Value = Convert.ToInt32(RR * 100);
							this.sRG_B.Value = Convert.ToInt32(RG * 100);
							this.sRB_B.Value = Convert.ToInt32(RB * 100);
						}));
					}

					_Start = _XML.IndexOf("<MatrixGreen>");
					_End = _XML.IndexOf("</MatrixGreen>");
					_RedM = _XML.Substring(_Start, _End - _Start);
					if (_RedM != string.Empty)
					{
						_RedM = _RedM.Replace("<MatrixGreen>", string.Empty);
						string[] subs = _RedM.Split(',');
						GR = Convert.ToDecimal(subs[0]);
						GG = Convert.ToDecimal(subs[1]);
						GB = Convert.ToDecimal(subs[2]);

						Invoke((MethodInvoker)(() =>
						{
							this.sGR_B.Value = Convert.ToInt32(GR * 100);
							this.sGG_B.Value = Convert.ToInt32(GG * 100);
							this.sGB_B.Value = Convert.ToInt32(GB * 100);
						}));
					}

					_Start = _XML.IndexOf("<MatrixBlue>");
					_End = _XML.IndexOf("</MatrixBlue>");
					_RedM = _XML.Substring(_Start, _End - _Start);
					if (_RedM != string.Empty)
					{
						_RedM = _RedM.Replace("<MatrixBlue>", string.Empty);
						string[] subs = _RedM.Split(',');
						BR = Convert.ToDecimal(subs[0]);
						BG = Convert.ToDecimal(subs[1]);
						BB = Convert.ToDecimal(subs[2]);

						Invoke((MethodInvoker)(() =>
						{
							this.sBR_B.Value = Convert.ToInt32(BR * 100);
							this.sBG_B.Value = Convert.ToInt32(BG * 100);
							this.sBB_B.Value = Convert.ToInt32(BB * 100);
						}));
					}
				}
			}
			catch (Exception ex)
			{
				Invoke((MethodInvoker)(() =>
				{
					MessageBox.Show(ex.Message);
				}));
			}
		}

		private void sliderBar_EditValueChanged(object sender, EventArgs e)
		{
			TrackBarControl _Slider = sender as TrackBarControl;
			if (_Slider != null)
			{
				DevExpress.XtraLayout.LayoutControl _Parent = _Slider.Parent as DevExpress.XtraLayout.LayoutControl;
				var X = (_Parent.GetItemByControl(_Slider)).Text = ((decimal)_Slider.Value / 100).ToString();
			}
		}
		private void sliderBar_KeyUp(object sender, KeyEventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				MakeXML_LOW();

				DrawPicture_SHIP();
				DrawPicture_STATION();
			});
		}
		private void sliderBar_MouseUp(object sender, MouseEventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				MakeXML_LOW();

				DrawPicture_SHIP();
				DrawPicture_STATION();

				DrawPortrait_SHIP();
				//DrawPicture_LOW();
				//DrawPortrait_LOW();
			});
		}

		private void cmdImportXML_2_Click(object sender, EventArgs e)
		{
			ImportXML_LOW(this.txtXML_LOW.Text);

			DrawPicture_SHIP();
			DrawPicture_STATION();
		}

		private void cmdOpenNO2O_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			//abre con el link con el Navegador predeterminado:
			System.Diagnostics.Process.Start("https://forums.frontier.co.uk/threads/no2o-the-definitive-list-of-1-7-2-2-compatible-hud-colour-color-configs-please-add-yours.259311/");
		}

		private void cmdSaveExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (this.xml_profile.IsNotEmpty())
				{
					this.xml_profile.Find(X => X.key == "x150").value = ((decimal)this.sRR_B.Value / 100);
					this.xml_profile.Find(X => X.key == "y150").value = ((decimal)this.sRG_B.Value / 100);
					this.xml_profile.Find(X => X.key == "z150").value = ((decimal)this.sRB_B.Value / 100);

					this.xml_profile.Find(X => X.key == "x151").value = ((decimal)this.sGR_B.Value / 100);
					this.xml_profile.Find(X => X.key == "y151").value = ((decimal)this.sGG_B.Value / 100);
					this.xml_profile.Find(X => X.key == "z151").value = ((decimal)this.sGB_B.Value / 100);

					this.xml_profile.Find(X => X.key == "x152").value = ((decimal)this.sBR_B.Value / 100);
					this.xml_profile.Find(X => X.key == "y152").value = ((decimal)this.sBG_B.Value / 100);
					this.xml_profile.Find(X => X.key == "z152").value = ((decimal)this.sBB_B.Value / 100);
				}
				Close();

				/*
				string INIpath = Path.Combine(this.ActiveInstance.path, "EDHM-ini", "XML-Profile.ini");
				//string INIpath = Path.Combine(this.ActiveTheme.folder, "XML-Profile.ini");
				if (File.Exists(INIpath))
				{
					this._Reader = new IniFile(INIpath);

					if (this._Reader != null)
					{
						this._Reader.WriteKey("x150", ((decimal)this.sRR_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y150", ((decimal)this.sRG_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z150", ((decimal)this.sRB_B.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x151", ((decimal)this.sGR_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y151", ((decimal)this.sGG_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z151", ((decimal)this.sGB_B.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x152", ((decimal)this.sBR_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y152", ((decimal)this.sBG_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z152", ((decimal)this.sBB_B.Value / 100).ToString(), "constants");

						Close();
					}
					else
					{
						throw new Exception("ERROR 404: Could not load the 'XML - Profile.ini' file!");
					}
				}
				else
				{
					throw new Exception("ERROR 404: Could not find the 'XML - Profile.ini' file!");
				}*/
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}
	}
}