using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;

namespace EDHM_UI
{
	public partial class ShadersEditor : Form
	{
		private json_settings JsonSettings = null;
		private string ED_X64_PATH = string.Empty;

		public ShadersEditor()
		{
			InitializeComponent();
		}

		private void ShadersEditor_Load(object sender, EventArgs e)
		{
			string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
			string JsonPath = System.IO.Path.Combine(AppExePath, "EDHM_UI_Presets.json");
			this.ED_X64_PATH = ConfigurationManager.AppSettings["EDx64_Path"].ToString();

			#region Ask for ED x64 Folder if haven't set it yet

			if (this.ED_X64_PATH == string.Empty)
			{
				//OpenFolder Dialog:
				FolderBrowserDialog Dir = new FolderBrowserDialog();
				Dir.Description = @"Please Select the Game's Directory:                                         'Elite Dangerous\Products\elite-dangerous-64' ";
				Dir.ShowNewFolderButton = true;
				Dir.SelectedPath = AppExePath;

				if (Dir.ShowDialog() == DialogResult.OK)
				{
					if (System.IO.Directory.Exists(Dir.SelectedPath))
					{
						this.ED_X64_PATH = Dir.SelectedPath;

						Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
						config.AppSettings.Settings["EDx64_Path"].Value = this.ED_X64_PATH;
						config.Save(ConfigurationSaveMode.Modified);
						ConfigurationManager.RefreshSection("appSettings");
					}
				}
				else
				{
					this.Close();
				}
			}

			#endregion

			#region Load the Preset Settings

			if (System.IO.File.Exists(JsonPath))
			{
				// Testing Only:
				//json_settings _JsonSettings = new json_settings();
				//_JsonSettings.current_preset = "Test_Preset";
				//_JsonSettings.Presets = new List<preset>();
				//Util.Serialize_ToJSON(System.IO.Path.Combine(AppExePath, "EDHM_UI_Presets2.json"), _JsonSettings);

				this.JsonSettings = Util.DeSerialize_FromJSON<json_settings>(JsonPath);
				if (this.JsonSettings != null)
				{
					//Load the Presets Names into the Combo Box:
					this.cboProfiles.Items.Clear();
					foreach (var _preset in this.JsonSettings.Presets)
					{
						this.cboProfiles.Items.Add(_preset.name);
					}

					//Load the Currently Applied Preset:
					if (!this.JsonSettings.current_preset.EmptyOrNull())
					{
						var _Preset = this.JsonSettings.Presets.Find(x => x.name == this.JsonSettings.current_preset);
						if (_Preset != null)
						{
							LoadPreset(_Preset);
						}						
					}
				}
			}
			else
			{
				MessageBox.Show("Unable to load the file 'EDHM_UI_Presets.json', make sure it is present next to this program.",
					"ERROR 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			#endregion
		}
		
		private void LoadPreset(preset _Preset)
		{
			try
			{
				if (this.JsonSettings != null && _Preset != null)
				{
					this.cboProfiles.Text = _Preset.name;
					this.flowLayoutPanel1.Controls.Clear();

					if (_Preset.Shaders != null && _Preset.Shaders.Count > 0)
					{
						foreach (var _Shader in _Preset.Shaders)
						{
							ShaderControl _SC = new ShaderControl(_Shader, this.ED_X64_PATH);
							_SC.ValueChanged += _SC_ValueChanged;

							this.flowLayoutPanel1.Controls.Add(_SC);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		private void LoadPreset(int Index)
		{
			try
			{
				if (this.JsonSettings != null && Index >= 0)
				{
					preset _Preset = this.JsonSettings.Presets[Index];
					this.flowLayoutPanel1.Controls.Clear();

					foreach (var _Shader in _Preset.Shaders)
					{
						ShaderControl _SC = new ShaderControl(_Shader, this.ED_X64_PATH);
						_SC.ValueChanged += _SC_ValueChanged;

						this.flowLayoutPanel1.Controls.Add(_SC);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void _SC_ValueChanged(object sender, EventArgs e)
		{
			//Here we can know when a Shader is Changed
		}

		private void cboProfiles_DropDownClosed(object sender, EventArgs e)
		{
			try
			{
				LoadPreset(this.cboProfiles.SelectedIndex);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void cmdImportProfile_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialog OFDialog = new OpenFileDialog()
				{
					Filter = "JSON File|*.json",
					FilterIndex = 0,
					DefaultExt = "json",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = true,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};
				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					preset _Preset = Util.DeSerialize_FromJSON<preset>(OFDialog.FileName);
					if (_Preset != null)
					{
						if (this.JsonSettings == null) this.JsonSettings.Presets = new List<preset>();
						this.JsonSettings.Presets.Add(_Preset);
						this.cboProfiles.Items.Add(_Preset.name);
						LoadPreset(this.cboProfiles.Items.Count - 1);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		private void cmdExportProfile_Click(object sender, EventArgs e)
		{
			try
			{
				if (this.JsonSettings.Presets != null)
				{
					preset _Preset = this.JsonSettings.Presets[this.cboProfiles.SelectedIndex];
					if (_Preset != null)
					{
						SaveFileDialog SFDialog = new SaveFileDialog()
						{
							Filter = "JSON File|*.json",
							FilterIndex = 0,
							DefaultExt = "json",
							AddExtension = true,
							CheckPathExists = true,
							OverwritePrompt = true,
							FileName = _Preset.name,
							InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
						};

						if (SFDialog.ShowDialog() == DialogResult.OK)
						{
							Util.Serialize_ToJSON(SFDialog.FileName, _Preset);
							if (System.IO.File.Exists(SFDialog.FileName))
							{
								MessageBox.Show("Preset Exported.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void cmdSaveProfile_Click(object sender, EventArgs e)
		{
			try
			{
				// THIS SAVES THE CHANGES IN THE PRESET INTO THE JSON FILE */
				string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
				string NewFilePath = System.IO.Path.Combine(AppExePath, "EDHM_UI_Presets.json");

				if (System.IO.File.Exists(NewFilePath))
				{
					if (this.JsonSettings != null)
					{
						Util.Serialize_ToJSON(NewFilePath, this.JsonSettings);
						MessageBox.Show("Preset Saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		private void cmdSaveShaderChanges_Click(object sender, EventArgs e)
		{
			/*  THIS APPLY THE CHANGES INTO THE CORRESPONDANT SHADER FILES */
			try
			{
				if (this.flowLayoutPanel1.Controls != null && this.flowLayoutPanel1.Controls.Count > 0)
				{
					this.Cursor = Cursors.WaitCursor;
					int _Counter = 0;

					foreach (ShaderControl _Control in this.flowLayoutPanel1.Controls)
					{
						if (_Control.SaveShader())
						{
							_Counter++;
						}
					}

					if (_Counter > 0)
					{
						this.JsonSettings.current_preset = this.cboProfiles.Text;
						MessageBox.Show(string.Format("{0} Shaders Saved", _Counter), "Saved Done",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						MessageBox.Show("Nothing has been Saved!", "No Changes",
							MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally { this.Cursor = Cursors.Default; }
		}

		private void cmdAddNewShader_Click(object sender, EventArgs e)
		{		
			/*  ADDS A NEW SHADER SETTING INTO THE CURRENT PRESET  */
			preset _Preset = this.JsonSettings.Presets[this.cboProfiles.SelectedIndex];
			if (_Preset != null)
			{
				shader _Shader = new shader();
				_Preset.Shaders.Add(_Shader);

				ShaderControl _SC = new ShaderControl(_Shader, this.ED_X64_PATH);
				_SC.ValueChanged += _SC_ValueChanged;

				this.flowLayoutPanel1.Controls.Add(_SC);
			}			
		}
		private void cmdRemoveShader_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure of Removing the selected shaders?", "Confirm Action?", 
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				int _Counter = 0;
				foreach (ShaderControl _Control in this.flowLayoutPanel1.Controls)
				{
					if (_Control.Shader.is_selected)
					{	
						this.JsonSettings.Presets[this.cboProfiles.SelectedIndex].Shaders.Remove(_Control.Shader);
						_Counter++;
					}
				}
				LoadPreset(this.cboProfiles.SelectedIndex);
				MessageBox.Show(string.Format("{0} Shaders Removed!\r\nMake sure of Saving the Changes when you are done.", _Counter), "Done",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
			}			
		}

		private void cmdCopyProfileAs_Click(object sender, EventArgs e)
		{
			/* Here we are gonna take all the selected Shaders (or them all if none is selected)
			 * and build a new Preset whith them 		 */
			if (this.JsonSettings.Presets != null)
			{
				preset _Preset = this.JsonSettings.Presets[this.cboProfiles.SelectedIndex];
				if (_Preset != null)
				{
					int _Counter = 0;
					preset _NewPreset = new preset();
					_NewPreset.Shaders = new List<shader>();

					foreach (var _Shader in _Preset.Shaders)
					{
						if (_Shader.is_selected) { _NewPreset.Shaders.Add(_Shader); _Counter++; }
					}

					if (_Counter <= 0) _NewPreset.Shaders = _Preset.Shaders;

					string _Name = string.Empty;
					if (Util.ShowInputDialog("Name?", ref _Name) == DialogResult.OK)
					{
						_NewPreset.name = _Name.Trim();
						this.JsonSettings.Presets.Add(_NewPreset);

						this.cboProfiles.Items.Add(_NewPreset.name);
						this.cboProfiles.SelectedIndex = this.cboProfiles.Items.Count - 1;

						LoadPreset(this.cboProfiles.SelectedIndex);
						MessageBox.Show("Preset Added!\r\nMake sure of Saving the Changes when you are done.", "Done",
									MessageBoxButtons.OK, MessageBoxIcon.Information);
					}					
				}
			}
		}
	}
}