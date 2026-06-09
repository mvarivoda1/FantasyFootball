# Sitemap — FantasyFootball

Popis svih URL-ova u aplikaciji, pripadajućih controller-a / akcija i view datoteka.
Routing je kombinacija **konvencijskog** usmjeravanja (definirano u [Program.cs](Program.cs))
i **atributnog** usmjeravanja (`[Route]` anotacije na akcijama).

## Konfiguracija routinga

**Program.cs** definira default konvencijsku rutu:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Akcije s `[Route]` atributom **isključuju** default rutu za tu akciju i dostupne
su samo preko navedenog custom URL-a.

## Stranice (GET)

| URL | Controller | Akcija | View | Napomena |
| --- | --- | --- | --- | --- |
| `/` | [HomeController](Controllers/HomeController.cs) | `Index` | [Views/Home/Index.cshtml](Views/Home/Index.cshtml) | Dashboard — koristi `_TotwContent`, `_TotwPlayer`, `_TeamJersey`, `_SquadPlayer` partial view-ove |
| `/Home/Index` | HomeController | `Index` | Views/Home/Index.cshtml | Default konvencijska ruta |
| `/Home/TotwPartial?gw=N` | HomeController | `TotwPartial` | [Views/Home/_TotwContent.cshtml](Views/Home/_TotwContent.cshtml) | AJAX endpoint — vraća samo TOTW widget |
| `/igraci` | [PlayerController](Controllers/PlayerController.cs) | `Index` | [Views/Player/Index.cshtml](Views/Player/Index.cshtml) | **Custom route** — `[Route("igraci")]` |
| `/igrac/{id:int}` | PlayerController | `Details` | [Views/Player/Details.cshtml](Views/Player/Details.cshtml) | **Custom route** — detalji igrača |
| `/lige` | [LeagueController](Controllers/LeagueController.cs) | `Index` | [Views/League/Index.cshtml](Views/League/Index.cshtml) | **Custom route** |
| `/liga/{id:int}` | LeagueController | `Details` | [Views/League/Details.cshtml](Views/League/Details.cshtml) | **Custom route** |
| `/transferi` | [TransferController](Controllers/TransferController.cs) | `Index` | [Views/Transfer/Index.cshtml](Views/Transfer/Index.cshtml) | **Custom route** — transfer tržište (teren + kupnja/prodaja) |
| `/Transfer/Confirm` | TransferController | `Confirm` | — (redirect na `Index`) | POST — batch potvrda transfera (`outIds`/`inIds`) |
| `/transferi/statistika` | TransferController | `Stats` | [Views/Transfer/Stats.cshtml](Views/Transfer/Stats.cshtml) | **Custom route** — statistika transfera (kasnije admin-only) |
| `/Transfer/Details/{id}` | TransferController | `Details` | [Views/Transfer/Details.cshtml](Views/Transfer/Details.cshtml) | Default konvencijska ruta |
| `/kola` | [GameweekController](Controllers/GameweekController.cs) | `Index` | [Views/Gameweek/Index.cshtml](Views/Gameweek/Index.cshtml) | **Custom route** |
| `/kolo/{id:int}` | GameweekController | `Details` | [Views/Gameweek/Details.cshtml](Views/Gameweek/Details.cshtml) | **Custom route** |
| `/FantasyTeam` | [FantasyTeamController](Controllers/FantasyTeamController.cs) | `Index` | [Views/FantasyTeam/Index.cshtml](Views/FantasyTeam/Index.cshtml) | Default konvencijska ruta |
| `/FantasyTeam/Details/{id}` | FantasyTeamController | `Details` | [Views/FantasyTeam/Details.cshtml](Views/FantasyTeam/Details.cshtml) | Default konvencijska ruta |
| `/Home/Error` | HomeController | `Error` | [Views/Shared/Error.cshtml](Views/Shared/Error.cshtml) | Globalni error handler |

## Shared view-ovi

Svi view-ovi koriste [_Layout.cshtml](Views/Shared/_Layout.cshtml) kao glavni layout.

| Datoteka | Namjena |
| --- | --- |
| [Views/Shared/_Layout.cshtml](Views/Shared/_Layout.cshtml) | Glavni layout (navbar, footer, skripte) |
| [Views/Shared/_SquadPlayer.cshtml](Views/Shared/_SquadPlayer.cshtml) | Partial — prikaz igrača u formaciji tima |
| [Views/Shared/_TeamJersey.cshtml](Views/Shared/_TeamJersey.cshtml) | Partial — vizualni prikaz dresa/tima |
| [Views/Shared/_TotwPlayer.cshtml](Views/Shared/_TotwPlayer.cshtml) | Partial — igrač u "Team of the Week" widgetu |
| [Views/Shared/_ValidationScriptsPartial.cshtml](Views/Shared/_ValidationScriptsPartial.cshtml) | jQuery validation skripte |
| [Views/Shared/Error.cshtml](Views/Shared/Error.cshtml) | Error page |

## Custom routing — popis

Sljedeće akcije imaju `[Route]` atribute koje isključuju default konvenciju:

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

Constraint `{id:int}` osigurava da ruta odgovara samo kad je `id` cijeli broj —
inače ASP.NET vraća 404 umjesto da pokuša parsirati string kao int.
