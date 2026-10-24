[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# Installation och inställningar

[README](../README.sv.md) · [Usage (English)](USAGE.en.md)

## 1. Installation

För Windows 10 / 11 (x64). Administratörsrättigheter och separat installation av .NET behövs inte.

1. Hämta **StickyNotes-win-x64.zip** från [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest). `Source code` innehåller källkoden.
2. Packa upp hela ZIP-filen och kör `Install.cmd`.
3. Starta **Markdown Sticky Notes** från Start-menyn.

Du kan också köra `StickyNotes.exe` direkt ur den uppackade mappen och behålla alla medföljande filer. Appen är osignerad och Windows kan be om bekräftelse. Automatisk start registreras inte.

Guiden gäller v0.0.11. Se [ändringsloggen (på engelska)](../CHANGELOG.md#v0-0-11) för flera valv, Tasks 8.2.2 och nio UI-språk.

## 2. Lagringsplats och anslutning

### Använda Obsidian CLI (standard)

1. Aktivera **Settings → General → Command line interface** i Obsidian och följ registreringsanvisningarna. Om valet saknas, kontrollera kraven på installationsprogrammet i den [officiella CLI-guiden](https://obsidian.md/help/cli).
2. Öppna det valda valvet och låt Obsidian vara igång.
3. Kontrollera den absoluta sökvägen till `Obsidian.com` i appens allmänna inställningar, vanligtvis `C:\Program Files\Obsidian\Obsidian.com`. Ändra den om Obsidian är installerat på en annan plats.
4. Aktivera CLI för vald post under valv och dagliga anteckningar. Ange valvets namn/ID och klicka på knappen för att hämta valvmappen via CLI. Ett tomt fält använder Obsidians standardval; enbart inmatning hämtar inte mappen.
5. Kontrollera det hämtade valvet och inställningarna för dagliga anteckningar, välj mapp för fästisar och spara inställningarna.

Vid mappbyte flyttar ”Ja” även befintliga anteckningar skapade av appen; ”Nej” ändrar bara framtida lagring; ”Avbryt” avbryter bytet. [Omfattning och återställning (på engelska)](STICKY-FOLDERS.md#english). CLI-fel växlar inte automatiskt till lokal läsning.

### Använda lokala filer

Stäng av Obsidian CLI i inställningarna, välj lokal basmapp och anteckningsmapp och spara. Ursprunglig lagringsplats är `Documents/StickyNotesData`. Tasks-/Dataview-frågor är inte tillgängliga. Lokala dagliga anteckningar kan ställas in med en mapp och ett reguljärt uttryck som fångar datumet. [Exempel (på engelska)](USAGE.en.md#daily).

## 3. Valfria integrationer

| Integration | I Obsidian / Google | I Sticky Notes |
| --- | --- | --- |
| Dagliga anteckningar | Aktivera kärntillägget Daily notes, välj mapp och datumformat, skapa dagens anteckning | Visa en daglig anteckning i menyn för att skapa/öppna och välj rubrik; tomt visar hela anteckningen |
| Tasks | Aktivera i rätt valv; v0.0.11 kräver stabil version 8.2.2 eller senare | Öppna en anteckning med ett `tasks`-block. [Inställningar och gränser (på engelska)](TASKS-CLI-PREVIEW.md#english) |
| Dataview | Aktivera i rätt valv | Visa LIST / TABLE / TASK i `dataview`-block. [Inställningar och gränser (på engelska)](DATAVIEW.md#english) |
| Google | Ställ in Calendar API, Tasks API och OAuth för skrivbordsapp i ett eget Cloud-projekt | Importera JSON, godkänn åtkomst och skriv `@calendar today` i ett eget stycke. [Anslutning (på engelska)](GOOGLE-CALENDAR.en.md#setup) |

Vanliga Markdown-kryssrutor kräver inte Tasks. Uppgiftsförslag är inställda på Automatiskt som standard och förblir avstängda utan Tasks om de inte aktiveras manuellt. Google-auktorisering behövs inte om Google-integrationen inte används.

## 4. Skapa och redigera anteckningar

Skapa eller öppna Markdown via menyn för att skapa/öppna under `…`. Klicka i texten eller tryck `Ctrl+E` för att redigera; `Ctrl+S` / `Esc` sparar. Den första dagliga länken kräver dagens fil; appen skapar den inte automatiskt. Välj visningsspråk i inställningarna, spara och starta om. Gränssnittet i v0.0.11 stöder de nio språken i README.

## Felsökning

| Symtom | Kontroll |
| --- | --- |
| CLI kan inte ansluta | Obsidian igång, CLI aktiverat, absolut sökväg till `Obsidian.com` och valvets namn/ID; hämta mappen igen |
| Dagens anteckning saknas | Inställningar för kärntillägget Daily notes och dagens fil; inställningar från andra tillägg för dagliga anteckningar hämtas inte automatiskt |
| Tasks begär uppdatering | Uppdatera Tasks till 8.2.2 eller senare i rätt valv och läs in Tasks / Dataview på nytt i fästisen |
| Konflikt vid sparande | Behåll utkastet och läs in på nytt; appen förhindrar överskrivning med gammalt innehåll |
| Google-auktorisering misslyckas | Kontrollera API:er, OAuth, testanvändare och behörigheter i [guiden (på engelska)](GOOGLE-CALENDAR.en.md) |

## Uppdatering, data och borttagning

För att uppdatera: spara ändringar, avsluta via meddelandefältet, packa upp ny ZIP i en annan mapp och kör `Install.cmd`. Anteckningar, inställningar och kopior ligger separat från programmet. Inställningar från v0.0.6 eller äldre utan CLI-val aktiverar nu CLI: stäng av och spara för lokalt läge; filer flyttas inte automatiskt. Egna dagliga mönster från v0.0.1 kan behöva rättas.

| Data | Plats |
| --- | --- |
| Program | %LOCALAPPDATA%/Programs/MarkdownStickyNotes |
| Inställningar och inloggningsuppgifter | %LOCALAPPDATA%/StickyNotes |
| Kopior före skrivning | %LOCALAPPDATA%/StickyNotes/backups |
| Anteckningar | Valt valv/mapp |

För att ta bort appen: avsluta, ta bort installationsmappen och genvägen i Start-menyn. Anteckningar, inställningar och kopior finns kvar; granska dem innan de tas bort separat.
