namespace EDHM_UI_Patcher
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
			System.Windows.Forms.Button cmdUpdate;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.lblInfo = new System.Windows.Forms.Label();
			this.circularProgressBar1 = new EDHM_UI_Patcher.CircularProgressBar();
			cmdUpdate = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// cmdUpdate
			// 
			cmdUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			cmdUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			cmdUpdate.ForeColor = System.Drawing.Color.Black;
			cmdUpdate.Location = new System.Drawing.Point(441, 145);
			cmdUpdate.Name = "cmdUpdate";
			cmdUpdate.Size = new System.Drawing.Size(75, 23);
			cmdUpdate.TabIndex = 0;
			cmdUpdate.Text = "&Update";
			cmdUpdate.UseVisualStyleBackColor = false;
			cmdUpdate.Click += new System.EventHandler(this.cmdUpdate_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(178, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(338, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = "Version 2.0.6.8";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(46)))), ((int)(((byte)(64)))));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.ForeColor = System.Drawing.Color.White;
			this.textBox1.Location = new System.Drawing.Point(184, 45);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(327, 94);
			this.textBox1.TabIndex = 2;
			this.textBox1.TabStop = false;
			this.textBox1.Text = "- Fixed Preview not showing in Horizons version.\r\n- 1 new theme added (for Odysse" +
    "y).";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(175, 29);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Changelog:";
			// 
			// lblInfo
			// 
			this.lblInfo.AutoSize = true;
			this.lblInfo.ForeColor = System.Drawing.Color.Goldenrod;
			this.lblInfo.Location = new System.Drawing.Point(181, 150);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(193, 13);
			this.lblInfo.TabIndex = 5;
			this.lblInfo.Text = "An Update is Available, Click to start -->";
			// 
			// circularProgressBar1
			// 
			this.circularProgressBar1.BackgroundImage = global::EDHM_UI_Patcher.Properties.Resources.triple_elite_250;
			this.circularProgressBar1.Font = new System.Drawing.Font("Segoe UI Semibold", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.circularProgressBar1.FontInner = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.circularProgressBar1.ForeColor = System.Drawing.Color.White;
			this.circularProgressBar1.InnerBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.circularProgressBar1.InnerPicture = null;
			this.circularProgressBar1.LineColor = System.Drawing.Color.Silver;
			this.circularProgressBar1.Location = new System.Drawing.Point(0, 0);
			this.circularProgressBar1.Maximum = ((long)(100));
			this.circularProgressBar1.MinimumSize = new System.Drawing.Size(100, 100);
			this.circularProgressBar1.Name = "circularProgressBar1";
			this.circularProgressBar1.ProgressColor1 = System.Drawing.Color.Yellow;
			this.circularProgressBar1.ProgressColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.circularProgressBar1.ProgressShape = EDHM_UI_Patcher.CircularProgressBar._ProgressShape.Round;
			this.circularProgressBar1.Size = new System.Drawing.Size(175, 175);
			this.circularProgressBar1.TabIndex = 4;
			this.circularProgressBar1.Text = "65%";
			this.circularProgressBar1.TextInner = "45%";
			this.circularProgressBar1.TextMode = EDHM_UI_Patcher.CircularProgressBar._TextMode.Percentage;
			this.circularProgressBar1.Value = ((long)(0));
			this.circularProgressBar1.Value_Inner = ((long)(0));
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(46)))), ((int)(((byte)(64)))));
			this.ClientSize = new System.Drawing.Size(523, 176);
			this.Controls.Add(this.lblInfo);
			this.Controls.Add(this.circularProgressBar1);
			this.Controls.Add(cmdUpdate);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "EDHM Live Patcher [by Blue Mystic]";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label2;
		private CircularProgressBar circularProgressBar1;
		private System.Windows.Forms.Label lblInfo;
	}
}

