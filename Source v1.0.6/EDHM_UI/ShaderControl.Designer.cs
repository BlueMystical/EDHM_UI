namespace EDHM_UI
{
	partial class ShaderControl
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

		#region Código generado por el Diseñador de componentes

		/// <summary> 
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido de este método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.txtName = new System.Windows.Forms.TextBox();
			this.txtValue = new System.Windows.Forms.NumericUpDown();
			this.Slider = new System.Windows.Forms.TrackBar();
			this.panelColor = new System.Windows.Forms.Panel();
			this.lblColorARGB = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cmdDetails = new System.Windows.Forms.Button();
			this.cmdColorChange = new System.Windows.Forms.Button();
			this.checkSelected = new System.Windows.Forms.CheckBox();
			this.cmdSaveShader = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.txtValue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Slider)).BeginInit();
			this.panelColor.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 10;
			this.label1.Text = "Name:";
			// 
			// txtName
			// 
			this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtName.Location = new System.Drawing.Point(49, 6);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(235, 20);
			this.txtName.TabIndex = 9;
			this.txtName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			// 
			// txtValue
			// 
			this.txtValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtValue.DecimalPlaces = 6;
			this.txtValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.txtValue.Location = new System.Drawing.Point(0, 66);
			this.txtValue.Name = "txtValue";
			this.txtValue.Size = new System.Drawing.Size(74, 20);
			this.txtValue.TabIndex = 19;
			this.txtValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txtValue.ValueChanged += new System.EventHandler(this.txtValue_ValueChanged);
			// 
			// Slider
			// 
			this.Slider.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Slider.LargeChange = 2;
			this.Slider.Location = new System.Drawing.Point(0, 0);
			this.Slider.Name = "Slider";
			this.Slider.Size = new System.Drawing.Size(260, 35);
			this.Slider.TabIndex = 25;
			this.Slider.Visible = false;
			this.Slider.ValueChanged += new System.EventHandler(this.Slider_ValueChanged);
			// 
			// panelColor
			// 
			this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelColor.Controls.Add(this.Slider);
			this.panelColor.Location = new System.Drawing.Point(24, 30);
			this.panelColor.Name = "panelColor";
			this.panelColor.Size = new System.Drawing.Size(260, 35);
			this.panelColor.TabIndex = 26;
			this.panelColor.DoubleClick += new System.EventHandler(this.panelColor_DoubleClick);
			// 
			// lblColorARGB
			// 
			this.lblColorARGB.AutoSize = true;
			this.lblColorARGB.Location = new System.Drawing.Point(78, 69);
			this.lblColorARGB.Name = "lblColorARGB";
			this.lblColorARGB.Size = new System.Drawing.Size(162, 13);
			this.lblColorARGB.TabIndex = 26;
			this.lblColorARGB.Text = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.label2.Dock = System.Windows.Forms.DockStyle.Top;
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(315, 2);
			this.label2.TabIndex = 30;
			// 
			// cmdDetails
			// 
			this.cmdDetails.FlatAppearance.BorderSize = 0;
			this.cmdDetails.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.cmdDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdDetails.Image = global::EDHM_UI.Properties.Resources.gear_in;
			this.cmdDetails.Location = new System.Drawing.Point(290, 4);
			this.cmdDetails.Name = "cmdDetails";
			this.cmdDetails.Size = new System.Drawing.Size(22, 22);
			this.cmdDetails.TabIndex = 29;
			this.cmdDetails.UseVisualStyleBackColor = true;
			this.cmdDetails.Click += new System.EventHandler(this.cmdDetails_Click);
			// 
			// cmdColorChange
			// 
			this.cmdColorChange.FlatAppearance.BorderSize = 0;
			this.cmdColorChange.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.cmdColorChange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdColorChange.Image = global::EDHM_UI.Properties.Resources._1352900523_colour_copy;
			this.cmdColorChange.Location = new System.Drawing.Point(290, 26);
			this.cmdColorChange.Name = "cmdColorChange";
			this.cmdColorChange.Size = new System.Drawing.Size(22, 22);
			this.cmdColorChange.TabIndex = 31;
			this.cmdColorChange.UseVisualStyleBackColor = true;
			this.cmdColorChange.Visible = false;
			this.cmdColorChange.Click += new System.EventHandler(this.cmdColorChange_Click);
			// 
			// checkSelected
			// 
			this.checkSelected.AutoSize = true;
			this.checkSelected.Location = new System.Drawing.Point(6, 29);
			this.checkSelected.Name = "checkSelected";
			this.checkSelected.Size = new System.Drawing.Size(15, 14);
			this.checkSelected.TabIndex = 32;
			this.checkSelected.UseVisualStyleBackColor = true;
			this.checkSelected.CheckedChanged += new System.EventHandler(this.checkSelected_CheckedChanged);
			// 
			// cmdSaveShader
			// 
			this.cmdSaveShader.FlatAppearance.BorderSize = 0;
			this.cmdSaveShader.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.cmdSaveShader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdSaveShader.Image = global::EDHM_UI.Properties.Resources._1352899444_disk;
			this.cmdSaveShader.Location = new System.Drawing.Point(0, 43);
			this.cmdSaveShader.Name = "cmdSaveShader";
			this.cmdSaveShader.Size = new System.Drawing.Size(22, 22);
			this.cmdSaveShader.TabIndex = 33;
			this.cmdSaveShader.UseVisualStyleBackColor = true;
			this.cmdSaveShader.Visible = false;
			this.cmdSaveShader.Click += new System.EventHandler(this.cmdSaveShader_Click);
			// 
			// ShaderControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cmdSaveShader);
			this.Controls.Add(this.checkSelected);
			this.Controls.Add(this.lblColorARGB);
			this.Controls.Add(this.cmdColorChange);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cmdDetails);
			this.Controls.Add(this.panelColor);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.txtValue);
			this.Name = "ShaderControl";
			this.Size = new System.Drawing.Size(315, 88);
			this.Load += new System.EventHandler(this.ShaderControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.txtValue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Slider)).EndInit();
			this.panelColor.ResumeLayout(false);
			this.panelColor.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.NumericUpDown txtValue;
		private System.Windows.Forms.TrackBar Slider;
		private System.Windows.Forms.Panel panelColor;
		private System.Windows.Forms.Button cmdDetails;
		private System.Windows.Forms.Label lblColorARGB;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button cmdColorChange;
		private System.Windows.Forms.CheckBox checkSelected;
		private System.Windows.Forms.Button cmdSaveShader;
	}
}
