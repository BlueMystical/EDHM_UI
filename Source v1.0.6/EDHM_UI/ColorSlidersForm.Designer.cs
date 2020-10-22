namespace EDHM_UI
{
	partial class ColorSlidersForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.trackRed = new System.Windows.Forms.TrackBar();
			this.trackGreen = new System.Windows.Forms.TrackBar();
			this.trackBlue = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.trackAlpha = new System.Windows.Forms.TrackBar();
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.txtColorFormats = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.trackRed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackGreen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBlue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackAlpha)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(37, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Value:";
			// 
			// txtValue
			// 
			this.txtValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtValue.Location = new System.Drawing.Point(53, 11);
			this.txtValue.Name = "txtValue";
			this.txtValue.ReadOnly = true;
			this.txtValue.Size = new System.Drawing.Size(278, 20);
			this.txtValue.TabIndex = 1;
			this.txtValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// trackRed
			// 
			this.trackRed.Location = new System.Drawing.Point(53, 63);
			this.trackRed.Maximum = 255;
			this.trackRed.Name = "trackRed";
			this.trackRed.Size = new System.Drawing.Size(278, 45);
			this.trackRed.TabIndex = 0;
			this.trackRed.TickFrequency = 10;
			this.trackRed.ValueChanged += new System.EventHandler(this.trackRed_ValueChanged);
			// 
			// trackGreen
			// 
			this.trackGreen.Location = new System.Drawing.Point(53, 94);
			this.trackGreen.Maximum = 255;
			this.trackGreen.Name = "trackGreen";
			this.trackGreen.Size = new System.Drawing.Size(278, 45);
			this.trackGreen.TabIndex = 1;
			this.trackGreen.TickFrequency = 10;
			this.trackGreen.ValueChanged += new System.EventHandler(this.trackGreen_ValueChanged);
			// 
			// trackBlue
			// 
			this.trackBlue.Location = new System.Drawing.Point(53, 127);
			this.trackBlue.Maximum = 255;
			this.trackBlue.Name = "trackBlue";
			this.trackBlue.Size = new System.Drawing.Size(278, 45);
			this.trackBlue.TabIndex = 2;
			this.trackBlue.TickFrequency = 10;
			this.trackBlue.ValueChanged += new System.EventHandler(this.trackBlue_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 68);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(30, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Red:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 98);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(39, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Green:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 130);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(31, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Blue:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(7, 166);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(37, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "Alpha:";
			// 
			// trackAlpha
			// 
			this.trackAlpha.Location = new System.Drawing.Point(53, 160);
			this.trackAlpha.Maximum = 255;
			this.trackAlpha.Name = "trackAlpha";
			this.trackAlpha.Size = new System.Drawing.Size(278, 45);
			this.trackAlpha.TabIndex = 3;
			this.trackAlpha.TickFrequency = 10;
			this.trackAlpha.ValueChanged += new System.EventHandler(this.trackAlpha_ValueChanged);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdOK.Location = new System.Drawing.Point(256, 200);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 4;
			this.cmdOK.Text = "&OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdCancel.Location = new System.Drawing.Point(175, 200);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panel1.BackColor = System.Drawing.Color.Orange;
			this.panel1.Location = new System.Drawing.Point(10, 199);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(151, 24);
			this.panel1.TabIndex = 12;
			// 
			// txtColorFormats
			// 
			this.txtColorFormats.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtColorFormats.Location = new System.Drawing.Point(53, 35);
			this.txtColorFormats.Name = "txtColorFormats";
			this.txtColorFormats.ReadOnly = true;
			this.txtColorFormats.Size = new System.Drawing.Size(278, 13);
			this.txtColorFormats.TabIndex = 13;
			this.txtColorFormats.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// ColorSlidersForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(343, 235);
			this.Controls.Add(this.txtColorFormats);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.trackAlpha);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.trackBlue);
			this.Controls.Add(this.trackGreen);
			this.Controls.Add(this.trackRed);
			this.Controls.Add(this.txtValue);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ColorSlidersForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ColorSlidersForm";
			this.Load += new System.EventHandler(this.ColorSlidersForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackRed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackGreen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBlue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackAlpha)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtValue;
		private System.Windows.Forms.TrackBar trackRed;
		private System.Windows.Forms.TrackBar trackGreen;
		private System.Windows.Forms.TrackBar trackBlue;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TrackBar trackAlpha;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox txtColorFormats;
	}
}