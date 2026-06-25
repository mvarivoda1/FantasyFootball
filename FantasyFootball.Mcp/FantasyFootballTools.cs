using System.ComponentModel;
using ModelContextProtocol.Server;

namespace FantasyFootball.Mcp;

// Read-only MCP alati nad FantasyFootball REST API-jem. Web aplikacija mora biti
// pokrenuta (anonimni GET endpointi /api/...). Svaki alat vraća JSON odgovor API-ja.
[McpServerToolType]
public static class FantasyFootballTools
{
    [McpServerTool(Name = "list_players")]
    [Description("Vrati listu svih igrača (JSON). Web aplikacija mora biti pokrenuta.")]
    public static Task<string> ListPlayers(IHttpClientFactory httpFactory)
        => GetAsync(httpFactory, "/api/player");

    [McpServerTool(Name = "search_players")]
    [Description("Pretraži igrače po imenu, klubu ili nacionalnosti. Parametar 'query' je pojam pretrage.")]
    public static Task<string> SearchPlayers(
        IHttpClientFactory httpFactory,
        [Description("Pojam pretrage (ime, prezime, klub ili nacionalnost).")] string query)
        => GetAsync(httpFactory, $"/api/player?q={Uri.EscapeDataString(query ?? string.Empty)}");

    [McpServerTool(Name = "get_player")]
    [Description("Dohvati jednog igrača po ID-u (JSON).")]
    public static Task<string> GetPlayer(
        IHttpClientFactory httpFactory,
        [Description("ID igrača.")] int id)
        => GetAsync(httpFactory, $"/api/player/{id}");

    [McpServerTool(Name = "list_leagues")]
    [Description("Vrati listu svih liga (JSON).")]
    public static Task<string> ListLeagues(IHttpClientFactory httpFactory)
        => GetAsync(httpFactory, "/api/league");

    [McpServerTool(Name = "list_teams")]
    [Description("Vrati listu svih fantasy timova (JSON).")]
    public static Task<string> ListTeams(IHttpClientFactory httpFactory)
        => GetAsync(httpFactory, "/api/fantasyteam");

    [McpServerTool(Name = "list_gameweeks")]
    [Description("Vrati listu svih kola (gameweeks) (JSON).")]
    public static Task<string> ListGameweeks(IHttpClientFactory httpFactory)
        => GetAsync(httpFactory, "/api/gameweek");

    private static async Task<string> GetAsync(IHttpClientFactory httpFactory, string path)
    {
        var client = httpFactory.CreateClient("ff");
        try
        {
            var response = await client.GetAsync(path);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return $"Greška {(int)response.StatusCode}: {body}";
            return body;
        }
        catch (Exception ex)
        {
            return $"Greška pri pozivu API-ja ({path}): {ex.Message}. " +
                   "Provjerite je li FantasyFootball aplikacija pokrenuta i je li FF_BASE_URL ispravan.";
        }
    }
}
