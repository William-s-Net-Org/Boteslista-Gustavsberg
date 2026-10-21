# Gustavsbergs Bandy – Böteslista A-lag

WinUI 3 för Windows 10 (version 2004 eller senare) och Windows 11, x64. Svenskt gränssnitt med klubbens logotyp, trupp, böteskategorier, numrerad böteslista, betalningshistorik och redigerbart tvåveckorsschema.

## Starta

I Windows-paketet: packa upp hela arkivet och öppna `app/WinUI.exe`. Det kräver .NET 10 Desktop Runtime för x64. `Starta.ps1` är en alternativ startfil.

För källkoden: öppna `GustavsbergBandy.slnx` i Visual Studio med stöd för .NET 10 och WinUI/Windows App SDK, välj WinUI som startprojekt och x64. Alternativt i PowerShell från denna katalog:

```powershell
dotnet build src/WinUI/WinUI.csproj -m:1
& ./src/WinUI/bin/Debug/net10.0-windows10.0.19041.0/win-x64/WinUI.exe
```

NuGet behöver kunna nås vid första installationen. Windows App SDK 2.5.1 är låst i projektfilen. Appen är en opaketerad Windowsapp; denna leverans är inte en signerad Store-installation.

## Användning

1. Lägg till spelare med tröjnummer, förnamn, efternamn, position och ålder.
2. Skapa böteskategorier med orsak, beskrivning och kostnad i kronor. Använd decimalcomma, exempelvis `50,50`.
3. Välj spelare och kategori i Böteslista och registrera boten.
4. Markera betald eller obetald. Betalda böter finns kvar i lista och totalsumma. Sammanställningen visar totalt, betalt och kvar för laget och varje spelare.
5. Ta bort felaktiga böter efter bekräftelse. De tas ur listan och summorna, men löpnumret återanvänds aldrig.
6. Ange ISO-år och startvecka i schemat. Det visar måndag–söndag för två veckor och hanterar årsskiften. Lägg till eller redigera event med datum, tid och beskrivning. Tom tid betyder ej fastställd.

## Enkel DDD och Clean Architecture

```text
Domain          Spelare, böteskategori, bot, lagevent och lagets tillstånd
Application     TeamService: användningsfall, validering och repository-kontrakt
Infrastructure  SQLite-lagring implementerar kontraktet
WinUI           Presentation och sammansättning av beroenden
tests           Körbara kontroller för affärsregler och verklig SQLite-lagring
```

Referenser går inåt: Domain saknar tekniska beroenden; Application refererar Domain; Infrastructure refererar Application. WinUI kopplar ihop delarna vid start. Strukturen använder domänens ord utan extra generiska repositories, mediatorer eller ramverk för små operationer. Det är en lätt DDD-tillämpning; reglerna samlas i användningsfallen för att hålla modeller och kod enkla.

En bot lagrar namn, orsak, beskrivning och belopp som en historisk ögonblicksbild. Pengar använder `decimal`. Databasen lagrar ett sammanhängande JSON-dokument i en SQLite-tabell; en skrivning är atomisk. Det är medvetet enkelt för denna lilla lokala app. Vid en framtida serverversion bör entiteterna lagras i egna tabeller med samtidighetskontroll och migreringar.

## Databas och säkerhetskopiering

Allt sparas automatiskt i `%LOCALAPPDATA%\GustavsbergBandy\team.db`. SQLite ingår i Windows, så ingen databasserver behövs. Stäng appen innan du kopierar hela databasens katalog som säkerhetskopia. Ett skrivfel visas i gränssnittet och osparade ändringar laddas om från databasen. Appen tillåter en körande instans per Windows-session för att undvika konkurrerande lokala skrivningar.

Data är lokal för Windows-användaren. Ingen inloggning, molnsynkronisering eller gemensam lagdatabas ingår. Känsliga laguppgifter bör därför inte delas genom att lägga databasfilen i en publik mapp.

## Matchschema 2026/2027

Samtliga **15 Gustavsbergsmatcher som publicerades** på [Bandygrytans Division 1 Herr Nord 2026](https://bandygrytan.se/#/competitions/2026/fx_1181126-fx_3413337-) har lagts in. Kontrollerat 21 oktober 2026. Varje match har en länk till sin match hos Bandygrytan. UNIK borta den 8 november är kl. 17.30 enligt Bandygrytan, vilket ersätter klubbens tidigare okända tid.

Listan innehåller inte någon publicerad hemmamatch mot Helenelund vid kontrollen. Ett event för denna match måste läggas till när datum finns; inget datum har hittats på. Fem träningsmatcher kommer från [klubbens matchlista](https://www.gustavsbergsbandy.se/match/?ID=269134). Matchernas arenor kan ses i källan; seriematcherna har inga gissade arenor i appen.

Matcher importeras en gång vid första start. Redigerade eller borttagna matcher återställs därför inte vid omstart. Appen bevakar inte flyttade matcher automatiskt. Uppdatera schemat manuellt när källan ändras. Klubbens logotyp ligger lokalt i `src/WinUI/Assets/club.webp` och hämtades från klubbens officiella sida. Bekräfta klubbens tillstånd före offentlig butikspublicering.

## Kontrollera

```powershell
dotnet build tests/Checks.csproj -m:1
dotnet tests/bin/Debug/net10.0/Checks.dll
```

Kontrollerna använder en separat temporär SQLite-databas och verifierar dubbla tröjnummer, betalda böter i totalsumman, löpnummer efter borttagning, redigering av event över årsskifte, ISO-veckor, engångsimport och omstart med svenska tecken. Användargränssnitt bör även provas manuellt före distribution.

## Nästa steg: telefoner och butik

WinUI 3 bygger Windowsappar och publiceras via Microsoft Store. Den kan inte skickas till Apples App Store eller köras på Android. För nästa steg kan Domain och Application återanvändas i en .NET MAUI-app för iPhone och Android. Byt lokal lagring mot ett autentiserat API med gemensam databas, lagmedlemskap och roller (administratör/medlem). Lägg till serverbaserad behörighetskontroll, säkerhetskopior och samtidighetskontroll innan laget delar data.

Butikspublicering kräver utvecklarkonton, appidentiteter, signerad paketering, integritetsinformation, skärmbilder och butikernas granskning. iOS kräver även Mac/Xcode vid byggnad och signering. Ingen app har publicerats i någon butik i denna leverans.
