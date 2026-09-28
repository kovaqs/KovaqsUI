# KovaqsUI

Primera versión de una biblioteca de controles **Windows Forms / .NET 8**. Incluye infraestructura compartida y ocho controles: `KButton`, `KTextBox`, `KCheckBox`, `KRadioButton`, `KComboBox`, `KProgressBar`, `KPanel` y `KLabel`. Implementación propia, sin WPF ni dependencias UI externas.

El [análisis visual y diseño](docs/DESIGN.md) contiene la paleta, medidas, estados inferidos, arquitectura y alcance. La segunda entrega amplía la suite según la solicitud del usuario; KToggle y KCard no se incluyen todavía.

Consulta [Trabajar en otra computadora](docs/GETTING-STARTED.md) para clonar, compilar, usar la DLL y continuar el desarrollo. El historial de la entrega está en [CHANGELOG.md](CHANGELOG.md).

## Compilar y ejecutar

Windows con SDK .NET 8 y soporte Windows Desktop:

```powershell
dotnet build KovaqsUI.sln -c Release
dotnet run --project demo/KovaqsUI.Demo
dotnet run --project tests/KovaqsUI.SmokeTests
dotnet pack src/KovaqsUI -c Release -p:IsPackable=true -o artifacts/packages
```

En esta máquina se preparó un SDK local, excluido de Git. `./build.ps1` lo usa automáticamente si no hay `dotnet` en PATH; `./build.ps1 -Demo` ejecuta la Demo después de compilar y probar.

En la Demo, **Personalizar…** abre un inspector sobre los controles reales. Seleccione una muestra y expanda `Appearance` para cambiar Normal/Hover/Pressed/Focused/Disabled, o `FocusAppearance` para ajustar el indicador de foco. También puede editar formas, dimensiones, imágenes y opciones del editor. Los cambios son temporales y permanecen al cerrar el inspector; reiniciar la Demo recupera las muestras iniciales.

La DLL queda en `src/KovaqsUI/bin/Release/net8.0-windows/KovaqsUI.dll`. El paquete NuGet no se publica automáticamente.

## Migración desde KovaUI

Al actualizar una aplicación que use la biblioteca anterior, cambia la referencia al proyecto, DLL o paquete por `KovaqsUI` y los `using KovaUI.*` por `using KovaqsUI.*`. Las clases y sus propiedades mantienen sus nombres y comportamiento. Si desactivabas las fuentes integradas mediante `AppContext.SetSwitch`, usa `KovaqsUI.DisableEmbeddedFonts`; el nombre anterior se acepta como alias.

## Uso

```csharp
using KovaqsUI.Controls;
using KovaqsUI.Styling;

var save = new KButton {
    Text = "&Guardar",
    Variant = KButtonVariant.Primary,
    CornerRadius = 10,
    FillColor = Color.MediumSlateBlue,
    GradientColor = Color.Indigo,
    ForeColor = Color.White,
    DialogResult = DialogResult.OK
};
save.Appearance.Hover.FillColor = Color.SlateBlue;
save.Appearance.Pressed.FillColor = Color.DarkSlateBlue;
save.Appearance.Normal.GradientAngle = 45;
// Cambiar dirección o desactivar el degradado en un estado sin duplicar colores:
save.Appearance.Hover.GradientAngle = 0;
save.Appearance.Pressed.GradientMode = KGradientMode.Solid;
save.Click += (_, _) => Save();
form.Controls.Add(save);
form.AcceptButton = save;

var name = new KTextBox { PlaceholderText = "Nombre", Width = 280 };
name.TextChanged += (_, _) => Console.WriteLine(name.Text);
form.Controls.Add(name);
// Toda la API original del editor sigue disponible:
name.Editor.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
name.Editor.AutoCompleteSource = AutoCompleteSource.CustomSource;
name.Editor.AutoCompleteCustomSource.AddRange(["Ana", "Andrea"]);

var theme = KTheme.Light with { Primary = Color.Teal, OnPrimary = Color.White };
KThemeManager.Apply(form, theme);
```

