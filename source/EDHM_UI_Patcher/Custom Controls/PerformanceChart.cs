using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Chart_Test
{
	public partial class PerformanceChart : UserControl
	{
		#region *** Constants ***

		// Keep only a maximum MAX_VALUE_COUNT amount of values; This will allow
		private const int MAX_VALUE_COUNT = 512;
		// Draw a background grid with a fixed line spacing
		private const int GRID_SPACING = 16;

		#endregion

		#region *** Member Variables ***

		// Amount of currently visible values (calculated from control width and value spacing)
		private int visibleValues = 0;
		// Horizontal value space in Pixels
		private int valueSpacing = 5;
		// The currently highest displayed value, required for Relative Scale Mode
		private decimal currentMaxValue = 0;
		// Offset value for the scrolling grid
		private int gridScrollOffset = 0;
		// The current average value
		private decimal averageValue = 0;
		// Border Style
		private Border3DStyle b3dstyle = Border3DStyle.Sunken;
		// Scale mode for value aspect ratio
		private ScaleMode scaleMode = ScaleMode.Absolute;
		// Timer Mode
		private TimerMode timerMode;
		// List of stored values
		private List<decimal> drawValues = new List<decimal>(MAX_VALUE_COUNT);
		// Value queue for Timer Modes
		private Queue<decimal> waitingValues = new Queue<decimal>();
		// Style and Design
		private PerfChartStyle perfChartStyle;

		#endregion

		#region Constructor

		public PerformanceChart()
		{
			InitializeComponent();

			// Initialize Variables
			perfChartStyle = new PerfChartStyle();

			// Set Optimized Double Buffer to reduce flickering
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

			// Redraw when resized
			this.SetStyle(ControlStyles.ResizeRedraw, true);

			this.Font = SystemInformation.MenuFont;
		} 

		#endregion

		#region *** Properties ***

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Appearance"), Description("Appearance and Style")]
		public PerfChartStyle PerfChartStyle
		{
			get { return perfChartStyle; }
			set { perfChartStyle = value; }
		}


		[DefaultValue(typeof(Border3DStyle), "Sunken"), Description("BorderStyle"), Category("Appearance")]
		new public Border3DStyle BorderStyle
		{
			get
			{
				return b3dstyle;
			}
			set
			{
				b3dstyle = value;
				Invalidate();
			}
		}

		public ScaleMode ScaleMode
		{
			get { return scaleMode; }
			set { scaleMode = value; }
		}

		public TimerMode TimerMode
		{
			get { return timerMode; }
			set
			{
				if (value == TimerMode.Disabled)
				{
					// Stop and append only when changed
					if (timerMode != TimerMode.Disabled)
					{
						timerMode = value;

						tmrRefresh.Stop();
						// If there are any values in the queue, append them
						ChartAppendFromQueue();
					}
				}
				else
				{
					timerMode = value;
					tmrRefresh.Start();
				}
			}
		}

		public int TimerInterval
		{
			get { return tmrRefresh.Interval; }
			set
			{
				if (value < 15)
					throw new ArgumentOutOfRangeException("TimerInterval", value, "The Timer interval must be greater then 15");
				else
					tmrRefresh.Interval = value;
			}
		}

		#endregion

		#region *** Public Methods ***

		/// <summary>
		/// Clears the whole chart
		/// </summary>
		public void Clear()
		{
			drawValues.Clear();
			Invalidate();
		}


		/// <summary>
		/// Adds a value to the Chart Line
		/// </summary>
		/// <param name="value">progress value</param>
		public void AddValue(decimal value)
		{
			if (scaleMode == ScaleMode.Absolute && value > 100M)
				throw new Exception(String.Format("Values greater then 100 not allowed in ScaleMode: Absolute ({0})", value));

			switch (timerMode)
			{
				case TimerMode.Disabled:
					ChartAppend(value);
					Invalidate();
					break;
				case TimerMode.Simple:
				case TimerMode.SynchronizedAverage:
				case TimerMode.SynchronizedSum:
					// For all Timer Modes, the Values are stored in the Queue
					AddValueToQueue(value);
					break;
				default:
					throw new Exception(String.Format("Unsupported TimerMode: {0}", timerMode));
			}
		}

		#endregion
		
		#region *** Private Methods: Common ***

		/// <summary>
		/// Add value to the queue for a timed refresh
		/// </summary>
		/// <param name="value"></param>
		private void AddValueToQueue(decimal value)
		{
			waitingValues.Enqueue(value);
		}


		/// <summary>
		/// Appends value <paramref name="value"/> to the chart (without redrawing)
		/// </summary>
		/// <param name="value">performance value</param>
		private void ChartAppend(decimal value)
		{
			// Insert at first position; Negative values are flatten to 0 (zero)
			drawValues.Insert(0, Math.Max(value, 0));

			// Remove last item if maximum value count is reached
			if (drawValues.Count > MAX_VALUE_COUNT)
				drawValues.RemoveAt(MAX_VALUE_COUNT);

			// Calculate horizontal grid offset for "scrolling" effect
			gridScrollOffset += valueSpacing;
			if (gridScrollOffset > GRID_SPACING)
				gridScrollOffset = gridScrollOffset % GRID_SPACING;
		}


		/// <summary>
		/// Appends Values from queue
		/// </summary>
		private void ChartAppendFromQueue()
		{
			// Proceed only if there are values at all
			if (waitingValues.Count > 0)
			{
				if (timerMode == TimerMode.Simple)
				{
					while (waitingValues.Count > 0)
						ChartAppend(waitingValues.Dequeue());
				}
				else if (timerMode == TimerMode.SynchronizedAverage ||
						 timerMode == TimerMode.SynchronizedSum)
				{
					// appendValue variable is used for calculating the average or sum value
					decimal appendValue = Decimal.Zero;
					int valueCount = waitingValues.Count;

					while (waitingValues.Count > 0)
						appendValue += waitingValues.Dequeue();

					// Calculate Average value in SynchronizedAverage Mode
					if (timerMode == TimerMode.SynchronizedAverage)
						appendValue = appendValue / (decimal)valueCount;

					// Finally append the value
					ChartAppend(appendValue);
				}
			}
			else
			{
				// Always add 0 (Zero) if there are no values in the queue
				ChartAppend(Decimal.Zero);
			}

			// Refresh the Chart
			Invalidate();
		}

		/// <summary>
		/// Calculates the vertical Position of a value in relation the chart size,
		/// Scale Mode and, if ScaleMode is Relative, to the current maximum value
		/// </summary>
		/// <param name="value">performance value</param>
		/// <returns>vertical Point position in Pixels</returns>
		private int CalcVerticalPosition(decimal value)
		{
			decimal result = Decimal.Zero;

			if (scaleMode == ScaleMode.Absolute)
				result = value * this.Height / 100;
			else if (scaleMode == ScaleMode.Relative)
				result = (currentMaxValue > 0) ? (value * this.Height / currentMaxValue) : 0;

			result = this.Height - result;

			return Convert.ToInt32(Math.Round(result));
		}


		/// <summary>
		/// Returns the currently highest (displayed) value, for Relative ScaleMode
		/// </summary>
		/// <returns></returns>
		private decimal GetHighestValueForRelativeMode()
		{
			decimal maxValue = 0;

			for (int i = 0; i < visibleValues; i++)
			{
				// Set if higher then previous max value
				if (drawValues[i] > maxValue)
					maxValue = drawValues[i];
			}

			return maxValue;
		}

		#endregion
		
		#region *** Private Methods: Drawing ***

		/// <summary>
		/// Draws the chart (w/o background or grid, but with border) to the Graphics canvas
		/// </summary>
		/// <param name="g">Graphics</param>
		private void DrawChart(Graphics g)
		{
			visibleValues = Math.Min(this.Width / valueSpacing, drawValues.Count);

			if (scaleMode == ScaleMode.Relative)
				currentMaxValue = GetHighestValueForRelativeMode();

			// Dirty little "trick": initialize the first previous Point outside the bounds
			Point previousPoint = new Point(Width + valueSpacing, Height);
			Point currentPoint = new Point();

			// Only draw average line when possible (visibleValues) and needed (style setting)
			if (visibleValues > 0 && perfChartStyle.ShowAverageLine)
			{
				averageValue = 0;
				DrawAverageLine(g);
			}

			// Connect all visible values with lines
			for (int i = 0; i < visibleValues; i++)
			{
				currentPoint.X = previousPoint.X - valueSpacing;
				currentPoint.Y = CalcVerticalPosition(drawValues[i]);

				// Actually draw the line
				g.DrawLine(perfChartStyle.ChartLinePen.Pen, previousPoint, currentPoint);

				previousPoint = currentPoint;
			}

			// Draw current relative maximum value stirng
			if (scaleMode == ScaleMode.Relative)
			{
				//SolidBrush sb = new SolidBrush(perfChartStyle.ChartLinePen.Color);
				SolidBrush sb = new SolidBrush(this.ForeColor);
				g.DrawString(currentMaxValue.ToString(), this.Font, sb, 4.0f, 2.0f);
			}

			// Draw Border on top
			ControlPaint.DrawBorder3D(g, 0, 0, Width, Height, b3dstyle);
		}
		
		private void DrawAverageLine(Graphics g)
		{
			for (int i = 0; i < visibleValues; i++)
				averageValue += drawValues[i];

			averageValue = averageValue / visibleValues;

			int verticalPosition = CalcVerticalPosition(averageValue);
			g.DrawLine(perfChartStyle.AvgLinePen.Pen, 0, verticalPosition, Width, verticalPosition);
		}

		/// <summary>
		/// Draws the background gradient and the grid into Graphics <paramref name="g"/>
		/// </summary>
		/// <param name="g">Graphic</param>
		private void DrawBackgroundAndGrid(Graphics g)
		{
			// Draw the background Gradient rectangle
			Rectangle baseRectangle = new Rectangle(0, 0, this.Width, this.Height);
			using (Brush gradientBrush = new LinearGradientBrush(baseRectangle, perfChartStyle.BackgroundColorTop, perfChartStyle.BackgroundColorBottom, LinearGradientMode.Vertical))
			{
				g.FillRectangle(gradientBrush, baseRectangle);
			}

			// Draw all visible, vertical gridlines (if wanted)
			if (perfChartStyle.ShowVerticalGridLines)
			{
				for (int i = Width - gridScrollOffset; i >= 0; i -= GRID_SPACING)
				{
					g.DrawLine(perfChartStyle.VerticalGridPen.Pen, i, 0, i, Height);
				}
			}

			// Draw all visible, horizontal gridlines (if wanted)
			if (perfChartStyle.ShowHorizontalGridLines)
			{
				for (int i = 0; i < Height; i += GRID_SPACING)
				{
					g.DrawLine(perfChartStyle.HorizontalGridPen.Pen, 0, i, Width, i);
				}
			}
		}

		#endregion
		
		#region *** Overrides ***

		/// Override OnPaint method
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// Enable AntiAliasing, if needed
			if (perfChartStyle.AntiAliasing)
				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			DrawBackgroundAndGrid(e.Graphics);
			DrawChart(e.Graphics);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			Invalidate();
		}

		#endregion
		
		#region *** Event Handlers ***

		private void colorSet_ColorSetChanged(object sender, EventArgs e)
		{
			//Refresh Chart on Resize
			Invalidate();
		}

		private void tmrRefresh_Tick(object sender, EventArgs e)
		{
			// Don't execute event if running in design time
			if (this.DesignMode) return;

			ChartAppendFromQueue();
		}

		#endregion
	}


	/// <summary>
	/// Scale mode for value aspect ratio
	/// </summary>
	public enum ScaleMode
	{
		/// <summary>
		/// Absolute Scale Mode: Values from 0 to 100 are accepted and displayed
		/// </summary>
		Absolute,
		/// <summary>
		/// Relative Scale Mode: All values are allowed and displayed in a proper relation
		/// </summary>
		Relative
	}
	
	/// <summary>
	/// Chart Refresh Mode Timer Control Mode
	/// </summary>
	public enum TimerMode
	{
		/// <summary>
		/// Chart is refreshed when a value is added
		/// </summary>
		Disabled,
		/// <summary>
		/// Chart is refreshed every <c>TimerInterval</c> milliseconds, adding all values
		/// in the queue to the chart. If there are no values in the queue, a 0 (zero) is added
		/// </summary>
		Simple,
		/// <summary>
		/// Chart is refreshed every <c>TimerInterval</c> milliseconds, adding an average of
		/// all values in the queue to the chart. If there are no values in the queue,
		/// 0 (zero) is added
		/// </summary>
		SynchronizedAverage,
		/// <summary>
		/// Chart is refreshed every <c>TimerInterval</c> milliseconds, adding the sum of
		/// all values in the queue to the chart. If there are no values in the queue,
		/// 0 (zero) is added
		/// </summary>
		SynchronizedSum
	}

	[TypeConverterAttribute(typeof(ExpandableObjectConverter))]
	public class PerfChartStyle
	{
		private ChartPen verticalGridPen;
		private ChartPen horizontalGridPen;
		private ChartPen avgLinePen;
		private ChartPen chartLinePen;

		private Color backgroundColorTop = Color.DarkGreen;
		private Color backgroundColorBottom = Color.DarkGreen;

		private bool showVerticalGridLines = true;
		private bool showHorizontalGridLines = true;
		private bool showAverageLine = true;
		private bool antiAliasing = true;

		public PerfChartStyle()
		{
			verticalGridPen = new ChartPen();
			horizontalGridPen = new ChartPen();
			avgLinePen = new ChartPen();
			chartLinePen = new ChartPen();
		}

		public bool ShowVerticalGridLines
		{
			get { return showVerticalGridLines; }
			set { showVerticalGridLines = value; }
		}

		public bool ShowHorizontalGridLines
		{
			get { return showHorizontalGridLines; }
			set { showHorizontalGridLines = value; }
		}

		public bool ShowAverageLine
		{
			get { return showAverageLine; }
			set { showAverageLine = value; }
		}

		public ChartPen VerticalGridPen
		{
			get { return verticalGridPen; }
			set { verticalGridPen = value; }
		}

		public ChartPen HorizontalGridPen
		{
			get { return horizontalGridPen; }
			set { horizontalGridPen = value; }
		}

		public ChartPen AvgLinePen
		{
			get { return avgLinePen; }
			set { avgLinePen = value; }
		}

		public ChartPen ChartLinePen
		{
			get { return chartLinePen; }
			set { chartLinePen = value; }
		}

		public bool AntiAliasing
		{
			get { return antiAliasing; }
			set { antiAliasing = value; }
		}

		public Color BackgroundColorTop
		{
			get { return backgroundColorTop; }
			set { backgroundColorTop = value; }
		}

		public Color BackgroundColorBottom
		{
			get { return backgroundColorBottom; }
			set { backgroundColorBottom = value; }
		}
	}

	[TypeConverterAttribute(typeof(ExpandableObjectConverter))]
	public class ChartPen
	{
		private Pen pen;

		public ChartPen()
		{
			pen = new Pen(Color.Black);
		}

		public Color Color
		{
			get { return pen.Color; }
			set { pen.Color = value; }
		}

		public System.Drawing.Drawing2D.DashStyle DashStyle
		{
			get { return pen.DashStyle; }
			set { pen.DashStyle = value; }
		}

		public float Width
		{
			get { return pen.Width; }
			set { pen.Width = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Pen Pen
		{
			get { return pen; }
		}
	}


}
