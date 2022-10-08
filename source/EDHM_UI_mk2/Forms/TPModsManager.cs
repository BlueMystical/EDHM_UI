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
using System.Xml.Linq;

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

		private string UI_DOCUMENTS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Elite Dangerous\EDHM_UI");

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
				dockReadMe.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
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
							_Mod.file =_Mod.file.Replace("%LOCALAPPDATA%", LocalAppData);
							if (File.Exists(_Mod.file))
							{
								ModFullPath = _Mod.file;
								this.cmdSaveChanges.Caption = "Apply XML Changes";

								//Read the values from an XML file.
								this._XmlReader = new XmlDocument();
								_XmlReader.Load(_Mod.file);

								if (_Mod.sections != null && _Mod.sections.Count > 0)
								{
									foreach (var _Section in _Mod.sections)
									{
										if (_Section.keys.IsNotEmpty())
										{
											string[] SectionPath = _Section.ini_section.Split(new char[] { '\\' });

											foreach (var _key in _Section.keys)
											{
												if (_key.visible)
												{
													_key.value = Util.GetXMLValue(_XmlReader, _Section.ini_section, _key.key, _key.value);

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
																#region Muestra un Toggle Switch:

																Invoke((MethodInvoker)(() =>
																{
																	RepositoryItemToggleSwitch _ToggleControl = new RepositoryItemToggleSwitch();
																	_ToggleControl.Name = string.Format("{0}|{1}", _Section.ini_section, _key.name);
																	_ToggleControl.EditValueChanged += PropertyGrid_EditValueChanged;
																	_ToggleControl.EditValueChangedDelay = 500;
																	_ToggleControl.Tag = _key;

																//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

																_Fila.Properties.RowEdit = _ToggleControl;
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
								this._IniReader_OLD = new IniFile(ModFullPath);
								if (_IniReader_OLD != null)
								{
									this.cmdSaveChanges.Caption = "Close";

									if (_Mod.sections.IsNotEmpty())
									{
										foreach (var _Section in _Mod.sections)
										{
											if (_Section.keys.IsNotEmpty())
											{
												foreach (var _key in _Section.keys)
												{
													//Reads the Value from the Ini File:
													_key.value = _IniReader_OLD.ReadKey(_key.key, _Section.ini_section);

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
															#region Muestra un Toggle Switch:

															Invoke((MethodInvoker)(() =>
															{
																RepositoryItemToggleSwitch _ToggleControl = new RepositoryItemToggleSwitch();
																_ToggleControl.Name = string.Format("{0}|{1}", _Section.ini_section, _key.name);
																_ToggleControl.EditValueChanged += PropertyGrid_EditValueChanged;
																_ToggleControl.EditValueChangedDelay = 500;
																_ToggleControl.Tag = _key;

																//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
																//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

																_Fila.Properties.RowEdit = _ToggleControl;
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
								this.vGridDetalles.EndUpdate();

								if (UI_Themes != null && UI_Themes.Count > 0)
								{
									this.gridThemes.DataSource = UI_Themes;
									dock3PM_Themes.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
								}
								else
								{
									gridThemes.DataSource = null;
								}

								if (!_Mod.read_me.EmptyOrNull())
								{
									this.cmdReadMe.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
								}
								else
								{
									this.cmdReadMe.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
								}

								LoadElementPreviews(_Mod);

								this.lblMODdescription.Text = string.Format("<b>{0}</b>\r\n{1}",
									string.Format("<b>{0}</b> ( {1} )", _Mod.mod_name, _Mod.managed ? "<color=0, 255, 0>Managed</color>" : "<color=255, 0, 0>Unmanaged</color>"),
									string.Format("           By {0}\r\n\r\nDescription:\r\n<i>{1}</i>", _Mod.author, _Mod.description));

								CurrentdMod = _Mod;
							}
							catch (Exception)
							{

								throw;
							}
							finally { Cursor = Cursors.Default; }
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
				if (this.CurrentdMod != null && this.CurrentdMod.mod_type == "XMLConfig")
				{
					this._XmlReader = new XmlDocument();
					var GridSections = vGridDetalles.Rows;
					this._XmlReader.Load(this.CurrentdMod.file);

					foreach (var ModSection in this.CurrentdMod.sections)
					{
						foreach (var ModKey in ModSection.keys)
						{
							foreach (var _Section in GridSections)
							{
								if (_Section.Name == ModSection.name)
								{
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
								}								
							}

							Util.SetXMLValue(_XmlReader, ModSection.ini_section + ModKey.key, ModKey.value);
						}
					}

					this._XmlReader.Save(this.ModFullPath);

					if (!Silent)
					{
						MessageBox.Show("Changes in the XML Configuration had been saved!\r\nYou will need to re-start the game", 
							"ERROR", MessageBoxButtons.OK, MessageBoxIcon.Information);
						this._XmlReader = null;
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
					 * ├── [AUTHOR_NAME].credits
					   ├── ShaderFixes		
					   |	└── [Optional Shader Files]
					   └── EDHM-ini
					   |	└── 3rdPartyMods
					   |		└── [MOD_NAME]					<- Files can either be in the root or in a subfolder
					   |			├── [MOD_NAME].ini
					   |			├── [MOD_NAME].json
					   |			├── [MOD_NAME].bat
					   |			└── [MOD_NAME].png	
					   └── Themes								<- [Optional Themes]
							├──	[AUTHOR_NAME].credits
					   		└──	[MOD_NAME]						<- CPM-Anaconda
									├── [THEME_NAME]			<- @Elite Default
									|		├── [MOD_NAME].ini	<- CPM-Anaconda.ini
									|		├── [MOD_NAME].json
									|		└── [MOD_NAME].png
									└── [THEME_NAME]
											├── [MOD_NAME].ini
											├── [MOD_NAME].json
											└── [MOD_NAME].png 
					 */

					Util.DoNetZIP_UnCompressFile(OFDialog.FileName, ActiveInstance.path);

					XtraMessageBox.Show("Mod Imported.", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
					LoadModList();
					Cursor = Cursors.Default;
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void UninstallMod()
		{
			try
			{
				if (CurrentdMod != null)
				{
					if (XtraMessageBox.Show(string.Format("Are you sure of Deleting the MOD: '{0}'?", CurrentdMod.mod_name),
						"Confirm Deletion?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						if (!CurrentdMod.file_full.EmptyOrNull() && Directory.Exists(CurrentdMod.root_folder))
						{
							//Remove Mod's main files:
							var dir = new DirectoryInfo(CurrentdMod.root_folder);
							foreach (var file in dir.EnumerateFiles(string.Format("{0}.*", Path.GetFileNameWithoutExtension(CurrentdMod.file))))
							{
								file.Delete();
							}
							if (CurrentdMod.dependencies.IsNotEmpty())
							{
								//Remove Mod dependencies:
								foreach (string _Dependency in CurrentdMod.dependencies)
								{
									string _File = _Dependency.Replace("ShaderFixes", Path.Combine(ActiveInstance.path, "ShaderFixes"));
									if (File.Exists(_File))
									{
										File.Delete(_File);
									}
								}
							}
							if (CurrentdMod.IsRootMod)
							{
								System.GC.Collect();
								System.GC.WaitForPendingFinalizers();
								try
								{
									Directory.Delete(CurrentdMod.root_folder, true);
								}
								catch { }
							}

							LoadModList(); //<- Actualizar los cambios
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

		private void ExportMod(TPMod_Config _Mod)
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
					string TempPath =	Path.Combine(Path.GetTempPath(), "EDHM_UI", ThemeName);					

					//2. Crear una Carpeta Temporal para los Archivos del Tema:
					if (Directory.Exists(TempPath)) Directory.Delete(TempPath, true);
					Directory.CreateDirectory(TempPath);

					if (Directory.Exists(TempPath))
					{
						/*  Theme ZIP estructure:
						[MOD_NAME]						<- CPM-Anaconda
							├── [THEME_NAME]			<- @Elite Default
							|		├── [MOD_NAME].ini	<- CPM-Anaconda.ini
							|		├── [MOD_NAME].json
							|		└── [MOD_NAME].png
							├── [THEME_NAME]
							|		├── [MOD_NAME].ini
							|		├── [MOD_NAME].json
							|		└── [MOD_NAME].png
							└── [AUTHOR_NAME].credits
						 */

						//3. Crea la Estructura de Directorios necesaria: %TEMP_FOLDER%\%MOD_NAME%\%ThemeName%    
						string ThemeFolder = Path.Combine(TempPath, ModName, ThemeName);
						Directory.CreateDirectory(ThemeFolder);

						if (Directory.Exists(ThemeFolder))
						{
							//4. Copiar los Archivos del MOD a la carpeta Temporal:
							string[] FileList = Directory.GetFiles(ModPath, ModName + "*.*");
							if (FileList != null && FileList.Length > 0)
							{
								foreach (string _file in FileList)
								{
									File.Copy(_file, Path.Combine(ThemeFolder, Path.GetFileName(_file)), true);
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
							Util.Serialize_ToJSON(Path.Combine(TempPath, string.Format("{0}.credits", _Mod.author)), ThemeDetails);

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
						Util.Serialize_ToJSON(ThemePath + ".json", CurrentdMod);

						ExportMod(CurrentdMod);
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
					/* MOD ZIP Estructure:
					 * ├── [AUTHOR_NAME].credits
					   ├── ShaderFixes		
					   |	└── [Optional Shader Files]
					   └── EDHM-ini
							└──3rdPartyMods					//<- Files can either be in the root or in a subfolder
								├── [MOD_NAME]			
								|	  ├── [MOD_NAME].ini
								|	  ├── [MOD_NAME].json
								|	  ├── [MOD_NAME].bat
								|	  └── [MOD_NAME].png	
						----------------------------------------------------------------------		
						/*  Theme ZIP estructure:
						[MOD_NAME]						<- CPM-Anaconda
							├── [THEME_NAME]			<- @Elite Default
							|		├── [MOD_NAME].ini	<- CPM-Anaconda.ini
							|		├── [MOD_NAME].json
							|		└── [MOD_NAME].png
							├── [THEME_NAME]
							|		├── [MOD_NAME].ini
							|		├── [MOD_NAME].json
							|		└── [MOD_NAME].png
							└── [AUTHOR_NAME].credits
						 */

					theme_details  ThemeDetails = null;
					List<FileInfo> ThemeFiles = null;

					string ThemesFolder = Path.Combine(UI_DOCUMENTS, "ODYSS", "3PMods");
					string ThemeName = Path.GetFileNameWithoutExtension(OFDialog.FileName);
					string TempPath = Path.Combine(Path.GetTempPath(), "EDHM_UI", ThemeName);
					
					string ModPath_Root = string.Empty;
					string Mod_Root = string.Empty;
					string Mod_Name = string.Empty;

					int ModCount = 0;
					int ThemeCount = 0;

					//2. Crear una Carpeta Temporal para los Archivos del Tema:
					if (Directory.Exists(TempPath)) Directory.Delete(TempPath, true);
					Directory.CreateDirectory(TempPath);

					if (Directory.Exists(TempPath))
					{
						Util.DoNetZIP_UnCompressFile(OFDialog.FileName, TempPath);
						//%Temp%\EDHM_UI\%ZIP_NAME%

						//Buscar el Archivo que identifica al Autor del Tema:	
						FileInfo Credits = null;
						string CreditsFile = new DirectoryInfo(TempPath).GetFiles("*.credits").Select(fi => fi.Name).FirstOrDefault().NVL("Unknown.credits");
						if (!CreditsFile.EmptyOrNull())
						{
							try
							{
								Credits = new FileInfo(Path.Combine(TempPath, CreditsFile));
								ThemeDetails = Util.DeSerialize_FromJSON<theme_details>(Path.Combine(TempPath, CreditsFile));
							}
							catch { }
						}

						List<string> MOD_DIRS = new List<string>(Directory.GetDirectories(TempPath));
						if (MOD_DIRS != null)
						{
							ThemeFiles = new List<FileInfo>();
							ModCount = MOD_DIRS.Count;

							//Guarda el Tema en la carpeta 'Mis Documentos' del Usuario:
							// %USERPROFILE%\Documents\Elite Dangerous\EDHM_UI\ODYSS\3PMods\[MOD_NAME]\[THEME_NAME]\
							
							if (!Directory.Exists(ThemesFolder)) Directory.CreateDirectory(ThemesFolder);

							//directories.Add(System.IO.Directory.GetParent(TempPath).FullName); //<- Need to search the Root too
							foreach (string _ModFolder in MOD_DIRS)
							{								
								List<string> THEME_DIRS = new List<string>(Directory.GetDirectories(_ModFolder));
								Mod_Name = System.IO.Path.GetFileNameWithoutExtension(_ModFolder);
								ThemeCount = THEME_DIRS.Count;

								foreach (string _ThemeFolder in THEME_DIRS)
								{
									ThemeName = System.IO.Path.GetFileNameWithoutExtension(_ThemeFolder);
									//Crea la Carpeta si no existe:
									if (!Directory.Exists(Path.Combine(ThemesFolder, Mod_Name, ThemeName)))
									{
										Directory.CreateDirectory(Path.Combine(ThemesFolder, Mod_Name, ThemeName));
									}

									//Copia los archivos del tema:
									FileInfo[] _Files = new DirectoryInfo(_ThemeFolder).GetFiles(Mod_Name + ".*");
									if (_Files != null && _Files.Length > 0)
									{
										foreach (var item in _Files)
										{
											item.CopyTo(Path.Combine(ThemesFolder, Mod_Name, ThemeName, item.Name), true);
											item.CopyTo(Path.Combine(ThemesFolder, Mod_Name, ThemeName, item.Name), true);
										}
									}

									//Copia el archivo .credits:
									Credits.CopyTo(Path.Combine(ThemesFolder, Mod_Name, ThemeName, Credits.Name), true);
								}								
							}
						}						
					}			

					// Borrar la Carpeta Temporal
					if (Directory.Exists(TempPath)) Directory.Delete(TempPath, true);
					string Message = string.Format("{0} Mods and {1} Themes Imported.", ModCount, ThemeCount);
					XtraMessageBox.Show(Message, "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
					LoadModList();
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
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
					ErrorDialogParentHandle = this.Handle

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

					if (CurrentdMod != null) _ModFileName = CurrentdMod.file;

					//Obtiene el Valor del Control adecuado:
					_SelectedElement = GetSelectedElement(sender);
					_SectionName = _SelectedElement.section_name;

					/* AQUI SE GUARDAN LOS CAMBIOS EN EL ARCHIVO DE CONFIGURACION DEL MOD */
					if (CurrentdMod.mod_type == "INIConfig")
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
									Color _Color = Color.FromArgb(Convert.ToInt32(_SelectedElement.value));
									List<double> _GammaColors = GetGammaCorrected_RGBA(_Color);
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
						ToggleSwitch _ToggleControl = Element as ToggleSwitch;						
						if (_ToggleControl.Properties.Tag != null)
						{
							_ret = _ToggleControl.Properties.Tag as TPMod_Key;
							_ret.value = Util.BoolToInteger(Util.ValidarNulo(_ToggleControl.EditValue, false)).ToString();
							_ret.section_name = _ToggleControl.Properties.Name.Split(new char[] { '|' })[0];
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
					case "ColorPickEdit":
						ColorPickEdit _ColorEd = Element as ColorPickEdit;						
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
				//var _Row = vGridDetalles.FocusedRow;
				//if (_Row != null)
				//{
				//	if (CurrentdMod != null)
				//	{
				//		if (_Row is CategoryRow Categoria)
				//		{
				//			//Edita la Seccion
				//			Dock_SectionEditor_Show(Categoria, "Edit Section:");
				//		}
				//		if (_Row is EditorRow _Fila)
				//		{
				//			if (_Fila.Tag != null && _Fila.Tag is TPMod_Key _Key)
				//			{
				//				//Edita la Clave:
				//				Dock_KeyEditor_Show(_Key, "Edit Key:");
				//			}
				//		}
				//	}
				//}
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
					if (MessageBox.Show("Do you want to Apply this Theme now?", "Apply Theme?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						/*  Theme ZIP estructure:
						[MOD_NAME]						<- CPM-Anaconda
							├── [THEME_NAME]			<- @Elite Default
							|		├── [MOD_NAME].ini	<- CPM-Anaconda.ini
							|		├── [MOD_NAME].json
							|		└── [MOD_NAME].png
							├── [THEME_NAME]
							|		├── [MOD_NAME].ini
							|		├── [MOD_NAME].json
							|		└── [MOD_NAME].png
							└── [AUTHOR_NAME].credits
						 */
						string ThemePath = System.IO.Path.GetDirectoryName(_Theme.folder);
						string[] FileList = Directory.GetFiles(ThemePath, "*.*");
						if (FileList != null && FileList.Length > 0)
						{
							var x = this.CurrentdMod;
							string ModFolder = this.CurrentdMod.root_folder; // System.IO.Path.GetDirectoryName(this.CurrentdMod.root_folder);

							foreach (string _file in FileList)
							{
								File.Copy(_file, Path.Combine(ModFolder, Path.GetFileName(_file)), true);
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
			UninstallMod();
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
				this.lblReadMe.Text = CurrentdMod.read_me;
				this.dockReadMe.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
				this.dockReadMe.ShowSliding();
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
	}
}