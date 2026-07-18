# Sitemap — FantasyFootball

Popis svih URL-ova u aplikaciji, pripadajućih controller-a / akcija i view datoteka.
Web UI živi u projektu `FantasyFootball.Web`, a REST API u `FantasyFootball.Api`
(vlastiti host, vidi [REST API](#rest-api--fantasyfootballapi) na dnu).
Routing je kombinacija **konvencijskog** usmjeravanja (definirano u
[FantasyFootball.Web/Program.cs](FantasyFootball.Web/Program.cs)) i **atributnog**
usmjeravanja (`[Route]` anotacije na akcijama).

## Konfiguracija routinga i autorizacije

**Program.cs** definira default konvencijsku rutu:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Akcije s `[Route]` atributom **isključuju** default rutu za tu akciju i dostupne
su samo preko navedenog custom URL-a.

**Globalna autorizacija**: `FallbackPolicy` zahtijeva prijavu za **sve** rute
osim onih označenih `[AllowAnonymous]` (login/registracija, globalna pretraga,
error stranica). Admin-only akcije koriste `[Authorize(Roles = DbSeeder.AdminRole)]`
(rola `"Admin"`). U tablicama niže: **anon** = bez prijave, **prijava** = bilo
koji prijavljeni korisnik, **Admin** = samo admin rola.

## Stranice (GET)

| URL | Controller | Akcija | View | Pristup | Napomena |
| --- | --- | --- | --- | --- | --- |
| `/` (i `/Home/Index`) | [HomeController](FantasyFootball.Web/Controllers/HomeController.cs) | `Index` | [Views/Home/Index.cshtml](FantasyFootball.Web/Views/Home/Index.cshtml) | prijava | Dashboard — TOTW widget, `?gw=N` bira kolo |
| `/Home/Error` | HomeController | `Error` | [Views/Shared/Error.cshtml](FantasyFootball.Web/Views/Shared/Error.cshtml) | anon | Globalni error handler |
| `/Account/Login` | [AccountController](FantasyFootball.Web/Controllers/AccountController.cs) | `Login` | [Views/Account/Login.cshtml](FantasyFootball.Web/Views/Account/Login.cshtml) | anon | Podržava `?returnUrl=` |
| `/Account/Register` | AccountController | `Register` | [Views/Account/Register.cshtml](FantasyFootball.Web/Views/Account/Register.cshtml) | anon | |
| `/Account/AccessDenied` | AccountController | `AccessDenied` | [Views/Account/AccessDenied.cshtml](FantasyFootball.Web/Views/Account/AccessDenied.cshtml) | anon | |
| `/Account/ExternalLoginCallback` | AccountController | `ExternalLoginCallback` | — (redirect ili [ExternalLoginConfirmation.cshtml](FantasyFootball.Web/Views/Account/ExternalLoginConfirmation.cshtml)) | anon | Povratni URL Google logina |
| `/igraci` | [PlayerController](FantasyFootball.Web/Controllers/PlayerController.cs) | `Index` | [Views/Player/Index.cshtml](FantasyFootball.Web/Views/Player/Index.cshtml) | prijava | **Custom route** — `[Route("igraci")]` |
| `/igrac/{id:int}` | PlayerController | `Details` | [Views/Player/Details.cshtml](FantasyFootball.Web/Views/Player/Details.cshtml) | prijava | **Custom route** — detalji igrača |
| `/Player/Create` | PlayerController | `Create` | [Views/Player/Create.cshtml](FantasyFootball.Web/Views/Player/Create.cshtml) | Admin | Forma ima i AI unos (vidi `AiParse` niže) |
| `/Player/Edit/{id}` | PlayerController | `Edit` | [Views/Player/Edit.cshtml](FantasyFootball.Web/Views/Player/Edit.cshtml) | Admin | |
| `/Player/Delete/{id}` | PlayerController | `Delete` | [Views/Player/Delete.cshtml](FantasyFootball.Web/Views/Player/Delete.cshtml) | Admin | Soft delete (igrač ostaje u timovima) |
| `/lige` | [LeagueController](FantasyFootball.Web/Controllers/LeagueController.cs) | `Index` | [Views/League/Index.cshtml](FantasyFootball.Web/Views/League/Index.cshtml) | prijava | **Custom route** |
| `/liga/{id:int}` | LeagueController | `Details` | [Views/League/Details.cshtml](FantasyFootball.Web/Views/League/Details.cshtml) | prijava | **Custom route** |
| `/League/Create` | LeagueController | `Create` | [Views/League/Create.cshtml](FantasyFootball.Web/Views/League/Create.cshtml) | prijava | |
| `/League/Created/{id}` | LeagueController | `Created` | [Views/League/Created.cshtml](FantasyFootball.Web/Views/League/Created.cshtml) | prijava | Potvrda s join šifrom lige |
| `/League/Join` | LeagueController | `Join` | [Views/League/Join.cshtml](FantasyFootball.Web/Views/League/Join.cshtml) | prijava | Pridruživanje preko 6-znakovne šifre |
| `/League/Edit/{id}` | LeagueController | `Edit` | [Views/League/Edit.cshtml](FantasyFootball.Web/Views/League/Edit.cshtml) | prijava | |
| `/League/Delete/{id}` | LeagueController | `Delete` | [Views/League/Delete.cshtml](FantasyFootball.Web/Views/League/Delete.cshtml) | prijava | |
| `/transferi` | [TransferController](FantasyFootball.Web/Controllers/TransferController.cs) | `Index` | [Views/Transfer/Index.cshtml](FantasyFootball.Web/Views/Transfer/Index.cshtml) | prijava | **Custom route** — transfer tržište (teren + kupnja/prodaja) |
| `/transferi/statistika` | TransferController | `Stats` | [Views/Transfer/Stats.cshtml](FantasyFootball.Web/Views/Transfer/Stats.cshtml) | prijava | **Custom route** — statistika transfera |
| `/Transfer/Details/{id}` | TransferController | `Details` | [Views/Transfer/Details.cshtml](FantasyFootball.Web/Views/Transfer/Details.cshtml) | prijava | Default konvencijska ruta |
| `/kola` | [GameweekController](FantasyFootball.Web/Controllers/GameweekController.cs) | `Index` | [Views/Gameweek/Index.cshtml](FantasyFootball.Web/Views/Gameweek/Index.cshtml) | prijava | **Custom route** |
| `/kolo/{id:int}` | GameweekController | `Details` | [Views/Gameweek/Details.cshtml](FantasyFootball.Web/Views/Gameweek/Details.cshtml) | prijava | **Custom route** — rezultati, simulacija (admin gumbi) |
| `/Gameweek/Create` | GameweekController | `Create` | [Views/Gameweek/Create.cshtml](FantasyFootball.Web/Views/Gameweek/Create.cshtml) | Admin | |
| `/Gameweek/Edit/{id}` | GameweekController | `Edit` | [Views/Gameweek/Edit.cshtml](FantasyFootball.Web/Views/Gameweek/Edit.cshtml) | Admin | |
| `/Gameweek/Delete/{id}` | GameweekController | `Delete` | [Views/Gameweek/Delete.cshtml](FantasyFootball.Web/Views/Gameweek/Delete.cshtml) | Admin | Poništava i rezultate kola |
| `/FantasyTeam` | [FantasyTeamController](FantasyFootball.Web/Controllers/FantasyTeamController.cs) | `Index` | [Views/FantasyTeam/Index.cshtml](FantasyFootball.Web/Views/FantasyTeam/Index.cshtml) | prijava | Default konvencijska ruta |
| `/FantasyTeam/Details/{id}` | FantasyTeamController | `Details` | [Views/FantasyTeam/Details.cshtml](FantasyFootball.Web/Views/FantasyTeam/Details.cshtml) | prijava | |
| `/FantasyTeam/Build` | FantasyTeamController | `Build` | [Views/FantasyTeam/Build.cshtml](FantasyFootball.Web/Views/FantasyTeam/Build.cshtml) | prijava | Kreiranje vlastite momčadi |
| `/FantasyTeam/MyTeam` | FantasyTeamController | `MyTeam` | [Views/FantasyTeam/MyTeam.cshtml](FantasyFootball.Web/Views/FantasyTeam/MyTeam.cshtml) | prijava | "Moja momčad" — postava; `?gw=N` slider po kolima |
| `/FantasyTeam/Edit/{id}` | FantasyTeamController | `Edit` | [Views/FantasyTeam/Edit.cshtml](FantasyFootball.Web/Views/FantasyTeam/Edit.cshtml) | prijava | |
| `/FantasyTeam/Delete/{id}` | FantasyTeamController | `Delete` | [Views/FantasyTeam/Delete.cshtml](FantasyFootball.Web/Views/FantasyTeam/Delete.cshtml) | prijava | |
| `/Search?q=...` | [SearchController](FantasyFootball.Web/Controllers/SearchController.cs) | `Index` | [Views/Search/Index.cshtml](FantasyFootball.Web/Views/Search/Index.cshtml) | anon | Globalna pretraga (igrači, timovi, lige, kola, stranice) |

## POST akcije

| URL | Controller.Akcija | Pristup | Napomena |
| --- | --- | --- | --- |
| `/Account/Login` | `AccountController.Login` | anon | |
| `/Account/Register` | `AccountController.Register` | anon | |
| `/Account/Logout` | `AccountController.Logout` | prijava | |
| `/Account/ExternalLogin` | `AccountController.ExternalLogin` | anon | Pokreće Google login |
| `/Account/ExternalLoginConfirmation` | `AccountController.ExternalLoginConfirmation` | anon | Dovršetak registracije nakon Google logina |
| `/FantasyTeam/Build` | `FantasyTeamController.Build` | prijava | Spremanje nove momčadi |
| `/FantasyTeam/SaveLineup` | `FantasyTeamController.SaveLineup` | prijava | Spremanje startnih 11 (`teamId`, `starterIds`) |
| `/FantasyTeam/Edit/{id}` | `FantasyTeamController.EditPost` | prijava | |
| `/FantasyTeam/Delete/{id}` | `FantasyTeamController.DeleteConfirmed` | prijava | |
| `/FantasyTeam/UploadLogo` | `FantasyTeamController.UploadLogo` | prijava | Upload loga tima (Dropzone) |
| `/FantasyTeam/RemoveLogo` | `FantasyTeamController.RemoveLogo` | prijava | |
| `/League/Create` | `LeagueController.Create` | prijava | |
| `/League/Join` | `LeagueController.Join` | prijava | |
| `/League/Edit/{id}` | `LeagueController.EditPost` | prijava | |
| `/League/Delete/{id}` | `LeagueController.DeleteConfirmed` | prijava | |
| `/Player/Create` | `PlayerController.Create` | Admin | |
| `/Player/AiParse` | `PlayerController.AiParse` | Admin | AI unos — prirodnojezični opis → prijedlog forme (JSON); graceful fallback ako AI nije dostupan |
| `/Player/Edit/{id}` | `PlayerController.EditPost` | Admin | |
| `/Player/Delete/{id}` | `PlayerController.DeleteConfirmed` | Admin | Soft delete |
| `/Gameweek/Create` | `GameweekController.Create` | Admin | |
| `/Gameweek/Edit/{id}` | `GameweekController.EditPost` | Admin | |
| `/Gameweek/Delete/{id}` | `GameweekController.DeleteConfirmed` | Admin | |
| `/Gameweek/Simulate/{id}` | `GameweekController.Simulate` | Admin | Vraća [SimulatePreview.cshtml](FantasyFootball.Web/Views/Gameweek/SimulatePreview.cshtml) — pregled rezultata (po seedu) prije potvrde |
| `/Gameweek/Confirm/{id}` | `GameweekController.Confirm` | Admin | Potvrđuje simulaciju (`seed`) — sprema učinke i bodove |
| `/Transfer/Confirm` | `TransferController.Confirm` | prijava | Batch potvrda transfera (`outIds`/`inIds`), redirect na Index |

## AJAX / JSON endpointi

| URL | Controller.Akcija | Pristup | Vraća |
| --- | --- | --- | --- |
| `/Home/TotwPartial?gw=N` | `HomeController.TotwPartial` | prijava | HTML partial — [_TotwContent.cshtml](FantasyFootball.Web/Views/Home/_TotwContent.cshtml) (TOTW widget) |
| `/Search/Suggest?q=` | `SearchController.Suggest` | anon | JSON prijedlozi za navbar autocomplete |
| `/Player/Search?q=&position=&club=` | `PlayerController.Search` | prijava | JSON popis igrača (autocomplete/filteri) |
| `/Player/Clubs?q=` | `PlayerController.Clubs` | prijava | JSON popis klubova |
| `/League/Search?q=` | `LeagueController.Search` | prijava | JSON popis liga |
| `/Gameweek/Search?q=` | `GameweekController.Search` | prijava | JSON popis kola |

## Shared view-ovi

Svi view-ovi koriste [_Layout.cshtml](FantasyFootball.Web/Views/Shared/_Layout.cshtml) kao glavni layout.

| Datoteka | Namjena |
| --- | --- |
| [Views/Shared/_Layout.cshtml](FantasyFootball.Web/Views/Shared/_Layout.cshtml) | Glavni layout (navbar sa search poljem, footer, skripte) |
| [Views/Shared/_Autocomplete.cshtml](FantasyFootball.Web/Views/Shared/_Autocomplete.cshtml) | Partial — autocomplete input polje |
| [Views/Shared/_DatePicker.cshtml](FantasyFootball.Web/Views/Shared/_DatePicker.cshtml) | Partial — date picker polje |
| [Views/Shared/_PitchPlayer.cshtml](FantasyFootball.Web/Views/Shared/_PitchPlayer.cshtml) | Partial — igrač na terenu (transfer tržište) |
| [Views/Shared/_SquadPlayer.cshtml](FantasyFootball.Web/Views/Shared/_SquadPlayer.cshtml) | Partial — prikaz igrača u formaciji tima |
| [Views/Shared/_TeamJersey.cshtml](FantasyFootball.Web/Views/Shared/_TeamJersey.cshtml) | Partial — vizualni prikaz dresa/tima |
| [Views/Shared/_TotwPlayer.cshtml](FantasyFootball.Web/Views/Shared/_TotwPlayer.cshtml) | Partial — igrač u "Team of the Week" widgetu |
| [Views/Shared/_ValidationScriptsPartial.cshtml](FantasyFootball.Web/Views/Shared/_ValidationScriptsPartial.cshtml) | jQuery validation skripte |
| [Views/Shared/Error.cshtml](FantasyFootball.Web/Views/Shared/Error.cshtml) | Error page |

## Custom routing — popis

Sljedeće akcije imaju imenovane `[Route]` atribute koji isključuju default konvenciju:

| Akcija | Route template | Named route | Metoda |
| --- | --- | --- | --- |
| `PlayerController.Index` | `igraci` | `PlayerIndex` | GET |
| `PlayerController.Details` | `igrac/{id:int}` | `PlayerDetails` | GET |
| `LeagueController.Index` | `lige` | `LeagueIndex` | GET |
| `LeagueController.Details` | `liga/{id:int}` | `LeagueDetails` | GET |
| `TransferController.Index` | `transferi` | `TransferIndex` | GET |
| `TransferController.Stats` | `transferi/statistika` | `TransferStats` | GET |
| `GameweekController.Index` | `kola` | `GameweekIndex` | GET |
| `GameweekController.Details` | `kolo/{id:int}` | `GameweekDetails` | GET |

Uz njih, JSON search akcije (`Player/Search`, `Player/Clubs`, `League/Search`,
`Gameweek/Search`) imaju neimenovane `[Route]` atribute s istim URL-om kao
default konvencija — fiksirane su da ne ovise o konvencijskoj ruti.

Constraint `{id:int}` osigurava da ruta odgovara samo kad je `id` cijeli broj —
inače ASP.NET vraća 404 umjesto da pokuša parsirati string kao int.

## REST API — FantasyFootball.Api

Zaseban host ([FantasyFootball.Api/Program.cs](FantasyFootball.Api/Program.cs),
lokalno `http://localhost:5264`). Sve rute su atributne (`[ApiController]`),
rade s DTO objektima iz `FantasyFootball.Core/Models/DTO/`; koristi ga i MCP
server (`FantasyFootball.Mcp`). Svaki resurs ima istih 5 operacija:

| Metoda | URL | Opis |
| --- | --- | --- |
| GET | `/api/{resurs}?q=` | Popis (opcionalni tekstualni filter `q`) |
| GET | `/api/{resurs}/{id}` | Jedan zapis |
| POST | `/api/{resurs}` | Kreiranje |
| PUT | `/api/{resurs}/{id}` | Izmjena |
| DELETE | `/api/{resurs}/{id}` | Brisanje |

Resursi: `api/player` ([PlayerApiController](FantasyFootball.Api/Controllers/PlayerApiController.cs)),
`api/fantasyteam` ([FantasyTeamApiController](FantasyFootball.Api/Controllers/FantasyTeamApiController.cs)),
`api/league` ([LeagueApiController](FantasyFootball.Api/Controllers/LeagueApiController.cs)),
`api/gameweek` ([GameweekApiController](FantasyFootball.Api/Controllers/GameweekApiController.cs)),
`api/transfer` ([TransferApiController](FantasyFootball.Api/Controllers/TransferApiController.cs)).

Uz njih, `GET /health` je health-check endpoint dostupan bez prijave.
