using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ColorPickEditControl;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Popup;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraTab;
using EDHM_UI_mk2;

namespace Util_Test
{
	/* THIS IS A CUSTOM VERSION OF THE COLOR PICKER TO ALLOW THE 'STANDARD' COLORS TAB */

	[UserRepositoryItem("RegisterMyColorPickEdit")]
	public class RepositoryItemMyColorPickEdit : RepositoryItemColorPickEdit
	{
		static RepositoryItemMyColorPickEdit()
		{
			RegisterMyColorPickEdit();
		}

		public const string CustomEditName = "MyColorPickEdit";
		public RepositoryItemMyColorPickEdit()
		{
			// Initialize your custom standard colors
			this.myStandardColors = CreateMyStandardColors();
		}

		public override string EditorTypeName => CustomEditName;

		public static void RegisterMyColorPickEdit()
		{
			Image img = null;
			EditorRegistrationInfo.Default.Editors.Add(
				new EditorClassInfo(CustomEditName,
				typeof(MyColorPickEdit), typeof(RepositoryItemMyColorPickEdit),
				typeof(ColorEditViewInfo), new ColorEditPainter(), true, img));
		}

		// Custom property to check if your custom color tab is shown
		private bool showMyCustomColors = true;
		[Description("Gets or sets the visibility of the My Custom Tab"), DefaultValue(true)]
		public bool ShowMyCustomColors
		{
			get { return this.showMyCustomColors; }
			set
			{
				if (this.showMyCustomColors != value)
				{
					this.showMyCustomColors = value;
					OnPropertiesChanged();
				}
			}
		}

		// Custom property to check if your custom color tab is shown
		private bool showMyPastelColors = true;
		[Description("Gets or sets the visibility of the Pastel Colors Tab"), DefaultValue(false)]
		public bool ShowMyPastelColors
		{
			get { return this.showMyPastelColors; }
			set
			{
				if (this.showMyPastelColors != value)
				{
					this.showMyPastelColors = value;
					OnPropertiesChanged();
				}
			}
		}

		Matrix myStandardColors;
		[Description("Provides access to colors displayed in the My Standard Colors group in your Custom tab."), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Matrix MyStandardColors
		{
			get { return this.myStandardColors; }
			set
			{
				if (this.myStandardColors == value) return;
				this.myStandardColors = value;
				OnPropertiesChanged();
			}
		}

		public override void Assign(RepositoryItem item)
		{
			BeginUpdate();
			try
			{
				base.Assign(item);
				RepositoryItemMyColorPickEdit source = item as RepositoryItemMyColorPickEdit;
				if (source == null) return;
				// copy custom properties to clone repository items
				this.showMyCustomColors = source.ShowMyCustomColors;
				this.showMyPastelColors = source.ShowMyPastelColors;
				this.myStandardColors = source.MyStandardColors;
			}
			finally
			{
				EndUpdate();
			}
		}



		// Methods and properties for your custom standard colors
		private Color[,] myStandardColorArray = null;
		protected Color[,] MyStandardColorArray
		{
			get
			{
				if (this.myStandardColorArray == null)
					this.myStandardColorArray = CreateMyStandardColorsCore();
				return this.myStandardColorArray;
			}
		}

		protected virtual Color[,] CreateMyStandardColorsCore()
		{
			// Setup colors here
			return new Color[,] {
				{
					Color.White,
					Color.Red,
					Color.Orange,
					Color.Yellow,
					Color.Green,
					Color.Cyan,
					Color.Blue,
					Color.Purple,
					Color.White,
					Color.White,

					Color.FromArgb(224,224,224),					
					Color.FromArgb(255,192,192),
					Color.FromArgb(255,224,192),
					Color.FromArgb(255,255,192),
					Color.FromArgb(192,255,192),
					Color.FromArgb(192,255,255),
					Color.FromArgb(192,192,255),
					Color.FromArgb(255,192,255),
					Color.White,
					Color.White,

					Color.FromArgb(192,192,192),
					Color.FromArgb(255,128,128),
					Color.FromArgb(255,192,128),
					Color.FromArgb(255,255,128),
					Color.FromArgb(128,255,128),
					Color.FromArgb(128,255,255),
					Color.FromArgb(128,128,255),
					Color.FromArgb(255,128,255),
					Color.White,
					Color.White,

					Color.FromArgb(128,128,128),
					Color.FromArgb(255,0,0),
					Color.FromArgb(255,128,0),
					Color.FromArgb(255,255,0),
					Color.FromArgb(0,255,0),
					Color.FromArgb(0,255,255),
					Color.FromArgb(0,0,255),
					Color.FromArgb(255,0,255),
					Color.White,
					Color.White,

					Color.FromArgb(64,64,64),
					Color.FromArgb(192,0,0),
					Color.FromArgb(192,64,0),
					Color.FromArgb(192,192,0),
					Color.FromArgb(0,192,0),
					Color.FromArgb(0,192,192),
					Color.FromArgb(0,0,192),
					Color.FromArgb(192,0,192),
					Color.White,
					Color.White,

					Color.FromArgb(0,0,0),
					Color.FromArgb(128,0,0),
					Color.FromArgb(128,64,0),
					Color.FromArgb(128,128,0),
					Color.FromArgb(0,128,0),
					Color.FromArgb(0,128,128),
					Color.FromArgb(0,0,128),
					Color.FromArgb(128,0,128),
					Color.White,
					Color.White

				}
			};
		}
		protected virtual Matrix CreateMyStandardColors()
		{
			return Matrix.FromArray(this.MyStandardColorArray);
		}
	}

