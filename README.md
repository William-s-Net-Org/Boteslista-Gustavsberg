# Gustavsbergs Bandy – Böteslista A-lag

En Windowsapplikation för att hantera A-lagets trupp, böter och aktiviteter. Byggd med **C#, WinUI 3 och SQLite**, med en enkel struktur inspirerad av **DDD och Clean Architecture**.

Projektets mål är att hålla koden lättläst, med tydliga ansvarsområden och få beroenden.

## Funktioner

- **Startsida** med Gustavsbergs Bandys logotyp och välkomstmeddelandet ”Böteslista A-lag”.
- **Trupphantering** med tröjnummer, förnamn, efternamn, position och ålder.
- **Böteskategorier** med orsak, kort beskrivning och kostnad.
- **Böteslista** med löpnummer i registreringsordning och möjlighet att ta bort felaktiga böter.
- **Betalstatus** som kan ändras mellan betald och obetald.
- **Sammanställning** av totalt, betalt och kvar att betala för laget och varje spelare.
- **Tvåveckorsschema** med val av ISO-år och startvecka samt möjlighet att lägga till, redigera och ta bort aktiviteter.
- **Förinlagda matcher** för säsongen 2026/2027 med länkar till matchkällorna.
- **Automatisk lokal lagring** som finns kvar efter omstart.

Betalda böter behålls i historiken och totalsumman. Borttagna böter räknas inte med, och deras löpnummer återanvänds aldrig.

## Teknik

| Del | Teknik |
| --- | --- |
| Språk | C# |
| Plattform | .NET 10, Windows x64 |
| Gränssnitt | WinUI 3, Windows App SDK 2.5.1 |
| Databas | SQLite via Windows inbyggda `winsqlite3` |
| Arkitektur | Domain, Application, Infrastructure och presentation |
| Verifiering | Körbara kontroller av affärsregler och SQLite-lagring |

## Kom igång

### Förutsättningar

- Windows 10 version 2004 eller senare, eller Windows 11.
- .NET 10 SDK för att bygga projektet.
- Internetanslutning för att återställa NuGet-paket första gången.
- Vid utveckling i Visual Studio: stöd för .NET 10 och WinUI/Windows App SDK.

### Bygg och starta

Klona eller ladda ner repot och öppna en terminal i katalogen som innehåller `GustavsbergBandy.slnx`.

```powershell
dotnet restore src/WinUI/WinUI.csproj
dotnet build src/WinUI/WinUI.csproj --no-restore -m:1
& ./src/WinUI/bin/Debug/net10.0-windows10.0.19041.0/win-x64/WinUI.exe
```

Du kan även öppna lösningen i Visual Studio och välja **WinUI** som startprojekt och **x64** som plattform.

Om du använder det färdigbyggda Windowsarkivet: packa upp hela arkivet och öppna `app/WinUI.exe`. Det kräver .NET 10 Desktop Runtime för x64. Appen är för närvarande opaketerad och saknar signerad Store-installation.

## Användning

1. Öppna **Truppen** och lägg till spelarna.
2. Skapa lagets böteskategorier under **Böteskategorier**. Ange belopp med decimalcomma, exempelvis `50,50`.
3. Öppna **Böteslista**, välj spelare och kategori och registrera boten.
4. Markera boten som betald när betalningen är gjord. Betalstatus är en manuell registrering; appen genomför inga betalningar.
5. Följ lagets och spelarnas belopp under **Sammanställning**.
6. Välj startvecka och ISO-år under **Tvåveckorsschema**. Lägg till eller redigera aktiviteter med datum, tid, rubrik och beskrivning. Tom tid betyder att tiden inte är fastställd.

## Projektstruktur

```text
GustavsbergBandy/
├── GustavsbergBandy.slnx
├── Directory.Build.props
├── src/
│   ├── Domain/
│   │   └── Models.cs
│   ├── Application/
│   │   └── TeamService.cs
│   ├── Infrastructure/
│   │   └── SqliteTeamRepository.cs
│   └── WinUI/
│       ├── Assets/
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── MainWindow.cs
│       └── Fixtures.cs
├── tests/
│   ├── Checks.csproj
│   └── Program.cs
├── Matcher-2026-2027.csv
├── VERIFIERING.md
└── README.md
```

### Arkitektur

**Domain** innehåller domänmodellerna: spelare, böteskategori, bot, lagevent och lagets tillstånd. Lagret saknar beroenden till UI och databas.

**Application** innehåller användningsfallen i `TeamService`, validering och kontraktet `ITeamRepository`.

**Infrastructure** implementerar databaslagringen bakom repository-kontraktet.

**WinUI** innehåller presentationen och kopplar ihop tjänster och lagring vid start.

