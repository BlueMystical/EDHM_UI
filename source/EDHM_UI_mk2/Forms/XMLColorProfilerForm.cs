using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using EDHM_UI_mk2;

namespace EDHM_DX
{
	public partial class XMLColorProfilerForm : DevExpress.XtraEditors.XtraForm
	{
		/// <summary>Devuelve la Matrix de Color Elejida.</summary>
		public float[][] ColorMatrix { get; set; }

		private Image StationOriginalImage = null;
		private Image StationPortraitImage = null;

		private Image ShipPanelOriginalImage = null;
		private Image ShipPanelPortraitImage = null;

		private IniFile _Reader = null;
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;

		public game_instance ActiveInstance { get; set; }

		public XMLColorProfilerForm(game_instance _ActiveInstance)
		{
			InitializeComponent();
			this.LookAndFeel.TouchUIMode = DevExpress.Utils.DefaultBoolean.False;
			this.LookAndFeel.TouchScaleFactor = 1;
			this.ActiveInstance = _ActiveInstance;
		}

		private void XMLColorProfilerForm_Load(object sender, EventArgs e)
		{
			//Obligar a usar los puntos y las comas;
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			customCulture.NumberFormat.NumberGroupSeparator = ",";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
			customCulture.NumberFormat.CurrencyGroupSeparator = ",";

			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			this.StationOriginalImage = Image.FromFile(Path.Combine(AppExePath, @"Images\ColorMatrix_Preview_1a.png"));
			this.StationPortraitImage = Image.FromFile(Path.Combine(AppExePath, @"Images\ColorMatrix_Preview_1b.png"));

			this.ShipPanelOriginalImage = Image.FromFile(Path.Combine(AppExePath, @"Images\ColorMatrix_Preview_2a.png"));
			this.ShipPanelPortraitImage = Image.FromFile(Path.Combine(AppExePath, @"Images\ColorMatrix_Preview_2b.png"));

			this.pictureBox1.Image = this.StationOriginalImage;
			this.pictureBox2.Image = this.ShipPanelOriginalImage;

			List<combo_item> _OnOff = new List<combo_item>();
			_OnOff.Add(new combo_item("ONOFF", "Use the Ship Panel XML", 0));
			_OnOff.Add(new combo_item("ONOFF", "Use the Station panel XML", 1));

			cboPortrasitModel.Properties.ValueMember = "Index";
			cboPortrasitModel.Properties.DisplayMember = "Name";
			cboPortrasitModel.Properties.DataSource = _OnOff;
		}

		private void XMLColorProfilerForm_Shown(object sender, EventArgs e)
		{
			LoadINIProfile();

			DrawPicture_TOP();
			DrawPortrait_TOP();

			DrawPicture_LOW();
			DrawPortrait_LOW();
		}

