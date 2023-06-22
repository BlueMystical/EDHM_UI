using DevExpress.Utils;
using DevExpress.Utils.Extensions;
using DevExpress.Utils.Html;
using DevExpress.XtraBars.Alerter;
using DevExpress.XtraBars.Docking2010.Customization;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ButtonsPanelControl;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EDHM_UI_mk2
{
	/* MODO DE USO:
	-------------------------------------------------------------------------------
	AlertMessage.ShowAlert(this.ParentForm, AlertMessage.TipoAlerta.Advertencia,
		"Hay Errores que debe corregir!", "Arreglélos o Cancele los Cambios.");

	-------------------------------------------------------------------------------
	//Para Ajustar la Posicion de la Ventana:
	 AlertMessage.ShowAlert(this.ParentForm, AlertMessage.TipoAlerta.Advertencia,
		"Hay Errores que debe corregir!", "Arreglélos o Cancele los Cambios.", 
		new Point(
			this.PointToScreen(Point.Empty).X - 40,
			this.PointToScreen(Point.Empty).Y + this.Height + 10
	)); 
	-------------------------------------------------------------------------------
	//Documentación:
	https://docs.devexpress.com/WindowsForms/403775/controls-and-libraries/messages-notifications-and-dialogs/alert-windows/alert-windows-with-html-templates
	-------------------------------------------------------------------------------
	 */
	/// <summary>Muestra Mensajes de Alerta con estilo HTML y CSS.
	/// <para>Autor: Jhollman Chacon - 2023</para></summary>
	public static class Mensajero
	{
		public enum TipoAlerta
		{
			Info,
			Exito,
			Question,
			Advertencia,
			Error
		}

		/* ----------------------- EVENTOS ------------------------------------------------------------------------------------------------------------  */

		/// <summary>Manejador de Eventos para los Click en Botones</summary>
		private static Action<object, MensajeroEventArgs> ButtonClickHandler;

		/// <summary>Evento que ocurre al dar click en un boton del Dialogo o Panel:
		/// <para>'Sender' es la instancia del Mensaje, puede ser del tipo 'AlertControl' o 'FlyoutPanel'</para>
		/// <para>'e' contiene datos del botón clickeado</para></summary>
		public static event Action<object, MensajeroEventArgs> ButtonClick
		{
			add => ButtonClickHandler += value;
			remove => ButtonClickHandler -= value;
		}
		/// <summary>Previene multiples invocaciones entre llamadas a la misma instancia del evento</summary>
		public static void ResetEvents()
		{
			ButtonClickHandler = null;
		}

		/* -----------------------ALERTAS------------------------------------------------------------------------------------------------------------  */
		/*  MODO DE USO:
		 *  Mensajero.ShowAlert(this, Mensajero.TipoAlerta.Exito, "Operación Exitosa!", "Los Cambios han sido guardado correctamente.");
		 */

		/// <summary>Muestra Mensajes de Alerta con estilo HTML.</summary>
		/// <param name="pOwner">[Requerido] Formulario Padre para esta Ventana</param>
		/// <param name="pTipoAlerta">Determina el Color y Estilo de la Alerta</param>
		/// <param name="pTitulo">Texto para el Titulo</param>
		/// <param name="pMensaje">Texto del Mensaje</param>
		/// <param name="DarkMode">[Opcional] Usar Tema Oscuro, Defecto = false</param>
		/// <param name="AutoCloseTime">[Opcional] Tiempo (milisegundos) que Espera antes de Cerrarse, defecto 7s. 0=NoAutoClose</param>
		/// <param name="Location">[Opcional] Si se pasa 'Point.Empty' La Alerta se muestra en la Esquina Inferior Derecha de la Pantalla,
		/// De lo contrario se muestra en las coordenadas indicadas. Usar 'PointToScreen' para coordenadas correctas.</param>
		public static void ShowAlert(Form pOwner, TipoAlerta pTipoAlerta, string pTitulo, string pMensaje, bool DarkMode = false, int AutoCloseTime = 7000, Point Location = default(Point))
		{
			try
			{
				AlertControl Alerta = new AlertControl()
				{
					ShowPinButton = false,
					HtmlImages = GetSvgImages(pTipoAlerta),
					FormLocation = AlertFormLocation.BottomRight,
					FormShowingEffect = AlertFormShowingEffect.SlideVertical,
					AutoFormDelay = AutoCloseTime,   //<- Cierra automaticamente tras 7 segundos
					FormMaxCount = 4                //<- The number of simultaneously displayed alert windows
				};
				Alerta.HtmlTemplate.Assign(GetTemplateAlert(pTipoAlerta, DarkMode));
				if (Location != Point.Empty)
				{
					Alerta.BeforeFormShow += (object _Sender, AlertFormEventArgs _E) =>
					{
						//Si se pasa una Ubicacion, movemos la Alerta al lugar indicado:
						if (Location != Point.Empty) _E.Location = Location;

						//No Cerrar Automaticamente:
						_E.HtmlPopup.Pinned = (AutoCloseTime <= 0);
					};
				}

				Alerta.Show(pOwner, new AlertInfo(pTitulo, pMensaje, true));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/* ----------------------NOTIFICACIONES-------------------------------------------------------------------------------------------------------------  */
		/*  MODO DE USO:  
		 *  //[OPCIONAL] Enlazar al evento para saber que boton se presiona:
			Mensajero.MensajeroButtonClick += (object _Sender, MensajeroEventArgs _E) => {
				var control = _Sender as DevExpress.XtraBars.Alerter.AlertControl;
				if (_E.EditValue.ToString() == "okButton")
				{
					//Hacer algo aqui
				}
			};
			Mensajero.ShowMessage(this, Mensajero.TipoAlerta.Advertencia,
				"Hay Errores que debe corregir!", "Arreglélos o Cancele los Cambios.");
		 *  
		 */

		/// <summary>Muestra Mensajes de Notificacion con estilo HTML.
		/// <para>[Opcional] Puede enlazar al evento 'MensajeroButtonClick' para saber cuando se presiona el boton del dialogo.</para></summary>
		/// <param name="pOwner">[Requerido] Formulario Padre para esta Ventana</param>
		/// <param name="pTipoAlerta">Determina el Color y Estilo de la Alerta</param>
		/// <param name="pTitulo">Texto para el Titulo</param>
		/// <param name="pMensaje">Texto del Mensaje</param>
		/// <param name="DarkMode">[Opcional] Usar Tema Oscuro, Defecto = false</param>
		/// <param name="AutoCloseTime">[Opcional] Tiempo (milisegundos) que Espera antes de Cerrarse, defecto 7s. 0=NoAutoClose</param>
		/// <param name="Location">[Opcional] Si se pasa 'Point.Empty' La Alerta se muestra en la Esquina Inferior Derecha de la Pantalla,
		/// De lo contrario se muestra en las coordenadas indicadas. Usar 'PointToScreen' para coordenadas correctas.</param>
		public static void ShowNotification(Form pOwner, TipoAlerta pTipoAlerta, string pTitulo, string pMensaje, bool DarkMode = false, int AutoCloseTime = 7000, Point Location = default(Point))
		{
			try
			{
				AlertControl Alerta = new AlertControl()
				{
					ShowPinButton = false,
					HtmlImages = GetSvgImages(pTipoAlerta),
					FormLocation = AlertFormLocation.BottomRight,
					FormShowingEffect = AlertFormShowingEffect.FadeIn,
					AutoFormDelay = AutoCloseTime,   //<- Cierra automaticamente tras 7 segundos
				};
				Alerta.HtmlElementMouseClick += (object sender, AlertHtmlElementMouseEventArgs e) =>
				{
					//El Usuario debe enlazar al evento para saber que boton se presinó
					ButtonClickHandler?.Invoke(Alerta, new MensajeroEventArgs(e.ElementId)); //<- Dispara el Evento				
					e.HtmlPopup.Close(); //<- Cierra el Cuadro
					ResetEvents(); //<- Previene multiples llamadas 
				};
				Alerta.HtmlTemplate.Assign(GetTemplateMessage(pTipoAlerta, DarkMode));
				if (Location != Point.Empty)
				{
					Alerta.BeforeFormShow += (object _Sender, AlertFormEventArgs _E) =>
					{
						//Si se pasa una Ubicacion, movemos la Alerta al lugar indicado:
						if (Location != Point.Empty) _E.Location = Location;

						//No Cerrar Automaticamente:
						_E.HtmlPopup.Pinned = (AutoCloseTime <= 0);
					};
				}

				Alerta.Show(pOwner, new AlertInfo(pTitulo, pMensaje, true));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>Muestra Mensajes de Notificacion con estilo HTML.</summary>
		/// <param name="pOwner">Formulario Padre para esta Ventana</param>
		/// <param name="pTitulo">Texto para el Titulo</param>
		/// <param name="pMensaje">Texto del Mensaje</param>
		/// <param name="DarkMode">[Opcional] Usar Tema Oscuro, Defecto = false</param>
		public static void ShowNotification(Form pOwner, string pTitulo, string pMensaje, bool DarkMode = true)
		{
			int AlertaWidth = 600;
			int AlertaHeight = 400;

			//Mostrar Centrada en la Pantalla
			Point Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - AlertaWidth) / 2,
									   (Screen.PrimaryScreen.WorkingArea.Height - AlertaHeight) / 2);

			ShowNotification(pOwner, TipoAlerta.Exito, pTitulo, pMensaje, DarkMode);
		}

		/* ---------------------MessageBox--------------------------------------------------------------------------------------------------------------  */
		/* MODO DE USO:
		 * Exactamente igual que un MessageBox normal 
		 */

		/// <summary>Muestra un Dialogo MessageBox con estilo HTML.</summary>
		/// <param name="pTitulo">Texto para el Titulo</param>
		/// <param name="pMensaje">Texto del Mensaje</param>
		/// <param name="pButtons">[Opcional] Boton(es) mostrados en el Mensaje. Determina el valor de Retorno.</param>
		/// <param name="pIcon">[Opcional] Icono del Mensaje</param>
		/// <param name="DarkMode">[Opcional] Usar Tema Oscuro, Defecto = false</param>
		/// <param name="AutoCloseTime">[Opcional] Tiempo (milisegundos) que Espera antes de Cerrarse, defecto 20s. 0=NoAutoClose</param>
		public static DialogResult ShowMessage(string pTitulo, string pMensaje, MessageBoxButtons pButtons = MessageBoxButtons.OK, MessageBoxIcon pIcon = MessageBoxIcon.None, bool DarkMode = false, int AutoCloseTime = 0, string Language = "en")
		{
			XtraMessageBoxArgs msgArgs = new XtraMessageBoxArgs()
			{
				Caption = pTitulo,
				Text = pMensaje,

				AllowHtmlText = DefaultBoolean.True,
				AutoCloseOptions = new AutoCloseOptions()
				{
					Delay = AutoCloseTime,
					ShowTimerOnDefaultButton = false
				}
			};

			msgArgs.HtmlTemplate.Assign( GetTemplateDialog(pButtons, pIcon, DarkMode, Language) );
			msgArgs.ImageOptions.MessageBoxIcon = pIcon;
			//msgArgs.ImageOptions.Image = SetDialogImage(pIcon);
			msgArgs.DefaultButtonIndex = 2;

			return XtraMessageBox.Show(msgArgs);
		}


		/// <summary>Muestra un Cuadro de Dialogo con estilo HTML.</summary>
		/// <param name="pTitulo">Texto para el Titulo</param>
		/// <param name="pMensaje">Texto del Mensaje</param>
		/// <param name="pButtons">[Opcional] Boton(es) mostrados en el Mensaje. Determina el valor de Retorno.</param>
		/// <param name="pIcon">[Opcional] Icono del Mensaje</param>
		public static DialogResult ShowMessageDark(string pTitulo, string pMensaje, MessageBoxButtons pButtons = MessageBoxButtons.OK, MessageBoxIcon pIcon = MessageBoxIcon.None, string Language = "en")
		{
			return ShowMessage(pTitulo, pMensaje, pButtons, pIcon, true, Language: Language);
		}


		/* ---------------------XtraDialog--------------------------------------------------------------------------------------------------------------  */
		/* MODO DE USO:
		 * 
			//DataSource para el Combo:
			List<MyClass> _DataSource = new List<MyClass>
			{
				new MyClass() { codigo = 1, descripcion = "Uno" },
				new MyClass() { codigo = 2, descripcion = "Dos" },
				new MyClass() { codigo = 3, descripcion = "Tres" }
			};

			//Controles a Mostrar en el Form:
			List<MensajeroControl> pContent = new List<MensajeroControl>()
			{
				new MensajeroControl() { Caption = "Nombre:", EditValue = "Jhollman Chacon" },
				new MensajeroControl() { Caption = "Fecha Desde:", Type = typeof(DateTime), EditValue = DateTime.Today },
				new MensajeroControl() { Caption = "Fecha Hasta:", Type = typeof(DateTime), EditValue = DateTime.Today.AddDays(20) },
				new MensajeroControl() { Caption = "Es Verdad?",   Type = typeof(bool),     EditValue = true },
			};

			//Si un control necesita tratamiento especial se puede hacer aparte:
			var MyCombo = new MensajeroControl()
			{
				Caption = "Elija Uno:", Type = typeof(MyClass), EditValue = 2, DataSource = _DataSource, 
				ValueMember = "codigo", DisplayMember = "descripcion"
			};
			//Se puede reaccionar en tiempo real al Cambio del EditValue, para hacer Validaciones x ejemplo:
			MyCombo.EditValueChanged += (object _Sender, MensajeroControlEventArgs _E) =>
			{
				//Hacer algo aqui cuando el Valor de este control Cambia
				if (Convert.ToInt32(_E.EditValue) != 1)
				{
					MyCombo.Validate("Le dije que elijiera Uno!");
				}
				else
				{
					MyCombo.Validate(""); //<- Elimina el Error
					Mensajero.ShowMessage(this, "EditValueChanged", string.Format("Valor Cambiado!: {0}", _E.EditValue));
				}	
			  //NOTA: Si tiene Errores no deja Cerrar el Dialogo, a menos que presione 'Cancelar'
			};
			pContent.Add(MyCombo);

			// AQUI SE LLAMA AL DIALOGO:
			if (Mensajero.ShowForm(ref pContent, "Hola Mundo!", MessageBoxButtons.OKCancel, Size: new Size(300, 170) ) == DialogResult.OK)
			{
				//Hacer algo aqui
				Console.WriteLine(pContent[1].EditValue.ToString()); //<- Datos Modificados x el Usuario
			}
		 */

		/// <summary>Muestra un Formulario de Dialogo con Contenido, Sin Estilos.</summary>		
		/// <param name="Content">Controles a Mostrar deltro del formulario</param>
		/// <param name="Titulo">Titulo de la Ventana</param>
		/// <param name="Buttons">[Opcional] Botones, determina el 'DialogResult'</param>
		/// <param name="Size">[Opcional] Tamaño del Contenido</param>
		public static DialogResult ShowForm(ref List<MensajeroControl> Content, string Titulo, MessageBoxButtons Buttons = MessageBoxButtons.OKCancel)
		{
			XtraUserControl Contenido = GetDialogContent(Content, false);
			XtraDialogArgs msArgs = new XtraDialogArgs
			{
				Caption = Titulo,
				AllowHtmlText = DefaultBoolean.True,
				Content = Contenido,
				ContentPadding = new Padding(4, 4, 4, 10),
				ButtonAlignment = HorzAlignment.Far,

			};
			//UserLookAndFeel lookAndFeelError = new UserLookAndFeel(this);
			//lookAndFeelError.SkinName = "MyCustomSkin";
			//lookAndFeelError.Style = LookAndFeelStyle.Skin;
			//lookAndFeelError.UseDefaultLookAndFeel = false;
			XtraDialog.AllowCustomLookAndFeel = true;

			switch (Buttons)
			{
				case MessageBoxButtons.OK:
					msArgs.Buttons = new DialogResult[] { DialogResult.OK };
					msArgs.DefaultButtonIndex = 0;
					break;
				case MessageBoxButtons.OKCancel:
					msArgs.Buttons = new DialogResult[] { DialogResult.Cancel, DialogResult.OK };
					msArgs.DefaultButtonIndex = 1;
					break;

				case MessageBoxButtons.YesNoCancel:
					msArgs.Buttons = new DialogResult[] { DialogResult.Cancel, DialogResult.No, DialogResult.Yes };
					msArgs.DefaultButtonIndex = 2;
					break;
				case MessageBoxButtons.YesNo:
					msArgs.Buttons = new DialogResult[] { DialogResult.No, DialogResult.Yes };
					msArgs.DefaultButtonIndex = 1;
					break;

				case MessageBoxButtons.RetryCancel:
					msArgs.Buttons = new DialogResult[] { DialogResult.Cancel, DialogResult.Retry };
					msArgs.DefaultButtonIndex = 1;
					break;
				case MessageBoxButtons.AbortRetryIgnore:
					msArgs.Buttons = new DialogResult[] { DialogResult.Abort, DialogResult.Ignore, DialogResult.Retry };
					msArgs.DefaultButtonIndex = 2;
					break;
				default:
					msArgs.Buttons = new DialogResult[] { DialogResult.None };
					break;
			}
			msArgs.Showing += (object Sender, XtraMessageShowingArgs E) =>
			{
				E.MessageBoxForm.Height = Contenido.Height + 42;

				//Los botones son del tipo 'SimpleButton':
				//SimpleButton btn = E.Buttons[System.Windows.Forms.DialogResult.OK] as SimpleButton;

				try
				{
					E.Buttons[DialogResult.Cancel].Text = "Cancelar";
					E.Buttons[DialogResult.Cancel].PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
					E.MessageBoxForm.CancelButton = E.Buttons[DialogResult.Cancel];

					E.Buttons[DialogResult.OK].ShowFocusRectangle = DefaultBoolean.False;
					E.Buttons[DialogResult.OK].Appearance.BackColor = Color.DodgerBlue;
					E.Buttons[DialogResult.OK].Text = "Aceptar";
					E.MessageBoxForm.AcceptButton = E.Buttons[DialogResult.OK];
				}
				catch { }

				try
				{
					E.Buttons[DialogResult.No].Text = "No";
					E.Buttons[DialogResult.No].PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;

					E.Buttons[DialogResult.Yes].ShowFocusRectangle = DefaultBoolean.False;
					E.Buttons[DialogResult.Yes].Appearance.BackColor = Color.DodgerBlue;
					E.Buttons[DialogResult.Yes].Text = "Si";
				}
				catch { }

				try
				{
					E.Buttons[DialogResult.Retry].ShowFocusRectangle = DefaultBoolean.False;
					E.Buttons[DialogResult.Retry].Appearance.BackColor = Color.DodgerBlue;
					E.Buttons[DialogResult.Retry].Text = "Reintentar";
					E.Buttons[DialogResult.Abort].Text = "Abortar";
					E.Buttons[DialogResult.Ignore].Text = "Ignorar";
				}
				catch { }

				//** Prevenir el Cierre del Dialogo si tiene Errores:
				E.Form.FormClosing += (object sender, FormClosingEventArgs e) =>
				{
					bool _ret = false;

					/* Si presiona Cancelar no valida nada  */
					if (E.Form.DialogResult != DialogResult.Cancel)
					{
						if (Contenido != null && Contenido.Controls.Count > 0)
						{
							LayoutControl lc = Contenido.Controls[0] as LayoutControl;
							foreach (var control in lc.Controls)
							{
								if (control.GetType().ToString().Contains("DevExpress.XtraEditors"))
								{
									var Ctrl = (BaseEdit)control;
									if (Ctrl.ErrorText != string.Empty) _ret = true;
								}
							}
							e.Cancel = _ret;
						}
					}
				};
			};

			return XtraDialog.Show(msArgs);
		}


		/// <summary>Muestra un Formulario de Dialogo con Contenido, usa estilo HTML.</summary>		
		/// <param name="Content">Controles a Mostrar deltro del formulario</param>
		/// <param name="Titulo">Titulo de la Ventana</param>
		/// <param name="Buttons">[Opcional] Botones, determina el 'DialogResult'</param>
		/// <param name="DarkMode">[Opcional] Si es true usa un tema oscuro.</param>
		/// <param name="Size">[Opcional] Tamaño del Contenido</param>
		public static DialogResult ShowFormHTML(ref List<MensajeroControl> Content, string Titulo, MessageBoxButtons Buttons = MessageBoxButtons.OKCancel, bool DarkMode = false)
		{
			XtraUserControl Contenido = GetDialogContent(Content, DarkMode);
			XtraDialogArgs msArgs = new XtraDialogArgs()
			{
				Caption = Titulo,
				AllowHtmlText = DefaultBoolean.True,
				HtmlImages = GetSvgImages(TipoAlerta.Info),
				Content = Contenido,
				Buttons = new DialogResult[] { DialogResult.Cancel, DialogResult.Yes }
			};
			msArgs.HtmlTemplate.Assign(GetTemplateXtraDialog(Buttons, DarkMode));
			msArgs.DefaultButtonIndex = 1;
			msArgs.Showing += (object Sender, XtraMessageShowingArgs E) =>
			{
				//** Prevenir el Cierre del Dialogo si tiene Errores:
				E.MessageBoxForm.FormClosing += (object sender, FormClosingEventArgs e) =>
				{
					bool _ret = false;

					/* Si presiona Cancelar no valida nada  */
					if (e.CloseReason != CloseReason.UserClosing)
					{
						if (Contenido != null && Contenido.Controls.Count > 0)
						{
							LayoutControl lc = Contenido.Controls[0] as LayoutControl;
							foreach (var control in lc.Controls)
							{
								if (control.GetType().ToString().Contains("DevExpress.XtraEditors"))
								{
									var Ctrl = (BaseEdit)control;
									if (Ctrl.ErrorText != string.Empty) _ret = true;
								}
							}
							e.Cancel = _ret;
						}
					}
				};
			};
			//msArgs.HtmlElementMouseClick

			return XtraDialog.Show(msArgs);
		}

		public static DialogResult ShowFormHTML(XtraUserControl Contenido, string Titulo, MessageBoxButtons Buttons = MessageBoxButtons.OKCancel, bool DarkMode = false)
		{
			XtraDialogArgs msArgs = new XtraDialogArgs()
			{
				Caption = Titulo,
				Content = Contenido,
				DefaultButtonIndex = 2,
				AllowHtmlText = DefaultBoolean.True,
				HtmlImages = GetSvgImages(TipoAlerta.Info),				
				Buttons = new DialogResult[] { DialogResult.Cancel, DialogResult.OK }
			};

			msArgs.HtmlTemplate.Assign(GetTemplateXtraDialog(Buttons, DarkMode));			

			MensajeroColors colors = new MensajeroColors(DarkMode);
			Contenido.BackColor = colors.WindowBackColor;
			Contenido.ForeColor = colors.WindowForeColor;
			
			return XtraDialog.Show(msArgs);
		}


		/*  MODO DE USO:
		 *  -------------------------------------------------------------------
			var handle = Mensajero.ShowOverlayForm(this);
			var t = Task.Factory.StartNew(delegate
			{
				try
				{
					Thread.Sleep(5000);  //<- Hacer algo aqui
				}
				catch (ThreadAbortException) { Thread.CurrentThread.Join(); }
				catch (Exception) { }
				finally
				{
					Invoke((MethodInvoker)(() => handle.Close() ));
				}
			});
		* -------------------------------------------------------------------
			// Tambien se puede usar sin Procesos:
            using (var handle = Mensajero.ShowOverlayForm(this))
            {				
				Thread.Sleep(5000); //<- Hacer algo aqui
			}
		* ------------------------------------------------------------------- */
		/// <summary>Cubre la Ventana del 'pOwner' con una sombra mostrando una animacion de progreso.</summary>
		/// <param name="pOwner">Formulario dueño de esta ventana</param>
		public static IOverlaySplashScreenHandle ShowOverlayForm(Form pOwner)
		{
			OverlayWindowOptions options = new OverlayWindowOptions(
				backColor: Color.Black,
				opacity: 0.8,
				fadeIn: true,
				fadeOut: true,
				imageSize: new Size(64, 64)
			);
			return SplashScreenManager.ShowOverlayForm(pOwner, options);
		}

		/* ---------------------FlyoutDialog--------------------------------------------------------------------------------------------------------------  */
		/*	MODO DE USO:
		 *		//eSTOS SON LOS cONTROLES QUE SE MUESTRAN:
				List<MensajeroControl> pContent = new List<MensajeroControl>()
					{
						new MensajeroControl() { Caption = "Nombre:", EditValue = "Jhollman Chacon" },
						new MensajeroControl() { Caption = "Fecha Desde:", Type = typeof(DateTime), EditValue = DateTime.Today },
						new MensajeroControl() { Caption = "Fecha Hasta:", Type = typeof(DateTime), EditValue = DateTime.Today.AddDays(20) },
					};

				//Se puede reaccionar al Cambio del EditValue, para hacer Validaciones x ejemplo:
				var checkMark = new MensajeroControl() { Caption = "Es Verdad?", Type = typeof(bool), EditValue = false };
				checkMark.EditValueChanged += (object _Sender, MensajeroControlEventArgs _E) =>
				{
						//Hacer algo aqui cuando el Valor de este control Cambia
						if (Convert.ToBoolean(_E.EditValue) != true) { checkMark.Validate("No es Cierto!"); }
						else
						{
							checkMark.Validate(""); //<- Elimina el Error
									Mensajero.ShowMessage(this, "EditValueChanged", string.Format("Valor Cambiado!: {0}", _E.EditValue));
						}
				};
				pContent.Add(checkMark);

				if (Mensajero.ShowFlyoutDialog(this, "Hola Mundo!", ref pContent, MessageBoxButtons.OKCancel) == DialogResult.OK)
				{
					Mensajero.ShowMessage(this, Mensajero.TipoAlerta.Exito, "Exito!",
						pContent[1].EditValue.ToString() //<- Datos Modificados x el Usuario
					);
				}
		*/

		/// <summary>Muestra un Mensaje cubriendo la pantalla con un fondo oscuro.</summary>
		/// <param name="pOwner">[Requerido] Formulario Padre para esta Ventana</param>
		/// <param name="Titulo">[Requerido] Titulo del Mensaje</param>
		/// <param name="Mensaje">[Requerido] Cuerpo del Mensaje, admite stilos HTML.</param>
		/// <param name="Buttons">[Opcional] Botones del Dialogo</param>
		public static DialogResult ShowFlyoutDialog(Form pOwner, string Titulo, string Mensaje, MessageBoxButtons? Buttons = MessageBoxButtons.OKCancel)
		{
			/* El _Form deberá manejar la Salida (botones OK, Cancel) */

			FlyoutAction action = new FlyoutAction() { Caption = Titulo, Description = Mensaje };
			FlyoutCommand DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };

			//Establecer los Botones:
			if (Buttons != null)
			{
				switch (Buttons)
				{
					case MessageBoxButtons.OK:
						DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };
						action.Commands.Add(DefaultButton);
						break;

					case MessageBoxButtons.OKCancel:
						DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };
						action.Commands.Add(new FlyoutCommand() { Text = "Cancelar", Result = DialogResult.Cancel });
						action.Commands.Add(DefaultButton);
						break;

					case MessageBoxButtons.YesNo:
						DefaultButton = new FlyoutCommand() { Text = "Si", Result = DialogResult.Yes };
						action.Commands.Add(new FlyoutCommand() { Text = "No", Result = DialogResult.No });
						action.Commands.Add(DefaultButton);
						break;

					case MessageBoxButtons.YesNoCancel:
						DefaultButton = new FlyoutCommand() { Text = "Si", Result = DialogResult.Yes };
						action.Commands.Add(DefaultButton);
						action.Commands.Add(new FlyoutCommand() { Text = "No", Result = DialogResult.No });
						action.Commands.Add(new FlyoutCommand() { Text = "Cancelar", Result = DialogResult.Cancel });
						break;

					case MessageBoxButtons.RetryCancel:
						DefaultButton = new FlyoutCommand() { Text = "Cancelar", Result = DialogResult.Cancel };
						action.Commands.Add(DefaultButton);
						action.Commands.Add(new FlyoutCommand() { Text = "Reintentar", Result = DialogResult.Retry });
						break;

					case MessageBoxButtons.AbortRetryIgnore:
						DefaultButton = new FlyoutCommand() { Text = "Abortar", Result = DialogResult.Abort };
						action.Commands.Add(DefaultButton);
						action.Commands.Add(new FlyoutCommand() { Text = "Reintentar", Result = DialogResult.Retry });
						action.Commands.Add(new FlyoutCommand() { Text = "Ignorar", Result = DialogResult.Ignore });
						break;

					default:
						DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };
						action.Commands.Add(DefaultButton);
						break;
				}
			}
			else
			{
				action.Commands.Add(DefaultButton);
			}
			action.SelectedCommand = DefaultButton;

			FlyoutProperties properties = new FlyoutProperties()
			{
				ShowCaption = true,
				Style = FlyoutStyle.Popup,
				Alignment = ContentAlignment.MiddleCenter,
			};

			return FlyoutDialog.Show(pOwner, action, properties);
		}

		/// <summary>Muestra un Formulario o UserControl cubriendo la pantalla con un fondo oscuro.</summary>
		/// <param name="pOwner">[Requerido] Formulario Padre para el cuadro de dialogo.</param>
		/// <param name="_Form">UserControl que se mostrará</param>
		/// <param name="Titulo">[Requerido] Titulo del Mensaje</param>
		public static DialogResult ShowFlyoutDialog(Form pOwner, UserControl _Form, string Titulo)
		{
			/* El _Form deberá manejar la Salida (botones OK, Cancel) */

			FlyoutAction action = new FlyoutAction() { Caption = Titulo };
			FlyoutProperties properties = new FlyoutProperties() { Style = FlyoutStyle.Popup, ShowCaption = true };

			return FlyoutDialog.Show(pOwner, _Form, action, properties);
		}

		/// <summary>Muestra un Formulario con Controles cubriendo la pantalla con un fondo oscuro.</summary>
		/// <param name="pOwner">[Requerido] Formulario Padre para el cuadro de dialogo.</param>
		/// <param name="Content">[Requerido] Lista de Controles a mostrar, admite eventos de validacion.</param>
		/// <param name="Titulo">[Requerido] Titulo del Mensaje</param>
		/// <param name="Buttons">Botones del Dialogo</param>
		public static DialogResult ShowFlyoutDialog(Form Owner, ref List<MensajeroControl> Content, string Titulo, MessageBoxButtons? Buttons = MessageBoxButtons.OKCancel)
		{
			FlyoutAction action = new FlyoutAction() { Caption = Titulo };
			FlyoutCommand DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };

			//Establecer los Botones:
			if (Buttons != null)
			{
				switch (Buttons)
				{
					case MessageBoxButtons.OK:
						DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };
						action.Commands.Add(DefaultButton);
						break;

					case MessageBoxButtons.OKCancel:
						DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };
						action.Commands.Add(new FlyoutCommand() { Text = "Cancelar", Result = DialogResult.Cancel });
						action.Commands.Add(DefaultButton);
						break;

					case MessageBoxButtons.YesNo:
						DefaultButton = new FlyoutCommand() { Text = "Si", Result = DialogResult.Yes };
						action.Commands.Add(new FlyoutCommand() { Text = "No", Result = DialogResult.No });
						action.Commands.Add(DefaultButton);
						break;

					case MessageBoxButtons.YesNoCancel:
						DefaultButton = new FlyoutCommand() { Text = "Si", Result = DialogResult.Yes };
						action.Commands.Add(DefaultButton);
						action.Commands.Add(new FlyoutCommand() { Text = "No", Result = DialogResult.No });
						action.Commands.Add(new FlyoutCommand() { Text = "Cancelar", Result = DialogResult.Cancel });
						break;

					case MessageBoxButtons.RetryCancel:
						DefaultButton = new FlyoutCommand() { Text = "Cancelar", Result = DialogResult.Cancel };
						action.Commands.Add(DefaultButton);
						action.Commands.Add(new FlyoutCommand() { Text = "Reintentar", Result = DialogResult.Retry });
						break;

					case MessageBoxButtons.AbortRetryIgnore:
						DefaultButton = new FlyoutCommand() { Text = "Abortar", Result = DialogResult.Abort };
						action.Commands.Add(DefaultButton);
						action.Commands.Add(new FlyoutCommand() { Text = "Reintentar", Result = DialogResult.Retry });
						action.Commands.Add(new FlyoutCommand() { Text = "Ignorar", Result = DialogResult.Ignore });
						break;

					default:
						DefaultButton = new FlyoutCommand() { Text = "Aceptar", Result = DialogResult.OK };
						action.Commands.Add(DefaultButton);
						break;
				}
			}
			else
			{
				action.Commands.Add(DefaultButton);
			}
			action.SelectedCommand = DefaultButton;

			FlyoutProperties properties = new FlyoutProperties()
			{
				ShowCaption = true,
				Style = FlyoutStyle.Popup,
				Alignment = ContentAlignment.MiddleCenter,
			};

			//Obtiene los controles a Mostrar:
			XtraUserControl _Contenido = GetDialogContent(Content);

			// Evita que se Cierre el Dialogo si tiene Errores:	
			Predicate<DialogResult> predicate = (DialogResult parameter) =>
			{
				bool _ret = true;

				/* Si hay algun Error en los Controles previene el Cierre del Dialogo  */
				if (_Contenido != null && _Contenido.Controls.Count > 0)
				{
					LayoutControl lc = _Contenido.Controls[0] as LayoutControl;
					foreach (var control in lc.Controls)
					{
						if (control.GetType().ToString().Contains("DevExpress.XtraEditors"))
						{
							var Ctrl = (BaseEdit)control;
							if (Ctrl.ErrorText != string.Empty) _ret = false;
						}
					}
				}
				return _ret;
			};

			return FlyoutDialog.Show(Owner, _Contenido, action, properties, predicate);
		}


		/* ---------------------FlyoutPanel--------------------------------------------------------------------------------------------------------------  */
		/*  MODO DE USO:
		 *  
		//Mostrar un Simple Mensaje:
		Mensajero.ShowFlyoutPanel(this, Mensajero.TipoAlerta.Exito, "Operación Exitosa!", "Todo salió bien.");

		* // [OPCIONAL] Enlazar al evento para saber que boton se presiona:
			Mensajero.FlyoutPanelButtonClick += (FlyoutPanel _Sender, MensajeroControlEventArgs _E) => {
				switch (_E.EditValue.ToString())
				{
					case "Aceptar": break;
					case "Cancelar": break;
					case "Si": break; 
					case "No": break;
					default:
						break;
				}
			};
		 */

		/// <summary>Muestra un Panel deslizante con Controles a todo lo ancho del formulario. Muestra Controles en el interiror.
		/// <para>Enlaze el evento 'FlyoutPanelButtonClick' para saber cuando se presionan los botones del panel.</para></summary>
		/// <param name="pOwner">[Requerido] Formulario Padre para esta Ventana</param>
		/// <param name="Content">Controles a Mostrar deltro del formulario</param>
		public static void ShowFlyoutPanel(Form pOwner, ref List<MensajeroControl> Content)
		{
			try
			{
				FlyoutPanel panel = new FlyoutPanel();
				panel.OwnerControl = pOwner;
				panel.AnimationRate = 60;
				panel.Options.AnchorType = DevExpress.Utils.Win.PopupToolWindowAnchor.Bottom;
				panel.Options.AnimationType = DevExpress.Utils.Win.PopupToolWindowAnimation.Slide;
				panel.Options.CloseOnOuterClick = true;

				#region Contenido

				LayoutControl LC = GetContentControls(Content, false, CenterVert: false);
				panel.AddControl(LC);
				panel.Height = LC.Height + 40;

				#endregion

				#region Botones

				var _ButtonImages = new SvgImageCollection()
				{
					{ "btn_aceptar", "image://svgimages/icon builder/actions_check.svg" },
					{ "btn_cancelar", "image://svgimages/dashboards/delete.svg" }
				};

				panel.OptionsButtonPanel.ShowButtonPanel = true;
				panel.OptionsButtonPanel.ButtonPanelHeight = 35;
				panel.OptionsButtonPanel.ButtonPanelLocation = FlyoutPanelButtonPanelLocation.Bottom;
				panel.OptionsButtonPanel.ButtonPanelContentAlignment = ContentAlignment.BottomRight;

				panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Aceptar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_aceptar"], SvgImageSize = new Size(24, 24) }));
				panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Cancelar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_cancelar"], SvgImageSize = new Size(24, 24) }));

				panel.ButtonClick += (object sender, FlyoutPanelButtonClickEventArgs e) =>
				{
					//El Usuario debe enlazar al evento para saber que boton se presinó
					ButtonClickHandler?.Invoke(panel, new MensajeroEventArgs() { EditValue = e.Button.Caption });
					(sender as FlyoutPanel).HidePopup();
					ResetEvents();
				};

				#endregion

				panel.ShowPopup();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>Muestra un Panel deslizante con Controles a todo lo ancho del formulario. Muestra un Mensaje o alerta.</summary>
		/// <param name="pOwner">[Requerido] Formulario Padre para esta Ventana</param>
		/// <param name="pTipoAlerta">Determina el Color y Estilo de la Alerta</param>
		/// <param name="pTitulo">Texto para el Titulo</param>
		/// <param name="pMensaje">Texto del Mensaje</param>
		/// <param name="Buttons">Botones a Mostrar</param>
		public static void ShowFlyoutPanel(Form pOwner, TipoAlerta pTipoAlerta, string pTitulo, string pMensaje, MessageBoxButtons? Buttons = MessageBoxButtons.OK)
		{
			try
			{
				FlyoutPanel panel = new FlyoutPanel();
				panel.OwnerControl = pOwner;
				panel.AnimationRate = 60;
				panel.Options.AnchorType = DevExpress.Utils.Win.PopupToolWindowAnchor.Bottom;
				panel.Options.AnimationType = DevExpress.Utils.Win.PopupToolWindowAnimation.Slide;
				panel.Options.CloseOnOuterClick = true;
				//panel.BackColor = Color.PaleGreen;

				#region Botones

				var _ButtonImages = new SvgImageCollection()
				{
					{ "btn_aceptar", @"image://svgimages/icon builder/actions_check.svg" },
					{ "btn_cancelar", @"image://svgimages/dashboards/delete.svg" },
					{ "btn_retry", @"image://svgimages/icon builder/actions_reload.svg" },
					{ "btn_abort", @"image://svgimages/icon builder/actions_forbid.svg" },
					{ "btn_ignore", @"image://svgimages/dashboards/redo.svg" }
				};

				panel.OptionsButtonPanel.ShowButtonPanel = true;
				//panel.OptionsButtonPanel.ButtonPanelHeight = 38;
				panel.OptionsButtonPanel.ButtonPanelLocation = FlyoutPanelButtonPanelLocation.Bottom;
				panel.OptionsButtonPanel.ButtonPanelContentAlignment = ContentAlignment.BottomRight;

				switch (Buttons)
				{
					case MessageBoxButtons.OK:
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Aceptar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_aceptar"], SvgImageSize = new Size(24, 24) }));
						break;
					case MessageBoxButtons.OKCancel:
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Aceptar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_aceptar"], SvgImageSize = new Size(24, 24) }));
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Cancelar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_cancelar"], SvgImageSize = new Size(24, 24) }));
						break;

					case MessageBoxButtons.YesNo:
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Si", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_aceptar"], SvgImageSize = new Size(24, 24) }));
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("No", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_cancelar"], SvgImageSize = new Size(24, 24) }));
						break;
					case MessageBoxButtons.YesNoCancel:
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Si", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_aceptar"], SvgImageSize = new Size(24, 24) }));
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("No", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_cancelar"], SvgImageSize = new Size(24, 24) }));
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton() { Caption = "Cancelar" });
						break;

					case MessageBoxButtons.RetryCancel:
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Reintentar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_retry"], SvgImageSize = new Size(24, 24) }));
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Cancelar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_cancelar"], SvgImageSize = new Size(24, 24) }));
						break;

					case MessageBoxButtons.AbortRetryIgnore:
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Abortar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_abort"], SvgImageSize = new Size(24, 24) }));
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Reintentar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_retry"], SvgImageSize = new Size(24, 24) }));
						panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Ignorar", true, new ButtonImageOptions { SvgImage = _ButtonImages["btn_ignore"], SvgImageSize = new Size(24, 24) }));
						break;
					default:
						break;
				}

				panel.ButtonClick += (object sender, FlyoutPanelButtonClickEventArgs e) =>
				{
					//El Usuario debe enlazar al evento para saber que boton se presinó
					ButtonClickHandler?.Invoke(panel, new MensajeroEventArgs(e.Button.Caption));
					ResetEvents();
					(sender as FlyoutPanel).HidePopup();
				};

				#endregion

				#region Contenido				

				var _MsgImages = GetSvgImages(pTipoAlerta);
				LayoutControl lc = new LayoutControl
				{
					Dock = DockStyle.Fill,
					BackColor = panel.BackColor,
				};
				lc.BeginUpdate();

				lc.Root.Text = pTitulo;
				lc.Root.GroupBordersVisible = !String.IsNullOrEmpty(pTitulo);
				lc.Root.AppearanceGroup.Font = new Font(lc.Root.AppearanceGroup.Font.Name, 10, FontStyle.Bold);
				lc.Root.AppearanceGroup.TextOptions.HAlignment = HorzAlignment.Center;

				switch (pTipoAlerta)
				{
					case TipoAlerta.Info:
						lc.Root.AppearanceGroup.BorderColor = Color.Gainsboro;
						break;
					case TipoAlerta.Exito:
						lc.Root.AppearanceGroup.BorderColor = Color.ForestGreen;
						break;
					case TipoAlerta.Question:
						lc.Root.AppearanceGroup.BorderColor = Color.DodgerBlue;
						break;
					case TipoAlerta.Advertencia:
						lc.Root.AppearanceGroup.BorderColor = Color.DarkOrange;
						break;
					case TipoAlerta.Error:
						lc.Root.AppearanceGroup.BorderColor = Color.Red;
						break;
					default:
						break;
				}

				//El Titulo:
				//var labelTitulo = new LabelControl()
				//{
				//	Name = "lblTitulo",
				//	AutoSizeMode = LabelAutoSizeMode.Vertical,					
				//	AllowHtmlString = true,
				//	Text = pTitulo
				//};
				//labelTitulo.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
				//labelTitulo.Font = new Font(labelTitulo.Font.Name, 14, FontStyle.Bold);

				//El Mensaje:
				var labelMensaje = new LabelControl()
				{
					Name = "lblMensaje",
					AutoSizeMode = LabelAutoSizeMode.Vertical,
					AllowHtmlString = true,
					Text = pMensaje
				};
				labelMensaje.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
				labelMensaje.Appearance.TextOptions.VAlignment = VertAlignment.Center;
				labelMensaje.Font = new Font(labelMensaje.Font.Name, 10);

				//EL Icono:
				var imgIcono = new PictureEdit();
				imgIcono.Size = new Size(87, 87);
				imgIcono.SvgImage = _MsgImages["icon_left"];
				imgIcono.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
				imgIcono.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
				imgIcono.BackColor = lc.BackColor;

				// ---------------------------------
				//Ahora Colocamos los Elementos donde deben ir:

				var Empty1 = lc.Root.AddItem(new EmptySpaceItem());
				if (labelMensaje.Height > lc.Root.Height)
				{
					Empty1.SizeConstraintsType = SizeConstraintsType.Custom;
					Empty1.MaxSize = new Size(0, 10);
				}

				var PicItem = lc.Root.AddItem(string.Empty, imgIcono);
				PicItem.SizeConstraintsType = SizeConstraintsType.Custom;
				PicItem.MaxSize = new Size(87, 87);
				PicItem.MinSize = new Size(87, 87);
				PicItem.TextVisible = false;
				PicItem.Move(Empty1, DevExpress.XtraLayout.Utils.InsertType.Right);

				var BodyItem = lc.Root.AddItem(string.Empty, labelMensaje);
				BodyItem.Move(Empty1, DevExpress.XtraLayout.Utils.InsertType.Bottom);
				BodyItem.TextVisible = false;

				var Empty2 = lc.Root.AddItem(new EmptySpaceItem());
				Empty2.Move(BodyItem, DevExpress.XtraLayout.Utils.InsertType.Bottom);

				lc.EndUpdate();

				panel.AddControl(lc);
				panel.Height = lc.Height + 20;

				lc.BestFit();

				#endregion

				panel.ShowPopup();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>Muestra un Panel Popup con controles personalizados apuntando al control indicado.</summary>
		/// <param name="pOwner">[Requerido] Control Padre para esta Ventana, deben ser controles simples: Button, Textbox, Lookup, etc.</param>
		/// <param name="Content">Controles a Mostrar deltro del formulario</param>
		/// <param name="Size">[Opcional] Tamaño del Contenido</param>
		public static void ShowBeakForm(Control pOwner, ref List<MensajeroControl> Content )
		{
			try
			{
				FlyoutPanel panel = new FlyoutPanel();
				panel.OwnerControl = pOwner;
				panel.Options.CloseOnOuterClick = false;
				//panel.Size = (Size != default) ? Size : new Size(150, 220);

				#region Contenido

				LayoutControl LC = GetContentControls(Content, false, CenterVert: true, VerticalAlign: true);
				panel.AddControl(LC);

				#endregion

				#region Botonera

				var _ButtonImages = new SvgImageCollection()
				{
					{ "btn_aceptar", "image://svgimages/icon builder/actions_check.svg" },
					{ "btn_cancelar", "image://svgimages/dashboards/delete.svg" }
				};
				panel.OptionsButtonPanel.ShowButtonPanel = true;
				panel.OptionsButtonPanel.ButtonPanelLocation = FlyoutPanelButtonPanelLocation.Bottom;
				panel.OptionsButtonPanel.ButtonPanelContentAlignment = ContentAlignment.BottomRight;

				panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Aceptar", false, new ButtonImageOptions { SvgImage = _ButtonImages["btn_aceptar"], SvgImageSize = new Size(16, 16) }));
				panel.OptionsButtonPanel.Buttons.Add(new PeekFormButton("Cancelar", false, new ButtonImageOptions { SvgImage = _ButtonImages["btn_cancelar"], SvgImageSize = new Size(16, 16) }));

				panel.ButtonClick += (object sender, FlyoutPanelButtonClickEventArgs e) =>
				{
					//El Usuario debe enlazar al evento para saber que boton se presinó
					(sender as FlyoutPanel).HideBeakForm();

					ButtonClickHandler?.Invoke(panel, new MensajeroEventArgs(e.Button.Caption));
					ResetEvents();
				};

				#endregion

				panel.ShowBeakForm();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/* ---------------------Metodos Privados----------------------------------------------------------------------------------------------------------  */

		/// <summary>Aqui se Obtiene la Plantilla HTML y CSS para la ventana.</summary>
		private static HtmlTemplate GetTemplateAlert(TipoAlerta pTipoAlerta, bool DarkMode = false)
		{
			HtmlTemplate template = null;
			try
			{
				/* En el HTML:
					'icon_left' e 'icon_right' son los nombres de las Imagenes de los Iconos,
						estos nombres deben corresponderse con la 'key' de las imagenes en la 'SvgImageCollection'.
				   
					'${Caption}':	Corresponde al campo 'AlertInfo.Caption' 
					'${Text}':		Corresponde al campo 'AlertInfo.Text' 

				*  En el CSS: 
					'@Primary'		Controla el Color de los Bordes
					'@WindowText'	Controla el Color de los Textos
					'@Window'		Controla el Color de Fondo

				*  Pueden usarse Nombres de Colores: 
								'Blue', 'Green', 'Red', 'DodgerBlue', etc.
				*  Pueden usarse Colores en Hexadecimal: 
								'#121212'	<- Dark Theme Back Color
								'#DDFFFFFF' <- Dark Theme Text Color (Blanco al 87%)
				*/

				string TempName = string.Empty;
				string PrimaryColor = string.Empty;
				string TextColor = "@WindowText";
				string BackColor = "@Window";

				switch (pTipoAlerta)
				{
					case TipoAlerta.Info:
						PrimaryColor = "@DodgerBlue"; TempName = "Aviso";
						break;
					case TipoAlerta.Exito:
						PrimaryColor = "@Green"; TempName = "Exito";
						break;
					case TipoAlerta.Question:
						PrimaryColor = "@BlueViolet"; TempName = "Question";
						break;
					case TipoAlerta.Advertencia:
						PrimaryColor = "@OrangeRed"; TempName = "Advertencia";
						break;
					case TipoAlerta.Error:
						PrimaryColor = "@Red"; TempName = "Error";
						break;
					default:
						break;
				}
				if (DarkMode)
				{
					BackColor = "@#121212";      //< -Dark Theme Back Color, Recomendado por 'Material Design' https://m2.material.io/design/color/dark-theme.html
					TextColor = "@#DDFFFFFF";    //<- Dark Theme Text Color (Blanco al 87%), Recomendado por 'Material Design'
				}

				template = new HtmlTemplate()
				{
					Template = "<div class='container'><div class='popup'><div class='stripe'></div><div class='content'><div class='icon-container'><img class='icon' src='icon_left'></div><div class='message'><div class='caption'>${Caption}</div><div class='text'><p>${Text}</p></div></div><div id='closeButton' class='close-button'><img class='close-icon' src='icon_right'></div></div></div></div>",
					Styles = ".container{ width: 378px; height: auto; padding: 7px 12px 12px 7px; } .popup{ background-color: @Window/1.0; border-radius: 6px; border-style: solid; border-width: 1px 1px 1px 0px; box-shadow: 2px 2px 12px @Primary/0.4; border-color: @Primary/0.6; display: flex; flex-direction: row; } .content{ width: 100%; display: flex; flex-direction: row; align-items: center; background-color: @Primary/0.015; } .stripe{ width: 3px; background-color: @Primary/0.9; height: 100%; border-radius: 6px 0px 0px 6px; } .message{ display: flex; flex-direction: column; padding: 8px; font-family: 'Segoe UI'; color: @WindowText; } .icon-container{ padding: 8px; } .icon{ width: 48px; height: 48px; } .caption{ font-size: 11pt; font-weight: bold; padding: 6px; } p { white-space: pre-line; } .text{ font-size: 10.5pt; padding: 0px 6px 6px 2px; } .close-button{ padding: 8px; border-radius: 4px 0px 0px 4px; } .close-button:hover{ background-color: @Primary/0.1; } .close-button:active{ background-color: @Primary/0.2; } .close-icon{ width: 22px; height: 22px; }"
							  .Replace("@Primary", PrimaryColor)
							  .Replace("@WindowText", TextColor)
							  .Replace("@Window", BackColor),
					Name = TempName
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return template;
		}
		private static HtmlTemplate GetTemplateMessage(TipoAlerta pTipoAlerta, bool DarkMode = false)
		{
			HtmlTemplate template = null;
			try
			{
				/* En el HTML: 'icon_left', es el nombre del Icono,
					debe corresponderse con la 'key' de las imagenes en la 'SvgImageCollection'.

				 * En el CSS: 
					'@Primary'      Controla el Color de los Botones
					'@WindowText'   Controla el Color de los Textos
					'@Window'       Controla el Color de Fondo
				*/

				string TempName = string.Empty;
				string PrimaryColor = string.Empty;
				string TextColor = "@WindowText";
				string BackColor = "@Window";

				switch (pTipoAlerta)
				{
					case TipoAlerta.Info:
						PrimaryColor = "@DodgerBlue"; TempName = "Aviso";
						break;
					case TipoAlerta.Exito:
						PrimaryColor = "@Green"; TempName = "Exito";
						break;
					case TipoAlerta.Question:
						PrimaryColor = "@BlueViolet"; TempName = "Question";
						break;
					case TipoAlerta.Advertencia:
						PrimaryColor = "@OrangeRed"; TempName = "Advertencia";
						break;
					case TipoAlerta.Error:
						PrimaryColor = "@Red"; TempName = "Error";
						break;
				}
				if (DarkMode)
				{
					BackColor = "@#121212";      //<- Dark Theme Back Color, Recomendado por 'Material Design' https://m2.material.io/design/color/dark-theme.html
					TextColor = "@#DDFFFFFF";    //<- Dark Theme Text Color (Blanco al 87%), Recomendado por 'Material Design'
				}

				template = new HtmlTemplate()
				{
					Template = "<div class='container'><div class='popup'><img src='icon_left' class='image'><div class='caption'>${Caption}</div><div class='text'>${Text}</div><div id='okButton' class='ok-button'>Aceptar</div></div></div>",
					Styles = ".container{ padding: 5px 12px 12px 5px; } .popup{ width: 274px; height: auto; padding: 12px; background-color: @Window; border-radius: 6px; box-shadow: 3px 3px 10px rgba(0,0,0,0.2); display: flex; flex-direction: column; color: @WindowText; font-family: \"Segoe UI\"; text-align: center; justify-content: space-around; border: 1px solid @Control; } .image{ width: 48px; height: 48px; align-self: center; } .caption{ padding: 4px; font-size: 11.5pt; font-weight: bold; } .text{ padding: 4px; font-size: 10pt; } .ok-button{ margin: 12px 3px 3px 3px; padding: 6px; width: 60px; color: @White; background-color: @Primary; border-radius: 3px; font-size: 16px; align-self: center; } .ok-button:hover{ background-color: @Primary/0.8; } .ok-button:active{ background-color: @Primary/0.6; }"
							  .Replace("@Primary", PrimaryColor)
							  .Replace("@WindowText", TextColor)
							  .Replace("@Window", BackColor),
					Name = TempName
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return template;
		}
		private static HtmlTemplate GetTemplateXtraDialog(MessageBoxButtons pButtons, bool DarkMode = false)
		{
			HtmlTemplate template = null;
			try
			{
				/* En el HTML:

					'${Caption}':		Corresponde al campo 'XtraMessageBoxArgs.Caption' 
					'${MessageText}':	Corresponde al campo 'XtraMessageBoxArgs.Text' 

				 * En el CSS: 
					'@WindowText'   Controla el Color de el Texto
					'@Window'       Controla el Color de Fondo
					'@Control'		Controla el Color de los Botones en estado Normal
					'@ControlText'	Controla el Color de los Textos dentro de los Botones
	
					'@HotTrackedForeColor'	Color del Texto del Boton cuando pasa el Mouse
					'@HighlightAlternate'	Color del Boton cuando pasa el Mouse

				* Colores:  Normal,		DarkTheme
				*        --------------------------------  
							Window,		#121212
							WindowText, #DDFFFFFF
							Control,	#141414
							ControlText, DimGray
				*/

				string TempName = "DialogHtmlTemplate";

				string BackColor = "@Window";
				string TextColor = "@WindowText";

				string ControlColor = "@Control";
				string ControlText = "@ControlText";

				string HtmlButtons = string.Empty; //<- Los Botones
				string HtmlIcon = string.Empty; //<- El Icono

				// Establecemos los Colores:
				if (DarkMode)
				{
					BackColor = "@#121212";      //<- Dark Theme Back Color, Recomendado por 'Material Design' https://m2.material.io/design/color/dark-theme.html
					TextColor = "@#DDFFFFFF";    //<- Dark Theme Text Color (Blanco al 87%), Recomendado por 'Material Design'

					ControlColor = "@#141414";  //<- Un Negro un poco mas claro para los Botones
					ControlText = "@DimGray";
				}

				switch (pButtons)
				{
					case MessageBoxButtons.OK:
						HtmlButtons = "<div class='button' tabindex='1' id='dialogresult-ok'>OK</div>";
						break;
					case MessageBoxButtons.OKCancel:
						HtmlButtons = "<div class='button' tabindex='1' id='dialogresult-cancel'>Cancel</div><div class='button' tabindex='2' id='dialogresult-ok'>OK</div>";
						break;

					case MessageBoxButtons.YesNo:
						HtmlButtons = "<div class='button' tabindex='1' id='dialogresult-no'>No</div><div class='button' tabindex='2' id='dialogresult-yes'>Si</div>";
						break;
					case MessageBoxButtons.YesNoCancel:
						HtmlButtons = "<div class='button' tabindex='1' id='dialogresult-cancel'>Cancelar</div><div class='button' tabindex='2' id='dialogresult-no'>No</div><div class='button' tabindex='3' id='dialogresult-yes'>Si</div>";
						break;

					case MessageBoxButtons.RetryCancel:
						HtmlButtons = "<div class='button' tabindex='2' id='dialogresult-cancel'>Cancelar</div><div class='button' tabindex='1' id='dialogresult-retry'>Re-intentar</div>";
						break;
					case MessageBoxButtons.AbortRetryIgnore:
						HtmlButtons = "<div class='button' tabindex='2' id='dialogresult-abort'>Abortar</div><div class='button' tabindex='1' id='dialogresult-retry'>Re-intentar</div><div class='button' tabindex='3' id='dialogresult-ignore'>Ignorar</div>";
						break;

					default:
						HtmlButtons = "<div class='button' tabindex='1' id='dialogresult-ok'>Aceptar</div>";
						break;
				}

				template = new HtmlTemplate()
				{
					Template = "<div class='shadow'><div class='frame' id='frame'><div class='header'><div class='header-text'>${Caption}</div><img class='close-button' src='icon_close' id='closebutton'></div><div class='content' id='content'></div><div class='buttons' id='footerbar'>${Buttons}</div></div></div>"
						.Replace("${Buttons}", HtmlButtons),

					Styles = "body { padding: 35px; } .shadow { border-radius: 14px; } .frame { height: 100%; display: flex; flex-direction: column; border-radius: 14px; border: 1px solid rgba(0, 0, 0, 0.2); background-color: @Window; /* Window @#121212 */ box-shadow: 0px 0px 8px 0px rgba(0, 0, 0, 0.2); } .header{ padding: 11px 12px 11px 11px; display: flex; justify-content: space-between; flex-direction: row; height: 25px; } .header-text{ display: flex; flex-direction: row; font-size: 11pt; font-family: 'Segoe UI'; margin-left: 5px; color: @WindowText; /* @WindowText, @#DDFFFFFF */ } .close-button { width: 16px; height: 16px; border-radius: 5px; padding: 5px; background-color: @Danger/0.7; fill:@White; } .close-button:hover { background-color: @Danger; } .content { flex-grow: 1; } .buttons { border-top: 1px solid @Black/0.1; border-radius: 0px 0px 14px 14px; background-color: @Control; /* Control, @#121212 */ display: flex; flex-direction: row; justify-content: center; } .button{ width: 150px; margin: 25px 5px 25px 5px; padding: 5px; border-radius: 4px; background-color: @Control; /* @Control, @#121212 */ color: @ControlText; /* @ControlText, @DimGray */ font-size: 10pt; font-family: 'Segoe UI'; border: 1px solid @Black/0.15; text-align: center; } .button:hover{ background-color: @Black/0.1; } .button:focus{ background-color: @HighlightAlternate; border: 1px solid @HighlightAlternate; color: @White; }"
						.Replace("@ControlText", ControlText)
						.Replace("@Control", ControlColor)
						.Replace("@WindowText", TextColor)
						.Replace("@Window", BackColor),

					Name = TempName
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return template;
		}
		private static HtmlTemplate GetTemplateDialog(MessageBoxButtons pButtons, MessageBoxIcon pIcon, bool DarkMode = false, string Language = "en")
		{
			HtmlTemplate template = null;
			try
			{
				/* En el HTML:

					'${Caption}':		Corresponde al campo 'XtraMessageBoxArgs.Caption' 
					'${MessageText}':	Corresponde al campo 'XtraMessageBoxArgs.Text' 

				 * En el CSS: 
					'@WindowText'   Controla el Color de el Texto
					'@Window'       Controla el Color de Fondo
					'@Control'		Controla el Color de los Botones en estado Normal
					'@ControlText'	Controla el Color de los Textos dentro de los Botones
	
					'@HotTrackedForeColor'	Color del Texto del Boton cuando pasa el Mouse
					'@HighlightAlternate'	Color del Boton cuando pasa el Mouse

				* Colores:  Normal,		DarkTheme
				*        --------------------------------  
							Window,		#121212
							WindowText, #DDFFFFFF
							Control,	#141414
							ControlText, DimGray
				*/

				string TempName = "DialogHtmlTemplate";

				string BackColor = "@Window";
				string TextColor = "black"; //"@#000000"

				string ControlColor = "@Control";
				string ControlText = "@#000000";

				string ButtonColor = "@ButtonColor"; //<- Color de fondo del boton

				string HtmlButtons = string.Empty; //<- Los Botones
				string HtmlIcon = string.Empty; //<- El Icono


				//Establecemos los colores según el tipo de icono:
				switch (pIcon)
				{
					case MessageBoxIcon.Information:
						BackColor = "@#edf7fa";
						ControlColor = "@#FFFFFF";
						ButtonColor = "#5c98ed";
						break;
					case MessageBoxIcon.Question:
						BackColor = "@#ede1f9";
						ControlColor = "@#FFFFFF";
						ButtonColor = "#BB8FCE";
						break;
					case MessageBoxIcon.Warning:
						BackColor = "@#fdfdcb";
						ControlColor = "@#FFFFFF"; ;
						ButtonColor = "#ffa952";
						break;
					case MessageBoxIcon.Error:
						BackColor = "@#ffcccc";
						ControlColor = "@#FFFFFF";
						ButtonColor = "#ef6a7c";
						break;

					case MessageBoxIcon.None:
						ButtonColor = "@#28B463";
						break;
					default:
						break;
				}

				if (DarkMode)
				{
					BackColor = "@#121212";      //<- Dark Theme Back Color, Recomendado por 'Material Design' https://m2.material.io/design/color/dark-theme.html
					TextColor = "@#DDFFFFFF";    //<- Dark Theme Text Color (Blanco al 87%), Recomendado por 'Material Design'

					ControlColor = "@#141414";  //<- Un Negro un poco mas claro para los Botones
					ControlText = "@DimGray";
				}

				if (pIcon != MessageBoxIcon.None)
				{
					HtmlIcon = "<img src='${MessageIcon}' class='icon'>";
				}

				string Button_1 = string.Empty;
				string Button_2 = string.Empty;
				string Button_3 = string.Empty;

				switch (pButtons)
				{
					case MessageBoxButtons.OK:
						switch (Language)
						{
							case "en": Button_1 = "OK";  break;
							case "de": Button_1 = "Akzeptieren";  break;
							case "fr": Button_1 = "Accepter";  break;
							case "ru": Button_1 = "Принимать";  break;
							case "pt": Button_1 = "Aceitar";  break;
							case "it": Button_1 = "Accettare";  break;
							case "es": Button_1 = "Aceptar";  break;
							default:   Button_1 = "OK";  break;
						}
						HtmlButtons = string.Format("<div class='button' tabindex='1' id='dialogresult-ok'>{0}</div>", Button_1);
						break;
					case MessageBoxButtons.OKCancel:
						switch (Language)
						{
							case "en": Button_1 = "OK"; Button_2 = "Cancel"; break;
							case "de": Button_1 = "Akzeptieren"; Button_2 = "Stornieren"; break;
							case "fr": Button_1 = "Accepter"; Button_2 = "Annuler"; break;
							case "ru": Button_1 = "Принимать"; Button_2 = "Отмена"; break;
							case "pt": Button_1 = "Aceitar"; Button_2 = "Cancelar"; break;
							case "it": Button_1 = "Accettare"; Button_2 = "Annulla"; break;
							case "es": Button_1 = "Aceptar"; Button_2 = "Cancelar"; break;
							default: Button_1 = "OK"; Button_2 = "Cancel"; break;
						}
						HtmlButtons = string.Format("<div class='button' tabindex='1' id='dialogresult-cancel'>{0}</div><div class='button' tabindex='2' id='dialogresult-ok'>{1}</div>", Button_2, Button_1 );
						break;

					case MessageBoxButtons.YesNo:
						switch (Language)
						{
							case "en": Button_1 = "Yes"; Button_2 = "No";  break;
							case "de": Button_1 = "Ja"; Button_2 = "NEIN";  break;
							case "fr": Button_1 = "Ouais"; Button_2 = "Non";  break;
							case "ru": Button_1 = "Ага"; Button_2 = "Нет";  break;
							case "pt": Button_1 = "Sim"; Button_2 = "Não";  break;
							case "it": Button_1 = "Sì"; Button_2 = "NO";  break;
							case "es": Button_1 = "Si"; Button_2 = "No";  break;
							default: Button_1 = "Yes"; Button_2 = "No";  break;
						}
						HtmlButtons = string.Format("<div class='button' tabindex='1' id='dialogresult-no'>{0}</div><div class='button' tabindex='2' id='dialogresult-yes'>{1}</div>", Button_2, Button_1);
						break;
					case MessageBoxButtons.YesNoCancel:
						switch (Language)
						{
							case "en": Button_1 = "Yes"; Button_2 = "No"; Button_3 = "Cancel"; break;
							case "de": Button_1 = "Ja"; Button_2 = "NEIN"; Button_3 = "Stornieren"; break;
							case "fr": Button_1 = "Ouais"; Button_2 = "Non"; Button_3 = "Annuler"; break;
							case "ru": Button_1 = "Ага"; Button_2 = "Нет"; Button_3 = "Отмена"; break;
							case "pt": Button_1 = "Sim"; Button_2 = "Não"; Button_3 = "Cancelar"; break;
							case "it": Button_1 = "Sì"; Button_2 = "NO"; Button_3 = "Annulla"; break;
							case "es": Button_1 = "Si"; Button_2 = "No"; Button_3 = "Cancelar"; break;
							default: Button_1 = "Yes"; Button_2 = "No"; Button_3 = "Cancel"; break;
						}
						HtmlButtons = string.Format("<div class='button' tabindex='1' id='dialogresult-cancel'>{0}</div><div class='button' tabindex='2' id='dialogresult-no'>{1}</div><div class='button' tabindex='3' id='dialogresult-yes'>{2}</div>",
										Button_3, Button_2, Button_1);
						break;

					case MessageBoxButtons.RetryCancel:
						switch (Language)
						{
							case "en": Button_1 = "Retry"; Button_2 = "Cancel"; break;
							case "de": Button_1 = "Wiederholen"; Button_2 = "Stornieren"; break;
							case "fr": Button_1 = "Recommencez"; Button_2 = "Annuler"; break;
							case "ru": Button_1 = "Повторить попытку"; Button_2 = "Отмена"; break;
							case "pt": Button_1 = "Tentar novamente"; Button_2 = "Cancelar"; break;
							case "it": Button_1 = "Riprova"; Button_2 = "Annulla"; break;
							case "es": Button_1 = "Reintentar"; Button_2 = "Cancelar"; break;
							default: Button_1 = "Retry"; Button_2 = "Cancel"; break;
						}
						HtmlButtons = string.Format("<div class='button' tabindex='2' id='dialogresult-cancel'>Cancelar</div><div class='button' tabindex='1' id='dialogresult-retry'>Re-intentar</div>", Button_2, Button_1);
						break;
					case MessageBoxButtons.AbortRetryIgnore:
						switch (Language)
						{
							case "en": Button_1 = "Abort"; Button_2 = "Retry"; Button_3 = "Ignore"; break;
							case "de": Button_1 = "Abbrechen"; Button_2 = "Wiederholen"; Button_3 = "Ignorieren"; break;
							case "fr": Button_1 = "Avorter"; Button_2 = "Recommencez"; Button_3 = "Ignorer"; break;
							case "ru": Button_1 = "Прервать"; Button_2 = "Повторить попытку"; Button_3 = "Игнорировать"; break;
							case "pt": Button_1 = "Abortar"; Button_2 = "Tentar novamente"; Button_3 = "Ignorar"; break;
							case "it": Button_1 = "Interrompi"; Button_2 = "Riprova"; Button_3 = "Ignorare"; break;
							case "es": Button_1 = "Abortar"; Button_2 = "Reintentar"; Button_3 = "Ignorar"; break;
							default:   Button_1 = "Abort"; Button_2 = "Retry"; Button_3 = "Ignore"; break;
						}
						HtmlButtons = string.Format("<div class='button' tabindex='2' id='dialogresult-abort'>{0}</div><div class='button' tabindex='1' id='dialogresult-retry'>{1}</div><div class='button' tabindex='3' id='dialogresult-ignore'>{2}</div>",
							Button_3, Button_2, Button_1); 
						break;

					default: //<- OK
						switch (Language)
						{
							case "en": Button_1 = "OK"; break;
							case "de": Button_1 = "Akzeptieren"; break;
							case "fr": Button_1 = "Accepter"; break;
							case "ru": Button_1 = "Принимать"; break;
							case "pt": Button_1 = "Aceitar"; break;
							case "it": Button_1 = "Accettare"; break;
							case "es": Button_1 = "Aceptar"; break;
							default: Button_1 = "OK"; break;
						}
						HtmlButtons = string.Format("<div class='button' tabindex='1' id='dialogresult-ok'>{0}</div>", Button_1);
						break;
				}

				template = new HtmlTemplate()
				{
					Template = "<div class='frame' id='frame'><div class='content'><div class='text caption'>${Caption}</div><div id='content'><div class='image'>${MessageIcon}</div><div class='text message'>${MessageText}</div></div></div><div class='buttons'>${Buttons}</div></div>"
						.Replace("${MessageIcon}", HtmlIcon)
						.Replace("${Buttons}", HtmlButtons),

					Styles = "body{ padding: 15px; font-size: 10pt; font-family: 'Segoe UI'; text-align: center; } .frame{ color: @WindowText; /* WindowText, #DDFFFFFF */ background-color: @Window;/* Window, #121212 */ border: 2px solid @Black/0.2; border-left: 5px solid @ButtonColor; border-radius: 10px; box-shadow: 0px 5px 10px 0px rgba(0, 0, 0, 0.2); } .content { padding: 15px; } .text { padding: 10px; text-align: left; color: @WindowText; } .caption { font-size: 14pt; font-family: 'Segoe UI Semibold'; padding-top: 0px; padding-left: 41px; white-space: pre;} .message { white-space: pre; padding-left: 40px;} .buttons { background-color: @Control; /* Control, #141414 */ padding: 10px; display: flex; flex-direction: row; justify-content: flex-end; border-top: 1px solid @Black/0.1; border-radius: 0px 0px 10px 10px; } .button { color: @Black; /* ControlText, DimGray */ background-color: @Control;\t/* Control, #141414 */ min-width: 80px; margin: 0px 5px; padding: 5px; border: 1px solid @Black/0.15; border-radius: 5px; } .button:hover { background-color: @Black/0.1; color: @Black;} .button:focus { background-color: @ButtonColor; border: 1px solid @ButtonColor; color: @White; } .icon { width: 30px; height: 30px; position: absolute; top: 10%; transform: translateY(-50%);} .image{ position: relative; } #content {display: flex; flex-direction: row;} "
						.Replace("@ControlText", ControlText)
						.Replace("@Control", ControlColor)
						.Replace("@WindowText", TextColor)
						.Replace("@Window", BackColor)
						.Replace("@ButtonColor", ButtonColor),

					Name = TempName
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return template;
		}

		private static SvgImageCollection GetSvgImages(TipoAlerta pTipoAlerta)
		{
			//Agrega Imagenes SVG desde la Galeria del DevExpress:
			//-->  You should deploy the DevExpress.Images.v22.2 library when assigning images using URI names.

			switch (pTipoAlerta)
			{
				case TipoAlerta.Info:
					return new SvgImageCollection()
					{
						{ "icon_left", "image://svgimages/icon builder/actions_info.svg" },
						{ "icon_right", "image://svgimages/icon builder/actions_send.svg" },
						{ "icon_close", "image://svgimages/dashboards/delete.svg" }
					};

				case TipoAlerta.Exito:
					return new SvgImageCollection()
					{
						{ "icon_left", "image://svgimages/icon builder/actions_checkcircled.svg" },
						{ "icon_right", "image://svgimages/icon builder/actions_check.svg" },
						{ "icon_close", "image://svgimages/dashboards/delete.svg" }
					};

				case TipoAlerta.Question:
					return new SvgImageCollection()
					{
						{ "icon_left", "image://svgimages/outlook inspired/support.svg" },
						{ "icon_right","image://svgimages/icon builder/actions_question.svg" },
						{ "icon_close", "image://svgimages/dashboards/delete.svg" }
					};

				case TipoAlerta.Advertencia:
					return new SvgImageCollection()
					{
						{ "icon_left", "image://svgimages/status/warning.svg" },
						{ "icon_right", "image://svgimages/icon builder/actions_bell.svg" },
						{ "icon_close", "image://svgimages/dashboards/delete.svg" }
					};

				case TipoAlerta.Error:
					return new SvgImageCollection()
					{
						{ "icon_left", "image://svgimages/icon builder/security_warningcircled1.svg" },
						{ "icon_right", "image://svgimages/icon builder/actions_deletecircled.svg"  },
						{ "icon_close", "image://svgimages/dashboards/delete.svg" }
					};

				default:
					return new SvgImageCollection()
					{
						{ "icon_left", "image://svgimages/business objects/bo_mydetails.svg" },
						{ "icon_right", "image://svgimages/icon builder/actions_info.svg" },
						{ "icon_close", "image://svgimages/dashboards/delete.svg" }
					};
			}
		}


		private static XtraUserControl GetDialogContent(List<MensajeroControl> pContent, bool DarkMode = false )
		{
			/*  El contenido va dentro de un UserControl
			 *  Y usamos un Layout para que se encarge automaticamente de los tamaños y organizacion de los controles 
			 */

			MensajeroColors Colores = new MensajeroColors(DarkMode);

			XtraUserControl _ret = new XtraUserControl()
			{
				Width = 300,            //<- Ancho x defecto
				Dock = DockStyle.Fill,
				BackColor = Colores.WindowBackColor,
				ForeColor = Colores.WindowForeColor,
				BorderStyle = BorderStyle.None,
			};

			//if (pSize != default) _ret.Size = pSize;

			if (pContent != null && pContent.Count > 0)
			{
				LayoutControl LC = GetContentControls(pContent, DarkMode);

				_ret.Controls.Add(LC);
				_ret.Height = LC.Height + 20;
			}

			return _ret;
		}
		private static LayoutControl GetContentControls(List<MensajeroControl> pContent, bool DarkMode = false, bool CenterHorz = true, bool CenterVert = true, bool VerticalAlign = false)
		{
			MensajeroColors _Colors = new MensajeroColors(DarkMode);
			LayoutControl lc = new LayoutControl
			{
				Dock = DockStyle.Fill,
				ForeColor = _Colors.ControlForeColor,
				BackColor = _Colors.WindowBackColor
			};

			if (pContent != null && pContent.Count > 0)
			{
				int Contador = 0; //<- id para los nombres de los controles.
				EmptySpaceItem eTop = new EmptySpaceItem(); eTop.SizeConstraintsType = SizeConstraintsType.Custom;
				EmptySpaceItem eLeft = new EmptySpaceItem();
				EmptySpaceItem eRight = new EmptySpaceItem();
				EmptySpaceItem eBottom = new EmptySpaceItem();

				lc.BeginUpdate();
				lc.Root.GroupBordersVisible = false;

				//Creo un Contenedor para los Controles:
				LayoutControlGroup group1 = new LayoutControlGroup
				{
					Name = "Grupo_0",
					TextVisible = false,
					GroupBordersVisible = false
				};

				//Este es el Manejador de Errores para TODOS los controles:
				DXErrorProvider errorHandler = new DXErrorProvider(lc);

				group1.BeginUpdate();
				foreach (MensajeroControl _Control in pContent)
				{
					var _LayoutEditor = AddControl(group1, _Control, Contador, _Colors, errorHandler, VerticalAlign);
					Contador++;
				}
				group1.EndUpdate();

				if (CenterVert) lc.Root.AddItem(eTop);  //<- Un Espacio al principio para Centrar el Contenido Verticalmente				
				if (CenterHorz) lc.Root.AddItem(eLeft); //<- Agrego un Espacio a la Izquierda  para Centrar el Contenido Horizontalmente

				lc.Root.Add(group1);    //<- Agrego el Grupo de Controles:

				if (CenterHorz)
				{
					group1.Move(eLeft, DevExpress.XtraLayout.Utils.InsertType.Right);
					lc.Root.AddItem(eRight); //<- Agrego un Espacio a la derecha para Centrar el Contenido Horizontalmente
					eRight.Move(group1, DevExpress.XtraLayout.Utils.InsertType.Right);
				}

				if (CenterVert) lc.Root.AddItem(eBottom); //<- Un Espacio al final para Centrar el Contenido Verticalmente
				lc.EndUpdate();
				lc.BestFit();

				eTop.Height = lc.Height * (10 / 100);
				eLeft.Width = lc.Width * (10 / 100);
			}

			return lc;
		}
		private static LayoutItem AddControl(LayoutControlGroup Group, MensajeroControl ControlData, int pControlID, MensajeroColors Colors, DXErrorProvider errorHandler, bool VerticalAlign = false)
		{
			/* Crea el control indicado en 'MensajeroControl' y devuelve un item para el LayoutControl con el control ya incluido.  */

			LayoutItem _ret = null;
			BaseEdit _Editor = null;
			string DataFormat = "";

			if (ControlData.Control == MensajeroControl.ControlType.Auto)
			{
				//Determinar el Control adecuado segun el tipo de datos
				switch (Type.GetTypeCode(ControlData.Type))
				{
					case TypeCode.Char: //<- PARA AGREGAR ETIQUETAS COMO TITULOS
						ControlData.Control = MensajeroControl.ControlType.Title;
						break;

					case TypeCode.String:
						ControlData.Control = MensajeroControl.ControlType.TextEdit;
						break;

					case TypeCode.Int32:
						DataFormat = "n0";
						ControlData.Control = MensajeroControl.ControlType.SpinEdit;
						break;

					case TypeCode.Boolean:
						ControlData.Control = MensajeroControl.ControlType.CheckEdit;
						break;

					case TypeCode.Decimal:
						DataFormat = "n2";
						ControlData.Control = MensajeroControl.ControlType.SpinEdit;
						break;

					case TypeCode.DateTime:
						DataFormat = "d";
						ControlData.Control = MensajeroControl.ControlType.DateEdit;
						break;

					default:
						ControlData.Control = MensajeroControl.ControlType.ComboEdit;
						break;
				}
			}

			switch (ControlData.Control)
			{
				case MensajeroControl.ControlType.TextEdit:
					_Editor = new DevExpress.XtraEditors.TextEdit()
					{
						Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("editor_{0}", pControlID) : ControlData.Name,
						EditValue = ControlData.EditValue
					};
					_ret = Group.AddItem(ControlData.Caption, _Editor);
					_Editor.EditValueChanged += (object _Sender, EventArgs _E) =>
					{
						ControlData.EditValue = _Editor.EditValue; //<- Cuando se cambia el Valor, se actualiza el DataSource.
					};
					break;

				case MensajeroControl.ControlType.SpinEdit:
					var spinEdit = new SpinEdit
					{
						Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("editor_{0}", pControlID) : ControlData.Name,
						EditValue = ControlData.EditValue
					};
					spinEdit.Properties.MaskSettings.MaskExpression = DataFormat;
					spinEdit.Properties.UseMaskAsDisplayFormat = true;
					spinEdit.EditValueChanged += (object _Sender, EventArgs _E) =>
					{
						ControlData.EditValue = spinEdit.EditValue; 
					};
					_ret = Group.AddItem(ControlData.Caption, spinEdit);
					_Editor = spinEdit;					
					break;

				case MensajeroControl.ControlType.DateEdit:
					var dateEdit = new DateEdit
					{
						Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("editor_{0}", pControlID) : ControlData.Name,
						EditValue = ControlData.EditValue
					};
					dateEdit.Properties.MaskSettings.MaskExpression = DataFormat;
					dateEdit.Properties.UseMaskAsDisplayFormat = true;
					dateEdit.EditValueChanged += (object _Sender, EventArgs _E) =>
					{
						ControlData.EditValue = dateEdit.EditValue;
					};
					_ret = Group.AddItem(ControlData.Caption, dateEdit);
					_Editor = dateEdit;					
					break;

				case MensajeroControl.ControlType.CheckEdit:
					var checkEdit = new CheckEdit
					{
						Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("editor_{0}", pControlID) : ControlData.Name,
						Text = ControlData.Caption,
						EditValue = ControlData.EditValue,
						Checked = Convert.ToBoolean(ControlData.EditValue),
						BackColor = Colors.WindowBackColor
					};
					checkEdit.EditValueChanged += (object _Sender, EventArgs _E) =>
					{
						ControlData.EditValue = checkEdit.EditValue;
					};
					_ret = Group.AddItem(ControlData.Caption, checkEdit);
					_Editor = checkEdit;					
					break;

				case MensajeroControl.ControlType.ComboEdit:
					var cboEditor1 = new LookUpEdit()
					{
						Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("editor_{0}", pControlID) : ControlData.Name,
						EditValue = ControlData.EditValue
					};
					if (ControlData.DataSource != null)
					{
						cboEditor1.Properties.DataSource = ControlData.DataSource;
						cboEditor1.Properties.ValueMember = ControlData.ValueMember;
						cboEditor1.Properties.DisplayMember = ControlData.DisplayMember;
					}
					cboEditor1.EditValueChanged += (object _Sender, EventArgs _E) =>
					{
						ControlData.EditValue = cboEditor1.EditValue;
						ControlData.DataValue = cboEditor1.GetSelectedDataRow();
					};
					_ret = Group.AddItem(ControlData.Caption, cboEditor1);
					_Editor = cboEditor1;					
					break;

				case MensajeroControl.ControlType.Button:
					var simpleButton = new SimpleButton()
					{
						Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("button_{0}", pControlID) : ControlData.Name,
						Text = ControlData.Caption,
					};
					simpleButton.Click += (object _Sender, EventArgs _E) =>
					{
						ControlData.EditValue = simpleButton.Text; //<- Devuleve la etiqueta del Boton clickeado
						ButtonClickHandler?.Invoke(simpleButton, new MensajeroEventArgs(simpleButton.Text));
						ResetEvents();
					};
					_ret = Group.AddItem(ControlData.Caption, simpleButton);
					_ret.TextVisible = false;
					break;

				case MensajeroControl.ControlType.Title:
					var labelControl = new LabelControl()
					{
						Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("editor_{0}", pControlID) : ControlData.Name,
						AutoSizeMode = LabelAutoSizeMode.None,
						Text = ControlData.Caption,
						AllowHtmlString = true
					};
					labelControl.Font = new Font(labelControl.Font.Name, 11, FontStyle.Bold);

					_ret = Group.AddItem(ControlData.Caption, labelControl);
					_ret.TextVisible = false;
					break;

				case MensajeroControl.ControlType.Separador:
					_ret = Group.AddItem(string.Empty, new SeparatorControl());
					_ret.TextVisible = false;
					break;

				default:
					break;
			}

			if (_Editor != null)
			{
				_Editor.Properties.Appearance.BackColor = Colors.ControlBackColor;
				_Editor.Properties.Appearance.ForeColor = Colors.ControlForeColor;
				_Editor.Properties.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
				_Editor.Properties.AllowHtmlDraw = DefaultBoolean.True;
			}			

			//Ocurre cuando el usuario envia un Error de Validacion, este se muestra sobre el control con un icono rojo
			ControlData.OnErrorValitadion += (object _ErrorText, EventArgs _E) =>
			{
				errorHandler.SetError(_Editor, _ErrorText.ToString());
			};			

			_ret.Name = string.IsNullOrEmpty(ControlData.Name) ? string.Format("layoutItem_{0}", pControlID) : ControlData.Name;
			_ret.TextLocation = (VerticalAlign ? Locations.Top : Locations.Left);

			return _ret;
		}

	}

	/// <summary>Control personalizado que se muestra dentro de los Dialogos.</summary>
	public class MensajeroControl
	{
		#region Variables Privadas

		private object _EditValue = null;
		private string _Name = string.Empty;

		#endregion

		#region Constructores		

		public MensajeroControl() { }
		public MensajeroControl(string pCaption)
		{
			Caption = pCaption;
		}
		public MensajeroControl(string pName, string pCaption)
		{
			Name = pName;
			Caption = pCaption;
		}

		#endregion

		#region Eventos Publicos

		//Manejador de Eventos Personalizado, permite enviar datos en el objeto 'e':
		public delegate void ControlEventHandler(object sender, MensajeroEventArgs e);

		/// <summary>Ocurre cuando cambia el Valor del Control. 'e' tiene el nuevo valor. 'sender' es una instancia de esta clase.</summary>
		public event ControlEventHandler EditValueChanged;

		/// <summary>Ocurre cuando el Usuario quiere enviar un mensaje de Validacion al Control.
		/// <para>El usuario debe encargarse de las validaciones, aqui sólo muestro u oculto el mensaje de error.</para></summary>
		public event EventHandler OnErrorValitadion;

		#endregion

		#region Propiedades Publicas

		public enum ControlType
		{
			Auto = 0,
			TextEdit,
			SpinEdit,
			DateEdit,
			CheckEdit,
			ComboEdit,
			Button,
			Title,
			Separador
		}

		/// <summary>Determina el Tipo de Datos del Control</summary>
		public Type Type { get; set; } = typeof(String);

		/// <summary>[Opcional] Determina el tipo de control a usar</summary>
		public ControlType Control { get; set; } = ControlType.Auto;

		/// <summary>[Opcional] Determina el Nombre del Control</summary>
		public string Name
		{
			get
			{
				if (this._Name == string.Empty)
				{
					this._Name = string.Format("{0}Control_{1}", Type.Name,
						Convert.ToInt32((DateTime.Now - new DateTime(1975, 4, 18)).TotalSeconds % 100000000).ToString());
				}
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}

		/// <summary>Determina la Etiqueta del Control</summary>
		public string Caption { get; set; } = string.Empty;


		/// <summary>Determina el Valor del Control</summary>
		public object EditValue
		{
			get { return _EditValue; }
			set
			{
				_EditValue = value;

				//Si el evento está instanciado, aqui lo dispara:
				EditValueChanged?.Invoke(this, new MensajeroEventArgs(value, DataSource != null ? DataValue : null));
			}
		}

		/// <summary>Cuando 'Type' es una 'List<T>', DataSource almacena los Valores posibles del Control.</summary>
		public object DataSource { get; set; } = null;

		/// <summary>Cuando 'Type' es una 'List<T>', DataValue almacena el Objeto correspondiente al EditValue seleccionado.</summary>
		public object DataValue { get; set; } = null;

		/// <summary>Cuando 'Type' es una 'List<T>', ValueMember establece la propiedad cuyo valor se usa para obtener el 'EditValue'</summary>
		public string ValueMember { get; set; } = string.Empty;

		/// <summary>Cuando 'Type' es una 'List<T>', DisplayMember establece la propiedad cuyo valor se Muestra en el Combo</summary>
		public string DisplayMember { get; set; } = string.Empty;

		#endregion

		#region Metodos Publicos

		/// <summary>Muestra un Mensaje de Validacion en el Control.</summary>
		/// <param name="pMensaje">Si es 'String.Empty' se borra el Error.</param>
		public void Validate(string pMensaje)
		{
			OnErrorValitadion(pMensaje, null);
		}

		#endregion
	}

	//Argumentos para los eventos generados x los dialogos
	public class MensajeroEventArgs : EventArgs
	{
		public MensajeroEventArgs() { }
		public MensajeroEventArgs(object pEditValue)
		{
			EditValue = pEditValue;
		}
		public MensajeroEventArgs(object pEditValue, object pDataValue)
		{
			EditValue = pEditValue;
			DataValue = pDataValue;
		}

		/// <summary>Valor de Edicion del Control.</summary>
		public object EditValue { get; set; }

		/// <summary>[Opcional] Cuando 'Type' es una 'List<T>', DataValue almacena el Objeto correspondiente al EditValue seleccionado.</summary>
		public object DataValue { get; set; }
	}

	/// <summary>Determina los Colores de Fondo y de Texto/// </summary>
	public class MensajeroColors
	{
		public MensajeroColors(bool pDarkMode = false)
		{
			this.DarkMode = pDarkMode;

			this.WindowBackColor = DarkMode ? ColorTranslator.FromHtml("#121212") : Color.White;		//<- Dark Theme Back Color, Recomendado por 'Material Design' https://m2.material.io/design/color/dark-theme.html
			this.WindowForeColor = DarkMode ? ColorTranslator.FromHtml("#DDFFFFFF") : Color.Black;		//<- Dark Theme Text Color (Blanco al 87%), Recomendado por 'Material Design'

			this.ControlBackColor = DarkMode ? ColorTranslator.FromHtml("#141414") : Color.WhiteSmoke;      //<- Un Negro un poco mas claro para los Botones
			this.ControlForeColor = DarkMode ? Color.FromKnownColor(KnownColor.DimGray) : Color.Black;
		}
		
		public Color WindowBackColor { get; set; }
		public Color WindowForeColor { get; set; }

		public Color ControlBackColor { get; set; }
		public Color ControlForeColor { get; set; }

		public bool DarkMode { get; set; }
	}
}

