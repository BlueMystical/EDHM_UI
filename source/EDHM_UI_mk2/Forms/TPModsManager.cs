using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraVerticalGrid.Rows;

using IniParser;
using IniParser.Model;

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
		//private FileIniDataParser IniReader = null; //<- Lector INI para el MOD actual
		//private IniData _IniData = null; //<- Datos del Archivo INI para el mod actual

		#endregion

		#region Constructores

		public TPModsManager(game_instance pActiveInstance)
		{
			InitializeComponent();
			this.ActiveInstance = pActiveInstance;
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
				this.Cursor = Cursors.WaitCursor;
				this.lst3PMods.BeginUpdate();
				this.lst3PMods.Elements.Clear();

				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
				var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
				{
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
					this.TPMods_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-ini\3rdPartyMods");

					if (Directory.Exists(this.TPMods_Path))
					{
						var Mods = new DirectoryInfo(this.TPMods_Path).GetFiles("*.ini");
						if (Mods != null && Mods.Length > 0)
						{
							this.TPMods = new List<TPMod_Config>(); //<- Lista de MODs detectados (Manejados e Independientes)

							foreach (FileInfo _File in Mods)
							{
								//Revisa si el MOD tiene un JSON acompañante:
								string FileName = System.IO.Path.GetFileNameWithoutExtension(_File.FullName); //<- Nombre sin Extension ni Path
								TPMod_Config _ManagedMod = null;
								//Image _Image = null;

								if (File.Exists(Path.Combine(this.TPMods_Path, FileName + ".json")))
								{
									#region Si hay JSON es un Mod Manejado:

									_ManagedMod = Util.DeSerialize_FromJSON<TPMod_Config>(
																			Path.Combine(this.TPMods_Path, FileName + ".json"));
									if (_ManagedMod != null)
									{
										_ManagedMod.managed = true;

										//Si Tiene una Imagen:
										if (File.Exists(Path.Combine(this.TPMods_Path, FileName + ".png")))
										{
											_ManagedMod.Thumbnail = Image.FromFile(Path.Combine(this.TPMods_Path, FileName + ".png"));
										}
										else
										{
											if (File.Exists(Path.Combine(this.AppExePath, @"Images\3PM_Default.png")))
											{
												_ManagedMod.Thumbnail = Image.FromFile(Path.Combine(this.AppExePath, @"Images\3PM_Default.png"));
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
										managed = false
									};

									_ManagedMod.types = new List<TPMod_Type>();
									_ManagedMod.types.Add(new TPMod_Type("text", "Text"));
									_ManagedMod.types.Add(new TPMod_Type("number", "Integer Number"));
									_ManagedMod.types.Add(new TPMod_Type("decimal", "Decimal Number"));
									_ManagedMod.types.Add(new TPMod_Type("color", "Color Value"));
									_ManagedMod.types.Add(new TPMod_Type("toggle", "ON/OFF Switch"));

									if (File.Exists(Path.Combine(this.AppExePath, @"Images\3PM_Default.png")))
									{
										_ManagedMod.Thumbnail = Image.FromFile(Path.Combine(this.AppExePath, @"Images\3PM_Default.png"));
									}

									/* Abre el INI y Carga sus Secciones y Claves  */
									if (File.Exists(_File.FullName))
									{
										var IniReader = new FileIniDataParser();
										IniData _IniData = IniReader.ReadFile(Path.Combine(this.TPMods_Path, FileName + ".ini"));

										if (IniReader != null && _IniData != null)
										{
											if (_IniData.Sections != null && _IniData.Sections.Count > 0)
											{
												_ManagedMod.sections = new List<TPMod_Section>();

												foreach (var _RawSection in _IniData.Sections)
												{
													TPMod_Section _Section = new TPMod_Section
													{
														name = _RawSection.SectionName,
														title = _RawSection.SectionName
													};

													var _RawKeys = _RawSection.Keys; //IniReader.GetAllKeys(_RawSection);
													if (_RawKeys != null && _RawKeys.Count > 0)
													{
														_Section.keys = new List<TPMod_Key>();
														foreach (var _Rawkey in _RawKeys)
														{
															StringBuilder _Comments = new StringBuilder();
															if (_Rawkey.Comments != null && _Rawkey.Comments.Count > 0)
															{
																foreach (string _Comment in _Rawkey.Comments)
																{
																	_Comments.AppendLine(_Comment);
																}
															}

															TPMod_Key _Key = new TPMod_Key
															{
																name = _Rawkey.KeyName,
																type = "text",
																key = _Rawkey.KeyName,
																description = _Comments.Length > 0 ? _Comments.ToString() : string.Empty,
																value = _Rawkey.Value
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

									this.TPMods.Add(_ManagedMod);

									Invoke((MethodInvoker)(() =>
									{
										SuperToolTipSetupArgs args = new SuperToolTipSetupArgs();
										args.Title.Text = string.Format("{0} ({1})", _ManagedMod.mod_name, _ManagedMod.managed ? "Managed" : "Unmanaged");
										args.Contents.Text = string.Format("    By {0}\r\n\r\nDescription:\r\n{1}", _ManagedMod.author, _ManagedMod.description);
										SuperToolTip sTooltip2 = new SuperToolTip();
										sTooltip2.Setup(args);

										AccordionControlElement _Element = new AccordionControlElement()
										{
											Expanded = false,
											Name = _ManagedMod.mod_name,
											Text = _ManagedMod.Thumbnail != null ? "" : _ManagedMod.mod_name,
											Style = ElementStyle.Item,
											SuperTip = sTooltip2,
											Tag = _ManagedMod,
											Image = _ManagedMod.Thumbnail
										};
										this.lst3PMods.Elements.Add(_Element);

									}));
								}
							}
						}
					}
					Invoke((MethodInvoker)(() =>
					{
						this.Cursor = Cursors.Default;
						this.lst3PMods.EndUpdate();

						if (this.TPMods.IsNotEmpty())
						{
							this.lblStatus.Caption = string.Format("{0} Mods detected.", this.TPMods.Count);
							this.SelectedThemeElement = this.lst3PMods.Elements[0];
							LoadMod(this.TPMods[0]);
						}
						else
						{
							this.lblStatus.Caption = "NO mods detected.";
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
					this.Cursor = Cursors.WaitCursor;
					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					var t = System.Threading.Tasks.Task.Factory.StartNew(delegate
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
						this.ModFullPath = Path.Combine(this.TPMods_Path, _Mod.file);

						if (File.Exists(this.ModFullPath))
						{
							this._IniReader_OLD = new IniFile(this.ModFullPath);
							//this.IniReader = new FileIniDataParser();
							//this._IniData = this.IniReader.ReadFile(this.ModFullPath);


							//if (this._IniData != null)
							if (this._IniReader_OLD != null)
							{
								this.vGridDetalles.BeginUpdate();
								this.vGridDetalles.Rows.Clear();

								if (_Mod.sections.IsNotEmpty())
								{
									foreach (var _Section in _Mod.sections)
									{
										if (_Section.keys.IsNotEmpty())
										{
											foreach (var _key in _Section.keys)
											{
												Console.WriteLine(string.Format("{0};{1};{2}", _key.key, _key.name, _Mod.file));
												_key.value = this._IniReader_OLD.ReadKey(_key.key, _Section.ini_section);

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

														//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
														//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

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

													case "decimal":
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

															//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
															//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

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

															//_KeyData = this._IniData.Sections[_Section.ini_section].GetKeyData(_key.key);
															//_key.value = _KeyData != null ? _KeyData.Value : _key.value;

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
																this._IniReader_OLD.ReadKey(_ColorKey, _Section.ini_section)));
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
													if (!(this.vGridDetalles.Rows[_Section.name] is CategoryRow Categoria))
													{
														Categoria = new CategoryRow(_Section.title)
														{
															Name = _Section.name,
															Tag = _Section
														};
														this.vGridDetalles.Rows.Add(Categoria);
													}
													Categoria.ChildRows.Add(_Fila);
												}
												else
												{
													this.vGridDetalles.Rows.Add(_Fila);
												}

												#endregion
											}
										}
									}
								}
							}
						}

						Invoke((MethodInvoker)(() =>
						{
							this.vGridDetalles.EndUpdate();

							this.lblMODdescription.Text = string.Format("<b>{0}</b>\r\n{1}",
								string.Format("<b>{0}</b> ( {1} )", _Mod.mod_name, _Mod.managed ? "<color=0, 255, 0>Managed</color>" : "<color=255, 0, 0>Unmanaged</color>"),
								string.Format("           By {0}\r\n\r\nDescription:\r\n<i>{1}</i>", _Mod.author, _Mod.description));

							this.CurrentdMod = _Mod;
							this.Cursor = Cursors.Default;
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
				string ModFullPath = Path.Combine(this.TPMods_Path, FileName + ".json");
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

						if (File.Exists(Path.Combine(this.TPMods_Path, FileName + ".png")) == false)
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
							ThumbNail.Save(Path.Combine(this.TPMods_Path, FileName + ".png"), System.Drawing.Imaging.ImageFormat.Png);
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
				if (_GammaComponents.Count > 3) alpha = _GammaComponents[3];

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
		//private void PropertyGrid_EditValueChanged(object sender, EventArgs e)
		//{
		//	try
		//	{
		//		TPMod_Key _SelectedElement = null;
		//		string _SectionName = string.Empty;
		//		string _ModFileName = string.Empty;

		//		if (this.CurrentdMod != null)
		//		{
		//			_ModFileName = this.CurrentdMod.file;
		//		}

		//		//Obtiene el Valor del Control adecuado
		//		switch (sender.GetType().Name)
		//		{
		//			case "TextEdit":
		//				TextEdit _TextControl = sender as TextEdit;
		//				_SectionName = _TextControl.Properties.Name.Split(new char[] { '|' })[0];
		//				if (_TextControl.Properties.Tag != null)
		//				{
		//					_SelectedElement = _TextControl.Properties.Tag as TPMod_Key;
		//					_SelectedElement.value = Util.ValidarNulo(_TextControl.EditValue, "");
		//				}
		//				break;

		//			case "SpinEdit":
		//				SpinEdit _NumericControl = sender as SpinEdit;
		//				_SectionName = _NumericControl.Properties.Name.Split(new char[] { '|' })[0];
		//				if (_NumericControl.Properties.Tag != null)
		//				{
		//					_SelectedElement = _NumericControl.Properties.Tag as TPMod_Key;
		//					_SelectedElement.value = Util.ValidarNulo(_NumericControl.EditValue, 0).ToString();
		//				}
		//				break;

		//			case "ToggleSwitch":
		//				ToggleSwitch _ToogleControl = sender as ToggleSwitch;
		//				_SectionName = _ToogleControl.Properties.Name.Split(new char[] { '|' })[0];
		//				if (_ToogleControl.Properties.Tag != null)
		//				{
		//					_SelectedElement = _ToogleControl.Properties.Tag as TPMod_Key;
		//					_SelectedElement.value = Util.BoolToInteger(Util.ValidarNulo(_ToogleControl.EditValue, false)).ToString();
		//				}
		//				break;

		//			case "LookUpEdit":
		//				LookUpEdit _Combo = sender as LookUpEdit;
		//				_SectionName = _Combo.Properties.Name.Split(new char[] { '|' })[0];
		//				if (_Combo.Properties.Tag != null)
		//				{
		//					_SelectedElement = _Combo.Properties.Tag as TPMod_Key;
		//					_SelectedElement.value = Util.ValidarNulo(_Combo.EditValue, "");
		//				}
		//				break;

		//			case "TrackBarControl":
		//				TrackBarControl _TrackBar = sender as TrackBarControl;
		//				_SectionName = _TrackBar.Properties.Name.Split(new char[] { '|' })[0];
		//				if (_TrackBar.Properties.Tag != null)
		//				{
		//					_SelectedElement = _TrackBar.Properties.Tag as TPMod_Key;
		//					_SelectedElement.value = (Util.ValidarNulo(_TrackBar.EditValue, 0m) / 10m).ToString();
		//				}
		//				break;

		//			case "ColorPickEdit":
		//				ColorPickEdit _ColorEd = sender as ColorPickEdit;
		//				_SectionName = _ColorEd.Properties.Name.Split(new char[] { '|' })[0];
		//				if (_ColorEd.Properties.Tag != null)
		//				{
		//					_SelectedElement = _ColorEd.Properties.Tag as TPMod_Key;
		//					if (_ColorEd.EditValue != null)
		//					{
		//						_SelectedElement.value = _ColorEd.Color.ToArgb().ToString();
		//					}
		//				}
		//				break;

		//			default:
		//				break;
		//		}


		//		/* AQUI SE GUARDAN LOS CAMBIOS EN EL INI FILE  */
		//		//if (this.IniReader != null && this._IniData != null)
		//		if (this._IniReader_OLD != null)
		//		{
		//			if (_SelectedElement.type != "color")
		//			{
		//				//Primero Leemos la Clave (con comentarios) desde el archivo:
		//				var _Data = this._IniData.Sections[_SectionName].GetKeyData(_SelectedElement.key);
		//				if (_Data != null)
		//				{
		//					_Data.Value = _SelectedElement.value;
		//				}
		//				else
		//				{
		//					//Si la clave no existe la creamos nueva:
		//					_Data = new KeyData(_SelectedElement.key)
		//					{
		//						Value = _SelectedElement.value
		//					};

		//					List<string> _comm = null; //Agregamos la Descripcion como Comentarios:
		//					if (!_SelectedElement.description.EmptyOrNull())
		//					{
		//						_comm = new List<string>(_SelectedElement.description.Split(new string[] { "\r\n" }, StringSplitOptions.None));
		//						if (_comm.IsNotEmpty())
		//						{
		//							_Data.Comments = new List<string>();
		//							foreach (string s in _comm)
		//							{
		//								_Data.Comments.Add("; " + s);
		//							}
		//						}
		//					}
		//				}

		//				this._IniData[_SectionName].SetKeyData(_Data);
		//				//this._IniReader_OLD.WriteKey(_SelectedElement.key, _SelectedElement.value, _SectionName);
		//			}
		//			else
		//			{
		//				//Los Colores tienen las claves RGBA en el campo key: 'xR|xG|xB|xA'
		//				string[] keys = _SelectedElement.key.Split(new char[] { '|' });
		//				if (keys != null && keys.Length > 0)
		//				{
		//					//Convertir Color RGB -> sRGB -> GammaCorrected
		//					Color _Color = Color.FromArgb(Convert.ToInt32(_SelectedElement.value));
		//					List<double> _GammaColors = GetGammaCorrected_RGBA(_Color);

		//					int i = 0;
		//					foreach (string _Key in keys)
		//					{
		//						/*//Primero Leemos la Clave (con comentarios) desde el archivo:
		//						var _Data = this._IniData[_SectionName].GetKeyData(_Key);
		//						if (_Data != null)
		//						{
		//							_Data.Value = _GammaColors[i].ToString();
		//						}
		//						else
		//						{
		//							//Si la clave no existe la creamos nueva:
		//							_Data = new KeyData(_Key)
		//							{
		//								Value = _GammaColors[i].ToString()
		//							};

		//							if (!_SelectedElement.description.EmptyOrNull())
		//							{
		//								_Data.Comments = new List<string>(_SelectedElement.description.Split(new string[] { "\r\n" }, StringSplitOptions.None));
		//							}
		//						}*/
		//						//this._IniData[_SectionName].SetKeyData(_Data);
		//						this._IniReader_OLD.WriteKey(_Key, _GammaColors[i].ToString(), _SectionName);
		//						//this._IniReader_OLD.WriteKey(_SelectedElement.key,  _SelectedElement.value, _SectionName);
		//						i++;
		//					}
		//				}
		//			}

		//			//this.IniReader.WriteFile(this.ModFullPath, this._IniData, Encoding.UTF8);
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//	}
		//}

		/* AQUI SE CAPTURAN LOS CAMBIOS HECHOS EN LOS CONTROLES del cuadro de Propiedades */
		private void PropertyGrid_EditValueChanged(object sender, EventArgs e)
		{
			try
			{
				TPMod_Key _SelectedElement = null;
				string _SectionName = string.Empty;
				string _ModFileName = string.Empty;

				if (this.CurrentdMod != null) _ModFileName = this.CurrentdMod.file;

				//Obtiene el Valor del Control adecuado
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
				if (this._IniReader_OLD != null)
				{
					if (_SelectedElement.type != "color")
					{
						//Primero Leemos la Clave (con comentarios) desde el archivo:
						//var _Data = this._IniData.Sections[_SectionName].GetKeyData(_SelectedElement.key);
						//if (_Data != null)
						//{
						//	_Data.Value = _SelectedElement.value;
						//}
						//else
						//{
						//	//Si la clave no existe la creamos nueva:
						//	_Data = new KeyData(_SelectedElement.key)
						//	{
						//		Value = _SelectedElement.value
						//	};
						//	List<string> _comm = null; //Agregamos la Descripcion como Comentarios:
						//	if (!_SelectedElement.description.EmptyOrNull())
						//	{
						//		_comm = new List<string>
						//		(_SelectedElement.description.Split(new string[] { "\r\n" }, StringSplitOptions.None));
						//		if (_comm.IsNotEmpty())
						//		{
						//			_Data.Comments = new List<string>();
						//			foreach (string s in _comm)
						//			{
						//				_Data.Comments.Add("; " + s);
						//			}
						//		}
						//	}
						//}
						//this._IniData[_SectionName].SetKeyData(_Data);
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
								//Primero Leemos la Clave (con comentarios) desde el archivo:
								//var _Data = this._IniData[_SectionName].GetKeyData(_Key);
								//if (_Data != null)
								//{
								//	_Data.Value = _GammaColors[i].ToString();
								//}
								//else
								//{
								//	//Si la clave no existe la creamos nueva:
								//	_Data = new KeyData(_Key)
								//	{
								//		Value = _GammaColors[i].ToString()
								//	};

								//	if (!_SelectedElement.description.EmptyOrNull())
								//	{
								//		_Data.Comments = new List<string>
								//		(_SelectedElement.description.Split(new string[] { "\r\n" }, StringSplitOptions.None));
								//	}
								//}
								//this._IniData[_SectionName].SetKeyData(_Data);
								_IniReader_OLD.WriteKey(_Key, _GammaColors[i].ToString(), _SectionName);
								i++;
							}
						}
					}
					//this.IniReader.WriteFile(this.ModFullPath, this._IniData, new UTF8Encoding(false)); //<- UTF8 NO BOM
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void vGridDetalles_FocusedRowChanged(object sender, DevExpress.XtraVerticalGrid.Events.FocusedRowChangedEventArgs e)
		{
			DevExpress.XtraVerticalGrid.PropertyGridControl pg = sender as DevExpress.XtraVerticalGrid.PropertyGridControl;
			// Do not display anything when a category row is selected.
			if (e.Row is CategoryRow)
			{
				Console.WriteLine(string.Format("Category '{0}' is Selected.", e.Row.Name));
				return;
			}
			//this.vGridDetalles.Tag = e.Row;
		}
		private void vGridDetalles_DoubleClick(object sender, EventArgs e)
		{
			var _Row = this.vGridDetalles.FocusedRow;
			if (_Row != null)
			{
				if (this.CurrentdMod != null)
				{
					if (_Row is CategoryRow Categoria)
					{
						//Edita la Seccion
						Dock_SectionEditor_Show(Categoria, "Edit Section:");
					}
					if (_Row is EditorRow _Fila)
					{
						if (_Fila.Tag != null && _Fila.Tag is TPMod_Key _Key)
						{
							//Edita la Clave:
							Dock_KeyEditor_Show(_Key, "Edit Key:");
						}
					}
				}
			}
		}

		/* AL SELECCIONAR UN MOD DE LA LISTA  */
		private AccordionControlElement SelectedThemeElement = null;
		private void lst3PMods_ElementClick(object sender, ElementClickEventArgs e)
		{
			if (sender != null)
			{
				if (e.Element != null && e.Element.Tag != null)
				{
					if (e.Element.Tag is TPMod_Config _Mod)
					{
						this.SelectedThemeElement = e.Element;
						LoadMod(_Mod);
					}
				}
			}
		}
		private void lst3PMods_CustomDrawElement(object sender, CustomDrawElementEventArgs e)
		{
			//Dibuja Borde sobre el tema seleccionado
			try
			{
				if (this.SelectedThemeElement != null)
				{
					//Si el elemento está seleccionado, Dibuja un borde Amarillo alrededor
					if (e.ObjectInfo.Element == this.SelectedThemeElement || this.SelectedThemeElement.OwnerElement == e.ObjectInfo.Element)
					{
						if (e.Element.Tag != null)
						{
							TPMod_Config _Theme = e.Element.Tag as TPMod_Config;
							Rectangle _Box = e.ObjectInfo.HeaderBounds; _Box.Width = 200;

							//Aqui dibuja la imagen normalmente:
							e.Cache.DrawImage(_Theme.Thumbnail, _Box);
						}

						//Aqui le dibuja un Borde Amarillo a la izquierda
						e.Cache.FillRectangle(Color.Orange, new Rectangle(e.ObjectInfo.HeaderBounds.Location, new Size(this.ScaleHelper.ScaleHorizontal(3),
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

		#endregion

		#region Botoneras

		private void cmdSaveChanges_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			this.DialogResult = DialogResult.OK;
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
				if (this.CurrentdMod != null)
				{
					string TPMods_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-ini\3rdPartyMods");
					string ModFullPath = Path.Combine(TPMods_Path, this.CurrentdMod.file);

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
					 * - Folder 'ShaderFixes'				<- Contains the Added/Modified Shader files.
					 * - Folder 'EDHM-ini\3rdPartyMods\'	<- Contains the Mod's INI files. */

					System.IO.FileInfo file = new System.IO.FileInfo(OFDialog.FileName);
					if (Util.DoNetZIP_UnCompressFile(OFDialog.FileName, this.ActiveInstance.path))
					{
						LoadModList();
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void cmdEdit_RemoveMod_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (this.CurrentdMod != null)
				{
					if (XtraMessageBox.Show(string.Format("Are you sure of Deleting the MOD: '{0}'?", this.CurrentdMod.mod_name),
						"Confirm Deletion?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						string TPMods_Path = Path.Combine(this.ActiveInstance.path, @"EDHM-ini\3rdPartyMods");
						string ModFullPath = Path.Combine(TPMods_Path, this.CurrentdMod.file);
						string FileName = Path.GetFileNameWithoutExtension(ModFullPath); //<- Nombre sin Extension ni Path

						if (File.Exists(Path.Combine(TPMods_Path, FileName + ".bat")))
						{
							/* usamos el BAT desinstalador */
							ExecuteBAT(Path.Combine(TPMods_Path, FileName + ".bat"), "-silent");
						}
						else
						{
							XtraMessageBox.Show("This Mod doesnt have an Un-Installer, so it is possible that some files remain, if you experience problems, contact this mod's author.",
								"WARNING!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							if (File.Exists(ModFullPath))
							{
								var dir = new DirectoryInfo(TPMods_Path);

								foreach (var file in dir.EnumerateFiles(string.Format("{0}.*", FileName)))
								{
									file.Delete();
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
				if (this.CurrentdMod != null)
				{
					string FileName = Path.GetFileNameWithoutExtension(this.CurrentdMod.file); //<- Nombre sin Extension ni Path
					string ModFullPath = Path.Combine(this.TPMods_Path, FileName + ".json");

					if (this.CurrentdMod.managed)
					{
						if (File.Exists(ModFullPath))
						{
							System.Diagnostics.Process.Start(ModFullPath);
						}
					}
					else
					{
						SaveMod(this.CurrentdMod);
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

		#endregion

		#region Editor de Secciones

		private void Dock_SectionEditor_Show(CategoryRow Categoria, string Caption)
		{
			try
			{
				this.DockSection_txtName.Text = Categoria.Name;
				this.DockSection_txtTitle.Text = Categoria.Properties.Caption;

				this.dockSection.Tag = Categoria;
				this.dockSection.Text = Caption;
				this.dockSection.Show();
				this.dockSection.Dock = DevExpress.XtraBars.Docking.DockingStyle.Float;

				this.dockSection.FloatLocation = new System.Drawing.Point((int)((Screen.PrimaryScreen.Bounds.Width - this.dockSection.Width) / 2),
								(int)((Screen.PrimaryScreen.Bounds.Height - this.dockSection.Height) / 2));
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdEdit_AddSection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (this.CurrentdMod != null)
				{
					// Mostrar el Panel en Modo Flotante Centrado en la Ventana:
					this.dockSection.Text = "Add New Section:";
					this.dockSection.Show();
					this.dockSection.Dock = DevExpress.XtraBars.Docking.DockingStyle.Float;

					this.dockSection.FloatLocation = new System.Drawing.Point((int)((Screen.PrimaryScreen.Bounds.Width - this.dockSection.Width) / 2),
									(int)((Screen.PrimaryScreen.Bounds.Height - this.dockSection.Height) / 2));

				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdEdit_EditSection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				var _Row = this.vGridDetalles.FocusedRow;
				if (_Row != null)
				{
					if (this.CurrentdMod != null)
					{
						if (_Row is CategoryRow Categoria)
						{
							//Edita la Seccion
							Dock_SectionEditor_Show(Categoria, "Edit Section:");
						}
						if (_Row is EditorRow _Fila)
						{
							if (_Fila.Tag != null && _Fila.Tag is TPMod_Key _Key)
							{
								//Edita la Clave:
								Dock_KeyEditor_Show(_Key, "Edit Key:");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdEdit_RemoveSection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (this.CurrentdMod != null)
				{
					if (this.vGridDetalles.FocusedRow is CategoryRow Categoria)
					{
						if (XtraMessageBox.Show(string.Format("Are you sure of Deleting the section '{0}' and ALL it's Keys and Values?", Categoria.Name),
							"Confirm Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							this.vGridDetalles.Rows.Remove(Categoria);
							if (Categoria.Tag is TPMod_Section _EditSection)
							{
								this.CurrentdMod.sections.Remove(_EditSection);
							}

							string ModFullPath = Path.Combine(this.TPMods_Path, this.CurrentdMod.file);
							string FileName = Path.GetFileNameWithoutExtension(this.CurrentdMod.file); //<- Nombre sin Extension ni Path
							ModFullPath = Path.Combine(this.TPMods_Path, FileName + ".json");

							Util.Serialize_ToJSON(ModFullPath, this.CurrentdMod);
						}
					}

				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void DockSection_cmdApply_Click(object sender, EventArgs e)
		{
			try
			{
				//Agrega la Categoria del Elemento, si ya existe, usa esa, sino, la crea nueva:
				if (!this.DockSection_txtName.Text.EmptyOrNull())
				{
					TPMod_Section _Section = new TPMod_Section
					{
						name = this.DockSection_txtName.Text,
						title = this.DockSection_txtTitle.Text
					};

					if (this.dockSection.Text == "Add New Section:")
					{
						//Si la Seccion NO Existe:
						if (!(this.vGridDetalles.Rows[_Section.name] is CategoryRow Categoria))
						{
							#region Agrega la Seccion a la Grilla

							Categoria = new CategoryRow(_Section.title)
							{
								Name = _Section.name
							};
							this.vGridDetalles.Rows.Add(Categoria);

							#endregion

							if (this.CurrentdMod != null)
							{
								string FileName = Path.GetFileNameWithoutExtension(this.CurrentdMod.file);

								this.CurrentdMod.sections.Add(_Section); //<- la agrega al Mod Actual

								#region Guarda los cambios en el JSON

								if (this.CurrentdMod.managed)
								{
									Util.Serialize_ToJSON(Path.Combine(this.TPMods_Path, FileName + ".json"), this.CurrentdMod);
								}

								#endregion

								#region Crear la Seccion en el INI 

								//this._IniData.Sections.Add(new SectionData(_Section.name));
								//this.IniReader.WriteFile(Path.Combine(this.TPMods_Path, FileName + ".ini"), this._IniData);

								#endregion

								//LoadModList(); //<- Refresh Mod List
							}

							this.dockSection.Close();
						}
						else
						{
							XtraMessageBox.Show(string.Format("The section '{0}' already exists!", _Section.name));
						}
					}
					if (this.dockSection.Text == "Edit Section:")
					{
						if (this.dockSection.Tag is CategoryRow _Categoria)
						{
							_Categoria.Name = _Section.name;
							_Categoria.Properties.Caption = _Section.title;

							if (_Categoria.Tag is TPMod_Section _EditSection)
							{
								_EditSection.name = _Section.name;
								_EditSection.title = _Section.title;
							}

							//TODO: Crear la Seccion en el INI y guardar los cambios en el JSON
							this.dockSection.Close();
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		#endregion

		#region Editor de claves

		private bool AddingNew_KEY = false;
		private void Dock_KeyEditor_Show(TPMod_Key _Key, string Caption)
		{
			try
			{
				if (this.CurrentdMod != null)
				{
					this.DockKeys_txtKey.Text = _Key.key;
					this.DockKeys_txtName.Text = _Key.name;
					this.DockKeys_txtValue.Text = _Key.value;
					this.DockKeys_cboType.EditValue = _Key.type;
					this.DockKeys_memoDescription.Text = _Key.description;

					List<TPMod_Type> _TypesList = new List<TPMod_Type>();
					_TypesList.AddRange(this.CurrentdMod.types);

					if (this.CurrentdMod.custom_types.IsNotEmpty())
					{
						List<TPMod_Type> Uniques = this.CurrentdMod.custom_types.GroupBy(x => x.type).Select(x => x.First()).ToList<TPMod_Type>();
						_TypesList.AddRange(Uniques);
					}

					this.DockKeys_cboType.Properties.ValueMember = "type";
					this.DockKeys_cboType.Properties.DisplayMember = "type";
					this.DockKeys_cboType.Properties.DataSource = _TypesList;
					this.DockKeys_cboType.Properties.BestFit();
					this.DockKeys_cboType.EditValue = _Key.type;

					this.dockKeys.Tag = _Key;
					this.dockKeys.Text = Caption;
					this.dockKeys.Show();
					this.dockKeys.Dock = DevExpress.XtraBars.Docking.DockingStyle.Float;

					this.dockKeys.FloatLocation = new Point((int)((Screen.PrimaryScreen.Bounds.Width - this.dockKeys.Width) / 2),
									(int)((Screen.PrimaryScreen.Bounds.Height - this.dockKeys.Height) / 2));
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdEdit_AddKey_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (this.CurrentdMod != null)
				{
					this.AddingNew_KEY = true;
					TPMod_Key _Key = new TPMod_Key();
					Dock_KeyEditor_Show(_Key, "Add New Key:");
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdEdit_EditKey_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				var _Row = this.vGridDetalles.FocusedRow;
				if (_Row != null)
				{
					if (this.CurrentdMod != null)
					{
						if (_Row is CategoryRow Categoria)
						{
							//Edita la Seccion
							Dock_SectionEditor_Show(Categoria, "Edit Section:");
						}
						if (_Row is EditorRow _Fila)
						{
							if (_Fila.Tag != null && _Fila.Tag is TPMod_Key _Key)
							{
								//Edita la Clave:
								Dock_KeyEditor_Show(_Key, "Edit Key:");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void cmdEdit_RemoveKey_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				var _Row = this.vGridDetalles.FocusedRow;
				if (_Row != null)
				{
					if (this.CurrentdMod != null)
					{
						if (_Row is EditorRow _Fila)
						{
							if (_Fila.Tag != null && _Fila.Tag is TPMod_Key _Key)
							{
								if (_Fila.ParentRow is CategoryRow _Parent)
								{
									if (_Parent.Tag is TPMod_Section _Section)
									{
										if (XtraMessageBox.Show(string.Format("Are you sure of Deleting the key '{0}'?", _Key.name),
											"Confirm Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
										{
											TPMod_Section _SEC = this.CurrentdMod.sections.Find(x => x.name == _Section.name);
											if (_SEC != null)
											{
												TPMod_Key _KAY = _SEC.keys.Find(x => x.name == _Key.name);
												_SEC.keys.Remove(_KAY);

												SaveMod(this.CurrentdMod);
												LoadMod(this.CurrentdMod);
											}
										}
									}
								}

							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void DockKeys_cmdApply_Click(object sender, EventArgs e)
		{
			try
			{
				var _Row = this.vGridDetalles.FocusedRow;
				if (_Row != null)
				{
					if (_Row is EditorRow _Fila)
					{
						if (this.CurrentdMod != null)
						{
							if (_Fila.ParentRow is CategoryRow _Parent)
							{
								if (_Parent.Tag is TPMod_Section _Section)
								{
									if (_Fila.Tag != null && _Fila.Tag is TPMod_Key _Key)
									{
										if (this.CurrentdMod.sections.IsNotEmpty())
										{
											TPMod_Section _SEC = this.CurrentdMod.sections.Find(x => x.name == _Section.name);
											if (_SEC != null)
											{
												//Revisa si la Clave Existe:
												TPMod_Key _KAY = _SEC.keys.Find(x => x.key == this.DockKeys_txtKey.Text);

												#region Prepara los Datos de la Clave

												if (this.AddingNew_KEY) //<- Modo Agregar Clave
												{
													if (_KAY != null)
													{
														//La Clave ya Existe!
														XtraMessageBox.Show(string.Format("The Key '{0}' already Exists!", _KAY.name), "Error");
														return;
													}
													else
													{
														_KAY = new TPMod_Key();
														_KAY.name = this.DockKeys_txtName.Text;
														_KAY.type = this.DockKeys_cboType.EditValue.NVL("");
														_KAY.key = this.DockKeys_txtKey.Text;
														_KAY.value = this.DockKeys_txtValue.Text;
														_KAY.description = this.DockKeys_memoDescription.Text;

														_SEC.keys.Add(_KAY); //<- agrega la nueva clave al JSON
													}
												}
												else //<- Modo Editar Clave
												{
													_KAY = _SEC.keys.Find(x => x.name == _Key.name);
													if (_KAY != null)
													{
														//Edita la Clave:													
														{
															_KAY.name = this.DockKeys_txtName.Text;
															_KAY.type = this.DockKeys_cboType.EditValue.NVL("");
															_KAY.key = this.DockKeys_txtKey.Text;
															_KAY.value = this.DockKeys_txtValue.Text;
															_KAY.description = this.DockKeys_memoDescription.Text;
														}
													}
													else
													{
														//La Clave NO existe!
														XtraMessageBox.Show(string.Format("The Key '{0}' does NOT Exists!", this.DockKeys_txtName.Text), "Error");
														return;
													}
												}

												#endregion

												#region Guarda la Clave en el INI

												/*bool EditINI = false;
												if (EditINI && this.IniReader != null && this._IniData != null)
												{
													var _Data = this._IniData.Sections[_SEC.name].GetKeyData(_Key.name);
													if (_Data != null)
													{
														_Data.KeyName = _KAY.name;
														_Data.Value = _KAY.value;
													}
													else
													{
														//Si la clave no existe la creamos nueva:
														_Data = new KeyData(_KAY.name)
														{
															Value = _KAY.value
														};
														List<string> _comm = null; //Agregamos la Descripcion como Comentarios:
														if (!_KAY.description.EmptyOrNull())
														{
															_comm = new List<string>(_KAY.description.Split(new string[] { "\r\n" }, StringSplitOptions.None));
															if (_comm.IsNotEmpty())
															{
																_Data.Comments = new List<string>();
																foreach (string s in _comm)
																{
																	_Data.Comments.Add("; " + s);
																}
															}
														}
													}

													this._IniData[_SEC.name].SetKeyData(_Data);
													this.IniReader.WriteFile(this.ModFullPath, this._IniData, Encoding.UTF8);
												}*/

												#endregion

												SaveMod(this.CurrentdMod);
												LoadMod(this.CurrentdMod);

												this.AddingNew_KEY = false;
												this.dockKeys.Hide();
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		#endregion

		#region Custom Types Editor

		private bool AddingNewType = false;
		private void LoadCustomTypes(TPMod_Config _Mod)
		{
			try
			{
				if (_Mod.custom_types.IsNotEmpty())
				{
					List<TPMod_Type> Uniques = _Mod.custom_types.GroupBy(x => x.type).Select(x => x.First()).ToList<TPMod_Type>();
					this.DockCustomTypes_lstGroupTypes.DataSource = Uniques;
					this.DockCustomTypes_lstGroupTypes.DisplayMember = "type";
					this.DockCustomTypes_lstGroupTypes.ValueMember = "type";
					this.DockCustomTypes_lstGroupTypes.SelectedValueChanged += (object _Sender, EventArgs _E) =>
					{
						if (this.DockCustomTypes_lstGroupTypes.DataSource != null)
						{
							if (this.DockCustomTypes_lstGroupTypes.SelectedItem is TPMod_Type _SelectedGroup)
							{
								this.AddingNewType = false;

								List<TPMod_Type> _GroupDetails = _Mod.custom_types.FindAll(x => x.type == _SelectedGroup.type);

								if (_GroupDetails.IsNotEmpty())
								{
									this.DockCustomTypes_lstTypes.DataSource = _GroupDetails;
									this.DockCustomTypes_lstTypes.DisplayMember = "name";
									this.DockCustomTypes_lstTypes.ValueMember = "name";
									this.DockCustomTypes_lstTypes.SelectedItem = _GroupDetails[0];
									this.DockCustomTypes_lstTypes.SelectedValueChanged += (object _Sender2, EventArgs _E2) =>
									{
										this.AddingNewType = false;

										if (this.DockCustomTypes_lstTypes.DataSource != null)
										{
											if (this.DockCustomTypes_lstTypes.SelectedItem is TPMod_Type _SelectedType)
											{
												this.DockCustomTypes_txtType.Text = _SelectedType.type;
												this.DockCustomTypes_txtName.Text = _SelectedType.name;
												this.DockCustomTypes_txtValue.Text = _SelectedType.value;
											}
										}
									};
								}
							}
						}
					};
				}
				else
				{
					this.DockCustomTypes_lstGroupTypes.DataSource = null;
					this.DockCustomTypes_lstTypes.DataSource = null;
					this.DockCustomTypes_txtType.Text = null;
					this.DockCustomTypes_txtName.Text = null;
					this.DockCustomTypes_txtValue.Text = null;
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void DockKeys_cmdCustomTypes_Click(object sender, EventArgs e)
		{
			try
			{
				if (this.CurrentdMod != null)
				{
					LoadCustomTypes(this.CurrentdMod);

					this.dockCustomTypesEditor.Text = "Custom Types Editor";
					this.dockCustomTypesEditor.Show();
					this.dockCustomTypesEditor.Dock = DevExpress.XtraBars.Docking.DockingStyle.Float;

					this.dockCustomTypesEditor.FloatLocation = new System.Drawing.Point((int)((Screen.PrimaryScreen.Bounds.Width - this.dockCustomTypesEditor.Width) / 2),
									(int)((Screen.PrimaryScreen.Bounds.Height - this.dockCustomTypesEditor.Height) / 2));
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}

		private void DockCustomTypes_cmdAddNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			this.AddingNewType = true;
			this.DockCustomTypes_txtType.Text = null;
			this.DockCustomTypes_txtName.Text = null;
			this.DockCustomTypes_txtValue.Text = null;

			this.DockCustomTypes_txtType.Focus();
		}
		private void DockCustomTypes_cmdRemove_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			try
			{
				if (this.CurrentdMod != null)
				{
					if (this.CurrentdMod.custom_types.IsNotEmpty())
					{
						if (this.DockCustomTypes_lstTypes.SelectedItem is TPMod_Type _SelectedType)
						{
							if (XtraMessageBox.Show(string.Format("Are you sure to delete the type '{0}'?", _SelectedType.name),
								"Confirm Deletion?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
							{
								this.CurrentdMod.custom_types.Remove(_SelectedType);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void DockCustomTypes_cmdEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			//EL NOMBRE ESTA MAL, EN REALIDAD AQUI SE GUARDAN LOS CAMBIOS
			try
			{
				if (this.CurrentdMod != null)
				{
					if (this.AddingNewType) /* <- AGREGANDO UN NUEVO TIPO */
					{
						TPMod_Type _SelectedType = new TPMod_Type
						{
							name = this.DockCustomTypes_txtName.Text,
							type = this.DockCustomTypes_txtType.Text,
							value = this.DockCustomTypes_txtValue.Text
						};
						if (this.CurrentdMod.custom_types == null) this.CurrentdMod.custom_types = new List<TPMod_Type>();
						var _ExistingType = this.CurrentdMod.custom_types.Find(x => x.type == _SelectedType.type && x.name == _SelectedType.name);
						if (_ExistingType == null)
						{
							this.CurrentdMod.custom_types.Add(_SelectedType);

							LoadCustomTypes(this.CurrentdMod);
						}
					}
					else //<- EDITANDO UN TIPO EXISTENTE:
					{
						if (this.CurrentdMod.custom_types.IsNotEmpty())
						{
							if (this.DockCustomTypes_lstTypes.SelectedItem is TPMod_Type _SelectedType)
							{
								_SelectedType.name = this.DockCustomTypes_txtName.Text;
								_SelectedType.type = this.DockCustomTypes_txtType.Text;
								_SelectedType.value = this.DockCustomTypes_txtValue.Text;

								this.DockCustomTypes_lstTypes.Focus();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message + ex.StackTrace);
			}
		}
		private void DockCustomTypes_CmdApply_Click(object sender, EventArgs e)
		{
			/* APLICA LOS CAMBIOS AL MOD */
			if (this.CurrentdMod != null)
			{
				SaveMod(this.CurrentdMod);

				this.dockCustomTypesEditor.Close();
				this.dockKeys.Hide();

				LoadMod(this.CurrentdMod);
				LoadCustomTypes(this.CurrentdMod);

			}
		}

		#endregion
	}
}