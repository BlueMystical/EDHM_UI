using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorConverterSRGB
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void colorControl1_OnCustomColorsChanged(object sender, EventArgs e)
		{
			//Cuando se agregan colores personalizados
		}

		private void colorControl1_OnColorChanged(object sender, EventArgs e)
		{
			Color color = (Color)sender;
			List<double> sRGBA = GetGammaCorrected_RGBA(color, (double)txGamma.Value);
			txSRGB_C_R.Text = sRGBA[0].ToString("n3");
			txSRGB_C_G.Text = sRGBA[1].ToString("n3");
			txSRGB_C_B.Text = sRGBA[2].ToString("n3");
			txSRGB_C_A.Text = sRGBA[3].ToString("n3");

			txRGBint.Value = color.ToArgb();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			double R = Convert.ToDouble(txSRGB_C_R.Text);
			double G = Convert.ToDouble(txSRGB_C_G.Text);
			double B = Convert.ToDouble(txSRGB_C_B.Text);
			double A = Convert.ToDouble(txSRGB_C_A.Text);

			Color color = ReverseGammaCorrected(R, G, B, A, (double)txGamma.Value);
			colorControl1.SetColorFrom(color);
			txRGBint.Value = color.ToArgb();
		}


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

		private List<double> GetGammaCorrected_RGBA(System.Drawing.Color _Color, double _GammaValue = 2.4)
		{
			List<double> _ret = null; //<- valores en formato RGBA
			try
			{
				//El proceso de conversion es asi:  RGB -> sRGB (no lineal) -> Gamma Corrected

				ColorManagment.ColorConverter.Init();

				//Convertir al espacio sRGB no lineal: 
				ColorManagment.ColorRGB sRGBcolor = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, _Color.R, _Color.G, _Color.B);

				//Get Gamma Corrected Values:
				_ret = new List<double>();
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.R, _GammaValue), 4));
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.G, _GammaValue), 4));
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.B, _GammaValue), 4));
				_ret.Add(Math.Round(Convert.ToDouble(NormalizeNumber(_Color.A, 0m, 255m, 0m, 1m)), 4)); //alpha remains linear!
			}
			catch (Exception ex) { MessageBox.Show("ERROR!", ex.Message + ex.StackTrace); }
			return _ret;
		}
		
		private Color ReverseGammaCorrected(double _Gamma_R, double _Gamma_G, double _Gamma_B, double _Gamma_A = 1.0, double _GammaValue = 2.4)
		{
			System.Drawing.Color _ret = System.Drawing.Color.White;
			try
			{
				//El proceso de de conversion es asi:   GammaCorrectedValue -> sRGB -> Linear sRGB -> RGB

				//Undo Gamma Correction: Produce Valores en sRGB
				var inv_R = Convert_sRGB_FromLinear(_Gamma_R, _GammaValue);
				var inv_G = Convert_sRGB_FromLinear(_Gamma_G, _GammaValue);
				var inv_B = Convert_sRGB_FromLinear(_Gamma_B, _GammaValue);

				//Linearizar el sRGB:
				var colorB = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, inv_R, inv_G, inv_B, true);

				//Convertir a RGB normal:
				int R = (int)Math.Round(colorB.R * 255, MidpointRounding.AwayFromZero);
				int G = (int)Math.Round(colorB.G * 255, MidpointRounding.AwayFromZero);
				int B = (int)Math.Round(colorB.B * 255, MidpointRounding.AwayFromZero);
				int A = (int)NormalizeNumber(Convert.ToDecimal(_Gamma_A), 0.0m, 1m, 0m, 255m);

				_ret = System.Drawing.Color.FromArgb(A, R, G, B);
			}
			catch (Exception ex) { MessageBox.Show("ERROR!", ex.Message + ex.StackTrace); }
			return _ret;
		}
		private Color ReverseGammaCorrected(List<double> _GammaComponents, double _GammaValue = 2.4)
		{
			System.Drawing.Color _ret = System.Drawing.Color.White;
			try
			{
				//El proceso de de conversion es asi:   GammaCorrectedValue -> sRGB -> Linear sRGB -> RGB

				//Undo Gamma Correction: Produce Valores en sRGB
				double inv_R = Convert_sRGB_FromLinear(_GammaComponents[0], _GammaValue);
				double inv_G = Convert_sRGB_FromLinear(_GammaComponents[1], _GammaValue);
				double inv_B = Convert_sRGB_FromLinear(_GammaComponents[2], _GammaValue);
				double alpha = 1;
				if (_GammaComponents.Count > 3)
				{
					alpha = _GammaComponents[3];
				}

				//Linearizar el sRGB:
				var colorB = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, inv_R, inv_G, inv_B, true);

				//Convertir a RGB normal:
				int R = (int)Math.Round(colorB.R * 255, MidpointRounding.AwayFromZero);
				int G = (int)Math.Round(colorB.G * 255, MidpointRounding.AwayFromZero);
				int B = (int)Math.Round(colorB.B * 255, MidpointRounding.AwayFromZero);
				int A = (int)NormalizeNumber(Convert.ToDecimal(alpha), 0.0m, 1m, 0m, 255m);

				_ret = System.Drawing.Color.FromArgb(A, R, G, B);
			}
			catch (Exception ex) { MessageBox.Show(ex.Message + ex.StackTrace); }
			return _ret;
		}

		public String Number_To_RGBA_Normalized(decimal _Value, decimal A_MinValue = 0, decimal A_MaxValue = 1, decimal B_MinValue = 0, decimal B_MaxValue = 1, int DecimalPlaces = 6)
		{
			string _Ret = string.Empty;
			decimal _RedDec = NormalizeNumber(A_MinValue, A_MaxValue, B_MinValue, B_MaxValue);

			_Ret = string.Format("{0}", Math.Round(_RedDec, DecimalPlaces).ToString().Replace(',', '.').PadRight(DecimalPlaces + 2, '0'));
			return _Ret;
		}

		public decimal NormalizeNumber(decimal _Value, decimal A_MinValue = 0, decimal A_MaxValue = 1, decimal B_MinValue = 0, decimal B_MaxValue = 10)
		{
			decimal _ret = 0;
			if (B_MaxValue > B_MinValue && A_MaxValue > A_MinValue)
			{
				_ret = (B_MaxValue - B_MinValue) / (A_MaxValue - A_MinValue) * (_Value - A_MaxValue) + B_MaxValue;
			}
			if (_ret < B_MinValue)
			{
				_ret = B_MinValue;
			}

			if (_ret > B_MaxValue)
			{
				_ret = B_MaxValue;
			}

			return _ret;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Color color = Color.FromArgb((int)txRGBint.Value);
			colorControl1.SetColorFrom(color);
		}
	}
}
