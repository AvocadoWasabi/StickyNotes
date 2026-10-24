[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# Installation und Einrichtung

[README](../README.de.md) · [Usage (English)](USAGE.en.md)

## 1. Installation

Für Windows 10 / 11 (x64). Administratorrechte und eine zusätzliche .NET-Installation sind nicht erforderlich.

1. Laden Sie **StickyNotes-win-x64.zip** aus den [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest). `Source code` enthält den Quellcode.
2. Entpacken Sie das gesamte ZIP und führen Sie `Install.cmd` aus.
3. Starten Sie **Markdown Sticky Notes** über das Startmenü.

Alternativ `StickyNotes.exe` direkt aus dem entpackten Ordner starten und alle Begleitdateien behalten. Die App ist unsigniert; Windows kann eine Bestätigung verlangen. Autostart wird nicht eingerichtet.

Diese Anleitung gilt für v0.0.11. Mehrere Vaults, Tasks 8.2.2 und neun UI-Sprachen sind im [Änderungsprotokoll (Englisch)](../CHANGELOG.md#v0-0-11) beschrieben.

## 2. Speicherort und Verbindung

### Obsidian CLI verwenden (Standard)

1. Aktivieren Sie in Obsidian **Settings → General → Command line interface** und folgen Sie den Registrierungshinweisen. Fehlt der Eintrag, prüfen Sie die Installer-Anforderungen in der [offiziellen CLI-Anleitung](https://obsidian.md/help/cli).
2. Öffnen Sie den gewünschten Vault und lassen Sie Obsidian laufen.
3. Prüfen Sie in den allgemeinen App-Einstellungen den absoluten Pfad zu `Obsidian.com`, normalerweise `C:\Program Files\Obsidian\Obsidian.com`. Passen Sie ihn bei einem anderen Installationsort an.
4. Aktivieren Sie unter Vaults und Tagesnotizen CLI für den gewünschten Eintrag. Geben Sie Vault-Name/ID ein und klicken Sie auf den Knopf zum Abrufen des Vault-Ordners per CLI. Ein leeres Feld verwendet die Obsidian-Standardauswahl; die Eingabe allein ruft den Ordner nicht ab.
5. Prüfen Sie den abgerufenen Vault und die Tageseinstellungen, wählen Sie den Speicherordner für Haftnotizen und speichern Sie die Einstellungen.

Beim Ordnerwechsel verschiebt „Ja“ auch vorhandene, von der App erstellte Notizen; „Nein“ ändert nur den zukünftigen Speicherort; „Abbrechen“ verwirft den Wechsel. [Umfang und Wiederherstellung (Englisch)](STICKY-FOLDERS.md#english). Bei CLI-Fehlern wird nicht automatisch auf lokales Lesen umgeschaltet.

### Lokale Dateien verwenden

Deaktivieren Sie Obsidian CLI in den Einstellungen, wählen Sie den lokalen Basis- und Notizordner und speichern Sie. Der anfängliche Speicherort ist `Documents/StickyNotesData`. Tasks-/Dataview-Abfragen sind nicht verfügbar. Lokale Tagesnotizen lassen sich über einen Ordner und einen regulären Ausdruck mit Datumserfassung konfigurieren. [Beispiele (Englisch)](USAGE.en.md#daily).

## 3. Optionale Integrationen

| Integration | In Obsidian / Google | In Sticky Notes |
| --- | --- | --- |
| Tagesnotizen | Core-Plugin Daily notes aktivieren, Ordner und Datumsformat setzen, heutige Notiz erstellen | Im Menü zum Erstellen/Öffnen eine Tagesnotiz anzeigen und eine Überschrift auswählen; leer zeigt die ganze Notiz |
| Tasks | Im Ziel-Vault aktivieren; v0.0.11 erfordert stabile Version 8.2.2 oder neuer | Notiz mit einem `tasks`-Codeblock öffnen. [Anleitung und Grenzen (Englisch)](TASKS-CLI-PREVIEW.md#english) |
| Dataview | Im Ziel-Vault aktivieren | LIST / TABLE / TASK in `dataview`-Blöcken anzeigen. [Anleitung und Grenzen (Englisch)](DATAVIEW.md#english) |
| Google | Calendar API, Tasks API und Desktop-OAuth im eigenen Cloud-Projekt einrichten | JSON importieren, autorisieren und `@calendar today` in einen eigenen Absatz schreiben. [Verbindung (Englisch)](GOOGLE-CALENDAR.en.md#setup) |

Normale Markdown-Checkboxen benötigen Tasks nicht. Aufgabenvorschläge stehen standardmäßig auf Automatisch und bleiben ohne Tasks deaktiviert, sofern sie nicht manuell aktiviert werden. Ohne Google-Integration ist keine Google-Autorisierung erforderlich.

## 4. Notizen erstellen und bearbeiten

Erstellen oder öffnen Sie Markdown über das Menü zum Erstellen/Öffnen unter `…`. Klicken Sie in den Text oder drücken Sie `Ctrl+E` zum Bearbeiten; `Ctrl+S` / `Esc` speichert. Für die erste Tagesverknüpfung muss die heutige Datei vorhanden sein; sie wird nicht automatisch erstellt. Wählen Sie die Anzeigesprache in den Einstellungen, speichern Sie und starten Sie neu. Die UI von v0.0.11 unterstützt die neun im README genannten Sprachen.

## Fehlerbehebung

| Problem | Prüfung |
| --- | --- |
| Keine CLI-Verbindung | Obsidian muss laufen; CLI-Aktivierung, absoluten `Obsidian.com`-Pfad und Vault-Name/ID prüfen, dann erneut abrufen |
| Heutige Notiz fehlt | Core-Daily-notes-Einstellungen und heutige Datei prüfen; Einstellungen anderer Tagesnotiz-Plugins werden nicht automatisch übernommen |
| Tasks fordert ein Update | Tasks im Ziel-Vault auf 8.2.2 oder neuer aktualisieren und Tasks / Dataview in der Haftnotiz neu laden |
| Speicherkonflikt | Entwurf behalten und neu laden; das Überschreiben mit veraltetem Inhalt wird verhindert |
| Google-Autorisierung fehlgeschlagen | APIs, OAuth, Testnutzer und Berechtigungen anhand der [Anleitung (Englisch)](GOOGLE-CALENDAR.en.md) prüfen |

## Aktualisieren, Daten und Entfernen

Zum Aktualisieren Änderungen speichern, die App im Infobereich beenden, das neue ZIP in einen anderen Ordner entpacken und `Install.cmd` ausführen. Notizen, Einstellungen und Sicherungen liegen getrennt vom Programm. Bei Einstellungen aus v0.0.6 oder älter ohne CLI-Auswahl ist CLI jetzt aktiviert: für lokalen Modus ausschalten und speichern; Dateien werden nicht automatisch verschoben. Eigene Tagesmuster aus v0.0.1 gegebenenfalls korrigieren.

| Daten | Speicherort |
| --- | --- |
| Programm | %LOCALAPPDATA%/Programs/MarkdownStickyNotes |
| Einstellungen und Zugangsdaten | %LOCALAPPDATA%/StickyNotes |
| Sicherungen vor Änderungen | %LOCALAPPDATA%/StickyNotes/backups |
| Notizen | Gewählter Vault/Ordner |

Zum Entfernen die App beenden, den Installationsordner und die Startmenü-Verknüpfung löschen. Notizen, Einstellungen und Sicherungen bleiben erhalten; vor separatem Löschen prüfen.
