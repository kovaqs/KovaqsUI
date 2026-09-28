# Entrega 0.2.0

## Alcance entregado

La solicitud inicial se amplió durante el desarrollo. Esta entrega incluye KButton, KTextBox, KCheckBox, KRadioButton, KComboBox, KProgressBar, KPanel y KLabel, con demos, código fuente y empaquetado NuGet local. KToggle y KCard quedaron como propuestas futuras; no forman parte de esta ampliación solicitada.

| Solicitud | Implementación y evidencia |
|---|---|
| WinForms .NET moderno, DLL reutilizable | Solución net8.0-windows, sin dependencias UI externas. Compilación Release completa sin errores ni advertencias. |
| Diseño consistente y configurable | KTheme, KAppearance, geometría compartida, paleta sustituible y degradados por estado; demos clara/oscura e inspector de propiedades. |
| Comportamiento nativo | KButton deriva de Button; selección y progreso usan sus bases WinForms; KTextBox conserva un TextBox real accesible mediante Editor. Pruebas de teclado, eventos, selección, validación, enlace de datos y agrupación. |
| Animación de presión y clic | Intensidad gradual sin movimiento predeterminado del texto; onda de clic con entrada/salida gradual; colores, opacidad, duración y easing editables. |
| Optimización | Reloj compartido detenido en reposo, cancelación al desactivar movimiento, recursos gráficos liberados y caché de sombra. Pruebas de 2.000 renderizados y recursos GDI. |
| Foco sin borde inicial | FocusAppearance comienza oculto, sin color ni grosor. |
| Mano como cursor | Cursor Hand por defecto en KButton y controles de selección. |
| Etiquetas de las demos reutilizables | KLabel con roles tipográficos y personalización de fuente/color; integrado en ambas demos. |
| Diseñador | Atributos de propiedades y pruebas de serialización; validación manual del usuario aceptada, sin volver a bloquear por automatización del IDE. |
| Continuar en otro equipo | Guía GETTING-STARTED.md, solución completa y plantilla opcional de compilación Windows. No requiere archivos locales ignorados. |

Validación local final: **194 comprobaciones correctas**. Las pruebas comprueban rutas concretas; no equivalen a certificar cada combinación posible de WinForms, todos los métodos de entrada o cada monitor físico. Las diferencias intencionales del renderizado respecto a WinForms se explican en README.md. Los documentos de validación anteriores se conservan como historial técnico.

## Privacidad del repositorio

El commit utiliza una dirección de GitHub noreply. Se excluyen credenciales, archivos .env, certificados, claves, estado del IDE, SDK y herramientas locales, compilaciones y capturas temporales.


