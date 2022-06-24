using DevExpress.Utils;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraVerticalGrid.Rows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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

		private void LoadModList()
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
							var Mods = new DirectoryInfo(_Folder).GetFiles("*.ini");
							if (Mods != null && Mods.Length > 0)
							{
								int ModCountThisFolder = 0;
								AccordionControlElement _ModElement = null;

								foreach (FileInfo _File in Mods)
								{
									//Revisa si el MOD tiene un JSON acompañante:
									string FileName = System.IO.Path.GetFileNameWithoutExtension(_File.FullName); //<- Nombre sin Extension ni Path
									TPMod_Config _ManagedMod = null;

									if (File.Exists(Path.Combine(_Folder, FileName + ".json")))
									{
										#region Si hay JSON es un Mod Manejado:

										_ManagedMod = Util.DeSerialize_FromJSON<TPMod_Config>(
																				Path.Combine(_Folder, FileName + ".json"));
										if (_ManagedMod != null)
										{
											_ManagedMod.managed = true;
											_ManagedMod.file_full = _Folder;

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
										}

										#endregion
									}
									else
									{
										#region Un-Managed Mod

										_ManagedMod = new TPMod_Config
										{
											mod_name = FileName,
											author = "Unknown",
											description = @"¯\_(ツ)_/¯",
											file = _File.Name,
											file_full = _Folder,
											managed = false
										};

										_ManagedMod.types = new List<TPMod_Type>();
										_ManagedMod.types.Add(new TPMod_Type("text", "Text"));
										_ManagedMod.types.Add(new TPMod_Type("number", "Integer Number"));
										_ManagedMod.types.Add(new TPMod_Type("decimal", "Decimal Number"));
										_ManagedMod.types.Add(new TPMod_Type("color", "Color Value"));
										_ManagedMod.types.Add(new TPMod_Type("toggle", "ON/OFF Switch"));

										if (File.Exists(Path.Combine(AppExePath, @"Images\3PM_Default.png")))
										{
											//_ManagedMod.Thumbnail = Image.FromFile(Path.Combine(AppExePath, @"Images\3PM_Default.png"));
											//Carga la Imagen sin dejara 'en uso':
											using (Stream stream = File.OpenRead(Path.Combine(AppExePath, @"Images\3PM_Default.png")))
											{
												_ManagedMod.Thumbnail = System.Drawing.Image.FromStream(stream);
											}
										}

										/* Abre el INI y Carga sus Secciones y Claves  */
										if (File.Exists(_File.FullName))
										{
											var IniReader = new IniFile(Path.Combine(_Folder, FileName + ".ini"));

											if (IniReader != null)
											{
												var _IniSections = IniReader.GetAllSectionNames();

												if (_IniSections != null && _IniSections.Count > 0)
												{
													_ManagedMod.sections = new List<TPMod_Section>();

													foreach (var _RawSection in _IniSections)
													{
														TPMod_Section _Section = new TPMod_Section
														{
															name = _RawSection,
															title = _RawSection
														};

														var _RawKeys = IniReader.GetAllKeys(_RawSection);
														if (_RawKeys != null && _RawKeys.Count > 0)
														{
															_Section.keys = new List<TPMod_Key>();
															foreach (var _Rawkey in _RawKeys)
															{
																TPMod_Key _Key = new TPMod_Key
																{
																	name = _Rawkey,
																	key = _Rawkey,
																	type = "text",
																	description = _Rawkey, //_Comments.Length > 0 ? _Comments.ToString() : string.Empty,
																	value = IniReader.ReadKey(_Rawkey, _RawSection)
																};
																_Section.keys.Add(_Key);
															}
														}
														_ManagedMod.sections.Add(_Section);
													}
												}
											}
										}

										#endregion
									}

									if (_ManagedMod != null)
									{
										ModCountThisFolder++;

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

										TPMods.Add(_ManagedMod);

										Invoke((MethodInvoker)(() =>
										{
											SuperToolTipSetupArgs args = new SuperToolTipSetupArgs();
											args.Title.Text = string.Format("{0} ({1})", _ManagedMod.mod_name, _ManagedMod.managed ? "Managed" : "Unmanaged");
											args.Contents.Text = string.Format("    By {0}\r\n\r\nDescription:\r\n{1}", _ManagedMod.author, _ManagedMod.description);
											SuperToolTip sTooltip2 = new SuperToolTip();
											sTooltip2.Setup(args);

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
									}
								}
							}
						}
					}
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
				});
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void LoadMod(TPMod_Config _Mod)
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
						ModFullPath = Path.Combine(_Mod.file_full, _Mod.file);

						if (File.Exists(ModFullPath))
						{
							/* READS THE VALUES FROM THE INI FILES */
							_IniReader_OLD = new IniFile(ModFullPath);
							if (_IniReader_OLD != null)
							{
								if (_Mod.sections.IsNotEmpty())
								{
									foreach (var _Section in _Mod.sections)
									{
										if (_Section.keys.IsNotEmpty())
										{
											foreach (var _key in _Section.keys)
											{
												Console.WriteLine(string.Format("{0};{1};{2}", _key.key, _key.name, _Mod.file));
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
													if (!(vGridDetalles.Rows[_Section.name] is CategoryRow Categoria))
													{
														Categoria = new CategoryRow(_Section.title)
														{
															Name = _Section.name,
															Tag = _Section
														};
														Invoke((MethodInvoker)(() =>
														{
															vGridDetalles.Rows.Add(Categoria);
														}));
													}
													Categoria.ChildRows.Add(_Fila);
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
									string RootFolder = System.IO.Path.GetFileNameWithoutExtension(_Folder);
									var Theme = new DirectoryInfo(_Folder).GetFiles("*.json");
									if (Theme != null && Theme.Length > 0)
									{
										var ThemeDetails = Util.DeSerialize_FromJSON<TPMod_Config>(Theme[0].FullName);
										ui_preset_new _theme = new ui_preset_new(RootFolder, Theme[0].FullName)
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
												e.DrawString(RootFolder, new System.Drawing.Font("Tahoma", 8, FontStyle.Bold),
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
									dock3PM_Themes.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
								}
								else
								{
									gridThemes.DataSource = null;
								}

								lblMODdescription.Text = string.Format("<b>{0}</b>\r\n{1}",
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
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
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
					   ├── ShaderFixes		
					   |	└── [Optional Shader Files]
					   └── EDHM-ini
							└──3rdPartyMods
								├── [MOD_NAME]			//<- Files can either be in the root or in a subfolder
								|	  └── [Files]
								├── [MOD_NAME].ini
								├── [MOD_NAME].json
								├── [MOD_NAME].bat
								└── [MOD_NAME].png					 
					 */

					Util.DoNetZIP_UnCompressFile(OFDialog.FileName, ActiveInstance.path);

					LoadModList();
				}
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
				string ModPath = _Mod.file_full; // Path.GetDirectoryName(this.ModFullPath);
				string ModName = Path.GetFileNameWithoutExtension(_Mod.file); //Path.GetFileNameWithoutExtension(this.ModFullPath);

				//1. Preguntar donde se Guarda el Mod (o Tema):
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

					string ThemeName = System.IO.Path.GetFileNameWithoutExtension(XSFD.FileName);
					string TempPath = Path.Combine(Path.GetTempPath(), "EDHM_UI", ThemeName);
					string RootFolder = Path.GetFileNameWithoutExtension(_Mod.file_full);

					//2. Crear una Carpeta Temporal para los Archivos del Tema:
					if (Directory.Exists(TempPath))
					{
						Directory.Delete(TempPath, true);
					}

					Directory.CreateDirectory(TempPath);

					if (Directory.Exists(TempPath))
					{
						//3. Crea la Estructura de Directorios necesaria:  %TEMP_FOLDER%\EDHM-ini\3rdPartyMods\%MOD_NAME%\
						string ThemeFolder = Path.Combine(TempPath, "EDHM-ini", "3rdPartyMods");
						if (RootFolder != "3rdPartyMods")
						{
							ThemeFolder = Path.Combine(ThemeFolder, RootFolder);
						}
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

							//5. Comprimir la Carpeta Temporal:
							Util.DoNetZIP_CompressFolder(Path.Combine(TempPath, "EDHM-ini"), XSFD.FileName);
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
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
			finally { Cursor = Cursors.Default; }
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

				if (OFDialog.ShowDialog() == DialogResult.OK)
				{
					/* ZIP Estructure:
					   ├── ShaderFixes		
					   |	└── [Optional Shader Files]
					   └── EDHM-ini
							└──3rdPartyMods
								├── [MOD_NAME]			//<- Files can either be in the root or in a subfolder
								|	  └── [Files]
								├── [MOD_NAME].ini
								├── [MOD_NAME].json
								├── [MOD_NAME].bat
								└── [MOD_NAME].png					 
					 */
					string ThemeName = Path.GetFileNameWithoutExtension(OFDialog.FileName);
					string TempPath = Path.Combine(Path.GetTempPath(), "EDHM_UI", ThemeName);
					List<FileInfo> ThemeFiles = null;
					string ModPath_Root = string.Empty;
					string Mod_Root = string.Empty;
					string Mod_Name = string.Empty;


					//2. Crear una Carpeta Temporal para los Archivos del Tema:
					if (Directory.Exists(TempPath))
					{
						Directory.Delete(TempPath, true);
					}

					Directory.CreateDirectory(TempPath);
					if (Directory.Exists(TempPath))
					{
						Util.DoNetZIP_UnCompressFile(OFDialog.FileName, TempPath);

						ModPath_Root = Path.Combine(TempPath, "EDHM-ini", "3rdPartyMods");
						if (Directory.Exists(ModPath_Root))
						{
							List<string> directories = new List<string>(Directory.GetDirectories(ModPath_Root));
							if (directories != null)
							{
								directories.Add(ModPath_Root); //<- Need to search the Root too
								ThemeFiles = new List<FileInfo>();

								foreach (string _Folder in directories)
								{
									FileInfo[] _Files = new DirectoryInfo(_Folder).GetFiles("*.ini");
									if (_Files != null && _Files.Length > 0)
									{
										ThemeFiles.AddRange(_Files);
									}
								}
							}
						}
					}


					if (MessageBox.Show(string.Format("Do you want to add '{0}' as a Theme?", ThemeName), "Add Theme?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						// %USERPROFILE%\Documents\Elite Dangerous\EDHM_UI\ODYSS\3PMods\[MOD_NAME]\[THEME_NAME]\
						string ThemesFolder = Path.Combine(UI_DOCUMENTS, "ODYSS", "3PMods");
						if (!Directory.Exists(ThemesFolder))
						{
							Directory.CreateDirectory(ThemesFolder);
						}

						if (ThemeFiles != null && ThemeFiles.Count > 0)
						{
							foreach (var _file in ThemeFiles)
							{
								Mod_Name = Path.GetFileNameWithoutExtension(_file.Name);
								if (!Directory.Exists(Path.Combine(ThemesFolder, Mod_Name, ThemeName)))
								{
									Directory.CreateDirectory(Path.Combine(ThemesFolder, Mod_Name, ThemeName));
								}

								FileInfo[] _Files = _file.Directory.GetFiles(Mod_Name + ".*");
								foreach (var item in _Files)
								{
									item.CopyTo(Path.Combine(ThemesFolder, Mod_Name, ThemeName, item.Name), true);
								}
							}
						}
					}

					// Borrar la Carpeta Temporal
					if (Directory.Exists(TempPath))
					{
						Directory.Delete(TempPath, true);
					}

					XtraMessageBox.Show("Import Complete.", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
					CreateNoWindow = true, //This hides the dos-style black window that the command prompt usually shows
					FileName = @"cmd.exe",
					Verb = "runas", //<-   'open', 'runas', 'runasuser' | This is what actually runs the command as administrator
					WorkingDirectory = System.IO.Path.GetDirectoryName(FilePath),
					Arguments = string.Format("/C \"{0}\"", FilePath) //<- Comillas para las rutas con espacios
				};
				if (!_Arguments.EmptyOrNull())
				{
					_DOS.Arguments = string.Format("/C \"{0}\" {1}", FilePath, _Arguments);
				}
				try
				{
					var _BAT = new System.Diagnostics.Process();
					_BAT.StartInfo = _DOS;
					_BAT.Start();
					_BAT.WaitForExit();

					Console.WriteLine("ExitCode: {0}", _BAT.ExitCode);
					if (_BAT.ExitCode >= 0)
					{
						//ExitCode: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
						//SUCCESS= 0, INVALID_FUNCTION = 1, FILE_NOT_FOUND = 2, PATH_NOT_FOUND = 3, ACCESS_DENIED = 5

						//MessageBox.Show(string.Format("Process Complete. \r\nExitCode: {0}", _BAT.ExitCode), "Done",
						//	MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					_BAT.Close();
				}
				catch (Exception)
				{
					//If you are here the user clicked decline to grant admin privileges (or he's not administrator)
					MessageBox.Show("You have to Aprove the program execution!", "Canceled!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
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

					if (CurrentdMod != null)
					{
						_ModFileName = CurrentdMod.file;
					}


					//Obtiene el Valor del Control adecuado:
					switch (sender.GetType().Name)
					{
						case "TextEdit":
							TextEdit _TextControl = sender as TextEdit;
							_SectionName = _TextControl.Properties.Name.Split(new char[] { '|' })[0];
							if (_TextControl.Properties.Tag != null)
							{
								_SelectedElement = _TextControl.Properties.Tag as TPMod_Key;
								_SelectedElement.value = Util.ValidarNulo(_TextControl.EditValue, "");
							}
							break;
						case "SpinEdit":
							SpinEdit _NumericControl = sender as SpinEdit;
							_SectionName = _NumericControl.Properties.Name.Split(new char[] { '|' })[0];
							if (_NumericControl.Properties.Tag != null)
							{
								_SelectedElement = _NumericControl.Properties.Tag as TPMod_Key;
								_SelectedElement.value = Util.ValidarNulo(_NumericControl.EditValue, 0).ToString();
							}
							break;
						case "ToggleSwitch":
							ToggleSwitch _ToogleControl = sender as ToggleSwitch;
							_SectionName = _ToogleControl.Properties.Name.Split(new char[] { '|' })[0];
							if (_ToogleControl.Properties.Tag != null)
							{
								_SelectedElement = _ToogleControl.Properties.Tag as TPMod_Key;
								_SelectedElement.value = Util.BoolToInteger(Util.ValidarNulo(_ToogleControl.EditValue, false)).ToString();
							}
							break;
						case "LookUpEdit":
							LookUpEdit _Combo = sender as LookUpEdit;
							_SectionName = _Combo.Properties.Name.Split(new char[] { '|' })[0];
							if (_Combo.Properties.Tag != null)
							{
								_SelectedElement = _Combo.Properties.Tag as TPMod_Key;
								_SelectedElement.value = Util.ValidarNulo(_Combo.EditValue, "");
							}
							break;
						case "TrackBarControl":
							TrackBarControl _TrackBar = sender as TrackBarControl;
							_SectionName = _TrackBar.Properties.Name.Split(new char[] { '|' })[0];
							if (_TrackBar.Properties.Tag != null)
							{
								_SelectedElement = _TrackBar.Properties.Tag as TPMod_Key;
								_SelectedElement.value = (Util.ValidarNulo(_TrackBar.EditValue, 0m) / 10m).ToString();
							}
							break;
						case "ColorPickEdit":
							ColorPickEdit _ColorEd = sender as ColorPickEdit;
							_SectionName = _ColorEd.Properties.Name.Split(new char[] { '|' })[0];
							if (_ColorEd.Properties.Tag != null)
							{
								_SelectedElement = _ColorEd.Properties.Tag as TPMod_Key;
								if (_ColorEd.EditValue != null)
								{
									_SelectedElement.value = _ColorEd.Color.ToArgb().ToString();
								}
							}
							break;
						default:
							break;
					}

					/* AQUI SE GUARDAN LOS CAMBIOS EN EL INI FILE */
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
				}));
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
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

		private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
		{
			/* AL SELECCIONAR UN TEMA DE LA LISTA  */
			DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
			if (view.IsRowSelected(e.RowHandle) && e.Column.FieldName == "Preview")
			{
				if (view.GetRow(e.RowHandle) is ui_preset_new _Theme)
				{
					//Cargar el Thema Seleccionado:
					if (MessageBox.Show("Do you want to Apply this Theme now?", "Apply Theme?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						string ThemePath = System.IO.Path.GetDirectoryName(_Theme.folder);
						string[] FileList = Directory.GetFiles(ThemePath, "*.*");
						if (FileList != null && FileList.Length > 0)
						{
							string ModFolder = System.IO.Path.GetDirectoryName(ModFullPath);

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
					string TPMods_Path = Path.Combine(ActiveInstance.path, @"EDHM-ini\3rdPartyMods");
					string ModFullPath = Path.Combine(TPMods_Path, CurrentdMod.file);

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

		private void cmdImportMod_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			ImportMod();
		}

		private void cmdEdit_RemoveMod_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (CurrentdMod != null)
				{
					if (XtraMessageBox.Show(string.Format("Are you sure of Deleting the MOD: '{0}'?", CurrentdMod.mod_name),
						"Confirm Deletion?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						if (!CurrentdMod.file_full.EmptyOrNull() && Directory.Exists(CurrentdMod.file_full))
						{
							string FileName = Path.GetFileNameWithoutExtension(Path.Combine(CurrentdMod.file_full, CurrentdMod.file)); //<- Nombre sin Extension ni Path

							if (File.Exists(Path.Combine(CurrentdMod.file_full, FileName + ".bat")))
							{
								/* usamos el BAT desinstalador */
								ExecuteBAT(Path.Combine(CurrentdMod.file_full, FileName + ".bat"), "-silent");
							}
							else
							{
								XtraMessageBox.Show(
									string.Format("This Mod doesnt have an Un-Installer.\r\nThen, it is possible that some files remain, like Shaders.\r\nIf you experience problems, contact this mod's author ({0}).\r\nNow We'r going to try to delete the files manually, hold on..", CurrentdMod.author),
									"WARNING!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								if (File.Exists(Path.Combine(CurrentdMod.file_full, CurrentdMod.file)))
								{
									var dir = new DirectoryInfo(CurrentdMod.file_full);

									foreach (var file in dir.EnumerateFiles(string.Format("{0}.*", CurrentdMod.file)))
									{
										file.Delete();
									}
								}
							}
						}

						LoadModList(); //<- Actualizar los cambios
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
					string FileName = Path.GetFileNameWithoutExtension(CurrentdMod.file); //<- Nombre sin Extension ni Path
					string ModFullPath = Path.Combine(TPMods_Path, FileName + ".json");

					if (CurrentdMod.managed)
					{
						if (File.Exists(ModFullPath))
						{
							System.Diagnostics.Process.Start(ModFullPath);
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
			if (CurrentdMod != null)
			{
				string ThemePath = Path.Combine(CurrentdMod.file_full, Path.GetFileNameWithoutExtension(CurrentdMod.file));
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

		#endregion


		private void cmdImportTheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			ImportTheme();
		}
	}
}
