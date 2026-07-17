# FantasyFootball — Code Map

Vodič kroz kod: **što vidiš u aplikaciji ↔ gdje je to napisano u kodu**. Za svaki ekran je navedena frontend datoteka (Razor View), backend koji stoji iza njega (controller akcija, servisi, repozitoriji) i podaci koje koristi.

---

## 1. Kako nastaje jedna stranica (tok zahtjeva)

```
Preglednik (URL, npr. /igraci)
   │
   ▼
Routing ................ Program.cs (default ruta {controller}/{action}/{id?} + [Route] atributi na akcijama)
   │
   ▼
Auth + filteri ......... Program.cs (globalna autorizacija — sve traži login osim [AllowAnonymous])
   │                     Filters/RequireFantasyTeamFilter.cs (korisnika bez tima preusmjerava na Build)
   ▼
Controller ............. Controllers/*.cs (dohvaća podatke, validira, puni ViewModel)
   │
   ▼
Repository / DbContext . Repositories/*.cs + DAL/FantasyFootballDbContext.cs (EF Core → SQL Server)
   │
   ▼
View (HTML) ............ Views/<Controller>/<Akcija>.cshtml (Razor — renderira ViewModel u HTML)
   │                     umotano u Views/Shared/_Layout.cshtml (navbar, footer, skripte)
   ▼
Preglednik ............. wwwroot/css/site.css (izgled) + wwwroot/js/*.js (interaktivnost)
```

Konvencija: akcija `Index()` u `PlayerController` renderira `Views/Player/Index.cshtml`. ViewModel klase koje view prima su u `Models/ViewModels/`.

---

## 2. Zajednički okvir (vidljiv na svakoj stranici)

| Što vidiš | Frontend | Backend / logika |
|---|---|---|
| Navbar (Početna, Moj tim, Igrači, Timovi, Lige, Kola, Transferi), login/logout gumbi | [Views/Shared/_Layout.cshtml](Views/Shared/_Layout.cshtml) | Linkovi pokazuju na controllere navedene u §3; Logout je POST na `AccountController.Logout` |
| Search polje u navbaru s live prijedlozima | [_Layout.cshtml](Views/Shared/_Layout.cshtml) + [wwwroot/js/ff-search.js](wwwroot/js/ff-search.js) | AJAX `GET /Search/Suggest` → [SearchController.Suggest](Controllers/SearchController.cs) |
| Navbar se skriva pri scrollu prema dolje | [wwwroot/js/site.js](wwwroot/js/site.js) | — (čisti frontend) |
| Sav custom izgled (boje, kartice, teren…) | [wwwroot/css/site.css](wwwroot/css/site.css) + Bootstrap 5 ([wwwroot/lib/bootstrap/](wwwroot/lib/bootstrap/)) | — |
| Validacijske poruke na formama | [Views/Shared/_ValidationScriptsPartial.cshtml](Views/Shared/_ValidationScriptsPartial.cshtml) (jQuery validation) | Server-side: DataAnnotations na ViewModelima + `ModelState` provjere u controllerima |

---

## 3. Ekran po ekran

