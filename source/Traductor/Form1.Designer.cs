namespace Traductor
{
	partial class Form1
	{
		/// <summary>
		/// Variable del diseñador necesaria.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Limpiar los recursos que se estén usando.
		/// </summary>
		/// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Código generado por el Diseñador de Windows Forms

		/// <summary>
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido de este método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.settingsJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuLoadSourceFile = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSaveDestinationFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menusJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuMenus_LoadFile = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuMenus_SaveFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(12, 27);
			this.textBox1.MaxLength = 0;
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBox1.Size = new System.Drawing.Size(450, 437);
			this.textBox1.TabIndex = 1;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsJSONToolStripMenuItem,
            this.menusJSONToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(470, 24);
			this.menuStrip1.TabIndex = 3;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// settingsJSONToolStripMenuItem
			// 
			this.settingsJSONToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLoadSourceFile,
            this.mnuSaveDestinationFile});
			this.settingsJSONToolStripMenuItem.Name = "settingsJSONToolStripMenuItem";
			this.settingsJSONToolStripMenuItem.Size = new System.Drawing.Size(92, 20);
			this.settingsJSONToolStripMenuItem.Text = "Settings JSON";
			// 
			// mnuLoadSourceFile
			// 
			this.mnuLoadSourceFile.Name = "mnuLoadSourceFile";
			this.mnuLoadSourceFile.Size = new System.Drawing.Size(180, 22);
			this.mnuLoadSourceFile.Text = "&Load Souce File..";
			this.mnuLoadSourceFile.Click += new System.EventHandler(this.mnuLoadSourceFile_Click);
			// 
			// mnuSaveDestinationFile
			// 
			this.mnuSaveDestinationFile.Name = "mnuSaveDestinationFile";
			this.mnuSaveDestinationFile.Size = new System.Drawing.Size(180, 22);
			this.mnuSaveDestinationFile.Text = "&Save JSON File..";
			this.mnuSaveDestinationFile.Click += new System.EventHandler(this.mnuSaveDestinationFile_Click);
			// 
			// menusJSONToolStripMenuItem
			// 
			this.menusJSONToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMenus_LoadFile,
            this.mnuMenus_SaveFile});
			this.menusJSONToolStripMenuItem.Name = "menusJSONToolStripMenuItem";
			this.menusJSONToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
			this.menusJSONToolStripMenuItem.Text = "Menus JSON";
			// 
			// mnuMenus_LoadFile
			// 
			this.mnuMenus_LoadFile.Name = "mnuMenus_LoadFile";
			this.mnuMenus_LoadFile.Size = new System.Drawing.Size(180, 22);
			this.mnuMenus_LoadFile.Text = "Load Source File..";
			this.mnuMenus_LoadFile.Click += new System.EventHandler(this.mnuMenus_LoadFile_Click);
			// 
			// mnuMenus_SaveFile
			// 
			this.mnuMenus_SaveFile.Name = "mnuMenus_SaveFile";
			this.mnuMenus_SaveFile.Size = new System.Drawing.Size(180, 22);
			this.mnuMenus_SaveFile.Text = "Save Translation..";
			this.mnuMenus_SaveFile.Click += new System.EventHandler(this.mnuMenus_SaveFile_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(470, 465);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Elite - Dangerous (CLIENT)";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem settingsJSONToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuLoadSourceFile;
		private System.Windows.Forms.ToolStripMenuItem mnuSaveDestinationFile;
		private System.Windows.Forms.ToolStripMenuItem menusJSONToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuMenus_LoadFile;
		private System.Windows.Forms.ToolStripMenuItem mnuMenus_SaveFile;
	}
}

