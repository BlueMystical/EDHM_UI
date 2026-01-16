using System.Drawing.Imaging;

namespace ColorMatrixForm
{
	public partial class Form1 : Form
	{
		/// <summary>Devuelve la Matrix de Color Elejida.</summary>
		public float[][] ColorMatrix { get; set; }

		private Image StationOriginalImage = null;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//Obligar a usar los puntos y las comas;
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			customCulture.NumberFormat.NumberGroupSeparator = ",";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
			customCulture.NumberFormat.CurrencyGroupSeparator = ",";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
		}
		private void Form1_Shown(object sender, EventArgs e)
		{
			this.StationOriginalImage = this.picStationOriginalImage.Image;

			ImportXML(this.textBox1.Text);
			DrawPicture_STATION();
		}

		private void DrawPicture_STATION()
		{
			try
			{
				Invoke((MethodInvoker)(() =>
				{
					this.picShipPanels.Image = null;
					this.picShipPanels.Refresh();
				}));

				Image image = (Image)this.StationOriginalImage.Clone();
				
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

				System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(colorMatrixElements);
				ImageAttributes imageAttributes = new ImageAttributes();
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

				//float Gamma = (float)(Convert.ToInt32(trackGamma.Value) / 10);
				//image = Util.AdjustGamma(image, Gamma);

				//float saturation = 1f - (Convert.ToInt32(trackSaturation.Value) / 10f);
				//image = Util.SetSaturation(image, saturation);

				Invoke((MethodInvoker)(() =>
				{
					this.picShipPanels.Image = image;
					this.picShipPanels.Refresh();

					//DrawPortrait_STATION();
					TransformXMLColors();
				}));
			}
			catch (Exception ex)
			{
				Invoke((MethodInvoker)(() =>
				{
					MessageBox.Show(ex.Message + ex.StackTrace);
				}));
			}
		}

		public void TransformXMLColors()
		{
			try
			{
				Color CustomColor = Color.Orange; // picTransform_ED_Custom.Color;

				// Get XML Values from the Sliders:
				XmlValues xmlValues = new XmlValues()
				{
					Red = new RgbValues(this.sRR_B.Value / 100m, this.sRG_B.Value / 100m, this.sRB_B.Value / 100m),  //<- Slider Values are multiplied by 100;
					Green = new RgbValues(this.sGR_B.Value / 100m, this.sGG_B.Value / 100m, this.sGB_B.Value / 100m),
					Blue = new RgbValues(this.sBR_B.Value / 100m, this.sBG_B.Value / 100m, this.sBB_B.Value / 100m)
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
				/*
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
				*/
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
				decimal Raw_R = (xml.Red.Red * Percentages.Red) + (xml.Green.Red * Percentages.Green) + (xml.Blue.Red * Percentages.Blue);
				decimal Raw_G = (xml.Red.Green * Percentages.Red) + (xml.Green.Green * Percentages.Green) + (xml.Blue.Green * Percentages.Blue);
				decimal Raw_B = (xml.Red.Blue * Percentages.Red) + (xml.Green.Blue * Percentages.Green) + (xml.Blue.Blue * Percentages.Blue);

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

		private void ImportXML(string _XML)
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

		#region Utilities

		/// <summary>Devuelve 'true' si el color contiene valores RGB validos.</summary>
		/// <param name="_RGBA">Valores RGBA, (A es opcional)</param>
		private bool IsColor(List<double> _RGBA)
		{
			bool _ret = true;
			try
			{
				if (_RGBA != null && _RGBA.Count > 0)
				{
					foreach (var item in _RGBA)
					{
						if (item < 0)
						{
							_ret = false;
						}
					}
				}
			}
			catch { }
			return _ret;
		}

		private double Convert_sRGB_FromLinear(double theLinearValue, double _GammaValue = 2.4)
		{ return theLinearValue <= 0.0031308f ? theLinearValue * 12.92f : Math.Pow(theLinearValue, 1.0 / _GammaValue) * 1.055 - 0.055; }
		private double Convert_sRGB_ToLinear(double thesRGBValue, double _GammaValue = 2.4)
		{ return thesRGBValue <= 0.04045f ? thesRGBValue / 12.92f : Math.Pow((thesRGBValue + 0.055f) / 1.055f, _GammaValue); }


		#endregion

		private void sRGBB_Scroll(object sender, EventArgs e)
		{
			DrawPicture_STATION();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			ImportXML(this.textBox1.Text);
			DrawPicture_STATION();
		}
	}
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