Los controles no modifican los tokens del tema. Las propiedades locales prevalecen al cambiar de tema. `Color.Empty` recupera la herencia. Use `Appearance` para colores de estados; `FillColor` controla la superficie, mientras `BackColor` es el fondo del control/contenedor. `ForeColor` y `Appearance.Normal.ForeColor` se sincronizan en ambas direcciones y notifican `ForeColorChanged` una sola vez. Los colores de otros estados no modifican el color normal.

`GradientMode` permite `Inherit`, `Solid` y `Linear` por estado. `GradientAngle = null` hereda la dirección anterior (90° por defecto); un ángulo explícito funciona aunque ese estado no redefina `GradientColor`. Al pasar de Solid a Linear se conservan los colores del degradado heredado.

`FocusAppearance` permite configurar `Color`, `Thickness`, `Inset` y `Visible` en ambos controles. Por solicitud del usuario, empieza desactivado, sin color, grosor ni separación inicial. Para activarlo, establezca `Visible = true`, `Thickness` y un color. Es independiente de `BorderThickness`. El color vacío hereda `Appearance.Focused.BorderColor` o `Theme.Focus`.

`KTextBox.Appearance.Pressed` se activa al mantener el botón izquierdo del mouse sobre el marco o el editor. Se limpia al soltarlo, perder captura/foco o deshabilitar el campo; no sustituye la selección nativa del texto.

