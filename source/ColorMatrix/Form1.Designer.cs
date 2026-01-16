namespace ColorMatrixForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			groupBox1 = new GroupBox();
			label7 = new Label();
			sBB_B = new TrackBar();
			label8 = new Label();
			sBG_B = new TrackBar();
			label9 = new Label();
			sBR_B = new TrackBar();
			label4 = new Label();
			sGB_B = new TrackBar();
			label5 = new Label();
			sGG_B = new TrackBar();
			label6 = new Label();
			sGR_B = new TrackBar();
			label3 = new Label();
			sRB_B = new TrackBar();
			label2 = new Label();
			sRG_B = new TrackBar();
			label1 = new Label();
			sRR_B = new TrackBar();
			textBox1 = new TextBox();
			label10 = new Label();
			picStationOriginalImage = new PictureBox();
			picShipPanels = new PictureBox();
			trackGamma = new TrackBar();
			label11 = new Label();
			trackSaturation = new TrackBar();
			label12 = new Label();
			groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)sBB_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sBG_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sBR_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sGB_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sGG_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sGR_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sRB_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sRG_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)sRR_B).BeginInit();
			((System.ComponentModel.ISupportInitialize)picStationOriginalImage).BeginInit();
			((System.ComponentModel.ISupportInitialize)picShipPanels).BeginInit();
			((System.ComponentModel.ISupportInitialize)trackGamma).BeginInit();
			((System.ComponentModel.ISupportInitialize)trackSaturation).BeginInit();
			SuspendLayout();
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(label7);
			groupBox1.Controls.Add(sBB_B);
			groupBox1.Controls.Add(label8);
			groupBox1.Controls.Add(sBG_B);
			groupBox1.Controls.Add(label9);
			groupBox1.Controls.Add(sBR_B);
			groupBox1.Controls.Add(label4);
			groupBox1.Controls.Add(sGB_B);
			groupBox1.Controls.Add(label5);
			groupBox1.Controls.Add(sGG_B);
			groupBox1.Controls.Add(label6);
			groupBox1.Controls.Add(sGR_B);
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(sRB_B);
			groupBox1.Controls.Add(label2);
			groupBox1.Controls.Add(sRG_B);
			groupBox1.Controls.Add(label1);
			groupBox1.Controls.Add(sRR_B);
			groupBox1.Location = new Point(12, 12);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(444, 159);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox1.Text = "Color Matrix Values";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new Point(306, 94);
			label7.Name = "label7";
			label7.Size = new Size(35, 15);
			label7.TabIndex = 17;
			label7.Text = "B: 1.0";
			// 
			// sBB_B
			// 
			sBB_B.LargeChange = 20;
			sBB_B.Location = new Point(303, 105);
			sBB_B.Maximum = 200;
			sBB_B.Minimum = -200;
			sBB_B.Name = "sBB_B";
			sBB_B.Size = new Size(133, 45);
			sBB_B.TabIndex = 16;
			sBB_B.TickFrequency = 10;
			sBB_B.Value = 1;
			sBB_B.Scroll += sRGBB_Scroll;
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Location = new Point(167, 94);
			label8.Name = "label8";
			label8.Size = new Size(39, 15);
			label8.TabIndex = 15;
			label8.Text = "G:  0.0";
			// 
			// sBG_B
			// 
			sBG_B.LargeChange = 20;
			sBG_B.Location = new Point(164, 105);
			sBG_B.Maximum = 200;
			sBG_B.Minimum = -200;
			sBG_B.Name = "sBG_B";
			sBG_B.Size = new Size(133, 45);
			sBG_B.TabIndex = 14;
			sBG_B.TickFrequency = 10;
			sBG_B.Scroll += sRGBB_Scroll;
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Location = new Point(28, 94);
			label9.Name = "label9";
			label9.Size = new Size(38, 15);
			label9.TabIndex = 13;
			label9.Text = "R:  0.0";
			// 
			// sBR_B
			// 
			sBR_B.LargeChange = 20;
			sBR_B.Location = new Point(25, 105);
			sBR_B.Maximum = 200;
			sBR_B.Minimum = -200;
			sBR_B.Name = "sBR_B";
			sBR_B.Size = new Size(133, 45);
			sBR_B.TabIndex = 12;
			sBR_B.TickFrequency = 10;
			sBR_B.Scroll += sRGBB_Scroll;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new Point(306, 57);
			label4.Name = "label4";
			label4.Size = new Size(35, 15);
			label4.TabIndex = 11;
			label4.Text = "B: 0.0";
			// 
			// sGB_B
			// 
			sGB_B.LargeChange = 20;
			sGB_B.Location = new Point(303, 68);
			sGB_B.Maximum = 200;
			sGB_B.Minimum = -200;
			sGB_B.Name = "sGB_B";
			sGB_B.Size = new Size(133, 45);
			sGB_B.TabIndex = 10;
			sGB_B.TickFrequency = 10;
			sGB_B.Scroll += sRGBB_Scroll;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(167, 57);
			label5.Name = "label5";
			label5.Size = new Size(39, 15);
			label5.TabIndex = 9;
			label5.Text = "G:  1.0";
			// 
			// sGG_B
			// 
			sGG_B.LargeChange = 20;
			sGG_B.Location = new Point(164, 68);
			sGG_B.Maximum = 200;
			sGG_B.Minimum = -200;
			sGG_B.Name = "sGG_B";
			sGG_B.Size = new Size(133, 45);
			sGG_B.TabIndex = 8;
			sGG_B.TickFrequency = 10;
			sGG_B.Value = 1;
			sGG_B.Scroll += sRGBB_Scroll;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new Point(28, 57);
			label6.Name = "label6";
			label6.Size = new Size(38, 15);
			label6.TabIndex = 7;
			label6.Text = "R:  0.0";
			// 
			// sGR_B
			// 
			sGR_B.LargeChange = 20;
			sGR_B.Location = new Point(25, 68);
			sGR_B.Maximum = 200;
			sGR_B.Minimum = -200;
			sGR_B.Name = "sGR_B";
			sGR_B.Size = new Size(133, 45);
			sGR_B.TabIndex = 6;
			sGR_B.TickFrequency = 10;
			sGR_B.Scroll += sRGBB_Scroll;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(306, 21);
			label3.Name = "label3";
			label3.Size = new Size(35, 15);
			label3.TabIndex = 5;
			label3.Text = "B: 0.0";
			// 
			// sRB_B
			// 
			sRB_B.LargeChange = 20;
			sRB_B.Location = new Point(303, 32);
			sRB_B.Maximum = 200;
			sRB_B.Minimum = -200;
			sRB_B.Name = "sRB_B";
			sRB_B.Size = new Size(133, 45);
			sRB_B.TabIndex = 4;
			sRB_B.TickFrequency = 10;
			sRB_B.Scroll += sRGBB_Scroll;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(167, 21);
			label2.Name = "label2";
			label2.Size = new Size(39, 15);
			label2.TabIndex = 3;
			label2.Text = "G:  0.0";
			// 
			// sRG_B
			// 
			sRG_B.LargeChange = 20;
			sRG_B.Location = new Point(164, 32);
			sRG_B.Maximum = 200;
			sRG_B.Minimum = -200;
			sRG_B.Name = "sRG_B";
			sRG_B.Size = new Size(133, 45);
			sRG_B.TabIndex = 2;
			sRG_B.TickFrequency = 10;
			sRG_B.Scroll += sRGBB_Scroll;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(28, 21);
			label1.Name = "label1";
			label1.Size = new Size(38, 15);
			label1.TabIndex = 1;
			label1.Text = "R:  1.0";
			// 
			// sRR_B
			// 
			sRR_B.LargeChange = 20;
			sRR_B.Location = new Point(25, 32);
			sRR_B.Maximum = 200;
			sRR_B.Minimum = -200;
			sRR_B.Name = "sRR_B";
			sRR_B.Size = new Size(133, 45);
			sRR_B.TabIndex = 0;
			sRR_B.TickFrequency = 10;
			sRR_B.Value = 1;
			sRR_B.Scroll += sRGBB_Scroll;
			// 
			// textBox1
			// 
			textBox1.Location = new Point(473, 32);
			textBox1.Multiline = true;
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(300, 75);
			textBox1.TabIndex = 1;
			textBox1.Text = "<MatrixRed>1, 0, 0</MatrixRed>\r\n<MatrixGreen>0, 1, 0</MatrixGreen>\r\n<MatrixBlue>0, 0, 1</MatrixBlue>\r\n";
			textBox1.TextChanged += textBox1_TextChanged;
			// 
			// label10
			// 
			label10.AutoSize = true;
			label10.Location = new Point(466, 12);
			label10.Name = "label10";
			label10.Size = new Size(102, 15);
			label10.TabIndex = 2;
			label10.Text = "Color Matrix XML:";
			// 
			// picStationOriginalImage
			// 
			picStationOriginalImage.Image = Properties.Resources.xml_base;
			picStationOriginalImage.Location = new Point(124, 214);
			picStationOriginalImage.Name = "picStationOriginalImage";
			picStationOriginalImage.Size = new Size(67, 48);
			picStationOriginalImage.TabIndex = 3;
			picStationOriginalImage.TabStop = false;
			picStationOriginalImage.Visible = false;
			// 
			// picShipPanels
			// 
			picShipPanels.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			picShipPanels.Location = new Point(12, 177);
			picShipPanels.Name = "picShipPanels";
			picShipPanels.Size = new Size(761, 355);
			picShipPanels.TabIndex = 4;
			picShipPanels.TabStop = false;
			// 
			// trackGamma
			// 
			trackGamma.Location = new Point(543, 113);
			trackGamma.Maximum = 200;
			trackGamma.Minimum = -200;
			trackGamma.Name = "trackGamma";
			trackGamma.Size = new Size(230, 45);
			trackGamma.TabIndex = 5;
			trackGamma.TickFrequency = 10;
			trackGamma.Value = 10;
			// 
			// label11
			// 
			label11.AutoSize = true;
			label11.Location = new Point(474, 116);
			label11.Name = "label11";
			label11.Size = new Size(52, 15);
			label11.TabIndex = 6;
			label11.Text = "Gamma:";
			// 
			// trackSaturation
			// 
			trackSaturation.Location = new Point(543, 144);
			trackSaturation.Maximum = 200;
			trackSaturation.Minimum = -200;
			trackSaturation.Name = "trackSaturation";
			trackSaturation.Size = new Size(230, 45);
			trackSaturation.TabIndex = 7;
			trackSaturation.TickFrequency = 10;
			// 
			// label12
			// 
			label12.AutoSize = true;
			label12.Location = new Point(474, 145);
			label12.Name = "label12";
			label12.Size = new Size(66, 15);
			label12.TabIndex = 8;
			label12.Text = "Saturacion:";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(775, 544);
			Controls.Add(picShipPanels);
			Controls.Add(label12);
			Controls.Add(trackSaturation);
			Controls.Add(label11);
			Controls.Add(trackGamma);
			Controls.Add(picStationOriginalImage);
			Controls.Add(label10);
			Controls.Add(textBox1);
			Controls.Add(groupBox1);
			Name = "Form1";
			Text = "Color Matrix";
			Load += Form1_Load;
			Shown += Form1_Shown;
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)sBB_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sBG_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sBR_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sGB_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sGG_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sGR_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sRB_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sRG_B).EndInit();
			((System.ComponentModel.ISupportInitialize)sRR_B).EndInit();
			((System.ComponentModel.ISupportInitialize)picStationOriginalImage).EndInit();
			((System.ComponentModel.ISupportInitialize)picShipPanels).EndInit();
			((System.ComponentModel.ISupportInitialize)trackGamma).EndInit();
			((System.ComponentModel.ISupportInitialize)trackSaturation).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private GroupBox groupBox1;
		private Label label1;
		private TrackBar sRR_B;
		private Label label7;
		private TrackBar sBB_B;
		private Label label8;
		private TrackBar sBG_B;
		private Label label9;
		private TrackBar sBR_B;
		private Label label4;
		private TrackBar sGB_B;
		private Label label5;
		private TrackBar sGG_B;
		private Label label6;
		private TrackBar sGR_B;
		private Label label3;
		private TrackBar sRB_B;
		private Label label2;
		private TrackBar sRG_B;
		private TextBox textBox1;
		private Label label10;
		private PictureBox picStationOriginalImage;
		private PictureBox picShipPanels;
		private TrackBar trackGamma;
		private Label label11;
		private TrackBar trackSaturation;
		private Label label12;
	}
}
