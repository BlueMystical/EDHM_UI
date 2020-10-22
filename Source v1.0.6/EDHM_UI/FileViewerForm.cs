using System;
using System.Windows.Forms;

namespace EDHM_UI
{
	public partial class FileViewerForm : Form
	{
		public string[] FileLines { get; set; }
		private int LineNumber = 0;

		public FileViewerForm(string[] _Lines, int _Number)
		{
			InitializeComponent();
			this.FileLines = _Lines;
			this.LineNumber = _Number;
		}

		private void FileViewerForm_Load(object sender, EventArgs e)
		{
			if (this.FileLines != null && this.FileLines.Length > 0)
			{
				int i = 0;
				foreach (string _Line in this.FileLines)
				{
					i++;
					ListViewItem Fila = new ListViewItem();
					Fila = this.listView1.Items.Add(i.ToString());       //1a Columna
					Fila.SubItems.Add(_Line);							//2a Columna
				}
				this.listView1.Items.Find(this.LineNumber.ToString(), false);
			}
		}
	}
}
