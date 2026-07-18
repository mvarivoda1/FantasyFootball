# Split na Web + API projekte + GitLab CI/CD deploy na VM (Docker)

## Context

Aplikacija je danas jedan ASP.NET Core MVC projekt (`FantasyFootball.csproj`, net9.0) koji sadrži i Razor UI (45 viewova, 8 MVC controllera) i REST API (5 controllera u `Controllers/Api/`). Cilj:

1. **Razdvojiti** solution na zasebno deployabilni Web (Razor UI) i Api (REST) projekt — bez rewrite-a UI-ja u SPA (potvrđeno: opcija "Split na Web + API (.NET)").
2. **Automatizirati build i deploy** kroz GitLab CI: runner na Ubuntu VM-u, aplikacija živi u Docker kontejnerima na istom VM-u.
3. Pipeline: **build → test → deploy → e2e** (xUnit + Playwright).

Zatečeno stanje bitno za plan:
- `Dockerfile` + `docker-compose.yml` (web + SQL Server) **već postoje**; Dockerfile koristi .NET **9** image-e i csproj cilja **net9.0** → verzije se poklapaju, novi Dockerfile-i rade se po istom uzoru.
- Migracije + seed se automatski izvršavaju na startu (`Program.cs`: `Database.Migrate()` + `DbSeeder`).
- e2e testovi (`e2e/`, Playwright): prijava kroz UI (`auth.setup.ts`, admin marko@gmail.com), a `api.spec.ts` gađa `/api/*` na **istom baseURL-u** kao UI → web i api u produkciji moraju dijeliti origin (reverse proxy) i auth cookie.
- Svi xUnit testovi u `FantasyFootball.Tests/` su **API integracijski testovi** (`WebApplicationFactory<Program>` + InMemory DB + TestAuthHandler) → nakon splita referenciraju Api projekt.
- `FantasyFootball.Mcp/` je standalone (bez project reference, zove API preko HTTP-a) — kod se ne dira, samo se ažurira base URL za lokalni dev.
- Repo je na GitHubu; GitLab instanca još nije odabrana → plan pretpostavlja **gitlab.com** (besplatni Container Registry; vlastiti runner ne troši CI minute), uz napomenu za self-hosted.
- Postoje necommitane izmjene (datepicker/gameweek) — **prvo ih committati na main**, split raditi na feature branchu.

## Ciljna arhitektura

```
GitLab (gitlab.com) ── push ──> pipeline (runner na VM-u, docker executor)
  stages: build → test → package (docker images → registry) → deploy → e2e

Ubuntu VM (Docker):
┌────────────────────────────────────────────────────┐
│ ff_proxy  (nginx)            :80  → jedini ulaz    │
│   ├── /api/* → ff_api:8080                         │
│   └── /*     → ff_web:8080                         │
│ ff_web    (FantasyFootball.Web — Razor UI)         │
│ ff_api    (FantasyFootball.Api — REST)             │
│ ff_database (SQL Server 2025)                      │
│ volumes: sqldata, dpkeys (dijeljeni), uploads      │
└────────────────────────────────────────────────────┘
```

Jedan git repo (monorepo) — Core library se dijeli između Web i Api, pa bi dva repoa zahtijevala NuGet feed. "Dva projekta" se ostvaruje kao dva zasebno buildana/deployana projekta s odvojenim image-ima i pipeline jobovima (path-filtrirano po potrebi).

---

## Faza 1 — Restrukturiranje soluciona (Core + Web + Api)

Novi raspored (siblings postojećim `FantasyFootball.Tests/` i `FantasyFootball.Mcp/`; **namespace-ovi se NE mijenjaju** — samo `git mv` datoteka i novi .csproj-evi, čime je diff minimalan):

| Sada | Ide u |
|---|---|
| `Models/*.cs` (entiteti, `Position`, `Standing`…) + `Models/DTO/` | `FantasyFootball.Core/Models/` |
| `DAL/` (DbContext, DbSeeder), `Migrations/`, `Repositories/` | `FantasyFootball.Core/…` |
| `Services/GameweekSimulationService.cs`, `Services/AiPlayerParser.cs` | `FantasyFootball.Core/Services/` |
| `Controllers/*.cs` (MVC), `Views/`, `wwwroot/`, `Filters/`, `Models/ViewModels/` + `ErrorViewModel.cs`, `Services/AppUserClaimsPrincipalFactory.cs`, `Program.cs`, `appsettings*.json`, `Properties/launchSettings.json` | `FantasyFootball.Web/…` |
| `Controllers/Api/*.cs` | `FantasyFootball.Api/Controllers/` |
| root `FantasyFootball.csproj` | briše se (nestaju i `Compile Remove` hackovi) |

