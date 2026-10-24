[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# Instalación y configuración

[README](../README.es.md) · [Usage (English)](USAGE.en.md)

## 1. Primeros pasos

Windows 10 / 11 (x64). El ZIP incluye .NET; no necesitas permisos de administrador. CLI está activada en las configuraciones nuevas: desactívala para archivos locales. No hace falta un complemento específico de Sticky Notes para Obsidian.

1. Descarga `StickyNotes-win-x64.zip` de Releases, extrae todo y ejecuta `Install.cmd`.
2. Abre Obsidian, activa CLI y mantén abierta la bóveda que quieras usar.
3. En los ajustes de la aplicación comprueba la ruta absoluta de `Obsidian.com` y el nombre/ID de la bóveda. Obtén la carpeta mediante CLI, elige una carpeta de notas y guarda.
4. En `…`, crea/abre una nota o vincula una sección o nota diaria.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest)

También puedes ejecutar `StickyNotes.exe` desde la carpeta extraída manteniendo todos sus archivos. La aplicación no está firmada; Windows puede pedir confirmación. No se configura inicio automático.

Esta guía corresponde a v0.0.11. Consulta el [registro de cambios (en inglés)](../CHANGELOG.md#v0-0-11) para varias bóvedas, Tasks 8.2.2 y los nueve idiomas de interfaz.

## 2. Obsidian o archivos locales

Activa CLI en Obsidian y sigue las instrucciones de registro. Si no aparece, revisa los requisitos del instalador en la guía oficial de CLI. En los ajustes de la aplicación comprueba la ruta absoluta (normalmente `C:\Program Files\Obsidian\Obsidian.com`). En bóvedas y notas diarias activa CLI para la entrada, escribe el nombre/ID y pulsa el botón para obtener la carpeta. Vacío usa la selección predeterminada de Obsidian; escribir no inicia la consulta. Revisa las carpetas devueltas, selecciona la carpeta de notas y guarda.

[Obsidian CLI](https://obsidian.md/help/cli)

Sin Obsidian, desactiva CLI, selecciona carpeta base y carpeta de notas locales, y guarda. La ubicación inicial es `Documents/StickyNotesData`. No habrá consultas Tasks/Dataview. Las notas diarias locales pueden usar una carpeta y una expresión regular con grupos de fecha.

Al cambiar carpeta, Sí migra también las notas existentes creadas por la aplicación; No cambia solo el destino futuro; Cancelar anula el cambio. Un error de CLI no activa automáticamente la lectura local.

[Storage (English)](STICKY-FOLDERS.md#english) · [Daily notes (English)](USAGE.en.md#daily)

## 3. Integraciones opcionales

Daily notes: configura carpeta/formato en Obsidian, crea el archivo de hoy y muestra una nota diaria desde el menú de crear/abrir. Sin título seleccionado se muestra toda la nota. Activa Tasks 8.2.2+ o Dataview en la bóveda y abre bloques `tasks`/`dataview`. Las casillas normales no requieren Tasks. Las sugerencias están en Automático y permanecen desactivadas sin Tasks, salvo activación manual. Para Google configura tus API y OAuth de escritorio, importa JSON, autoriza y escribe `@calendar today` en un párrafo aparte. No se necesita autorización de Google si no usas esa integración.

[Tasks (English)](TASKS-CLI-PREVIEW.md#english) · [Dataview (English)](DATAVIEW.md#english) · [Google (English)](GOOGLE-CALENDAR.en.md#setup)

## 4. Uso básico

Haz clic en el texto (uno/dos clics configurable) o pulsa `Ctrl+E` para editar; `Ctrl+S` / `Esc` guarda. El primer Esc cierra las sugerencias abiertas. Marca casillas en modo lectura. `○ / ●` fija la nota. Cierra una nota desde `…` y la aplicación desde el área de notificación.

La interfaz, README, instalación e imágenes admiten los nueve idiomas de arriba. Elige el idioma en ajustes, guarda y reinicia. Las referencias detalladas siguen principalmente en japonés, inglés y chino simplificado.

## Solución de problemas

CLI: comprueba Obsidian abierto, CLI activada, ruta absoluta e ID de bóveda, y vuelve a obtener la carpeta. Falta la nota de hoy: revisa Daily notes principal y el archivo de hoy; no se deducen ajustes de otros complementos diarios. Aviso de Tasks: actualiza el complemento en la bóveda afectada y refresca los datos vinculados. Conflicto al guardar: conserva el borrador y recarga. Google: revisa API, usuarios de prueba y permisos con la guía en inglés.

## Actualización, datos y desinstalación

Para actualizar, guarda las ediciones, sal desde el área de notificación, extrae el nuevo ZIP en otra carpeta y ejecuta `Install.cmd`. Notas, ajustes y copias se guardan fuera del programa. En ajustes de v0.0.6 o anterior sin elección de CLI, ahora se activa CLI: desactívala y guarda para modo local; los archivos no se migran solos. Los patrones diarios personalizados de v0.0.1 pueden requerir corrección.

| Datos | Ubicación |
| --- | --- |
| Programa | %LOCALAPPDATA%/Programs/MarkdownStickyNotes |
| Ajustes y credenciales | %LOCALAPPDATA%/StickyNotes |
| Copias antes de escribir | %LOCALAPPDATA%/StickyNotes/backups |
| Notas | Bóveda/carpeta elegida |

Para desinstalar, sal de la aplicación, elimina su carpeta de instalación y el acceso directo del menú Inicio. Notas, ajustes y copias permanecen; revísalos antes de eliminarlos por separado.
