# Cambios

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
