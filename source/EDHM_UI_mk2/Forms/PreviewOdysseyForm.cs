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

namespace EDHM_UI_mk2.Forms
{
	public partial class PreviewOdysseyForm : DevExpress.XtraEditors.XtraForm
	{
		private IniFile _Reader = null;
		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private string ImagesPath = string.Empty;

		public game_instance ActiveInstance { get; set; }
		public float[][] ColorMatrix { get; set; }
		public ui_setting Settings { get; set; }

		public string XMLColors = string.Empty;

		public event EventHandler OnPreviewLoaded;//<- Ocurre cuando termino de Actualizar el Preview

		public PreviewOdysseyForm(game_instance _ActiveInstance)
		{
			InitializeComponent();
			this.LookAndFeel.TouchUIMode = DevExpress.Utils.DefaultBoolean.False;
			this.LookAndFeel.TouchScaleFactor = 1;
			this.ActiveInstance = _ActiveInstance;
		}

		private void PreviewOdysseyForm_Load(object sender, EventArgs e)
		{
			//Obligar a usar los puntos y las comas;
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			customCulture.NumberFormat.NumberGroupSeparator = ",";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
			customCulture.NumberFormat.CurrencyGroupSeparator = ",";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			ImagesPath = Path.Combine(AppExePath, "Images", ActiveInstance.key);

			this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, Screen.PrimaryScreen.Bounds.Height - this.Height - 50);
		}
		private void PreviewOdysseyForm_Shown(object sender, EventArgs e)
		{
		}

