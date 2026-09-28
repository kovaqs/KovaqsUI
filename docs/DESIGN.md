# KovaUI · definición visual y técnica

## 1. Lectura de la referencia

La imagen muestra una interfaz clara y espaciosa: controles compactos arriba, filtros de mayor anchura, chips informativos y tarjetas con cifras grandes. El contraste jerárquico viene del tamaño, del espacio y de dos acentos (naranja y casi negro), no de efectos volumétricos. Los contenedores son blancos, con un borde gris fino y una sombra corta y difusa. Los iconos acompañan al texto y no compiten con él. El texto descriptivo es gris medio; las cifras son casi negras. Verde comunica variación positiva y azul identifica enlaces.

La captura está ampliada y no revela su DPI ni su escala de origen. Las medidas siguientes son una reconstrucción en **píxeles lógicos a 96 DPI**, no mediciones exactas del producto. Los estados interactivos no pueden observarse en esta imagen estática: se proponen explícitamente más abajo.

## 2. Paleta aproximada

| Token | Valor inicial | Uso |
|---|---|---|
| Background | `#F8F8F8` | Fondo general |
| Surface | `#FFFFFF` | Campos, botones neutros, futuras tarjetas |
| SurfaceHover | `#F4F4F4` | Hover neutro |
| SurfacePressed | `#E9E9E9` | Presión neutra |
| Border | `#DADADA` | Contorno de 1 px |
| TextPrimary | `#0D1517` | Texto principal y cifras |
| TextSecondary | `#777777` | Descripciones |
| Primary | `#FF7E1C` | Acción principal |
| PrimaryHover / Pressed | `#F27314` / `#DD630C` | Interacción propuesta |
| Secondary | `#0D1517` | Acción oscura |
| Danger | `#D0411E` | Texto de acción destructiva |
| Focus | `#2E72DA` | Foco visible |
| DisabledFill / Text / Border | `#EEEEEE` / `#929292` / `#E0E0E0` | Deshabilitado |
| Positivo (futuro) | `#22C55E` | Indicadores positivos |
| Enlace (futuro) | `#347CF5` | Navegación |

La referencia usa texto blanco sobre naranja. La propuesta inicial usa texto oscuro en Primary para mejorar su legibilidad; `OnPrimary` y `ForeColor` permiten reproducir el blanco de la referencia. Estos colores son **valores iniciales reemplazables**, no restricciones. El tema oscuro es una extensión propuesta, no una inferencia de la imagen.

## 3. Geometría, alturas y espacio

| Familia | Tokens lógicos propuestos |
|---|---|
| Radio | Small 8, Medium 12, Large 16 |
| Espacio | XS 4, S 8, M 12, L 16, XL 24 |
| Alturas | Small 32, Medium 40, Large 48 |
| Bordes | 1 por defecto; grosor configurable |
| Padding de botón | 12 horizontal, 4 vertical |
| Padding de campo | 12 horizontal, 8 vertical |
| Icono | 18 × 18, separación 8 |
| Tipografía | Segoe UI del sistema; heredar `Font` del formulario |
| Jerarquía | 10–11 pt controles, 9 pt rótulos, 24–28 pt encabezado |

El padding vertical del botón pequeño se reduce para que el texto no quede cortado. El editor se centra mediante su altura nativa; en multilínea ocupa el área interior. Fuentes grandes requieren aumentar la altura o usar `GetPreferredSize`/AutoSize donde corresponde. No se fuerza una fuente privada en cada control.

## 4. Estados y propuesta de controles

**KButton:** Primary naranja, Secondary oscuro, Outline blanco con borde, Ghost sin borde y Danger con texto rojizo. Las variantes proceden del tema. Radio 12, bordes de 1, texto centrado e imagen opcional con relaciones izquierda/derecha/arriba/abajo/superpuesta. Hover y pressed cambian discretamente el fondo; focused muestra un anillo interior; disabled usa tokens específicos. El botón por defecto también se identifica con el anillo. Se permiten rectángulos, esquinas redondeadas, cápsulas y elipses. Las formas afectan al dibujo; el área de hit testing sigue siendo rectangular como en WinForms.

