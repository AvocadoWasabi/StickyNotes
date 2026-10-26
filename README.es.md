[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

[v0.0.11](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.11) · Windows 10 / 11 (x64)

Aplicación de Windows para mostrar y editar archivos Markdown en ventanas de notas adhesivas. Admite archivos locales, notas de Obsidian y secciones concretas de notas diarias. Las ediciones y los cambios en las casillas se guardan en el archivo Markdown original.

Admite resultados de consultas Tasks y Dataview mediante Obsidian CLI, así como la obtención de eventos de Google Calendar y tareas de Google Tasks.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [Instalación y configuración](docs/INSTALL.es.md) · [Ejemplos de pantalla](docs/SCREENSHOTS.es.md) · [v0.0.11](CHANGELOG.md#v0-0-11)

## Ejemplos de pantalla

| Uso de casillas | Visualización de Markdown | Sección de la nota diaria |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-es.png" alt="Uso de casillas" width="280">](docs/images/sticky-tasks-es.png) | [<img src="docs/images/sticky-markdown-es.png" alt="Visualización de Markdown" width="280">](docs/images/sticky-markdown-es.png) | [<img src="docs/images/sticky-daily-es.png" alt="Sección de la nota diaria" width="280">](docs/images/sticky-daily-es.png) |

Notas de ejemplo renderizadas con los controles WPF de la aplicación. No son capturas del escritorio. Consulta los [ejemplos por idioma y las instrucciones de generación](docs/SCREENSHOTS.es.md).

## Requisitos del sistema y dependencias

Windows 10 / 11 (x64). El ZIP incluye .NET; no necesitas permisos de administrador. CLI está activada en las configuraciones nuevas: desactívala para archivos locales. No hace falta un complemento específico de Sticky Notes para Obsidian.

| Uso | Requisito | Configuración |
| --- | --- | --- |
| Solo Markdown | Esta aplicación | Desactiva Obsidian CLI y elige una carpeta local |
| Notas de Obsidian | Obsidian abierto + CLI integrada | Activa Settings → General → Command line interface y conecta la bóveda |
| Notas diarias | CLI + complemento principal Daily notes | Configura carpeta/formato de fecha y crea la nota de hoy |
| Consultas Tasks | CLI + complemento Tasks | Actívalo en la bóveda; v0.0.11 requiere la versión estable 8.2.2+ |
| Consultas Dataview | CLI + Dataview | Actívalo en la bóveda; LIST / TABLE / TASK |
| Google | Cuenta de Google + configuración OAuth propia | Activa Calendar API / Tasks API, importa JSON OAuth de escritorio y autoriza |

[Obsidian CLI](https://obsidian.md/help/cli) · [Google OAuth (English)](docs/GOOGLE-CALENDAR.en.md#setup)

## Instalación y configuración inicial

1. Descarga `StickyNotes-win-x64.zip` de Releases, extrae todo y ejecuta `Install.cmd`.
2. Abre Obsidian, activa CLI y mantén abierta la bóveda que quieras usar.
3. En los ajustes de la aplicación comprueba la ruta absoluta de `Obsidian.com` y el nombre/ID de la bóveda. Obtén la carpeta mediante CLI, elige una carpeta de notas y guarda.
4. En `…`, crea/abre una nota o vincula una sección o nota diaria.

La configuración de archivos locales, las actualizaciones y la desinstalación se describen en la [guía de instalación](docs/INSTALL.es.md). Las funciones y ajustes corresponden a v0.0.11. Los cambios por versión figuran en el [registro de cambios (en inglés)](CHANGELOG.md#v0-0-11).

## Funciones

| Funciones | Uso |
| --- | --- |
| Markdown | Títulos, listas, casillas, tablas, código y enlaces; archivos `.md` normales |
| Secciones y notas diarias | Mostrar y editar una sección, seguir los cambios de fecha y conservar la nota de ayer si falta la de hoy |
| Tasks / Dataview | Resultados de consultas y casillas con correspondencia verificada al origen; conserva las consultas |
| Entrada de tareas | Sugerencias de palabras/fechas, Enter continúa listas, Tab sangra y atajo configurable para casillas |
| Google | `@calendar today`: eventos y tareas pendientes con fecha; edita título/descripción del evento tras confirmar |
| Ventanas | Colores, tamaño, escala 50–200 %, fijación, primer plano temporal y barra de tareas |
| Gestión | Selecciona/migra carpeta, reabre notas, explora archivos, previsualiza, renombra y elimina |
| Copias de seguridad y detección de conflictos | Copias antes de escribir y comprobación de cambios externos |
| Varias bóvedas | Notas diarias por bóveda, nombres editables y bóveda predeterminada |

## Uso básico

| Acción | Método |
| --- | --- |
| Editar | Clic en el texto (uno/dos clics configurable) o `Ctrl+E` |
| Guardar y terminar la edición | `Ctrl+S` / `Esc`; el primer Esc cierra las sugerencias abiertas |
| Marcar una tarea | Clic en la casilla en modo lectura |
| Primer plano | Fijar con `○ / ●`; `Alt+Z` muestra todas las notas al frente durante 10 segundos |
| Cerrar / salir | Cerrar la nota desde `…` / salir de la aplicación desde el área de notificación |

## Alcance y limitaciones

- La interfaz, README, instalación e imágenes admiten los nueve idiomas de arriba. Elige el idioma en ajustes, guarda y reinicia. Las referencias detalladas siguen principalmente en japonés, inglés y chino simplificado.
- Obsidian debe estar abierto para CLI; cambios en sus API internas pueden afectar la compatibilidad.
- Las notas diarias no se crean automáticamente. Prepara la nota de hoy antes de vincularla por primera vez.
- No se reproducen los temas de Obsidian ni todas las vistas de complementos. Las imágenes muestran texto alternativo. No se admiten DataviewJS, expresiones en línea ni CALENDAR.
- Google Tasks es de solo lectura; los vencimientos se obtienen solo como fechas. No se admiten horas de vencimiento, sincronización automática con tareas Markdown ni creación de eventos de Google Calendar.
- Copias: `%LOCALAPPDATA%/StickyNotes/backups`. Ante un conflicto, conserva el borrador, recarga y reintenta.

## Documentación e informes de errores (en inglés)

[Uso](docs/USAGE.en.md) · [Almacenamiento y traslado](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [Entrada de tareas](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [Varias bóvedas](docs/MULTIPLE-VAULTS.md#english) · [Gestión de notas](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [Desarrollo](docs/DEVELOPMENT.en.md) · [Licencias](THIRD-PARTY-NOTICES.txt)

Para informar de errores, indica versiones de aplicación/Windows/complementos, modo CLI o local y pasos para reproducirlos. No adjuntes notas personales, JSON OAuth ni tokens. [Issues](https://github.com/AvocadoWasabi/StickyNotes/issues)

## Licencia

El código y la documentación de este proyecto cuyos derechos de autor pertenecen a AvocadoWasabi se publican bajo la [licencia MIT](LICENSE). El código y las bibliotecas de terceros conservan sus respectivas licencias.

Markdig utiliza BSD 2-Clause. YamlDotNet, el código de sugerencias y los materiales de prueba derivados de Obsidian Tasks, y .NET / Windows Desktop / System.Security.Cryptography.ProtectedData utilizan MIT. Los derechos de autor y las condiciones figuran en los [avisos de terceros](THIRD-PARTY-NOTICES.txt). El ZIP incluye LICENSE del proyecto, los avisos de terceros y las licencias de .NET y avisos adicionales de terceros en licenses/.
