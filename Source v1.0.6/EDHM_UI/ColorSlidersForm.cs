using System;
using System.Drawing;
using System.Windows.Forms;

namespace EDHM_UI
{
	public partial class ColorSlidersForm : Form
	{
		public Color Color { get; set; }
		public decimal MinValue { get; set; }
		public decimal MaxValue { get; set; }
		public string RGBA {			
			get
			{
				string _Ret = string.Empty;
				if (this.Color != null)
				{
					_Ret = Util.Color_To_RGBA_Normalized(this.Color, this.MinValue, this.MaxValue);
				}
				return _Ret;
			}
		}
		public string Title {
			set
			{
				this.Text = value;
			}
		}
		
		private bool _IsLoaded = false;

		public ColorSlidersForm(Color _Color, decimal _MinValue, decimal _MaxValue)
		{
			InitializeComponent();
			this.Color = _Color;
			this.MinValue = _MinValue;
			this.MaxValue = _MaxValue;
		}

		private void ColorSlidersForm_Load(object sender, EventArgs e)
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			customCulture.NumberFormat.NumberGroupSeparator = ",";
			customCulture.NumberFormat.CurrencyDecimalSeparator = ".";
			customCulture.NumberFormat.CurrencyGroupSeparator = ",";

			if (this.Color != null)
			{
				this.trackAlpha.Value = Convert.ToInt32(this.Color.A);
				this.trackRed.Value = Convert.ToInt32(this.Color.R);
				this.trackGreen.Value = Convert.ToInt32(this.Color.G);
				this.trackBlue.Value = Convert.ToInt32(this.Color.B);

				this.panel1.BackColor = this.Color;
				this.txtValue.Text = this.RGBA;
				this.txtColorFormats.Text = string.Format("{0} | Int32: {1}",
					Util.ARGBConverter(this.Color), this.Color.ToArgb());
				_IsLoaded = true;
			}
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}


		private void trackRed_ValueChanged(object sender, EventArgs e)
		{
			if (_IsLoaded)
			{
				this.Color = Color.FromArgb(this.trackAlpha.Value, this.trackRed.Value, this.trackGreen.Value, this.trackBlue.Value);
				this.panel1.BackColor = this.Color;
				this.txtValue.Text = this.RGBA;
				this.txtColorFormats.Text = string.Format("{0} | INT({1})",
					Util.ARGBConverter(this.Color), this.Color.ToArgb());
			}
		}

		private void trackGreen_ValueChanged(object sender, EventArgs e)
		{
			if (_IsLoaded)
			{
				this.Color = Color.FromArgb(this.trackAlpha.Value, this.trackRed.Value, this.trackGreen.Value, this.trackBlue.Value);
				this.panel1.BackColor = this.Color;
				this.txtValue.Text = this.RGBA;
				this.txtColorFormats.Text = string.Format("{0} | INT({1})",
					Util.ARGBConverter(this.Color), this.Color.ToArgb());
			}
		}

		private void trackBlue_ValueChanged(object sender, EventArgs e)
		{
			if (_IsLoaded)
			{
				this.Color = Color.FromArgb(this.trackAlpha.Value, this.trackRed.Value, this.trackGreen.Value, this.trackBlue.Value);
				this.panel1.BackColor = this.Color;
				this.txtValue.Text = this.RGBA;
				this.txtColorFormats.Text = string.Format("{0} | INT({1})",
					Util.ARGBConverter(this.Color), this.Color.ToArgb());
			}
		}

		private void trackAlpha_ValueChanged(object sender, EventArgs e)
		{
			if (_IsLoaded)
			{
				this.Color = Color.FromArgb(this.trackAlpha.Value, this.trackRed.Value, this.trackGreen.Value, this.trackBlue.Value);
				this.panel1.BackColor = this.Color;
				this.txtValue.Text = this.RGBA;
				this.txtColorFormats.Text = string.Format("{0} | INT({1})",
					Util.ARGBConverter(this.Color), this.Color.ToArgb());
			}
		}
	}
}
