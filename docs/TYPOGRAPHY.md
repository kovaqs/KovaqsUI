# Tipografía integrada

Outfit es la tipografía predeterminada de los ocho controles. Sus cuatro TTF estáticos se incluyen como EmbeddedResource dentro de KovaUI.dll. No se descargan fuentes en ejecución, no se escribe en la carpeta Fonts de Windows y no se requiere instalación ni acceso a Internet. El paquete NuGet contiene la misma DLL.

## API y propiedad de los objetos

```csharp
using KovaUI.Styling;

using var regular = KFonts.Outfit(10f, KFontWeight.Regular);
using var medium = KFonts.Outfit(10f, KFontWeight.Medium);
using var semibold = KFonts.Outfit(10f, KFontWeight.SemiBold);
using var bold = KFonts.Outfit(10f, KFontWeight.Bold);
```

Cada llamada crea un Font independiente, propiedad del consumidor. No lo deseches mientras un control lo utilice. Si lo asignas a un formulario o control, consérvalo y llama a Dispose después de desechar esos controles, por ejemplo al cerrar el formulario. Los controles no desechan una Font que les hayas asignado. El código de las demos muestra esa propiedad explícita.

Los presets también crean un Font independiente en cada acceso. Obtén el preset una vez; no lo solicites desde Paint ni en cada frame:

| Preset | Tamaño en puntos | Peso |
|---|---:|---|
| KTypography.Body | 10 | Regular |
| KTypography.BodyMedium | 10 | Medium |
| KTypography.Label | 10 | Medium |
| KTypography.Button | 10 | Medium |
| KTypography.Title | 23 | SemiBold |
| KTypography.Subtitle | 11 | Regular |
| KTypography.Caption | 9 | Regular |
| KTypography.Section | 9 | SemiBold |

Cada control obtiene y conserva su fuente predeterminada una sola vez, y la desecha con el control. KLabel recrea su fuente propia al cambiar TextRole, sin tocar las fuentes del consumidor. Font sigue siendo editable. Restablecer Font desde Properties, ResetFont() o asignar null recupera el preset. Cambiar de tema no sustituye una fuente personalizada. El preset propio no se serializa como una fuente elegida manualmente.

KLabel utiliza Label para su rol Body predeterminado, y los presets correspondientes para Title, Subtitle, Caption y Section. KButton utiliza Button; checkbox y radio utilizan Label; campo de texto, combo, panel y progreso utilizan Body.

## Carga privada y duración de recursos

La inicialización es Lazy con ExecutionAndPublication. Existe una sola PrivateFontCollection y se conservan cuatro buffers y cuatro registros GDI durante la vida del proceso. No hay una caché creciente de objetos Font. La creación de fuentes se sincroniza sobre el estado compartido.

AddMemoryFont registra las fuentes para GDI+. AddFontMemResourceEx las registra para GDI, TextRenderer y ventanas nativas EDIT/ComboBox. El registro es privado del proceso y Windows lo retira al terminarlo. Se conservan la colección y los buffers mientras puedan existir fuentes o ventanas que los utilicen; no hay un método público de descarga que pueda invalidarlas prematuramente. En una inicialización fallida se liberan los recursos adquiridos y se selecciona el fallback.

Regular/Bold usan la familia Outfit. Medium y SemiBold usan las familias estáticas Outfit Medium y Outfit SemiBold. Por eso Font.Bold puede ser false para SemiBold: su grosor procede del archivo de esa familia, no de aplicar negrita sintética. Las pruebas verifican los pesos efectivos 400/500/600/700 en GDI.

## Diseñador y fallback

La misma carga privada se intenta en runtime y en el proceso del diseñador; no requiere que el control tenga Site ni que LicenseManager.UsageMode indique runtime. Los recursos están disponibles desde el ensamblado antes de crear el HWND.

Si faltan recursos o la plataforma rechaza su registro, se usa Segoe UI. Si esa familia tampoco existe, System.Drawing realiza su sustitución habitual. OutfitLoaded permite consultar el registro y OutfitLoadError contiene el diagnóstico de carga. Los tamaños y pesos inválidos de la API sí producen ArgumentOutOfRangeException.

Un host que restrinja las fuentes privadas puede elegir el fallback antes de crear cualquier control o acceder a KFonts:

```csharp
AppContext.SetSwitch("KovaUI.DisableEmbeddedFonts", true);
```

La decisión de carga se toma una vez por proceso; para cambiarla es necesario reiniciar. Las fuentes privadas no se enumeran como fuentes instaladas en el selector de Windows. Los controles las usan automáticamente aunque el selector no las liste.

## DPI

Las fuentes utilizan GraphicsUnit.Point. WinForms convierte puntos a píxeles según el DPI de la ventana. No multipliques Font.Size por DeviceDpi/96 al asignarla. El editor nativo mantiene PreferredHeight, y AutoSize/GetPreferredSize miden el texto con la fuente actual. El consumidor debe usar AutoScaleMode apropiado y habilitar PerMonitorV2, como en la Demo.

Se comprueban las métricas a 96, 120, 144 y 192 DPI (100/125/150/200 %), además del renderizado y el comportamiento nativo. Estas comprobaciones no afirman una prueba física con cuatro monitores.

## Distribución y licencia de la fuente

Los archivos oficiales se redistribuyen sin modificaciones. La procedencia y hashes están en Resources/Fonts/Outfit/NOTICE.md dentro del proyecto de biblioteca; OFL.txt conserva el copyright y la licencia originales. Ambos se incluyen en el NuGet bajo licenses/Outfit.

La licencia completa también está embebida en la DLL y es consultable mediante KFonts.OutfitLicense, de modo que viajar sólo con la DLL conserva los avisos. Puedes mostrar esa propiedad en la sección de créditos de tu aplicación. Conserva estos avisos al redistribuir.

La SIL Open Font License 1.1 se aplica a Outfit. No se añade una licencia al código de KovaUI ni se declara la licencia de la fuente como licencia general del paquete.

Fuentes oficiales: [Outfit](https://github.com/Outfitio/Outfit-Fonts/tree/902773808eb372f70fb34e8946dd1ffe604efc79), [OFL original](https://github.com/Outfitio/Outfit-Fonts/blob/902773808eb372f70fb34e8946dd1ffe604efc79/OFL.txt), [registro privado GDI](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-addfontmemresourceex).