### 3.1 Početna / Dashboard — `/`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Cijeli dashboard (status kola, deadline, widgeti) | [Views/Home/Index.cshtml](Views/Home/Index.cshtml) | [HomeController.Index](Controllers/HomeController.cs#L32) — puni [DashboardViewModel](Models/ViewModels/DashboardViewModel.cs) iz svih 5 repozitorija |
| Team of the Week (teren s 11 igrača, strelice ← →) | [Views/Home/_TotwContent.cshtml](Views/Home/_TotwContent.cshtml) + [_TotwPlayer.cshtml](Views/Shared/_TotwPlayer.cshtml); strelice: AJAX u [site.js](wwwroot/js/site.js) (`/Home/TotwPartial?gw=`) | [HomeController.TotwPartial](Controllers/HomeController.cs#L212) + `PopulateTotw` (bira najbolju formaciju 1-3/5-2/5-1/3 po bodovima) |
| Igrač tjedna | dio [Index.cshtml](Views/Home/Index.cshtml) | `HomeController.Index` — najviše `PointsEarned` u zadnjem kolu |
| Top transferi IN/OUT | dio [Index.cshtml](Views/Home/Index.cshtml) | `HomeController.Index` — grupiranje `Transfer` zapisa iz [TransferRepository](Repositories/TransferRepository.cs) |
| Dostupnost igrača (Available/Doubt/Suspended) | dio [Index.cshtml](Views/Home/Index.cshtml) | `HomeController.Index` — izvedeno iz `MatchPerformance` zadnjeg kola (crveni karton → Suspended itd.) |
| Najvrjedniji timovi / najbolje lige / top strijelci | dio [Index.cshtml](Views/Home/Index.cshtml) | `HomeController.Index` — [FantasyTeamRepository](Repositories/FantasyTeamRepository.cs), [LeagueRepository](Repositories/LeagueRepository.cs), [PlayerRepository](Repositories/PlayerRepository.cs) |

### 3.2 Prijava / registracija — `/Account/...`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Login forma | [Views/Account/Login.cshtml](Views/Account/Login.cshtml) | [AccountController.Login](Controllers/AccountController.cs#L27) (GET + POST) — ASP.NET Identity `SignInManager` |
| Registracija (email, lozinka, OIB, JMBG) | [Views/Account/Register.cshtml](Views/Account/Register.cshtml) | [AccountController.Register](Controllers/AccountController.cs#L72) — `UserManager.CreateAsync`, dodjela role `User`, redirect na Build |
| "Prijava Googleom" gumb | dio Login/Register viewa | [AccountController.ExternalLogin / ExternalLoginCallback](Controllers/AccountController.cs#L135) — Google OAuth konfiguriran u [Program.cs](Program.cs#L63) |
| Dovršetak Google registracije (OIB/JMBG) | [Views/Account/ExternalLoginConfirmation.cshtml](Views/Account/ExternalLoginConfirmation.cshtml) | [AccountController.ExternalLoginConfirmation](Controllers/AccountController.cs#L188) |
| "Pristup odbijen" stranica | [Views/Account/AccessDenied.cshtml](Views/Account/AccessDenied.cshtml) | Cookie postavke u [Program.cs](Program.cs#L52) |

Korisnički model: [Models/AppUser.cs](Models/AppUser.cs) (Identity user + `Budget`, `FantasyTeamId`, OIB, JMBG). Claim `FantasyTeamId` dodaje [Services/AppUserClaimsPrincipalFactory.cs](Services/AppUserClaimsPrincipalFactory.cs).

### 3.3 Kreiranje tima (Build) — `/FantasyTeam/Build`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Odabir 15 igrača, budžet 100M, brojači po pozicijama | [Views/FantasyTeam/Build.cshtml](Views/FantasyTeam/Build.cshtml) (sva interaktivnost je inline `<script>` u tom viewu) | [FantasyTeamController.Build](Controllers/FantasyTeamController.cs#L61) GET/POST — server ponovno validira: 15 igrača, 2/5/5/3 formacija, budžet, max 3 po klubu |
| Automatski dolazak na ovu stranicu dok nemaš tim | — | [Filters/RequireFantasyTeamFilter.cs](Filters/RequireFantasyTeamFilter.cs) — globalni filter registriran u [Program.cs](Program.cs#L23) |

### 3.4 Moj tim — `/FantasyTeam/MyTeam`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Teren s početnih 11 + klupa, spremanje postave | [Views/FantasyTeam/MyTeam.cshtml](Views/FantasyTeam/MyTeam.cshtml) (drag/izbor logika inline u viewu) + [_PitchPlayer.cshtml](Views/Shared/_PitchPlayer.cshtml) | [FantasyTeamController.MyTeam](Controllers/FantasyTeamController.cs#L151) — postava se čita/piše kao CSV u `FantasyTeam.StartingLineupIds`; spremanje: [SaveLineup](Controllers/FantasyTeamController.cs#L212) (validira 1 GK, min 3 DEF…) |
| Slider po kolima s bodovima igrača/tima | dio [MyTeam.cshtml](Views/FantasyTeam/MyTeam.cshtml) | `MyTeam(gw)` — čita `MatchPerformances` i `GameweekTeamScores` iz DbContexta |
| ViewModel | [Models/ViewModels/MyTeamViewModel.cs](Models/ViewModels/MyTeamViewModel.cs) | — |

### 3.5 Uređivanje / brisanje tima, logo

| Što vidiš | Frontend | Backend |
|---|---|---|
| Edit forma (ime tima, vlasnik) | [Views/FantasyTeam/Edit.cshtml](Views/FantasyTeam/Edit.cshtml) | [FantasyTeamController.Edit/EditPost](Controllers/FantasyTeamController.cs#L269) — samo vlasnik (`Forbid` inače) |
| Upload loga (drag & drop) | Dropzone u [Edit.cshtml](Views/FantasyTeam/Edit.cshtml) ([wwwroot/lib/dropzone/](wwwroot/lib/dropzone/)) | [UploadLogo](Controllers/FantasyTeamController.cs#L512) / [RemoveLogo](Controllers/FantasyTeamController.cs#L551) — sprema u `wwwroot/uploads/teams/<id>/`, putanja u `FantasyTeam.LogoPath` |
| Brisanje tima s potvrdom | [Views/FantasyTeam/Delete.cshtml](Views/FantasyTeam/Delete.cshtml) | [DeleteConfirmed](Controllers/FantasyTeamController.cs#L340) — briše i transfere, bodove kola, logo; resetira budžet |

### 3.6 Igrači — `/igraci`, `/igrac/{id}`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Lista igrača + live pretraga/filteri | [Views/Player/Index.cshtml](Views/Player/Index.cshtml) (AJAX u `@section Scripts`) | [PlayerController.Index](Controllers/PlayerController.cs#L26); live pretraga: `GET /Player/Search` → [PlayerController.Search](Controllers/PlayerController.cs#L215) (vraća JSON) |
| Detalji igrača | [Views/Player/Details.cshtml](Views/Player/Details.cshtml) | [PlayerController.Details](Controllers/PlayerController.cs#L33) — [PlayerRepository.GetById](Repositories/PlayerRepository.cs) |
| Dodavanje igrača (samo Admin) | [Views/Player/Create.cshtml](Views/Player/Create.cshtml) | [PlayerController.Create](Controllers/PlayerController.cs#L44) — `[Authorize(Roles = "Admin")]` |
| **AI unos** ("opiši igrača tekstom") | AI panel u [Create.cshtml](Views/Player/Create.cshtml) — `fetch('/Player/AiParse')` | [PlayerController.AiParse](Controllers/PlayerController.cs#L55) → [Services/AiPlayerParser.cs](Services/AiPlayerParser.cs) (OpenAI GPT-4o mini, structured output → popunjava formu) |
| Autocomplete za klub u formi | [Views/Shared/_Autocomplete.cshtml](Views/Shared/_Autocomplete.cshtml) + [ff-autocomplete.js](wwwroot/js/ff-autocomplete.js) | `GET /Player/Clubs` → [PlayerController.Clubs](Controllers/PlayerController.cs#L258) |
| Datum rođenja picker | [Views/Shared/_DatePicker.cshtml](Views/Shared/_DatePicker.cshtml) + [ff-datepicker.js](wwwroot/js/ff-datepicker.js) | — |
| Edit / brisanje (Admin) | [Edit.cshtml](Views/Player/Edit.cshtml) / [Delete.cshtml](Views/Player/Delete.cshtml) | [EditPost](Controllers/PlayerController.cs#L145) / [DeleteConfirmed](Controllers/PlayerController.cs#L192) (brani brisanje ako je igrač u nekom timu) |

Entitet: [Models/Player.cs](Models/Player.cs), pozicije: [Models/Position.cs](Models/Position.cs), forma: [Models/ViewModels/PlayerFormViewModel.cs](Models/ViewModels/PlayerFormViewModel.cs).

### 3.7 Timovi — `/FantasyTeam`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Lista svih fantasy timova | [Views/FantasyTeam/Index.cshtml](Views/FantasyTeam/Index.cshtml) | [FantasyTeamController.Index](Controllers/FantasyTeamController.cs#L47) |
| Detalji tima (sastav) | [Views/FantasyTeam/Details.cshtml](Views/FantasyTeam/Details.cshtml) + [_SquadPlayer.cshtml](Views/Shared/_SquadPlayer.cshtml), [_TeamJersey.cshtml](Views/Shared/_TeamJersey.cshtml) | [FantasyTeamController.Details](Controllers/FantasyTeamController.cs#L53) |

### 3.8 Lige — `/lige`, `/liga/{id}`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Lista liga + live pretraga | [Views/League/Index.cshtml](Views/League/Index.cshtml) (`fetch('/League/Search')`) | [LeagueController.Index](Controllers/LeagueController.cs#L26) + [Search](Controllers/LeagueController.cs#L274) (JSON) |
| Detalji lige (tablica poretka) | [Views/League/Details.cshtml](Views/League/Details.cshtml) | [LeagueController.Details](Controllers/LeagueController.cs#L33) — [LeagueRepository](Repositories/LeagueRepository.cs); poredak: [Models/Standing.cs](Models/Standing.cs) |
| Kreiranje lige | [Views/League/Create.cshtml](Views/League/Create.cshtml) | [LeagueController.Create](Controllers/LeagueController.cs#L52) — generira jedinstveni 6-znakovni join kod (kriptografski RNG), kreator se automatski učlanjuje |
| "Liga kreirana" ekran s kodom za dijeljenje | [Views/League/Created.cshtml](Views/League/Created.cshtml) | [LeagueController.Created](Controllers/LeagueController.cs#L94) — vidljivo samo kreatoru |
| Pridruživanje ligi kodom | [Views/League/Join.cshtml](Views/League/Join.cshtml) | [LeagueController.Join](Controllers/LeagueController.cs#L116) — provjere: kod postoji, liga nije puna, nisi već član |
| Edit / brisanje (samo kreator) | [Edit.cshtml](Views/League/Edit.cshtml) / [Delete.cshtml](Views/League/Delete.cshtml) | [EditPost](Controllers/LeagueController.cs#L186) / [DeleteConfirmed](Controllers/LeagueController.cs#L247) — provjera `CreatorUserId` |

Entitet: [Models/League.cs](Models/League.cs).

### 3.9 Kola (Gameweeks) — `/kola`, `/kolo/{id}`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Lista kola + pretraga po broju | [Views/Gameweek/Index.cshtml](Views/Gameweek/Index.cshtml) (`fetch('/Gameweek/Search')`) | [GameweekController.Index](Controllers/GameweekController.cs#L29) + [Search](Controllers/GameweekController.cs#L218) |
| Detalji kola (utakmice, rezultati) | [Views/Gameweek/Details.cshtml](Views/Gameweek/Details.cshtml) | [GameweekController.Details](Controllers/GameweekController.cs#L36) → [GameweekDetailsViewModel](Models/ViewModels/GameweekDetailsViewModel.cs) |
| **"Simuliraj kolo"** gumb (Admin) | gumb u [Details.cshtml](Views/Gameweek/Details.cshtml) | [Simulate](Controllers/GameweekController.cs#L58) → [Services/GameweekSimulationService.cs](Services/GameweekSimulationService.cs) `PreviewAsync` (deterministički po seedu, ništa se ne sprema) |
| Pregled simuliranih rezultata prije potvrde | [Views/Gameweek/SimulatePreview.cshtml](Views/Gameweek/SimulatePreview.cshtml) | [Confirm](Controllers/GameweekController.cs#L87) → `ConfirmAsync` — sprema `Fixture` + `MatchPerformance`, računa FPL bodove igračima i timovima (`GameweekTeamScore`) |
| Kreiranje / edit / brisanje kola (Admin) | [Create.cshtml](Views/Gameweek/Create.cshtml) / [Edit.cshtml](Views/Gameweek/Edit.cshtml) / [Delete.cshtml](Views/Gameweek/Delete.cshtml) | [Create](Controllers/GameweekController.cs#L100) / [EditPost](Controllers/GameweekController.cs#L159) / [DeleteConfirmed](Controllers/GameweekController.cs#L202) — brisanje poništava bodove (`DeleteWithReversalAsync`) |

Entiteti: [Models/Gameweek.cs](Models/Gameweek.cs), [Models/Fixture.cs](Models/Fixture.cs), [Models/MatchPerformance.cs](Models/MatchPerformance.cs), [Models/GameweekTeamScore.cs](Models/GameweekTeamScore.cs).

### 3.10 Transferi — `/transferi`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Transfer tržište (tvoj sastav + svi igrači, OUT/IN odabir, budžet) | [Views/Transfer/Index.cshtml](Views/Transfer/Index.cshtml) (sva košarica-logika inline u `@section Scripts`) | [TransferController.Index](Controllers/TransferController.cs#L31) → [TransferMarketViewModel](Models/ViewModels/TransferMarketViewModel.cs) |
| Potvrda transfera | POST forma iz [Index.cshtml](Views/Transfer/Index.cshtml) | [TransferController.Confirm](Controllers/TransferController.cs#L78) — validira sastav (15, 2/5/5/3, max 3 po klubu, budžet), piše `Transfer` zapise, obračunava **besplatne transfere i −4 kaznu** (pretsezona = neograničeno), resetira postavu ako je prodan starter |
| Statistika transfera (trendovi) — `/transferi/statistika` | [Views/Transfer/Stats.cshtml](Views/Transfer/Stats.cshtml) | [TransferController.Stats](Controllers/TransferController.cs#L251) — [TransferRepository](Repositories/TransferRepository.cs) |
| Detalji pojedinog transfera | [Views/Transfer/Details.cshtml](Views/Transfer/Details.cshtml) | [TransferController.Details](Controllers/TransferController.cs#L257) |

Entitet: [Models/Transfer.cs](Models/Transfer.cs) + [Models/TransferDirection.cs](Models/TransferDirection.cs).

### 3.11 Globalna pretraga — `/Search?q=...`

| Što vidiš | Frontend | Backend |
|---|---|---|
| Stranica rezultata grupirana po tipu (igrači, timovi, lige, kola, stranice) | [Views/Search/Index.cshtml](Views/Search/Index.cshtml) | [SearchController.Index](Controllers/SearchController.cs#L36) — poziva `Search()` na sva 4 repozitorija |
| Live dropdown u navbaru | [ff-search.js](wwwroot/js/ff-search.js) (`fetch('/Search/Suggest')`, tipkovnica ↑↓ Enter) | [SearchController.Suggest](Controllers/SearchController.cs#L58) — vraća JSON prijedloge (max 12) |

### 3.12 Greške

| Što vidiš | Frontend | Backend |
|---|---|---|
| Error stranica | [Views/Shared/Error.cshtml](Views/Shared/Error.cshtml) | [HomeController.Error](Controllers/HomeController.cs#L290); registrirano u [Program.cs](Program.cs#L110) (`UseExceptionHandler`) |

---

## 4. Backend slojevi (tko što radi)

| Sloj | Datoteke | Uloga |
|---|---|---|
| **Controlleri (MVC)** | [Controllers/](Controllers/) | Primaju HTTP zahtjev, validiraju, zovu repozitorije/servise, vraćaju View ili JSON |
| **REST API controlleri** | [Controllers/Api/](Controllers/Api/) | Čisti JSON CRUD: `api/player`, `api/fantasyteam`, `api/league`, `api/gameweek`, `api/transfer` (GET/POST/PUT/DELETE). Ne koriste ih stranice — služe kao programski API; pokriveni integracijskim testovima |
| **Repozitoriji** | [Repositories/](Repositories/) — `PlayerRepository`, `FantasyTeamRepository`, `LeagueRepository`, `GameweekRepository`, `TransferRepository` | Enkapsuliraju EF Core upite (`GetAll`, `GetById`, `Search`). `*MockRepository` varijante su in-memory ostaci ranije faze — produkcija koristi EF verzije (registrirane u [Program.cs](Program.cs#L80)) |
| **Servisi** | [Services/GameweekSimulationService.cs](Services/GameweekSimulationService.cs) | Simulacija kola: parovi 20 klubova → 10 utakmica, generiranje učinaka, FPL bodovanje; preview/confirm/reversal |
| | [Services/AiPlayerParser.cs](Services/AiPlayerParser.cs) | OpenAI (GPT-4o mini) — tekstualni opis igrača → popunjena forma; ključ iz user-secrets ili `OPENAI_API_KEY` |
| | [Services/AppUserClaimsPrincipalFactory.cs](Services/AppUserClaimsPrincipalFactory.cs) | Dodaje `FantasyTeamId` claim u login cookie (na njemu radi `RequireFantasyTeamFilter`) |
| **DAL (baza)** | [DAL/FantasyFootballDbContext.cs](DAL/FantasyFootballDbContext.cs) | EF Core DbContext (IdentityDbContext) — svi DbSet-ovi i konfiguracija relacija |
| | [DAL/DbSeeder.cs](DAL/DbSeeder.cs) | Početni podaci pri startu: role `Admin`/`User`, korisnici, igrači, lige… |
| | [Migrations/](Migrations/) | EF migracije — automatski se primjenjuju pri startu ([Program.cs](Program.cs#L93)) |
| **Filteri** | [Filters/RequireFantasyTeamFilter.cs](Filters/RequireFantasyTeamFilter.cs) | Globalno: prijavljen korisnik bez tima → redirect na Build (preskače API, Account, Admine) |
| **Entiteti** | [Models/](Models/) — `Player`, `FantasyTeam`, `League`, `Gameweek`, `Fixture`, `MatchPerformance`, `GameweekTeamScore`, `Transfer`, `AppUser`, `Standing` | Tablice u bazi (EF Core) |
| **ViewModeli** | [Models/ViewModels/](Models/ViewModels/) | Oblik podataka točno za pojedini ekran/formu (nikad se ne spremaju u bazu) |
| **DTO-i** | [Models/DTO/](Models/DTO/) | JSON oblici za REST API controllere |

---

## 5. Frontend building-blocks (dijeljeni dijelovi)

| Komponenta | View (HTML) | JavaScript | Backend endpoint |
|---|---|---|---|
| Autocomplete polje | [_Autocomplete.cshtml](Views/Shared/_Autocomplete.cshtml) | [ff-autocomplete.js](wwwroot/js/ff-autocomplete.js) | konfigurabilan (npr. `/Player/Clubs`) |
| Date picker | [_DatePicker.cshtml](Views/Shared/_DatePicker.cshtml) | [ff-datepicker.js](wwwroot/js/ff-datepicker.js) | — |
| Globalni search dropdown | markup u [_Layout.cshtml](Views/Shared/_Layout.cshtml) | [ff-search.js](wwwroot/js/ff-search.js) | `/Search/Suggest` |
| Igrač na terenu (dres + bodovi) | [_PitchPlayer.cshtml](Views/Shared/_PitchPlayer.cshtml), [_TotwPlayer.cshtml](Views/Shared/_TotwPlayer.cshtml) | — | — |
| Kartica igrača u sastavu | [_SquadPlayer.cshtml](Views/Shared/_SquadPlayer.cshtml) | — | — |
| Dres tima (boje) | [_TeamJersey.cshtml](Views/Shared/_TeamJersey.cshtml) | — | — |
| Navbar ponašanje + TOTW strelice | — | [site.js](wwwroot/js/site.js) | `/Home/TotwPartial` |

Veći ekrani (Build, MyTeam, Transferi, Player Create) imaju svoju logiku **inline u `@section Scripts`** unutar samog viewa, ne u zasebnim .js datotekama.

---

## 6. Cross-cutting (vrijedi svugdje)

| Ponašanje | Gdje je definirano |
|---|---|
| Sve stranice traže prijavu (osim login/register/search) | [Program.cs](Program.cs#L72) — `FallbackPolicy`; iznimke su `[AllowAnonymous]` na akcijama |
| Admin-only funkcije (CRUD igrača, kola, simulacija) | `[Authorize(Roles = DbSeeder.AdminRole)]` na akcijama |
| Bez tima → uvijek završiš na Build | [Filters/RequireFantasyTeamFilter.cs](Filters/RequireFantasyTeamFilter.cs) |
| Logiranje (konzola + `logs/ff-<datum>.log`) | Serilog u [Program.cs](Program.cs#L17) + `appsettings.json`; svaki HTTP zahtjev logira `UseSerilogRequestLogging` |
| Lokalizacija (hr-HR default, en-US) | [Program.cs](Program.cs#L126) |
| Anti-forgery zaštita formi | `[ValidateAntiForgeryToken]` na svim POST akcijama + `<form asp-...>` tag helperi |

---

## 7. Ostali projekti u repou

| Projekt | Uloga |
|---|---|
| [FantasyFootball.Tests/](FantasyFootball.Tests/) | Integracijski testovi REST API-ja (`WebApplicationFactory` + InMemory baza, [TestAuthHandler](FantasyFootball.Tests/TestAuthHandler.cs) lažira login) |
| [FantasyFootball.Mcp/](FantasyFootball.Mcp/) | MCP server — izlaže podatke aplikacije (igrači, timovi, lige, kola) kao alate za AI klijente ([FantasyFootballTools.cs](FantasyFootball.Mcp/FantasyFootballTools.cs)) |
| [e2e/tests/](e2e/tests/) (Playwright) | E2E testovi u browseru (TypeScript) — auth, igrači, lige, kola, transferi, pretraga, full journey |
