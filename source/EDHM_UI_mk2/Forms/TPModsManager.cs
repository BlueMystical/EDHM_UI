using DevExpress.Utils;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraVerticalGrid.Rows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Util_Test;

namespace EDHM_UI_mk2.Forms
{
	public partial class TPModsManager : DevExpress.XtraEditors.XtraForm
	{
		#region Propiedades y Declaraciones

		public game_instance ActiveInstance { get; set; }

		private string AppExePath = AppDomain.CurrentDomain.BaseDirectory;
		private string TPMods_Path = null; //<- Ruta donde se guardar los Mods
		private string ModFullPath = null; //<- Ruta del Mod actualmente seleccionado

		private List<TPMod_Config> TPMods = null; //<- Lista de MODs detectados (Manejados e Independientes), los manejados son los que tienen un JSON
		private TPMod_Config CurrentdMod = null; //<- El Mod Actualmente Seleccionado

		private IniFile _IniReader_OLD = null;
		private XmlDocument _XmlReader = null;
		
		/// <summary>Folder where all Themes and User's preferences get saved.</summary>
		private string UI_DOCUMENTS = @"%USERPROFILE%\EDHM_UI";
		//private string UI_DOCUMENTS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Elite Dangerous\EDHM_UI");

		#endregion

		#region Constructores

		public TPModsManager(game_instance pActiveInstance)
		{
			InitializeComponent();
			ActiveInstance = pActiveInstance;
		}

