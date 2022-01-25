using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDHM_UI_mk2.Forms
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void panelControl1_Paint(object sender, PaintEventArgs e)
		{
			Rectangle _Rec = new Rectangle(new Point(0, 0), new Size(20, 20));
			Pen _Color = new Pen(Color.Red);

			

			e.Graphics.FillRectangle(Brushes.AliceBlue, _Rec);
		}
	}
}
