[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# Instalación y configuración

[README](../README.es.md) · [Usage (English)](USAGE.en.md)

## 1. Instalación

Para Windows 10 / 11 (x64). No se requieren permisos de administrador ni una instalación adicional de .NET.

1. Descarga **StickyNotes-win-x64.zip** de [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest). `Source code` contiene el código fuente.
2. Extrae todo el ZIP y ejecuta `Install.cmd`.
3. Inicia **Markdown Sticky Notes** desde el menú Inicio.

También puedes ejecutar `StickyNotes.exe` desde la carpeta extraída manteniendo todos sus archivos. La aplicación no está firmada; Windows puede pedir confirmación. No se configura inicio automático.

Esta guía corresponde a v0.0.11. Consulta el [registro de cambios (en inglés)](../CHANGELOG.md#v0-0-11) para varias bóvedas, Tasks 8.2.2 y los nueve idiomas de interfaz.

## 2. Ubicación y método de conexión

### Uso de Obsidian CLI (predeterminado)

1. Activa **Settings → General → Command line interface** en Obsidian y sigue las indicaciones de registro. Si no aparece, comprueba los requisitos de actualización del instalador en la [guía oficial de CLI](https://obsidian.md/help/cli).
2. Abre la bóveda de destino y mantén Obsidian en ejecución.
3. En los ajustes generales de la aplicación, comprueba la ruta absoluta de `Obsidian.com`, normalmente `C:\Program Files\Obsidian\Obsidian.com`. Ajústala si la instalación está en otra ubicación.
4. En la sección de bóvedas y notas diarias, activa CLI para la entrada elegida, introduce el nombre/ID de la bóveda y pulsa el botón para obtener la carpeta mediante CLI. El campo vacío utiliza la selección predeterminada de Obsidian; escribir el nombre no obtiene la carpeta por sí solo.
5. Comprueba la bóveda y los ajustes diarios obtenidos, selecciona la carpeta de notas y guarda los ajustes.

Al cambiar de carpeta, «Sí» también traslada las notas existentes creadas por la aplicación; «No» cambia solo el destino de futuras notas; «Cancelar» anula el cambio. [Alcance y recuperación (en inglés)](STICKY-FOLDERS.md#english). Un fallo de CLI no cambia automáticamente a lectura local.

### Uso de archivos locales

Desactiva Obsidian CLI en los ajustes, selecciona la carpeta base local y la carpeta de notas, y guarda. La ubicación inicial es `Documents/StickyNotesData`. Las consultas Tasks/Dataview no están disponibles. Las notas diarias locales se configuran con una carpeta y una expresión regular que capture la fecha. [Ejemplos (en inglés)](USAGE.en.md#daily).

## 3. Integraciones opcionales

| Integración | En Obsidian / Google | En Sticky Notes |
| --- | --- | --- |
| Notas diarias | Activar el complemento principal Daily notes, configurar carpeta y formato de fecha, crear la nota de hoy | Elegir una nota diaria y un encabezado en el menú de creación/apertura; vacío muestra toda la nota |
| Tasks | Activar en la bóveda de destino; v0.0.11 requiere la versión estable 8.2.2 o posterior | Abrir una nota con un bloque `tasks`. [Configuración y límites (en inglés)](TASKS-CLI-PREVIEW.md#english) |
| Dataview | Activar en la bóveda de destino | Mostrar LIST / TABLE / TASK en bloques `dataview`. [Configuración y límites (en inglés)](DATAVIEW.md#english) |
| Google | Configurar Calendar API, Tasks API y OAuth de escritorio en un proyecto Cloud propio | Importar JSON, autorizar y escribir `@calendar today` en un párrafo separado. [Conexión (en inglés)](GOOGLE-CALENDAR.en.md#setup) |

Las casillas normales de Markdown no requieren Tasks. Las sugerencias de tareas están en modo Automático por defecto y permanecen desactivadas sin Tasks salvo que se activen manualmente. No se necesita autorización de Google si no se utiliza esa integración.

## 4. Creación y edición de notas

Crea o abre Markdown desde el menú de creación/apertura en `…`. Haz clic en el texto o pulsa `Ctrl+E` para editar; `Ctrl+S` / `Esc` guarda. La primera vinculación diaria necesita el archivo de hoy; no se crea automáticamente. Elige el idioma en los ajustes, guarda y reinicia. La interfaz de v0.0.11 admite los nueve idiomas del README.

## Solución de problemas

| Síntoma | Comprobación |
| --- | --- |
| CLI no conecta | Obsidian en ejecución, CLI activada, ruta absoluta de `Obsidian.com` y nombre/ID de bóveda; volver a obtener la carpeta |
| Falta la nota de hoy | Ajustes del complemento principal Daily notes y archivo de hoy; no se detectan automáticamente los ajustes de otros complementos de notas diarias |
| Tasks pide actualizar | Actualizar Tasks a 8.2.2 o posterior en la bóveda de destino y recargar Tasks / Dataview en la nota |
| Conflicto al guardar | Conservar el borrador y recargar; se evita sobrescribir con contenido desactualizado |
| Falla la autorización de Google | Comprobar API, OAuth, usuarios de prueba y permisos en la [guía (en inglés)](GOOGLE-CALENDAR.en.md) |

## Actualización, datos y desinstalación

Para actualizar, guarda las ediciones, sal desde el área de notificación, extrae el nuevo ZIP en otra carpeta y ejecuta `Install.cmd`. Notas, ajustes y copias se guardan fuera del programa. En ajustes de v0.0.6 o anterior sin elección de CLI, ahora se activa CLI: desactívala y guarda para modo local; los archivos no se migran solos. Los patrones diarios personalizados de v0.0.1 pueden requerir corrección.

| Datos | Ubicación |
| --- | --- |
| Programa | %LOCALAPPDATA%/Programs/MarkdownStickyNotes |
| Ajustes y credenciales | %LOCALAPPDATA%/StickyNotes |
| Copias antes de escribir | %LOCALAPPDATA%/StickyNotes/backups |
| Notas | Bóveda/carpeta elegida |

Para desinstalar, sal de la aplicación, elimina su carpeta de instalación y el acceso directo del menú Inicio. Notas, ajustes y copias permanecen; revísalos antes de eliminarlos por separado.