		private void LoadINIProfile()
		{
			try
			{
				this._Reader = new IniFile(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_XML-Profile.ini"));  //<- Open and Read the INI file

				if (this._Reader != null)
				{
					StringBuilder _XML_Matrix = new StringBuilder();

					//** CONFIGS:
					cboPortrasitModel.EditValue = Convert.ToDecimal(this._Reader.ReadKey("x124", "constants"));
					trackPortraitBrightness.Value = Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("w153", "constants")) * 100);
					trackShipPanelsBrightness.Value = Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("z157", "constants")) * 100);
					trackHiglightsBrightness.Value = Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("w157", "constants")) * 100);

					//-----------------------------------------------------------------------------------;
					decimal tRR, tRG, tRB = 0;
					decimal tGR, tGG, tGB = 0;
					decimal tBR, tBG, tBB = 0;

					//** --== XML Station-Carrier Profile ==--
					tRR = Convert.ToDecimal(this._Reader.ReadKey("x158", "constants"));
					tRG = Convert.ToDecimal(this._Reader.ReadKey("y158", "constants"));
					tRB = Convert.ToDecimal(this._Reader.ReadKey("z158", "constants"));

					tGR = Convert.ToDecimal(this._Reader.ReadKey("x159", "constants"));
					tGG = Convert.ToDecimal(this._Reader.ReadKey("y159", "constants"));
					tGB = Convert.ToDecimal(this._Reader.ReadKey("z159", "constants"));

					tBR = Convert.ToDecimal(this._Reader.ReadKey("x160", "constants"));
					tBG = Convert.ToDecimal(this._Reader.ReadKey("y160", "constants"));
					tBB = Convert.ToDecimal(this._Reader.ReadKey("z160", "constants"));

					_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", tRR, tRG, tRB));
					_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", tGR, tGG, tGB));
					_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", tBR, tBG, tBB));

					this.txtXML.Text = _XML_Matrix.ToString();

					this.sRR.Value = Convert.ToInt32(tRR * 100);
					this.sRG.Value = Convert.ToInt32(tRG * 100);
					this.sRB.Value = Convert.ToInt32(tRB * 100);

					this.sGR.Value = Convert.ToInt32(tGR * 100);
					this.sGG.Value = Convert.ToInt32(tGG * 100);
					this.sGB.Value = Convert.ToInt32(tGB * 100);

					this.sBR.Value = Convert.ToInt32(tBR * 100);
					this.sBG.Value = Convert.ToInt32(tBG * 100);
					this.sBB.Value = Convert.ToInt32(tBB * 100);

					//-----------------------------------------------------------------------------------;
					//**   --== XML Portrait Fix ==--

					decimal mRR, mRG, mRB = 0;
					decimal mGR, mGG, mGB = 0;
					decimal mBR, mBG, mBB = 0;

					mRR = Convert.ToDecimal(this._Reader.ReadKey("x153", "constants"));
					mRG = Convert.ToDecimal(this._Reader.ReadKey("y153", "constants"));
					mRB = Convert.ToDecimal(this._Reader.ReadKey("z153", "constants"));

					mGR = Convert.ToDecimal(this._Reader.ReadKey("x154", "constants"));
					mGG = Convert.ToDecimal(this._Reader.ReadKey("y154", "constants"));
					mGB = Convert.ToDecimal(this._Reader.ReadKey("z154", "constants"));

					mBR = Convert.ToDecimal(this._Reader.ReadKey("x155", "constants"));
					mBG = Convert.ToDecimal(this._Reader.ReadKey("y155", "constants"));
					mBB = Convert.ToDecimal(this._Reader.ReadKey("z155", "constants"));

					invRR.Value = mRR;
					invRG.Value = mRG;
					invRB.Value = mRB;

					invGR.Value = mGR;
					invGG.Value = mGG;
					invGB.Value = mGB;

					invBR.Value = mBR;
					invBG.Value = mBG;
					invBB.Value = mBB;

					//-----------------------------------------------------------------------------------;
					//** --== XML Ship Profile ==--

					decimal bRR, bRG, bRB = 0;
					decimal bGR, bGG, bGB = 0;
					decimal bBR, bBG, bBB = 0;

					bRR = Convert.ToDecimal(this._Reader.ReadKey("x150", "constants"));
					bRG = Convert.ToDecimal(this._Reader.ReadKey("y150", "constants"));
					bRB = Convert.ToDecimal(this._Reader.ReadKey("z150", "constants"));

					bGR = Convert.ToDecimal(this._Reader.ReadKey("x151", "constants"));
					bGG = Convert.ToDecimal(this._Reader.ReadKey("y151", "constants"));
					bGB = Convert.ToDecimal(this._Reader.ReadKey("z151", "constants"));

					bBR = Convert.ToDecimal(this._Reader.ReadKey("x152", "constants"));
					bBG = Convert.ToDecimal(this._Reader.ReadKey("y152", "constants"));
					bBB = Convert.ToDecimal(this._Reader.ReadKey("z152", "constants"));

					_XML_Matrix.Length = 0;
					_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", bRR, bRG, bRB));
					_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", bGR, bGG, bGB));
					_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", bBR, bBG, bBB));
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
				}
				//});
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		private Matrix<double> CalculateInverseValues()
		{
			Matrix<double> _ret = null;
			try
			{
				Matrix<double> colorMatrixElements = DenseMatrix.OfArray(new double[,] {
					{ (double)this.sRR.Value / 100, (double)this.sRG.Value / 100, (double)this.sRB.Value / 100,  0, 0 },
					{ (double)this.sGR.Value / 100, (double)this.sGG.Value / 100, (double)this.sGB.Value / 100,  0, 0 },
					{ (double)this.sBR.Value / 100, (double)this.sBG.Value / 100, (double)this.sBB.Value / 100,  0, 0 },
					{0,  0,  0,  1, 0},
					{0, 0, 0, 0, 1}
				});
				Vector<double>[] nullspace = colorMatrixElements.Kernel();
				Matrix<double> inv = colorMatrixElements.Inverse();
				_ret = inv;

				//var inv = MatrixInverse(colorMatrixElements);
				if (inv != null && !this.toggleLock_MID.IsOn)
				{
					this.invRR.Value = Convert.ToDecimal(Math.Round(inv[0, 0], 3));
					this.invRG.Value = Convert.ToDecimal(Math.Round(inv[0, 1], 3));
					this.invRB.Value = Convert.ToDecimal(Math.Round(inv[0, 2], 3));

					this.invGR.Value = Convert.ToDecimal(Math.Round(inv[1, 0], 3));
					this.invGG.Value = Convert.ToDecimal(Math.Round(inv[1, 1], 3));
					this.invGB.Value = Convert.ToDecimal(Math.Round(inv[1, 2], 3));

					this.invBR.Value = Convert.ToDecimal(Math.Round(inv[2, 0], 3));
					this.invBG.Value = Convert.ToDecimal(Math.Round(inv[2, 1], 3));
					this.invBB.Value = Convert.ToDecimal(Math.Round(inv[2, 2], 3));
				}

				if (!this.toggleLock_LOW.IsOn)
				{
					this.sRR_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[0, 0], 3) * 100);
					this.sRG_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[0, 1], 3) * 100);
					this.sRB_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[0, 2], 3) * 100);

					this.sGR_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[1, 0], 3) * 100);
					this.sGG_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[1, 1], 3) * 100);
					this.sGB_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[1, 2], 3) * 100);

					this.sBR_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[2, 0], 3) * 100);
					this.sBG_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[2, 1], 3) * 100);
					this.sBB_B.Value = Convert.ToInt32(Math.Round(colorMatrixElements[2, 2], 3) * 100);

					MakeXML_LOW();
					DrawPicture_LOW();
					DrawPortrait_LOW();
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
			return _ret;
		}

		private void MakeXML_TOP()
		{
			decimal tRR, tRG, tRB;
			decimal tGR, tGG, tGB;
			decimal tBR, tBG, tBB;

			tRR = (decimal)this.sRR.Value / 100;
			tRG = (decimal)this.sRG.Value / 100;
			tRB = (decimal)this.sRB.Value / 100;

			tGR = (decimal)this.sGR.Value / 100;
			tGG = (decimal)this.sGG.Value / 100;
			tGB = (decimal)this.sGB.Value / 100;

			tBR = (decimal)this.sBR.Value / 100;
			tBG = (decimal)this.sBG.Value / 100;
			tBB = (decimal)this.sBB.Value / 100;

			StringBuilder _XML_Matrix = new StringBuilder();
			_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", tRR, tRG, tRB));
			_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", tGR, tGG, tGB));
			_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", tBR, tBG, tBB));

			this.txtXML.Text = _XML_Matrix.ToString();
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

			this.txtXML_LOW.Text = _XML_Matrix.ToString();
		}

		private void ImportXML_TOP(string _XML)
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
						RR = Convert.ToDecimal(subs[0].NVL("0"));
						RG = Convert.ToDecimal(subs[1].NVL("0"));
						RB = Convert.ToDecimal(subs[2].NVL("0"));

						this.sRR.Value = Convert.ToInt32(RR * 100);
						this.sRG.Value = Convert.ToInt32(RG * 100);
						this.sRB.Value = Convert.ToInt32(RB * 100);
					}

					_Start = _XML.IndexOf("<MatrixGreen>");
					_End = _XML.IndexOf("</MatrixGreen>");
					_RedM = _XML.Substring(_Start, _End - _Start);
					if (_RedM != string.Empty)
					{
						_RedM = _RedM.Replace("<MatrixGreen>", string.Empty);
						string[] subs = _RedM.Split(',');
						GR = Convert.ToDecimal(subs[0].NVL("0"));
						GG = Convert.ToDecimal(subs[1].NVL("0"));
						GB = Convert.ToDecimal(subs[2].NVL("0"));

						this.sGR.Value = Convert.ToInt32(GR * 100);
						this.sGG.Value = Convert.ToInt32(GG * 100);
						this.sGB.Value = Convert.ToInt32(GB * 100);
					}

					_Start = _XML.IndexOf("<MatrixBlue>");
					_End = _XML.IndexOf("</MatrixBlue>");
					_RedM = _XML.Substring(_Start, _End - _Start);
					if (_RedM != string.Empty)
					{
						_RedM = _RedM.Replace("<MatrixBlue>", string.Empty);
						string[] subs = _RedM.Split(',');
						BR = Convert.ToDecimal(subs[0].NVL("0"));
						BG = Convert.ToDecimal(subs[1].NVL("0"));
						BB = Convert.ToDecimal(subs[2].NVL("0"));

						this.sBR.Value = Convert.ToInt32(BR * 100);
						this.sBG.Value = Convert.ToInt32(BG * 100);
						this.sBB.Value = Convert.ToInt32(BB * 100);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
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

						this.sRR_B.Value = Convert.ToInt32(RR * 100); //tRR.Text = ((decimal)sRR.Value / 100).ToString().Replace(',', '.');
						this.sRG_B.Value = Convert.ToInt32(RG * 100); //tRG.Text = ((decimal)sRG.Value / 100).ToString().Replace(',', '.');
						this.sRB_B.Value = Convert.ToInt32(RB * 100); //tRB.Text = ((decimal)sRB.Value / 100).ToString().Replace(',', '.');
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

						this.sGR_B.Value = Convert.ToInt32(GR * 100); //tGR.Text = ((decimal)sGR.Value / 100).ToString().Replace(',', '.');
						this.sGG_B.Value = Convert.ToInt32(GG * 100); //tGG.Text = ((decimal)sGG.Value / 100).ToString().Replace(',', '.');
						this.sGB_B.Value = Convert.ToInt32(GB * 100); //tGB.Text = ((decimal)sGB.Value / 100).ToString().Replace(',', '.');
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

						this.sBR_B.Value = Convert.ToInt32(BR * 100); //tBR.Text = ((decimal)sBR.Value / 100).ToString().Replace(',', '.');
						this.sBG_B.Value = Convert.ToInt32(BG * 100); //tBG.Text = ((decimal)sBG.Value / 100).ToString().Replace(',', '.');
						this.sBB_B.Value = Convert.ToInt32(BB * 100); //tBB.Text = ((decimal)sBB.Value / 100).ToString().Replace(',', '.');
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void DrawPicture_TOP()
		{
			try
			{
				this.pictureBox1.Image = null;
				this.pictureBox1.Refresh();

				Image image = (Image)this.StationOriginalImage.Clone();

				ImageAttributes imageAttributes = new ImageAttributes();
				int width = image.Width;
				int height = image.Height;

				float[][] colorMatrixElements = {
				   new float[] { (float)this.sRR.Value / 100, (float)this.sRG.Value / 100, (float)this.sRB.Value / 100,  1, 1},        // red scaling factor of 2
				   new float[] { (float)this.sGR.Value / 100, (float)this.sGG.Value / 100, (float)this.sGB.Value / 100,  1, 1},        // green scaling factor of 1
				   new float[] { (float)this.sBR.Value / 100, (float)this.sBG.Value / 100, (float)this.sBB.Value / 100,  1, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
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

				this.pictureBox1.Image = image;
				this.pictureBox1.Refresh();
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		private void DrawPortrait_TOP()
		{
			Image _BaseLayer = null;
			Image _Layer_1 = null;

			float[][] PanelColorMatrix = null;

			decimal _PortraitMode = Convert.ToDecimal(this.cboPortrasitModel.EditValue);
			decimal _PortraitBrightness = (decimal)this.trackPortraitBrightness.Value / 100;

			float[][] PortraitColorMatrix = {
				   new float[] { (float)this.invRR.Value, (float)this.invRG.Value, (float)this.invRB.Value,  0, 1},        // red scaling factor of 2
				   new float[] { (float)this.invGR.Value, (float)this.invGG.Value, (float)this.invGB.Value,  0, 1},        // green scaling factor of 1
				   new float[] { (float)this.invBR.Value, (float)this.invBG.Value, (float)this.invBB.Value,  0, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
			};
			if (_PortraitMode == 0)
			{
				float[][] _ColorMatrix = {
				   new float[] { (float)this.sRR_B.Value / 100, (float)this.sRG_B.Value / 100, (float)this.sRB_B.Value / 100,  0, 1},        // red scaling factor of 2
				   new float[] { (float)this.sGR_B.Value / 100, (float)this.sGG_B.Value / 100, (float)this.sGB_B.Value / 100,  0, 1},        // green scaling factor of 1
				   new float[] { (float)this.sBR_B.Value / 100, (float)this.sBG_B.Value / 100, (float)this.sBB_B.Value / 100,  0, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
				};
				PanelColorMatrix = _ColorMatrix;
			}
			if (_PortraitMode == 1)
			{
				float[][] _ColorMatrix = {
				   new float[] { (float)this.sRR.Value / 100, (float)this.sRG.Value / 100, (float)this.sRB.Value / 100,  0, 1},        // red scaling factor of 2
				   new float[] { (float)this.sGR.Value / 100, (float)this.sGG.Value / 100, (float)this.sGB.Value / 100,  0, 1},        // green scaling factor of 1
				   new float[] { (float)this.sBR.Value / 100, (float)this.sBG.Value / 100, (float)this.sBB.Value / 100,  0, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
				};
				PanelColorMatrix = _ColorMatrix;
			}

			_BaseLayer = this.pictureBox1.Image;

			_Layer_1 = (Image)StationPortraitImage.Clone();
			_Layer_1 = ApplyColorMatrix(_Layer_1, PanelColorMatrix);
			_Layer_1 = ApplyColorMatrix(_Layer_1, PortraitColorMatrix);

			_Layer_1 = Util.SetBrightness(_Layer_1, (float)_PortraitBrightness);
			_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1), 0);

			this.pictureBox1.Image = _Layer_1;
			this.pictureBox1.Refresh();
		}

		private void DrawPicture_LOW()
		{
			try
			{
				this.pictureBox2.Image = null;
				this.pictureBox2.Refresh();

				Image image = (Image)this.ShipPanelOriginalImage.Clone();

				decimal _Brightness = (decimal)this.trackShipPanelsBrightness.Value / 100;

				ImageAttributes imageAttributes = new ImageAttributes();
				int width = image.Width;
				int height = image.Height;

				float[][] colorMatrixElements = {
				   new float[] { (float)this.sRR_B.Value / 100, (float)this.sRG_B.Value / 100, (float)this.sRB_B.Value / 100,  1, 1},        // red scaling factor of 2
				   new float[] { (float)this.sGR_B.Value / 100, (float)this.sGG_B.Value / 100, (float)this.sGB_B.Value / 100,  1, 1},        // green scaling factor of 1
				   new float[] { (float)this.sBR_B.Value / 100, (float)this.sBG_B.Value / 100, (float)this.sBB_B.Value / 100,  1, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
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

				image = Util.SetBrightness(image, (float)_Brightness);

				this.pictureBox2.Image = image;
				this.pictureBox2.Refresh();
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		private void DrawPortrait_LOW()
		{
			Image _BaseLayer = null;
			Image _Layer_1 = null;

			float[][] PanelColorMatrix = null;

			decimal _PortraitMode = Convert.ToDecimal(this.cboPortrasitModel.EditValue);
			decimal _PortraitBrightness = (decimal)this.trackPortraitBrightness.Value / 100;

			float[][] PortraitColorMatrix = {
				   new float[] { (float)this.invRR.Value, (float)this.invRG.Value, (float)this.invRB.Value,  0, 1},        // red scaling factor of 2
				   new float[] { (float)this.invGR.Value, (float)this.invGG.Value, (float)this.invGB.Value,  0, 1},        // green scaling factor of 1
				   new float[] { (float)this.invBR.Value, (float)this.invBG.Value, (float)this.invBB.Value,  0, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
			};
			if (_PortraitMode == 0)
			{
				float[][] _ColorMatrix = {
				   new float[] { (float)this.sRR_B.Value / 100, (float)this.sRG_B.Value / 100, (float)this.sRB_B.Value / 100,  0, 1},        // red scaling factor of 2
				   new float[] { (float)this.sGR_B.Value / 100, (float)this.sGG_B.Value / 100, (float)this.sGB_B.Value / 100,  0, 1},        // green scaling factor of 1
				   new float[] { (float)this.sBR_B.Value / 100, (float)this.sBG_B.Value / 100, (float)this.sBB_B.Value / 100,  0, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
				};
				PanelColorMatrix = _ColorMatrix;
			}
			if (_PortraitMode == 1)
			{
				float[][] _ColorMatrix = {
				   new float[] { (float)this.sRR.Value / 100, (float)this.sRG.Value / 100, (float)this.sRB.Value / 100,  0, 1},        // red scaling factor of 2
				   new float[] { (float)this.sGR.Value / 100, (float)this.sGG.Value / 100, (float)this.sGB.Value / 100,  0, 1},        // green scaling factor of 1
				   new float[] { (float)this.sBR.Value / 100, (float)this.sBG.Value / 100, (float)this.sBB.Value / 100,  0, 1},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 0f}			// three translations of 0.2
				};
				PanelColorMatrix = _ColorMatrix;
			}

			_BaseLayer = this.pictureBox2.Image;

			_Layer_1 = (Image)ShipPanelPortraitImage.Clone();
			_Layer_1 = ApplyColorMatrix(_Layer_1, PanelColorMatrix);
			_Layer_1 = ApplyColorMatrix(_Layer_1, PortraitColorMatrix);

			_Layer_1 = Util.SetBrightness(_Layer_1, (float)_PortraitBrightness);
			_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1), 0);

			this.pictureBox2.Image = _Layer_1;
			this.pictureBox2.Refresh();
		}

		private Image ApplyColorMatrix(Image _Image, float[][] colorMatrix)
		{
			Image _ret = null;
			try
			{
				ImageAttributes imageAttributes = new ImageAttributes();
				int width = _Image.Width;
				int height = _Image.Height;

				ColorMatrix ImageMatrix = new ColorMatrix(colorMatrix);
				imageAttributes.SetColorMatrix(ImageMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				using (System.Drawing.Graphics G = Graphics.FromImage(_Image))
				{
					G.DrawImage(
					   _Image,
					   new Rectangle(0, 0, width, height),  // destination rectangle
					   0, 0,        // upper-left corner of source rectangle
					   width,       // width of source rectangle
					   height,      // height of source rectangle
					   GraphicsUnit.Pixel,
					   imageAttributes);
				}
				_ret = _Image;
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
			return _ret;
		}

		private void sRR_EditValueChanged(object sender, EventArgs e)
		{
			TrackBarControl _Slider = sender as TrackBarControl;
			if (_Slider != null)
			{
				DevExpress.XtraLayout.LayoutControl _Parent = _Slider.Parent as DevExpress.XtraLayout.LayoutControl;
				var X = (_Parent.GetItemByControl(_Slider)).Text = ((decimal)_Slider.Value / 100).ToString();
			}
		}

		private void sBB_MouseUp(object sender, MouseEventArgs e)
		{
			MakeXML_TOP();
			CalculateInverseValues();
			DrawPicture_TOP();
			DrawPortrait_TOP();
		}

		private void sRR_KeyUp(object sender, KeyEventArgs e)
		{
			MakeXML_TOP();
			CalculateInverseValues();
			DrawPicture_TOP();
			DrawPortrait_TOP();
		}

		private void sRR_B_MouseUp(object sender, MouseEventArgs e)
		{
			MakeXML_LOW();
			DrawPortrait_TOP();
			DrawPicture_LOW();
			DrawPortrait_LOW();
		}

		private void sRR_B_KeyUp(object sender, KeyEventArgs e)
		{
			MakeXML_LOW();
			DrawPortrait_TOP();
			DrawPicture_LOW();
			DrawPortrait_LOW();
		}

		private void invRR_MouseUp(object sender, MouseEventArgs e)
		{
			DrawPortrait_TOP();
			DrawPortrait_LOW();
		}

		private void invRG_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				DrawPortrait_TOP();
				DrawPortrait_LOW();
			}
		}

		private void cmdImportXML_1_Click(object sender, EventArgs e)
		{
			ImportXML_TOP(this.txtXML.Text);
			CalculateInverseValues();
			DrawPicture_TOP();
			DrawPortrait_TOP();
			DrawPortrait_LOW();
		}

		private void cmdImportXML_2_Click(object sender, EventArgs e)
		{
			ImportXML_LOW(this.txtXML_LOW.Text);
			DrawPicture_LOW();
			DrawPortrait_LOW();
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{
			try
			{
				if (File.Exists(Path.Combine(AppExePath, @"XML-Profile.ini")))
				{
					this._Reader = new IniFile(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_XML-Profile.ini"));

					if (this._Reader != null)
					{
						this._Reader.WriteKey("x124", Convert.ToInt32(cboPortrasitModel.EditValue).ToString(), "constants");

						this._Reader.WriteKey("w153", ((decimal)trackPortraitBrightness.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z157", ((decimal)trackShipPanelsBrightness.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("w157", ((decimal)trackHiglightsBrightness.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x158", ((decimal)sRR.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y158", ((decimal)sRG.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z158", ((decimal)sRB.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x159", ((decimal)sGR.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y159", ((decimal)sGG.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z159", ((decimal)sGB.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x160", ((decimal)sBR.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y160", ((decimal)sBG.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z160", ((decimal)sBB.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x153", invRR.Value.ToString(), "constants");
						this._Reader.WriteKey("y153", invRG.Value.ToString(), "constants");
						this._Reader.WriteKey("z153", invRB.Value.ToString(), "constants");

						this._Reader.WriteKey("x154", invGR.Value.ToString(), "constants");
						this._Reader.WriteKey("y154", invGG.Value.ToString(), "constants");
						this._Reader.WriteKey("z154", invGB.Value.ToString(), "constants");

						this._Reader.WriteKey("x155", invBR.Value.ToString(), "constants");
						this._Reader.WriteKey("y155", invBG.Value.ToString(), "constants");
						this._Reader.WriteKey("z155", invBB.Value.ToString(), "constants");

						this._Reader.WriteKey("x150", ((decimal)sRR_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y150", ((decimal)sRG_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z150", ((decimal)sRB_B.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x151", ((decimal)sGR_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y151", ((decimal)sGG_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z151", ((decimal)sGB_B.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x152", ((decimal)sBR_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y152", ((decimal)sBG_B.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z152", ((decimal)sBB_B.Value / 100).ToString(), "constants");

						this.Close();
					}
					else
					{
						throw new Exception("ERROR 404: Could not load the 'XML - Profile.ini' file!");
					}
				}
				else
				{
					throw new Exception("ERROR 404: Could not find the 'XML - Profile.ini' file!");
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private double[][] MatrixCreate(int rows, int cols)
		{
			double[][] result = new double[rows][];
			for (int i = 0; i < rows; ++i)
				result[i] = new double[cols];
			return result;
		}

		private double[][] MatrixIdentity(int n)
		{
			// return an n x n Identity matrix
			double[][] result = MatrixCreate(n, n);
			for (int i = 0; i < n; ++i)
				result[i][i] = 1.0;

			return result;
		}

		private double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
		{
			int aRows = matrixA.Length; int aCols = matrixA[0].Length;
			int bRows = matrixB.Length; int bCols = matrixB[0].Length;
			if (aCols != bRows)
				throw new Exception("Non-conformable matrices in MatrixProduct");

			double[][] result = MatrixCreate(aRows, bCols);

			for (int i = 0; i < aRows; ++i) // each row of A
				for (int j = 0; j < bCols; ++j) // each col of B
					for (int k = 0; k < aCols; ++k) // could use k less-than bRows
						result[i][j] += matrixA[i][k] * matrixB[k][j];

			return result;
		}

		private double[][] MatrixInverse(double[][] matrix)
		{
			int n = matrix.Length;
			double[][] result = MatrixDuplicate(matrix);

			int[] perm;
			int toggle;
			double[][] lum = MatrixDecompose(matrix, out perm,
			  out toggle);
			if (lum == null)
				throw new Exception("Unable to compute inverse");

			double[] b = new double[n];
			for (int i = 0; i < n; ++i)
			{
				for (int j = 0; j < n; ++j)
				{
					if (i == perm[j])
						b[j] = 1.0;
					else
						b[j] = 0.0;
				}

				double[] x = HelperSolve(lum, b);

				for (int j = 0; j < n; ++j)
					result[j][i] = x[j];
			}
			return result;
		}

		private double[][] MatrixDuplicate(double[][] matrix)
		{
			// allocates/creates a duplicate of a matrix.
			double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
			for (int i = 0; i < matrix.Length; ++i) // copy the values
				for (int j = 0; j < matrix[i].Length; ++j)
					result[i][j] = matrix[i][j];
			return result;
		}

		private double[] HelperSolve(double[][] luMatrix, double[] b)
		{
			// before calling this helper, permute b using the perm array
			// from MatrixDecompose that generated luMatrix
			int n = luMatrix.Length;
			double[] x = new double[n];
			b.CopyTo(x, 0);

			for (int i = 1; i < n; ++i)
			{
				double sum = x[i];
				for (int j = 0; j < i; ++j)
					sum -= luMatrix[i][j] * x[j];
				x[i] = sum;
			}

			x[n - 1] /= luMatrix[n - 1][n - 1];
			for (int i = n - 2; i >= 0; --i)
			{
				double sum = x[i];
				for (int j = i + 1; j < n; ++j)
					sum -= luMatrix[i][j] * x[j];
				x[i] = sum / luMatrix[i][i];
			}

			return x;
		}

		private double[][] MatrixDecompose(double[][] matrix, out int[] perm, out int toggle)
		{
			// Doolittle LUP decomposition with partial pivoting.
			// rerturns: result is L (with 1s on diagonal) and U;
			// perm holds row permutations; toggle is +1 or -1 (even or odd)
			int rows = matrix.Length;
			int cols = matrix[0].Length; // assume square
			if (rows != cols)
				throw new Exception("Attempt to decompose a non-square m");

			int n = rows; // convenience

			double[][] result = MatrixDuplicate(matrix);

			perm = new int[n]; // set up row permutation result
			for (int i = 0; i < n; ++i) { perm[i] = i; }

			toggle = 1; // toggle tracks row swaps.
						// +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

			for (int j = 0; j < n - 1; ++j) // each column
			{
				double colMax = Math.Abs(result[j][j]); // find largest val in col
				int pRow = j;
				//for (int i = j + 1; i less-than n; ++i)
				//{
				//  if (result[i][j] greater-than colMax)
				//  {
				//    colMax = result[i][j];
				//    pRow = i;
				//  }
				//}

				// reader Matt V needed this:
				for (int i = j + 1; i < n; ++i)
				{
					if (Math.Abs(result[i][j]) > colMax)
					{
						colMax = Math.Abs(result[i][j]);
						pRow = i;
					}
				}
				// Not sure if this approach is needed always, or not.

				if (pRow != j) // if largest value not on pivot, swap rows
				{
					double[] rowPtr = result[pRow];
					result[pRow] = result[j];
					result[j] = rowPtr;

					int tmp = perm[pRow]; // and swap perm info
					perm[pRow] = perm[j];
					perm[j] = tmp;

					toggle = -toggle; // adjust the row-swap toggle
				}

				// --------------------------------------------------
				// This part added later (not in original)
				// and replaces the 'return null' below.
				// if there is a 0 on the diagonal, find a good row
				// from i = j+1 down that doesn't have
				// a 0 in column j, and swap that good row with row j
				// --------------------------------------------------

				if (result[j][j] == 0.0)
				{
					// find a good row to swap
					int goodRow = -1;
					for (int row = j + 1; row < n; ++row)
					{
						if (result[row][j] != 0.0)
							goodRow = row;
					}

					if (goodRow == -1) goodRow = 1;
					//	throw new Exception("Cannot use Doolittle's method");

					// swap rows so 0.0 no longer on diagonal
					double[] rowPtr = result[goodRow];
					result[goodRow] = result[j];
					result[j] = rowPtr;

					int tmp = perm[goodRow]; // and swap perm info
					perm[goodRow] = perm[j];
					perm[j] = tmp;

					toggle = -toggle; // adjust the row-swap toggle
				}
				// --------------------------------------------------
				// if diagonal after swap is zero . .
				//if (Math.Abs(result[j][j]) less-than 1.0E-20)
				//  return null; // consider a throw

				for (int i = j + 1; i < n; ++i)
				{
					result[i][j] /= result[j][j];
					for (int k = j + 1; k < n; ++k)
					{
						result[i][k] -= result[i][j] * result[j][k];
					}
				}
			} // main j column loop

			return result;
		}

		private void trackPortraitBrightness_EditValueChanged(object sender, EventArgs e)
		{
			TrackBarControl _Slider = sender as TrackBarControl;
			if (_Slider != null)
			{
				DevExpress.XtraLayout.LayoutControl _Parent = _Slider.Parent as DevExpress.XtraLayout.LayoutControl;
				var X = (_Parent.GetItemByControl(_Slider)).Text = string.Format("Brightness: [{0}]", ((decimal)_Slider.Value / 100).ToString());
			}
		}

		private void trackPortraitBrightness_MouseUp(object sender, MouseEventArgs e)
		{
			//Ajusta el Brillo del Portrait
			DrawPortrait_TOP();
		}

		private void trackPortraitBrightness_KeyUp(object sender, KeyEventArgs e)
		{
			//Ajusta el Brillo del Portrait
			DrawPortrait_TOP();
		}

		private void trackShipPanelsBrightness_EditValueChanged(object sender, EventArgs e)
		{
			TrackBarControl _Slider = sender as TrackBarControl;
			if (_Slider != null)
			{
				DevExpress.XtraLayout.LayoutControl _Parent = _Slider.Parent as DevExpress.XtraLayout.LayoutControl;
				var X = (_Parent.GetItemByControl(_Slider)).Text = string.Format("Brightness: [{0}]", ((decimal)_Slider.Value / 100).ToString());
			}
		}

		private void trackShipPanelsBrightness_KeyUp(object sender, KeyEventArgs e)
		{
			//Ajustar el Brillo del Panel
			DrawPicture_LOW();
		}

		private void trackShipPanelsBrightness_MouseUp(object sender, MouseEventArgs e)
		{
			DrawPicture_LOW();
		}

		private void trackHiglightsBrightness_EditValueChanged(object sender, EventArgs e)
		{
			TrackBarControl _Slider = sender as TrackBarControl;
			if (_Slider != null)
			{
				DevExpress.XtraLayout.LayoutControl _Parent = _Slider.Parent as DevExpress.XtraLayout.LayoutControl;
				var X = (_Parent.GetItemByControl(_Slider)).Text = string.Format("Highlights: [{0}]", ((decimal)_Slider.Value / 100).ToString());
			}
		}

		private void cboPortrasitModel_EditValueChanged(object sender, EventArgs e)
		{
			DrawPortrait_TOP();
		}

		private void cmdXMLWebColors_Click(object sender, EventArgs e)
		{
			//abre con el programa predeterminado:
			System.Diagnostics.Process.Start("https://forums.frontier.co.uk/threads/no2o-the-definitive-list-of-1-7-2-2-compatible-hud-colour-color-configs-please-add-yours.259311/");
		}
	}
}