	[ToolboxItem(true)]
	public class MyColorPickEdit : ColorPickEdit
	{
		static MyColorPickEdit()
		{
			RepositoryItemMyColorPickEdit.RegisterMyColorPickEdit();
		}

		public MyColorPickEdit()
		{
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new RepositoryItemMyColorPickEdit Properties => base.Properties as RepositoryItemMyColorPickEdit;

		public override string EditorTypeName => RepositoryItemMyColorPickEdit.CustomEditName;

		protected override PopupBaseForm CreatePopupForm()
		{
			// Use custom popup form
			return new MyColorPickEditPopupForm(this);
		}
	}

	public class MyColorPickEditPopupForm : PopupColorPickEditForm
	{
		public MyColorPickEditPopupForm(MyColorPickEdit ownerEdit) : base(ownerEdit)
		{
			
		}

		protected override PopupColorBuilder CreatePopupColorEditBuilder()
		{
			// Use custom PopupColorBuilder
			return new MyPopupColorBuilder(this);
		}

		public class MyPopupColorBuilder : PopupColorBuilderEx
		{
			public MyPopupColorBuilder(IPopupColorEdit owner) : base(owner)
			{
				this.Properties.Buttons[0].Caption = "xxxx";
			}

			// Your custom control inside tab page (created similar to the control inside custom tab)
			private InnerColorPickControl myCustomTabInnerControl = null;
			private InnerColorPickControl myCustomTabInnerControl2 = null;
			public InnerColorPickControl MyCustomTabInnerControl
			{
				get
				{
					if (this.myCustomTabInnerControl == null)
					{
						this.myCustomTabInnerControl = CreateCustomTabInnerControl();
					}
					return this.myCustomTabInnerControl;
				}
			}
			public InnerColorPickControl MyCustomTabInnerControl2
			{
				get
				{
					if (this.myCustomTabInnerControl2 == null)
					{
						this.myCustomTabInnerControl2 = CreateCustomTabInnerControl();
					}
					return this.myCustomTabInnerControl2;
				}
			}

			protected override void RaiseColorPickDialogShowing(XtraForm frm)
			{
				if (frm.Controls.Count > 0) { }
				base.RaiseColorPickDialogShowing(frm);
			}

			protected override bool FindEditColor(Color color)
			{
				this.MyCustomTabInnerControl.SetColor(color, false); // Preserve "selected" state to the inner control
				return base.FindEditColor(color);
			}

			protected override void UpdateInnerControls()
			{
				base.UpdateInnerControls();

				DoAssign(this.MyCustomTabInnerControl); // Setup the additional InnerColorPickControl similar with the control in the default custom tab page

				// Customize the colors in your custom tab control
				this.MyCustomTabInnerControl.StandardColors.Clear();
				this.MyCustomTabInnerControl.ShowThemePalette = false;
				this.MyCustomTabInnerControl.StandardGroupCaption = "Standard Colors";
				this.MyCustomTabInnerControl2.ThemeGroupCaption = "Pastel Colors";
				if (this.Item is RepositoryItemMyColorPickEdit myItem)
				{
					this.MyCustomTabInnerControl.StandardColors.AddColorRange(myItem.MyStandardColors.ToList());
				}
			}

