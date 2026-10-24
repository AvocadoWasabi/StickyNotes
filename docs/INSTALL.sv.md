[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# Installation och inställningar

[README](../README.sv.md) · [Usage (English)](USAGE.en.md)

## 1. Kom igång

Windows 10 / 11 (x64). .NET ingår i ZIP-filen; administratörsrättigheter behövs inte. CLI är på i nya inställningar: stäng av för lokala filer. Inget särskilt Sticky Notes-tillägg för Obsidian behövs.

1. Hämta `StickyNotes-win-x64.zip` från Releases, packa upp allt och kör `Install.cmd`.
2. Öppna Obsidian, aktivera CLI och håll det valda valvet öppet.
3. Kontrollera den absoluta sökvägen till `Obsidian.com` och valvets namn/ID i appens inställningar. Hämta valvmappen via CLI, välj anteckningsmapp och spara.
4. Skapa/öppna en anteckning eller länka ett avsnitt eller en daglig anteckning under `…`.

[Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest)

Du kan också köra `StickyNotes.exe` direkt ur den uppackade mappen och behålla alla medföljande filer. Appen är osignerad och Windows kan be om bekräftelse. Automatisk start registreras inte.

Guiden gäller v0.0.11. Se [ändringsloggen (på engelska)](../CHANGELOG.md#v0-0-11) för flera valv, Tasks 8.2.2 och nio UI-språk.

## 2. Obsidian eller lokala filer

Aktivera CLI i Obsidian och följ registreringsanvisningarna. Saknas inställningen, kontrollera installationskraven i den officiella CLI-guiden. Kontrollera den absoluta sökvägen i appens inställningar (oftast `C:\Program Files\Obsidian\Obsidian.com`). Under valv och dagliga anteckningar aktiverar du CLI för posten, anger namn/ID och trycker på knappen för att hämta mappen. Tomt använder Obsidians standardval; att skriva hämtar inget. Kontrollera mapparna, välj anteckningsmapp och spara.

[Obsidian CLI](https://obsidian.md/help/cli)

Utan Obsidian: stäng av CLI, välj lokal basmapp och anteckningsmapp och spara. Första platsen är `Documents/StickyNotesData`. Tasks-/Dataview-frågor är inte tillgängliga. Lokala dagliga anteckningar kan använda en mapp och ett reguljärt uttryck med datumgrupper.

Vid mappbyte flyttar Ja även befintliga anteckningar skapade av appen; Nej ändrar bara framtida lagring; Avbryt stoppar bytet. CLI-fel växlar inte automatiskt till lokal läsning.

[Storage (English)](STICKY-FOLDERS.md#english) · [Daily notes (English)](USAGE.en.md#daily)

## 3. Valfria integrationer

Daily notes: ange mapp/datumformat i Obsidian, skapa dagens fil och visa en daglig anteckning via menyn för att skapa/öppna. Utan vald rubrik visas hela anteckningen. Aktivera Tasks 8.2.2+ eller Dataview i valvet och öppna `tasks`-/`dataview`-block. Vanliga kryssrutor behöver inte Tasks. Förslag är automatiska som standard och av utan Tasks, om de inte aktiveras manuellt. För Google: konfigurera egna API:er och OAuth för skrivbord, importera JSON, godkänn och skriv `@calendar today` i ett eget stycke. Utan Google-integrationen behövs inget Google-godkännande.

[Tasks (English)](TASKS-CLI-PREVIEW.md#english) · [Dataview (English)](DATAVIEW.md#english) · [Google (English)](GOOGLE-CALENDAR.en.md#setup)

## 4. Grundläggande användning

Klicka i texten (enkel-/dubbelklick kan väljas) eller tryck `Ctrl+E` för att redigera; `Ctrl+S` / `Esc` sparar. Första Esc stänger öppna förslag. Bocka i läsläget. `○ / ●` fäster anteckningen. Stäng en anteckning via `…` och avsluta appen från meddelandefältet.

Gränssnitt, README, installation och bilder finns på de nio språken ovan. Välj språk i inställningarna, spara och starta om. Utförliga referenser är främst på japanska, engelska och förenklad kinesiska.

## Felsökning

CLI: kontrollera att Obsidian körs, CLI är på och sökväg/valv-ID stämmer; hämta mappen igen. Dagens anteckning saknas: kontrollera kärntillägget Daily notes och dagens fil; andra tilläggs inställningar upptäcks inte. Tasks-varning: uppdatera tillägget i rätt valv och uppdatera länkade data. Sparningskonflikt: behåll utkastet och läs in igen. Google: kontrollera API:er, testanvändare och behörigheter i den engelska guiden.

## Uppdatering, data och borttagning

För att uppdatera: spara ändringar, avsluta via meddelandefältet, packa upp ny ZIP i en annan mapp och kör `Install.cmd`. Anteckningar, inställningar och kopior ligger separat från programmet. Inställningar från v0.0.6 eller äldre utan CLI-val aktiverar nu CLI: stäng av och spara för lokalt läge; filer flyttas inte automatiskt. Egna dagliga mönster från v0.0.1 kan behöva rättas.

| Data | Plats |
| --- | --- |
| Program | %LOCALAPPDATA%/Programs/MarkdownStickyNotes |
| Inställningar och inloggningsuppgifter | %LOCALAPPDATA%/StickyNotes |
| Kopior före skrivning | %LOCALAPPDATA%/StickyNotes/backups |
| Anteckningar | Valt valv/mapp |

För att ta bort appen: avsluta, ta bort installationsmappen och genvägen i Start-menyn. Anteckningar, inställningar och kopior finns kvar; granska dem innan de tas bort separat.
