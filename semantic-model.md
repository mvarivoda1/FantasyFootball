# Semantički DB model — FantasyFootball

Baza podataka se sastoji od 6 glavnih tablica i 1 join tablice za N-N vezu.
Tablice su definirane kroz EF Core modele u [Models/](Models/), a relacije su
konfigurirane u [DAL/FantasyFootballDbContext.cs](DAL/FantasyFootballDbContext.cs).

## Pregled tablica

| Tablica | Opis |
| --- | --- |
| `Players` | Stvarni nogometaši — osnovni podaci i statistika sezone |
| `FantasyTeams` | Korisnički fantasy timovi |
| `Leagues` | Lige u kojima se natječu fantasy timovi |
| `Transfers` | Transferi igrača između fantasy timova |
| `Gameweeks` | Rundi sezone (kola) |
| `MatchPerformances` | Učinci igrača u pojedinoj utakmici (dio kola) |
| `FantasyTeamPlayer` | Join tablica za N-N vezu Player ↔ FantasyTeam (EF je automatski generira) |

## Entiteti i svojstva

### Player — [Models/Player.cs](Models/Player.cs)
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
| `FantasyTeams` | ICollection<FantasyTeam> | N-N — timovi koji imaju ovog igrača |

### FantasyTeam — [Models/FantasyTeam.cs](Models/FantasyTeam.cs)
Korisnički fantasy tim koji se natječe u jednoj ligi.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `Name`, `OwnerName` | string | |
| `CreatedAt` | DateTime | |
| `Budget` | double | Preostali budžet |
| `TotalPoints` | int | Skupljeni bodovi u sezoni |
| `LeagueId` | int? | FK → Leagues.Id (nullable) |
| `League` | League? | navigacijsko svojstvo |
| `Players` | ICollection<Player> | N-N — igrači u timu |

### League — [Models/League.cs](Models/League.cs)
Liga koja okuplja više fantasy timova i transfera.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `Name`, `Season`, `Description` | string | |
| `CreatedAt` | DateTime | |
| `MaxTeams` | int | |
| `Teams` | ICollection<FantasyTeam> | 1-N — timovi u ligi |
| `Transfers` | ICollection<Transfer> | 1-N — svi transferi unutar lige |

### Transfer — [Models/Transfer.cs](Models/Transfer.cs)
Pojedinačni transfer igrača između dva fantasy tima (ili ulazak igrača u tim kad `FromTeam` je null).

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `PlayerId` | int | FK → Players.Id |
| `Player` | Player | navigacija |
| `FromTeamId` | int? | FK → FantasyTeams.Id (nullable — slobodni agent) |
| `FromTeam` | FantasyTeam? | navigacija |
| `ToTeamId` | int | FK → FantasyTeams.Id |
| `ToTeam` | FantasyTeam | navigacija |
| `TransferDate` | DateTime | |
| `Price` | double | |
| `Status` | TransferStatus (enum) | Pending / Accepted / Rejected / Cancelled |
| `LeagueId` | int? | FK → Leagues.Id |
| `League` | League? | navigacija |

### Gameweek — [Models/Gameweek.cs](Models/Gameweek.cs)
Jedno kolo (runda) sezone.

| Svojstvo | Tip | Napomena |
| --- | --- | --- |
| `Id` | int | PK |
| `WeekNumber` | int | Redni broj kola |
| `StartDate`, `EndDate` | DateTime | |
| `Performances` | ICollection<MatchPerformance> | 1-N — učinci igrača u kolu |

### MatchPerformance — [Models/MatchPerformance.cs](Models/MatchPerformance.cs)
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
| `CleanSheet` | bool | |

## Veze između tablica

```
┌──────────┐  1    N  ┌──────────────┐  N ─────── N  ┌─────────┐
│ Leagues  │─────────>│ FantasyTeams │◄─────────────>│ Players │
└──────────┘          └──────────────┘   (FantasyTeamPlayer)   └─────────┘
     │ 1                     ▲                            ▲  ▲
     │                       │ From/To                    │  │
     │ N                     │                          1 │  │ 1
     ▼                   ┌──────────┐                     │  │
  ┌──────────┐           │ Transfer │─────────────────────┘  │
  │Transfers │<──────────┴──────────┘                        │
  └──────────┘                                               │
                                                             │
                          ┌────────────┐  1        N  ┌──────┴────────────┐
                          │ Gameweeks  │─────────────>│ MatchPerformances │
                          └────────────┘              └───────────────────┘
```

### Detalji relacija

| Od | Do | Tip | Brisanje |
| --- | --- | --- | --- |
| League | FantasyTeam | 1-N | `SetNull` — tim ostaje nakon brisanja lige |
| League | Transfer | 1-N | `SetNull` |
| FantasyTeam | Player | N-N | (default) — preko join tablice `FantasyTeamPlayer` |
| Transfer | Player | N-1 (required) | `Restrict` |
| Transfer | FantasyTeam (FromTeam) | N-1 (nullable) | `Restrict` |
| Transfer | FantasyTeam (ToTeam) | N-1 (required) | `Restrict` |
| Gameweek | MatchPerformance | 1-N | `Cascade` — brisanjem kola brišu se učinci |
| Player | MatchPerformance | 1-N (required) | `Restrict` |

Svi `Restrict` ciljevi su postavljeni namjerno da se izbjegnu SQL Server
"multi-cascade path" greške (jer Transfer i MatchPerformance referenciraju
Player, a više puteva cascade-a do iste tablice nije dozvoljeno u MSSQL-u).

## Pomoćni enumi

- [Models/Position.cs](Models/Position.cs) — Goalkeeper, Defender, Midfielder, Forward
- [Models/TransferStatus.cs](Models/TransferStatus.cs) — Pending, Accepted, Rejected, Cancelled

Oba enuma EF Core sprema kao `int` u bazu.
