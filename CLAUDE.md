# FantasyFootball - Claude Code Instrukcije

## Projekt
ASP.NET MVC aplikacija za fantasy football. Tech stack: C#, Razor Views, Bootstrap 5, jQuery.

## UI/UX Pravilo
Kada generiras, modificiras ili pregledavas UI kod (Views, CSS, JavaScript, layout promjene):
- **OBAVEZNO** delegiraj posao `ux-designer` sub-agentu koristeci Agent tool sa `subagent_type: "ux-designer"`
- Ovo ukljucuje: nove Views (.cshtml), promjene u _Layout.cshtml, CSS promjene, dodavanje Bootstrap komponenti, forme, tablice, navigaciju
- UX agent ce osigurati konzistentan dizajn, pristupacnost i responsive ponasanje

## Struktura Projekta
Solution je podijeljen na tri projekta (namespace-ovi su ostali `FantasyFootball.*` u sva tri):
- `FantasyFootball.Core/` - dijeljena classlib: `Models/` (entiteti + DTO), `DAL/` (DbContext, DbSeeder), `Migrations/`, `Repositories/`, `Services/` (GameweekSimulationService, AiPlayerParser, AppUserClaimsPrincipalFactory)
- `FantasyFootball.Web/` - Razor UI (MVC): `Controllers/`, `Views/`, `wwwroot/`, `Filters/`, `Models/ViewModels/`, `Program.cs` (lokalno: https://localhost:7031 / http://localhost:5263)
- `FantasyFootball.Api/` - REST API: `Controllers/` (api/* rute), vlastiti `Program.cs` s `/health` endpointom (lokalno: http://localhost:5264)
- `FantasyFootball.Tests/` - xUnit API integracijski testovi (referenciraju Api projekt)
- `FantasyFootball.Mcp/` - MCP server (zove Api preko HTTP-a, `FF_BASE_URL`)
- `FantasyFootball.Web/Views/Shared/_Layout.cshtml` - glavni layout
- `FantasyFootball.Web/wwwroot/css/site.css` - custom stilovi
- `FantasyFootball.Web/wwwroot/js/site.js` - custom JavaScript
- `.claude/agents/ux-designer.md` - UX sub-agent definicija

## EF Core migracije
Migracije žive u Core projektu, startup projekt je Web:
```powershell
dotnet ef migrations add <Naziv> -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext
dotnet ef database update -p FantasyFootball.Core -s FantasyFootball.Web --context FantasyFootballDbContext
```
