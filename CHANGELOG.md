# Cambios

## 0.5.0

- El proyecto, la solución, la DLL, el paquete y los espacios de nombres pasan a llamarse KovaqsUI.
- Se actualizan la Demo, las pruebas, la documentación y los nombres internos de los recursos tipográficos.
- El interruptor `KovaUI.DisableEmbeddedFonts` sigue funcionando como alias de compatibilidad; el nombre recomendado es `KovaqsUI.DisableEmbeddedFonts`.
- Se estabiliza la carga privada de Outfit cuando GDI+ agrupa temporalmente los nombres de sus pesos y se comprueban los cuatro pesos con GDI.
- Las aplicaciones consumidoras deben actualizar sus referencias al ensamblado y los `using` al nuevo espacio de nombres.

## 0.4.1

- Solución integral al problema de sustitución y degradación a "Microsoft Sans Serif" al modificar el tamaño o peso de la fuente en tiempo de diseño y ejecución.
- Introducción de KFontConverter como convertidor tipográfico especializado para el PropertyGrid y serialización de Windows Forms, preservando las familias privadas (Inter y Outfit).
- Implementación de KFontManager.ResolveFont(...) y registro automático en la propiedad Font de los ocho controles de la biblioteca (KButton, KTextBox, KCheckBox, KRadioButton, KComboBox, KProgressBar, KPanel y KLabel).
- Métodos de extensión ergonómicos font.WithSize(size) y font.WithWeight(weight) en KFontExtensions para manipular dimensiones y pesos sin riesgos de degradación.
- Respeto total hacia fuentes de sistema personalizadas asignadas intencionalmente por el consumidor (como Arial, Consolas o asignaciones explícitas de Microsoft Sans Serif).
- Suite ampliada de pruebas automatizadas a 303 verificaciones, comprobando exhaustivamente la edición en PropertyGrid, instanciación por nombre, cambio de tamaño y liberación de recursos GDI.

## 0.4.0

- Inter integrada como tipografía predeterminada en los ocho controles de la biblioteca, sin requerir instalación en Windows.
- Inclusión de los cuatro archivos TTF estáticos oficiales (Regular, Medium, SemiBold y Bold) y licencia SIL OFL en recursos incrustados y NuGet.
- Introducción de KFontManager como gestor centralizado de colecciones privadas y registro dual en memoria para GDI+ y GDI (AddFontMemResourceEx).
- Exposición de la API KFonts.Inter con resolución de pesos nativos reales (400, 500, 600 y 700), conservando KFonts.Outfit para compatibilidad retroactiva.
- Actualización de los presets de KTypography a Inter para todos los controles, preservando asignaciones explícitas de la propiedad Font.
- Verificación exhaustiva de 243 pruebas automáticas cubriendo concurrencia, métricas DPI, diseñador de Visual Studio y fallback seguro a Segoe UI.

## 0.3.0

- Outfit integrada en la DLL, sin instalación en Windows, en pesos Regular, Medium, SemiBold y Bold.
- KFonts y presets centralizados KTypography; valores Font personalizados conservados.
- Carga privada única para GDI/GDI+, con fallback Segoe UI y soporte de inicialización en diseño.
- Avisos y licencia originales de la fuente incluidos en recursos y NuGet.
- Demos actualizadas y comprobaciones de pesos nativos, métricas DPI, serialización y fallback.

## 0.2.0

- Suite de ocho controles: KButton, KTextBox, KCheckBox, KRadioButton, KComboBox, KProgressBar, KPanel y KLabel.
- Temas, colores por estado, degradados, formas, bordes y sombras configurables.
- Presión suave sin desplazamiento predeterminado del texto; color e intensidad configurables. Onda de clic con entrada y salida gradual.
- Reloj compartido para animaciones, detenido en reposo y al desactivar el movimiento.
- Selección, edición, eventos y agrupación sobre los controles nativos de WinForms.
- Sin indicador de foco inicial; se activa explícitamente mediante FocusAppearance.
- Cursor de mano predeterminado en botones y controles de selección.
- Tipografía reutilizable con KLabel, conservando Font y ForeColor personalizados.
- Flecha del ComboBox configurable y animada; sincronización de colores con las propiedades nativas.
- Demos con inspector de propiedades, formulario de diseñador y guía para otra computadora.

La suite implementa un lenguaje visual propio. Las diferencias intencionales respecto al dibujo de los controles clásicos se describen en README.md.
