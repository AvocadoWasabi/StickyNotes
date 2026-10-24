[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

[v0.0.11](https://github.com/AvocadoWasabi/StickyNotes/releases/tag/v0.0.11) · Windows 10 / 11 (x64)

Eine Windows-Anwendung zum Anzeigen und Bearbeiten von Markdown-Dateien in Haftnotizfenstern. Sie unterstützt lokale Dateien, Obsidian-Notizen und einzelne Abschnitte von Tagesnotizen. Bearbeitungen und Änderungen an Checkboxen werden in der ursprünglichen Markdown-Datei gespeichert.

Die Anwendung zeigt Tasks- und Dataview-Abfrageergebnisse über Obsidian CLI an und ruft Google-Calendar-Termine sowie Google Tasks ab.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [Installation und Einrichtung](docs/INSTALL.de.md) · [Bildschirmbeispiele](docs/SCREENSHOTS.de.md) · [v0.0.11](CHANGELOG.md#v0-0-11)

## Bildschirmbeispiele

| Checkbox-Bedienung | Markdown-Darstellung | Abschnitt einer Tagesnotiz |
| :---: | :---: | :---: |
| [<img src="docs/images/sticky-tasks-de.png" alt="Checkbox-Bedienung" width="280">](docs/images/sticky-tasks-de.png) | [<img src="docs/images/sticky-markdown-de.png" alt="Markdown-Darstellung" width="280">](docs/images/sticky-markdown-de.png) | [<img src="docs/images/sticky-daily-de.png" alt="Abschnitt einer Tagesnotiz" width="280">](docs/images/sticky-daily-de.png) |

Mit den WPF-Steuerelementen der Anwendung gerenderte Beispielnotizen. Dies sind keine Desktop-Aufnahmen. Siehe [Sprachbeispiele und Erzeugung](docs/SCREENSHOTS.de.md).

## Systemanforderungen und Abhängigkeiten

Windows 10 / 11 (x64). .NET ist im ZIP enthalten; keine Administratorrechte nötig. CLI ist bei neuen Einstellungen aktiviert. Für lokale Dateien ausschalten. Kein eigenes Sticky-Notes-Plugin für Obsidian erforderlich.

| Zweck | Voraussetzung | Einrichtung |
| --- | --- | --- |
| Nur Markdown | Diese App | Obsidian CLI in den Einstellungen ausschalten und lokalen Ordner wählen |
| Obsidian-Notizen | Laufendes Obsidian + integrierte CLI | In Obsidian Settings → General → Command line interface aktivieren; Vault verbinden |
| Tagesnotizen | CLI + Core-Plugin Daily notes | Ordner/Datumsformat einstellen und die heutige Notiz erstellen |
| Tasks-Abfragen | CLI + Community-Plugin Tasks | Im Ziel-Vault aktivieren; v0.0.11 benötigt stabile Version 8.2.2+ |
| Dataview-Abfragen | CLI + Dataview | Im Ziel-Vault aktivieren; LIST / TABLE / TASK |
| Google | Google-Konto + eigene OAuth-Konfiguration | Calendar API / Tasks API aktivieren, Desktop-OAuth-JSON importieren und autorisieren |

[Obsidian CLI](https://obsidian.md/help/cli) · [Google OAuth (English)](docs/GOOGLE-CALENDAR.en.md#setup)

## Installation und Einrichtung

1. `StickyNotes-win-x64.zip` aus den Releases laden, vollständig entpacken und `Install.cmd` starten.
2. Obsidian öffnen, CLI aktivieren und den gewünschten Vault geöffnet lassen.
3. In den App-Einstellungen den absoluten Pfad zu `Obsidian.com` und den Vault-Namen bzw. die ID prüfen. Den Vault-Ordner per CLI abrufen, Notizordner wählen und speichern.
4. Unter `…` eine Notiz erstellen/öffnen oder eine Tagesnotiz bzw. einen Abschnitt verknüpfen.

Einstellungen für lokale Dateien, Updates und Deinstallation beschreibt die [Installationsanleitung](docs/INSTALL.de.md). Die beschriebenen Funktionen und Einstellungen gelten für v0.0.11. Änderungen je Version stehen im [Änderungsprotokoll (Englisch)](CHANGELOG.md#v0-0-11).

## Funktionen

| Funktionen | Zweck |
| --- | --- |
| Markdown | Überschriften, Listen, Checkboxen, Tabellen, Code und Links; normale `.md`-Dateien |
| Abschnitte und Tagesnotizen | Abschnitt anzeigen und bearbeiten, dem Datumswechsel folgen und die gestrige Notiz behalten, wenn die heutige fehlt |
| Tasks / Dataview | Abfragen anzeigen, geprüfte Quellzuordnungen beim Abhaken nutzen, Abfragetext erhalten |
| Aufgabeneingabe | Schlüsselwort-/Datumsvorschläge, Listen mit Enter fortsetzen, Tab zum Einrücken, konfigurierbarer Checkbox-Kurzbefehl |
| Google | `@calendar today`: Termine und datierte offene Aufgaben; Termintitel/-beschreibung nach Bestätigung bearbeiten |
| Fenster | Farben, Größe, Skalierung 50–200 %, Anheften, temporärer Vordergrund und Taskleistenanzeige |
| Notizverwaltung | Speicherort wählen/umziehen, geschlossene Notizen öffnen, Dateien durchsuchen, Vorschau, Umbenennen und Löschen |
| Sicherungen und Konflikterkennung | Sicherungen vor Schreibvorgängen und Prüfung auf externe Änderungen |
| Mehrere Vaults | Tagesnotizen je Vault, Namen ändern und Standard-Vault wählen |

## Grundlegende Bedienung

| Aktion | Methode |
| --- | --- |
| Bearbeiten | Text anklicken (einfacher/doppelter Klick einstellbar) oder `Ctrl+E` |
| Speichern und Bearbeitung beenden | `Ctrl+S` / `Esc`; das erste Esc schließt offene Vorschläge |
| Abhaken | Checkbox im Lesemodus anklicken |
| Vordergrund | Mit `○ / ●` anheften; `Alt+Z` zeigt alle Notizen für 10 Sekunden im Vordergrund |
| Schließen / Beenden | Notiz über `…` schließen / App im Infobereich beenden |

## Unterstützung und Einschränkungen

- UI, README, Einrichtung und Bilder unterstützen die neun oben aufgeführten Sprachen. Sprache in den Einstellungen wählen, speichern und neu starten. Ausführliche Referenzen bleiben überwiegend Japanisch/Englisch/Vereinfachtes Chinesisch.
- Obsidian muss für CLI laufen; interne API-Änderungen können die Kompatibilität beeinflussen.
- Tagesnotizen werden nicht automatisch erstellt. Vor dem ersten Verknüpfen muss die heutige Notiz vorhanden sein.
- Obsidian-Themes und alle Plugin-Ansichten werden nicht nachgebildet. Bilder erscheinen als Alternativtext. DataviewJS, Inline-Ausdrücke und CALENDAR werden nicht unterstützt.
- Google Tasks ist schreibgeschützt; Fälligkeiten werden nur als Datum abgerufen. Fälligkeitszeiten, automatische Synchronisierung mit Markdown-Aufgaben und das Erstellen von Google-Calendar-Terminen werden nicht unterstützt.
- Sicherungen: `%LOCALAPPDATA%/StickyNotes/backups`. Bei Konflikten den Entwurf behalten, neu laden und erneut versuchen.

## Weitere Dokumentation und Fehlerberichte (Englisch)

[Bedienung](docs/USAGE.en.md) · [Speicherort und Umzug](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [Aufgabeneingabe](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [Mehrere Vaults](docs/MULTIPLE-VAULTS.md#english) · [Notizverwaltung](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [Entwicklung](docs/DEVELOPMENT.en.md) · [Lizenzhinweise](THIRD-PARTY-NOTICES.txt)

Bei Fehlern App-/Windows-/Plugin-Versionen, CLI- oder lokalen Modus und Schritte zum Nachstellen angeben. Keine persönlichen Notizen, OAuth-JSON-Dateien oder Tokens anhängen. [Issues](https://github.com/AvocadoWasabi/StickyNotes/issues)