**KTextBox:** superficie blanca, borde gris, radio 12, editor nativo sin borde interior. Hover modifica la superficie; el foco cambia el borde a azul. Placeholder, selección, contraseña, readonly, multilínea, scroll y autocompletado usan el editor WinForms. Iconos izquierdo y derecho opcionales con eventos de clic. Un icono que represente una acción indispensable debe tener una alternativa de teclado en la aplicación; estos iconos no son botones accesibles separados.

**Degradados:** opcionales mediante `GradientColor` y `Appearance.*.GradientAngle`. Cada estado admite `GradientMode` (Inherit, Solid o Linear), y un ángulo nullable que hereda cuando está vacío. Así se puede cambiar únicamente la dirección, apagar el degradado durante la presión o recuperarlo después sin duplicar sus colores. La Demo naranja cambia la dirección en hover y usa relleno sólido al presionar. Sin sombras grandes, brillos ni animaciones de adorno. Los campos tienen marco degradable, pero el área de edición mantiene fondo sólido por ser una ventana EDIT nativa. No se implementa una falsa transparencia que estropee selección o caret.

**Selected/checked/indeterminate:** no corresponden a estos dos controles. Se reservan para los seis controles pendientes; esta versión no simula un CheckBox mediante KButton.

## 5. Arquitectura mínima

```text
src/KovaUI/
  Controls/KButton.cs      hereda Button
  Controls/KTextBox.cs     UserControl + TextBox nativo
  Styling/KTheme.cs        tema inmutable, métricas, variantes y formas
  Styling/KStateStyle.cs   estados editables y serializables
  Drawing/KRenderer.cs    resolución de estados y geometría GDI+
demo/KovaUI.Demo/          evaluación conjunta
tests/KovaUI.SmokeTests/   comportamiento, metadatos y recursos
```

No se impone un KControl común que obligue a perder las bases nativas. Se comparte el renderizador, la geometría y el sistema de estilos. Los temas se aplican a un árbol de controles explícitamente, sin registros estáticos que retengan formularios destruidos. Los controles agregados después deben recibir el tema al añadirse o reaplicar `KThemeManager.Apply`.

Los estilos vacíos heredan. Se resuelve tema → Normal → Focused → Hover → Pressed; Disabled tiene prioridad exclusiva y sus propios valores. Un color normal personalizado se conserva en hover/pressed salvo un override explícito. Los cambios invalidan el control inmediatamente, también al editar propiedades anidadas. El foco no agrega un borde automáticamente; Appearance.Focused y FocusAppearance permiten configurarlo explícitamente.

## 6. Rendimiento y alcance de validación

Renderizado bajo demanda y doble buffer. Las transiciones usan un temporizador por hilo UI, activo únicamente mientras hay controles visibles animándose. No se generan imágenes por frame para las transiciones. Los paths, pinceles y plumas se liberan en cada repintado. No se cachean recursos dependientes de tamaño/DPI/tema antes de tener evidencia de necesidad; el test de GDI comprueba que no se acumulan handles. Las imágenes proporcionadas son del consumidor y no se liberan al destruir el control.

La Demo permite tema claro/oscuro, tamaños, colores personalizados, degradados, readonly, contraseña, multilínea, navegación de teclado, cambio de fuente y Enabled. La captura mediante DrawToBitmap no siempre incluye el cue banner (placeholder) del EDIT nativo; comprobarlo en la ventana viva.

Se verifican eventos, selección, Unicode, Space, mnemonic, propiedades de diálogo, estilos del diseñador y estrés de dibujo. La prueba de tamaños no equivale a cambiar el DPI del monitor. Antes de producción: validación manual en Visual Studio, narrador/lector de pantalla, IME, portapapeles, Tab/Shift+Tab y diálogos modales reales, y 100/125/150/200 % de DPI. No se afirma que esas comprobaciones manuales estén realizadas.


### Flecha de KComboBox

El grupo Kova Arrow permite configurar ArrowColor, DisabledArrowColor, ArrowSize y ArrowThickness. Los colores vacíos heredan el texto del estado correspondiente; tamaño o grosor cero ocultan la flecha. Las medidas son lógicas y se escalan con DPI, limitando el dibujo al espacio disponible. Abrir y cerrar el desplegable rota la flecha usando Animation y el reloj compartido, sin cambiar la selección o el teclado nativos.

