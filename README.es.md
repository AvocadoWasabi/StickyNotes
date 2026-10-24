[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

[v0.0.11](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.11) · Windows 10 / 11 (x64)

**Tus tareas de Obsidian en el escritorio. Márcalas como completadas allí mismo.**

Convierte una nota Markdown completa o una sección de la nota diaria en una nota adhesiva. Los cambios se guardan en el archivo original. También puedes consultar resultados de Tasks/Dataview y eventos de Google.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [Instalación y configuración](docs/INSTALL.es.md) · [Ejemplos de pantalla](docs/SCREENSHOTS.es.md) · [v0.0.11](CHANGELOG.md#v0-0-11)

## ¿Por qué usarlo?

- El día a la vista: la sección elegida sigue automáticamente la fecha.
- Un solo archivo: editar y marcar actualiza el `.md` original. Cerrar la ventana conserva el archivo.
- Acceso rápido: fija las notas o pulsa `Alt+Z` para traerlas todas al frente durante diez segundos.

## Ejemplos de pantalla

| Marcar tareas | Escribir Markdown | Seguir la nota diaria |
| --- | --- | --- |
| [<img src="docs/images/sticky-tasks-es.png" alt="Marcar tareas" width="280">](docs/images/sticky-tasks-es.png) | [<img src="docs/images/sticky-markdown-es.png" alt="Escribir Markdown" width="280">](docs/images/sticky-markdown-es.png) | [<img src="docs/images/sticky-daily-es.png" alt="Seguir la nota diaria" width="280">](docs/images/sticky-daily-es.png) |

Notas ficticias renderizadas con los controles WPF de v0.0.11; no son capturas del escritorio. [Ejemplos de pantalla](docs/SCREENSHOTS.es.md).

## Herramientas y ajustes necesarios

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

## Primeros pasos

1. Descarga `StickyNotes-win-x64.zip` de Releases, extrae todo y ejecuta `Install.cmd`.
2. Abre Obsidian, activa CLI y mantén abierta la bóveda que quieras usar.
3. En los ajustes de la aplicación comprueba la ruta absoluta de `Obsidian.com` y el nombre/ID de la bóveda. Obtén la carpeta mediante CLI, elige una carpeta de notas y guarda.
4. En `…`, crea/abre una nota o vincula una sección o nota diaria.

Esta guía corresponde a v0.0.11. Consulta el [registro de cambios (en inglés)](CHANGELOG.md#v0-0-11) para varias bóvedas, Tasks 8.2.2 y los nueve idiomas de interfaz.

## Funciones

| Funciones | Uso |
| --- | --- |
| Markdown | Títulos, listas, casillas, tablas, código y enlaces; archivos `.md` normales |
| Secciones y notas diarias | Edita parte de una nota, sigue el cambio de fecha y conserva opcionalmente la nota de ayer |
| Tasks / Dataview | Resultados de consultas y casillas con correspondencia verificada al origen; conserva las consultas |
| Entrada de tareas | Sugerencias de palabras/fechas, Enter continúa listas, Tab sangra y atajo configurable para casillas |
| Google | `@calendar today`: eventos y tareas pendientes con fecha; edita título/descripción del evento tras confirmar |
| Ventanas | Colores, tamaño, escala 50–200 %, fijación, primer plano temporal y barra de tareas |
| Gestión | Selecciona/migra carpeta, reabre notas, explora archivos, previsualiza, renombra y elimina |
| Protección de datos | Copias antes de escribir y comprobación de cambios externos |
| Varias bóvedas (v0.0.11) | Notas diarias por bóveda, nombres editables y bóveda predeterminada |

## Uso básico

Haz clic en el texto (uno/dos clics configurable) o pulsa `Ctrl+E` para editar; `Ctrl+S` / `Esc` guarda. El primer Esc cierra las sugerencias abiertas. Marca casillas en modo lectura. `○ / ●` fija la nota. Cierra una nota desde `…` y la aplicación desde el área de notificación.

## Antes de empezar

- La interfaz, README, instalación e imágenes admiten los nueve idiomas de arriba. Elige el idioma en ajustes, guarda y reinicia. Las referencias detalladas siguen principalmente en japonés, inglés y chino simplificado.
- Obsidian debe estar abierto para CLI; cambios en sus API internas pueden afectar la compatibilidad. Las notas diarias no se crean automáticamente.
- No se reproducen los temas de Obsidian ni todas las vistas de complementos. Las imágenes muestran texto alternativo. No se admiten DataviewJS, expresiones en línea ni CALENDAR.
- Google Tasks es de solo lectura y solo proporciona fechas. No se admiten horas, sincronización automática de tareas Markdown ni creación de eventos.
- Copias: `%LOCALAPPDATA%/StickyNotes/backups`. Ante un conflicto, conserva el borrador, recarga y reintenta.

## Más información (en inglés)

[Usage](docs/USAGE.en.md) · [Storage](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [Task entry](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [Multiple vaults](docs/MULTIPLE-VAULTS.md#english) · [Note management](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [Development](docs/DEVELOPMENT.en.md) · [License notices](THIRD-PARTY-NOTICES.txt)

Para informar de errores, indica versiones de aplicación/Windows/complementos, modo CLI o local y pasos para reproducirlos. No adjuntes notas personales, JSON OAuth ni tokens. [Issues](https://github.com/AvocadoWasabi/StickyNotes/issues)
