# Trabajar en otra computadora

## Preparación

Instala Git y Visual Studio 2022 (17.8 o posterior) con la carga de trabajo **Desarrollo de escritorio de .NET** y el SDK .NET 8. Inicia sesión en GitHub con la cuenta que tiene acceso al repositorio privado y clónalo desde Visual Studio o `git clone` usando su URL HTTPS.

```powershell
git clone https://github.com/kovaqs/KovaUI.git
cd KovaUI
```

Abre `KovaUI.sln` y establece `KovaUI.Demo` como proyecto de inicio. Compila y ejecuta con F5. **Más controles…** abre la galería ampliada; **Personalizar…** permite editar las muestras en vivo. `DesignerSandbox.cs` es el formulario editable para probar el diseñador.

No necesitas el SDK local de esta máquina ni copiar carpetas bin/obj. El repositorio contiene el código fuente completo, los proyectos, recursos y pruebas.

## Línea de comandos

Desde la carpeta del repositorio, en Windows:

```powershell
dotnet build KovaUI.sln -c Release
dotnet run --project demo/KovaUI.Demo -c Release --no-build
dotnet run --project demo/KovaUI.Demo -c Release --no-build -- --suite
```

Para las pruebas de comportamiento, usa una sesión de escritorio de Windows:

```powershell
dotnet run --project tests/KovaUI.SmokeTests -c Release --no-build
```

Las pruebas abren ventanas temporales. No necesitan automatizar Visual Studio. Para generar el paquete local:

```powershell
dotnet pack src/KovaUI/KovaUI.csproj -c Release -p:IsPackable=true -o artifacts/packages
```

## Usar los controles en tu aplicación

En un proyecto WinForms .NET 8, agrega una referencia al proyecto `src/KovaUI/KovaUI.csproj`, o a la DLL compilada en `src/KovaUI/bin/Release/net8.0-windows/KovaUI.dll`. También puedes instalar el paquete generado desde una fuente NuGet local.

```csharp
using KovaUI.Controls;
using KovaUI.Styling;

var button = new KButton { Text = "Guardar", Width = 160 };
button.FillColor = Color.SlateBlue;
button.GradientColor = Color.Indigo;
button.ForeColor = Color.White;
button.Appearance.Hover.FillColor = Color.MediumSlateBlue;
button.Animation.Duration = 180;
button.PressColor = Color.White;
button.PressOpacity = 20;
button.ClickOpacity = 24;
Controls.Add(button);
```

Los colores locales prevalecen sobre el tema. `Color.Empty` recupera la herencia. Para etiquetas, usa `KLabel.TextRole` y personaliza normalmente `Font` y `ForeColor`. Para sombras del panel, expande `Shadow` en Properties.

## Animaciones

`Animation.Duration`, `Easing` y `Enabled` controlan las transiciones. La presión del botón usa un cambio gradual de intensidad (`PressColor` y `PressOpacity`); `PressDepth` es cero por defecto para mantener estable el texto. Un valor mayor permite el desplazamiento opcional en píxeles lógicos. La onda de clic tiene entrada y salida gradual, con color y opacidad configurables. `ClickAnimation = false` la desactiva.

`KAnimationSettings.Enabled = false` desactiva el movimiento global. El temporizador compartido se detiene cuando no quedan animaciones. No se ejecutan animaciones en el diseñador.

## Continuar el trabajo

Antes de cambiar de computadora, guarda tus cambios con un commit y haz push. En la otra, usa pull antes de editar. Las carpetas `.vs`, `.tools`, `bin`, `obj` y `artifacts` no se suben porque se regeneran o contienen estado local.

Los cambios hechos en el inspector de la Demo son temporales: pásalos al diseñador o al código de tu aplicación para guardarlos.

## Compilación en GitHub Actions (opcional)

La plantilla está en `docs/ci/windows-build.yml`. No está activada: el acceso usado para la subida no incluye el permiso workflow. Para activarla posteriormente, copia ese archivo a `.github/workflows/build.yml` desde una sesión autorizada de GitHub. Compila y empaqueta en Windows; no ejecuta las pruebas que abren ventanas de escritorio.

