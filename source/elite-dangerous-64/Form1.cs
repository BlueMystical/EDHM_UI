using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace elite_dangerous_64
{
	public partial class Form1 : Form, IMessageFilter
	{
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		public const int WM_LBUTTONDOWN = 0x0201;

		[DllImportAttribute("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();

		private HashSet<Control> controlsToMove = new HashSet<Control>();

		public Form1()
		{
			InitializeComponent();

			Application.AddMessageFilter(this);

			controlsToMove.Add(this);
			controlsToMove.Add(this.label1);//Add whatever controls here you want to move the form when it is clicked and dragged
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			List<ed_ship> _Ships = new List<ed_ship>();
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Adder",
				ed_short = "adder",
				ship_name = "MyAdder",
				ship_ident = ""
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Anaconda",
				ed_short = "anaconda",
				ship_name = "Bite This",
				ship_ident = "AC-BT"
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Cobra Mk 3",
				ed_short = "cobramkiii",
				ship_name = "Cobra Kai",
				ship_ident = "CMK3-01"
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Diamondback Explorer",
				ed_short = "diamondbackxl",
				ship_name = "DBS Explorer",
				ship_ident = "DBS-01"
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Federal Corvette",
				ed_short = "federation_corvette",
				ship_name = "Vette",
				ship_ident = "FC-01"
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Fer-de-lance",
				ed_short = "ferdelance",
				ship_name = "Overpowered",
				ship_ident = "FDL-OP"
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Imperial Courier",
				ed_short = "empire_courier",
				ship_name = "Thunderbolt",
				ship_ident = "IC-TB"
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Imperial Cutter",
				ed_short = "empire_cutter",
				ship_name = "Golden Heart",
				ship_ident = "IC-GH"
			});
			_Ships.Add(new ed_ship
			{
				ed_full_name = "Krait Mk 3",
				ed_short = "krait_mkii",
				ship_name = "AlienHunter",
				ship_ident = "K3-AH"
			});

			this.comboBox1.DataSource = _Ships;
			this.comboBox1.DisplayMember = "ed_full_name";
			this.comboBox1.ValueMember = "ed_short";
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			//KeyPress puede detectar las siguientes Teclas:
			//a - z, A - Z; CTRL; Punctuation marks; ENTER; Number keys, both across the top of the keyboard and on the numeric keypad.
			if (e.KeyChar != (char)Keys.ControlKey)
			{
				//this.listBox1.Items.Add(string.Format("KeyPress: {0}", e.KeyChar.ToString()));
				this.lblStatus.Text = string.Format("KeyPress: {0}", e.KeyChar.ToString());
			}
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			// CTRL+G:
			//if (e.Control)
			//{
			string Modifiers = e.Modifiers.ToString();
			this.lblStatus.Text = string.Format("KeyDown: {0}{1}",
				Modifiers != "None" ? Modifiers + " + " : string.Empty,
				e.KeyCode.ToString());
			//}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void comboBox1_DropDownClosed(object sender, EventArgs e)
		{
			string EDJournalDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous");
			if (Directory.Exists(EDJournalDir))
			{
				DirectoryInfo di = new DirectoryInfo(EDJournalDir);

				//Busca el Archivo de Log mas reciente:
				FileInfo JournalFile = di.GetFiles("Journal.*.log")
							.OrderByDescending(f => f.LastWriteTime).First();

				if (JournalFile != null)
				{
					ed_ship _Ship = this.comboBox1.SelectedItem as ed_ship;

					TextWriter tw = new StreamWriter(JournalFile.FullName, append: true);
					tw.WriteLine("{ \"timestamp\":\"2021 - 11 - 26T01: 53:47Z\", \"event\":\"Loadout\", \"Ship\":\"" + _Ship.ed_short + "\", \"ShipID\":6, \"ShipName\":\"" + _Ship.ship_name + "\", \"ShipIdent\":\"" + _Ship.ship_ident + "\" } ");
					tw.Close();

					this.lblStatus.Text = string.Format("Journal - Ship Loadout: {0}", _Ship.ed_short);
				}
			}
		}

		private void label1_Click(object sender, EventArgs e)
		{
			
		}
		private void label1_DoubleClick(object sender, EventArgs e)
		{
			OpenFileDialog OFDialog = new OpenFileDialog()
			{
				Filter = "Archivos de Imagenes|*.jpg;*.bmp;*.gif;*.png|Todos los archivos|*.*",
				FilterIndex = 0,
				DefaultExt = "png",
				AddExtension = true,
				CheckPathExists = true,
				CheckFileExists = true,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};

			if (OFDialog.ShowDialog() == DialogResult.OK)
			{
				this.BackgroundImage = Image.FromFile(OFDialog.FileName);
				this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
				this.ControlBox = false;
				this.Text = String.Empty;

				label1.BackColor = Color.Transparent;
				label1.ForeColor = Color.White;
				label1.Text = "ALT+F4 para Salir.";
				label1.TextAlign = ContentAlignment.MiddleRight;
			}
		}

		//Para mover la ventana sin titulo
		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == WM_LBUTTONDOWN &&
				 controlsToMove.Contains(Control.FromHandle(m.HWnd)))
			{
				ReleaseCapture();
				SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
				return true;
			}
			return false;
		}

		private void cmdEmbark_Click(object sender, EventArgs e)
		{
			string EDJournalDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous");
			if (Directory.Exists(EDJournalDir))
			{
				DirectoryInfo di = new DirectoryInfo(EDJournalDir);

				//Busca el Archivo de Log mas reciente:
				FileInfo JournalFile = di.GetFiles("Journal.*.log")
							.OrderByDescending(f => f.LastWriteTime).First();

				if (JournalFile != null)
				{
					ed_ship _Ship = this.comboBox1.SelectedItem as ed_ship;

					TextWriter tw = new StreamWriter(JournalFile.FullName, append: true);
					tw.WriteLine("{\"timestamp\":\"2023 - 11 - 27T08: 23:44Z\",\"event\":\"Embark\",\"SRV\":false,\"Taxi\":false,\"Multicrew\":false,\"ID\":72,\"StarSystem\":\"Bleae Thua WH-G b38-5\",\"SystemAddress\":11653454440777,\"Body\":\"Bleae Thua WH-G b38-5 A\",\"BodyID\":2,\"OnStation\":false,\"OnPlanet\":false} " );
					tw.Close();

					this.lblStatus.Text = string.Format("Journal - Embark: {0}", _Ship.ed_short);
				}
			}
		}
	}

	[Serializable]
	public class ed_ship
	{
		public ed_ship() { }

		public string ed_full_name { get; set; }
		public string ed_short { get; set; }
		public string ship_name { get; set; }
		public string ship_ident { get; set; }
	}
}