**Projekti:**
- `FantasyFootball.Core` (classlib, net9.0): paketi `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `OpenAI`. Migracije ostaju uz DbContext (default MigrationsAssembly = Core ✓).
- `FantasyFootball.Web` (Sdk.Web): postojeći `Program.cs` gotovo netaknut (Identity, Google, lokalizacija, Serilog, filter, repozitoriji, AiPlayerParser). `launchSettings.json` ostaje isti → **lokalni dev i FF_BASE_URL=https://localhost:7031 rade kao i do sada**. Paketi: Google auth, Serilog.*, EF Design/Tools.
- `FantasyFootball.Api` (Sdk.Web, novi `Program.cs` + `public partial class Program`): Serilog, DbContext, **isti** `AddIdentity` + `ConfigureApplicationCookie` blok kao Web (cookie se validira, redirect na login ide kroz proxy na Web — ponašanje identično današnjem), isti fallback authorization policy, `AddControllers`, `/health` endpoint (`MapHealthChecks`). Novi `launchSettings.json` s vlastitim portom (npr. http 5264). Bez Google/lokalizacije/static files.
- **Data Protection** (nužno da Api može čitati cookie koji je izdao Web): u oba `Program.cs` — `AddDataProtection().SetApplicationName("FantasyFootball")` + `PersistKeysToFileSystem` kada je zadan `DataProtection__KeysPath` (env u kontejnerima; lokalno oba procesa dijele isti user-profile keys path — provjeriti, po potrebi zadati zajednički lokalni path).
- **Migracije + seed ostaju u OBA** `Program.cs` (guard `IsRelational()` postoji): lokalni dev i dalje radi pokretanjem samo Weba; u composeu ih serijalizira ovisnost `db → api (healthy) → web`.
- `FantasyFootball.Tests.csproj`: ProjectReference → `FantasyFootball.Api` (testovi su API-jevi; `CustomWebApplicationFactory` radi bez izmjena logike).
- `.mcp.json` / Mcp konfiguracija: base URL za lokalni dev preusmjeriti na Api-jev port (API više ne živi na 5263/7031 lokalno).
- Ažurirati `FantasyFootball.slnx`, `CLAUDE.md` (struktura projekta + napomena za `dotnet ef -p FantasyFootball.Core -s FantasyFootball.Web`).

## Faza 2 — Docker (2 image-a + nginx + compose)

- `FantasyFootball.Web/Dockerfile` i `FantasyFootball.Api/Dockerfile`: multi-stage po uzoru na postojeći (**`sdk:9.0` / `aspnet:9.0`**), context = repo root, kopira Core + dotični projekt.
- `deploy/nginx.conf`: `location /api/ → proxy_pass http://api:8080;`, `location / → proxy_pass http://web:8080;` + `X-Forwarded-*` headeri; u Web/Api dodati `UseForwardedHeaders`.
- `docker-compose.yml` (lokalni smoke-test, `up --build`) i `docker-compose.prod.yml` (VM: `image:` iz GitLab registryja + `pull`): servisi `db` (postojeći, healthcheck; port 1433 bind na 127.0.0.1), `api` (healthcheck `/health`, čeka db), `web` (čeka api healthy), `proxy` (nginx, port 80). Volumes: `sqldata`, `dpkeys` (mount u web+api, `DataProtection__KeysPath=/keys`), `uploads` (mount na `FantasyFootball.Web/wwwroot/uploads` — bez toga se uploadi timova gube na redeploy).
- Secrets kroz env (compose ih čita iz okoline/`.env` na VM-u): `MSSQL_SA_PASSWORD`, `ConnectionStrings__FantasyFootballDbContext`, `OPENAI_API_KEY`, opcionalno Google OAuth.

