
namespace ColorConverterSRGB
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
			this.txGamma = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.txSRGB_C_B = new System.Windows.Forms.TextBox();
			this.txSRGB_C_G = new System.Windows.Forms.TextBox();
			this.txSRGB_C_R = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.txSRGB_C_A = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.txRGBint = new System.Windows.Forms.NumericUpDown();
			this.button2 = new System.Windows.Forms.Button();
			this.colorControl1 = new BlueControls.ColorControl();
			((System.ComponentModel.ISupportInitialize)(this.txGamma)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txRGBint)).BeginInit();
			this.SuspendLayout();
			// 
			// txGamma
			// 
			this.txGamma.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.txGamma.DecimalPlaces = 2;
			this.txGamma.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.txGamma.Location = new System.Drawing.Point(332, 46);
			this.txGamma.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.txGamma.Name = "txGamma";
			this.txGamma.Size = new System.Drawing.Size(58, 20);
			this.txGamma.TabIndex = 2;
			this.txGamma.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txGamma.Value = new decimal(new int[] {
            24,
            0,
            0,
            65536});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(341, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(46, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Gamma:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(131, 77);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(15, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "A";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(187, 77);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(16, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "R";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(243, 77);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(16, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "G";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(299, 77);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(15, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "B";
			// 
			// txSRGB_C_B
			// 
			this.txSRGB_C_B.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.txSRGB_C_B.Location = new System.Drawing.Point(281, 94);
			this.txSRGB_C_B.Name = "txSRGB_C_B";
			this.txSRGB_C_B.Size = new System.Drawing.Size(50, 20);
			this.txSRGB_C_B.TabIndex = 17;
			this.txSRGB_C_B.Text = "0,000";
			this.txSRGB_C_B.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txSRGB_C_G
			// 
			this.txSRGB_C_G.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.txSRGB_C_G.Location = new System.Drawing.Point(225, 94);
			this.txSRGB_C_G.Name = "txSRGB_C_G";
			this.txSRGB_C_G.Size = new System.Drawing.Size(50, 20);
			this.txSRGB_C_G.TabIndex = 16;
			this.txSRGB_C_G.Text = "0,000";
			this.txSRGB_C_G.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txSRGB_C_R
			// 
			this.txSRGB_C_R.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.txSRGB_C_R.Location = new System.Drawing.Point(169, 94);
			this.txSRGB_C_R.Name = "txSRGB_C_R";
			this.txSRGB_C_R.Size = new System.Drawing.Size(50, 20);
			this.txSRGB_C_R.TabIndex = 15;
			this.txSRGB_C_R.Text = "0,000";
			this.txSRGB_C_R.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(25, 84);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(82, 30);
			this.label7.TabIndex = 14;
			this.label7.Text = "sRGB Gamma Corrected:";
			// 
			// txSRGB_C_A
			// 
			this.txSRGB_C_A.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.txSRGB_C_A.Location = new System.Drawing.Point(113, 94);
			this.txSRGB_C_A.Name = "txSRGB_C_A";
			this.txSRGB_C_A.Size = new System.Drawing.Size(50, 20);
			this.txSRGB_C_A.TabIndex = 13;
			this.txSRGB_C_A.Text = "0,000";
			this.txSRGB_C_A.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(334, 92);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(56, 23);
			this.button1.TabIndex = 18;
			this.button1.Text = "To RGB";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(28, 131);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(69, 13);
			this.label2.TabIndex = 19;
			this.label2.Text = "RGB Integer:";
			// 
			// txRGBint
			// 
			this.txRGBint.Location = new System.Drawing.Point(113, 125);
			this.txRGBint.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
			this.txRGBint.Minimum = new decimal(new int[] {
            1410065407,
            2,
            0,
            -2147483648});
			this.txRGBint.Name = "txRGBint";
			this.txRGBint.Size = new System.Drawing.Size(106, 20);
			this.txRGBint.TabIndex = 20;
			this.txRGBint.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(225, 122);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 21;
			this.button2.Text = "To Color";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// colorControl1
			// 
			this.colorControl1.ColorValue = System.Drawing.Color.White;
			this.colorControl1.CustomColors = new int[] {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0};
			this.colorControl1.Location = new System.Drawing.Point(12, 12);
			this.colorControl1.Name = "colorControl1";
			this.colorControl1.Size = new System.Drawing.Size(304, 64);
			this.colorControl1.TabIndex = 1;
			this.colorControl1.Time = new decimal(new int[] {
            0,
            0,
            0,
            0});
			this.colorControl1.OnCustomColorsChanged += new System.EventHandler(this.colorControl1_OnCustomColorsChanged);
			this.colorControl1.OnColorChanged += new System.EventHandler(this.colorControl1_OnColorChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(401, 150);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.txRGBint);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.txSRGB_C_B);
			this.Controls.Add(this.txSRGB_C_G);
			this.Controls.Add(this.txSRGB_C_R);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.txSRGB_C_A);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txGamma);
			this.Controls.Add(this.colorControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Text = "Color Converter: RGB to sRGB";
			((System.ComponentModel.ISupportInitialize)(this.txGamma)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txRGBint)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BlueControls.ColorControl colorControl1;
		private System.Windows.Forms.NumericUpDown txGamma;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txSRGB_C_B;
		private System.Windows.Forms.TextBox txSRGB_C_G;
		private System.Windows.Forms.TextBox txSRGB_C_R;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txSRGB_C_A;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown txRGBint;
		private System.Windows.Forms.Button button2;
	}
}

