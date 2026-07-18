# Semantički DB model — FantasyFootball

Baza podataka se sastoji od 8 domenskih tablica, 1 join tablice za N-N vezu i
standardnih ASP.NET Core Identity tablica (`AspNetUsers`, `AspNetRoles`, ...).
Tablice su definirane kroz EF Core modele u
[FantasyFootball.Core/Models/](FantasyFootball.Core/Models/), a relacije su
konfigurirane u
[FantasyFootball.Core/DAL/FantasyFootballDbContext.cs](FantasyFootball.Core/DAL/FantasyFootballDbContext.cs)
(context nasljeđuje `IdentityDbContext<AppUser, IdentityRole, string>`).

## Pregled tablica

| Tablica | Opis |
| --- | --- |
| `Players` | Stvarni nogometaši — osnovni podaci i statistika sezone |
| `FantasyTeams` | Korisnički fantasy timovi |
| `Leagues` | Lige u kojima se natječu fantasy timovi |
| `Transfers` | Transferi (kupnje/prodaje) igrača — jedan tim + smjer |
| `Gameweeks` | Runde sezone (kola) |
| `MatchPerformances` | Učinci igrača u pojedinoj utakmici (dio kola) |
| `Fixtures` | Odigrane utakmice kola s rezultatom (10 po kolu, generira ih simulacija) |
| `GameweekTeamScores` | Snapshot bodova fantasy tima za jedno kolo (uz startnih 11) |
| `FantasyTeamPlayer` | Join tablica za N-N vezu Player ↔ FantasyTeam (EF je automatski generira) |
| `AspNetUsers` | Korisnici (Identity) — prošireni kroz `AppUser` (OIB, JMBG, budžet, FK na tim) |

## Entiteti i svojstva

### Player — [FantasyFootball.Core/Models/Player.cs](FantasyFootball.Core/Models/Player.cs)
Predstavlja stvarnog igrača (Haaland, Salah, ...).

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK, IDENTITY |
| `FirstName` | string | |
| `LastName` | string | |
| `Position` | Position (enum) | Goalkeeper / Defender / Midfielder / Forward |
| `Club` | string | Stvarni klub (npr. "Liverpool") |
| `Nationality` | string | |
| `DateOfBirth` | DateTime | |
| `MarketValue` | double | Tržišna vrijednost (mil €) |
| `Goals`, `Assists`, `CleanSheets`, `TotalPoints` | int | Kumulativna statistika sezone |
| `IsDeleted` | bool | Soft delete — obrisani igrač ostaje u timovima (zasivljen) dok ga vlasnici ne prodaju; ne može se kupiti niti dobiva bodove |
| `FantasyTeams` | ICollection\<FantasyTeam> | N-N — timovi koji imaju ovog igrača |

### FantasyTeam — [FantasyFootball.Core/Models/FantasyTeam.cs](FantasyFootball.Core/Models/FantasyTeam.cs)
Korisnički fantasy tim koji se natječe u jednoj ligi.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `Name`, `OwnerName` | string | |
| `CreatedAt` | DateTime | |
| `SquadValue` | double | Vrijednost momčadi (transfer budžet je na korisniku — `AppUser.Budget`) |
| `TotalPoints` | int | Skupljeni bodovi u sezoni |
| `FreeTransfers` | int | Broj besplatnih transfera; vrijedi tek nakon prvog odigranog kola (+1 po kolu, gomilaju se); svaki transfer preko kvote stoji −4 boda |
| `TransferPointHits` | int | Ukupno oduzetih bodova zbog transfera preko besplatne kvote |
| `StartingLineupIds` | string? | CSV ID-eva startnih 11 igrača |
| `LogoPath` | string? | Putanja do uploadanog loga tima (null = nema loga) |
| `LeagueId` | int? | FK → Leagues.Id (nullable) |
| `League` | League? | navigacijsko svojstvo |
| `Players` | ICollection\<Player> | N-N — igrači u timu |
| `Owner` | AppUser? | 1-1 — vlasnik tima (FK je na strani `AspNetUsers`) |

### League — [FantasyFootball.Core/Models/League.cs](FantasyFootball.Core/Models/League.cs)
Liga koja okuplja više fantasy timova i transfera.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `Name`, `Season`, `Description` | string | |
| `CreatedAt` | DateTime | |
| `MaxTeams` | int | 2–20 |
| `JoinCode` | string | Šifra od 6 znakova za pridruživanje ligi (npr. "A7K9PQ") — **unique index** |
| `CreatorUserId` | string? | Identity ID korisnika koji je kreirao ligu (null za stare seedane lige) |
| `Teams` | ICollection\<FantasyTeam> | 1-N — timovi u ligi |
| `Transfers` | ICollection\<Transfer> | 1-N — svi transferi unutar lige |

