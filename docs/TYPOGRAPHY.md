# Tipografía integrada

Inter es la tipografía predeterminada de los ocho controles de la biblioteca. Sus cuatro archivos TTF estáticos oficiales (junto con los de Outfit para compatibilidad) se incluyen como `EmbeddedResource` dentro de `KovaUI.dll`. No se descargan fuentes en ejecución, no se escribe en la carpeta Fonts de Windows y no se requiere instalación en el sistema operativo ni acceso a Internet. El paquete NuGet contiene la misma DLL y los avisos de licencia correspondientes.

## API y propiedad de los objetos

```csharp
using KovaUI.Styling;

using var regular = KFonts.Inter(10f, KFontWeight.Regular);
using var medium = KFonts.Inter(10f, KFontWeight.Medium);
using var semibold = KFonts.Inter(10f, KFontWeight.SemiBold);
using var bold = KFonts.Inter(10f, KFontWeight.Bold);
```

Para compatibilidad, la familia Outfit sigue estando disponible a través de `KFonts.Outfit(size, weight)`.

Cada llamada crea un `Font` independiente, propiedad del consumidor. No lo deseches mientras un control lo utilice. Si lo asignas a un formulario o control, consérvalo y llama a `Dispose` después de desechar esos controles, por ejemplo al cerrar el formulario. Los controles no desechan una fuente que les hayas asignado manualmente.

Los presets de `KTypography` centralizan la tipografía de la biblioteca y retornan instancias independientes basadas en Inter:

| Preset | Tamaño en puntos | Peso | Fuente |
|---|---:|---|---|
| KTypography.Body | 10 | Regular | Inter Regular |
| KTypography.BodyMedium | 10 | Medium | Inter Medium |
| KTypography.Label | 10 | Medium | Inter Medium |
| KTypography.Button | 10 | Medium | Inter Medium |
| KTypography.Title | 23 | SemiBold | Inter SemiBold |
| KTypography.Subtitle | 11 | Regular | Inter Regular |
| KTypography.Caption | 9 | Regular | Inter Regular |
| KTypography.Section | 9 | SemiBold | Inter SemiBold |

Cada control obtiene y conserva su fuente predeterminada una sola vez al instanciarse, y la desecha al destruirse el control. `KLabel` recrea su fuente propia al cambiar `TextRole`, sin alterar las fuentes asignadas explícitamente por el consumidor. La propiedad `Font` sigue siendo completamente editable: restablecer `Font` desde el diseñador (`Properties`), llamar a `ResetFont()` o asignar `null` recupera el preset predeterminado. Cambiar de tema no sustituye una fuente personalizada. El preset propio no se serializa como una propiedad modificada manualmente.

## Gestor centralizado y duración de recursos (KFontManager)

La inicialización está centralizada en `KFontManager` mediante un contenedor `Lazy<FontResources>` thread-safe (`ExecutionAndPublication`).
Existe una única `PrivateFontCollection` en el proceso y se conservan los buffers no administrados (`Marshal.AllocHGlobal`) y los registros GDI durante la vida del proceso. No hay una caché descontrolada de instancias `Font`.

- `AddMemoryFont`: Registra las fuentes en `PrivateFontCollection` para el motor GDI+ (`Graphics.DrawString`).
- `AddFontMemResourceEx`: Registra las fuentes en GDI para `TextRenderer` y ventanas nativas (`EDIT`, `ComboBox`).

El registro es privado del proceso y Windows lo retira al terminarlo. Se conservan la colección y los búferes mientras puedan existir fuentes o ventanas nativas activas que los utilicen. Si la inicialización falla, se liberan los recursos adquiridos y se activa el fallback.

En Inter, Regular y Bold se resuelven sobre la familia `Inter` con `FontStyle.Regular` y `FontStyle.Bold`, mientras que Medium y SemiBold se resuelven sobre las familias estáticas `Inter Medium` e `Inter SemiBold`. Las pruebas en GDI confirman los pesos nativos exactos: 400, 500, 600 y 700.

## Diseñador y fallback

La carga privada funciona idénticamente en tiempo de ejecución y dentro del proceso del diseñador de Visual Studio; no requiere que el control tenga `Site` ni que `LicenseManager.UsageMode` sea runtime.

Si las políticas del sistema restringen fuentes privadas o la carga no tiene éxito, la biblioteca activa de forma automática e imperceptible un fallback a **Segoe UI**. Si esta tampoco estuviera presente, el subsistema GDI+ realiza su sustitución habitual.

Se dispone de las propiedades de diagnóstico:
- `KFonts.InterLoaded` y `KFonts.InterLoadError`
- `KFonts.OutfitLoaded` y `KFonts.OutfitLoadError`

Si se desea desactivar explícitamente las fuentes privadas a nivel de proceso (por ejemplo, para entornos restringidos o pruebas):

```csharp
AppContext.SetSwitch("KovaUI.DisableEmbeddedFonts", true);
```

## DPI y escalado

Las fuentes utilizan `GraphicsUnit.Point`. Windows Forms calcula la conversión a píxeles según el DPI de la ventana (`DeviceDpi`). No se debe multiplicar `Font.Size` por el factor de escala manualmente. Los controles ajustan sus alturas, padding y alineaciones basándose en las métricas de la fuente actual.

Se comprueban métricas a 96, 120, 144 y 192 DPI (100, 125, 150 y 200 %), garantizando escalado proporcional y alineación vertical precisa.

## Distribución y licencias

Los archivos oficiales de Inter y Outfit se redistribuyen sin alteraciones bajo la **SIL Open Font License 1.1**:
- Procedencia y hashes SHA-256 en `src/KovaUI/Resources/Fonts/Inter/NOTICE.md` y `Outfit/NOTICE.md`.
- Archivos `OFL.txt` incluidos en los recursos embebidos y empaquetados en el NuGet bajo `licenses/Inter/` y `licenses/Outfit/`.
- Licencias consultables en ejecución mediante `KFonts.InterLicense` y `KFonts.OutfitLicense`.

La licencia SIL OFL aplica exclusivamente a los archivos tipográficos. No altera ni impone licencias sobre el código de KovaUI.
