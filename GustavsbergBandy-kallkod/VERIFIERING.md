# Verifiering 21 oktober 2026

- WinUI-projektet kompilerat med .NET SDK 10.0.401, Windows App SDK 2.5.1, x64.
- Åtta automatiserade kontroller godkända: dubbla tröjnummer, betalda böter i totalen, löpnummer efter borttagning, event över årsskifte, ISO-vecka 53, import en gång, SQLite-omstart med svenska tecken och återställning av betalstatus.
- Appen startad och startsidan inspekterad i Windows: officiell klubblogotyp, exakt rubrik “Böteslista A-lag”, välkomsttext och navigation synliga.
- Bandygrytans seriesida läst i webbläsare. Samtliga 15 där publicerade Gustavsbergsmatcher inlagda med datum, tid och individuell källänk.
- NuGet gav nätverksvarningar NU1801 och NU1900 under byggnad: cachelagrade paket kunde användas, men sårbarhetskontrollen kunde inte nå servern. Kör restore med internetåtkomst före distribution.
- Ingen fullständig manuell genomgång av samtliga formulär/dialoger genomförd. Ingen Store-certifiering, signerad installation, molnsynkronisering eller mobilversion levererad.