### Transfer — [FantasyFootball.Core/Models/Transfer.cs](FantasyFootball.Core/Models/Transfer.cs)
Jedna transfer akcija: tim je **kupio** (`In`) ili **prodao** (`Out`) igrača.
(Stariji model s `FromTeam`/`ToTeam` i statusom je zamijenjen — sada jedan tim + smjer.)

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `PlayerId` | int | FK → Players.Id |
| `Player` | Player | navigacija |
| `TeamId` | int | FK → FantasyTeams.Id — tim koji je izveo akciju |
| `Team` | FantasyTeam | navigacija |
| `Direction` | TransferDirection (enum) | In (kupnja) / Out (prodaja) |
| `TransferDate` | DateTime | |
| `Price` | double | |
| `LeagueId` | int? | FK → Leagues.Id |
| `League` | League? | navigacija |

### Gameweek — [FantasyFootball.Core/Models/Gameweek.cs](FantasyFootball.Core/Models/Gameweek.cs)
Jedno kolo (runda) sezone. Kolo se smatra "odigranim" ako ima fixture-a.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `WeekNumber` | int | Redni broj kola (1–38) |
| `StartDate`, `EndDate` | DateTime | `IValidatableObject` provjerava EndDate > StartDate |
| `Performances` | ICollection\<MatchPerformance> | 1-N — učinci igrača u kolu |
| `Fixtures` | ICollection\<Fixture> | 1-N — odigrane utakmice kola |

### MatchPerformance — [FantasyFootball.Core/Models/MatchPerformance.cs](FantasyFootball.Core/Models/MatchPerformance.cs)
Učinak jednog igrača u jednoj utakmici unutar kola.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `PlayerId` | int | FK → Players.Id |
| `Player` | Player | navigacija |
| `GameweekId` | int | FK → Gameweeks.Id |
| `Gameweek` | Gameweek | navigacija |
| `MatchDate` | DateTime | |
| `Opponent` | string | |
| `Goals`, `Assists`, `YellowCards`, `RedCards`, `MinutesPlayed`, `PointsEarned` | int | |
| `Saves` | int | Golman: obrane (1 bod na svake 3) |
| `GoalsConceded` | int | GK/DEF: primljeni golovi (−1 na svaka 2) |
| `Bonus` | int | Bonus bodovi (1–3) za najbolje igrače utakmice |
| `CleanSheet` | bool | |

### Fixture — [FantasyFootball.Core/Models/Fixture.cs](FantasyFootball.Core/Models/Fixture.cs)
Jedna odigrana utakmica unutar kola (10 po kolu). Generira se simulacijom i
prikazuje u statistici kola kao rezultat (HomeClub H–A AwayClub).

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `GameweekId` | int | FK → Gameweeks.Id |
| `Gameweek` | Gameweek | navigacija |
| `HomeClub`, `AwayClub` | string | |
| `HomeGoals`, `AwayGoals` | int | |

### GameweekTeamScore — [FantasyFootball.Core/Models/GameweekTeamScore.cs](FantasyFootball.Core/Models/GameweekTeamScore.cs)
Snapshot rezultata fantasy tima za jedno kolo — "bodovi koje si upisao".
Kreira se pri potvrdi simulacije; koristi ga slider u "Moja momčad" i
poništavanje (brisanje) kola.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `FantasyTeamId` | int | FK → FantasyTeams.Id |
| `FantasyTeam` | FantasyTeam | navigacija |
| `GameweekId` | int | FK → Gameweeks.Id |
| `Gameweek` | Gameweek | navigacija |
| `Points` | int | Bodovi tima u tom kolu |
| `LineupIds` | string? | CSV ID-eva startnih 11 u trenutku potvrde kola (povijesni snapshot) |

