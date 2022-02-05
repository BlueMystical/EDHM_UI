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
using EDHM_UI_mk2;

namespace EDHM_DX
{
	public partial class ShipPreviewForm : DevExpress.XtraEditors.XtraForm
	{
		//private theme _CurrentTheme = new theme();
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;

		public ui_setting Settings { get; set; }
		public float[][] ColorMatrix { get; set; }
		public bool AnalysisMode { get; set; }

		public game_instance ActiveInstance { get; set; }

		public ShipPreviewForm(game_instance _ActiveInstance)
		{
			InitializeComponent();
			this.ActiveInstance = _ActiveInstance;
		}

		private void ShipPreviewForm_Load(object sender, EventArgs e)
		{
			this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, Screen.PrimaryScreen.Bounds.Height - this.Height - 50);
		}

		public void PreviewTheme()
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					if (this.Settings != null)
					{
						string ImagesPath = Path.Combine(this.AppExePath, "Images", this.ActiveInstance.key);
						string _FilePath = string.Empty;
						int _ElementIndex = 0;

						Image _BaseLayer = null;
						Image _Layer_1 = null;
						Image _Layer_2 = null;

						LoadINIProfile(); //<- Obtiene la Matrix de Colores XML desde el Archivo ini.

						//Adding Layers from Bottom to Top: "Lower HUD"
						ui_group _BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Lighting");

						//Layer 1: To be able to modify the HUD from the Base layer
						_FilePath = Path.Combine(ImagesPath, "Blank.png");
						_Layer_1 = Bitmap.FromFile(_FilePath);

						//Base Layer:
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "AmbientLights").Value);
						if (_ElementIndex == 199)
						{
							//Carga La Imagen Base:
							_FilePath = Path.Combine(ImagesPath, "AmbientLights_0.png");
							_BaseLayer = Bitmap.FromFile(_FilePath);

							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "AmbientLights_0p.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("AmbientLights_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_BaseLayer = Bitmap.FromFile(_FilePath);
							}
						}

						//Layer 2: Here we'll be adding the different UI elements
						//Starting with the Common Group:
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Radar & Common Group");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "CommonGroup").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "CommonGroup_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("CommonGroup_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						//SignatureBar (Ship's Heat Signature):
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Signature & Fuel Bars");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "SignatureBar").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "SignatureBar_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("SignatureBar_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						//Shield:
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Shield & Holo Ship");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "ShieldColor").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "Shield_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("Shield_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						//Ship Hologram:
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "OwnShipHolo").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "ShipHolo_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("ShipHolo_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						//Radar:
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Radar & Common Group");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "RadarColor").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "Radar_Colour_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("Radar_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						//Power Dist.:
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Power Distributor");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "PowerDistributor").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "PowerDistributor_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("PowerDistributor_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}
						// ------------------------------------------------------------------------------------------------

						//Panel Lines (lower):
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Panel (Lower)");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "PanelLinesLOW").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "PanelLines_Lower_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("PanelLines_Lower_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						//Panel Lines (upper):
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Panel (Upper)");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "PanelLinesUP").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "PannelLines_UP_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("PannelLines_UP_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						if (this.AnalysisMode)
						{
							//Alanysis Mode:
							_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Name == "Analysis HUD (A-HUD) Colour").Value);
							_FilePath = Path.Combine(ImagesPath, string.Format("AnalysisHUD_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}
						else
						{
							//Combat HUD Mode and Color:
							_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Combat HUD");
							int _CHudMode = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "CombatHudMode").Value);
							_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "CombatHUD").Value);
							if (_ElementIndex == 199)
							{
								//Aplica la Matrix de Color:
								_FilePath = Path.Combine(ImagesPath, "CombatHUD_0.png");
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
							else
							{
								_FilePath = Path.Combine(ImagesPath, string.Format("CombatHUD_{0}{1}.png", _ElementIndex,
														Util.IIf(_CHudMode == 1, "b", string.Empty)));
								if (File.Exists(_FilePath))
								{
									_Layer_2 = Bitmap.FromFile(_FilePath);
									_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
								}
							}
						}

						//Circle Around the Target
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Targeting Reticle");
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "EnvironmentObject").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "EnvironmentObject_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("EnvironmentObject_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}

						//Target Reticle:
						_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Type == "TargetingReticle").Value);
						if (_ElementIndex == 199)
						{
							//Aplica la Matrix de Color:
							_FilePath = Path.Combine(ImagesPath, "TargetingReticle_0.png");
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_Layer_2 = ApplyColorMatrix(_Layer_2, this.ColorMatrix);
							_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
						}
						else
						{
							_FilePath = Path.Combine(ImagesPath, string.Format("TargetingReticle_{0}.png", _ElementIndex));
							if (File.Exists(_FilePath))
							{
								_Layer_2 = Bitmap.FromFile(_FilePath);
								_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
							}
						}
						//----------------------------------------------------------------------------------------;
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "Lighting");

						float _Value = 0.0f;
						string _Dimmer = "";

						//Base Layer (Ship and Ambient Lights) Needs to be Obscured Depending on the Value of 'LightingDimmer':
						_Value = (float)_BaseHud.Elements.Find(x => x.Name == "Lighting Dimmer").Value;
						switch (_Value)
						{
							case 1.0f: _Dimmer = "Bright"; break;
							case 0.6f: _Dimmer = "Medium"; break;
							case 0.3f: _Dimmer = "Low"; break;
							default: _Dimmer = "Low"; break;
						}
						_FilePath = Path.Combine(ImagesPath, string.Format("LightingDimmer_{0}.png", _Dimmer));
						if (File.Exists(_FilePath))
						{
							_Layer_2 = Bitmap.FromFile(_FilePath);
							_BaseLayer = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_2));
						}

						//Layer_1 (the HUD elements) Needs to be Alpha Blended depending on the Value of 'HudDimmer':
						//----------------------------------------------------------------------------------------;
						_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "HUD Settings");
						_Value = (float)_BaseHud.Elements.Find(x => x.Type == "HudDimmer").Value;
						switch (_Value)
						{
							case 5: _Value = 1.0f; break;
							case 3: _Value = 0.6f; break;
							case 1: _Value = 0.3f; break;
							default: _Value = 1.0f; break;
						}
						_Layer_1 = Util.SetOpacity(_Layer_1, _Value);

						//Finally we Merge Both:
						_Layer_1 = Util.Superimpose(new Bitmap(_BaseLayer), new Bitmap(_Layer_1));

						Invoke((MethodInvoker)(() =>
						{
							//And Show it into the PictureBox:
							//this.picturePreview.Image = null;
							this.picturePreview.Image = _Layer_1;
							this.picturePreview.Refresh();
						}));
					}
				});
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private void LoadINIProfile()
		{
			try
			{
				IniFile _Reader = new IniFile(Path.Combine(this.AppExePath, "Data", this.ActiveInstance.key + "_XML-Profile.ini")); //<- Open and Read the INI file
				if (_Reader != null)
				{
					decimal RR = Convert.ToDecimal(_Reader.ReadKey("x150", "constants"));
					decimal RG = Convert.ToDecimal(_Reader.ReadKey("y150", "constants"));
					decimal RB = Convert.ToDecimal(_Reader.ReadKey("z150", "constants"));

					decimal GR = Convert.ToDecimal(_Reader.ReadKey("x151", "constants"));
					decimal GG = Convert.ToDecimal(_Reader.ReadKey("y151", "constants"));
					decimal GB = Convert.ToDecimal(_Reader.ReadKey("z151", "constants"));

					decimal BR = Convert.ToDecimal(_Reader.ReadKey("x152", "constants"));
					decimal BG = Convert.ToDecimal(_Reader.ReadKey("y152", "constants"));
					decimal BB = Convert.ToDecimal(_Reader.ReadKey("z152", "constants"));

					StringBuilder _XML_Matrix = new StringBuilder();
					_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", RR, RG, RB));
					_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", GR, GG, GB));
					_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", BR, BG, BB));

					float[][] colorMatrixElements = {
					   new float[] { (float)RR, (float)RG, (float)RB,  0, 1},        // red scaling factor of 2
					   new float[] { (float)GR, (float)GG, (float)GB,  0, 1},        // green scaling factor of 1
					   new float[] { (float)BR, (float)BG, (float)BR / 100,  0, 1},        // blue scaling factor of 1
					   new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
					   new float[] {0, 0, 0, 0, 1			// three translations of 0
					 } };
					this.ColorMatrix = colorMatrixElements;
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message);
			}
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
				imageAttributes.SetColorMatrix(ImageMatrix,  ColorMatrixFlag.Default,  ColorAdjustType.Bitmap);

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

		public void ModState(bool _State)
		{
			//For the 'Kill Switch'
			string ImagesPath = Path.Combine(AppExePath, "Images");
			if (!_State)
			{
				if (File.Exists(Path.Combine(ImagesPath, "MOD_OFF.png")))
				{
					this.picturePreview.Image = Image.FromFile(Path.Combine(ImagesPath, "MOD_OFF.png"));
				}
			}
			else
			{
				PreviewTheme();
			}
		}

		private void ShipPreviewForm_ResizeEnd(object sender, EventArgs e)
		{
			this.Text = string.Format("Theme Preview ({0}x{1})", this.ClientSize.Width, this.ClientSize.Height);
		}
	}
}