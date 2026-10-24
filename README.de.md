[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

**Obsidian-Aufgaben auf dem Desktop sehen und direkt abhaken.**

Zeige eine ganze Markdown-Notiz oder nur einen Abschnitt der Tagesnotiz als Haftnotiz an. Änderungen werden in der Originaldatei gespeichert. Auch Tasks-/Dataview-Abfragen und Google-Termine bleiben so im Blick.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [Installation und Einrichtung](docs/INSTALL.de.md) · [Bildschirmbeispiele](docs/SCREENSHOTS.de.md) · [Unreleased](CHANGELOG.md#unreleased)

## Warum Sticky Notes?

- Heute im Blick: Der gewählte Abschnitt folgt automatisch dem Datum.
- Eine Datei: Bearbeiten und Abhaken aktualisiert die ursprüngliche `.md`-Datei. Schließen behält die Datei.
- Schnell erreichbar: Notizen anheften oder mit `Alt+Z` für zehn Sekunden nach vorn holen.

## Bildschirmbeispiele

| Aufgaben abhaken | Markdown schreiben | Der Tagesnotiz folgen |
| --- | --- | --- |
| [<img src="docs/images/sticky-tasks-de.png" alt="Aufgaben abhaken" width="280">](docs/images/sticky-tasks-de.png) | [<img src="docs/images/sticky-markdown-de.png" alt="Markdown schreiben" width="280">](docs/images/sticky-markdown-de.png) | [<img src="docs/images/sticky-daily-de.png" alt="Der Tagesnotiz folgen" width="280">](docs/images/sticky-daily-de.png) |

Fiktive Notizen, mit den WPF-Steuerelementen des aktuellen Entwicklungsstands gerendert; keine Desktop-Aufnahmen. [Bildschirmbeispiele](docs/SCREENSHOTS.de.md).

## Benötigte Werkzeuge und Einstellungen

Windows 10 / 11 (x64). .NET ist im ZIP enthalten; keine Administratorrechte nötig. CLI ist bei neuen Einstellungen aktiviert. Für lokale Dateien ausschalten. Kein eigenes Sticky-Notes-Plugin für Obsidian erforderlich.

| Zweck | Voraussetzung | Einrichtung |
| --- | --- | --- |
| Nur Markdown | Diese App | Obsidian CLI in den Einstellungen ausschalten und lokalen Ordner wählen |
| Obsidian-Notizen | Laufendes Obsidian + integrierte CLI | In Obsidian Settings → General → Command line interface aktivieren; Vault verbinden |
| Tagesnotizen | CLI + Core-Plugin Daily notes | Ordner/Datumsformat einstellen und die heutige Notiz erstellen |
| Tasks-Abfragen | CLI + Community-Plugin Tasks | Im Ziel-Vault aktivieren; aktueller Entwicklungsstand benötigt stabile Version 8.2.2+ |
| Dataview-Abfragen | CLI + Dataview | Im Ziel-Vault aktivieren; LIST / TABLE / TASK |
| Google | Google-Konto + eigene OAuth-Konfiguration | Calendar API / Tasks API aktivieren, Desktop-OAuth-JSON importieren und autorisieren |

[Obsidian CLI](https://obsidian.md/help/cli) · [Google OAuth (English)](docs/GOOGLE-CALENDAR.en.md#setup)

## Erste Schritte

1. `StickyNotes-win-x64.zip` aus den Releases laden, vollständig entpacken und `Install.cmd` starten.
2. Obsidian öffnen, CLI aktivieren und den gewünschten Vault geöffnet lassen.
3. In den App-Einstellungen den absoluten Pfad zu `Obsidian.com` und den Vault-Namen bzw. die ID prüfen. Den Vault-Ordner per CLI abrufen, Notizordner wählen und speichern.
4. Unter `…` eine Notiz erstellen/öffnen oder eine Tagesnotiz bzw. einen Abschnitt verknüpfen.

Diese Anleitung beschreibt den aktuellen Entwicklungsstand. Unterschiede zum veröffentlichten ZIP stehen unter Unreleased.

## Funktionen

| Funktionen | Zweck |
| --- | --- |
| Markdown | Überschriften, Listen, Checkboxen, Tabellen, Code und Links; normale `.md`-Dateien |
| Abschnitte und Tagesnotizen | Teil einer Notiz bearbeiten, Datumswechsel verfolgen, gestrige Notiz optional behalten |
| Tasks / Dataview | Abfragen anzeigen, geprüfte Quellzuordnungen beim Abhaken nutzen, Abfragetext erhalten |
| Aufgabeneingabe | Schlüsselwort-/Datumsvorschläge, Listen mit Enter fortsetzen, Tab zum Einrücken, konfigurierbarer Checkbox-Kurzbefehl |
| Google | `@calendar today`: Termine und datierte offene Aufgaben; Termintitel/-beschreibung nach Bestätigung bearbeiten |
| Fenster | Farben, Größe, Skalierung 50–200 %, Anheften, temporärer Vordergrund und Taskleistenanzeige |
| Notizverwaltung | Speicherort wählen/umziehen, geschlossene Notizen öffnen, Dateien durchsuchen, Vorschau, Umbenennen und Löschen |
| Datenschutz | Sicherungen vor Schreibvorgängen und Prüfung auf externe Änderungen |
| Mehrere Vaults (unveröffentlicht) | Tagesnotizen je Vault, Namen ändern und Standard-Vault wählen |

## Bedienung

Text anklicken (einfacher/doppelter Klick einstellbar) oder `Ctrl+E` zum Bearbeiten; `Ctrl+S` / `Esc` speichert. Das erste Esc schließt offene Vorschläge. Checkboxen im Lesemodus anklicken. `○ / ●` heftet an. Eine Notiz über `…` schließen, die App über den Infobereich beenden.

## Vor dem Start

- UI, README, Einrichtung und Bilder unterstützen die neun oben aufgeführten Sprachen. Sprache in den Einstellungen wählen, speichern und neu starten. Ausführliche Referenzen bleiben überwiegend Japanisch/Englisch/Vereinfachtes Chinesisch.
- Obsidian muss für CLI laufen; interne API-Änderungen können die Kompatibilität beeinflussen. Tagesnotizen werden nicht automatisch erstellt.
- Obsidian-Themes und alle Plugin-Ansichten werden nicht nachgebildet. Bilder erscheinen als Alternativtext. DataviewJS, Inline-Ausdrücke und CALENDAR werden nicht unterstützt.
- Google Tasks ist schreibgeschützt und liefert nur Datumsangaben. Uhrzeiten, automatische Markdown-Aufgabensynchronisation und neue Termine werden nicht unterstützt.
- Sicherungen: `%LOCALAPPDATA%/StickyNotes/backups`. Bei Konflikten den Entwurf behalten, neu laden und erneut versuchen.

## Weitere Hilfe (Englisch)

[Usage](docs/USAGE.en.md) · [Storage](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [Task entry](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [Multiple vaults](docs/MULTIPLE-VAULTS.md#english) · [Note management](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [Development](docs/DEVELOPMENT.en.md) · [License notices](THIRD-PARTY-NOTICES.txt)

Bei Fehlern App-/Windows-/Plugin-Versionen, CLI- oder lokalen Modus und Schritte zum Nachstellen angeben. Keine persönlichen Notizen, OAuth-JSON-Dateien oder Tokens anhängen. [Issues](https://github.com/AvocadoWasabi/StickyNotes/issues)
