using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Skins;
using System.Collections;

namespace EDHM_UI_mk2.Forms
{
	public partial class ThemeColors : DevExpress.XtraEditors.XtraForm
	{
		public SkinColors CurrentSkin { get; set; }

		public ThemeColors()
		{
			InitializeComponent();
		}
		public ThemeColors(SkinColors currentSkin)
		{
			InitializeComponent();
			this.CurrentSkin = currentSkin;
		}		

		private void ThemeColors_Load(object sender, EventArgs e)
		{
			
		}

		private void ThemeColors_Shown(object sender, EventArgs e)
		{
			if (this.CurrentSkin != null)
			{
				//DisabledControl, HotTrackedForeColor, HighlightText, Menu, InactiveCaptionText
				//Highlight, DisabledText, InfoText, Window, Info
				//Success, Information, QuestionFill, Critical, WindowText
				//Warning, Primary, WarningFill, Danger, Question
				//Control, HotTrackedColor, ControlText, HighlightAlternate, MenuText,
				//HideSelection, ReadOnly

				int posY = 4;

				#region Color 1

				LabelControl lbl1 = new LabelControl
				{
					Text = "DisabledControl",
					Location = new Point(4, posY)
				};
				Panel pnl1 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("DisabledControl"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl1.Height,
					Visible = true
				};
				posY += lbl1.Height + 2;

				this.Controls.Add(lbl1);
				this.Controls.Add(pnl1);

				#endregion

				#region Color 2

				LabelControl lbl2 = new LabelControl
				{
					Text = "HotTrackedForeColor",
					Location = new Point(4, posY)
				};
				Panel pnl2 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("HotTrackedForeColor"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl2.Height,
					Visible = true
				};
				posY += lbl2.Height + 2;

				this.Controls.Add(lbl2);
				this.Controls.Add(pnl2);

				#endregion

				#region Color 3

				LabelControl lbl3 = new LabelControl
				{
					Text = "HighlightText",
					Location = new Point(4, posY),
					Visible = true
				};
				Panel pnl3 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("HighlightText"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl3.Height,
					Visible = true
				};
				posY += lbl3.Height + 2;

				this.Controls.Add(lbl3);
				this.Controls.Add(pnl3);

				#endregion

				#region Color 4

				LabelControl lbl4 = new LabelControl
				{
					Text = "Menu",
					Location = new Point(4, posY)
				};
				Panel pnl4 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Menu"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl4.Height,
					Visible = true
				};
				posY += lbl4.Height + 2;

				this.Controls.Add(lbl4);
				this.Controls.Add(pnl4);

				#endregion

				#region Color 5

				LabelControl lbl5 = new LabelControl
				{
					Text = "InactiveCaptionText",
					Location = new Point(4, posY)
				};
				Panel pnl5 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("InactiveCaptionText"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl5.Height,
					Visible = true
				};
				posY += lbl5.Height + 2;

				this.Controls.Add(lbl5);
				this.Controls.Add(pnl5);

				#endregion

				#region Color 6

				LabelControl lbl6 = new LabelControl
				{
					Text = "Highlight",
					Location = new Point(4, posY)
				};
				Panel pnl6 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Highlight"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl6.Height,
					Visible = true
				};
				posY += lbl6.Height + 2;

				this.Controls.Add(lbl6);
				this.Controls.Add(pnl6);

				#endregion

				#region Color 7

				LabelControl lbl7 = new LabelControl
				{
					Text = "DisabledText",
					Location = new Point(4, posY)
				};
				Panel pnl7 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("DisabledText"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl7.Height,
					Visible = true
				};
				posY += lbl7.Height + 2;

				this.Controls.Add(lbl7);
				this.Controls.Add(pnl7);

				#endregion

				#region Color 8

				LabelControl lbl8 = new LabelControl
				{
					Text = "InfoText",
					Location = new Point(4, posY)
				};
				Panel pnl8 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("InfoText"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl8.Height,
					Visible = true
				};
				posY += lbl8.Height + 2;

				this.Controls.Add(lbl8);
				this.Controls.Add(pnl8);

				#endregion

				#region Color 9

				LabelControl lbl9 = new LabelControl
				{
					Text = "Window",
					Location = new Point(4, posY)
				};
				Panel pnl9 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Window"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl9.Height,
					Visible = true
				};
				posY += lbl9.Height + 2;

				this.Controls.Add(lbl9);
				this.Controls.Add(pnl9);

				#endregion

				#region Color 10

