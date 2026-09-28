<!-- Registro histórico: los pendientes antiguos del IDE fueron sustituidos por la validación manual del usuario. -->
# Registro de verificación manual

Esta guía registra comprobaciones no cubiertas por la ejecución automática. Ninguna casilla implica aprobación por defecto. Anotar versión de Windows, versión del IDE, escala del monitor y resultado observado.

## Diseñador y Toolbox

Requisito: Visual Studio con soporte .NET 8 y carga de trabajo de desarrollo de escritorio .NET. La instalación actual se identifica como Visual Studio Community 2026 (18.10); falta completar la prueba directa del diseñador.

1. Abrir KovaqsUI.sln y compilar Release.
2. Crear una aplicación Windows Forms .NET 8 de prueba y agregar referencia al proyecto KovaqsUI.
3. Abrir Form1 en el diseñador. Confirmar KButton y KTextBox en Toolbox; si no aparecen automáticamente, registrar el resultado de Choose Items con la DLL compilada.
4. Arrastrar ambos controles. Verificar tamaño, selección, movimiento, alineación, Dock y Anchor, sin excepciones del diseñador.
5. Editar Appearance.Normal, Hover, Pressed, Focused y Disabled. Probar colores arbitrarios, GradientColor, GradientAngle y GradientMode.
6. Editar CornerRadius, Shape, BorderThickness, Font, Padding e Image/ImageList. En el campo, editar placeholder, multiline, contraseña e iconos.
7. Guardar, cerrar el diseñador y volverlo a abrir. Confirmar que persisten valores y que el editor interno no aparece duplicado.
8. Restablecer un color/ángulo desde Properties. Guardar y reabrir para verificar la recuperación de la herencia.
9. Ejecutar el formulario y comprobar que la apariencia coincide con lo configurado.

Resultado: **pendiente**. La prueba automática de DesignSurface/CodeDOM es complementaria y no sustituye estos pasos.

## DPI físico

1. Ejecutar la Demo con escala 100 %, 125 %, 150 % y 200 % en Windows; registrar DeviceDpi y capturas.
2. Revisar botones de 32/40/48, iconos, texto, bordes, foco y campos con fuente 10/13. Buscar recortes, doble escalado y desalineación.
3. Cambiar el tamaño de la ventana y comprobar scroll y contenido.
4. Si hay dos monitores con escalas distintas, mover la ventana entre ellos varias veces. Verificar que fuente, editor interno, imágenes y bordes cambian juntos y recuperan sus proporciones al volver.
5. Repetir con la ventana maximizada y con el campo enfocado.

Resultado: **pendiente**. Los mensajes WM_DPICHANGED sintéticos sólo verifican la ruta del formulario; los controles hijos siguen consultando el DPI físico.

## Edición y accesibilidad de extremo a extremo

1. En el campo de la Demo, probar Ctrl+A/C/X/V, menú contextual y Ctrl+Z con texto Unicode de prueba. Registrar selección y caret antes/después.
2. Con un IME instalado, componer y confirmar texto, cancelar composición y mover el caret; repetir en multilínea.
3. Con Narrador u otro lector de pantalla, recorrer con Tab/Shift+Tab. Confirmar nombre, rol, valor, readonly, contraseña y deshabilitado; activar el botón mediante su acción accesible.
4. Cambiar AccessibleName y AccessibleDescription durante ejecución y comprobar el anuncio actualizado.
5. Verificar Enter/Escape en un diálogo modal y la cancelación de Validating al salir del campo.

Resultado: **pendiente**. Las pruebas MSAA, WM_CHAR y de enlace de datos no equivalen a una sesión completa de lector de pantalla, IME o portapapeles.

## Registro

| Entorno y versión | Caso | Resultado observado | Evidencia | Incidencia |
|---|---|---|---|---|
| Pendiente | Toolbox/diseñador | Sin ejecutar | — | — |
| Pendiente | DPI físico | Sin ejecutar | — | — |
| Pendiente | Clipboard/IME/lector | Sin ejecutar | — | — |

Las diferencias visuales deliberadas están documentadas en README.md: KTextBox usa composición, su zona editable es sólida y KButton conserva el dibujo Kova al normalizar FlatStyle.System. No debe afirmarse compatibilidad visual idéntica con Button.


## Validación parcial en Visual Studio — 2026-09-27

Visual Studio Community 2026 abrió `DesignerSandbox.cs [Diseño]` con `actionButton` y `nameField`. El árbol accesible del IDE muestra el botón «Continuar» y un solo editor nativo bajo el campo. La lista de errores muestra cero errores y cero advertencias. El guardado mediante Ctrl+S quitó la marca de cambios y generó InitializeComponent en DesignerSandbox.Designer.cs. Se conservaron los ajustes hechos por el usuario en tamaño, posición y TabStop.

Toolbox muestra el grupo KovaqsUI con `KButton (KovaqsUI.Controls)` y `KTextBox (KovaqsUI.Controls)`. Esto verifica descubrimiento en el IDE. Aún no verifica inserción por arrastre, edición y reapertura de todas las propiedades anidadas. La captura gráfica del escritorio sigue fallando (`FrameArrived timed out`); la evidencia anterior procede del árbol de accesibilidad y de los archivos guardados, no de una inspección visual por captura.

Se corrigió el metadato predeterminado de KTextBox.TabStop a true, coincidiendo con el constructor. La prueba de TypeDescriptor verifica que Restablecer recupera true y sincroniza el editor interno. Suite actual: 160 comprobaciones aprobadas.

### Evidencia adicional del usuario

La captura `codex-clipboard-8a56b00e-e106-4865-a5af-041a50ca9a09.png` muestra Animation expandido (Duration 150, Easing Linear, Enabled True) y Appearance con estados y propiedades de degradado. Confirma que esas propiedades se pueden desplegar en el IDE; no prueba todavía su persistencia tras cerrar y reabrir.

El archivo DesignerSandbox.Designer.cs guardado contiene dos instancias adicionales, kButton1 y kTextBox1, con declaraciones, construcción, propiedades y Controls.Add. La solución con esas instancias compila en Release sin errores ni advertencias. Esto prueba que las instancias añadidas quedaron guardadas y son compilables; el gesto concreto de arrastre no fue observado. El archivo aún no contiene asignaciones Animation.Duration o Animation.Easing: queda pendiente comprobar un valor modificado y guardado, seguido de reapertura del diseñador.

### Guardado de propiedades anidadas confirmado

Tras la edición del usuario, DesignerSandbox.Designer.cs contiene `kButton1.Animation.Duration = 180`, `kButton1.Animation.Easing = Styling.KEasing.Linear` y `kButton1.Appearance.Normal.BorderColor = Color.Black`. Visual Studio muestra la pestaña sin cambios pendientes y Properties identifica kButton1 como KovaqsUI.Controls.KButton. La solución con ese código compila Release con cero errores y advertencias. Queda confirmado el guardado real desde Properties de Animation y Appearance; no sólo el despliegue de sus categorías.

Se cerró la pestaña de diseñador mediante Ctrl+F4 para probar recarga. La selección posterior del archivo falló con `coordinate input geometry is unavailable`, por lo que la reapertura todavía no se declara verificada. No se modificaron los valores elegidos por el usuario.

## Decisión del usuario

El usuario confirma haber realizado cambios y verificado el diseñador, y solicita omitir la automatización del IDE cuando no sea posible. Se acepta esta validación manual; los pasos pendientes de automatización no bloquean la ampliación solicitada. No se presentan como ejecutadas por el agente las comprobaciones que realizó el usuario.