		private void PreviewOdysseyForm_ResizeEnd(object sender, EventArgs e)
		{
			this.Text = string.Format("Theme Preview ({0}x{1})", this.ClientSize.Width, this.ClientSize.Height);
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
					_XML_Matrix.AppendLine(string.Format("<MatrixRed>{0}, {1}, {2}</MatrixRed>", RR, RG, RB));
					_XML_Matrix.AppendLine(string.Format("<MatrixGreen>{0}, {1}, {2}</MatrixGreen>", GR, GG, GB));
					_XML_Matrix.AppendLine(string.Format("<MatrixBlue>{0}, {1}, {2}</MatrixBlue>", BR, BG, BB));
					this.XMLColors = _XML_Matrix.ToString();

					float[][] colorMatrixElements = {
					   new float[] { (float)RR, (float)RG, (float)RB,       0,      0 },
					   new float[] { (float)GR, (float)GG, (float)GB,       0,      0 },
					   new float[] { (float)BR, (float)BG, (float)BR,       0,      0 },
					   new float[] { 0,         0,          0,              1,      0 },
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

		public void PreviewTheme(bool XMLchanged = false)
		{
			try
			{
				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

					ui_group _BaseHud = null;
					Image _BaseLayer = null;
					Image _Layer_1 = null;
					Image _Layer_2 = null;
					Image _Layer_3 = null;

					int _ElementIndex = 0;
					int _ColorInteger = 0;
					decimal _Brillo = 0.0m;

					Color ED_Orange = Color.FromArgb(255, 194, 54); //#FFC236
					Color ED_Orange_H = Color.FromArgb(255, 250, 100); //#FFFA64
					Color ED_Orange_D = Color.FromArgb(251, 167, 6); //#FBA706

					if (XMLchanged) LoadINIProfile(); //<- Obtiene la Matrix de Colores XML desde el Archivo ini Local.

					#region Base Layer (Imagenes de Fondo):

					_BaseLayer = Util.GetElementBitmap(Path.Combine(ImagesPath, "SHIP_FDL.png")); //<- Imagen de Fondo (NO cambiar)
					_Layer_1 = Util.GetElementBitmap(Path.Combine(ImagesPath, "BLANK.png")); //Sobre esta Imagen iremos agregando Capa sobre capa

					#endregion

					#region Radar

					_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "RadarHUD");

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_Grid.png")); //<- Cargar la Imagen Base del Radar
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Ship Radar" && x.Title == "Radar Mode").Value);

					if (_ElementIndex == 199) //<- Usa XML en lugar de Color
					{
						#region Radar Grid

						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange_D);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);

						//Cargar y Cambiar el Brillo:
						_Brillo = _BaseHud.Elements.Find(x => x.Category == "Radar Grid" && x.Title == "Brightness").Value;
						_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
						_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Radar Outer Rim

						//2. Cargar la Imagen del Borde Exterior:
						_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_Border.png"));
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);

						//Cargar y Cambiar el Brillo:
						_Brillo = _BaseHud.Elements.Find(x => x.Category == "Ship Radar" && x.Title == "Outer Rim Brightness").Value;
						_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
						_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion
					}
					else
					{
						#region Radar Grid

						//Cargar y Cambiar el Color:
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Radar Grid" && x.Title == "Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));

						//Cargar y Cambiar el Brillo:
						_Brillo = _BaseHud.Elements.Find(x => x.Category == "Radar Grid" && x.Title == "Brightness").Value;
						_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
						_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion

						#region Radar Outer Rim

						//2. Cargar la Imagen del Borde Exterior:
						_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_Border.png"));
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Ship Radar" && x.Title == "Radar Outer Rim").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));

						//Cargar y Cambiar el Brillo:
						_Brillo = _BaseHud.Elements.Find(x => x.Category == "Ship Radar" && x.Title == "Outer Rim Brightness").Value;
						_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
						_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

						#endregion
					}

					#region Radar Triangle Widget

					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Radar Triangle Widget" && x.Title == "Mode").Value);
					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_Triangle.png"));

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Radar Triangle Widget" && x.Title == "Custom Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					#endregion

					#region Radar Zoom Widget

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_Zoom.png"));
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Radar Zoom Widget" && x.Title == "Mode").Value);
					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Radar Zoom Widget" && x.Title == "Level lines").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					//Cargar y Cambiar el Brillo:
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Radar Zoom Widget" && x.Title == "Level lines Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					#endregion

					#region Radar Outer Components (Heat & Speed Bars) + Optimal Speed Bar

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_OuterComponents.png"));
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Radar Outer Components" && x.Title == "Color Mode").Value);
					if (_ElementIndex == 199)
					{
						#region Heat & Speed Bars

						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);

						#endregion

						#region Optimal Speed Bar

						_Layer_3 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_OptimalSpeedBar.png"));
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_3 = Util.ApplyColorMatrix(_Layer_3, this.ColorMatrix);
						_Layer_2 = Util.Superimpose(new Bitmap(_Layer_2), new Bitmap(_Layer_3)); //<- Se juntas los 2 componentes de esta capa

						#endregion
					}
					else
					{
						#region Heat & Speed Bars

						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Radar Outer Components" && x.Title == "Custom Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));

						#endregion

						#region Optimal Speed Bar

						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Optimal Speed Bar" && x.Title == "Custom Color").Value);
						_Layer_3 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_OptimalSpeedBar.png"));
						_Layer_3 = Util.ChangeToColor((Bitmap)_Layer_3, Color.FromArgb(_ColorInteger));
						_Layer_2 = Util.Superimpose(new Bitmap(_Layer_2), new Bitmap(_Layer_3)); //<- Se juntas los 2 componentes de esta capa

						#endregion
					}
					//Cargar y Cambiar el Brillo:
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Radar Outer Components" && x.Title == "Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					#endregion

					#endregion

					#region Compass

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_Compass.png")); //<- Cargar la Imagen Base del Radar
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Compass" && x.Title == "Color Mode").Value);

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);

						#region Compass Dot

						_Layer_3 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_CompassDot.png"));
						_Layer_3 = Util.ChangeToColor((Bitmap)_Layer_3, Color.FromArgb(105, 169, 170));
						_Layer_3 = Util.ApplyColorMatrix(_Layer_3, this.ColorMatrix);

						#endregion
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Compass" && x.Title == "Custom Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));

						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Compass" && x.Title == "Compass Dot").Value);
						_Layer_3 = Util.GetElementBitmap(Path.Combine(ImagesPath, "RADAR_CompassDot.png"));
						_Layer_3 = Util.ChangeToColor((Bitmap)_Layer_3, Color.FromArgb(_ColorInteger));
					}
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Compass" && x.Title == "Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Compass" && x.Title == "Compass Dot Brightness").Value;
					_Layer_3 = Util.SetBrightness(_Layer_3, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_3));

					#endregion

					_BaseHud = this.Settings.ui_groups.Find(x => x.Name == "ShieldsHolo");

					#region Circular Swirls - BOTTOM

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "SWIRLS_Bottom.png")); //<- Cargar la Imagen Base del Radar
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Bottom Swirls - Mode").Value);

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Bottom Swirls - Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));
					//No tiene Brillo, sólo Alpha

					#endregion

					#region Circular Swirls - MIDDLE

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "SWIRLS_Middle.png")); //<- Cargar la Imagen Base del Radar
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Middle Swirls - Mode").Value);

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Middle Swirls - Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Middle Swirls - Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					#endregion

					#region Hull Gauge

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "HULL_Gauge.png"));
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Hull Gauge" && x.Title == "Color Mode").Value);

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Hull Gauge" && x.Title == "Custom Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Hull Gauge" && x.Title == "Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					#endregion

					#region Circular Swirls - TOP

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "SWIRLS_Top.png"));
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Top Swirls - Mode").Value);

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, ED_Orange);
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Top Swirls - Color").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Circular Swirls" && x.Title == "Top Swirls - Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					#endregion

					#region Ship Holo

					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "OwnShip Hologram" && x.Title == "Hologram - Mode").Value);

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "SHIP_Holo_XML.png"));
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "SHIP_Holo.png"));
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "OwnShip Hologram" && x.Title == "Hologram - Color").Value);

						//_Layer_2 = Util.ColorRise(_Layer_2, Color.White, Color.FromArgb(_ColorInteger));
						//_Layer_2 = Util.ColorReplace(_Layer_2, 0, Color.White, Color.FromArgb(_ColorInteger));
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "OwnShip Hologram" && x.Title == "Overall Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

					#endregion

					#region Shields

					_Layer_2 = Util.GetElementBitmap(Path.Combine(ImagesPath, "SHIELDS_Single.png"));
					_ElementIndex = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Shields" && x.Title == "Shield Mode").Value);

					if (_ElementIndex == 199)
					{
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(105,169,170));
						_Layer_2 = Util.ApplyColorMatrix(_Layer_2, this.ColorMatrix);
					}
					else
					{
						_ColorInteger = Convert.ToInt32(_BaseHud.Elements.Find(x => x.Category == "Single-Shield" && x.Title == "Shields Up").Value);
						_Layer_2 = Util.ChangeToColor((Bitmap)_Layer_2, Color.FromArgb(_ColorInteger));
					}
					_Brillo = _BaseHud.Elements.Find(x => x.Category == "Single-Shield" && x.Title == "Brightness").Value;
					_Layer_2 = Util.SetBrightness(_Layer_2, (float)_Brillo);
					_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_Layer_2));

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
				});
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			PreviewTheme();
		}
	}
}