## Faza 3 — GitLab CI (`.gitlab-ci.yml`)

```yaml
stages: [build, test, package, deploy, e2e]
build:    # mcr.microsoft.com/dotnet/sdk:9.0 — dotnet restore + build cijelog slnx
test:     # isti image — dotnet test FantasyFootball.Tests (InMemory → ne treba baza)
package:  # docker:cli — build + push web/api image-a u $CI_REGISTRY, tag $CI_COMMIT_SHORT_SHA + latest (samo main)
deploy:   # docker:cli s compose pluginom — docker compose -f docker-compose.prod.yml pull && up -d (samo main; runner na VM-u → socket = host daemon)
e2e:      # mcr.microsoft.com/playwright:v<verzija-iz-package.json> — cd e2e && npm ci && FF_BASE_URL=http://localhost npx playwright test; artifacts: playwright-report (samo main, nakon deploya)
```

- CI/CD varijable (Settings → CI/CD → Variables, masked): `MSSQL_SA_PASSWORD`, `OPENAI_API_KEY`, (opc.) Google. Registry login ide ugrađenim `$CI_REGISTRY_USER`/`$CI_JOB_TOKEN`.
- Za self-hosted GitLab kasnije: mijenja se samo registry URL / dostupnost registryja (fallback: package+deploy spojiti u jedan job koji builda lokalno na VM-u, bez pusha).

## Faza 4 — Priprema VM-a (izvodi korisnik, dobiva točne komande u DEPLOY.md)

1. Ubuntu Server 24.04 + Docker Engine + compose plugin (službeni apt repo).
2. `gitlab-runner` (apt) + registracija na GitLab projekt: **docker executor**, `volumes = ["/var/run/docker.sock:/var/run/docker.sock"]`, `network_mode = "host"` (da test/e2e jobovi vide `localhost:80`).
3. Repo push na GitLab (`git remote add gitlab …`; GitHub može ostati kao drugi remote).

## Faza 5 — Dokumentacija

- `DEPLOY.md` prepisati: nova arhitektura, VM setup, runner, CI varijable; ispraviti zastarjeli `Anthropic__ApiKey` → `OPENAI_API_KEY` (kod čita `OpenAI:ApiKey` / `OPENAI_API_KEY`, `Services/AiPlayerParser.cs:26`).

## Redoslijed i git

1. Commit postojećih working izmjena na `main` (odvojen, ne miješa se sa splitom).
2. Branch `feature/split-web-api`: Faza 1 → verifikacija → Faza 2 → verifikacija → Faze 3+5 → merge.
3. Faza 4 (VM/GitLab račun) — korisnikova infra; nakon nje prvi pravi pipeline run.

## Verifikacija

1. **Nakon Faze 1 (lokalno):** `dotnet build` slnx; `dotnet test` (svi postojeći testovi zeleni); pokrenuti Web (`https://localhost:7031`) → login + par stranica; pokrenuti Api → `GET /api/player` vraća JSON.
2. **Nakon Faze 2 (lokalno, Docker Desktop):** `docker compose up --build` → na `http://localhost` proći: login (cookie kroz proxy), admin akcija koja gađa API, upload logotipa (uploads volume); `cd e2e && FF_BASE_URL=http://localhost npx playwright test` — kompletan e2e (uklj. `api.spec.ts` koji dokazuje da cookie izdan na Webu vrijedi na Api-ju kroz proxy).
3. **End-to-end (nakon Faze 4):** push na main → pipeline zelen kroz svih 5 stageova → aplikacija dostupna na `http://<VM-IP>` → Playwright report artefakt u GitLabu.

## Rizici / napomene

- **e2e na VM-u mutira produkcijsku bazu** (api.spec CRUD; čisti za sobom, ali uz retry može ostati smeće). Za lab prihvatljivo; alternativa (kasnije): zaseban "staging" compose stack za e2e.
- Google OAuth na VM-u zahtijeva registraciju redirect URI-ja za VM adresu (opcionalno; placeholder kredencijali ne ruše app).
- Nema TLS-a (http na :80) — za lab OK; kasnije se ispred nginxa lako doda certbot/Caddy.
- gitlab.com odluka još visi — plan radi i za self-hosted uz gore opisanu zamjenu registryja.