### AppUser — [FantasyFootball.Core/Models/AppUser.cs](FantasyFootball.Core/Models/AppUser.cs)
Aplikacijski korisnik — proširuje `IdentityUser` (string PK; email, username i
password hash dolaze iz Identity baze). Tablica `AspNetUsers`.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | string | PK (Identity GUID) |
| `OIB` | string | Točno 11 znamenki |
| `JMBG` | string | Točno 13 znamenki |
| `CreatedAt` | DateTime | |
| `Budget` | double | Transfer budžet — koliko korisnik može potrošiti na nove igrače |
| `FantasyTeamId` | int? | FK → FantasyTeams.Id — 1-1, korisnik ima (opcionalno) jedan tim |
| `FantasyTeam` | FantasyTeam? | navigacija |

## Veze između tablica

Fantasy domena (korisnici, timovi, lige, transferi):

```
┌─────────────┐ 1   0..1 ┌──────────────┐  N ─────── N  ┌─────────┐
│ AspNetUsers │─────────>│ FantasyTeams │<─────────────>│ Players │
│  (AppUser)  │          └──────────────┘(FantasyTeamPlayer)──────┘
└─────────────┘            ▲          ▲ 1                 1 ▲
                         1 │          │                     │
                           │          │ N                 N │
  ┌──────────┐  1     N    │        ┌─┴───────────┐         │
  │ Leagues  │─────────────┘        │  Transfers  │─────────┘
  └────┬─────┘                      └─────────────┘
     1 │                                   ▲ N
       └───────────────────────────────────┘
```

Kola i simulacija:

```
                  ┌────────────┐
        ┌─────────│ Gameweeks  │─────────┐
      1 │         └──────┬─────┘         │ 1
        │              1 │               │
      N ▼              N ▼             N ▼
┌───────────┐ ┌────────────────────┐ ┌───────────────────┐
│ Fixtures  │ │ GameweekTeamScores │ │ MatchPerformances │
└───────────┘ └─────────┬──────────┘ └─────────┬─────────┘
                      N │                    N │
                      1 ▼                    1 ▼
              ┌──────────────┐          ┌─────────┐
              │ FantasyTeams │          │ Players │
              └──────────────┘          └─────────┘
```

### Detalji relacija

| Od | Do | Tip | Brisanje |
| --- | --- | --- | --- |
| League | FantasyTeam | 1-N | `SetNull` — tim ostaje nakon brisanja lige |
| League | Transfer | 1-N | `SetNull` |
| FantasyTeam | Player | N-N | (default) — preko join tablice `FantasyTeamPlayer` |
| Transfer | Player | N-1 (required) | `Restrict` |
| Transfer | FantasyTeam (Team) | N-1 (required) | `Restrict` |
| Gameweek | MatchPerformance | 1-N | `Cascade` — brisanjem kola brišu se učinci |
| Player | MatchPerformance | 1-N (required) | `Restrict` |
| Gameweek | Fixture | 1-N | `Cascade` — brisanjem kola brišu se i utakmice |
| Gameweek | GameweekTeamScore | 1-N | `Cascade` |
| FantasyTeam | GameweekTeamScore | 1-N | `Restrict` — tim se prije brisanja mora ručno očistiti od rezultata |
| AppUser | FantasyTeam | 1-0..1 (FK u `AspNetUsers`) | `SetNull` |

Uz relacije, `Leagues.JoinCode` ima **unique index**.

Svi `Restrict` ciljevi su postavljeni namjerno da se izbjegnu SQL Server
"multi-cascade path" greške (Transfer, MatchPerformance i GameweekTeamScore
referenciraju iste tablice kroz više puteva, a više cascade puteva do iste
tablice nije dozvoljeno u MSSQL-u).

## Pomoćni enumi

- [FantasyFootball.Core/Models/Position.cs](FantasyFootball.Core/Models/Position.cs) — Goalkeeper, Defender, Midfielder, Forward
- [FantasyFootball.Core/Models/TransferDirection.cs](FantasyFootball.Core/Models/TransferDirection.cs) — In (kupnja), Out (prodaja)

Oba enuma EF Core sprema kao `int` u bazu.

## Nemapirani pomoćni tipovi (nisu tablice)

- [FantasyFootball.Core/Models/Standing.cs](FantasyFootball.Core/Models/Standing.cs) — redak tablice poretka lige (Rank, Team, Points, GamesPlayed); računa se u memoriji, nema DbSet
- [FantasyFootball.Core/Models/DTO/](FantasyFootball.Core/Models/DTO/) — DTO objekti koje koristi REST API (`FantasyFootball.Api`)
- [FantasyFootball.Core/Models/ViewModels/](FantasyFootball.Core/Models/ViewModels/) — dijeljeni view modeli (npr. `PlayerFormViewModel`)
