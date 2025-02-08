using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BlueControls
{
	/// <summary>Custom Control to Show and Edit Color Values.</summary>
	public partial class ColorControl : UserControl
	{
		public event EventHandler OnCustomColorsChanged = delegate { };
		public event EventHandler OnColorChanged = delegate { };

		/// <summary>The Selected color.</summary>
		public Color ColorValue { get; set; } = Color.White;

		public int Red { get { return ColorValue.R; } }
		public int Green { get { return ColorValue.G; } }
		public int Blue { get { return ColorValue.B; } }
		public int Alpha { get { return ColorValue.A; } }

		public decimal Time { get; set; } = 0;

		private bool IsLoading = true;
		private Timer textChangedTimer;
		private TextBox activeTextBox;
		private ToolTip toolTip;

		/// <summary>Custom Colors for the colorPick Dialog</summary>
		public int[] CustomColors { get; set; } = new int[16]; // Enumerable.Repeat(unchecked(16777215), 16).ToArray();


		public ColorControl()
		{
			InitializeComponent();
		}
		public ColorControl(Color Value)
		{
			InitializeComponent();
			SetColorFrom(Value);
		}
		public ColorControl(int Value)
		{
			InitializeComponent();
			SetColorFrom(Value);
		}
		public ColorControl(int[] RGBA_Values)
		{
			InitializeComponent();
			SetColorFrom(RGBA_Values);
		}
		public ColorControl(decimal[] RGBA_Values)
		{
			InitializeComponent();
			SetColorFrom(RGBA_Values);
		}

		private void ColorControl_Load(object sender, EventArgs e)
		{
			textChangedTimer = new Timer
			{
				Interval = 500 // Set the delay interval in milliseconds
			};
			textChangedTimer.Tick += RGB_Value_TimerTick;

			toolTip = new ToolTip
			{
				// Optional: Customize tooltip appearance
				ToolTipTitle = "Information",
				ToolTipIcon = ToolTipIcon.Info,
				IsBalloon = true
			};
			toolTip.SetToolTip(txtHtmlValue, "Import any Color here, acepted formats:\r\nNumeric Value, HEX html, A;R;G;B, Named Color.");

			SetColorFrom(this.ColorValue);
		}

		private void ColorBox_DoubleClick(object sender, EventArgs e)
		{
			/* SHOWS A COLOR DIALOG TO PICK A NEW COLOR  */
			int Alpha = this.ColorValue.A;
			ColorDialog Dialog = new ColorDialog()
			{
				AnyColor = true,
				FullOpen = true,
				AllowFullOpen = true,
				SolidColorOnly = false,
				Color = this.ColorValue,
				CustomColors = CustomColors
			};
			if (Dialog.ShowDialog() == DialogResult.OK)
			{
				IsLoading = true;
				
				if (this.CustomColors != Dialog.CustomColors)
				{
					this.CustomColors = Dialog.CustomColors;
					OnCustomColorsChanged?.Invoke(this.CustomColors, EventArgs.Empty);
				}

				SetColorFrom(Color.FromArgb(Alpha, Dialog.Color.R, Dialog.Color.G, Dialog.Color.B));
			}
		}
		private void ColorBox_Paint(object sender, PaintEventArgs e)
		{
			/*  DRAWS A TRANSPARENCY GRID */
			int gridSize = 6; //<- Size of the squares
			int rows = (ColorBox.Height / gridSize) + 1; //<- To ensure grid covers the whole box
			int cols = (ColorBox.Width / gridSize) + 1;

			// Draw the grid layer:
			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					Rectangle rect = new Rectangle(col * gridSize, row * gridSize, gridSize, gridSize);
					e.Graphics.FillRectangle((row + col) % 2 == 0 ? Brushes.Gray : Brushes.White, rect);
				}
			}

			// Draw the ARGB color layer
			using (SolidBrush brush = new SolidBrush(this.ColorValue))
			{
				e.Graphics.FillRectangle(brush, ColorBox.ClientRectangle);
			}
		}

		private void RGB_Value_TextChanged(object sender, EventArgs e)
		{
			if (!IsLoading)
			{
				textChangedTimer.Stop();
				textChangedTimer.Start();

				activeTextBox = sender as TextBox;

				// Restart the timer on each text change
				textChangedTimer.Stop();
				textChangedTimer.Start();
			}
		}
		private void RGB_Value_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Allow only digits and control characters (like backspace)
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}
		private void RGB_Value_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up)
			{
				IncrementValue(sender as TextBox);
				e.Handled = true; // Prevent default behavior
			}
			else if (e.KeyCode == Keys.Down)
			{
				DecrementValue(sender as TextBox);
				e.Handled = true; // Prevent default behavior
			}
		}
		private void RGB_Value_TimerTick(object sender, EventArgs e)
		{
			// Stop the timer
			textChangedTimer.Stop();

			// Validate and correct the input value for the active textbox
			if (activeTextBox != null && int.TryParse(activeTextBox.Text, out int currentValue))
			{
				if (currentValue < 0)
				{
					activeTextBox.Text = "0";
				}
				else if (currentValue > 255)
				{
					activeTextBox.Text = "255";
				}
				// Move the cursor to the end of the text
				activeTextBox.SelectAll();

				SetColorFrom(Color.FromArgb(
					Convert.ToInt32(A_Value.Text),
					Convert.ToInt32(R_Value.Text),
					Convert.ToInt32(G_Value.Text),
					Convert.ToInt32(B_Value.Text)
				));
			}
			else if (activeTextBox != null && activeTextBox.Text != string.Empty)
			{
				activeTextBox.Text = "0";
			}
		}
		private void RGB_Value__Enter(object sender, EventArgs e)
		{
			// Select all text when the TextBox gains focus
			var textBox = sender as TextBox;
			if (textBox != null)
			{
				BeginInvoke((Action)delegate
				{
					textBox.SelectAll();
				});
			}
		}

		private void txtHtmlValue_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (!IsLoading)
				{
					SetColorFrom(ColorTranslator.FromHtml(txtHtmlValue.Text));
				}
			}
			catch { }
		}
		private void txtHtmlValue_Enter(object sender, EventArgs e)
		{
			// Raise the UserControl's Enter event
			OnEnter(e);
		}

		//The ColorTranslator.ToHtml method does not include the alpha component
		private string ColorToHtmlWithAlpha(Color color)
		{
			return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
		}


		/// <summary>Set and Show the selected color in all the Controls</summary><param name="Value">Color to show</param>
		public void SetColorFrom(Color Value)
		{
			try
			{
				IsLoading = true;
				ColorValue = Value;
				ColorBox.BackColor = this.ColorValue;

				A_Value.Text = ColorValue.A.ToString();
				R_Value.Text = ColorValue.R.ToString();
				G_Value.Text = ColorValue.G.ToString();
				B_Value.Text = ColorValue.B.ToString();

				txtHtmlValue.Text = ColorToHtmlWithAlpha(ColorValue);

				lbTime.Text = string.Format("{0}", ColorValue.ToArgb().ToString());
				lblAlpha.Text = string.Format("A:{0:n1}%", Convert.ToInt32(A_Value.Text) * 100 / 255);
				IsLoading = false;

				OnColorChanged?.Invoke(this.ColorValue, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		/// <summary>Sets the selected color in all the Controls</summary><param name="Value">Color to show</param>
		public void SetColorFrom(int Value)
		{
			SetColorFrom(Color.FromArgb(Value));
		}
		/// <summary>Sets the selected color in all the Controls</summary><param name="Value">Color to show, can be an INT, HEX, HTML, 'R;G;B;A' or a Named color</param>
		public void SetColorFrom(string Value)
		{
			SetColorFrom(ColorTranslator.FromHtml(Value));
		}
		/// <summary>Sets the selected color in all the Controls</summary><param name="Value">Color to show, can be RGB, ARGB or TARGB (Time).</param>
		public void SetColorFrom(int[] RGBA_Values)
		{
			if (RGBA_Values.Length == 3) //<- RGB
			{
				SetColorFrom(Color.FromArgb(RGBA_Values[0], RGBA_Values[1], RGBA_Values[2]));
			}
			if (RGBA_Values.Length == 4) //<- ARGB
			{
				SetColorFrom(Color.FromArgb(RGBA_Values[0], RGBA_Values[1], RGBA_Values[2], RGBA_Values[3]));
			}
			if (RGBA_Values.Length == 5) //<- TARGB, T=Timeframe
			{
				this.Time = RGBA_Values[0];
				lbTime.Text = string.Format("T:{0}", RGBA_Values[0]);
				SetColorFrom(Color.FromArgb(RGBA_Values[1], RGBA_Values[2], RGBA_Values[3], RGBA_Values[4]));
			}
		}
		/// <summary>Sets the selected color in all the Controls</summary><param name="Value">Color to show, can be RGB, ARGB or TARGB (Time).</param>
		public void SetColorFrom(decimal[] RGBA_Values)
		{
			if (RGBA_Values.Length == 3) //<- RGB
			{
				SetColorFrom(Color.FromArgb((int)RGBA_Values[0], (int)RGBA_Values[1], (int)RGBA_Values[2]));
			}
			if (RGBA_Values.Length == 4) //<- ARGB
			{
				SetColorFrom(Color.FromArgb((int)RGBA_Values[0], (int)RGBA_Values[1], (int)RGBA_Values[2], (int)RGBA_Values[3]));
			}
			if (RGBA_Values.Length == 5) //<- TARGB, T=Timeframe
			{
				this.Time = RGBA_Values[0];
				lbTime.Text = string.Format("T:{0:n4}", RGBA_Values[0]);
				SetColorFrom(Color.FromArgb((int)RGBA_Values[1], (int)RGBA_Values[2], (int)RGBA_Values[3], (int)RGBA_Values[4]));
			}
		}



		private void IncrementValue(TextBox control)
		{
			if (int.TryParse(control.Text, out int currentValue))
			{
				if (currentValue < 255)
				{
					control.Text = (currentValue + 1).ToString();
				}
			}
		}
		private void DecrementValue(TextBox control)
		{
			if (int.TryParse(control.Text, out int currentValue))
			{
				if (currentValue > 0)
				{
					control.Text = (currentValue - 1).ToString();
				}
			}
		}

		
	}
}
