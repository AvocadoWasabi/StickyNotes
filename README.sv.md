[日本語](README.md) | [English](README.en.md) | [简体中文](README.zh-CN.md) | [繁體中文（台灣）](README.zh-TW.md) | [한국어](README.ko.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Español](README.es.md) | [Svenska](README.sv.md)

# Markdown Sticky Notes

**Se dina Obsidian-uppgifter på skrivbordet och bocka av dem direkt.**

Visa en hel Markdown-anteckning eller ett avsnitt ur dagens anteckning som en fästis. Ändringar sparas i originalfilen. Du kan också ha Tasks-/Dataview-resultat och Google-händelser nära till hands.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest) · [Installation och inställningar](docs/INSTALL.sv.md) · [Skärmbilder](docs/SCREENSHOTS.sv.md) · [Unreleased](CHANGELOG.md#unreleased)

## Varför Sticky Notes?

- Dagen i blickfånget: det valda avsnittet följer automatiskt datumet.
- En enda fil: redigering och bockar uppdaterar originalets `.md`. Att stänga fönstret behåller filen.
- Snabb åtkomst: fäst anteckningar eller tryck `Alt+Z` för att visa alla framför andra fönster i tio sekunder.

## Skärmbilder

| Bocka av uppgifter | Skriv Markdown | Följ dagens anteckning |
| --- | --- | --- |
| [<img src="docs/images/sticky-tasks-sv.png" alt="Bocka av uppgifter" width="280">](docs/images/sticky-tasks-sv.png) | [<img src="docs/images/sticky-markdown-sv.png" alt="Skriv Markdown" width="280">](docs/images/sticky-markdown-sv.png) | [<img src="docs/images/sticky-daily-sv.png" alt="Följ dagens anteckning" width="280">](docs/images/sticky-daily-sv.png) |

Påhittade anteckningar renderade med WPF-kontroller från den aktuella utvecklingsversionen; inga skrivbordsbilder. [Skärmbilder](docs/SCREENSHOTS.sv.md).

## Verktyg och inställningar

Windows 10 / 11 (x64). .NET ingår i ZIP-filen; administratörsrättigheter behövs inte. CLI är på i nya inställningar: stäng av för lokala filer. Inget särskilt Sticky Notes-tillägg för Obsidian behövs.

| Användning | Krav | Inställning |
| --- | --- | --- |
| Bara Markdown | Den här appen | Stäng av Obsidian CLI och välj en lokal mapp |
| Obsidian-anteckningar | Obsidian igång + inbyggd CLI | Aktivera Settings → General → Command line interface och anslut valvet |
| Dagliga anteckningar | CLI + kärntillägget Daily notes | Ställ in mapp/datumformat och skapa dagens anteckning |
| Tasks-frågor | CLI + tillägget Tasks | Aktivera i rätt valv; utvecklingsversionen kräver stabil version 8.2.2+ |
| Dataview-frågor | CLI + Dataview | Aktivera i rätt valv; LIST / TABLE / TASK |
| Google | Google-konto + egen OAuth-konfiguration | Aktivera Calendar API / Tasks API, importera OAuth-JSON för skrivbordsapp och godkänn åtkomst |

[Obsidian CLI](https://obsidian.md/help/cli) · [Google OAuth (English)](docs/GOOGLE-CALENDAR.en.md#setup)

## Kom igång

1. Hämta `StickyNotes-win-x64.zip` från Releases, packa upp allt och kör `Install.cmd`.
2. Öppna Obsidian, aktivera CLI och håll det valda valvet öppet.
3. Kontrollera den absoluta sökvägen till `Obsidian.com` och valvets namn/ID i appens inställningar. Hämta valvmappen via CLI, välj anteckningsmapp och spara.
4. Skapa/öppna en anteckning eller länka ett avsnitt eller en daglig anteckning under `…`.

Dokumentationen beskriver den aktuella utvecklingsversionen. Se Unreleased för skillnader mot publicerad ZIP.

## Funktioner

| Funktioner | Användning |
| --- | --- |
| Markdown | Rubriker, listor, kryssrutor, tabeller, kod och länkar; vanliga `.md`-filer |
| Avsnitt och dagliga anteckningar | Redigera delar av anteckningar, följ datumbyten och behåll gårdagens anteckning om du vill |
| Tasks / Dataview | Frågeresultat och bockar med verifierad koppling till källan; frågetexten bevaras |
| Uppgiftsinmatning | Förslag på ord/datum, Enter fortsätter listor, Tab ger indrag och kortkommando för kryssruta kan väljas |
| Google | `@calendar today`: händelser och ofärdiga uppgifter med datum; redigera händelsens titel/beskrivning efter bekräftelse |
| Fönster | Färger, storlek, skala 50–200 %, fästning, tillfällig förgrund och visning i aktivitetsfältet |
| Hantering | Välj/flytta lagring, öppna stängda anteckningar, bläddra, förhandsgranska, byt namn och ta bort |
| Dataskydd | Säkerhetskopior före skrivning och kontroll av externa ändringar |
| Flera valv (ej släppt) | Dagliga anteckningar per valv, egna namn och standardvalv |

## Grundläggande användning

Klicka i texten (enkel-/dubbelklick kan väljas) eller tryck `Ctrl+E` för att redigera; `Ctrl+S` / `Esc` sparar. Första Esc stänger öppna förslag. Bocka i läsläget. `○ / ●` fäster anteckningen. Stäng en anteckning via `…` och avsluta appen från meddelandefältet.

## Innan du börjar

- Gränssnitt, README, installation och bilder finns på de nio språken ovan. Välj språk i inställningarna, spara och starta om. Utförliga referenser är främst på japanska, engelska och förenklad kinesiska.
- Obsidian måste köras för CLI; interna API-ändringar kan påverka kompatibiliteten. Dagliga anteckningar skapas inte automatiskt.
- Obsidian-teman och alla tilläggsvyer återskapas inte. Bilder visas som alternativtext. DataviewJS, inline-uttryck och CALENDAR stöds inte.
- Google Tasks är skrivskyddat och visar bara datum. Klockslag, automatisk synkning av Markdown-uppgifter och att skapa händelser stöds inte.
- Säkerhetskopior: `%LOCALAPPDATA%/StickyNotes/backups`. Vid konflikt, behåll utkastet, läs in på nytt och försök igen.

## Mer hjälp (på engelska)

[Usage](docs/USAGE.en.md) · [Storage](docs/STICKY-FOLDERS.md#english) · [Tasks / CLI](docs/TASKS-CLI-PREVIEW.md#english) · [Task entry](docs/TASK-EDITING.md#english) · [Dataview](docs/DATAVIEW.md#english) · [Multiple vaults](docs/MULTIPLE-VAULTS.md#english) · [Note management](docs/QUERY-EDITING-AND-BROWSER.md#english) · [Google](docs/GOOGLE-CALENDAR.en.md) · [Development](docs/DEVELOPMENT.en.md) · [License notices](THIRD-PARTY-NOTICES.txt)

Vid felrapportering: ange app-/Windows-/tilläggsversioner, CLI eller lokalt läge och steg för att återskapa felet. Bifoga inte personliga anteckningar, OAuth-JSON eller token. [Issues](https://github.com/AvocadoWasabi/StickyNotes/issues)
