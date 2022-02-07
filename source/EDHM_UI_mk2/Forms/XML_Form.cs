using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace EDHM_UI_mk2.Forms
{
	public partial class XML_Form : DevExpress.XtraEditors.XtraForm
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

		public XML_Form(game_instance _ActiveInstance, List<value_key> _Xml_profile)
		{
			InitializeComponent();
			this.LookAndFeel.TouchUIMode = DevExpress.Utils.DefaultBoolean.False;
			this.LookAndFeel.TouchScaleFactor = 1;
			this.ActiveInstance = _ActiveInstance;
			this.xml_profile = _Xml_profile;
		}

		private void XML_Form_Load(object sender, EventArgs e)
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

			this.PortraitMainImage = Image.FromFile(Path.Combine(ImagesPath, @"ColorMatrix_Preview_3a.png"));

			this.picStationPanels.Image = this.StationOriginalImage;
			this.picShipPanels.Image = this.ShipPanelOriginalImage;
			this.picPortraits.Image = this.PortraitMainImage;


			List<combo_item> _OnOff = new List<combo_item>();
			_OnOff.Add(new combo_item("ONOFF", "Use the Ship Panel XML", 0));
			_OnOff.Add(new combo_item("ONOFF", "Use the Station panel XML", 1));

			this.cboPortrasitModel.Properties.ValueMember = "Index";
			this.cboPortrasitModel.Properties.DisplayMember = "Name";
			this.cboPortrasitModel.Properties.DataSource = _OnOff;
		}
		private void XML_Form_Shown(object sender, EventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

				LoadINIProfile();

				DrawPicture_TOP();
				DrawPortrait_TOP();

				DrawPicture_LOW();
				DrawPortrait_LOW();
			});
		}

		private void LoadINIProfile()
		{
			try
			{
				//this._Reader = new IniFile(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_XML-Profile.ini"));  //<- Open and Read the INI file
				//this._Reader = new IniFile(Path.Combine(this.ActiveTheme.folder, "XML-Profile.ini"));

				if (true)
				{
					StringBuilder _XML_Matrix = new StringBuilder();

					//** CONFIGS:
					Invoke((MethodInvoker)(() =>
					{
						this.cboPortrasitModel.EditValue =			 this.xml_profile.Find(XML => XML.key == "x124").value; // Convert.ToDecimal(this._Reader.ReadKey("x124", "constants").NVL("100"));
						this.trackPortraitBrightness.Value =	(int)this.xml_profile.Find(XML => XML.key == "w153").value * 100; // Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("w153", "constants")) * 100);
						this.trackShipPanelsBrightness.Value =	(int)this.xml_profile.Find(XML => XML.key == "z157").value * 100; // Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("z157", "constants")) * 100);
						this.trackHiglightsBrightness.Value =	(int)this.xml_profile.Find(XML => XML.key == "w157").value * 100; // Convert.ToInt32(Convert.ToDecimal(this._Reader.ReadKey("w157", "constants")) * 100);
					}));

					//-----------------------------------------------------------------------------------;
					decimal tRR, tRG, tRB = 0;
					decimal tGR, tGG, tGB = 0;
					decimal tBR, tBG, tBB = 0;

					//** --== XML Station-Carrier Profile ==--
					tRR = this.xml_profile.Find(XML => XML.key == "x158").value;  // Convert.ToDecimal(this._Reader.ReadKey("x158", "constants"));
					tRG = this.xml_profile.Find(XML => XML.key == "y158").value;  // Convert.ToDecimal(this._Reader.ReadKey("y158", "constants"));
					tRB = this.xml_profile.Find(XML => XML.key == "z158").value;  // Convert.ToDecimal(this._Reader.ReadKey("z158", "constants"));

					tGR = this.xml_profile.Find(XML => XML.key == "x159").value;  // Convert.ToDecimal(this._Reader.ReadKey("x159", "constants"));
					tGG = this.xml_profile.Find(XML => XML.key == "y159").value;  // Convert.ToDecimal(this._Reader.ReadKey("y159", "constants"));
					tGB = this.xml_profile.Find(XML => XML.key == "z159").value;  // Convert.ToDecimal(this._Reader.ReadKey("z159", "constants"));

					tBR = this.xml_profile.Find(XML => XML.key == "x160").value;  // Convert.ToDecimal(this._Reader.ReadKey("x160", "constants"));
					tBG = this.xml_profile.Find(XML => XML.key == "y160").value;  // Convert.ToDecimal(this._Reader.ReadKey("y160", "constants"));
					tBB = this.xml_profile.Find(XML => XML.key == "z160").value;  // Convert.ToDecimal(this._Reader.ReadKey("z160", "constants"));

					_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", tRR, tRG, tRB));
					_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", tGR, tGG, tGB));
					_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", tBR, tBG, tBB));

					Invoke((MethodInvoker)(() =>
					{
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
					}));

					//-----------------------------------------------------------------------------------;
					//**   --== XML Portrait Fix ==--

					decimal mRR, mRG, mRB = 0;
					decimal mGR, mGG, mGB = 0;
					decimal mBR, mBG, mBB = 0;

					mRR = this.xml_profile.Find(XML => XML.key == "x153").value; //  Convert.ToDecimal(this._Reader.ReadKey("x153", "constants"));
					mRG = this.xml_profile.Find(XML => XML.key == "y153").value; //  Convert.ToDecimal(this._Reader.ReadKey("y153", "constants"));
					mRB = this.xml_profile.Find(XML => XML.key == "z153").value; //  Convert.ToDecimal(this._Reader.ReadKey("z153", "constants"));

					mGR = this.xml_profile.Find(XML => XML.key == "x154").value; //  Convert.ToDecimal(this._Reader.ReadKey("x154", "constants"));
					mGG = this.xml_profile.Find(XML => XML.key == "y154").value; //  Convert.ToDecimal(this._Reader.ReadKey("y154", "constants"));
					mGB = this.xml_profile.Find(XML => XML.key == "z154").value; //  Convert.ToDecimal(this._Reader.ReadKey("z154", "constants"));

					mBR = this.xml_profile.Find(XML => XML.key == "x155").value; //  Convert.ToDecimal(this._Reader.ReadKey("x155", "constants"));
					mBG = this.xml_profile.Find(XML => XML.key == "y155").value; //  Convert.ToDecimal(this._Reader.ReadKey("y155", "constants"));
					mBB = this.xml_profile.Find(XML => XML.key == "z155").value; //  Convert.ToDecimal(this._Reader.ReadKey("z155", "constants"));

					Invoke((MethodInvoker)(() =>
					{
						this.invRR.Value = mRR;
						this.invRG.Value = mRG;
						this.invRB.Value = mRB;

						this.invGR.Value = mGR;
						this.invGG.Value = mGG;
						this.invGB.Value = mGB;

						this.invBR.Value = mBR;
						this.invBG.Value = mBG;
						this.invBB.Value = mBB;
					}));

					//-----------------------------------------------------------------------------------;
					//** --== XML Ship Profile ==--

					decimal bRR, bRG, bRB = 0;
					decimal bGR, bGG, bGB = 0;
					decimal bBR, bBG, bBB = 0;

					bRR = this.xml_profile.Find(XML => XML.key == "x150").value; //  Convert.ToDecimal(this._Reader.ReadKey("x150", "constants"));
					bRG = this.xml_profile.Find(XML => XML.key == "y150").value; //  Convert.ToDecimal(this._Reader.ReadKey("y150", "constants"));
					bRB = this.xml_profile.Find(XML => XML.key == "z150").value; //  Convert.ToDecimal(this._Reader.ReadKey("z150", "constants"));

					bGR = this.xml_profile.Find(XML => XML.key == "x151").value; //  Convert.ToDecimal(this._Reader.ReadKey("x151", "constants"));
					bGG = this.xml_profile.Find(XML => XML.key == "y151").value; //  Convert.ToDecimal(this._Reader.ReadKey("y151", "constants"));
					bGB = this.xml_profile.Find(XML => XML.key == "z151").value; //  Convert.ToDecimal(this._Reader.ReadKey("z151", "constants"));

					bBR = this.xml_profile.Find(XML => XML.key == "x152").value; //  Convert.ToDecimal(this._Reader.ReadKey("x152", "constants"));
					bBG = this.xml_profile.Find(XML => XML.key == "y152").value; //  Convert.ToDecimal(this._Reader.ReadKey("y152", "constants"));
					bBB = this.xml_profile.Find(XML => XML.key == "z152").value; //  Convert.ToDecimal(this._Reader.ReadKey("z152", "constants"));

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
					Invoke((MethodInvoker)(() =>
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
					}));
				}

				if (!this.toggleLock_LOW.IsOn)
				{
					Invoke((MethodInvoker)(() =>
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
					}));

					MakeXML_LOW();
					DrawPicture_LOW();
					DrawPortrait_LOW();
				}
			}
			catch (Exception ex)
			{
				Invoke((MethodInvoker)(() =>
				{
					XtraMessageBox.Show(ex.Message);
				}));
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

			Invoke((MethodInvoker)(() =>
			{
				this.txtXML.Text = _XML_Matrix.ToString();
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

						Invoke((MethodInvoker)(() =>
						{
							this.sRR.Value = Convert.ToInt32(RR * 100);
							this.sRG.Value = Convert.ToInt32(RG * 100);
							this.sRB.Value = Convert.ToInt32(RB * 100);
						}));
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

						Invoke((MethodInvoker)(() =>
						{
							this.sGR.Value = Convert.ToInt32(GR * 100);
							this.sGG.Value = Convert.ToInt32(GG * 100);
							this.sGB.Value = Convert.ToInt32(GB * 100);
						}));
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

						Invoke((MethodInvoker)(() =>
						{
							this.sBR.Value = Convert.ToInt32(BR * 100);
							this.sBG.Value = Convert.ToInt32(BG * 100);
							this.sBB.Value = Convert.ToInt32(BB * 100);
						}));
					}
				}
			}
			catch (Exception ex)
			{
				Invoke((MethodInvoker)(() =>
				{

				}));
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

		/* Station Panel Image  */
		private void DrawPicture_TOP()
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

				Invoke((MethodInvoker)(() =>
				{
					this.picStationPanels.Image = image;
					this.picStationPanels.Refresh();
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
		private void DrawPortrait_TOP()
		{
			Image _BaseLayer = null;
			Image _Layer_1 = null;
			Image _Layer_2 = null;

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

			_BaseLayer = this.picStationPanels.Image;

			_Layer_1 = (Image)this.StationPortraitImage.Clone();
			_Layer_1 = Util.ApplyColorMatrix(_Layer_1, PanelColorMatrix);
			_Layer_1 = Util.ApplyColorMatrix(_Layer_1, PortraitColorMatrix);

			_Layer_1 = Util.SetBrightness(_Layer_1, (float)_PortraitBrightness);
			_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1), 0);

			_Layer_2 = (Image)this.PortraitMainImage.Clone();
			_Layer_2 = Util.ApplyColorMatrix(_Layer_2, PanelColorMatrix);
			_Layer_2 = Util.ApplyColorMatrix(_Layer_2, PortraitColorMatrix);
			_Layer_2 = Util.SetBrightness(_Layer_2, (float)_PortraitBrightness);

			Invoke((MethodInvoker)(() =>
			{
				this.picStationPanels.Image = _Layer_1;
				this.picStationPanels.Refresh();

				this.picPortraits.Image = _Layer_2;
				this.picPortraits.Refresh();
			}));
		}

		/* Ship Panels Image  */
		private void DrawPicture_LOW()
		{
			try
			{
				Invoke((MethodInvoker)(() =>
				{
					this.picShipPanels.Image = null;
					this.picShipPanels.Refresh();
				}));

				Image image = (Image)this.ShipPanelOriginalImage.Clone();
				decimal _Brightness = (decimal)this.trackShipPanelsBrightness.Value / 100;

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
				image = Util.SetBrightness(image, (float)_Brightness);				

				Invoke((MethodInvoker)(() =>
				{
					this.picShipPanels.Image = image;
					this.picShipPanels.Refresh();
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
		private void DrawPortrait_LOW()
		{
			Image _BaseLayer = null;
			Image _Layer_1 = null;

			float[][] PanelColorMatrix = null;

			decimal _PortraitMode = Convert.ToDecimal(this.cboPortrasitModel.EditValue);
			decimal _PortraitBrightness = (decimal)this.trackPortraitBrightness.Value / 100;

			float[][] PortraitColorMatrix = {
				   new float[] { (float)this.invRR.Value, (float)this.invRG.Value, (float)this.invRB.Value,  0, 0},
				   new float[] { (float)this.invGR.Value, (float)this.invGG.Value, (float)this.invGB.Value,  0, 0},
				   new float[] { (float)this.invBR.Value, (float)this.invBG.Value, (float)this.invBB.Value,  0, 0},
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 1f}			// three translations of 0.2
			};
			if (_PortraitMode == 0)
			{
				float[][] _ColorMatrix = {
				   new float[] { (float)this.sRR_B.Value / 100, (float)this.sRG_B.Value / 100, (float)this.sRB_B.Value / 100,  0, 0},        // red scaling factor of 2
				   new float[] { (float)this.sGR_B.Value / 100, (float)this.sGG_B.Value / 100, (float)this.sGB_B.Value / 100,  0, 0},        // green scaling factor of 1
				   new float[] { (float)this.sBR_B.Value / 100, (float)this.sBG_B.Value / 100, (float)this.sBB_B.Value / 100,  0, 0},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 1f}			// three translations of 0.2
				};
				PanelColorMatrix = _ColorMatrix;
			}
			if (_PortraitMode == 1)
			{
				float[][] _ColorMatrix = {
				   new float[] { (float)this.sRR.Value / 100, (float)this.sRG.Value / 100, (float)this.sRB.Value / 100,  0, 0},        // red scaling factor of 2
				   new float[] { (float)this.sGR.Value / 100, (float)this.sGG.Value / 100, (float)this.sGB.Value / 100,  0, 0},        // green scaling factor of 1
				   new float[] { (float)this.sBR.Value / 100, (float)this.sBG.Value / 100, (float)this.sBB.Value / 100,  0,0},        // blue scaling factor of 1
				   new float[] {0,  0,  0,  1, 0},			// alpha scaling factor of 1
				   new float[] {0f, 0f, 0f, 0f, 1f}			// three translations of 0.2
				};
				PanelColorMatrix = _ColorMatrix;
			}

			_BaseLayer = this.picShipPanels.Image;

			_Layer_1 = (Image)this.ShipPanelPortraitImage.Clone();
			_Layer_1 = Util.ApplyColorMatrix(_Layer_1, PanelColorMatrix);
			_Layer_1 = Util.ApplyColorMatrix(_Layer_1, PortraitColorMatrix);

			_Layer_1 = Util.SetBrightness(_Layer_1, (float)_PortraitBrightness);
			_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1), 0);

			Invoke((MethodInvoker)(() =>
			{
				this.picShipPanels.Image = _Layer_1;
				this.picShipPanels.Refresh();
			}));
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
		private void sRR_KeyUp(object sender, KeyEventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				MakeXML_LOW();
				DrawPortrait_TOP();
				DrawPicture_LOW();
				DrawPortrait_LOW();
			});
		}
		private void sBR_MouseUp(object sender, MouseEventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				MakeXML_LOW();
				DrawPortrait_TOP();
				DrawPicture_LOW();
				DrawPortrait_LOW();
			});
		}
		private void sRR_B_MouseUp(object sender, MouseEventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				MakeXML_TOP();
				CalculateInverseValues();
				DrawPicture_TOP();
				DrawPortrait_TOP();
			});
		}
		private void sRR_B_KeyUp(object sender, KeyEventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				MakeXML_TOP();
				CalculateInverseValues();
				DrawPicture_TOP();
				DrawPortrait_TOP();
			});
		}

		private void invRR_MouseUp(object sender, MouseEventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
				DrawPortrait_TOP();
				DrawPortrait_LOW();
			});
		}
		private void invRG_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
					DrawPortrait_TOP();
					DrawPortrait_LOW();
				});
			}
		}


		private void cmdSaveExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (this.xml_profile.IsNotEmpty())
				{
					this.xml_profile.Find(X => X.key == "x124").value = Convert.ToInt32(this.cboPortrasitModel.EditValue);

					this.xml_profile.Find(X => X.key == "w153").value = ((decimal)this.trackPortraitBrightness.Value / 100);
					this.xml_profile.Find(X => X.key == "z157").value = ((decimal)this.trackShipPanelsBrightness.Value / 100);
					this.xml_profile.Find(X => X.key == "w157").value = ((decimal)this.trackHiglightsBrightness.Value / 100);

					this.xml_profile.Find(X => X.key == "x158").value = ((decimal)this.sRR.Value / 100);
					this.xml_profile.Find(X => X.key == "y158").value = ((decimal)this.sRG.Value / 100);
					this.xml_profile.Find(X => X.key == "z158").value = ((decimal)this.sRB.Value / 100);

					this.xml_profile.Find(X => X.key == "x159").value = ((decimal)this.sGR.Value / 100);
					this.xml_profile.Find(X => X.key == "y159").value = ((decimal)this.sGG.Value / 100);
					this.xml_profile.Find(X => X.key == "z159").value = ((decimal)this.sGB.Value / 100);

					this.xml_profile.Find(X => X.key == "x160").value = ((decimal)this.sBR.Value / 100);
					this.xml_profile.Find(X => X.key == "y160").value = ((decimal)this.sBG.Value / 100);
					this.xml_profile.Find(X => X.key == "z160").value = ((decimal)this.sBB.Value / 100);

					this.xml_profile.Find(X => X.key == "x153").value = this.invRR.Value;
					this.xml_profile.Find(X => X.key == "y153").value = this.invRG.Value;
					this.xml_profile.Find(X => X.key == "z153").value = this.invRB.Value;

					this.xml_profile.Find(X => X.key == "x154").value = this.invGR.Value;
					this.xml_profile.Find(X => X.key == "y154").value = this.invGG.Value;
					this.xml_profile.Find(X => X.key == "z154").value = this.invGB.Value;

					this.xml_profile.Find(X => X.key == "x155").value = this.invBR.Value;
					this.xml_profile.Find(X => X.key == "y155").value = this.invBG.Value;
					this.xml_profile.Find(X => X.key == "z155").value = this.invBB.Value;


					this.xml_profile.Find(X => X.key == "x150").value = ((decimal)this.sRR_B.Value / 100);
					this.xml_profile.Find(X => X.key == "y150").value = ((decimal)this.sRG_B.Value / 100);
					this.xml_profile.Find(X => X.key == "z150").value = ((decimal)this.sRB_B.Value / 100);

					this.xml_profile.Find(X => X.key == "x151").value = ((decimal)this.sGR_B.Value / 100);
					this.xml_profile.Find(X => X.key == "y151").value = ((decimal)this.sGG_B.Value / 100);
					this.xml_profile.Find(X => X.key == "z151").value = ((decimal)this.sGB_B.Value / 100);

					this.xml_profile.Find(X => X.key == "x152").value = ((decimal)this.sBR_B.Value / 100);
					this.xml_profile.Find(X => X.key == "y152").value = ((decimal)this.sBG_B.Value / 100);
					this.xml_profile.Find(X => X.key == "z152").value = ((decimal)this.sBB_B.Value / 100);

					Close();
				}

				/*
				//string INIpath = Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_XML-Profile.ini");
				string INIpath = Path.Combine(this.ActiveInstance.path, "EDHM-ini", "XML-Profile.ini");
				if (File.Exists(INIpath))
				{
					this._Reader = new IniFile(INIpath);

					if (this._Reader != null)
					{
						this._Reader.WriteKey("x124", Convert.ToInt32(this.cboPortrasitModel.EditValue).ToString(), "constants");

						this._Reader.WriteKey("w153", ((decimal)this.trackPortraitBrightness.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z157", ((decimal)this.trackShipPanelsBrightness.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("w157", ((decimal)this.trackHiglightsBrightness.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x158", ((decimal)this.sRR.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y158", ((decimal)this.sRG.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z158", ((decimal)this.sRB.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x159", ((decimal)this.sGR.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y159", ((decimal)this.sGG.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z159", ((decimal)this.sGB.Value / 100).ToString(), "constants");

						this._Reader.WriteKey("x160", ((decimal)this.sBR.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("y160", ((decimal)this.sBG.Value / 100).ToString(), "constants");
						this._Reader.WriteKey("z160", ((decimal)this.sBB.Value / 100).ToString(), "constants");


						this._Reader.WriteKey("x153", this.invRR.Value.ToString(), "constants");
						this._Reader.WriteKey("y153", this.invRG.Value.ToString(), "constants");
						this._Reader.WriteKey("z153", this.invRB.Value.ToString(), "constants");

						this._Reader.WriteKey("x154", this.invGR.Value.ToString(), "constants");
						this._Reader.WriteKey("y154", this.invGG.Value.ToString(), "constants");
						this._Reader.WriteKey("z154", this.invGB.Value.ToString(), "constants");

						this._Reader.WriteKey("x155", this.invBR.Value.ToString(), "constants");
						this._Reader.WriteKey("y155", this.invBG.Value.ToString(), "constants");
						this._Reader.WriteKey("z155", this.invBB.Value.ToString(), "constants");


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

		private void cboPortrasitModel_EditValueChanged(object sender, EventArgs e)
		{
			DrawPortrait_TOP();
		}

		private void cmdOpenNO2O_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			//abre con el programa predeterminado:
			System.Diagnostics.Process.Start("https://forums.frontier.co.uk/threads/no2o-the-definitive-list-of-1-7-2-2-compatible-hud-colour-color-configs-please-add-yours.259311/");
		}


	}
}