namespace EDHM_UI
{
	partial class ReadShaderForm
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
			this.txtFileName = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtMinValue = new System.Windows.Forms.NumericUpDown();
			this.cmdFindFile = new System.Windows.Forms.Button();
			this.txtMaxValue = new System.Windows.Forms.NumericUpDown();
			this.txtLineNumber = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.cboKind = new System.Windows.Forms.ComboBox();
			this.cmdOK = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.txtLineText = new System.Windows.Forms.TextBox();
			this.cmdShowFile = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.txtMinValue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtMaxValue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtLineNumber)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtFileName
			// 
			this.txtFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFileName.Location = new System.Drawing.Point(46, 6);
			this.txtFileName.Name = "txtFileName";
			this.txtFileName.Size = new System.Drawing.Size(249, 20);
			this.txtFileName.TabIndex = 2;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(5, 59);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(27, 13);
			this.label7.TabIndex = 34;
			this.label7.Text = "Min:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(2, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(26, 13);
			this.label2.TabIndex = 25;
			this.label2.Text = "File:";
			// 
			// txtMinValue
			// 
			this.txtMinValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtMinValue.DecimalPlaces = 1;
			this.txtMinValue.Location = new System.Drawing.Point(46, 55);
			this.txtMinValue.Name = "txtMinValue";
			this.txtMinValue.Size = new System.Drawing.Size(79, 20);
			this.txtMinValue.TabIndex = 5;
			// 
			// cmdFindFile
			// 
			this.cmdFindFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdFindFile.FlatAppearance.BorderSize = 0;
			this.cmdFindFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.cmdFindFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdFindFile.Image = global::EDHM_UI.Properties.Resources._1352899653_folder_horizontal_open;
			this.cmdFindFile.Location = new System.Drawing.Point(301, 4);
			this.cmdFindFile.Name = "cmdFindFile";
			this.cmdFindFile.Size = new System.Drawing.Size(28, 22);
			this.cmdFindFile.TabIndex = 1;
			this.cmdFindFile.UseVisualStyleBackColor = false;
			this.cmdFindFile.Click += new System.EventHandler(this.cmdFindFile_Click);
			// 
			// txtMaxValue
			// 
			this.txtMaxValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMaxValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtMaxValue.DecimalPlaces = 1;
			this.txtMaxValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.txtMaxValue.Location = new System.Drawing.Point(172, 55);
			this.txtMaxValue.Name = "txtMaxValue";
			this.txtMaxValue.Size = new System.Drawing.Size(187, 20);
			this.txtMaxValue.TabIndex = 6;
			// 
			// txtLineNumber
			// 
			this.txtLineNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtLineNumber.Location = new System.Drawing.Point(46, 30);
			this.txtLineNumber.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.txtLineNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.txtLineNumber.Name = "txtLineNumber";
			this.txtLineNumber.Size = new System.Drawing.Size(79, 20);
			this.txtLineNumber.TabIndex = 3;
			this.txtLineNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.txtLineNumber.ValueChanged += new System.EventHandler(this.txtLineNumber_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(135, 59);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(30, 13);
			this.label6.TabIndex = 31;
			this.label6.Text = "Max:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(2, 34);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(30, 13);
			this.label3.TabIndex = 28;
			this.label3.Text = "Line:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(131, 34);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(34, 13);
			this.label4.TabIndex = 29;
			this.label4.Text = "Type:";
			// 
			// cboKind
			// 
			this.cboKind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboKind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboKind.FormattingEnabled = true;
			this.cboKind.Items.AddRange(new object[] {
            "Single Value (Decimal)",
            "Color Value",
            "Triple Value (Linked)",
            "Quadruple Value"});
			this.cboKind.Location = new System.Drawing.Point(171, 30);
			this.cboKind.Name = "cboKind";
			this.cboKind.Size = new System.Drawing.Size(188, 21);
			this.cboKind.TabIndex = 4;
			this.cboKind.DropDownClosed += new System.EventHandler(this.cboKind_DropDownClosed);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdOK.Location = new System.Drawing.Point(284, 180);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 0;
			this.cmdOK.Text = "&OK";
			this.cmdOK.UseVisualStyleBackColor = false;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.txtValue);
			this.groupBox1.Controls.Add(this.txtLineText);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.groupBox1.Location = new System.Drawing.Point(5, 83);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(352, 93);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Shader Text:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(7, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(68, 13);
			this.label8.TabIndex = 4;
			this.label8.Text = "Shader Text:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(103, 16);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(243, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "* This is What is currently saved in the Shader File";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 67);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Parsed Value:";
			// 
			// txtValue
			// 
			this.txtValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtValue.Location = new System.Drawing.Point(86, 64);
			this.txtValue.Name = "txtValue";
			this.txtValue.Size = new System.Drawing.Size(258, 20);
			this.txtValue.TabIndex = 1;
			this.txtValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txtLineText
			// 
			this.txtLineText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLineText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtLineText.Location = new System.Drawing.Point(7, 38);
			this.txtLineText.Name = "txtLineText";
			this.txtLineText.Size = new System.Drawing.Size(337, 20);
			this.txtLineText.TabIndex = 0;
			// 
			// cmdShowFile
			// 
			this.cmdShowFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdShowFile.FlatAppearance.BorderSize = 0;
			this.cmdShowFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.cmdShowFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdShowFile.Image = global::EDHM_UI.Properties.Resources.magnifier;
			this.cmdShowFile.Location = new System.Drawing.Point(329, 4);
			this.cmdShowFile.Name = "cmdShowFile";
			this.cmdShowFile.Size = new System.Drawing.Size(28, 22);
			this.cmdShowFile.TabIndex = 35;
			this.cmdShowFile.UseVisualStyleBackColor = false;
			this.cmdShowFile.Click += new System.EventHandler(this.cmdShowFile_Click);
			// 
			// ReadShaderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(363, 209);
			this.Controls.Add(this.cmdShowFile);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.txtFileName);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtMinValue);
			this.Controls.Add(this.cmdFindFile);
			this.Controls.Add(this.txtMaxValue);
			this.Controls.Add(this.txtLineNumber);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cboKind);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ReadShaderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ReadShaderForm";
			this.Load += new System.EventHandler(this.ReadShaderForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.txtMinValue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtMaxValue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtLineNumber)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtFileName;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown txtMinValue;
		private System.Windows.Forms.Button cmdFindFile;
		private System.Windows.Forms.NumericUpDown txtMaxValue;
		private System.Windows.Forms.NumericUpDown txtLineNumber;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox cboKind;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox txtLineText;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtValue;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button cmdShowFile;
	}
}