			protected override void SetTabPageProperties(int pageIndex, PopupBaseForm shadowForm)
			{
				if (pageIndex < 4) // Default tabs
					base.SetTabPageProperties(pageIndex, shadowForm);
				else
				{ // Custom tabs
					BaseStyleControl control = null;
					XtraTabPage page = this.TabControl.TabPages[pageIndex];
					switch (pageIndex)
					{
						case 4: // Copied from how default custom tab is created
							page.Text = "Standard Colors"; // Tab page caption
							this.MyCustomTabInnerControl.SelectedColorChanged += OnSelectedColorChanged;
							this.MyCustomTabInnerControl.MoreButtonClick += OnMoreButtonClick;
							this.MyCustomTabInnerControl.AutomaticButtonClick += OnAutomaticButtonClick;
							if (this.Item is RepositoryItemMyColorPickEdit myItem)
							{
								page.PageVisible = myItem.ShowMyCustomColors;
							}
							control = this.MyCustomTabInnerControl;
							break;

						case 5: // Copied from how default custom tab is created
							page.Text = "Pastel Colors"; // Tab page caption
							this.MyCustomTabInnerControl2.SelectedColorChanged += OnSelectedColorChanged;
							this.MyCustomTabInnerControl2.MoreButtonClick += OnMoreButtonClick;
							this.MyCustomTabInnerControl2.AutomaticButtonClick += OnAutomaticButtonClick;
							if (this.Item is RepositoryItemMyColorPickEdit myItem2)
							{
								page.PageVisible = myItem2.ShowMyCustomColors;
							}
							control = this.MyCustomTabInnerControl2;
							break;
					}
					control.Dock = DockStyle.Fill;
					if (this.Owner.LookAndFeel != null)
					{
						control.LookAndFeel.Assign(this.Owner.LookAndFeel);
					}
					page.Controls.Add(control);
				}
			}

			protected override int GetBestTabPageIndex(Color color)
			{
				RepositoryItemMyColorPickEdit prop = (RepositoryItemMyColorPickEdit)this.Owner.Properties;
				if (prop.ShowWebColors && this.WebTabInnerControl.ContainsColor(color))
				{
					return 1;
				}
				else if (prop.ShowSystemColors && this.SystemTabInnerControl.ContainsColor(color))
				{
					return 2;
				}
				else if (prop.ShowWebSafeColors && this.WebSafeTabInnerControl.ContainsColor(color))
				{
					return 3;
				}
				else if (prop.ShowMyCustomColors && this.MyCustomTabInnerControl.ContainsColor(color))
				{ // Add a check for your custom tab page
					return 4; // Page index of your own custom tab page
				}
				else if (prop.ShowMyPastelColors && this.MyCustomTabInnerControl2.ContainsColor(color))
				{ 
					return 5; // Page index of your own custom tab page
				}
				else if (prop.ShowCustomColors && this.CustomTabInnerControl.ContainsColor(color))
				{
					return 0;
				}
				return -1;
			}

			#region Event handlers from parent class

			protected override void OnSelectedColorChanged(object sender, InnerColorPickControlSelectedColorChangedEventArgs e)
			{
				base.OnSelectedColorChanged(sender, e);
			}

			void OnMoreButtonClick(object sender, EventArgs e)
			{
				//DoShowColorDialog();
				DXColorPicker MyColorPicker = new DXColorPicker((Color)this.ResultColor);
				if (Mensajero.ShowFormHTML(MyColorPicker, "Pick a Color", DarkMode: true) == DialogResult.OK)
				{
					var CP = sender as InnerColorPickControl;
					CP.SelectedColor = MyColorPicker.SelectedColor;
					CP.SetColor(MyColorPicker.SelectedColor, true);
					CP.AutomaticColor = MyColorPicker.SelectedColor;

					FindEditColor(MyColorPicker.SelectedColor);
					this.Owner.OwnerEdit.EditValue = MyColorPicker.SelectedColor;
				}
			}

			void OnBeforeShowPopup(object sender, EventArgs e)
			{
				OnBeforeShowPopup();
			}
			void OnAutomaticButtonClick(object sender, EventArgs e)
			{
				OnAutomaticButtonClick();
			}
			#endregion
		}

		protected override ColorEditTabControl CreateTabControl()
		{
			// Use Custom tab control with an additional page
			return new MyColorEditTabControl(this);
		}

		public class MyColorEditTabControl : ColorPickEditTabControl
		{
			public MyColorEditTabControl(IPopupColorPickEdit owner) : base(owner)
			{

			}

			protected override void CreatePages()
			{
				base.CreatePages();
				this.TabPages.Add(new DevExpress.XtraTab.XtraTabPage()); // Add a new custom tab page (index 4)
				//this.TabPages.Add(new DevExpress.XtraTab.XtraTabPage()); // Add a new custom tab page (index 5)
			}
		}
	}
}
