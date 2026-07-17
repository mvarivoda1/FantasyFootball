using FantasyFootball.Models;

namespace FantasyFootball.Models.ViewModels
{
    // Jedna stavka prijedloga u globalnoj pretrazi (koristi se za JSON dropdown i za
    // pretragu izbornika/stranica). Url je već razriješen u kontroleru.
    public class SearchSuggestion
    {
        public string Label { get; set; } = string.Empty;
        public string Sublabel { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;   // npr. "Igrač", "Tim", "Liga", "Kolo", "Stranica"
        public string Icon { get; set; } = string.Empty;    // bootstrap-icons klasa, npr. "bi-people"
        public string Url { get; set; } = string.Empty;
    }

    // Model rezultata globalne pretrage grupiran po tipu entiteta + statične stranice.
    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;

        public List<Player> Players { get; set; } = new();
        public List<FantasyTeam> Teams { get; set; } = new();
        public List<League> Leagues { get; set; } = new();
        public List<Gameweek> Gameweeks { get; set; } = new();
        public List<SearchSuggestion> Pages { get; set; } = new();

        public int TotalCount =>
            Players.Count + Teams.Count + Leagues.Count + Gameweeks.Count + Pages.Count;
    }
}
