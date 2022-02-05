using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace EDHM_UI_mk2.Forms
{
	public partial class PreviewForm : DevExpress.XtraEditors.XtraForm
	{
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		public event EventHandler OnPreviewLoaded; //<- Ocurre cuando termino de Actualizar el Preview

		public ui_setting Settings { get; set; }
		public float[][] ColorMatrix { get; set; }
		public game_instance ActiveInstance { get; set; }
		public string XMLColors = string.Empty;
		public bool AnalysisMode { get; set; }

		public PreviewForm(game_instance _ActiveInstance)
		{
			InitializeComponent();
			this.ActiveInstance = _ActiveInstance;
		}

		private void PreviewForm_Load(object sender, EventArgs e)
		{
			//Obligar a usar los puntos y las comas;
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ".";

			customCulture.NumberFormat.NumberGroupSeparator = ",";
			customCulture.NumberFormat.CurrencyGroupSeparator = ",";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			this.LookAndFeel.TouchUIMode = DevExpress.Utils.DefaultBoolean.False;
			this.LookAndFeel.TouchScaleFactor = 1;

			this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, Screen.PrimaryScreen.Bounds.Height - this.Height - 50);
		}

		private void PreviewForm_Shown(object sender, EventArgs e)
		{
		}

		private void PreviewForm_ResizeEnd(object sender, EventArgs e)
		{
			this.Text = string.Format("Theme Preview ({0}x{1})", this.ClientSize.Width, this.ClientSize.Height);
		}

		public void PreviewTheme(bool XMLchanged = false)
		{
			try
			{
				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

					if (this.Settings != null)
					{
						string ImagesPath = Path.Combine(this.AppExePath, "Images", this.ActiveInstance.key);
						int _ElementIndex = 0;

						ui_group _BaseHud = null;
						Image _BaseLayer = null;
						Image _Layer_1 = null;
						Image _Layer_2 = null;

						if (XMLchanged) LoadINIProfile(); //<- Obtiene la Matrix de Colores XML desde el Archivo ini Local.

						//Adding Layers from Bottom to Top:
						#region Base Layer (Imagenes de Fondo):

						_BaseLayer = Util.GetElementBitmap(Path.Combine(ImagesPath, "AmbientLights_0.png")); //<- Imagen de Fondo (NO cambiar)
						  _Layer_1 = Util.GetElementBitmap(Path.Combine(ImagesPath, "Blank.png")); //Sobre esta Imagen iremos agregando Capa sobre capa

						#endregion

						#region Ambient Lights

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Lighting");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "x103").Value); //<- AmbientLights
						if (_ElementIndex == 199) //<- usar XML Colors
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "AmbientLights_0p.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("AmbientLights_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_2), new Bitmap(_Layer_1), 0);

						#endregion

						#region Common Group

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "RadarHUD");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "x104").Value); //CommonGroup
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "CommonGroup_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("CommonGroup_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Radar

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "RadarHUD");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "y106").Value); //RadarColor
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "Radar_Colour_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("Radar_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Ship Hologram:

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "ShieldsHolo");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "y101").Value); //OwnShipHolo
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "ShipHolo_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("ShipHolo_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Shields

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "ShieldsHolo");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "z105").Value); //ShieldColor
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "Shield_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("Shield_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Power Distributor

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Power_Distributor");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "y108").Value); //PowerDistributor
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "PowerDistributor_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("PowerDistributor_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region SignatureBar (Ship's Heat Signature):

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "FuelBars");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "w106").Value); //SignatureBar
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "SignatureBar_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("SignatureBar_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Panel Lines (lower):

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Panel_Lower");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "z104").Value); //PanelLinesLOW
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "PanelLines_Lower_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("PanelLines_Lower_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Panel Lines (upper):

						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Panel_UP");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "y104").Value); //PanelLinesUP
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "PannelLines_UP_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("PannelLines_UP_{0}.png", _ElementIndex)));
						}
						if(_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Combat & Analysis HUD

						if (this.AnalysisMode)
						{
							//Alanysis Mode:
							_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "CHUD_AHUD");
							_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "y113").Value); //Analysis HUD Inner Lines
							if (_ElementIndex == 199)
							{
								//Aplica la Matrix de Color:
								_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "AnalysisHUD_0.png"));
								_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							}
							else
							{
								_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("AnalysisHUD_{0}.png", _ElementIndex)));
							}
						}
						else
						{
							//Combat HUD Mode and Color:
							_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "CHUD_AHUD");
							int _CHudMode = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "y105").Value); //CombatHudMode
							_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "x105").Value); //CombatHUD
							if (_ElementIndex == 199)
							{
								//Aplica la Matrix de Color:
								_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "CombatHUD_0.png"));
								_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
							else
							{
								string _FilePath = Path.Combine(ImagesPath, string.Format("CombatHUD_{0}{1}.png", _ElementIndex,
														Util.IIf(_CHudMode == 1, "b", string.Empty)));
								if (File.Exists(_FilePath))
								{
									_Layer_2 = Util.GetElementBitmap(_FilePath);
								}
							}
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Enviroment Target

						//Circle Around the Target
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Reticle");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "y107").Value); //EnvironmentObject
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "EnvironmentObject_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("EnvironmentObject_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Targeting Reticle

						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Key == "x107").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "TargetingReticle_0.png"));
							_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
						}
						else
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("TargetingReticle_{0}.png", _ElementIndex)));
						}
						if (_Layer_2 != null) _Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Lights & FX

						float _Value = 0.0f;
						string _Dimmer = "";

						//Base Layer (Ship and Ambient Lights) Needs to be Obscured Depending on the Value of 'LightingDimmer':
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Lighting");
						_Value = (float)_BaseHud.Elements.Find(x => x.Key == "w103").Value; //<- Lighting Dimmer
						switch (_Value)
						{
							case 1.0f: _Dimmer = "Bright"; break;
							case 0.6f: _Dimmer = "Medium"; break;
							case 0.3f: _Dimmer = "Low"; break;
							default: _Dimmer = "Low"; break;
						}

						if (File.Exists(Path.Combine(ImagesPath, string.Format("LightingDimmer_{0}.png", _Dimmer))))
						{
							_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, string.Format("LightingDimmer_{0}.png", _Dimmer)));
							_BaseLayer = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_2));
						}

						//Layer_1 (the HUD elements) Needs to be Alpha Blended depending on the Value of 'HudDimmer':
						//----------------------------------------------------------------------------------------;
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Settings");
						_Value = (float)_BaseHud.Elements.Find(x => x.Key == "y100").Value;
						switch (_Value)
						{
							case 5: _Value = 1.0f; break;
							case 3: _Value = 0.6f; break;
							case 1: _Value = 0.3f; break;
							default: _Value = 1.0f; break;
						}
						_Layer_1 = Util.SetOpacity(_Layer_1, _Value);

						#endregion

						//Finally we Merge the Base Layer with all the Added Layers:
						_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1), 0);

						Invoke((MethodInvoker)(() =>
						{
							//And Show it into the PictureBox:
							this.picturePreview.Image = _Layer_1;
							this.picturePreview.Refresh();

							OnPreviewLoaded(DialogResult.OK, null); //<- Disparador del Evento.

							this.Cursor = Cursors.Default;
						}));
					}
				} );
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadINIProfile()
		{
			try
			{
				IniFile _Reader = new IniFile(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_XML-Profile.ini")); //<- Open and Read the INI file
				if (_Reader != null)
				{
					//string _RR = _Reader.ReadKey("x150", "constants");
					decimal RR = Convert.ToDecimal(_Reader.ReadKey("x150", "constants").NVL("0"));
					decimal RG = Convert.ToDecimal(_Reader.ReadKey("y150", "constants").NVL("0"));
					decimal RB = Convert.ToDecimal(_Reader.ReadKey("z150", "constants").NVL("0"));

					decimal GR = Convert.ToDecimal(_Reader.ReadKey("x151", "constants").NVL("0"));
					decimal GG = Convert.ToDecimal(_Reader.ReadKey("y151", "constants").NVL("0"));
					decimal GB = Convert.ToDecimal(_Reader.ReadKey("z151", "constants").NVL("0"));

					decimal BR = Convert.ToDecimal(_Reader.ReadKey("x152", "constants").NVL("0"));
					decimal BG = Convert.ToDecimal(_Reader.ReadKey("y152", "constants").NVL("0"));
					decimal BB = Convert.ToDecimal(_Reader.ReadKey("z152", "constants").NVL("0"));

					StringBuilder _XML_Matrix = new StringBuilder();
					_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>",		RR, RG, RB));
					_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>",	GR, GG, GB));
					_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>",		BR, BG, BB));
					this.XMLColors = _XML_Matrix.ToString();

					float[][] colorMatrixElements = {
					   new float[] { (float)RR, (float)RG, (float)RB,		0,		0 },
					   new float[] { (float)GR, (float)GG, (float)GB,		0,		0 },
					   new float[] { (float)BR, (float)BG, (float)BR,		0,		0 },
					   new float[] { 0,         0,          0,				1,      0 },
					   new float[] { 0,         0,          0,              0,      1 }
					};

					this.ColorMatrix = colorMatrixElements;
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
		}

		/// <summary>Obtiene una Images Reducida del HUD. Tamaño 360x72</summary>
		public Image GetPreviewThumbnail()
		{
			Image _ret = null;
			try
			{
				if (this.picturePreview.Image != null)
				{
					Image _Preview = (Image)this.picturePreview.Image.Clone();
					byte[] imageBytes = ImageHelper.imageToByteArray(_Preview);
					byte[] croppedImage = ImageHelper.CropImage(imageBytes, 0, 615, _Preview.Width, 317);
					_Preview = ImageHelper.byteArrayToImage(croppedImage);

					_ret = ImageHelper.ScaleImage(_Preview, 72);
					_Preview.Dispose();
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private void barToggleSwitchItem1_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			this.AnalysisMode = barToggleSwitchItem1.Checked;
			if (this.AnalysisMode)
			{
				barToggleSwitchItem1.Caption = "Analysis Mode";
			}
			else
			{
				barToggleSwitchItem1.Caption = "Combat Mode";
			}
			PreviewTheme();
		}

		private void picturePreview_MouseClick(object sender, MouseEventArgs e)
		{
			Console.WriteLine(string.Format("X:{0}, Y:{1}, W:{2}, H:{3}, PW:{4}, PH:{5}",
				e.X, e.Y,
				picturePreview.Width, picturePreview.Height,
				this.Width, this.Height
			));

			GetPreviewThumbnail();
		}
	}
}