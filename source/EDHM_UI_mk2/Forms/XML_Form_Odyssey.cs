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
		#region Declaraciones

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

		#endregion

		#region Constructores

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

			this.picStationPanels.Image = this.StationOriginalImage;

			//this.trackGamma.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
			//this.trackSaturation.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
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

		#endregion

		#region Metodos

		private void LoadINIProfile()
		{
			try
			{
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

					bRR = this.xml_profile.Find(X => X.key == "x150").value; 
					bRG = this.xml_profile.Find(X => X.key == "y150").value; 
					bRB = this.xml_profile.Find(X => X.key == "z150").value; 

					bGR = this.xml_profile.Find(X => X.key == "x151").value; 
					bGG = this.xml_profile.Find(X => X.key == "y151").value;  
					bGB = this.xml_profile.Find(X => X.key == "z151").value; 

					bBR = this.xml_profile.Find(X => X.key == "x152").value;  
					bBG = this.xml_profile.Find(X => X.key == "y152").value; 
					bBB = this.xml_profile.Find(X => X.key == "z152").value; 

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
				   new float[] { 0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] { 0f, 0f, 0f, 0f, 1f}			// three translations of 0.2
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

				float Gamma = (float)(Convert.ToInt32(trackGamma.EditValue) / 10);
				image = Util.AdjustGamma(image, Gamma);

				float saturation = 1f - (Convert.ToInt32(trackSaturation.EditValue) / 10f);
				image = Util.SetSaturation(image, saturation);

				Invoke((MethodInvoker)(() =>
				{
					this.picStationPanels.Image = image;
					this.picStationPanels.Refresh();

					DrawPortrait_STATION();
					TransformXMLColors();
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

		public void TransformXMLColors()
		{
			try
			{
				Color CustomColor = picTransform_ED_Custom.Color;

				// Get XML Values from the Sliders:
				XmlValues xmlValues = new XmlValues()
				{
					Red =   new RgbValues(this.sRR_B.Value / 100m, this.sRG_B.Value / 100m, this.sRB_B.Value / 100m),  //<- Slider Values are multiplied by 100;
					Green = new RgbValues(this.sGR_B.Value / 100m, this.sGG_B.Value / 100m, this.sGB_B.Value / 100m),
					Blue =  new RgbValues(this.sBR_B.Value / 100m, this.sBG_B.Value / 100m, this.sBB_B.Value / 100m)
				};

				// Orange Transformation:
				RgbValues Percentages = new RgbValues(1.0m, 0.5m, 0.0m);
				Color TransformColor_Orange = TransformColorFromXML(xmlValues, Percentages);

				// White Transformation:
				Percentages = new RgbValues(1.0m, 1.0m, 1.0m);
				Color TransformColor_White = TransformColorFromXML(xmlValues, Percentages);

				// Red Transformation:
				Percentages = new RgbValues(1.0m, 0.0m, 0.0m);
				Color TransformColor_Red = TransformColorFromXML(xmlValues, Percentages);

				//Cyan Transformation:
				Percentages = new RgbValues(0.0m, 1.0m, 1.0m);
				Color TransformColor_Cyan = TransformColorFromXML(xmlValues, Percentages);

				// Custom Color Transformation:
				Percentages = new RgbValues(CustomColor.R / 255m, CustomColor.G / 255m, CustomColor.B / 255m);
				Color TransformColor_Custom = TransformColorFromXML(xmlValues, Percentages);

				#region Show the Colors

				picTransform_ED_Orange.BackColor = Color.Orange;
				picTransform_XML_Orange.BackColor = TransformColor_Orange;
				picTransform_RGB_Orange.Text = string.Format("{0};{1};{2}", TransformColor_Orange.R, TransformColor_Orange.G, TransformColor_Orange.B);
				picTransform_HEX_Orange.Text = ColorTranslator.ToHtml(TransformColor_Orange);

				picTransform_ED_White.BackColor = Color.White;
				picTransform_XML_White.BackColor = TransformColor_White;
				picTransform_RGB_White.Text = string.Format("{0};{1};{2}", TransformColor_White.R, TransformColor_White.G, TransformColor_White.B);
				picTransform_HEX_White.Text = ColorTranslator.ToHtml(TransformColor_White);

				picTransform_ED_Red.BackColor = Color.Red;
				picTransform_XML_Red.BackColor = TransformColor_Red;
				picTransform_RGB_Red.Text = string.Format("{0};{1};{2}", TransformColor_Red.R, TransformColor_Red.G, TransformColor_Red.B);
				picTransform_HEX_Red.Text = ColorTranslator.ToHtml(TransformColor_Red);

				picTransform_ED_Cyan.BackColor = Color.Cyan;
				picTransform_XML_Cyan.BackColor = TransformColor_Cyan;
				picTransform_RGB_Cyan.Text = string.Format("{0};{1};{2}", TransformColor_Cyan.R, TransformColor_Cyan.G, TransformColor_Cyan.B);
				picTransform_HEX_Cyan.Text = ColorTranslator.ToHtml(TransformColor_Cyan);

				picTransform_XML_Custom.BackColor = TransformColor_Custom;
				picTransform_RGB_Custom.Text = string.Format("{0};{1};{2}", TransformColor_Custom.R, TransformColor_Custom.G, TransformColor_Custom.B);
				picTransform_HEX_Custom.Text = ColorTranslator.ToHtml(TransformColor_Custom);

				#endregion
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private Color TransformColorFromXML(XmlValues xml, RgbValues Percentages)
		{
			Color _ret = Color.White;
			try
			{
				//1. Get Raw Values
				decimal Raw_R = (xml.Red.Red * Percentages.Red)		+ (xml.Green.Red * Percentages.Green)	+ (xml.Blue.Red * Percentages.Blue);
				decimal Raw_G = (xml.Red.Green * Percentages.Red)	+ (xml.Green.Green * Percentages.Green)	+ (xml.Blue.Green * Percentages.Blue);
				decimal Raw_B = (xml.Red.Blue * Percentages.Red)	+ (xml.Green.Blue * Percentages.Green)	+ (xml.Blue.Blue * Percentages.Blue);

				//2. Normalize_R = MAX(0; SI(MAX($D25:$F25) > 1; (1 / MAX($D25:$F25)) * D25; D25))
				decimal Max_V = Util.Max(Raw_R, Raw_G, Raw_B);

				decimal Norm_R = Math.Max(0.0m, Max_V > 1.0m ? (1 / Max_V) * Raw_R : Raw_R);
				decimal Norm_G = Math.Max(0.0m, Max_V > 1.0m ? (1 / Max_V) * Raw_G : Raw_G);
				decimal Norm_B = Math.Max(0.0m, Max_V > 1.0m ? (1 / Max_V) * Raw_B : Raw_B);

				//3. Convert to RGB values
				int rgb_R = Convert.ToInt32(Math.Round(Norm_R * 255m, 0));
				int rgb_G = Convert.ToInt32(Math.Round(Norm_G * 255m, 0));
				int rgb_B = Convert.ToInt32(Math.Round(Norm_B * 255m, 0));

				_ret = Color.FromArgb(255, rgb_R, rgb_G, rgb_B);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
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

		#endregion

		#region Eventos de Controles

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

		private void trackGammaRep_ValueChanged(object sender, EventArgs e)
		{
			DrawPicture_STATION();
		}
		private void trackSaturationRep_ValueChanged(object sender, EventArgs e)
		{
			DrawPicture_STATION();
		}
		private void picTransform_ED_Custom_EditValueChanged(object sender, EventArgs e)
		{
			decimal bRR, bRG, bRB = 0;
			decimal bGR, bGG, bGB = 0;
			decimal bBR, bBG, bBB = 0;

			bRR = this.sRR_B.Value / 100m;
			bRG = this.sRG_B.Value / 100m;
			bRB = this.sRB_B.Value / 100m;

			bGR = this.sGR_B.Value / 100m;
			bGG = this.sGG_B.Value / 100m;
			bGB = this.sGB_B.Value / 100m;

			bBR = this.sBR_B.Value / 100m;
			bBG = this.sBG_B.Value / 100m;
			bBB = this.sBB_B.Value / 100m;

			Color CustomColor = picTransform_ED_Custom.Color;

			decimal per_R = CustomColor.R / 255m;
			decimal per_G = CustomColor.G / 255m;
			decimal per_B = CustomColor.B / 255m;

			decimal Raw_R = (bRR * per_R) + (bGR * per_G) + (bBR * per_B);
			decimal Raw_G = (bRG * per_R) + (bGG * per_G) + (bBG * per_B);
			decimal Raw_B = (bRB * per_R) + (bGB * per_G) + (bBB * per_B);

			decimal Max_V = Util.Max(Raw_R, Raw_G, Raw_B);

			decimal Norm_R = Math.Max(0.0m, Max_V > 1.0m ? (1 / Max_V) * Raw_R : Raw_R);
			decimal Norm_G = Math.Max(0.0m, Max_V > 1.0m ? (1 / Max_V) * Raw_G : Raw_G);
			decimal Norm_B = Math.Max(0.0m, Max_V > 1.0m ? (1 / Max_V) * Raw_B : Raw_B);

			int rgb_R = Convert.ToInt32(Math.Round(Norm_R * 255m, 0));
			int rgb_G = Convert.ToInt32(Math.Round(Norm_G * 255m, 0));
			int rgb_B = Convert.ToInt32(Math.Round(Norm_B * 255m, 0));

			picTransform_XML_Custom.BackColor = Color.FromArgb(255, rgb_R, rgb_G, rgb_B);
			picTransform_RGB_Custom.Text = string.Format("{0};{1};{2}",
					picTransform_XML_Custom.BackColor.R, picTransform_XML_Custom.BackColor.G, picTransform_XML_Custom.BackColor.B);
			picTransform_HEX_Custom.Text = ColorTranslator.ToHtml(picTransform_XML_Custom.BackColor);
		}

		#endregion

		#region Botonera

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
				//Fija los valores de los Sliders en el XML cargado:
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
				//Quien llamó a este formulario debe encargarse de Guardar los cambios.

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

		#endregion

	}

	class XmlValues
	{
		public RgbValues Red { get; set; }
		public RgbValues Green { get; set; }
		public RgbValues Blue { get; set; }
	}
	class RgbValues
	{
		public RgbValues() { }
		public RgbValues(decimal pRed, decimal pGreen, decimal pBlue)
		{
			this.Red = pRed;
			this.Green = pGreen;
			this.Blue = pBlue;
		}

		public decimal Red { get; set; } = 0.0m;
		public decimal Green { get; set; } = 0.0m;
		public decimal Blue { get; set; } = 0.0m;
	}
}