				LabelControl lbl10 = new LabelControl
				{
					Text = "Info",
					Location = new Point(4, posY)
				};
				Panel pnl10 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Info"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl10.Height,
					Visible = true
				};
				posY += lbl10.Height + 2;

				this.Controls.Add(lbl10);
				this.Controls.Add(pnl10);

				#endregion

				#region Color 11

				LabelControl lbl11 = new LabelControl
				{
					Text = "Success",
					Location = new Point(4, posY)
				};
				Panel pnl11 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Success"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl11.Height,
					Visible = true
				};
				posY += lbl11.Height + 2;

				this.Controls.Add(lbl11);
				this.Controls.Add(pnl11);

				#endregion

				#region Color 12

				LabelControl lbl12 = new LabelControl
				{
					Text = "Information",
					Location = new Point(4, posY)
				};
				Panel pnl12 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Information"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl12.Height,
					Visible = true
				};
				posY += lbl12.Height + 2;

				this.Controls.Add(lbl12);
				this.Controls.Add(pnl12);

				#endregion

				#region Color 13

				LabelControl lbl13 = new LabelControl
				{
					Text = "QuestionFill",
					Location = new Point(4, posY)
				};
				Panel pnl13 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("QuestionFill"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl13.Height,
					Visible = true
				};
				posY += lbl13.Height + 2;

				this.Controls.Add(lbl13);
				this.Controls.Add(pnl13);

				#endregion

				#region Color 14

				LabelControl lbl14 = new LabelControl
				{
					Text = "Critical",
					Location = new Point(4, posY)
				};
				Panel pnl14 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Critical"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl14.Height,
					Visible = true
				};
				posY += lbl14.Height + 2;

				this.Controls.Add(lbl14);
				this.Controls.Add(pnl14);

				#endregion

				#region Color 15

				LabelControl lbl15 = new LabelControl
				{
					Text = "WindowText",
					Location = new Point(4, posY)
				};
				Panel pnl15 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("WindowText"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl15.Height,
					Visible = true
				};
				posY += lbl15.Height + 2;

				this.Controls.Add(lbl15);
				this.Controls.Add(pnl15);

				#endregion

				#region Color 16

				LabelControl lbl16 = new LabelControl
				{
					Text = "Warning",
					Location = new Point(4, posY)
				};
				Panel pnl16 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Warning"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl16.Height,
					Visible = true
				};
				posY += lbl16.Height + 2;

				this.Controls.Add(lbl16);
				this.Controls.Add(pnl16);

				#endregion

				#region Color 17

				LabelControl lbl17 = new LabelControl
				{
					Text = "Primary",
					Location = new Point(4, posY)
				};
				Panel pnl17 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Primary"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl17.Height,
					Visible = true
				};
				posY += lbl17.Height + 2;

				this.Controls.Add(lbl17);
				this.Controls.Add(pnl17);

				#endregion

				#region Color 18

				LabelControl lbl18 = new LabelControl
				{
					Text = "WarningFill",
					Location = new Point(4, posY)
				};
				Panel pnl18 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("WarningFill"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl18.Height,
					Visible = true
				};
				posY += lbl18.Height + 2;

				this.Controls.Add(lbl18);
				this.Controls.Add(pnl18);

				#endregion

				#region Color 19

				LabelControl lbl19 = new LabelControl
				{
					Text = "Danger",
					Location = new Point(4, posY)
				};
				Panel pnl19 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Danger"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = lbl19.Height,
					Visible = true
				};
				posY += lbl19.Height + 2;

				this.Controls.Add(lbl19);
				this.Controls.Add(pnl19);

				#endregion

				#region Color 20

				LabelControl lbl20 = new LabelControl
				{
					Text = "Question",
					Location = new Point(4, posY)
				};
				Panel pnl20 = new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Question"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				};
				posY += 14 + 2;

				this.Controls.Add(lbl20);
				this.Controls.Add(pnl20);

				#endregion

				#region Color 21

				this.Controls.Add(new LabelControl
				{
					Text = "Control",
					Location = new Point(4, posY)
				});
				this.Controls.Add(new Panel
				{
					BackColor = this.CurrentSkin.GetColor("Control"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				});
				posY += 16;

				#endregion

				#region Color 22

				this.Controls.Add(new LabelControl
				{
					Text = "HotTrackedColor",
					Location = new Point(4, posY)
				});
				this.Controls.Add(new Panel
				{
					BackColor = this.CurrentSkin.GetColor("HotTrackedColor"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				});
				posY += 16;

				#endregion

				#region Color 23

				this.Controls.Add(new LabelControl
				{
					Text = "ControlText",
					Location = new Point(4, posY)
				});
				this.Controls.Add(new Panel
				{
					BackColor = this.CurrentSkin.GetColor("ControlText"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				});
				posY += 16;

				#endregion

				#region Color 24

				this.Controls.Add(new LabelControl
				{
					Text = "HighlightAlternate",
					Location = new Point(4, posY)
				});
				this.Controls.Add(new Panel
				{
					BackColor = this.CurrentSkin.GetColor("HighlightAlternate"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				});
				posY += 16;

				#endregion

				#region Color 25

				this.Controls.Add(new LabelControl
				{
					Text = "MenuText",
					Location = new Point(4, posY)
				});
				this.Controls.Add(new Panel
				{
					BackColor = this.CurrentSkin.GetColor("MenuText"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				});
				posY += 16;

				#endregion

				#region Color 26

				this.Controls.Add(new LabelControl
				{
					Text = "HideSelection",
					Location = new Point(4, posY)
				});
				this.Controls.Add(new Panel
				{
					BackColor = this.CurrentSkin.GetColor("HideSelection"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				});
				posY += 16;

				#endregion

				#region Color 27

				this.Controls.Add(new LabelControl
				{
					Text = "ReadOnly",
					Location = new Point(4, posY)
				});
				this.Controls.Add(new Panel
				{
					BackColor = this.CurrentSkin.GetColor("ReadOnly"),
					BorderStyle = BorderStyle.FixedSingle,
					Location = new Point(200, posY),
					Height = 14,
					Visible = true
				});
				posY += 16;

				#endregion

			}
		}
	}
}