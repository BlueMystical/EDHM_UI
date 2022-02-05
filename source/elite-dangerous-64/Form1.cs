using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace elite_dangerous_64
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			//KeyPress puede detectar las siguientes Teclas:
			//a - z, A - Z; CTRL; Punctuation marks; ENTER; Number keys, both across the top of the keyboard and on the numeric keypad.
			if (e.KeyChar != (char)Keys.ControlKey)
			{
				this.listBox1.Items.Add(string.Format("KeyPress: {0}", e.KeyChar.ToString()));
			}
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			string Modifiers = e.Modifiers.ToString();
			this.listBox1.Items.Add(string.Format("KeyDown: {0}{1}",
				Modifiers != "None" ? Modifiers + " + " : string.Empty,
				e.KeyCode.ToString()));

			// CTRL+G:
			if (e.KeyCode == Keys.G && (e.Control))
			{
				//GotoLine();
			}
		}
	}
}
