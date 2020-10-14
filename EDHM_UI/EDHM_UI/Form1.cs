using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EDHM_UI
{
	/* INI Editor for EDHM Mod, Autor: Cmdr BlueMystic  */

	public partial class Form1 : Form
	{
		private IniFile _Reader = null;
		private json_Lists _Lists = null;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			try
			{				
				string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
				string NewFilePath = System.IO.Path.Combine(AppExePath, "EDHM_Lists.json");

				if (System.IO.File.Exists(NewFilePath))
				{
					this._Lists = Util.DeSerialize_FromJSON<json_Lists>(NewFilePath);
				}
				else
				{
					MessageBox.Show("Unable to load the file 'EDHM_Lists.json', make sure it is present next to this program.", 
						"ERROR 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void Form1_Shown(object sender, EventArgs e)
		{
			try
			{
				if (this._Lists != null)
				{
					#region 0. Load the Presets:

					if (this._Lists.Presets != null && this._Lists.Presets.Count > 0)
					{
						this.cboPresets.Items.Clear();

						foreach (preset _Preset in this._Lists.Presets)
						{
							this.cboPresets.Items.Add(_Preset.name.Trim());
						}
					}

					#endregion

					#region 1. Load the 'Ambient Cabin Lights' List:

					if (this._Lists.AmbientCabinLights != null && this._Lists.AmbientCabinLights.Count > 0)
					{
						this.cboAmbientCabinLights.Items.Clear();
						this.cboAmbientCabinLights.Items.Add(string.Empty);

						foreach (string _ValueKey in this._Lists.AmbientCabinLights)
						{
							string[] Palabras = _ValueKey.Split(new char[] { '=' });
							if (Palabras != null)
							{
								this.cboAmbientCabinLights.Items.Add(Palabras[1].Trim());
							}
						}
						numAmbientCabinLights.Minimum = 0;
						numAmbientCabinLights.Maximum = this._Lists.AmbientCabinLights.Count;
					}

					#endregion

					#region 2. Load the 'TargetingReticle' List:

					if (this._Lists.TargetingReticle != null)
					{
						this.cboTargetingReticle.Items.Clear();
						this.cboTargetingReticle.Items.Add(string.Empty);

						foreach (string _ValueKey in this._Lists.TargetingReticle)
						{
							string[] Palabras = _ValueKey.Split(new char[] { '=' });
							if (Palabras != null)
							{
								this.cboTargetingReticle.Items.Add(Palabras[1].Trim());
							}
						}
						numTargetingReticle.Minimum = 0;
						numTargetingReticle.Maximum = this._Lists.TargetingReticle.Count;
					}

					#endregion

					#region 3. Load the 'ShieldColour' List:

					if (this._Lists.ShieldColour != null)
					{
						this.cboShieldColor.Items.Clear();
						this.cboShieldColor.Items.Add(string.Empty);

						foreach (string _ValueKey in this._Lists.ShieldColour)
						{
							string[] Palabras = _ValueKey.Split(new char[] { '=' });
							if (Palabras != null)
							{
								this.cboShieldColor.Items.Add(Palabras[1].Trim());
							}
						}
						numShieldColor.Minimum = 0;
						numShieldColor.Maximum = this._Lists.ShieldColour.Count;
					}

					#endregion

					#region 4. Load the 'OwnShipHologram' List:

					if (this._Lists.OwnShipHologram != null)
					{
						this.cboShipHologram_1.Items.Clear();
						this.cboShipHologram_1.Items.Add(string.Empty);

						foreach (string _ValueKey in this._Lists.OwnShipHologram)
						{
							string[] Palabras = _ValueKey.Split(new char[] { '=' });
							if (Palabras != null)
							{
								this.cboShipHologram_1.Items.Add(Palabras[1].Trim());
							}
						}
						numShipHologram_1.Minimum = 0;
						numShipHologram_1.Maximum = this._Lists.OwnShipHologram.Count;
					}

					#endregion

					#region 5. Load the 'Distributor' List:

					if (this._Lists.Distributor != null)
					{
						this.cboDistributor.Items.Clear();
						this.cboDistributor.Items.Add(string.Empty);

						foreach (string _ValueKey in this._Lists.Distributor)
						{
							string[] Palabras = _ValueKey.Split(new char[] { '=' });
							if (Palabras != null)
							{
								this.cboDistributor.Items.Add(Palabras[1].Trim());
							}
						}
						numDistributor.Minimum = 0;
						numDistributor.Maximum = this._Lists.Distributor.Count;
					}

					#endregion
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void cmdOpenFile_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialog OFDialog = new OpenFileDialog();
				OFDialog.Filter = "INI File|*.ini|All Files|*.*";
				OFDialog.FilterIndex = 0;
				OFDialog.DefaultExt = "ini";
				OFDialog.AddExtension = true;
				OFDialog.CheckPathExists = true;
				OFDialog.CheckFileExists = true;
				OFDialog.FileName = "d3dx.ini";
				OFDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				//TODO: Open the correct File Location, or remember last opened folder?

				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					System.IO.FileInfo file = new System.IO.FileInfo(OFDialog.FileName);
					this.txtIniFile_Path.Text = OFDialog.FileName;
					this.labelStatus.Text = string.Format("Ini File Opened, {0:n1} kb Readed.", (file.Length / 1024));

					_Reader = new IniFile(OFDialog.FileName); //<- Open and Read the INI file

					#region Here we load the Options saved in the Ini File

					string _HudDimmer = _Reader.ReadKey("w7", "Constants");
					if (_HudDimmer != string.Empty)
					{
						this.trackHudDimmer.Value = Convert.ToInt32(_HudDimmer);
					}

					string _LightingDimmer = _Reader.ReadKey("w2", "Constants");
					if (_LightingDimmer != string.Empty)
					{
						decimal _Value = Convert.ToDecimal(_LightingDimmer);
						this.trackLightingDimmer.Value = Convert.ToInt32(_Value * 10);
					}

					string _AmbientCabinLights = _Reader.ReadKey("w4", "Constants");
					if (_AmbientCabinLights != string.Empty)
					{
						this.cboAmbientCabinLights.SelectedIndex = Convert.ToInt32(_AmbientCabinLights);
					}

					string _TargetingReticle = _Reader.ReadKey("w5", "Constants");
					if (_TargetingReticle != string.Empty)
					{
						this.cboTargetingReticle.SelectedIndex = Convert.ToInt32(_TargetingReticle);
					}

					string _ShieldColour = _Reader.ReadKey("w8", "Constants");
					if (_ShieldColour != string.Empty)
					{
						this.cboShieldColor.SelectedIndex = Convert.ToInt32(_ShieldColour);
					}

					string _OwnShipHologram = _Reader.ReadKey("w9", "Constants");
					if (_OwnShipHologram != string.Empty)
					{
						this.cboShipHologram_1.SelectedIndex = Convert.ToInt32(_OwnShipHologram);
					}

					string _Distributor = _Reader.ReadKey("w10", "Constants");
					if (_Distributor != string.Empty)
					{
						this.cboDistributor.SelectedIndex = Convert.ToInt32(_Distributor);
					}

					#endregion
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}			
		}
		private void cmdSaveSettings_Click(object sender, EventArgs e)
		{
			try
			{
				if (this.txtIniFile_Path.Text != string.Empty)
				{
					if (MessageBox.Show("Are you Sure of Saving the Current Settings?", "Confirm Save",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						_Reader.WriteKey("w7", this.trackHudDimmer.Value.ToString(), "Constants");
						_Reader.WriteKey("w2", (this.trackLightingDimmer.Value / 10).ToString(), "Constants");
						_Reader.WriteKey("w4", this.cboAmbientCabinLights.SelectedIndex.ToString(), "Constants");
						_Reader.WriteKey("w5", this.cboTargetingReticle.SelectedIndex.ToString(), "Constants");
						_Reader.WriteKey("w8", this.cboShieldColor.SelectedIndex.ToString(), "Constants");
						_Reader.WriteKey("w9", this.cboShipHologram_1.SelectedIndex.ToString(), "Constants");
						_Reader.WriteKey("w10", this.cboDistributor.SelectedIndex.ToString(), "Constants");
						_Reader.WriteKey("w7", this.trackHudDimmer.Value.ToString(), "Constants");

						//TODO: More settings here
						MessageBox.Show("Settings Saved Succesfully!", "Done",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				else
				{
					MessageBox.Show("You need to Open the 'd3dx.ini' file.", "ERROR", 
						MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void trackHudDimmer_ValueChanged(object sender, EventArgs e)
		{
			numHudDimmer.Value = trackHudDimmer.Value;
		}
		private void numHudDimmer_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(numHudDimmer.Value);
			trackHudDimmer.Value = Convert.ToInt32(_Value);
		}

		private void trackLightingDimmer_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(trackLightingDimmer.Value);
			numLightingDimmer.Value = _Value / 10;
		}
		private void numLightingDimmer_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(numLightingDimmer.Value);
			trackLightingDimmer.Value = Convert.ToInt32(_Value * 10);
		}

		private void cboAmbientCabinLights_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboAmbientCabinLights.Text != string.Empty)
			{
				numAmbientCabinLights.Value = cboAmbientCabinLights.SelectedIndex;
			}
		}
		private void numAmbientCabinLights_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(numAmbientCabinLights.Value);
			cboAmbientCabinLights.SelectedIndex = Convert.ToInt32(_Value);
		}

		private void cboTargetingReticle_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboTargetingReticle.Text != string.Empty)
			{
				numTargetingReticle.Value = cboTargetingReticle.SelectedIndex;
			}
		}
		private void numTargetingReticle_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(numTargetingReticle.Value);
			cboTargetingReticle.SelectedIndex = Convert.ToInt32(_Value);
		}

		private void cboShieldColor_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboShieldColor.Text != string.Empty)
			{
				numShieldColor.Value = cboShieldColor.SelectedIndex;
			}
		}
		private void numShieldColor_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(numShieldColor.Value);
			cboShieldColor.SelectedIndex = Convert.ToInt32(_Value);
		}

		private void cboShipHologram_1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboShipHologram_1.Text != string.Empty)
			{
				numShipHologram_1.Value = cboShipHologram_1.SelectedIndex;
			}
		}
		private void numShipHologram_1_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(numShipHologram_1.Value);
			cboShipHologram_1.SelectedIndex = Convert.ToInt32(_Value);
		}

		private void cboDistributor_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboDistributor.Text != string.Empty)
			{
				numDistributor.Value = cboDistributor.SelectedIndex;
			}
		}
		private void numDistributor_ValueChanged(object sender, EventArgs e)
		{
			decimal _Value = Convert.ToDecimal(numDistributor.Value);
			cboDistributor.SelectedIndex = Convert.ToInt32(_Value);
		}

		private void cboPresets_DropDownClosed(object sender, EventArgs e)
		{
			/* HERE WE GONNA LOAD A PRESET */
			try
			{
				int _Index = cboPresets.SelectedIndex;
				if (_Index >= 0)
				{
					if (this._Lists.Presets != null && this._Lists.Presets.Count > 0)
					{
						preset _Preset = this._Lists.Presets[_Index];
						if (_Preset != null)
						{
							if (_Preset.w7 >= 0) this.trackHudDimmer.Value = _Preset.w7;

							if (_Preset.w2 >= 0) this.trackLightingDimmer.Value = Convert.ToInt32(_Preset.w2 * 10);

							if (_Preset.w4 >= 0) this.cboAmbientCabinLights.SelectedIndex = _Preset.w4;

							if (_Preset.w5 >= 0) this.cboTargetingReticle.SelectedIndex = _Preset.w5;

							if (_Preset.w8 >= 0) this.cboShieldColor.SelectedIndex = _Preset.w8;

							if (_Preset.w9 >= 0) this.cboShipHologram_1.SelectedIndex = _Preset.w9;

							if (_Preset.w10 >= 0) this.cboDistributor.SelectedIndex = _Preset.w10;

							//TODO: Add more settings here
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void cmdSavePreset_Click(object sender, EventArgs e)
		{
			/*  HERE WE ARE GONNA SAVE THE CURRENT SETTINGS INTO A PRESET
			 *  Can Write a new Name for the Preset
			 *  Or if the Name is taken, then the existing Preset is Overwritten  */
			try
			{
				if (cboPresets.Text != string.Empty)
				{
					preset _Preset = new preset();
					_Preset.name = this.cboPresets.Text;
					_Preset.w7 = this.trackHudDimmer.Value;
					_Preset.w2 = this.trackLightingDimmer.Value / 10;
					_Preset.w4 = this.cboAmbientCabinLights.SelectedIndex;
					_Preset.w5 = this.cboTargetingReticle.SelectedIndex;
					_Preset.w8 = this.cboShieldColor.SelectedIndex;
					_Preset.w9 = this.cboShipHologram_1.SelectedIndex;
					_Preset.w10 = this.cboDistributor.SelectedIndex;
					//TODO: More settings here

					
					preset _P = _Lists.Presets.Find(x => x.name == _Preset.name);
					if (_P != null)
					{
						_P = _Preset;
					}
					else
					{
						_Lists.Presets.Add(_Preset);
					}

					//Saving the JSON file:
					string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
					string NewFilePath = System.IO.Path.Combine(AppExePath, "EDHM_Lists.json");
					string _JsonString = Util.Serialize_ToJSON(NewFilePath, _Lists);
					if (_JsonString != string.Empty)
					{
						MessageBox.Show("Preset Saved Succesfully!", "Done",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				else
				{
					MessageBox.Show("You need to type a name for the Preset!", "ERROR", 
						MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}