namespace elite_dangerous_64
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
			this.label1 = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(149, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(128, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Elite Dangerous Simulator";
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.BackColor = System.Drawing.Color.Black;
			this.lblStatus.ForeColor = System.Drawing.Color.Lime;
			this.lblStatus.Location = new System.Drawing.Point(13, 32);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(10, 13);
			this.lblStatus.TabIndex = 2;
			this.lblStatus.Text = ".";
			// 
			// comboBox1
			// 
			this.comboBox1.BackColor = System.Drawing.Color.Black;
			this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "python|NORMANDY|SR-03",
            "empire_courier|TEMPEST|BL-05E",
            "ferdelance|ANIMA|SR-02",
            "type9|DRWIGGLES|ABC",
            "anaconda|veritas|Zan-32",
            "krait_light|Copperhead|ZAN-31",
            "diamondbackxl|EXPLORATOR|ZA L69",
            "federation_corvette|INDOMITABLE|RW-01",
            "sidewinder|sidewinder|ZA-06S",
            "type9_military|TALOS|ZA-07T"});
			this.comboBox1.Location = new System.Drawing.Point(955, 29);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(121, 21);
			this.comboBox1.TabIndex = 3;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			this.comboBox1.DropDownClosed += new System.EventHandler(this.comboBox1_DropDownClosed);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = global::elite_dangerous_64.Properties.Resources.FDL_Clean;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.ClientSize = new System.Drawing.Size(1088, 631);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.label1);
			this.KeyPreview = true;
			this.Name = "Form1";
			this.Text = "Elite - Dangerous (CLIENT)";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.ComboBox comboBox1;
	}
}

