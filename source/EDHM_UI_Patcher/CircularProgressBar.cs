using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EDHM_UI_Patcher
{
	public class CircularProgressBar : Control
	{
		#region Enums

		public enum _ProgressShape
		{
			Round,
			Flat
		}

		public enum _TextMode
		{
			None,
			Value,
			Percentage,
			Custom
		}

		#endregion

		#region Private Variables

		private long _Value;
		private long _Value2;
		private long _Maximum = 100;
		private Color _ProgressColor1 = Color.Orange;
		private Color _ProgressColor2 = Color.Orange;
		private Color _InnerBarColor = Color.Orange;
		private Color _LineColor = Color.Silver;
		private _ProgressShape ProgressShapeVal;
		private _TextMode ProgressTextMode;

		#endregion

		#region Contructor

		public CircularProgressBar()
		{
			this.Size = new Size(130, 130);
			this.Font = new Font("Segoe UI", 15);
			this.FontInner = new Font("Segoe UI", 11);
			this.MinimumSize = new Size(100, 100);
			this.DoubleBuffered = true;
			this.Value = 57;
			this.Value_Inner = 25;
			this.ProgressShape = _ProgressShape.Flat;
			this.TextMode = _TextMode.Percentage;
			this.ForeColor = Color.DimGray;
		}

		#endregion

		#region Public Custom Properties

		/// <summary>Determina el Valor del Progreso</summary>
		[Description("Value for the Bar"), Category("Data")]
		public long Value
		{
			get { return this._Value; }
			set
			{
				this._Value = (value > this._Maximum ? this._Maximum : value);
				Invalidate();
			}
		}

		[Description("Value for the Second Bar"), Category("Data")]
		public long Value_Inner
		{
			get { return this._Value2; }
			set
			{
				this._Value2 = (value > this._Maximum ? this._Maximum : value);
				Invalidate();
			}
		}

		[Description("Maximum Value"), Category("Data")]
		public long Maximum
		{
			get { return this._Maximum; }
			set
			{
				if (value < 1)
					value = 1;
				this._Maximum = value;
				Invalidate();
			}
		}

		[Description("Initial color for the Bar"), Category("Appearance")]
		public Color ProgressColor1
		{
			get { return this._ProgressColor1; }
			set
			{
				this._ProgressColor1 = value;
				Invalidate();
			}
		}

		[Description("Ending color for the Bar"), Category("Appearance")]
		public Color ProgressColor2
		{
			get { return this._ProgressColor2; }
			set
			{
				this._ProgressColor2 = value;
				Invalidate();
			}
		}

		[Description("Color for the Inner Progressbar"), Category("Appearance")]
		public Color InnerBarColor
		{
			get { return this._InnerBarColor; }
			set
			{
				this._InnerBarColor = value;
				Invalidate();
			}
		}

		[Description("Color for the Middle Line"), Category("Appearance")]
		public Color LineColor
		{
			get { return this._LineColor; }
			set
			{
				this._LineColor = value;
				Invalidate();
			}
		}

		/// <summary>Obtiene o Establece la Forma de los terminales de la barra de progreso.</summary>
		[Description("Shape for the Endpoints of the bar"), Category("Appearance")]
		public _ProgressShape ProgressShape
		{
			get { return this.ProgressShapeVal; }
			set
			{
				this.ProgressShapeVal = value;
				Invalidate();
			}
		}

		/// <summary>Obtiene o Establece el Modo como se muestra el Texto dentro de la barra de Progreso.</summary>
		[Description("Obtiene o Establece el Modo como se muestra el Texto dentro de la barra de Progreso."), Category("Data")]
		public _TextMode TextMode
		{
			get { return this.ProgressTextMode; }
			set
			{
				this.ProgressTextMode = value;
				Invalidate();
			}
		}

		/// <summary>Texto que se muestra dentro del Control</summary>
		[Description("Custom Text for the Control"), Category("Data")]
		public override string Text { get; set; }

		[Description("Custom Text for the InnerBar"), Category("Data")]
		public string TextInner { get; set; }

		[Description("The image associated with the control"), Category("Appearance")]
		public Image InnerPicture { get; set; }

		[Description("Font for the Inner Text"), Category("Appearance")]
		public Font FontInner { get; set; }

		#endregion

		#region Events

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			SetStandardSize();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			SetStandardSize();
		}

		protected override void OnPaintBackground(PaintEventArgs p)
		{
			base.OnPaintBackground(p);
		}

		#endregion

		#region Methods

		private void SetStandardSize()
		{
			int _Size = Math.Max(this.Width, this.Height);
			this.Size = new Size(_Size, _Size);
		}

		public void Increment(int Val)
		{
			this._Value += Val;
			Invalidate();
		}

		public void Decrement(int Val)
		{
			this._Value -= Val;
			Invalidate();
		}

		private Bitmap ResizeImage(Bitmap originalBitmap, int newWidth, int maxHeight, bool onlyResizeIfWider)
		{
			if (onlyResizeIfWider)
			{
				if (originalBitmap.Width <= newWidth)
				{
					newWidth = originalBitmap.Width;
				}
			}

			int newHeight = originalBitmap.Height * newWidth / originalBitmap.Width;
			if (newHeight > maxHeight)
			{
				// Resize with height instead
				newWidth = originalBitmap.Width * maxHeight / originalBitmap.Height;
				newHeight = maxHeight;
			}

			var alteredImage = new Bitmap(originalBitmap, new Size(newWidth, newHeight));
			alteredImage.SetResolution(72, 72);
			return alteredImage;
		}

		#endregion

		#region Events

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			using (Bitmap bitmap = new Bitmap(this.Width, this.Height))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.Clear(this.BackColor);

					#region Dibuja la Imagen de Fondo

					if (this.BackgroundImage != null)
					{
						graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
						graphics.DrawImage(ResizeImage((Bitmap)this.BackgroundImage,
							this.Width - 0x29, this.Height - 0x28, true), new Point(1, 1));
					}

					#endregion

					#region Dibuja la Linea

					if (this.Value > 0 || this.Value_Inner > 0)
					{
						using (Pen pen2 = new Pen(this.LineColor))
						{
							graphics.DrawEllipse(pen2, 0x18 - 6, 0x18 - 6, (this.Width - 0x30) + 12, (this.Height - 0x30) + 12);
						}
					}

					#endregion

					#region Dibuja la Barra de Progreso

					using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, this._ProgressColor1, this._ProgressColor2, LinearGradientMode.ForwardDiagonal))
					{
						using (Pen pen = new Pen(brush, 14f))
						{
							switch (this.ProgressShapeVal)
							{
								case _ProgressShape.Round:
									pen.StartCap = LineCap.Round;
									pen.EndCap = LineCap.Round;
									break;

								case _ProgressShape.Flat:
									pen.StartCap = LineCap.Flat;
									pen.EndCap = LineCap.Flat;
									break;
							}
							//Aqui se dibuja realmente la Barra de Progreso
							graphics.DrawArc(pen, 0x12, 0x12, (this.Width - 0x23) - 2, (this.Height - 0x23) - 2, -90,
										 (int)Math.Round((double)((360.0 / ((double)this._Maximum)) * this._Value)));
						}
					}

					#endregion

					#region Dibuja la Barra de Progreso Interior

					using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, this._InnerBarColor, this._InnerBarColor, LinearGradientMode.ForwardDiagonal))
					{
						using (Pen pen = new Pen(brush, 10f))
						{
							switch (this.ProgressShapeVal)
							{
								case _ProgressShape.Round:
									pen.StartCap = LineCap.Round;
									pen.EndCap = LineCap.Round;
									break;

								case _ProgressShape.Flat:
									pen.StartCap = LineCap.Flat;
									pen.EndCap = LineCap.Flat;
									break;
							}
							//Aqui se dibuja realmente la Barra de Progreso
							graphics.DrawArc(pen, 0x12, 0x12, (this.Width - 0x23) - 2, (this.Height - 0x23) - 2, -90,
										 (int)Math.Round((double)((360.0 / ((double)this._Maximum)) * this._Value2)));
						}
					}

					#endregion

					#region Dibuja el Texto de Progreso:

					if (this.Value > 0)
					{
						this.Text = string.Empty;
						switch (this.TextMode)
						{
							case _TextMode.None:
								this.Text = string.Empty;
								break;

							case _TextMode.Value:
								this.Text = this._Value.ToString();
								break;

							case _TextMode.Percentage:
								this.Text = Convert.ToString(Convert.ToInt32((100 / this._Maximum) * this._Value)) + "%";
								break;

							case _TextMode.Custom:
								break;

							default:
								break;
						}

						if (this.Text != string.Empty)
						{
							Brush FontColor = new SolidBrush(this.ForeColor);
							SizeF MS = graphics.MeasureString(this.Text, this.Font);

							graphics.DrawString(this.Text, this.Font, FontColor,
								Convert.ToInt32(this.Width / 2 - MS.Width / 2),
								Convert.ToInt32(this.Height / 2 - MS.Height / 2) -10);
						}
					}
					#endregion

					#region Dibuja el Texto de Progreso Interior:

					if (this.Value_Inner > 0)
					{
						this.TextInner = string.Empty;
						switch (this.TextMode)
						{
							case _TextMode.None:
								this.TextInner = string.Empty;
								break;

							case _TextMode.Value:
								this.TextInner = this._Value2.ToString();
								break;

							case _TextMode.Percentage:
								this.TextInner = Convert.ToString(Convert.ToInt32((100 / this._Maximum) * this._Value2)) + "%";
								break;

							case _TextMode.Custom:
								break;

							default:
								break;
						}

						if (this.TextInner != string.Empty)
						{
							Brush FontColor = new SolidBrush(this.ForeColor);
							SizeF MS = graphics.MeasureString(this.TextInner, this.FontInner);

							graphics.DrawString(this.TextInner, this.FontInner, FontColor,
								Convert.ToInt32(this.Width / 2 - MS.Width / 2),
								Convert.ToInt32(this.Height / 2 - MS.Height / 2) + 20);
						}
					}

					#endregion

					e.Graphics.DrawImage(bitmap, 0, 0);
					graphics.Dispose();
					bitmap.Dispose();
				}
			}
		}

		#endregion
	}
}