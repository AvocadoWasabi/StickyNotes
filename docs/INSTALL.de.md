[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# Installation und Einrichtung

[README](../README.de.md) · [Usage (English)](USAGE.en.md)

## 1. Erste Schritte

Windows 10 / 11 (x64). .NET ist im ZIP enthalten; keine Administratorrechte nötig. CLI ist bei neuen Einstellungen aktiviert. Für lokale Dateien ausschalten. Kein eigenes Sticky-Notes-Plugin für Obsidian erforderlich.

1. `StickyNotes-win-x64.zip` aus den Releases laden, vollständig entpacken und `Install.cmd` starten.
2. Obsidian öffnen, CLI aktivieren und den gewünschten Vault geöffnet lassen.
3. In den App-Einstellungen den absoluten Pfad zu `Obsidian.com` und den Vault-Namen bzw. die ID prüfen. Den Vault-Ordner per CLI abrufen, Notizordner wählen und speichern.
4. Unter `…` eine Notiz erstellen/öffnen oder eine Tagesnotiz bzw. einen Abschnitt verknüpfen.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest)

Alternativ `StickyNotes.exe` direkt aus dem entpackten Ordner starten und alle Begleitdateien behalten. Die App ist unsigniert; Windows kann eine Bestätigung verlangen. Autostart wird nicht eingerichtet.

Diese Anleitung beschreibt den aktuellen Entwicklungsstand. Unterschiede zum veröffentlichten ZIP stehen unter Unreleased. [Unreleased](../CHANGELOG.md#unreleased)

## 2. Obsidian oder lokale Dateien

In Obsidian die CLI aktivieren und den Registrierungsanweisungen folgen. Fehlt die Einstellung, die Installer-Anforderungen im offiziellen CLI-Handbuch prüfen. In der App unter Einstellungen den absoluten Pfad prüfen (meist `C:\Program Files\Obsidian\Obsidian.com`). Unter Vaults und Tagesnotizen CLI für den Eintrag aktivieren, Vault-Namen/ID eingeben und den Abrufknopf drücken. Ein leeres Feld verwendet die Obsidian-Standardauswahl; Eingabe allein lädt nichts. Zurückgegebene Ordner prüfen, Notizordner wählen und speichern.

[Obsidian CLI](https://obsidian.md/help/cli)

Ohne Obsidian CLI ausschalten, lokalen Basis- und Notizordner wählen und speichern. Anfangs wird `Documents/StickyNotesData` verwendet. Tasks-/Dataview-Abfragen sind dann nicht verfügbar. Lokale Tagesnotizen lassen sich über Ordner und Datums-RegEx konfigurieren.

Beim Ordnerwechsel verschiebt Ja auch vorhandene von der App erstellte Notizen; Nein ändert nur den zukünftigen Speicherort; Abbrechen verwirft den Wechsel. CLI-Fehler wechseln nicht automatisch zu lokalem Lesen.

[Storage (English)](STICKY-FOLDERS.md#english) · [Daily notes (English)](USAGE.en.md#daily)

## 3. Optionale Integrationen

Daily notes: Ordner und Datumsformat in Obsidian setzen, die heutige Datei erstellen und im Erstellen/Öffnen-Menü eine Tagesnotiz anzeigen. Ohne gewählte Überschrift wird die ganze Notiz angezeigt. Tasks 8.2.2+ bzw. Dataview im Ziel-Vault aktivieren und `tasks`-/`dataview`-Codeblöcke öffnen. Normale Checkboxen benötigen Tasks nicht. Vorschläge stehen standardmäßig auf Automatisch und bleiben ohne Tasks aus, sofern nicht manuell aktiviert. Für Google eigene APIs und Desktop-OAuth einrichten, JSON importieren, autorisieren und `@calendar today` in einen eigenen Absatz schreiben. Ohne Google-Integration ist keine Google-Autorisierung nötig.

[Tasks (English)](TASKS-CLI-PREVIEW.md#english) · [Dataview (English)](DATAVIEW.md#english) · [Google (English)](GOOGLE-CALENDAR.en.md#setup)

## 4. Bedienung

Text anklicken (einfacher/doppelter Klick einstellbar) oder `Ctrl+E` zum Bearbeiten; `Ctrl+S` / `Esc` speichert. Das erste Esc schließt offene Vorschläge. Checkboxen im Lesemodus anklicken. `○ / ●` heftet an. Eine Notiz über `…` schließen, die App über den Infobereich beenden.

UI, README, Einrichtung und Bilder unterstützen die neun oben aufgeführten Sprachen. Sprache in den Einstellungen wählen, speichern und neu starten. Ausführliche Referenzen bleiben überwiegend Japanisch/Englisch/Vereinfachtes Chinesisch.

## Fehlerbehebung

CLI: laufendes Obsidian, aktivierte CLI, absoluten Pfad und Vault-ID prüfen, dann erneut abrufen. Fehlende Tagesnotiz: Core-Daily-notes-Einstellungen und heutige Datei prüfen; andere Tagesnotiz-Plugins werden nicht automatisch erkannt. Tasks-Warnung: Plugin im betroffenen Vault aktualisieren und die verknüpften Daten neu laden. Speicherkonflikt: Entwurf behalten und neu laden. Google-Probleme: APIs, Testnutzer und Berechtigungen anhand der englischen Anleitung prüfen.

## Aktualisieren, Daten und Entfernen

Zum Aktualisieren Änderungen speichern, die App im Infobereich beenden, das neue ZIP in einen anderen Ordner entpacken und `Install.cmd` ausführen. Notizen, Einstellungen und Sicherungen liegen getrennt vom Programm. Bei Einstellungen aus v0.0.6 oder älter ohne CLI-Auswahl ist CLI jetzt aktiviert: für lokalen Modus ausschalten und speichern; Dateien werden nicht automatisch verschoben. Eigene Tagesmuster aus v0.0.1 gegebenenfalls korrigieren.

| Daten | Speicherort |
| --- | --- |
| Programm | %LOCALAPPDATA%/Programs/MarkdownStickyNotes |
| Einstellungen und Zugangsdaten | %LOCALAPPDATA%/StickyNotes |
| Sicherungen vor Änderungen | %LOCALAPPDATA%/StickyNotes/backups |
| Notizen | Gewählter Vault/Ordner |

Zum Entfernen die App beenden, den Installationsordner und die Startmenü-Verknüpfung löschen. Notizen, Einstellungen und Sicherungen bleiben erhalten; vor separatem Löschen prüfen.
