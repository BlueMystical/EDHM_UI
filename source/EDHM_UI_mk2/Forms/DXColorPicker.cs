using DevExpress.XtraEditors;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace EDHM_UI_mk2
{
	public partial class DXColorPicker : DevExpress.XtraEditors.XtraUserControl
	{
		private bool ShowingColor = false; //<- Evita actualizaciones redundantes
		private Image _BaseLayer = null;	//<- Capa base para dibujar la linea de posicion

		public DXColorPicker()
		{
			InitializeComponent();
		}
		public DXColorPicker(Color pColor)
		{
			InitializeComponent();
			SelectedColor = pColor;
		}

		private void DXColorPicker_Load(object sender, EventArgs e)
		{
			_BaseLayer = this.picColorPick.Image;

			this.txtRGB_Red.BackColor = this.BackColor; 
			this.txtRGB_Green.BackColor = this.BackColor; 
			this.txtRGB_Blue.BackColor = this.BackColor;
			this.txtRGB_Alpha.BackColor = this.BackColor;

			this.txtRGB_Red.ForeColor = this.ForeColor;
			this.txtRGB_Green.ForeColor = this.ForeColor;
			this.txtRGB_Blue.ForeColor = this.ForeColor;
			this.txtRGB_Alpha.ForeColor = this.ForeColor;

			this.txtHTML.BackColor = this.BackColor;
			this.txtHTML.ForeColor = this.ForeColor;

			this.txtARGB.BackColor = this.BackColor;
			this.txtARGB.ForeColor = this.ForeColor;

			DisplaySelectedColor();
		}

		#region Propiedades

		/// <summary>Obtiene o Establece el Color Seleccionado.</summary>
		[Description("Obtiene o Establece el Color Seleccionado"), Category("Appearance")]
		public Color SelectedColor { get; set; }  = Color.White;

		/// <summary>Obtiene o Establece el Color Seleccionado desde un color HTML. Admite Colores con Nombre.</summary>
		[Description("Obtiene o Establece el Color Seleccionado desde un color HTML. Admite Colores con Nombre."), Category("Appearance")]
		public string HtmlColor
		{
			get
			{
				return ColorTranslator.ToHtml(SelectedColor);
			}
			set
			{
				SelectedColor = ColorTranslator.FromHtml(value);
			}
		}


        #endregion

        #region Metodos

        private void DisplaySelectedColor(bool DrawGradients = true)
		{
			ShowingColor = true;
			if (SelectedColor != null)
			{
				this.trackRGB_Red.EditValue =	SelectedColor.R;
				this.trackRGB_Green.EditValue = SelectedColor.G;
				this.trackRGB_Blue.EditValue =	SelectedColor.B;
				this.trackRGB_Alpha.EditValue = SelectedColor.A;

				this.txtRGB_Red.EditValue =		SelectedColor.R;
				this.txtRGB_Green.EditValue =	SelectedColor.G;
				this.txtRGB_Blue.EditValue =	SelectedColor.B;
				this.txtRGB_Alpha.EditValue =	SelectedColor.A;

				this.txtHTML.EditValue = ColorTranslator.ToHtml(SelectedColor);
				this.txtARGB.EditValue = string.Format("{0},{1},{2},{3}", SelectedColor.A, SelectedColor.R, SelectedColor.G, SelectedColor.B);

				this.picColorPreview.BackColor = SelectedColor;

				DrawColorPosition(SelectedColor.GetHue(), DrawGradients);
			}
			ShowingColor = false;
		}

		public void DrawColorPosition(float HUE, bool DrawGradients = true)
		{
			try
			{
				/* 	 HUE: 0.0 -> 360.0  
					 SAT: 1.0 -> 0.0
				 */

				//var HUE_PosX = (picColorPick.Width / 360) * e.X;
				//var SAT_PosY = Math.Abs(1 - e.Y) + 1;

				int HUE_PosX = (int)((picColorPick.Width * HUE) / 360) - 1;
				//int SAT_PosY = (int)Math.Abs(1 - (picColorPick.Height * SAT)) + 1;

				Image _Layer_1 = (Image)this._BaseLayer.Clone();
				Image _VerticalLine = new Bitmap(_Layer_1.Width, _Layer_1.Height);

				Pen blackPen = new Pen(Color.Black, 1);
				using (var graphics = Graphics.FromImage(_VerticalLine))
				{
					graphics.DrawLine(blackPen, HUE_PosX, 0, HUE_PosX, picColorPick.Height);
				}
				_Layer_1 = Util.Superimpose(new Bitmap(_Layer_1), new Bitmap(_VerticalLine), 0);

				this.picColorPick.Image = _Layer_1;
				this.picColorPick.Refresh();

				if (DrawGradients)
				{
					DrawDarkGradient(HUE_PosX);
				}				

				this.lblStatus.Text = string.Format("HUE:{0}, SAT:{1}", HUE, SelectedColor.GetSaturation());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void DrawDarkGradient(int posX)
		{
			try
			{
				//1. Obtener el color con saturacion 1.0 (Arriba del todo en la imagen):
				//Color TopColor;
				//using (Bitmap bmp = new Bitmap(_BaseLayer))
				//{
				//	TopColor = bmp.GetPixel(posX, 1);
				//};

				this.picDarkColors.BackColor = SelectedColor;
				this.picDarkColors.Image = Util.GradientImage(this.picDarkColors.Width, this.picDarkColors.Height, this.SelectedColor, Color.Black, 90);
				this.picDarkColors.Refresh();

				this.picClearColors.BackColor = SelectedColor;
				this.picClearColors.Image = Util.GradientImage(this.picClearColors.Width, this.picClearColors.Height, this.SelectedColor, Color.White, 90);
				this.picClearColors.Refresh();
			}
			catch { }
		}
		
		#endregion

		#region Eventos de Controles

		private void trackRGB_Red_ValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as TrackBarControl;

				int color_R = (int)_Control.EditValue;
				int color_G = (int)this.trackRGB_Green.EditValue;
				int color_B = (int)this.trackRGB_Blue.EditValue;
				int color_A = (int)this.trackRGB_Alpha.EditValue;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
				DisplaySelectedColor();
			}			
		}

		private void trackRGB_Green_ValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as TrackBarControl;

				int color_R = (int)this.trackRGB_Red.EditValue;
				int color_G = (int)_Control.EditValue;
				int color_B = (int)this.trackRGB_Blue.EditValue;
				int color_A = (int)this.trackRGB_Alpha.EditValue;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
				DisplaySelectedColor();
			}
		}

		private void trackRGB_Blue_ValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as TrackBarControl;

				int color_R = (int)this.trackRGB_Red.EditValue;
				int color_G = (int)this.trackRGB_Green.EditValue;
				int color_B = (int)_Control.EditValue;
				int color_A = (int)this.trackRGB_Alpha.EditValue;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
				DisplaySelectedColor();
			}			
		}

		private void trackRGB_Alpha_ValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as TrackBarControl;

				int color_R = (int)this.trackRGB_Red.EditValue;
				int color_G = (int)this.trackRGB_Green.EditValue;
				int color_B = (int)this.trackRGB_Blue.EditValue;
				int color_A = (int)_Control.EditValue;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
				DisplaySelectedColor();
			}
		}


		private void txtRGB_Red_EditValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as SpinEdit;

				int color_R = (int)_Control.Value;
				int color_G = (int)this.trackRGB_Green.EditValue;
				int color_B = (int)this.trackRGB_Blue.EditValue;
				int color_A = (int)this.trackRGB_Alpha.EditValue;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);

				DisplaySelectedColor();
			}			
		}

		private void txtRGB_Green_EditValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as SpinEdit;

				int color_R = (int)this.trackRGB_Red.EditValue;
				int color_G = (int)_Control.Value;
				int color_B = (int)this.trackRGB_Blue.EditValue;
				int color_A = (int)this.trackRGB_Alpha.EditValue;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
				DisplaySelectedColor();
			}
		}

		private void txtRGB_Blue_EditValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as SpinEdit;

				int color_R = (int)this.trackRGB_Red.EditValue;
				int color_G = (int)this.trackRGB_Green.EditValue;
				int color_B = (int)_Control.Value;
				int color_A = (int)this.trackRGB_Alpha.EditValue;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
				DisplaySelectedColor();
			}
		}

		private void txtRGB_Alpha_EditValueChanged(object sender, EventArgs e)
		{
			if (!ShowingColor)
			{
				var _Control = sender as SpinEdit;

				int color_R = (int)this.trackRGB_Red.EditValue;
				int color_G = (int)this.trackRGB_Green.EditValue;
				int color_B = (int)this.trackRGB_Blue.EditValue;
				int color_A = (int)_Control.Value;

				SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
				DisplaySelectedColor();
			}
		}


		private void txtHTML_EditValueChanged(object sender, EventArgs e)
		{
			try
			{
				if (!ShowingColor)
				{
					var _Control = sender as TextEdit;

					SelectedColor = ColorTranslator.FromHtml(txtHTML.Text);

					DisplaySelectedColor();
				}
			}
			catch { }			
		}
		private void txtHTML_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				//Se Presionó la Tecla ENTER
				e.Handled = true;
				txtHTML_EditValueChanged(sender, null);
			}
		}

		private void txtARGB_EditValueChanged(object sender, EventArgs e)
		{
			try
			{
				if (!ShowingColor)
				{
					var _Control = sender as TextEdit;
					if (_Control.EditValue != null && _Control.EditValue.ToString() != string.Empty)
					{
						string[] _ARGB = { };
						if (_Control.EditValue.ToString().Contains(","))
						{
							_ARGB = _Control.EditValue.ToString().Split(new char[] { ',' });
						}
						if (_Control.EditValue.ToString().Contains(";"))
						{
							_ARGB = _Control.EditValue.ToString().Split(new char[] { ';' });
						}

						if (_ARGB != null && _ARGB.Length == 3)
						{
							int color_A = 255;
							int color_R = Convert.ToInt32(_ARGB[0]);
							int color_G = Convert.ToInt32(_ARGB[1]);
							int color_B = Convert.ToInt32(_ARGB[2]);

							SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);
						}
						if (_ARGB != null && _ARGB.Length == 4)
						{
							int color_A = Convert.ToInt32(_ARGB[0]);
							int color_R = Convert.ToInt32(_ARGB[1]);
							int color_G = Convert.ToInt32(_ARGB[2]);
							int color_B = Convert.ToInt32(_ARGB[3]);

							SelectedColor = Color.FromArgb(color_A, color_R, color_G, color_B);							
						}
						DisplaySelectedColor();
					}
				}
			}
			catch { }
		}
		private void txtARGB_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				//Se Presionó la Tecla ENTER
				e.Handled = true;
				txtARGB_EditValueChanged(sender, null);
			}
		}

		private void picColorPick_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				using (Bitmap bmp = new Bitmap(picColorPick.Image))
				{
					SelectedColor = bmp.GetPixel(e.X, e.Y);
					DisplaySelectedColor();
				};
			}
			catch { }
		}
		private void picColorPick_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				/* 	 HUE: 0.0 -> 360.0  
					 SAT: 1.0 -> 0.0
				 */

				//float HUE = this.SelectedColor.GetHue();
				//float SAT = SelectedColor.GetSaturation();

				////int HUE_PosX = (int)((picColorPick.Width * HUE) / 360);
				//var HUE_PosX = (picColorPick.Width / 360) * e.X;
				//var SAT_PosY = Math.Abs(1 - e.Y) + 1;

				//// Y = -500H + 500
				////var SAT_PosY = -picColorPick.Height * SAT + picColorPick.Height;

				//var SAT_Y = (1 - SAT) ;
				//int SAT_PosY2 = (int)(picColorPick.Height * SAT_Y); //Math.Abs(1 - (picColorPick.Height * SAT)) + 1;

				//this.lblStatus.Text = string.Format("x:{0}, HUE_PosX:{1} | y:{2}, SAT_PosY:{3} : {4} | HUE:{5}, SAT:{6}",
				//				e.X, HUE_PosX,
				//				e.Y, SAT_PosY, SAT_PosY2,
				//				HUE,
				//				SAT
				//		);


				
			}
			catch { }			
		}

		private void picDarkColors_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				using (Bitmap bmp = new Bitmap(picDarkColors.Image))
				{
					SelectedColor = bmp.GetPixel(e.X, e.Y);
					DisplaySelectedColor(false);
				};
			}
			catch { }
		}
		private void picClearColors_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				using (Bitmap bmp = new Bitmap(picClearColors.Image))
				{
					SelectedColor = bmp.GetPixel(e.X, e.Y);
					DisplaySelectedColor(false);
				};
			}
			catch { }
		}

		#endregion
	}
}
