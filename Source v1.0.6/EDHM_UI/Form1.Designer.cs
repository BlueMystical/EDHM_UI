namespace EDHM_UI
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
			this.txtIniFile_Path = new System.Windows.Forms.TextBox();
			this.cmdOpenFile = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.trackHudDimmer = new System.Windows.Forms.TrackBar();
			this.trackLightingDimmer = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.numLightingDimmer = new System.Windows.Forms.NumericUpDown();
			this.numHudDimmer = new System.Windows.Forms.NumericUpDown();
			this.numAmbientCabinLights = new System.Windows.Forms.NumericUpDown();
			this.numTargetingReticle = new System.Windows.Forms.NumericUpDown();
			this.numShieldColor = new System.Windows.Forms.NumericUpDown();
			this.cboAmbientCabinLights = new System.Windows.Forms.ComboBox();
			this.cboTargetingReticle = new System.Windows.Forms.ComboBox();
			this.cboShieldColor = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cboShipHologram_1 = new System.Windows.Forms.ComboBox();
			this.numShipHologram_1 = new System.Windows.Forms.NumericUpDown();
			this.numDistributor = new System.Windows.Forms.NumericUpDown();
			this.cboDistributor = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.labelStatus = new System.Windows.Forms.Label();
			this.cmdSaveSettings = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cmdSavePreset = new System.Windows.Forms.Button();
			this.cboPresets = new System.Windows.Forms.ComboBox();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			((System.ComponentModel.ISupportInitialize)(this.trackHudDimmer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackLightingDimmer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numLightingDimmer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numHudDimmer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numAmbientCabinLights)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numTargetingReticle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numShieldColor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numShipHologram_1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numDistributor)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtIniFile_Path
			// 
			this.txtIniFile_Path.Location = new System.Drawing.Point(15, 12);
			this.txtIniFile_Path.Name = "txtIniFile_Path";
			this.txtIniFile_Path.Size = new System.Drawing.Size(392, 20);
			this.txtIniFile_Path.TabIndex = 0;
			// 
			// cmdOpenFile
			// 
			this.cmdOpenFile.Location = new System.Drawing.Point(413, 10);
			this.cmdOpenFile.Name = "cmdOpenFile";
			this.cmdOpenFile.Size = new System.Drawing.Size(66, 23);
			this.cmdOpenFile.TabIndex = 1;
			this.cmdOpenFile.Text = "&Open File";
			this.cmdOpenFile.UseVisualStyleBackColor = true;
			this.cmdOpenFile.Click += new System.EventHandler(this.cmdOpenFile_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 76);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "HUD Dimmer";
			// 
			// trackHudDimmer
			// 
			this.trackHudDimmer.LargeChange = 2;
			this.trackHudDimmer.Location = new System.Drawing.Point(127, 75);
			this.trackHudDimmer.Maximum = 6;
			this.trackHudDimmer.Name = "trackHudDimmer";
			this.trackHudDimmer.Size = new System.Drawing.Size(292, 45);
			this.trackHudDimmer.TabIndex = 3;
			this.trackHudDimmer.ValueChanged += new System.EventHandler(this.trackHudDimmer_ValueChanged);
			// 
			// trackLightingDimmer
			// 
			this.trackLightingDimmer.Location = new System.Drawing.Point(127, 112);
			this.trackLightingDimmer.Name = "trackLightingDimmer";
			this.trackLightingDimmer.Size = new System.Drawing.Size(292, 45);
			this.trackLightingDimmer.TabIndex = 4;
			this.trackLightingDimmer.ValueChanged += new System.EventHandler(this.trackLightingDimmer_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 112);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(82, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Lighting Dimmer";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(15, 156);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(106, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Ambient Cabin Lights";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(15, 183);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(83, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Targeting reticle";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(15, 210);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(63, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "Shield Color";
			// 
			// numLightingDimmer
			// 
			this.numLightingDimmer.DecimalPlaces = 1;
			this.numLightingDimmer.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numLightingDimmer.Location = new System.Drawing.Point(425, 112);
			this.numLightingDimmer.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numLightingDimmer.Name = "numLightingDimmer";
			this.numLightingDimmer.Size = new System.Drawing.Size(54, 20);
			this.numLightingDimmer.TabIndex = 11;
			this.numLightingDimmer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numLightingDimmer.ValueChanged += new System.EventHandler(this.numLightingDimmer_ValueChanged);
			// 
			// numHudDimmer
			// 
			this.numHudDimmer.Location = new System.Drawing.Point(425, 76);
			this.numHudDimmer.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
			this.numHudDimmer.Name = "numHudDimmer";
			this.numHudDimmer.Size = new System.Drawing.Size(54, 20);
			this.numHudDimmer.TabIndex = 12;
			this.numHudDimmer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numHudDimmer.ValueChanged += new System.EventHandler(this.numHudDimmer_ValueChanged);
			// 
			// numAmbientCabinLights
			// 
			this.numAmbientCabinLights.Location = new System.Drawing.Point(425, 152);
			this.numAmbientCabinLights.Name = "numAmbientCabinLights";
			this.numAmbientCabinLights.Size = new System.Drawing.Size(54, 20);
			this.numAmbientCabinLights.TabIndex = 13;
			this.numAmbientCabinLights.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numAmbientCabinLights.ValueChanged += new System.EventHandler(this.numAmbientCabinLights_ValueChanged);
			// 
			// numTargetingReticle
			// 
			this.numTargetingReticle.Location = new System.Drawing.Point(425, 179);
			this.numTargetingReticle.Name = "numTargetingReticle";
			this.numTargetingReticle.Size = new System.Drawing.Size(54, 20);
			this.numTargetingReticle.TabIndex = 14;
			this.numTargetingReticle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numTargetingReticle.ValueChanged += new System.EventHandler(this.numTargetingReticle_ValueChanged);
			// 
			// numShieldColor
			// 
			this.numShieldColor.Location = new System.Drawing.Point(425, 206);
			this.numShieldColor.Name = "numShieldColor";
			this.numShieldColor.Size = new System.Drawing.Size(54, 20);
			this.numShieldColor.TabIndex = 15;
			this.numShieldColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numShieldColor.ValueChanged += new System.EventHandler(this.numShieldColor_ValueChanged);
			// 
			// cboAmbientCabinLights
			// 
			this.cboAmbientCabinLights.FormattingEnabled = true;
			this.cboAmbientCabinLights.Location = new System.Drawing.Point(127, 152);
			this.cboAmbientCabinLights.Name = "cboAmbientCabinLights";
			this.cboAmbientCabinLights.Size = new System.Drawing.Size(292, 21);
			this.cboAmbientCabinLights.TabIndex = 16;
			this.cboAmbientCabinLights.SelectedIndexChanged += new System.EventHandler(this.cboAmbientCabinLights_SelectedIndexChanged);
			// 
			// cboTargetingReticle
			// 
			this.cboTargetingReticle.FormattingEnabled = true;
			this.cboTargetingReticle.Location = new System.Drawing.Point(127, 179);
			this.cboTargetingReticle.Name = "cboTargetingReticle";
			this.cboTargetingReticle.Size = new System.Drawing.Size(292, 21);
			this.cboTargetingReticle.TabIndex = 17;
			this.cboTargetingReticle.SelectedIndexChanged += new System.EventHandler(this.cboTargetingReticle_SelectedIndexChanged);
			// 
			// cboShieldColor
			// 
			this.cboShieldColor.FormattingEnabled = true;
			this.cboShieldColor.Location = new System.Drawing.Point(127, 206);
			this.cboShieldColor.Name = "cboShieldColor";
			this.cboShieldColor.Size = new System.Drawing.Size(292, 21);
			this.cboShieldColor.TabIndex = 18;
			this.cboShieldColor.SelectedIndexChanged += new System.EventHandler(this.cboShieldColor_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(15, 236);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(98, 13);
			this.label6.TabIndex = 19;
			this.label6.Text = "OwnShip Hologram";
			// 
			// cboShipHologram_1
			// 
			this.cboShipHologram_1.FormattingEnabled = true;
			this.cboShipHologram_1.Location = new System.Drawing.Point(127, 232);
			this.cboShipHologram_1.Name = "cboShipHologram_1";
			this.cboShipHologram_1.Size = new System.Drawing.Size(292, 21);
			this.cboShipHologram_1.TabIndex = 20;
			this.cboShipHologram_1.SelectedIndexChanged += new System.EventHandler(this.cboShipHologram_1_SelectedIndexChanged);
			// 
			// numShipHologram_1
			// 
			this.numShipHologram_1.Location = new System.Drawing.Point(425, 232);
			this.numShipHologram_1.Name = "numShipHologram_1";
			this.numShipHologram_1.Size = new System.Drawing.Size(54, 20);
			this.numShipHologram_1.TabIndex = 21;
			this.numShipHologram_1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numShipHologram_1.ValueChanged += new System.EventHandler(this.numShipHologram_1_ValueChanged);
			// 
			// numDistributor
			// 
			this.numDistributor.Location = new System.Drawing.Point(425, 259);
			this.numDistributor.Name = "numDistributor";
			this.numDistributor.Size = new System.Drawing.Size(54, 20);
			this.numDistributor.TabIndex = 24;
			this.numDistributor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numDistributor.ValueChanged += new System.EventHandler(this.numDistributor_ValueChanged);
			// 
			// cboDistributor
			// 
			this.cboDistributor.FormattingEnabled = true;
			this.cboDistributor.Location = new System.Drawing.Point(127, 259);
			this.cboDistributor.Name = "cboDistributor";
			this.cboDistributor.Size = new System.Drawing.Size(292, 21);
			this.cboDistributor.TabIndex = 23;
			this.cboDistributor.SelectedIndexChanged += new System.EventHandler(this.cboDistributor_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(15, 263);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(54, 13);
			this.label7.TabIndex = 22;
			this.label7.Text = "Distributor";
			// 
			// labelStatus
			// 
			this.labelStatus.AutoSize = true;
			this.labelStatus.Location = new System.Drawing.Point(15, 35);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(167, 13);
			this.labelStatus.TabIndex = 25;
			this.labelStatus.Text = "To Start, Select the EDHM Ini File";
			// 
			// cmdSaveSettings
			// 
			this.cmdSaveSettings.Location = new System.Drawing.Point(413, 39);
			this.cmdSaveSettings.Name = "cmdSaveSettings";
			this.cmdSaveSettings.Size = new System.Drawing.Size(66, 23);
			this.cmdSaveSettings.TabIndex = 26;
			this.cmdSaveSettings.Text = "&Save File";
			this.cmdSaveSettings.UseVisualStyleBackColor = true;
			this.cmdSaveSettings.Click += new System.EventHandler(this.cmdSaveSettings_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.cmdSavePreset);
			this.groupBox1.Controls.Add(this.cboPresets);
			this.groupBox1.Location = new System.Drawing.Point(16, 295);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(464, 65);
			this.groupBox1.TabIndex = 27;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Presets:";
			// 
			// cmdSavePreset
			// 
			this.cmdSavePreset.Location = new System.Drawing.Point(343, 24);
			this.cmdSavePreset.Name = "cmdSavePreset";
			this.cmdSavePreset.Size = new System.Drawing.Size(105, 23);
			this.cmdSavePreset.TabIndex = 1;
			this.cmdSavePreset.Text = "Save &Preset";
			this.cmdSavePreset.UseVisualStyleBackColor = true;
			this.cmdSavePreset.Click += new System.EventHandler(this.cmdSavePreset_Click);
			// 
			// cboPresets
			// 
			this.cboPresets.FormattingEnabled = true;
			this.cboPresets.Location = new System.Drawing.Point(15, 25);
			this.cboPresets.Name = "cboPresets";
			this.cboPresets.Size = new System.Drawing.Size(322, 21);
			this.cboPresets.TabIndex = 0;
			this.cboPresets.DropDownClosed += new System.EventHandler(this.cboPresets_DropDownClosed);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(491, 371);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cmdSaveSettings);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.numDistributor);
			this.Controls.Add(this.cboDistributor);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.numShipHologram_1);
			this.Controls.Add(this.cboShipHologram_1);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.cboShieldColor);
			this.Controls.Add(this.cboTargetingReticle);
			this.Controls.Add(this.cboAmbientCabinLights);
			this.Controls.Add(this.numShieldColor);
			this.Controls.Add(this.numTargetingReticle);
			this.Controls.Add(this.numAmbientCabinLights);
			this.Controls.Add(this.numHudDimmer);
			this.Controls.Add(this.numLightingDimmer);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.trackLightingDimmer);
			this.Controls.Add(this.trackHudDimmer);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cmdOpenFile);
			this.Controls.Add(this.txtIniFile_Path);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "EDHM Ini Editor v1.0";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			((System.ComponentModel.ISupportInitialize)(this.trackHudDimmer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackLightingDimmer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numLightingDimmer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numHudDimmer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numAmbientCabinLights)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numTargetingReticle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numShieldColor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numShipHologram_1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numDistributor)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtIniFile_Path;
		private System.Windows.Forms.Button cmdOpenFile;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TrackBar trackHudDimmer;
		private System.Windows.Forms.TrackBar trackLightingDimmer;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numLightingDimmer;
		private System.Windows.Forms.NumericUpDown numHudDimmer;
		private System.Windows.Forms.NumericUpDown numAmbientCabinLights;
		private System.Windows.Forms.NumericUpDown numTargetingReticle;
		private System.Windows.Forms.NumericUpDown numShieldColor;
		private System.Windows.Forms.ComboBox cboAmbientCabinLights;
		private System.Windows.Forms.ComboBox cboTargetingReticle;
		private System.Windows.Forms.ComboBox cboShieldColor;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cboShipHologram_1;
		private System.Windows.Forms.NumericUpDown numShipHologram_1;
		private System.Windows.Forms.NumericUpDown numDistributor;
		private System.Windows.Forms.ComboBox cboDistributor;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.Button cmdSaveSettings;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox cboPresets;
		private System.Windows.Forms.Button cmdSavePreset;
		private System.Windows.Forms.ColorDialog colorDialog1;
	}
}

