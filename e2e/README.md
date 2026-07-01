# FantasyFootball — Playwright E2E (TypeScript)

Black-box UI **i** REST API testovi koji pokreću pravu aplikaciju u pravom
pregledniku (Chromium). Pišu se u TypeScriptu i pokreću `@playwright/test`
runnerom — s **vizualnim HTML izvještajem**, trace viewerom i snimkama.

Svaki UI test automatski padne na neuhvaćenu JS iznimku (`pageerror`) ili HTTP
status ≥ 400 — to je "ništa se ne ruši" garancija.

## Preduvjeti

1. **Baza** (npr. `docker compose up -d db`) — migracije i seed se izvrše pri startu aplikacije.
2. **Aplikacija pokrenuta** na `http://localhost:5263`:
   ```bash
   ASPNETCORE_URLS=http://localhost:5263 dotnet run --project FantasyFootball.csproj --no-launch-profile
   ```
   Drugi URL: postavi `FF_BASE_URL`.
3. **Node.js 18+**.

Demo račun: **`marko@gmail.com` / `markopass`** — prvi seedani korisnik (ima
fantasy tim i Admin rolu, pa se pokrivaju i admin-only rute i write API-ji).

## Instalacija i pokretanje

```bash
cd e2e
npm install
npm run install:browsers          # jednokratno: preuzme Chromium

FF_BASE_URL=http://localhost:5263 npm test        # svi testovi
npm run report                    # otvori HTML izvještaj (playwright-report/)
```

Korisne varijante:

```bash
npm test -- players.spec.ts       # jedan spec
npm test -- -g "CRUD"             # po nazivu testa
npm run test:headed               # vidljiv preglednik
npm run test:ui                   # interaktivni UI mode
npx playwright show-trace         # trace viewer (za pale testove)
```

## Struktura

| Datoteka | Pokriva |
|----------|---------|
| `playwright.config.ts` | Konfiguracija: HTML reporter, baseURL, `setup` + `chromium` projekt (storageState prijava). |
| `tests/helpers.ts` | Prošireni `test` (auto-fail na `pageerror`), `gotoOk`, `getFirstId`, `getMyTeamId`, `isAdmin`. |
| `tests/auth.setup.ts` | Jednokratna prijava → sprema `playwright/.auth/user.json`. |
| `tests/api.spec.ts` | **Svih 25 REST endpointa** — GET liste + `?q` + 404, i puni CRUD (POST→GET→PUT→DELETE) za igrače, lige, timove, kola i transfere. |
| `tests/smoke-navigation.spec.ts` | Svaka glavna + detaljna stranica, klik kroz navbar. |
| `tests/auth.spec.ts` | Prijava/odjava, registracija, zaštita ruta, AccessDenied (odjavljeno stanje). |
| `tests/players.spec.ts` | Lista, AJAX live-search, filter, locker modal, detalji, 404, admin CRUD forme. |
| `tests/leagues.spec.ts` | Lista + sort, join validacija, puni CRUD kroz UI. |
| `tests/gameweeks.spec.ts` | Lista + pretraga, detalji, admin Create forma. |
| `tests/teams-transfers.spec.ts` | Ranking + row-klik, MyTeam (teren + spremanje sastava), Edit tima, transfer tržište/statistika. |
| `tests/search.spec.ts` | Navbar live-dropdown i puna stranica rezultata. |
| `tests/full-journey.spec.ts` | 10-koračni end-to-end scenarij. |

## API pokrivenost (25/25)

Za svaki od 5 kontrolera (`player`, `league`, `gameweek`, `fantasyteam`,
`transfer`) pokriveni su: `GET` lista, `GET {id}`, `POST`, `PUT`, `DELETE`, plus
`?q` filter i 404 na nepostojeći id. Write testovi su samodostatni CRUD krugovi
pa čiste za sobom.

## Napomene

- Testovi s `test.skip(...)` se preskaču (ne padaju) ako preduvjet ne postoji u
  bazi — npr. ako još nema kreiranih kola.
- `leagues.spec` CRUD i `MyTeam` spremanje sastava mijenjaju podatke, ali su
  idempotentni / čiste za sobom.
- `console.error` (npr. 404 za CDN resurs) se ne tretira kao pad — samo
  neuhvaćena JS iznimka ruši test.
