# FantasyFootball MCP Server

MCP (Model Context Protocol) server koji izlaže **read-only** alate nad FantasyFootball
REST API-jem, dostupne kroz agentic IDE (Claude Code, Cursor, ...).

## Alati

| Alat | Opis |
|---|---|
| `list_players` | Lista svih igrača (JSON) |
| `search_players` | Pretraga igrača po imenu / klubu / nacionalnosti (`query`) |
| `get_player` | Jedan igrač po `id` |
| `list_leagues` | Lista liga |
| `list_teams` | Lista fantasy timova |
| `list_gameweeks` | Lista kola |

Alati pozivaju anonimne `GET /api/...` endpointe web aplikacije, pa **web aplikacija
mora biti pokrenuta** (`dotnet run` u root projektu, HTTP na `http://localhost:5263`).

## Pokretanje

```bash
# 1. Pokreni web aplikaciju (u root direktoriju)
dotnet run

# 2. Buildaj MCP server (jednom)
dotnet build FantasyFootball.Mcp/FantasyFootball.Mcp.csproj
```

Bazni URL API-ja se može promijeniti preko env varijable `FF_BASE_URL` ili konfiguracije
`FantasyFootball:BaseUrl` (default `http://localhost:5263`).

## Spajanje iz agentic IDE-a

Repozitorij sadrži `.mcp.json` u rootu koji Claude Code automatski učita za ovaj projekt:

```json
{
  "mcpServers": {
    "fantasyfootball": {
      "command": "dotnet",
      "args": ["FantasyFootball.Mcp/bin/Debug/net9.0/FantasyFootball.Mcp.dll"],
      "env": { "FF_BASE_URL": "http://localhost:5263" }
    }
  }
}
```

> Napomena: koristi se već izbuildani `.dll` (a ne `dotnet run`) da build poruke ne bi
> pokvarile stdio JSON-RPC protokol. Nakon izmjena u MCP projektu ponovo buildaj.

Provjera: u IDE-u pozovi npr. `list_players` ili `search_players query="Haaland"`.