		private void TPModsManager_Load(object sender, EventArgs e)
		{
			try
			{
				Apply_DXSkinColors();

				if (dockReadMe.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Hidden)
				{
					dockReadMe.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
				}

				if (dockSection.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Hidden)
				{
					dockSection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
				}

				if (dockKeys.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Hidden)
				{
					dockKeys.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
				}

				OnSectionChanged += TPModsManager_OnSectionChanged;
				OnKeyChanged += TPModsManager_OnKeyChanged;

				UI_DOCUMENTS = GetUIDocumentsDir(); //<- @"%USERPROFILE%\EDHM_UI"
				LoadModList();
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void TPModsManager_Shown(object sender, EventArgs e)
		{
			try
			{

			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		#endregion

		#region Metodos

		public void LoadModList()
		{
			try
			{
				Cursor = Cursors.WaitCursor;
				lst3PMods.BeginUpdate();
				lst3PMods.Elements.Clear();

				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
					TPMods_Path = Path.Combine(ActiveInstance.path, @"EDHM-ini\3rdPartyMods");

					if (Directory.Exists(TPMods_Path))
					{
						//Obtiene la lista de SubDirectorios:
						List<string> directories = new List<string>(Directory.GetDirectories(TPMods_Path));
						directories.Add(TPMods_Path);

						TPMods = new List<TPMod_Config>(); //<- Lista de MODs detectados (Manejados e Independientes)

						foreach (string _Folder in directories)
						{
							var Mods = new DirectoryInfo(_Folder).GetFiles("*.json");
							if (Mods != null && Mods.Length > 0)
							{
								int ModCountThisFolder = 0;
								AccordionControlElement _ModElement = null;

								foreach (FileInfo _File in Mods)
								{
									string FileName = System.IO.Path.GetFileNameWithoutExtension(_File.FullName); //<- Nombre sin Extension ni Path
									TPMod_Config _ManagedMod = Util.DeSerialize_FromJSON<TPMod_Config>(_File.FullName);
									if (_ManagedMod != null)
									{
										_ManagedMod.managed = true;
										_ManagedMod.file_full = _File.FullName;
										_ManagedMod.root_folder = _Folder;

										#region Load Mod's Image

										//Si Tiene una Imagen:
										if (File.Exists(Path.Combine(_Folder, FileName + ".png")))
										{
											//Carga la Imagen sin dejara 'en uso':
											using (Stream stream = File.OpenRead(Path.Combine(_Folder, FileName + ".png")))
											{
												_ManagedMod.Thumbnail = System.Drawing.Image.FromStream(stream);
											}
										}
										else
										{
											if (File.Exists(Path.Combine(AppExePath, @"Images\3PM_Default.png")))
											{
												//Carga la Imagen sin dejara 'en uso':
												using (Stream stream = File.OpenRead(Path.Combine(AppExePath, @"Images\3PM_Default.png")))
												{
													_ManagedMod.Thumbnail = System.Drawing.Image.FromStream(stream);
												}
											}
										}

										#endregion

										#region Write Mod's Name

										//Escribe el Nombre del Mod Sobre la Imagen Thumbnail:
										if (_ManagedMod.Thumbnail != null)
										{
											Bitmap bm = new Bitmap(_ManagedMod.Thumbnail.Width, _ManagedMod.Thumbnail.Height);
											using (Graphics e = Graphics.FromImage(bm))
											{
												e.DrawImage(_ManagedMod.Thumbnail, new Rectangle(new Point(0, 0), new Size(_ManagedMod.Thumbnail.Width, _ManagedMod.Thumbnail.Height)));
												e.DrawString(_ManagedMod.mod_name, new System.Drawing.Font("Tahoma", 9, FontStyle.Bold),
														Brushes.White, 10, _ManagedMod.Thumbnail.Height - 20);
											}
											_ManagedMod.Thumbnail = bm;
										}

										#endregion

										#region KeyBindings

										if (_ManagedMod.mod_name == "Odyssey Key Bindings")
										{
											//Leo los Bindings desde el Registro, si existen:
											string RegValue = Util.WinReg_ReadKey("EDHM", "KeyBindings").NVL(string.Empty);
											if (!RegValue.EmptyOrNull())
											{
												var SavedBindings = Util.DeSerialize_FromJSON_String<List<TPMod_Section>>(RegValue);
												if (SavedBindings != null && _ManagedMod.sections != null)
												{
													foreach (var ModSection in _ManagedMod.sections)
													{
														var SavedSection = SavedBindings.Find(x => x.name == ModSection.name);
														if (SavedSection != null && SavedSection.keys.IsNotEmpty())
														{
															foreach (var ModKey in ModSection.keys)
															{
																var SavedKey = SavedSection.keys.Find(x => x.name == ModKey.name);
																if (SavedKey != null)
																{
																	ModKey.value = SavedKey.value;
																}
															}
														}
													}
												}												
											}
										}

										#endregion

										ModCountThisFolder++;

										TPMods.Add(_ManagedMod);

										#region Add Mod to the List

										Invoke((MethodInvoker)(() =>
										{
											SuperToolTipSetupArgs args = new SuperToolTipSetupArgs();
											args.Title.Text = string.Format("{0} ({1})", _ManagedMod.mod_name, _ManagedMod.managed ? "Managed" : "Unmanaged");
											args.Contents.Text = string.Format("    By {0}\r\n\r\nDescription:\r\n{1}", _ManagedMod.author, _ManagedMod.description);
											SuperToolTip sTooltip2 = new SuperToolTip();
											sTooltip2.Setup(args);

											//Si el Mod esta en el Root se reinicia la cuenta
											if (_ManagedMod.root_folder == TPMods_Path)
											{
												ModCountThisFolder = 0;
											}

											if (ModCountThisFolder > 1)
											{
												//Agrega un Sub-Componente:
												_ModElement.Style = ElementStyle.Group;
												_ModElement.Elements.Add(new AccordionControlElement()
												{
													Expanded = false,
													Name = _ManagedMod.mod_name,
													Text = _ManagedMod.Thumbnail != null ? "" : _ManagedMod.mod_name,
													Style = ElementStyle.Item,
													SuperTip = sTooltip2,
													Tag = _ManagedMod,
													Image = _ManagedMod.Thumbnail,
													ImageLayoutMode = ImageLayoutMode.OriginalSize
												});
											}
											else
											{
												//Agrega el Componente Root:
												_ManagedMod.IsRootMod = true;
												_ModElement = new AccordionControlElement()
												{
													Expanded = false,
													Name = _ManagedMod.mod_name,
													Text = _ManagedMod.Thumbnail != null ? "" : _ManagedMod.mod_name,
													Style = ElementStyle.Item,
													SuperTip = sTooltip2,
													Tag = _ManagedMod,
													Image = _ManagedMod.Thumbnail,
													ImageLayoutMode = ImageLayoutMode.OriginalSize,
												};
												lst3PMods.Elements.Add(_ModElement);
											}

										}));

										#endregion
									}
								}
							}
						}
					}

					#region Finishes the List Loading

					Invoke((MethodInvoker)(() =>
					{
						Cursor = Cursors.Default;
						lst3PMods.EndUpdate();

						if (TPMods.IsNotEmpty())
						{
							lblStatus.Caption = string.Format("{0} Mods detected.", TPMods.Count);
							SelectedThemeElement = lst3PMods.Elements[0];
							LoadMod(TPMods[0]);
						}
						else
						{
							lblStatus.Caption = "NO mods detected.";
						}
					}));

					#endregion
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void LoadMod(TPMod_Config _Mod)
		{
			try
			{
				if (_Mod != null)
				{
					Cursor = Cursors.WaitCursor;
					vGridDetalles.BeginUpdate();
					vGridDetalles.Rows.Clear();

					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
						string LocalAppData = Environment.GetEnvironmentVariable("LocalAppData");


						#region XMLConfig

						if (_Mod.mod_type == "XMLConfig")
						{
							_Mod.file = _Mod.file.Replace("%LOCALAPPDATA%", LocalAppData);
							_Mod.file = _Mod.file.Replace("%GAME_PATH%", ActiveInstance.path);

							if (File.Exists(_Mod.file))
							{
								ModFullPath = _Mod.file;
								cmdSaveChanges.Caption = "Apply XML Changes";

								//Read the values from an XML file.
								_XmlReader = new XmlDocument();
								_XmlReader.Load(_Mod.file);

								if (_Mod.sections != null && _Mod.sections.Count > 0)
								{
									foreach (var _Section in _Mod.sections)
									{
										if (_Section.keys.IsNotEmpty())
										{
											string[] SectionPath = _Section.ini_section.Split(new char[] { '\\' });

											#region Check the Config File to use

											string ModSectionDir = string.Empty;

											if (!_Section.file_override.EmptyOrNull())
											{
												//If the section has an override file:
												if (_Section.file_override.Substring(0, 2) == "./")
												{
													//Target is in the Root folder
													string RootFolder = System.IO.Path.GetDirectoryName(_Mod.root_folder);
													if (!RootFolder.EmptyOrNull())
													{
														ModSectionDir = Path.Combine(RootFolder, _Section.file_override.Substring(2));
													}
												}
												else
												{
													//Target is in the same folder:
													ModSectionDir = Path.Combine(_Mod.root_folder, _Section.file_override);
												}
											}
											else
											{
												//We use the Mod's file:
												ModSectionDir = Path.Combine(_Mod.root_folder, _Mod.file);
											}

											_XmlReader = File.Exists(ModSectionDir) ? new XmlDocument() : null;
											if (_XmlReader != null)
											{
												_XmlReader.Load(ModSectionDir);
											}

											#endregion

											foreach (var _key in _Section.keys)
											{
												if (_key.visible)
												{
													//Read the value from the Config file:
													_key.value = Util.XML_GetValue(_XmlReader, _Section.ini_section, _key.key, _key.value);
													_key.root_section = _Section;

													EditorRow _Fila = new DevExpress.XtraVerticalGrid.Rows.EditorRow(_key.key);
													_Fila.Properties.ToolTip = _key.description;
													_Fila.Properties.Caption = _key.name;
													_Fila.Properties.FieldName = "value";
													_Fila.Tag = _key;

													#region Controles de Edicion

													//KeyData _KeyData = null;
													if (_key.visible)
													{
														switch (_key.type)
														{
															case "text":
																#region Muestra un TextBox

																RepositoryItemTextEdit _TextItem = new RepositoryItemTextEdit()
																{
																	Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																	EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
																	EditValueChangedDelay = 500,
																	AllowFocused = true,
																	Tag = _key
																};
																_TextItem.EditValueChanged += PropertyGrid_EditValueChanged;
																_TextItem.AppearanceFocused.ForeColor = Color.Orange;

																_Fila.Properties.RowEdit = _TextItem;
																_Fila.Properties.Value = _key.value;

																#endregion
																break;

															case "number":
																#region Muestra un Numeric TextBox (Spin Edit) Integer

																RepositoryItemSpinEdit _NumericItem = new RepositoryItemSpinEdit()
																{
																	Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																	EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
																	EditValueChangedDelay = 500,
																	AllowFocused = true,
																	EditMask = "n0",
																	UseMaskAsDisplayFormat = true,
																	Tag = _key
																};
																_NumericItem.EditValueChanged += PropertyGrid_EditValueChanged;
																_NumericItem.AppearanceFocused.ForeColor = Color.Orange;

																_Fila.Properties.RowEdit = _NumericItem;
																_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0);

																#endregion
																break;

															case "number_dec":
																#region Muestra un Numeric TextBox (Spin Edit) Decimals n2

																RepositoryItemSpinEdit _NumericDecimal = new RepositoryItemSpinEdit()
																{
																	Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																	EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
																	UseMaskAsDisplayFormat = true,
																	EditValueChangedDelay = 500,
																	AllowMouseWheel = true,
																	AllowFocused = true,
																	IsFloatValue = true,
																	Increment = 0.01m,
																	EditMask = "n2",
																	MinValue = -1,
																	MaxValue = 1,
																	Tag = _key
																};
																_NumericDecimal.EditValueChanged += PropertyGrid_EditValueChanged;
																_NumericDecimal.AppearanceFocused.ForeColor = Color.Orange;

																_Fila.Properties.RowEdit = _NumericDecimal;
																_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m);

																#endregion
																break;

															case "decimal": //<- Max = 2.0
																#region Mostrar una TrackBar

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemTrackBar _DecimalItem = new RepositoryItemTrackBar
																	{
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		EditValueChangedDelay = 500,
																		ShowValueToolTip = true,
																		Maximum = 20,
																		Minimum = 0,
																		Tag = _key
																	};
																	_DecimalItem.ValueChanged += PropertyGrid_EditValueChanged;
																	_DecimalItem.EditValueChangedFiringMode =
																			DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;

																	_Fila.Properties.RowEdit = _DecimalItem;
																	_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m) * 10;
																}));

																#endregion
																break;

															case "decimal_1x":
																#region Mostrar una TrackBar

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemTrackBar _DecimalItem = new RepositoryItemTrackBar
																	{
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		EditValueChangedDelay = 500,
																		ShowValueToolTip = true,
																		Maximum = 10,
																		Minimum = 0,
																		Tag = _key
																	};
																	_DecimalItem.ValueChanged += PropertyGrid_EditValueChanged;
																	_DecimalItem.EditValueChangedFiringMode =
																			DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;

																	_Fila.Properties.RowEdit = _DecimalItem;
																	_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m) * 10;
																}));

																#endregion
																break;

															case "decimal_10x":
																#region Mostrar una TrackBar

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemTrackBar _DecimalItem = new RepositoryItemTrackBar
																	{
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		EditValueChangedDelay = 500,
																		ShowValueToolTip = true,
																		Maximum = 100,
																		Minimum = 0,
																		Tag = _key
																	};
																	_DecimalItem.ValueChanged += PropertyGrid_EditValueChanged;
																	_DecimalItem.EditValueChangedFiringMode =
																			DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;

																	_Fila.Properties.RowEdit = _DecimalItem;
																	_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m) * 10;
																}));

																#endregion
																break;

															case "color":
																#region Muestra un selector de Color

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemColorPickEdit _ComboColor = new RepositoryItemColorPickEdit
																	{
																		Tag = _key,
																		ShowCustomColors = true,
																		ShowSystemColors = false,
																		ShowWebSafeColors = true,
																		ShowMoreColorsButton = true,
																		EditValueChangedDelay = 500,
																		AutomaticColor = Color.Orange,
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced,
																		EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered
																	};
																	_ComboColor.EditValueChanged += PropertyGrid_EditValueChanged;
																	_ComboColor.ColorDialogOptions.ShowTabs = ShowTabs.RGBModel;

																	//Lee el Color del INI:
																	//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																	//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

																	int _keyIndex = 0;
																	List<double> _GammaColors = new List<double>();
																	string[] ColorKeys = _key.key.Split(new char[] { '|' });
																	foreach (string _ColorKey in ColorKeys)
																	{
																		//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_ColorKey);
																		_GammaColors.Add(Convert.ToDouble(
																		_IniReader_OLD.ReadKey(_ColorKey, _Section.ini_section)));
																		//(_KeyData != null ? _KeyData.Value : "0")));

																		string _keyRGBA = "R";
																		switch (_keyIndex)
																		{
																			case 0: _keyRGBA = "R"; break;
																			case 1: _keyRGBA = "G"; break;
																			case 2: _keyRGBA = "B"; break;
																			default: _keyRGBA = "A"; break;
																		}
																		_keyIndex++;
																		Console.WriteLine(string.Format("{0};{1} [{2}];{3}", _ColorKey, _key.name, _keyRGBA, _Mod.file));
																	}

																	//convertir de GammaCorrected -> sRGB -> RGB
																	_key.value = ReverseGammaCorrected(_GammaColors).ToArgb().ToString();

																	_Fila.Properties.RowEdit = _ComboColor;
																	_Fila.Properties.Value = ReverseGammaCorrected(_GammaColors).ToArgb();
																}));

																#endregion
																break;

															case "toggle":
																#region Muestra un Toogle Switch:

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemToggleSwitch _ToogleControl = new RepositoryItemToggleSwitch();
																	_ToogleControl.Name = string.Format("{0}|{1}", _Section.ini_section, _key.name);
																	_ToogleControl.EditValueChanged += PropertyGrid_EditValueChanged;
																	_ToogleControl.EditValueChangedDelay = 500;
																	_ToogleControl.Tag = _key;

																	//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																	//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

																	_Fila.Properties.RowEdit = _ToogleControl;
																	_Fila.Properties.Value = Util.IntegerToBool(Util.ValidarNulo(_key.value, 0));
																}));

																#endregion
																break;

															default:
																#region Mostrar un Combo con los Presets

																try
																{
																	if (_Mod.custom_types.IsNotEmpty())
																	{
																		List<TPMod_Type> _Presets = _Mod.custom_types.FindAll(x => x.type == _key.type);
																		if (_Presets.IsNotEmpty())
																		{
																			Invoke((MethodInvoker)(() =>
																			{
																				RepositoryItemLookUpEdit _ComboPreset = new RepositoryItemLookUpEdit()
																				{
																					Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																					DisplayMember = "name",
																					ValueMember = "value",
																					DataSource = _Presets,
																					Tag = _key,
																					AllowFocused = true
																				};
																				var _ColValue = new DevExpress.XtraEditors.Controls.LookUpColumnInfo("value");
																				_ColValue.Visible = false;
																				_ComboPreset.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("name"));
																				_ComboPreset.Columns.Add(_ColValue);
																				_ComboPreset.EditValueChanged += PropertyGrid_EditValueChanged;
																				_ComboPreset.AppearanceFocused.ForeColor = Color.Orange;
																				_ComboPreset.BestFit();

																				//Read the Value From the INI:
																				//if (this._IniData != null)
																				//{
																				//	_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																				//	if (_KeyData != null) _key.value = _KeyData != null ? _KeyData.Value : _key.value;
																				//}

																				_Fila.Properties.RowEdit = _ComboPreset;
																				_Fila.Properties.Value = _key.value;
																			}));
																		}
																	}
																}
																catch { }

																#endregion
																break;
														}
													}

													#endregion

													#region Secciones

													//Agrega la Categoria del Elemento, si ya existe, usa esa, sino, la crea nueva:
													if (!_Section.title.EmptyOrNull())
													{
														Invoke((MethodInvoker)(() =>
														{
															if (!(vGridDetalles.Rows[_Section.name] is CategoryRow Categoria))
															{
																Categoria = new CategoryRow(_Section.title)
																{
																	Name = _Section.name,
																	Tag = _Section
																};
																vGridDetalles.Rows.Add(Categoria);

															}
															Categoria.ChildRows.Add(_Fila);
														}));
													}
													else
													{
														Invoke((MethodInvoker)(() =>
														{
															vGridDetalles.Rows.Add(_Fila);
														}));
													}

													#endregion
												}
											}
										}
									}
								}
							}
						}

						#endregion

						#region INIConfig

						if (_Mod.mod_type == "INIConfig")
						{
							/* READS THE VALUES FROM THE INI FILES */
							ModFullPath = Path.Combine(_Mod.root_folder, _Mod.file);

							if (File.Exists(ModFullPath))
							{
								_IniReader_OLD = new IniFile(ModFullPath);
								if (_IniReader_OLD != null)
								{
									cmdSaveChanges.Caption = "Close";

									if (_Mod.sections.IsNotEmpty())
									{
										foreach (var _Section in _Mod.sections)
										{
											#region Check the Config File to use

											string ModSectionDir = string.Empty;
											if (!_Section.file_override.EmptyOrNull())
											{
												//If the section has an override file:
												if (_Section.file_override.Substring(0, 2) == "./")
												{
													//Target is in the Root folder
													string RootFolder = System.IO.Path.GetDirectoryName(_Mod.root_folder);
													if (!RootFolder.EmptyOrNull())
													{
														ModSectionDir = Path.Combine(RootFolder, _Section.file_override.Substring(2));
													}
												}
												else
												{
													//Target is in the same folder:
													ModSectionDir = Path.Combine(_Mod.root_folder, _Section.file_override);
												}
											}
											else
											{
												//We use the Mod's file:
												ModSectionDir = Path.Combine(_Mod.root_folder, _Mod.file);
											}

											_IniReader_OLD = File.Exists(ModSectionDir) ? new IniFile(ModSectionDir) : null;

											#endregion

											if (_IniReader_OLD != null && _Section.keys.IsNotEmpty())
											{
												foreach (var _key in _Section.keys)
												{
													if (_key.visible)
													{
														//Reads the Value from the Ini File:					
														_key.value = _IniReader_OLD.ReadKey(_key.key, _Section.ini_section);
														_key.root_section = _Section;

														#region Fila de Datos

														EditorRow _Fila = new DevExpress.XtraVerticalGrid.Rows.EditorRow(_key.key);
														_Fila.Properties.ToolTip = _key.description;
														_Fila.Properties.Caption = _key.name;
														_Fila.Properties.FieldName = "value";
														_Fila.Tag = _key;

														#endregion

														#region Controles de Edicion

														//KeyData _KeyData = null;
														switch (_key.type)
														{
															case "text":
																#region Muestra un TextBox

																RepositoryItemTextEdit _TextItem = new RepositoryItemTextEdit()
																{
																	Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																	EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
																	EditValueChangedDelay = 500,
																	AllowFocused = true,
																	Tag = _key
																};
																_TextItem.EditValueChanged += PropertyGrid_EditValueChanged;
																_TextItem.AppearanceFocused.ForeColor = Color.Orange;

																_Fila.Properties.RowEdit = _TextItem;
																_Fila.Properties.Value = _key.value;

																#endregion
																break;

															case "number":
																#region Muestra un Numeric TextBox (Spin Edit)

																RepositoryItemSpinEdit _NumericItem = new RepositoryItemSpinEdit()
																{
																	Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																	EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
																	EditValueChangedDelay = 500,
																	EditMask = "n0",
																	UseMaskAsDisplayFormat = true,
																	AllowFocused = true,
																	Tag = _key
																};
																_NumericItem.EditValueChanged += PropertyGrid_EditValueChanged;
																_NumericItem.AppearanceFocused.ForeColor = Color.Orange;

																_Fila.Properties.RowEdit = _NumericItem;
																_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0);

																#endregion
																break;

															case "number_dec":
																#region Muestra un Numeric TextBox (Spin Edit)

																RepositoryItemSpinEdit _NumericDecimal = new RepositoryItemSpinEdit()
																{
																	Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																	EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered,
																	UseMaskAsDisplayFormat = true,
																	EditValueChangedDelay = 500,
																	AllowMouseWheel = true,
																	AllowFocused = true,
																	IsFloatValue = true,
																	Increment = 0.01m,
																	EditMask = "n2",
																	MinValue = -1,
																	MaxValue = 1,
																	Tag = _key
																};
																_NumericDecimal.EditValueChanged += PropertyGrid_EditValueChanged;
																_NumericDecimal.AppearanceFocused.ForeColor = Color.Orange;

																_Fila.Properties.RowEdit = _NumericDecimal;
																_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m);

																#endregion
																break;

															case "decimal": //<- Max = 2.0
																#region Mostrar una TrackBar

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemTrackBar _DecimalItem = new RepositoryItemTrackBar
																	{
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		EditValueChangedDelay = 500,
																		ShowValueToolTip = true,
																		Maximum = 20,
																		Minimum = 0,
																		Tag = _key
																	};
																	_DecimalItem.ValueChanged += PropertyGrid_EditValueChanged;
																	_DecimalItem.EditValueChangedFiringMode =
																			DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;

																	_Fila.Properties.RowEdit = _DecimalItem;
																	_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m) * 10;
																}));

																#endregion
																break;

															case "decimal_1x":
																#region Mostrar una TrackBar

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemTrackBar _DecimalItem = new RepositoryItemTrackBar
																	{
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		EditValueChangedDelay = 500,
																		ShowValueToolTip = true,
																		Maximum = 10,
																		Minimum = 0,
																		Tag = _key
																	};
																	_DecimalItem.ValueChanged += PropertyGrid_EditValueChanged;
																	_DecimalItem.EditValueChangedFiringMode =
																			DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;

																	_Fila.Properties.RowEdit = _DecimalItem;
																	_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m) * 10;
																}));

																#endregion
																break;

															case "decimal_10x":
																#region Mostrar una TrackBar

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemTrackBar _DecimalItem = new RepositoryItemTrackBar
																	{
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		EditValueChangedDelay = 500,
																		ShowValueToolTip = true,
																		Maximum = 100,
																		Minimum = 0,
																		Tag = _key
																	};
																	_DecimalItem.ValueChanged += PropertyGrid_EditValueChanged;
																	_DecimalItem.EditValueChangedFiringMode =
																			DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;

																	_Fila.Properties.RowEdit = _DecimalItem;
																	_Fila.Properties.Value = Util.ValidarNulo(_key.value, 0.0m) * 10;
																}));

																#endregion
																break;

															case "color":
																#region Muestra un selector de Color

																Invoke((MethodInvoker)(() =>
																{
																	/*
																	RepositoryItemColorPickEdit _ComboColor = new RepositoryItemColorPickEdit
																	{
																		Tag = _key,
																		ShowCustomColors = true,
																		ShowSystemColors = false,
																		ShowWebSafeColors = true,
																		ShowMoreColorsButton = true,
																		EditValueChangedDelay = 500,
																		AutomaticColor = Color.Orange,
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced,
																		EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered
																	};
																	_ComboColor.EditValueChanged += PropertyGrid_EditValueChanged;
																	_ComboColor.ColorDialogOptions.ShowTabs = ShowTabs.RGBModel;*/

																	RepositoryItemMyColorPickEdit _ComboColor = new RepositoryItemMyColorPickEdit
																	{
																		Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																		ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Simple,
																		AutomaticColor = Color.Orange,
																		ShowMoreColorsButton = true,
																		ShowColorDialog = true,
																		ShowWebColors = false,
																		ShowCustomColors = true,
																		ShowSystemColors = false,
																		ShowWebSafeColors = false,
																		ShowMyCustomColors = true,
																		ShowMyPastelColors = false,
																		Tag = _key,
																	};

																	List<Color> _GColors = null;
																	var _StandardColors = _ComboColor.MyStandardColors;

																	_ComboColor.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
																	_ComboColor.EditValueChangedDelay = 500;
																	_ComboColor.ColorDialogOptions.ShowTabs = ShowTabs.RGBModel;
																	_ComboColor.ColorDialogOptions.AllowTransparency = true;
																	_ComboColor.ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced;
																	//_ComboColor.EditValueChanged += PropertyGrid_EditValueChanged;
																	_ComboColor.EditValueChanged += (object Sender, EventArgs E) =>
																	{
																		//Crea un Gradiente a Blanco usando el color seleccionado, lo pone en la ultima columna
																		Color _E = (Sender as Util_Test.MyColorPickEdit).Color;
																		var _CustomColors = _ComboColor.MyStandardColors;

																		_GColors = Util.GetColorGradients(_E, Color.Black, 7).ToList();

																		_CustomColors[0, 9] =  _GColors[0];
																		_CustomColors[0, 19] = _GColors[1];
																		_CustomColors[0, 29] = _GColors[2];
																		_CustomColors[0, 39] = _GColors[3];
																		_CustomColors[0, 49] = _GColors[4];
																		_CustomColors[0, 59] = _GColors[5];

																		_GColors = Util.GetColorGradients(_E, Color.White, 7).ToList();

																		_CustomColors[0, 8] =  _GColors[0];
																		_CustomColors[0, 18] = _GColors[1];
																		_CustomColors[0, 28] = _GColors[2];
																		_CustomColors[0, 38] = _GColors[3];
																		_CustomColors[0, 48] = _GColors[4];
																		_CustomColors[0, 58] = _GColors[5];

																		_ComboColor.AutomaticColor = _E;

																		PropertyGrid_EditValueChanged(Sender, E);
																	};

																	//Lee el Color del INI:
																	//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																	//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

																	int _keyIndex = 0;
																	List<double> _GammaColors = new List<double>();
																	string[] ColorKeys = _key.key.Split(new char[] { '|' });
																	foreach (string _ColorKey in ColorKeys)
																	{
																		//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_ColorKey);
																		_GammaColors.Add(Convert.ToDouble(
																	_IniReader_OLD.ReadKey(_ColorKey, _Section.ini_section)));
																		//(_KeyData != null ? _KeyData.Value : "0")));

																		string _keyRGBA = "R";
																		switch (_keyIndex)
																		{
																			case 0: _keyRGBA = "R"; break;
																			case 1: _keyRGBA = "G"; break;
																			case 2: _keyRGBA = "B"; break;
																			default: _keyRGBA = "A"; break;
																		}
																		_keyIndex++;
																		Console.WriteLine(string.Format("{0};{1} [{2}];{3}", _ColorKey, _key.name, _keyRGBA, _Mod.file));
																	}

																	//convertir de GammaCorrected -> sRGB -> RGB
																	_key.value = ReverseGammaCorrected(_GammaColors).ToArgb().ToString();

																	_Fila.Properties.RowEdit = _ComboColor;
																	_Fila.Properties.Value = ReverseGammaCorrected(_GammaColors).ToArgb();
																}));

																#endregion
																break;

															case "toggle":
																#region Muestra un Toogle Switch:

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemToggleSwitch _ToogleControl = new RepositoryItemToggleSwitch();
																	_ToogleControl.Name = string.Format("{0}|{1}", _Section.ini_section, _key.name);
																	_ToogleControl.EditValueChanged += PropertyGrid_EditValueChanged;
																	_ToogleControl.EditValueChangedDelay = 500;
																	_ToogleControl.Tag = _key;

																	//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																	//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

																	_Fila.Properties.RowEdit = _ToogleControl;
																	_Fila.Properties.Value = Util.IntegerToBool(Util.ValidarNulo(_key.value, 0));
																}));

																#endregion
																break;

															default:
																#region Mostrar un Combo con los Presets

																try
																{
																	if (_Mod.custom_types.IsNotEmpty())
																	{
																		List<TPMod_Type> _Presets = _Mod.custom_types.FindAll(x => x.type == _key.type);
																		if (_Presets.IsNotEmpty())
																		{
																			Invoke((MethodInvoker)(() =>
																			{
																				RepositoryItemLookUpEdit _ComboPreset = new RepositoryItemLookUpEdit()
																				{
																					Name = string.Format("{0}|{1}", _Section.ini_section, _key.name),
																					DisplayMember = "name",
																					ValueMember = "value",
																					DataSource = _Presets,
																					Tag = _key,
																					AllowFocused = true
																				};
																				var _ColValue = new DevExpress.XtraEditors.Controls.LookUpColumnInfo("value");
																				_ColValue.Visible = false;
																				_ComboPreset.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("name"));
																				_ComboPreset.Columns.Add(_ColValue);
																				_ComboPreset.EditValueChanged += PropertyGrid_EditValueChanged;
																				_ComboPreset.AppearanceFocused.ForeColor = Color.Orange;
																				_ComboPreset.BestFit();

																				//Read the Value From the INI:
																				//if (this._IniData != null)
																				//{
																				//	_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																				//	if (_KeyData != null) _key.value = _KeyData != null ? _KeyData.Value : _key.value;
																				//}

																				_Fila.Properties.RowEdit = _ComboPreset;
																				_Fila.Properties.Value = _key.value;
																			}));
																		}
																	}
																}
																catch { }

																#endregion
																break;
														}

														#endregion

														#region Secciones

														//Agrega la Categoria del Elemento, si ya existe, usa esa, sino, la crea nueva:
														if (!_Section.title.EmptyOrNull())
														{
															Invoke((MethodInvoker)(() =>
															{
																if (!(vGridDetalles.Rows[_Section.name] is CategoryRow Categoria))
																{
																	Categoria = new CategoryRow(_Section.title)
																	{
																		Name = _Section.name,
																		Tag = _Section
																	};
																	vGridDetalles.Rows.Add(Categoria);

																}
																Categoria.ChildRows.Add(_Fila);
															}));
														}
														else
														{
															Invoke((MethodInvoker)(() =>
															{
																vGridDetalles.Rows.Add(_Fila);
															}));
														}

														#endregion
													}
												}
											}
										}
									}
								}
							}
						}

						#endregion

						#region Themes

						/** CHECK IF THE 3PMOD HAS THEMES AND SHOW THEM IN THE SIDE LIST  */
						List<ui_preset_new> UI_Themes = null;
						string ThemesFolder = Path.Combine(UI_DOCUMENTS, "ODYSS", "3PMods", Path.GetFileNameWithoutExtension(_Mod.file));
						if (Directory.Exists(ThemesFolder))
						{
							List<string> directories = new List<string>(Directory.GetDirectories(ThemesFolder));
							if (directories != null && directories.Count > 0)
							{
								UI_Themes = new List<ui_preset_new>();
								foreach (string _Folder in directories)
								{
									//Buscar el Archivo que identifica al Autor del Tema:	
									theme_details ThemeCredits = null;
									string CreditsFile = new DirectoryInfo(_Folder).GetFiles("*.credits").Select(fi => fi.Name).FirstOrDefault().NVL("Unknown.credits");
									if (!CreditsFile.EmptyOrNull())
									{
										try
										{
											ThemeCredits = Util.DeSerialize_FromJSON<theme_details>(Path.Combine(_Folder, CreditsFile));
										}
										catch { }
									}

									string RootFolder = System.IO.Path.GetFileNameWithoutExtension(_Folder);
									var Theme = new DirectoryInfo(_Folder).GetFiles("*.json");
									if (Theme != null && Theme.Length > 0)
									{
										var ThemeDetails = Util.DeSerialize_FromJSON<TPMod_Config>(Theme[0].FullName);
										ui_preset_new _theme = new ui_preset_new(Util.NVL(ThemeDetails.theme_name, RootFolder), Theme[0].FullName)
										{
											author = ThemeDetails.author,
											description = ThemeDetails.description
										};

										//Buscar el archivo del Thumbnail:
										if (File.Exists(Path.Combine(_Folder, System.IO.Path.GetFileNameWithoutExtension(ThemeDetails.file) + ".png")))
										{
											//Carga la Imagen sin dejara 'en uso':
											using (Stream stream = File.OpenRead(Path.Combine(_Folder, Path.GetFileNameWithoutExtension(ThemeDetails.file) + ".png")))
											{
												_theme.Preview = System.Drawing.Image.FromStream(stream);
												_theme.HasPreview = true;
											}
										}
										else
										{
											//sI EL TEMA NO TIENE PREVIEW, USA UNA IMAGEN X DEFECTO;
											using (Stream stream = File.OpenRead(Path.Combine(AppExePath, "Images", "PREVIEW_DEFAULT.PNG")))
											{
												_theme.Preview = System.Drawing.Image.FromStream(stream);
												_theme.HasPreview = false;
											}
										}

										//Escribe el Nombre del Mod Sobre la Imagen Thumbnail:
										if (_theme.Preview != null)
										{
											Bitmap bm = new Bitmap(_theme.Preview.Width, _theme.Preview.Height);
											using (Graphics e = Graphics.FromImage(bm))
											{
												e.DrawImage(_theme.Preview, new Rectangle(new Point(0, 0), new Size(_theme.Preview.Width, _theme.Preview.Height)));
												e.DrawString(Util.NVL(_theme.name, RootFolder), new System.Drawing.Font("Tahoma", 8, FontStyle.Bold),
														Brushes.White, 10, _theme.Preview.Height - 20);
											}
											_theme.Preview = bm;
										}

										UI_Themes.Add(_theme);
									}
								}

							}
						}

						#endregion

						Invoke((MethodInvoker)(() =>
						{
							try
							{
								vGridDetalles.EndUpdate();

								if (UI_Themes != null && UI_Themes.Count > 0)
								{
									gridThemes.DataSource = UI_Themes;
									if (dock3PM_Themes.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
									{
										dock3PM_Themes.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
									}
								}
								else
								{
									gridThemes.DataSource = null;
								}

								if (!_Mod.read_me.EmptyOrNull())
								{
									cmdReadMe.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
								}
								else
								{
									cmdReadMe.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
								}

								LoadElementPreviews(_Mod);

								lblMODdescription.Text = string.Format("<b>{0}</b>\r\n{1}",
									string.Format("<b>{0}</b> ( {1} )", _Mod.mod_name, _Mod.managed ? "<color=0, 255, 0>Managed</color>" : "<color=255, 0, 0>Unmanaged</color>"),
									string.Format("           By {0}\r\n\r\nDescription:\r\n<i>{1}</i>", _Mod.author, _Mod.description));

								CurrentdMod = _Mod;
							}
							catch (Exception)
							{
								throw;
							}
							finally
							{
								Cursor = Cursors.Default;
								//dock3PM_Themes.HideSliding();
							}
						}));
					});
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void SaveXMLChanges(bool Silent = false)
		{
			try
			{
				if (CurrentdMod != null && CurrentdMod.mod_type == "XMLConfig")
				{
					var GridSections = vGridDetalles.Rows;

					foreach (var ModSection in CurrentdMod.sections)
					{
						foreach (var ModKey in ModSection.keys)
						{
							foreach (var _Section in GridSections)
							{
								if (_Section.Name == ModSection.name)
								{
									#region Check the Config File to use

									string ModSectionDir = string.Empty;

									if (!ModSection.file_override.EmptyOrNull())
									{
										//If the section has an override file:
										if (ModSection.file_override.Substring(0, 2) == "./")
										{
											//Target is in the Root folder
											string RootFolder = System.IO.Path.GetDirectoryName(CurrentdMod.root_folder);
											if (!RootFolder.EmptyOrNull())
											{
												ModSectionDir = Path.Combine(RootFolder, ModSection.file_override.Substring(2));
											}
										}
										else
										{
											//Target is in the same folder:
											ModSectionDir = Path.Combine(CurrentdMod.root_folder, ModSection.file_override);
										}
									}
									else
									{
										//We use the Mod's file:
										ModSectionDir = Path.Combine(CurrentdMod.root_folder, CurrentdMod.file);
									}

									#endregion

									#region Set Key Values

									foreach (var _Key in _Section.ChildRows)
									{
										if (_Key.Tag is TPMod_Key _Element)
										{
											if (_Element.key == ModKey.key)
											{
												ModKey.value = _Element.value;
											}
										}
									}
									#endregion

									#region Save XML

									_XmlReader = File.Exists(ModSectionDir) ? new XmlDocument() : null;
									if (_XmlReader != null)
									{
										_XmlReader.Load(ModSectionDir);
										_XmlReader.XML_SetValue(ModSection.ini_section + ModKey.key, ModKey.value);
										_XmlReader.XML_SaveBeautify(ModSectionDir);
									}

									#endregion
								}
							}
						}
					}

					if (!Silent)
					{
						MessageBox.Show("Changes in the XML Configuration had been saved!\r\nYou will need to re-start the game",
							"ERROR", MessageBoxButtons.OK, MessageBoxIcon.Information);
						_XmlReader = null;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void SaveMod(TPMod_Config _Mod)
		{
			try
			{
				string FileName = Path.GetFileNameWithoutExtension(_Mod.file); //<- Nombre sin Extension ni Path
				string ModFullPath = Path.Combine(_Mod.file_full, FileName + ".json");
				if (_Mod.managed)
				{
					Util.Serialize_ToJSON(ModFullPath, _Mod); //<- Crea el JSON
				}
				else
				{
					if (XtraMessageBox.Show(string.Format("The mod '{0}' is Un-managed and has no JSON file.\r\nWould you want to create one now?", FileName),
						"Manage Mod?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						Util.Serialize_ToJSON(ModFullPath, _Mod); //<- Crea el JSON

						if (File.Exists(Path.Combine(TPMods_Path, FileName + ".png")) == false)
						{
							//Ahora Creamos una Imagen que sirva de Thumbnail:  
							//Formato PNG, 200x61 pix, Fondo Gris semi-transparente, Borde Naranja
							Bitmap ThumbNail = new Bitmap(200, 61);
							using (Graphics g = Graphics.FromImage(ThumbNail))
							{
								Rectangle _Box = new Rectangle(0, 0, 200, 61);
								SolidBrush Background = new SolidBrush(Color.FromArgb(50, Color.Gray));

								g.FillRectangle(Background, _Box); //<- Dibuja el Fondo

								_Box.Width--; _Box.Height--; //Dibuja el Borde:
								using (Pen pen = new Pen(Color.OrangeRed, 1))
								{
									g.DrawRectangle(pen, _Box);
								}
							}
							ThumbNail.Save(Path.Combine(TPMods_Path, FileName + ".png"), System.Drawing.Imaging.ImageFormat.Png);
						}

						LoadModList(); //<- Refresh Mod List
					}
				}
				//XtraMessageBox.Show("All Changes Saved!");
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void ImportMod()
		{
			try
			{
				OpenFileDialog OFDialog = new OpenFileDialog()
				{
					Filter = "ZIP Files|*.zip",
					FilterIndex = 0,
					DefaultExt = "zip",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = true,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};

				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					/* ZIP Estructure:					 
					   ├── [AUTHOR_NAME].credits
					   ├── ShaderFixes							<- [Optional]
					   |	└── [Optional Shader Files]			
					   ├── EDHM-ini
					   |	└── 3rdPartyMods					<- Files can either be in the root or in a subfolder
					   |		└── [MOD_NAME]					<- ie: 'CockpitPaintMod'
					   |				├── [MOD_NAME].ini		
					   |				├── [MOD_NAME].json
					   |				├── [MOD_NAME].bat		<- [Optional]
					   |				└── [MOD_NAME].png	
					   |				....
					   |				├──[SUB_MOD_NAME].ini	<- [Optional] Sub-Mods, ie: 'CPM-Anaconda', 'CPM-Courier', etc.
					   |				├──[SUB_MOD_NAME].json
					   |				└──[SUB_MOD_NAME].png
					   └── Themes								<- [Optional]
							├──	[AUTHOR_NAME].credits
					   		├──	[MOD_NAME]						<- CPM-Anaconda
							|		├── [THEME_NAME]			<- ie: '@Elite Default'
							|		|		├── [MOD_NAME].ini	<- CPM-Anaconda.ini
							|		|		├── [MOD_NAME].json
							|		|		└── [MOD_NAME].png
							|		└── [THEME_NAME]			<- Another Theme (for Same Ship) ie: 'JumpaConda Black'
							|				├── [MOD_NAME].ini
							|				├── [MOD_NAME].json
							|				└── [MOD_NAME].png 
							├──	[MOD_NAME]						<- Another Theme (for Different ship) ie: 'CPM-Courier'		
							|		└── [THEME_NAME]			
							|				├── [MOD_NAME].ini
							|				├── [MOD_NAME].json
							|				└── [MOD_NAME].png
					 */

					string ThemesFolder = Path.Combine(UI_DOCUMENTS, "ODYSS", "3PMods");
					string ZIP_NAME = Path.GetFileNameWithoutExtension(OFDialog.FileName);
					string TempPath = Path.Combine(Path.GetTempPath(), "EDHM_UI", ZIP_NAME);

					//2. Crear una Carpeta Temporal para los Archivos del Tema:
					if (Directory.Exists(TempPath))
					{
						Directory.Delete(TempPath, true);
					}

					Directory.CreateDirectory(TempPath);
					if (Directory.Exists(TempPath))  //<- %Temp%\EDHM_UI\%ZIP_NAME%
					{
						Util.DoNetZIP_UnCompressFile(OFDialog.FileName, TempPath);

						CheckPreExisting(TempPath);						

						//Copia los archivos del MOD:
						if (Directory.Exists(Path.Combine(TempPath, "EDHM-ini")))
						{
							Util.CopyDirectory(
								new DirectoryInfo(Path.Combine(TempPath, "EDHM-ini")),
								new DirectoryInfo(Path.Combine(ActiveInstance.path, "EDHM-ini")));

							if (Directory.Exists(Path.Combine(TempPath, "ShaderFixes")))
							{
								Util.CopyDirectory(
									new DirectoryInfo(Path.Combine(TempPath, "ShaderFixes")),
									new DirectoryInfo(Path.Combine(ActiveInstance.path, "ShaderFixes")));
							}

							if (Directory.Exists(Path.Combine(TempPath, "Themes")))
							{
								//Hay temas para importar
								CopyThemeFiles(Path.Combine(TempPath, "Themes"));
							}

							XtraMessageBox.Show("Mod Imported.", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
							LoadModList();
						}
						else
						{
							MessageBox.Show("This doesn't look like a 3PMod file.\r\nIs it a theme?\r\nTry with the 'Themes' combo button.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}						
					}
					
					Cursor = Cursors.Default;
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private bool CheckPreExisting(string TempModPath)
		{
			/* REVISA LA PRE-EXISTENCIA DEL MOD Y LO DES-INSTALA */

			bool _ret = false;
			try
			{
				DirectoryInfo ModMainDirectory = null;

				if (!Directory.Exists(Path.Combine(TempModPath, "EDHM-ini")))
				{
					//This doesn't look like a 3PMod file
				}
				else
				{
					var TempModDirectory = new DirectoryInfo(Path.Combine(TempModPath, "EDHM-ini", "3rdPartyMods"));
					var SubDirectories = TempModDirectory.GetDirectories();
					if (SubDirectories != null && SubDirectories.Length > 0)
					{
						ModMainDirectory = SubDirectories[0]; //<- The first Directory in there gotta be the one
					}
					else
					{
						//No sub-directories, Root folder is the one
						ModMainDirectory = TempModDirectory;
					}

					if (ModMainDirectory != null)
					{
						string ModName = ModMainDirectory.Name;
						var ModConfigFiles = ModMainDirectory.GetFiles("*.json");
						if (ModConfigFiles != null && ModConfigFiles.Length > 0)
						{
							var ModConfigFile = ModConfigFiles[0];
							if (ModConfigFile != null)
							{
								TPMod_Config _ManagedMod = Util.DeSerialize_FromJSON<TPMod_Config>(ModConfigFile.FullName);
								if (_ManagedMod != null)
								{
									UninstallMod(_ManagedMod, true);
									_ret = true;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		private void UninstallMod(TPMod_Config _ManagedMod, bool Silent = false)
		{
			try
			{
				if (_ManagedMod != null)
				{
					bool Continuar = Silent;

					if (!Silent)
					{
						if (XtraMessageBox.Show(string.Format("Are you sure of Deleting the MOD: '{0}'?", _ManagedMod.mod_name),
						"Confirm Deletion?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{ Continuar = true; }
					}

					if (Continuar)
					{
						if (!_ManagedMod.file_full.EmptyOrNull() && Directory.Exists(_ManagedMod.root_folder))
						{
							//Remove Mod's main files:
							var dir = new DirectoryInfo(_ManagedMod.root_folder);
							foreach (var file in dir.EnumerateFiles(string.Format("{0}.*", Path.GetFileNameWithoutExtension(_ManagedMod.file))))
							{
								file.Delete();
							}
							if (_ManagedMod.dependencies.IsNotEmpty())
							{
								//Remove Mod dependencies:
								foreach (string _Dependency in _ManagedMod.dependencies)
								{
									if (_Dependency.Contains("%WIN_REGISTRY%"))
									{
										//TODO
									}
									else
									{
										string _File = _Dependency.Replace("ShaderFixes", Path.Combine(ActiveInstance.path, "ShaderFixes"));
										_File = _File.Replace("%USER_DOCUMENTS%", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
										_File = _File.Replace("%EDUI_DOCUMENTS%", UI_DOCUMENTS);
										_File = _File.Replace("%ACTIVE_GAME%", ActiveInstance.path);

										string file_name = System.IO.Path.GetFileName(_File);
										if (file_name.Contains("*"))
										{
											//Borra todos los Directorios y Archivos que coincidan con el patron
											string Criteria = Path.GetFileName(_File);
											var rootdir = new DirectoryInfo(Path.GetDirectoryName(_File));

											foreach (DirectoryInfo subDir in rootdir.GetDirectories(Criteria))
											{
												subDir.Delete(true);
											}

											foreach (FileInfo file in rootdir.EnumerateFiles(Criteria))
											{
												file.Delete();
											}
										}
										else
										{
											if (File.Exists(_File)) File.Delete(_File);
										}
									}
								}
							}

							string RootFolder = System.IO.Path.GetFileNameWithoutExtension(_ManagedMod.root_folder);
							if (_ManagedMod.IsRootMod && RootFolder != "3rdPartyMods" && RootFolder != "EDHM-ini")
							{
								System.GC.Collect();
								System.GC.WaitForPendingFinalizers();
								try
								{
									Directory.Delete(_ManagedMod.root_folder, true);
								}
								catch { }
							}

							if (!Silent)
							{
								LoadModList(); //<- Actualizar los cambios
							}
						}
					}
				}
			}
			catch (IOException ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void ExportTheme(TPMod_Config _Mod)
		{
			try
			{
				//1. Preguntar donde se Guarda el ZIP:
				SaveFileDialog XSFD = new SaveFileDialog()
				{
					Filter = "ZIP file|*.zip",
					FilterIndex = 0,
					DefaultExt = "zip",
					AddExtension = true,
					CheckPathExists = true,
					OverwritePrompt = true,
					FileName = _Mod.theme_name,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};

				if (XSFD.ShowDialog() == DialogResult.OK)
				{
					Cursor = Cursors.WaitCursor;

					string ModPath = _Mod.root_folder;
					string ModName = Path.GetFileNameWithoutExtension(_Mod.file_full);
					string RootFolder = Path.GetFileNameWithoutExtension(_Mod.file_full);

					string ThemeName = _Mod.theme_name;
					string TempPath = Path.Combine(Path.GetTempPath(), "EDHM_UI", ThemeName);

					//2. Crear una Carpeta Temporal para los Archivos del Tema:
					if (Directory.Exists(TempPath))
						Directory.Delete(TempPath, true);

					Directory.CreateDirectory(TempPath);

					if (Directory.Exists(TempPath))
					{
						/*  Theme ZIP estructure:
						[MOD_NAME]						<- CPM-Anaconda
							├── [THEME_NAME]			<- @Elite Default
							|		├── [MOD_NAME].json <- CPM-Anaconda.json
							|		└── [MOD_NAME].png
							├── [THEME_NAME]
							|		├── [MOD_NAME].json
							|		└── [MOD_NAME].png
							└── [AUTHOR_NAME].credits
						 */

						//3. Crea la Estructura de Directorios necesaria: %TEMP_FOLDER%\%MOD_NAME%\%ThemeName%    
						string ThemeFolder = Path.Combine(TempPath, ModName, ThemeName);
						Directory.CreateDirectory(ThemeFolder);

						if (Directory.Exists(ThemeFolder))
						{
							//Refresh the changes in the JSON:
							//Util.Serialize_ToJSON(ModPath, _Mod, true);

							//4. Copiar los Archivos del MOD a la carpeta Temporal:
							var allowedExtensions = new[] { ".json", ".png" };
							var FileList = Directory
								.GetFiles(ModPath, ModName + "*.*")
								.Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
								.ToList();
							if (FileList != null && FileList.Count > 0)
							{
								foreach (string _file in FileList)
								{
									File.Copy(_file,
										Path.Combine(ThemeFolder, Path.GetFileName(_file)),
										true
									);
								}
							}

							//5. Agregar el Identificador del Autor en el Root del ZIP:
							theme_details ThemeDetails = new theme_details
							{
								author = _Mod.author,
								theme = _Mod.theme_name,
								description = string.Format("** THIS THEME WAS MADE BY {0} **", _Mod.author.ToUpper()),
								preview = string.Empty
							};
							Util.Serialize_ToJSON(Path.Combine(TempPath, string.Format("{0}.credits", _Mod.author)), ThemeDetails, true);

							//6. Comprimir la Carpeta Temporal:
							Util.DoNetZIP_CompressFolder(TempPath, XSFD.FileName, false);

							if (File.Exists(XSFD.FileName))
							{
								Directory.Delete(TempPath, true); //<- Borra la Carpeta Temporal

								XtraMessageBox.Show(string.Format("The theme '{0}' had been Exported!", ThemeName), "Success!",
											MessageBoxButtons.OK, MessageBoxIcon.Information);

								//6. Muestra el ZIP creado en el Explorador de Windows:
								string argument = string.Format("/select, \"{0}\"", XSFD.FileName);
								System.Diagnostics.Process.Start("explorer.exe", argument);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { Cursor = Cursors.Default; }
		}
		private void ExportTheme()
		{
			try
			{
				if (CurrentdMod != null)
				{
					string ThemePath = Path.Combine(CurrentdMod.root_folder, Path.GetFileNameWithoutExtension(CurrentdMod.file_full));
					Image _Thumbnail = null;

					if (File.Exists(ThemePath + ".png"))
					{
						//Carga la Imagen sin dejara 'en uso':
						using (Stream stream = File.OpenRead(ThemePath + ".png"))
						{
							_Thumbnail = System.Drawing.Image.FromStream(stream);
						}
					}

					ThemeParametersForm _Form = new ThemeParametersForm
					{
						ThisIsAMod = true,
						ModName = CurrentdMod.mod_name,
						ThemeName = CurrentdMod.theme_name,
						Author = CurrentdMod.author,
						Description = CurrentdMod.description,
						Thumbnail = _Thumbnail
					};
					if (_Form.ShowDialog() == DialogResult.OK)
					{
						CurrentdMod.theme_name = _Form.ThemeName;
						CurrentdMod.author = _Form.Author;
						CurrentdMod.description = _Form.Description;
						CurrentdMod.Thumbnail = _Form.Thumbnail;

						if (CurrentdMod.Thumbnail != null)
						{
							//Save the Image as PNG:
							CurrentdMod.Thumbnail.Save(ThemePath + ".png",
								System.Drawing.Imaging.ImageFormat.Png);
						}

						//Save the changes in the JSON:
						Util.Serialize_ToJSON(ThemePath + ".json", CurrentdMod, true);

						ExportTheme(CurrentdMod);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ImportTheme()
		{
			try
			{
				OpenFileDialog OFDialog = new OpenFileDialog()
				{
					Filter = "ZIP Files|*.zip",
					FilterIndex = 0,
					DefaultExt = "zip",
					AddExtension = true,
					CheckPathExists = true,
					CheckFileExists = true,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				};
				//1. Ask for the ZIP to Import
				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					/*  Theme ZIP estructure:
						├──	[AUTHOR_NAME].credits
						├──	[MOD_NAME]						<- CPM-Anaconda
						|		├── [THEME_NAME]			<- @Elite Default
						|		|		├── [MOD_NAME].json  <- CPM-Anaconda.json
						|		|		└── [MOD_NAME].png
						|		└── [THEME_NAME]			<- Another Theme (for Same Ship) ie: 'JumpaConda Black'
						|				├── [MOD_NAME].json
						|				└── [MOD_NAME].png 
						├──	[MOD_NAME]						<- Another Theme (for Different ship) ie: 'CPM-Courier'		
						|		└── [THEME_NAME]			
						|				├── [MOD_NAME].json
						|				└── [MOD_NAME].png
					 */

					string ThemesFolder = Path.Combine(UI_DOCUMENTS, "ODYSS", "3PMods");
					string ThemeName = Path.GetFileNameWithoutExtension(OFDialog.FileName);
					string TempPath = Path.Combine(Path.GetTempPath(), "EDHM_UI", ThemeName);
					int[] _ret = new int[] { 0, 0 };

					//2. Crear una Carpeta Temporal para los Archivos del Tema:
					if (Directory.Exists(TempPath))
					{
						Directory.Delete(TempPath, true);
					}

					Directory.CreateDirectory(TempPath);
					if (Directory.Exists(TempPath))
					{
						Util.DoNetZIP_UnCompressFile(OFDialog.FileName, TempPath);
						var _choice = MessageBox.Show("Would you like to Apply the Imported Theme(s)?", "Apply Themes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						_ret = CopyThemeFiles(TempPath, (_choice == DialogResult.Yes ? true : false));
					}

					// Borrar la Carpeta Temporal
					if (Directory.Exists(TempPath))
					{
						Directory.Delete(TempPath, true);
					}

					string Message = string.Format("{0} Mods and {1} Themes Imported.", _ret[0], _ret[1]);
					XtraMessageBox.Show(Message, "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
					LoadModList();
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private int[] CopyThemeFiles(string TempPath, bool Apply = false)
		{
			int[] _ret = new int[] { 0, 0 };
			try
			{
				if (Directory.Exists(TempPath)) //%Temp%\EDHM_UI\%ZIP_NAME%
				{
					string ThemesFolder = Path.Combine(UI_DOCUMENTS, "ODYSS", "3PMods");
					theme_details ThemeDetails = null;
					List<FileInfo> ThemeFiles = null;
					string ThemeName = string.Empty;
					string Mod_Name = string.Empty;

					//Buscar el Archivo que identifica al Autor del Tema:	
					FileInfo Credits = null;
					string CreditsFile = new DirectoryInfo(TempPath).GetFiles("*.credits").Select(fi => fi.Name).FirstOrDefault().NVL("");
					if (!CreditsFile.EmptyOrNull())
					{
						try
						{
							Credits = new FileInfo(Path.Combine(TempPath, CreditsFile));
							ThemeDetails = Util.DeSerialize_FromJSON<theme_details>(Path.Combine(TempPath, CreditsFile));
						}
						catch { }
					}

					//Puede haber varias Carpetas con temas:
					List<string> MOD_DIRS = new List<string>(Directory.GetDirectories(TempPath));
					if (MOD_DIRS != null)
					{
						ThemeFiles = new List<FileInfo>();
						_ret[0] = MOD_DIRS.Count; //<- ModCount

						//Guarda el Tema en la carpeta 'Mis Documentos' del Usuario:
						// %USERPROFILE%\Documents\Elite Dangerous\EDHM_UI\ODYSS\3PMods\[MOD_NAME]\[THEME_NAME]\

						if (!Directory.Exists(ThemesFolder))
							Directory.CreateDirectory(ThemesFolder);

						//directories.Add(System.IO.Directory.GetParent(TempPath).FullName); //<- Need to search the Root too
						foreach (string _ModFolder in MOD_DIRS)
						{
							List<string> THEME_DIRS = new List<string>(Directory.GetDirectories(_ModFolder));
							Mod_Name = System.IO.Path.GetFileNameWithoutExtension(_ModFolder);
							_ret[1] += THEME_DIRS.Count; //<- ThemeCount

							foreach (string _ThemeFolder in THEME_DIRS)
							{
								ThemeName = System.IO.Path.GetFileNameWithoutExtension(_ThemeFolder);
								//Crea la Carpeta si no existe:
								if (!Directory.Exists(Path.Combine(ThemesFolder, Mod_Name, ThemeName)))
									Directory.CreateDirectory(Path.Combine(ThemesFolder, Mod_Name, ThemeName));

								//Copia los archivos del tema:
								var allowedExtensions = new[] { ".json", ".png" };
								var _Files = Directory
									.GetFiles(_ThemeFolder, Mod_Name + "*.*")
									.Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
									.ToList();

								if (_Files != null && _Files.Count > 0)
								{
									foreach (var item in _Files)
									{
										FileInfo _ThemeFile = new FileInfo(item);
										_ThemeFile.CopyTo(Path.Combine(ThemesFolder, Mod_Name, ThemeName, _ThemeFile.Name), true);

										//Si el usuario elije Aplicar el Tema Importado:
										string Ext = System.IO.Path.GetExtension(_ThemeFile.FullName); //<- Extension del archivo
										if (Apply && Ext.ToLower() == ".json" )
										{
											TPMod_Config ThemeData = Util.DeSerialize_FromJSON<TPMod_Config>(_ThemeFile.FullName);
											if (ThemeData != null && this.TPMods.IsNotEmpty())
											{
												var MooD = this.TPMods.Find(x => x.file == ThemeData.file);
												if (MooD != null)
												{
													ThemeData.root_folder = MooD.root_folder;
													ThemeData.file_full = MooD.file_full;
												}
											}
											ApplyTheme(ThemeData);
											MessageBox.Show(string.Format("The '{0}' theme for the '{1}' Mod was applied.", ThemeData.theme_name, ThemeData.mod_name), "Success!", 
												MessageBoxButtons.OK, MessageBoxIcon.Information);
										}										
									}
								}

								//Copia el archivo .credits:
								if (Credits != null)
									Credits.CopyTo(Path.Combine(ThemesFolder, Mod_Name, ThemeName, Credits.Name), true);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private void ApplyTheme(TPMod_Config ThemeData)
		{
			try
			{
				Invoke((MethodInvoker)(() =>
				{
					//TPMod_Key _SelectedElement = null;
					string _ModFileName = string.Empty;

					if (ThemeData != null)
					{
						_ModFileName = ThemeData.file;
					}

					/* AQUI SE GUARDAN LOS CAMBIOS EN EL ARCHIVO DE CONFIGURACION DEL MOD */
					if (ThemeData.mod_type == "INIConfig")
					{
						foreach (var section in ThemeData.sections)
						{
							string _SectionName = section.ini_section;       //<- INI section (constants)	

							#region Check the Config File to use

							string ModSectionDir = string.Empty;
							if (!section.file_override.EmptyOrNull())
							{
								//If the section has an override file:
								if (section.file_override.Substring(0, 2) == "./")
								{
									//Target is in the Root folder
									string RootFolder = System.IO.Path.GetDirectoryName(ThemeData.root_folder);
									if (!RootFolder.EmptyOrNull())
									{
										ModSectionDir = Path.Combine(RootFolder, section.file_override.Substring(2));
									}
								}
								else
								{
									//Target is in the same folder:
									ModSectionDir = Path.Combine(ThemeData.root_folder, section.file_override);
								}
							}
							else
							{
								//We use the Mod's file:
								ModSectionDir = Path.Combine(ThemeData.root_folder, ThemeData.file);
							}

							_IniReader_OLD = File.Exists(ModSectionDir) ? new IniFile(ModSectionDir) : null;

							#endregion

							#region Apply the Key Values

							foreach (TPMod_Key _SelectedElement in section.keys)
							{
								if (_IniReader_OLD != null)
								{
									if (_SelectedElement.type != "color")
									{
										_IniReader_OLD.WriteKey(_SelectedElement.key, _SelectedElement.value, _SectionName);
									}
									else
									{
										//Los Colores tienen las claves RGBA en el campo key: 'xR|xG|xB|xA'
										string[] keys = _SelectedElement.key.Split(new char[] { '|' });
										if (keys != null && keys.Length > 0)
										{
											//Convertir Color RGB -> sRGB -> GammaCorrected
											List<double> _GammaColors = GetGammaCorrected_RGBA(
												Color.FromArgb(Convert.ToInt32(_SelectedElement.value)));
											int i = 0;
											foreach (string _Key in keys)
											{
												_IniReader_OLD.WriteKey(_Key, _GammaColors[i].ToString(), _SectionName);
												i++;
											}
										}
									}
									//this.IniReader.WriteFile(this.ModFullPath, this._IniData, new UTF8Encoding(false)); //<- UTF8 NO BOM
								}
							}
							#endregion
						}
					}
					if (ThemeData.mod_type == "XMLConfig")
					{
						//TODO
						/*
						if (_SelectedElement.type == "color")
						{
							//Los Colores tienen las claves RGBA en el campo key: 'xR|xG|xB|xA'
							string[] keys = _SelectedElement.key.Split(new char[] { '|' });
							if (keys != null && keys.Length > 0)
							{
								//Convertir Color RGB -> sRGB -> GammaCorrected
								Color _Color = Color.FromArgb(Convert.ToInt32(_SelectedElement.value));
								List<double> _GammaColors = GetGammaCorrected_RGBA(_Color);
								int i = 0;
								foreach (string _Key in keys)
								{
									SetXMLValue(_XmlReader, _SectionName + _Key, _GammaColors[i].ToString());
									i++;
								}
							}
						}
						else
						{
							SetXMLValue(_XmlReader, _SectionName + _SelectedElement.key, _SelectedElement.value);
						}
						*/
					}
				}));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#region Color Conversion

		private double Convert_sRGB_FromLinear(double theLinearValue, double _GammaValue = 2.4)
		{ return theLinearValue <= 0.0031308f ? theLinearValue * 12.92f : Math.Pow(theLinearValue, 1.0 / _GammaValue) * 1.055 - 0.055; }
		private double Convert_sRGB_ToLinear(double thesRGBValue, double _GammaValue = 2.4)
		{ return thesRGBValue <= 0.04045f ? thesRGBValue / 12.92f : Math.Pow((thesRGBValue + 0.055f) / 1.055f, _GammaValue); }

		private List<double> GetGammaCorrected_RGBA(System.Drawing.Color _Color, double _GammaValue = 2.4)
		{
			List<double> _ret = null; //<- valores en formato RGBA
			try
			{
				//El proceso de conversion es asi:  RGB -> sRGB (no lineal) -> Gamma Corrected

				ColorManagment.ColorConverter.Init();

				//Convertir al espacio sRGB no lineal: 
				ColorManagment.ColorRGB sRGBcolor = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, _Color.R, _Color.G, _Color.B);

				//Get Gamma Corrected Values:
				_ret = new List<double>();
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.R, _GammaValue), 4));
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.G, _GammaValue), 4));
				_ret.Add(Math.Round(Convert_sRGB_ToLinear(sRGBcolor.B, _GammaValue), 4));
				_ret.Add(Math.Round(Convert.ToDouble(Util.NormalizeNumber(_Color.A, 0m, 255m, 0m, 1m)), 4)); //alpha remains linear!
			}
			catch (Exception ex) { MessageBox.Show(ex.Message + ex.StackTrace); }
			return _ret;
		}
		private Color ReverseGammaCorrected(double _Gamma_R, double _Gamma_G, double _Gamma_B, double _Gamma_A = 1.0, double _GammaValue = 2.4)
		{
			System.Drawing.Color _ret = System.Drawing.Color.White;
			try
			{
				//El proceso de de conversion es asi:   GammaCorrectedValue -> sRGB -> Linear sRGB -> RGB

				//Undo Gamma Correction: Produce Valores en sRGB
				var inv_R = Convert_sRGB_FromLinear(_Gamma_R, _GammaValue);
				var inv_G = Convert_sRGB_FromLinear(_Gamma_G, _GammaValue);
				var inv_B = Convert_sRGB_FromLinear(_Gamma_B, _GammaValue);

				//Linearizar el sRGB:
				var colorB = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, inv_R, inv_G, inv_B, true);

				//Convertir a RGB normal:
				int R = (int)Math.Round(colorB.R * 255, MidpointRounding.AwayFromZero);
				int G = (int)Math.Round(colorB.G * 255, MidpointRounding.AwayFromZero);
				int B = (int)Math.Round(colorB.B * 255, MidpointRounding.AwayFromZero);
				int A = (int)Util.NormalizeNumber(Convert.ToDecimal(_Gamma_A), 0.0m, 1m, 0m, 255m);

				_ret = System.Drawing.Color.FromArgb(A, R, G, B);
			}
			catch (Exception ex) { MessageBox.Show(ex.Message + ex.StackTrace); }
			return _ret;
		}
		private Color ReverseGammaCorrected(List<double> _GammaComponents, double _GammaValue = 2.4)
		{
			System.Drawing.Color _ret = System.Drawing.Color.White;
			try
			{
				//El proceso de de conversion es asi:   GammaCorrectedValue -> sRGB -> Linear sRGB -> RGB

				//Undo Gamma Correction: Produce Valores en sRGB
				double inv_R = Convert_sRGB_FromLinear(_GammaComponents[0], _GammaValue);
				double inv_G = Convert_sRGB_FromLinear(_GammaComponents[1], _GammaValue);
				double inv_B = Convert_sRGB_FromLinear(_GammaComponents[2], _GammaValue);
				double alpha = 1;
				if (_GammaComponents.Count > 3)
				{
					alpha = _GammaComponents[3];
				}

				//Linearizar el sRGB:
				var colorB = new ColorManagment.ColorRGB(ColorManagment.RGBSpaceName.sRGB, inv_R, inv_G, inv_B, true);

				//Convertir a RGB normal:
				int R = (int)Math.Round(colorB.R * 255, MidpointRounding.AwayFromZero);
				int G = (int)Math.Round(colorB.G * 255, MidpointRounding.AwayFromZero);
				int B = (int)Math.Round(colorB.B * 255, MidpointRounding.AwayFromZero);
				int A = (int)Util.NormalizeNumber(Convert.ToDecimal(alpha), 0.0m, 1m, 0m, 255m);

				_ret = System.Drawing.Color.FromArgb(A, R, G, B);
			}
			catch (Exception ex) { MessageBox.Show(ex.Message + ex.StackTrace); }
			return _ret;
		}

		#endregion

		public void Apply_DXSkinColors()
		{
			try
			{
				DevExpress.Skins.Skin currentSkin = DevExpress.Skins.CommonSkins.GetSkin(DevExpress.LookAndFeel.UserLookAndFeel.Default);

				var Bar_Back = currentSkin.Colors["Primary"];
				var Bar_Fore = currentSkin.Colors["Info"];
				var Bar_Hover = currentSkin.Colors["Danger"];
				var Bar_HText = currentSkin.Colors["InfoText"];

				var Button_Back = currentSkin.Colors["Danger"];
				var Button_Fore = currentSkin.Colors["ControlText"];
				var Button_Hover = currentSkin.Colors["Highlight"];
				var Button_HText = currentSkin.Colors["HighlightText"];

				var Menu_Back = currentSkin.Colors["Highlight"];
				var Menu_Fore = currentSkin.Colors["HighlightText"];
				var Menu_Hover = currentSkin.Colors["Primary"];
				var Menu_HText = currentSkin.Colors["InfoText"];


				this.vGridDetalles.Appearance.Category.ForeColor = Bar_Back;
				this.vGridDetalles.Appearance.FocusedCell.ForeColor = Bar_HText;
				this.vGridDetalles.Appearance.FocusedRecord.ForeColor = Bar_HText;

				this.vGridDetalles.Appearance.FixedLine.BackColor = Bar_Back;
				this.vGridDetalles.Appearance.FocusedRow.BackColor = Bar_Back;
				this.vGridDetalles.Appearance.HorzLine.BackColor = Bar_Hover;
				this.vGridDetalles.Appearance.VertLine.BackColor = Bar_Hover;

				//ThemeColors Form = new ThemeColors(currentSkin.Colors);
				//Form.Show();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ExecuteBAT(string FilePath, string _Arguments = "")
		{
			if (!FilePath.EmptyOrNull() && File.Exists(FilePath))
			{
				var _DOS = new System.Diagnostics.ProcessStartInfo
				{
					FileName = "cmd.exe",
					Verb = "runas", //<-   'open', 'runas', 'runasuser' | This is what actually runs the command as administrator
					WorkingDirectory = System.IO.Path.GetDirectoryName(FilePath),
					Arguments = string.Format("/C \"{0}\"", FilePath), //<- Argumentos para el CMD: /C=[Ejecuta el comando y Termina], [Ruta del BAT]=Comillas para las rutas con espacios

					ErrorDialog = true,
					ErrorDialogParentHandle = Handle

					//UseShellExecute = false,
					//RedirectStandardInput = true,
					//RedirectStandardOutput = true,

					//CreateNoWindow = true, //!_Arguments.EmptyOrNull(),
					//WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
				};
				if (!_Arguments.EmptyOrNull())
				{
					_DOS.Arguments = string.Format("/C \"{0}\" {1}", FilePath, _Arguments); //<- Agrega los Argumentos para el BAT (si los hay)
				}
				try
				{
					var _BAT = new System.Diagnostics.Process();
					_BAT.StartInfo = _DOS;
					_BAT.EnableRaisingEvents = true;
					_BAT.OutputDataReceived += (s, e) => Console.WriteLine("Data: {0}", e.Data);
					_BAT.ErrorDataReceived += (a, b) => Console.WriteLine("Error: {0}", b.Data);
					_BAT.Exited += (s, e) =>
					{
						//ExitCode: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
						//SUCCESS= 0, INVALID_FUNCTION = 1, FILE_NOT_FOUND = 2, PATH_NOT_FOUND = 3, ACCESS_DENIED = 5

						Console.WriteLine("OutPut: {0}", _BAT.StandardOutput.ReadToEnd());
						Console.WriteLine("ExitCode: {0}", _BAT.ExitCode);

						MessageBox.Show(string.Format("Process Complete. \r\nExitCode: {0}", _BAT.ExitCode), "Done",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					};
					_BAT.Start();
					_BAT.WaitForExit(1000 * 10); //<- Espera 60 segundos
					_BAT.Close();
				}
				catch (Exception)
				{
					//If you are here the user clicked decline to grant admin privileges (or he's not administrator)
					MessageBox.Show("You have to Aprove the program execution!", "Canceled!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}

		private void LoadElementPreviews(TPMod_Config _Mod)
		{
			/* CARGA LAS IMAGENES QUE SE USAN EN el READ_ME DEL MOD 
				- Las imagenes pueden ser PNG o JPG
				- Todas las imagenes deben ser del mismo tamaño.
				- La primera imagen decidirá el tamaño de las demas.
				- Puedes usar transparencias que cubran lo que sobra de una imagen.
				- Las imagenes deben estar dentro de una carpeta 'assets' en el root del mod.

			<image=ED-GT_StarCount.jpg;size=500,200;align=top>  	 */

			try
			{
				if (_Mod != null)
				{
					string ElementsFolder = Path.Combine(_Mod.root_folder, @"assets");
					if (Directory.Exists(ElementsFolder))
					{
						ElementsImgCollection.Images.Clear();

						System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
						var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
						{
							System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

							List<string> Imagefiles = Directory.EnumerateFiles(ElementsFolder, "*.*", SearchOption.AllDirectories)
												 .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"))
												 .ToList();
							if (Imagefiles != null)
							{
								ElementsImgCollection.ImageSize = Util.GetElementImage(Imagefiles[0]).Size;

								foreach (string _ImageFile in Imagefiles)
								{
									//Las imagenes se pueden acceder usando el nombre del archivo (con extension)
									Invoke((MethodInvoker)(() =>
									{
										ElementsImgCollection.AddImage(
											Util.GetElementImage(_ImageFile),
											Path.GetFileName(_ImageFile));
									}));
								}
							}
						});
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public string GetUIDocumentsDir(bool MakeDir = true)
		{
			string _ret = @"%USERPROFILE%\EDHM_UI";
			try
			{
				_ret = Util.AppConfig_GetValue("EDHM_DOCS").NVL(@"%USERPROFILE%\EDHM_UI"); //<- @"%USERPROFILE%\EDHM_UI"
				_ret = Environment.ExpandEnvironmentVariables(_ret); //<- Permite usar cualquier variable de Windows

				// https://pureinfotech.com/list-environment-variables-windows-10/   <- Windows's Enviroment Variables List

				if (MakeDir && !Directory.Exists(_ret))
				{
					Directory.CreateDirectory(_ret);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		#endregion

		#region Eventos Principales

		/* AQUI SE CAPTURAN LOS CAMBIOS HECHOS EN LOS CONTROLES del cuadro de Propiedades */
		private void PropertyGrid_EditValueChanged(object sender, EventArgs e)
		{
			try
			{
				Invoke((MethodInvoker)(() =>
				{
					TPMod_Key _SelectedElement = null;
					string _SectionName = string.Empty;
					string _ModFileName = string.Empty;

					if (CurrentdMod != null)
					{
						_ModFileName = CurrentdMod.file;
					}

					//Obtiene el Valor del Control adecuado:
					_SelectedElement = GetSelectedElement(sender);
					_SectionName = _SelectedElement.section_name;       //<- INI section (constants)			

					/* AQUI SE GUARDAN LOS CAMBIOS EN EL ARCHIVO DE CONFIGURACION DEL MOD */
					if (CurrentdMod.mod_type == "INIConfig")
					{
						#region Check the Config File to use

						string ModSectionDir = string.Empty;
						if (!_SelectedElement.root_section.file_override.EmptyOrNull())
						{
							//If the section has an override file:
							if (_SelectedElement.root_section.file_override.Substring(0, 2) == "./")
							{
								//Target is in the Root folder
								string RootFolder = System.IO.Path.GetDirectoryName(CurrentdMod.root_folder);
								if (!RootFolder.EmptyOrNull())
								{
									ModSectionDir = Path.Combine(RootFolder, _SelectedElement.root_section.file_override.Substring(2));
								}
							}
							else
							{
								//Target is in the same folder:
								ModSectionDir = Path.Combine(CurrentdMod.root_folder, _SelectedElement.root_section.file_override);
							}
						}
						else
						{
							//We use the Mod's file:
							ModSectionDir = Path.Combine(CurrentdMod.root_folder, CurrentdMod.file);
						}

						_IniReader_OLD = File.Exists(ModSectionDir) ? new IniFile(ModSectionDir) : null;

						#endregion

						if (_IniReader_OLD != null)
						{
							if (_SelectedElement.type == "color")
							{
								//Los Colores tienen las claves RGBA en el campo key: 'xR|yG|zB|wA'
								string[] keys = _SelectedElement.key.Split(new char[] { '|' });
								if (keys != null && keys.Length > 0)
								{
									//Convertir Color RGB -> sRGB -> GammaCorrected
									List<double> _GammaColors = GetGammaCorrected_RGBA(
										Color.FromArgb(Convert.ToInt32(_SelectedElement.value)));
									int i = 0;
									foreach (string _Key in keys)
									{
										_IniReader_OLD.WriteKey(_Key, _GammaColors[i].ToString(), _SectionName);
										i++;
									}
								}								
							}
							else
							{
								//La clave NO es un Color:
								_IniReader_OLD.WriteKey(_SelectedElement.key, _SelectedElement.value, _SectionName);
							}
						}

						//Guardo las Keybindings en el registro para persistir sus valores tras las actualizaciones:
						if (CurrentdMod.mod_name == "Odyssey Key Bindings")
						{
							string _Sections = Util.Serialize_ToJSON(CurrentdMod.sections);
							if (_Sections != null)
							{
								Util.WinReg_WriteKey("EDHM", "KeyBindings", _Sections);
							}
						}
					}
					if (CurrentdMod.mod_type == "XMLConfig")
					{
						/*
						if (_SelectedElement.type == "color")
						{
							//Los Colores tienen las claves RGBA en el campo key: 'xR|xG|xB|xA'
							string[] keys = _SelectedElement.key.Split(new char[] { '|' });
							if (keys != null && keys.Length > 0)
							{
								//Convertir Color RGB -> sRGB -> GammaCorrected
								Color _Color = Color.FromArgb(Convert.ToInt32(_SelectedElement.value));
								List<double> _GammaColors = GetGammaCorrected_RGBA(_Color);
								int i = 0;
								foreach (string _Key in keys)
								{
									SetXMLValue(_XmlReader, _SectionName + _Key, _GammaColors[i].ToString());
									i++;
								}
							}
						}
						else
						{
							SetXMLValue(_XmlReader, _SectionName + _SelectedElement.key, _SelectedElement.value);
						}
						*/
					}
				}));
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private TPMod_Key GetSelectedElement(object Element)
		{
			TPMod_Key _ret = null;
			try
			{
				Console.WriteLine(Element.GetType().Name);
				switch (Element.GetType().Name)
				{
					case "TextEdit":
						TextEdit _TextControl = Element as TextEdit;
						if (_TextControl.Properties.Tag != null)
						{
							_ret = _TextControl.Properties.Tag as TPMod_Key;
							_ret.value = Util.ValidarNulo(_TextControl.EditValue, "");
							_ret.section_name = _TextControl.Properties.Name.Split(new char[] { '|' })[0];
						}
						break;
					case "SpinEdit":
						SpinEdit _NumericControl = Element as SpinEdit;
						if (_NumericControl.Properties.Tag != null)
						{
							_ret = _NumericControl.Properties.Tag as TPMod_Key;
							_ret.value = Util.ValidarNulo(_NumericControl.EditValue, 0).ToString();
							_ret.section_name = _NumericControl.Properties.Name.Split(new char[] { '|' })[0];
						}
						break;
					case "ToggleSwitch":
						ToggleSwitch _ToogleControl = Element as ToggleSwitch;
						if (_ToogleControl.Properties.Tag != null)
						{
							_ret = _ToogleControl.Properties.Tag as TPMod_Key;
							_ret.value = Util.BoolToInteger(Util.ValidarNulo(_ToogleControl.EditValue, false)).ToString();
							_ret.section_name = _ToogleControl.Properties.Name.Split(new char[] { '|' })[0];
						}
						break;
					case "LookUpEdit":
						LookUpEdit _Combo = Element as LookUpEdit;
						if (_Combo.Properties.Tag != null)
						{
							_ret = _Combo.Properties.Tag as TPMod_Key;
							_ret.value = Util.ValidarNulo(_Combo.EditValue, "");
							_ret.section_name = _Combo.Properties.Name.Split(new char[] { '|' })[0];
						}
						break;
					case "TrackBarControl":
						TrackBarControl _TrackBar = Element as TrackBarControl;
						if (_TrackBar.Properties.Tag != null)
						{
							_ret = _TrackBar.Properties.Tag as TPMod_Key;
							_ret.value = (Util.ValidarNulo(_TrackBar.EditValue, 0m) / 10m).ToString();
							_ret.section_name = _TrackBar.Properties.Name.Split(new char[] { '|' })[0];
						}
						break;
					//case "ColorPickEdit": 
					//	ColorPickEdit _ColorEd = Element as ColorPickEdit;
					//	if (_ColorEd.Properties.Tag != null)
					//	{
					//		_ret = _ColorEd.Properties.Tag as TPMod_Key;
					//		if (_ColorEd.EditValue != null)
					//		{
					//			_ret.value = _ColorEd.Color.ToArgb().ToString();
					//			_ret.section_name = _ColorEd.Properties.Name.Split(new char[] { '|' })[0];
					//		}
					//	}
					//	break;
					case "MyColorPickEdit":
						MyColorPickEdit _ColorEd = Element as MyColorPickEdit;
						if (_ColorEd.Properties.Tag != null)
						{
							_ret = _ColorEd.Properties.Tag as TPMod_Key;
							if (_ColorEd.EditValue != null)
							{
								_ret.value = _ColorEd.Color.ToArgb().ToString();
								_ret.section_name = _ColorEd.Properties.Name.Split(new char[] { '|' })[0];
							}
						}
						break;
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private void vGridDetalles_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				var _Row = vGridDetalles.FocusedRow;
				if (_Row != null)
				{
					if (CurrentdMod != null)
					{
						if (_Row is CategoryRow Categoria)
						{
							//Edita la Seccion
							if (_Row.Tag != null && _Row.Tag is TPMod_Section _Section)
							{
								//Edita la Seccion
								DockSection_Edit_Section(_Section);
							}
						}
						if (_Row is EditorRow _Fila)
						{
							if (_Fila.Tag != null && _Fila.Tag is TPMod_Key _Key)
							{
								//Edita la Clave:
								DockSection_Edit_Key(_Key, _Row.ParentRow.Tag as TPMod_Section);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/* AL SELECCIONAR UN MOD DE LA LISTA  */
		private AccordionControlElement SelectedThemeElement = null;
		private void lst3PMods_ElementClick(object sender, ElementClickEventArgs e)
		{
			try
			{
				if (sender != null)
				{
					if (e.Element != null && e.Element.Tag != null)
					{
						if (e.Element.Tag is TPMod_Config _Mod)
						{
							//Util.Serialize_ToJSON(@"C:\Users\Jhollman\source\repos\myMod.json", _Mod);
							SelectedThemeElement = e.Element;
							LoadMod(_Mod);
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void lst3PMods_CustomDrawElement(object sender, CustomDrawElementEventArgs e)
		{
			//Dibuja Borde sobre el tema seleccionado
			try
			{
				if (SelectedThemeElement != null)
				{
					//Si el elemento está seleccionado, Dibuja un borde Amarillo alrededor
					if (e.ObjectInfo.Element == SelectedThemeElement || SelectedThemeElement.OwnerElement == e.ObjectInfo.Element)
					{
						if (e.Element.Tag != null)
						{
							TPMod_Config _Theme = e.Element.Tag as TPMod_Config;
							if (_Theme != null)
							{
								Rectangle _Box = e.ObjectInfo.HeaderBounds;
								_Box.Width = 200; _Box.Height = 62;

								//Aqui dibuja la imagen normalmente:
								if (_Theme.Thumbnail != null)
								{
									e.Cache.DrawImage(_Theme.Thumbnail, _Box);
								}
							}

						}

						//Aqui le dibuja un Borde Amarillo a la izquierda
						e.Cache.FillRectangle(Color.Orange, new Rectangle(e.ObjectInfo.HeaderBounds.Location, new Size(ScaleHelper.ScaleHorizontal(3),
								e.ObjectInfo.HeaderBounds.Height)));

						e.Handled = true;
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		/* AL SELECCIONAR UN TEMA DE LA LISTA  */
		private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
		{
			DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
			if (view.IsRowSelected(e.RowHandle) && e.Column.FieldName == "Preview")
			{
				if (view.GetRow(e.RowHandle) is ui_preset_new _Theme)
				{
					//Cargar el Thema Seleccionado:
					if (MessageBox.Show(string.Format("Do you want to Apply '{0}' Theme now?", _Theme.name),
						"Apply Theme?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						/*  Theme ZIP estructure:
						├──	[AUTHOR_NAME].credits
						├──	[MOD_NAME]						<- CPM-Anaconda
						|		├── [THEME_NAME]			<- @Elite Default
						|		|		├── [MOD_NAME].json  <- CPM-Anaconda.json
						|		|		└── [MOD_NAME].png
						|		└── [THEME_NAME]			<- Another Theme (for Same Ship) ie: 'JumpaConda Black'
						|				├── [MOD_NAME].json
						|				└── [MOD_NAME].png 
						├──	[MOD_NAME]						<- Another Theme (for Different ship) ie: 'CPM-Courier'		
						|		└── [THEME_NAME]			
						|				├── [MOD_NAME].json
						|				└── [MOD_NAME].png
						*/
						string ThemePath = System.IO.Path.GetDirectoryName(_Theme.folder);
						string Mod_Name = System.IO.Path.GetFileNameWithoutExtension(_Theme.folder);

						FileInfo[] _Files = new string[] { "*.json", "*.png" } //<- searchPatterns
							.SelectMany(searchPattern => new DirectoryInfo(ThemePath)
							.GetFiles(searchPattern, SearchOption.AllDirectories))
							.ToArray();

						if (_Files != null && _Files.Length > 0)
						{
							var x = CurrentdMod;
							string ModFolder = CurrentdMod.root_folder;

							foreach (FileInfo _file in _Files)
							{
								_file.CopyTo(Path.Combine(ModFolder, _file.Name), true);
							}

							//Apply the JSON values into the INI file:
							TPMod_Config MyTheme = Util.DeSerialize_FromJSON<TPMod_Config>(_Theme.folder);
							if (MyTheme != null)
							{
								MyTheme.root_folder = CurrentdMod.root_folder;
								ApplyTheme(MyTheme);
							}
						}
						LoadMod(CurrentdMod);
					}
				}
			}
		}

		#endregion

		#region Botoneras

		private void cmdSaveChanges_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			if (CurrentdMod.mod_type == "XMLConfig")
			{
				SaveXMLChanges();
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cmdRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			LoadModList();
		}

		private void cmdOpenRawFile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (CurrentdMod != null)
				{
					string ModFullPath = Path.Combine(CurrentdMod.root_folder, CurrentdMod.file);

					if (File.Exists(ModFullPath))
					{
						System.Diagnostics.Process.Start(ModFullPath);
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdOpenJsonFile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (CurrentdMod != null)
				{
					if (CurrentdMod.managed)
					{
						if (File.Exists(CurrentdMod.file_full))
						{
							System.Diagnostics.Process.Start(CurrentdMod.file_full);
						}
					}
					else
					{
						SaveMod(CurrentdMod);
					}

				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void cmdImportMod_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			ImportMod();
		}

		private void cmdEdit_RemoveMod_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			UninstallMod(CurrentdMod);
		}

		private void cmdHowTo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{

			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void cmdExport_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			ExportTheme();
		}

		private void cmdImportTheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			ImportTheme();
		}

		private void cmdFindMoreMods_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			System.Diagnostics.Process.Start("https://discord.com/channels/773552741632180224/897503811944460388");
		}

		private void cmdReadMe_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			if (!CurrentdMod.read_me.EmptyOrNull())
			{
				lblReadMe.Text = CurrentdMod.read_me;
				dockReadMe.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
				dockReadMe.ShowSliding();
			}
		}


		private void lblReadMe_HyperlinkClick(object sender, HyperlinkClickEventArgs e)
		{
			//Al dar click en un link dentro del tooltip, se abre en el Navegador x defecto:
			if (e.Link != string.Empty)
			{
				System.Diagnostics.Process.Start(e.Link);
			}
		}

		#endregion

		private TPMod_Section _CurrentSection = null;
		private event EventHandler OnSectionChanged; //<- Declaracion del Evento
		private event EventHandler OnTypesChanged;
		private event EventHandler OnKeyChanged;

		private void TPModsManager_OnSectionChanged(object _Sender, EventArgs e)
		{
			//Aplica los Cambios de la Seccion en el Mod:
			if (_Sender != null && _Sender is TPMod_Section)
			{
				var ModifiedSection = _Sender as TPMod_Section;
				var OriginalSection = CurrentdMod.sections.Find(x => x.name == ModifiedSection.name);
				if (OriginalSection != null)
				{
					OriginalSection = ModifiedSection.Clone() as TPMod_Section; //<- Aplica los cambios x Referencia

					//Save changes into the JSON:
					SaveMod(CurrentdMod);
					LoadMod(CurrentdMod);

					//Re-load the changes:
					if (dockSection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
					{
						DockSection_Edit_Section(ModifiedSection);
					}
				}
			}
		}
		private void TPModsManager_OnKeyChanged(object _Sender, EventArgs e)
		{
			if (_Sender != null && _Sender is TPMod_Key)
			{
				var OriginalKey = DockKeys_txtName.Tag as TPMod_Key;
				if (dockKeys.Tag is TPMod_Section OriginalSection && OriginalSection.keys.IsNotEmpty())
				{
					int index = OriginalSection.keys.FindIndex(x => x.name == OriginalKey.name);
					if (index >= 0)
					{
						OriginalSection.keys[index] = _Sender as TPMod_Key;

						OnSectionChanged(OriginalSection, null); //<- Disparador del Evento.
					}
				}
			}
		}

		public void DockSection_Edit_Section(TPMod_Section _Section)
		{
			try
			{
				if (_Section != null)
				{
					_CurrentSection = _Section;

					//Muestra los Valores de la Seccion Seleccionada:
					DockSection_txtName.EditValue = _CurrentSection.name;
					DockSection_txtTitle.EditValue = _CurrentSection.title;
					DockSection_txtSection.EditValue = _CurrentSection.ini_section;
					gridSectionKeys.DataSource = _CurrentSection.keys;
					dockSection.Tag = _Section;

					//Muestra la Ventana:
					dockSection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		public void DockSection_Edit_Key(TPMod_Key _Key, TPMod_Section _Section)
		{
			try
			{
				if (_Key != null && _Section != null && CurrentdMod != null)
				{
					#region Carga los tipos de datos:

					List<TPMod_Type> types = new List<TPMod_Type>();
					//Primero los tipos basicos:
					if (CurrentdMod.types != null)
					{
						types.AddRange(CurrentdMod.types);
					}

					if (CurrentdMod.custom_types != null)
					{
						//Carga los tipos 'Custom'
						List<TPMod_Type> Uniques = CurrentdMod.custom_types.GroupBy(x => x.type).Select(x => x.First()).ToList<TPMod_Type>();
						types.AddRange(Uniques);
						if (DockCustomTypes_lstGroupTypes.Items != null)
						{
							DockCustomTypes_lstGroupTypes.Items.Clear();
						}

						foreach (var custom_type in Uniques)
						{
							DockCustomTypes_lstGroupTypes.Items.Add(custom_type.type);
						}
					}
					//Muestra los tipos en el combo correspondiente:
					DockKeys_cboType.Properties.DataSource = types;
					DockKeys_cboType.Properties.ValueMember = "type";
					DockKeys_cboType.Properties.DisplayMember = "type";

					#endregion

					#region Carga los Valores de la Clave Seccionada

					DockKeys_txtName.EditValue = _Key.name;
					DockKeys_cboType.EditValue = _Key.type;
					DockKeys_txtKey.EditValue = _Key.key;
					DockKeys_txtValue.EditValue = _Key.value;
					DockKeys_memoDescription.EditValue = _Key.description;

					DockKeys_txtName.Tag = _Key; //<- Preserves the Original Values of the Key

					#endregion

					//Muestra la Ventana y espera el evento de Cambio:
					dockKeys.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
					dockKeys.Tag = _Section; //<- Preserves the Section this key belongs to
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		private void DockSection_cmdApply_Click(object sender, EventArgs e)
		{
			_CurrentSection.name = DockSection_txtName.EditValue.ToString();
			_CurrentSection.title = DockSection_txtTitle.EditValue.ToString();
			_CurrentSection.ini_section = DockSection_txtSection.EditValue.ToString();

			dockSection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
			OnSectionChanged(_CurrentSection, null); //<- Disparador del Evento.
		}
		private void DockKeys_cmdApply_Click(object sender, EventArgs e)
		{
			TPMod_Key _Key = new TPMod_Key()
			{
				name = DockKeys_txtName.EditValue.ToString(),
				type = DockKeys_cboType.EditValue.ToString(),
				key = DockKeys_txtKey.EditValue.ToString(),
				value = DockKeys_txtValue.EditValue.ToString(),
				description = DockKeys_memoDescription.EditValue.ToString()
			};
			_CurrentSection = dockKeys.Tag as TPMod_Section;

			dockKeys.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
			OnKeyChanged(_Key, null); //<- Disparador del Evento.
		}
		private void cmdSectionsEditor_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			//Invoke the Section Editor for the Selected Section:
			if (CurrentdMod != null)
			{
				if (vGridDetalles.FocusedRow != null)
				{
					if (vGridDetalles.FocusedRow is CategoryRow)
					{
						if (vGridDetalles.FocusedRow.Tag != null)
						{
							_CurrentSection = vGridDetalles.FocusedRow.Tag as TPMod_Section;
						}
					}
					else
					{
						if (vGridDetalles.FocusedRow.ParentRow.Tag != null)
						{
							_CurrentSection = vGridDetalles.FocusedRow.ParentRow.Tag as TPMod_Section;
						}
					}

					DockSection_Edit_Section(_CurrentSection);
				}
			}
		}

		private void gridviewSectionKeys_DoubleClick(object sender, EventArgs e)
		{
			TPMod_Key Row = (TPMod_Key)gridviewSectionKeys.GetFocusedRow();
			if (Row != null)
			{
				DockSection_Edit_Key(Row, dockSection.Tag as TPMod_Section);
			}
		}

		// -- CUSTOM TYPES:
		private void DockKeys_cmdCustomTypes_Click(object sender, EventArgs e)
		{
			//DockCustomTypes_lstGroupTypes.DataSource = this.CurrentdMod.custom_types;


			dockCustomTypesEditor.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
		}
		private void DockCustomTypes_lstGroupTypes_SelectedIndexChanged(object sender, EventArgs e)
		{
			var SelectedItem = DockCustomTypes_lstGroupTypes.SelectedItem;
			if (SelectedItem != null)
			{
				string _Text = SelectedItem.ToString();

				List<TPMod_Type> _CusTypes = CurrentdMod.custom_types.FindAll(x => x.type == _Text);
				if (_CusTypes != null)
				{
					DockCustomTypes_lstTypes.Items.Clear();
					foreach (var custom_type in _CusTypes)
					{
						DockCustomTypes_lstTypes.Items.Add(custom_type.name);
					}
				}
			}
		}
		private void DockCustomTypes_lstTypes_SelectedIndexChanged(object sender, EventArgs e)
		{
			var SelectedItem = DockCustomTypes_lstGroupTypes.SelectedItem;
			if (SelectedItem != null)
			{
				string _Type = SelectedItem.ToString();
				string _Text = SelectedItem.ToString();
				TPMod_Type myType = CurrentdMod.custom_types.Find(x => x.type == _Type && x.name == _Text);
				if (myType != null)
				{
					DockCustomTypes_txtType.EditValue = myType.type;
					DockCustomTypes_txtName.EditValue = myType.name;
					DockCustomTypes_txtValue.EditValue = myType.value;
				}
			}
		}
		private void DockCustomTypes_CmdApply_Click(object sender, EventArgs e)
		{
			OnTypesChanged(CurrentdMod.custom_types, null);
		}

		private void cmdKeysEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{

		}
		private void cmdKeysAddNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{

		}


	}
}