El texto deshabilitado también admite `Appearance.Disabled.ForeColor`. Windows impone `COLOR_GRAYTEXT` en un EDIT deshabilitado ([documentación de Microsoft](https://learn.microsoft.com/en-us/windows/win32/controls/wm-ctlcolorstatic)); KovaqsUI personaliza únicamente su presentación inactiva. La edición habilitada sigue siendo nativa, y se conservan el HWND, la selección y el carácter de contraseña. Windows dibuja la presentación inactiva en una máscara temporal; KovaqsUI cambia sus colores respetando la cobertura por canal de ClearType. No reconstruye líneas ni ordena caracteres. El resultado se cachea por control y se libera al invalidarse, cambiar contenido/estilo/desplazamiento, recrear la ventana o habilitar/destruir el editor. Las pruebas comparan alineación izquierda/centro/derecha de texto latino, hebreo y mixto, caracteres aislados, desplazamiento horizontal, varias líneas y contraseña. Estas muestras no sustituyen la validación de todos los idiomas y métodos de entrada.

`KButton` conserva `Button`, `IButtonControl`, `Click`, `PerformClick`, `DialogResult`, `AcceptButton/CancelButton`, mnemonic, Image/ImageList, AutoEllipsis y propiedades estándar de layout. El aspecto usa KovaqsUI: `FlatAppearance`, `FlatStyle` y `UseVisualStyleBackColor` no definen su dibujo. Al crear la ventana, `FlatStyle.System` se normaliza a `Flat` para conservar el dibujo Kova; por tanto no ofrece el modo visual System del botón estándar. Los cuatro valores heredados están probados para conservar la superficie personalizada y PerformClick. Para dibujo adicional se dispone del evento heredado `Paint` y del método virtual `OnContentPaint`. Los suscriptores de Paint reciben la superficie terminada, tanto desde referencias KButton como Control. Se conserva el ciclo de pintura de Button y se dibuja la superficie propia dentro del mismo doble buffer.

`KTextBox` es un contenedor, no una subclase de TextBox. Expone las propiedades comunes y el editor nativo mediante `Editor` para el resto de métodos/eventos. `TabStop` es true por defecto y controla también el editor: Tab/Shift+Tab entran directamente en él, sin una parada adicional en el marco; false omite todo el control. Los degradados sólo afectan al marco del campo; el área de edición es sólida. No destruya `Editor` ni lo quite de Controls.

El enlace de datos se configura sobre `Text` del contenedor mediante `DataBindings`. La edición nativa propaga `TextChanged`; el modo `OnPropertyChanged` actualiza el modelo y sus notificaciones actualizan el editor. Use `Validating` para cancelar la salida del campo; `CausesValidation` se propaga al editor. La suite verifica estas rutas y envía mensajes de entrada exclusivamente a sus propias ventanas de prueba, sin alterar el portapapeles del usuario.

## Visual Studio

Abra `KovaqsUI.sln` en una versión de Visual Studio compatible con .NET 8 y la carga de trabajo de escritorio .NET. Referencie el proyecto desde una aplicación WinForms .NET 8. Compile y agregue los controles desde Toolbox (si no aparecen automáticamente, use Choose Items y seleccione la DLL). Las clases son públicas, tienen constructor vacío y `ToolboxItem`; `Appearance` y sus estados exponen serialización de contenido y valores predeterminados para el panel Properties. Las pruebas crean los controles en un DesignSurface real, resuelven sus diseñadores, los inspeccionan con PropertyGrid y serializan/restauran los estilos anidados y el texto con CodeDomComponentSerializationService. El usuario confirmó la carga y edición en Visual Studio; esa validación manual se acepta para esta entrega.

## Estado

Esta entrega incluye los controles solicitados y una guía para continuar el desarrollo. Mantiene un diseño propio. Los límites y comprobaciones manuales pendientes están en [DESIGN.md](docs/DESIGN.md). Los temas, iconos y fuentes deben revisarse con los requisitos de accesibilidad de cada aplicación.





### Animaciones

KButton y KTextBox animan los colores de fondo, texto, borde y extremos/ángulo del degradado al cambiar de estado. La transición puede invertirse desde su valor actual sin saltos. El editor nativo conserva caret, selección y eventos.

```csharp
button.Animation.Enabled = true; // predeterminado
button.Animation.Duration = 150; // milisegundos
button.Animation.Easing = KEasing.EaseOutCubic; // también Linear y SmoothStep
field.Animation.Duration = 180;
KAnimationSettings.Enabled = false; // desactivar movimiento en toda la aplicación
```

Un temporizador compartido por hilo UI funciona sólo durante transiciones visibles. No se anima dentro del diseñador. Configure colores distintos entre estados para apreciar el efecto; no se imponen nuevos colores ni efectos decorativos. Las transiciones son configurables.

Si Visual Studio informa que no encuentra `Microsoft.NET.Sdk`, ejecute `./open-visual-studio.ps1` desde PowerShell. El script inicia el IDE con el SDK local de `.tools/dotnet` en su entorno, sin modificar las variables de Windows. La carga de trabajo de escritorio .NET debe estar instalada. Abra `DesignerSandbox.cs` en el proyecto Demo y use **Ver diseñador** para probar los controles con un formulario editable.

## Suite ampliada (0.2)

Abra **Más controles…** en la Demo o ejecute `dotnet run --project demo/KovaqsUI.Demo -- --suite`.

- **KButton**: cursor Hand de forma predeterminada. `PressDepth` controla el desplazamiento vertical del contenido al presionar (0–8 píxeles lógicos, 0 predeterminado). La respuesta predeterminada cambia suavemente la intensidad de la superficie mediante `PressColor` y `PressOpacity`, manteniendo estable el texto. `ClickAnimation` activa una onda de clic recortada por la forma; `ClickColor` define su color (vacío hereda el texto) y `ClickOpacity` su intensidad de 0 a 255. `Animation.Enabled` y `KAnimationSettings.Enabled` permiten desactivar el movimiento.
- **KCheckBox**: hereda CheckBox, incluidos ThreeState, CheckState, AutoCheck y CheckedChanged. `AppearanceStates` configura estados interactivos; `CheckedState` el relleno, degradado, borde y color de la marca. Se usa ese nombre porque `Appearance` ya es una propiedad nativa del CheckBox.
- **KRadioButton**: conserva Checked, AutoCheck, agrupación por contenedor y teclado nativos. Comparte los estilos de KCheckBox. Ambos exponen GlyphSize, TextSpacing, BorderThickness, CornerRadius y animación de selección.
- **KComboBox**: conserva Items, DataSource, DisplayMember, ValueMember, selección y edición nativas. `Appearance`, `ItemAppearance` y `SelectedItemAppearance` configuran marco e items. DropDownList es el modo inicial; DropDown mantiene un EDIT real. Simple conserva el marco nativo de lista incrustada. La lista desplegable y su geometría siguen siendo de Windows; los elementos se dibujan con OwnerDrawFixed. Si se elige DrawMode.Normal se recupera la pintura nativa de los items.
- **KProgressBar**: conserva Minimum, Maximum, Value, Step, Increment y PerformStep; anima el cambio de valor y ofrece Marquee. `Track` y `Fill` configuran colores y degradados; CornerRadius y BorderThickness, la forma.
- **KPanel**: contenedor nativo con Dock, Anchor, AutoScroll y Padding. `Surface` configura el fondo/degradado. `Shadow` permite Enabled, Color, Opacity, Blur y Offset. La sombra se dibuja dentro de sus límites y reserva espacio para los controles acoplados. Se cachea hasta cambiar tamaño, DPI o parámetros de sombra.

La Demo incluye un contador compuesto con dos KButton, un KLabel y un KPanel, inspirado en la referencia − / 3 / +. No es un control NumericUpDown independiente. Todos los controles admiten temas claro/oscuro y sustitución de colores; los colores iniciales sólo son valores del tema.

Las transiciones de color, selección, presión, clic, progreso y marquee comparten un reloj por hilo UI. El reloj se detiene cuando no quedan animaciones; sólo Marquee requiere movimiento continuo mientras está visible. El foco sigue sin borde predeterminado. Los controles de selección admiten también Appearance.Button, Image/ImageList, ImageAlign y TextImageRelation, conservando el comportamiento nativo de selección.

La comprobación manual del diseñador realizada por el usuario se acepta. No se bloquea esta entrega intentando automatizar de nuevo el IDE. Las pruebas automáticas cubren comportamiento básico y recursos.



## KLabel: tipografía reutilizable

Los títulos y textos de las demos ahora usan KLabel, disponible en la misma DLL y en el Toolbox. Hereda de Label y conserva AutoSize, AutoEllipsis, TextAlign, Image, UseMnemonic, Padding y el comportamiento nativo.

TextRole ofrece Body, Title, Subtitle, Caption y Section con los presets de KTypography e Inter integrada. Font y ForeColor siguen siendo editables: los valores explícitos se conservan al cambiar el tema o el rol. Restablecer Font recupera la tipografía del rol; restablecer ForeColor recupera el color del tema. El fondo es transparente por defecto. Las muestras se pueden editar desde Personalizar….

## Inter integrada por defecto (0.4)

Todos los controles utilizan **Inter** como fuente predeterminada sin necesidad de instalar fuentes en Windows. Sus cuatro pesos estáticos oficiales (Regular, Medium, SemiBold y Bold) y sus avisos de licencia SIL OFL viajan dentro de la DLL y el paquete NuGet.

La carga privada en memoria se centraliza mediante `KFontManager`, con registro dual para GDI+ (`PrivateFontCollection`) y GDI (`AddFontMemResourceEx`), garantizando renderizado nítido en el editor nativo de texto (`KTextBox`), combos y botones, con respaldo automático a Segoe UI.

`KFonts.Inter(10f, KFontWeight.Medium)` crea una fuente propia del consumidor. `KTypography` centraliza los presets Body, BodyMedium, Label, Button, Title y Caption basados en Inter. Para compatibilidad retroactiva, la familia Outfit permanece disponible mediante `KFonts.Outfit(size, weight)`. Cada resultado de `KFonts` o `KTypography` es un Font independiente que debe desecharse cuando ya no lo utilicen los controles. No solicites presets en Paint.

Al modificar el tamaño de fuente desde el diseñador de Visual Studio (`PropertyGrid`), código generado o mediante métodos como `font.WithSize(size)` o `font.WithWeight(weight)`, `KFontConverter` y `KFontManager.ResolveFont` preservan automáticamente la familia privada y evitan cualquier sustitución o degradación silenciosa a "Microsoft Sans Serif".

Consulta [tipografía, diseño, DPI y gestión de recursos](docs/TYPOGRAPHY.md), [avisos de Inter](src/KovaqsUI/Resources/Fonts/Inter/NOTICE.md) y [avisos de Outfit](src/KovaqsUI/Resources/Fonts/Outfit/NOTICE.md). La licencia SIL OFL corresponde únicamente a los archivos tipográficos.
