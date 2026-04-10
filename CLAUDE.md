# FantasyFootball - Claude Code Instrukcije

## Projekt
ASP.NET MVC aplikacija za fantasy football. Tech stack: C#, Razor Views, Bootstrap 5, jQuery.

## UI/UX Pravilo
Kada generiras, modificiras ili pregledavas UI kod (Views, CSS, JavaScript, layout promjene):
- **OBAVEZNO** delegiraj posao `ux-designer` sub-agentu koristeci Agent tool sa `subagent_type: "ux-designer"`
- Ovo ukljucuje: nove Views (.cshtml), promjene u _Layout.cshtml, CSS promjene, dodavanje Bootstrap komponenti, forme, tablice, navigaciju
- UX agent ce osigurati konzistentan dizajn, pristupacnost i responsive ponasanje

## Struktura Projekta
- `Views/` - Razor Views (.cshtml)
- `Views/Shared/_Layout.cshtml` - glavni layout
- `Controllers/` - MVC controlleri
- `Models/` - data modeli
- `wwwroot/css/site.css` - custom stilovi
- `wwwroot/js/site.js` - custom JavaScript
- `.claude/agents/ux-designer.md` - UX sub-agent definicija