Beroendena går från Application till Domain och från Infrastructure till Application. Presentationen använder användningsfallen och väljer lagringsimplementation vid start.

Detta är en lätt DDD-tillämpning. Domänens begrepp används i modeller och användningsfall, medan reglerna samlas i applikationstjänsten. Lösningen undviker extra abstraktionslager för att hålla den lilla appen enkel.

En registrerad bot sparar en ögonblicksbild av spelarens namn, orsaken, beskrivningen och beloppet. Historiska böter påverkas därför inte av framtida ändringar i grunduppgifterna. Belopp hanteras med `decimal`.

## Databas

Databasen skapas automatiskt här:

```text
%LOCALAPPDATA%\GustavsbergBandy\team.db
```

SQLite lagrar lagets tillstånd som ett JSON-dokument i en tabell. Hela dokumentet sparas atomiskt vid varje ändring. Ingen separat databasserver behöver installeras.

Appen tillåter en körande instans per Windows-session. Vid ett skrivfel visas ett felmeddelande och tillståndet läses om från databasen.

För säkerhetskopiering: stäng appen och kopiera hela katalogen `GustavsbergBandy` från `%LOCALAPPDATA%`. Lägg inte databasen med lagets personuppgifter i GitHub-repot.

## Matchschema 2026/2027

Schemat innehåller **samtliga 15 publicerade seriematcher för Gustavsberg** på [Bandygrytans sida för Division 1 Herr Nord 2026](https://bandygrytan.se/#/competitions/2026/fx_1181126-fx_3413337-), kontrollerade den **21 oktober 2026**. Varje seriematch har en individuell källänk.

Dessutom finns fem träningsmatcher från [Gustavsbergs Bandys officiella matchlista](https://www.gustavsbergsbandy.se/match/?ID=269134).

Vid kontrollen saknades en publicerad hemmamatch mot Helenelund. Den behöver läggas till när datum och tid finns tillgängliga.

Matchuppgifterna importeras en gång vid första start. De synkroniseras inte automatiskt med Bandygrytan. Manuellt redigerade eller borttagna matcher återställs inte vid omstart. Alla tider visas som svensk lokal tid.

Klubbens logotyp finns i `src/WinUI/Assets/club.webp` och kommer från klubbens officiella webbplats.

## Tester

Kör kontrollerna från repots rotkatalog:

```powershell
dotnet build tests/Checks.csproj -m:1
dotnet tests/bin/Debug/net10.0/Checks.dll
```

Kontrollerna använder en separat temporär SQLite-databas och verifierar:

- Att dubbla tröjnummer avvisas.
- Att betalda böter finns kvar i totalsumman.
- Att löpnummer inte återanvänds efter borttagning.
- Att event kan redigeras över ett årsskifte.
- Att ISO-veckor hanteras korrekt över årsskiften.
- Att matchimporten bara sker en gång.
- Att data, betalningar och svenska tecken finns kvar efter omstart.
- Att en betalmarkering kan återställas utan att totalsumman ändras.

Kontrollerna är ett körbart konsolprojekt, inte en testsamling som körs med `dotnet test`. Se [VERIFIERING.md](VERIFIERING.md) för genomförda kontroller och kvarstående begränsningar.

## Nuvarande begränsningar

- Appen körs endast på Windows x64.
- Data lagras lokalt för den aktuella Windows-användaren.
- Inloggning, behörighetsroller och gemensam lagdatabas saknas.
- Matchändringar hämtas inte automatiskt.
- Samtliga formulär och dialoger har ännu inte genomgått fullständig manuell verifiering.
- Appen är inte publicerad i Microsoft Store, App Store eller Google Play.

## Fortsatt utveckling

- [ ] Gemensam databas via ett autentiserat API.
- [ ] Inloggning och roller för administratörer och lagmedlemmar.
- [ ] Mobilapp för iPhone och Android som återanvänder Domain och Application.
- [ ] Synkronisering och hantering av samtidiga ändringar.
- [ ] Automatiserad säkerhetskopiering.
- [ ] Uppdatering av matchschemat från en godkänd datakälla.
- [ ] Signerad paketering och butikspublicering.

## Bidra

Rapportera buggar och förbättringsförslag via GitHub Issues. Beskriv vad du gjorde, vilket resultat du förväntade dig och vad som faktiskt hände. Använd påhittade spelaruppgifter och böter i exempel och felrapporter.

Vid kodändringar: följ projektets lagerindelning, håll metoderna små och kör kontrollerna innan du öppnar en pull request.

## Licens och klubbmaterial

Ingen öppen källkodslicens har valts för projektet ännu. Klubbens namn och logotyp ingår inte automatiskt i en framtida kodlicens. Tillstånd för användning av klubbmaterial behöver bekräftas inför offentlig distribution.
