
namespace BlueControls
{
	partial class ColorControl
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
			this.ColorBox = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblAlpha = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.txtHtmlValue = new System.Windows.Forms.TextBox();
			this.lbTime = new System.Windows.Forms.Label();
			this.A_Value = new System.Windows.Forms.TextBox();
			this.R_Value = new System.Windows.Forms.TextBox();
			this.G_Value = new System.Windows.Forms.TextBox();
			this.B_Value = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.ColorBox)).BeginInit();
			this.SuspendLayout();
			// 
			// ColorBox
			// 
			this.ColorBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.ColorBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ColorBox.Location = new System.Drawing.Point(4, 4);
			this.ColorBox.Name = "ColorBox";
			this.ColorBox.Size = new System.Drawing.Size(100, 54);
			this.ColorBox.TabIndex = 0;
			this.ColorBox.TabStop = false;
			this.ColorBox.Paint += new System.Windows.Forms.PaintEventHandler(this.ColorBox_Paint);
			this.ColorBox.DoubleClick += new System.EventHandler(this.ColorBox_DoubleClick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(170, 1);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(15, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "R";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(219, 1);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(15, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "G";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(270, 1);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(14, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "B";
			// 
			// lblAlpha
			// 
			this.lblAlpha.AutoSize = true;
			this.lblAlpha.Location = new System.Drawing.Point(106, 1);
			this.lblAlpha.Name = "lblAlpha";
			this.lblAlpha.Size = new System.Drawing.Size(43, 13);
			this.lblAlpha.TabIndex = 8;
			this.lblAlpha.Text = "A:100%";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(176, 41);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "HTML:";
			// 
			// txtHtmlValue
			// 
			this.txtHtmlValue.Location = new System.Drawing.Point(213, 38);
			this.txtHtmlValue.Name = "txtHtmlValue";
			this.txtHtmlValue.Size = new System.Drawing.Size(80, 20);
			this.txtHtmlValue.TabIndex = 10;
			this.txtHtmlValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txtHtmlValue.TextChanged += new System.EventHandler(this.txtHtmlValue_TextChanged);
			this.txtHtmlValue.Enter += new System.EventHandler(this.txtHtmlValue_Enter);
			// 
			// lbTime
			// 
			this.lbTime.AutoSize = true;
			this.lbTime.Location = new System.Drawing.Point(104, 41);
			this.lbTime.Name = "lbTime";
			this.lbTime.Size = new System.Drawing.Size(10, 13);
			this.lbTime.TabIndex = 11;
			this.lbTime.Text = ".";
			// 
			// A_Value
			// 
			this.A_Value.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.A_Value.Location = new System.Drawing.Point(109, 16);
			this.A_Value.Name = "A_Value";
			this.A_Value.Size = new System.Drawing.Size(34, 20);
			this.A_Value.TabIndex = 12;
			this.A_Value.Text = "255";
			this.A_Value.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.A_Value.TextChanged += new System.EventHandler(this.RGB_Value_TextChanged);
			this.A_Value.Enter += new System.EventHandler(this.RGB_Value__Enter);
			this.A_Value.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RGB_Value_KeyDown);
			this.A_Value.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RGB_Value_KeyPress);
			// 
			// R_Value
			// 
			this.R_Value.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.R_Value.Location = new System.Drawing.Point(159, 16);
			this.R_Value.Name = "R_Value";
			this.R_Value.Size = new System.Drawing.Size(34, 20);
			this.R_Value.TabIndex = 13;
			this.R_Value.Text = "255";
			this.R_Value.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.R_Value.TextChanged += new System.EventHandler(this.RGB_Value_TextChanged);
			this.R_Value.Enter += new System.EventHandler(this.RGB_Value__Enter);
			this.R_Value.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RGB_Value_KeyDown);
			this.R_Value.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RGB_Value_KeyPress);
			// 
			// G_Value
			// 
			this.G_Value.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.G_Value.Location = new System.Drawing.Point(209, 16);
			this.G_Value.Name = "G_Value";
			this.G_Value.Size = new System.Drawing.Size(34, 20);
			this.G_Value.TabIndex = 14;
			this.G_Value.Text = "255";
			this.G_Value.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.G_Value.TextChanged += new System.EventHandler(this.RGB_Value_TextChanged);
			this.G_Value.Enter += new System.EventHandler(this.RGB_Value__Enter);
			this.G_Value.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RGB_Value_KeyDown);
			this.G_Value.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RGB_Value_KeyPress);
			// 
			// B_Value
			// 
			this.B_Value.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.B_Value.Location = new System.Drawing.Point(259, 16);
			this.B_Value.Name = "B_Value";
			this.B_Value.Size = new System.Drawing.Size(34, 20);
			this.B_Value.TabIndex = 15;
			this.B_Value.Text = "255";
			this.B_Value.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.B_Value.TextChanged += new System.EventHandler(this.RGB_Value_TextChanged);
			this.B_Value.Enter += new System.EventHandler(this.RGB_Value__Enter);
			this.B_Value.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RGB_Value_KeyDown);
			this.B_Value.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RGB_Value_KeyPress);
			// 
			// ColorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.B_Value);
			this.Controls.Add(this.G_Value);
			this.Controls.Add(this.R_Value);
			this.Controls.Add(this.A_Value);
			this.Controls.Add(this.txtHtmlValue);
			this.Controls.Add(this.lbTime);
			this.Controls.Add(this.lblAlpha);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ColorBox);
			this.Controls.Add(this.label5);
			this.Name = "ColorControl";
			this.Size = new System.Drawing.Size(304, 64);
			this.Load += new System.EventHandler(this.ColorControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.ColorBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox ColorBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label lblAlpha;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtHtmlValue;
		private System.Windows.Forms.Label lbTime;
		private System.Windows.Forms.TextBox A_Value;
		private System.Windows.Forms.TextBox R_Value;
		private System.Windows.Forms.TextBox G_Value;
		private System.Windows.Forms.TextBox B_Value;
	}
}
