# Auditoría de la primera entrega

Alcance autorizado por la sección «Primera tarea» del adjunto: análisis, infraestructura, KButton, KTextBox y Demo. Los seis controles restantes están descritos como etapas posteriores y no se consideran implementados. La solicitud adicional exige personalización amplia, comportamiento original y optimización; una compilación verde por sí sola no prueba esas condiciones.

## Entregables de diseño numerados en el adjunto

| Requisito | Evidencia actual |
|---|---|
| 1. Análisis visual | DESIGN.md §1, a partir de la imagen proporcionada |
| 2. Paleta aproximada | DESIGN.md §2; valores iniciales en KTheme.cs |
| 3. Radios | DESIGN.md §3; KMetrics y CornerRadius |
| 4. Tamaños y alturas | DESIGN.md §3; muestras 32/40/48 de Demo |
| 5. Spacing | DESIGN.md §3; KMetrics y Padding |
| 6. Estados deducidos | DESIGN.md §4; diferencia explícita entre observado e inferido |
| 7. Tokens | KTheme.cs y DESIGN.md §2–3 |
| 8. Arquitectura mínima | DESIGN.md §5 y proyectos de biblioteca/Demo/pruebas |
| 9. Propuesta KButton | DESIGN.md §4 y control implementado |
| 10. Propuesta KTextBox | DESIGN.md §4 y control implementado |

## Implementación y verificación

| Requisito | Evidencia y límite |
|---|---|
| WinForms nativo, sin WPF | Proyectos net8.0-windows/UseWindowsForms; Button y TextBox reales |
| DLL reutilizable | Proyecto KovaqsUI, build y pack locales |
| Colores/bordes/radios/formas | API de controles y Appearance; pruebas de formas y tamaños mínimos |
| Degradados configurables por estado | Pruebas de píxeles para dirección, herencia, Solid y restauración Linear |
| Teclado y eventos de botón | Pruebas PerformClick, mnemonic, Space, Enter, Escape, deshabilitado |
| Propiedades heredadas del botón | Los cuatro FlatStyle conservan la pintura Kova y PerformClick, incluida asignación System antes de crear HWND. ImageList/ImageKey verificados por píxeles |
| Funcionalidad original completa | **No probada**: falta matriz comparativa completa; Paint ya conserva su evento nativo (comprobado desde referencias Button/Control), pero varias propiedades de dibujo nativas tienen diferencias documentadas |
| Texto nativo | Pruebas Unicode, selección/reemplazo, TextChanged, readonly, password, multilínea y focus |
| Navegación de texto | Pruebas con formulario visible de Tab/Shift+Tab, una sola parada y cambio de TabStop |
| Estado Pressed del campo | Mouse sobre marco y mensajes de mouse al EDIT activan el estilo; mouse-up, pérdida de captura y Enabled limpian el estado |
| Validación y enlace de datos | Pruebas de enlace bidireccional OnPropertyChanged, validación cancelada/aceptada al navegar y CausesValidation del editor |
| Edición por mensajes nativos | Pruebas WM_CHAR/EM_REPLACESEL en el HWND del editor: MaxLength, ReadOnly, CharacterCasing Unicode, supresión por KeyPress y Undo real |
| Coherencia de ForeColor | Appearance.Normal y la propiedad nativa se sincronizan; eventos únicos y estados independientes verificados |
| Foco personalizable | Indicador independiente del borde; pruebas de color propio, herencia del estado y ocultación explícita |
| Colores de texto deshabilitado | Máscara dibujada por Windows y recoloreada; pruebas de color, contraseña, selección al rehabilitar, alineación latina/hebrea/mixta y carácter aislado, scroll horizontal y multilínea. Caché verificada ante cambio de texto/color y ciclos Enabled, sin crecimiento GDI |
| Clipboard, IME y lector de pantalla | Implementación delegada al editor; **no verificado de extremo a extremo** |
| Proveedor de accesibilidad | Roles nativos de botón/texto, acción predeterminada, estado Unavailable y actualización del nombre/descripción. Recuperación MSAA real con AccessibleObjectFromWindow; aún falta una sesión de lector de pantalla |
| Iconos, alineación y escalado | Pruebas de píxeles de ImageAlign e inversión RTL; AutoSize ante cambios de tamaño/separación de imagen y ancho restringido; falta validar DPI físico |
| Diseñador de Visual Studio | DesignSurface real: creación de componentes, resolución de diseñadores, edición de propiedades, PropertyGrid y serialización/restauración CodeDOM de estilos anidados y texto sin duplicar el editor. **Verificación parcial en Visual Studio Community 2026**: ambos controles cargan en DesignerSandbox, aparecen en Toolbox y el IDE guarda InitializeComponent sin duplicar el editor. El archivo guardado incluye dos instancias adicionales y cambios anidados reales (Duration 180, Easing Linear, Normal.BorderColor Black), y compila sin errores. Pendiente: verificar la reapertura con esos valores; el gesto concreto de arrastre no fue observado. Detalles en MANUAL-VALIDATION.md |
| DPI 100/125/150/200 % | Unidades lógicas y PerMonitorV2. Mensajes WM_DPICHANGED sintéticos verifican el reajuste del formulario, límites del editor y vuelta a tamaños originales; los hijos siguen recibiendo el DPI real del monitor (120 en esta sesión). **No prueba dibujo de los hijos a 96/144/192 ni migración entre monitores físicos** |
| Demo visual | Capturas clara/oscura revisadas; conmutadores de tema, fuente, Enabled y variantes. Inspector modeless Personalizar… sobre muestras reales, con Appearance expandido inicialmente |
| Optimización | Timer compartido detenido en reposo; recursos temporales liberados; prueba de 2.000 renderizados sin aumento de handles GDI |
| Animaciones | Transiciones configurables de colores y degradados: pruebas de valores intermedios, inversión continua, estado final, editor sincronizado y disposición durante animación |

Esta auditoría mantiene abiertos los requisitos cuya evidencia es incompleta. No equivale a afirmar paridad total con WinForms ni aprobación del diseño por parte del usuario.






## Ampliación autorizada por el usuario

La solicitud posterior añade KCheckBox, KRadioButton, KComboBox, KProgressBar y KPanel. Se implementan con las bases nativas correspondientes, estilos compartidos y animaciones. El usuario autoriza omitir la automatización problemática del IDE y confirma sus verificaciones manuales; deja de ser una puerta de bloqueo para esta entrega. El panel incluye sombra interna cacheada, y la Demo ampliada presenta los cinco controles nuevos y la composición de contador de la imagen posterior.

