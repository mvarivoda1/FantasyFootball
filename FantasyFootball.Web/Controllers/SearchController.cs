using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    // Globalna pretraga: objedinjuje igrače, timove, lige, kola i navigacijske stranice.
    // Dostupna i neprijavljenima kako bi search polje u navbaru radilo svugdje;
    // pojedinačne ciljne stranice i dalje samostalno provjeravaju autorizaciju.
    [AllowAnonymous]
    public class SearchController : Controller
    {
        private readonly PlayerRepository _playerRepo;
        private readonly FantasyTeamRepository _teamRepo;
        private readonly LeagueRepository _leagueRepo;
        private readonly GameweekRepository _gameweekRepo;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            PlayerRepository playerRepo,
            FantasyTeamRepository teamRepo,
            LeagueRepository leagueRepo,
            GameweekRepository gameweekRepo,
            ILogger<SearchController> logger)
        {
            _playerRepo = playerRepo;
            _teamRepo = teamRepo;
            _leagueRepo = leagueRepo;
            _gameweekRepo = gameweekRepo;
            _logger = logger;
        }

        // GET /Search?q=...  → puna stranica rezultata grupirana po tipu.
        [HttpGet]
        public IActionResult Index(string? q)
        {
            var model = new SearchResultsViewModel { Query = q?.Trim() ?? string.Empty };

            if (!string.IsNullOrWhiteSpace(model.Query))
            {
                model.Players = _playerRepo.Search(model.Query, take: 25);
                model.Teams = _teamRepo.Search(model.Query, take: 25);
                model.Leagues = _leagueRepo.Search(model.Query, take: 25);
                model.Gameweeks = _gameweekRepo.Search(model.Query, take: 25);
                model.Pages = SearchPages(model.Query);

                _logger.LogInformation(
                    "Globalna pretraga '{Query}' vratila {Count} rezultata.",
                    model.Query, model.TotalCount);
            }

            return View(model);
        }

        // GET /Search/Suggest?q=...  → JSON za live dropdown (ograničen broj prijedloga).
        [HttpGet]
        public IActionResult Suggest(string? q)
        {
            var term = q?.Trim() ?? string.Empty;
            var suggestions = new List<SearchSuggestion>();

            if (string.IsNullOrWhiteSpace(term))
                return Json(suggestions);

            // Stranice prvo (najbrži put do navigacije).
            suggestions.AddRange(SearchPages(term));

            foreach (var p in _playerRepo.Search(term, take: 5))
            {
                suggestions.Add(new SearchSuggestion
                {
                    Label = p.FullName,
                    Sublabel = $"{p.Club} • {p.Position}",
                    Type = "Igrač",
                    Icon = "bi-person",
                    Url = Url.RouteUrl("PlayerDetails", new { id = p.Id }) ?? "#"
                });
            }

            foreach (var t in _teamRepo.Search(term, take: 5))
            {
                suggestions.Add(new SearchSuggestion
                {
                    Label = t.Name,
                    Sublabel = $"Vlasnik: {t.OwnerName}",
                    Type = "Tim",
                    Icon = "bi-shield",
                    Url = Url.Action("Details", "FantasyTeam", new { id = t.Id }) ?? "#"
                });
            }

            foreach (var l in _leagueRepo.Search(term, take: 5))
            {
                suggestions.Add(new SearchSuggestion
                {
                    Label = l.Name,
                    Sublabel = string.IsNullOrWhiteSpace(l.Season) ? "Liga" : $"Sezona {l.Season}",
                    Type = "Liga",
                    Icon = "bi-diagram-3",
                    Url = Url.RouteUrl("LeagueDetails", new { id = l.Id }) ?? "#"
                });
            }

            foreach (var g in _gameweekRepo.Search(term, take: 5))
            {
                suggestions.Add(new SearchSuggestion
                {
                    Label = $"Kolo {g.WeekNumber}",
                    Sublabel = $"{g.StartDate:dd.MM.} – {g.EndDate:dd.MM.yyyy}",
                    Type = "Kolo",
                    Icon = "bi-calendar-week",
                    Url = Url.RouteUrl("GameweekDetails", new { id = g.Id }) ?? "#"
                });
            }

            return Json(suggestions.Take(12));
        }

        // Statične navigacijske stranice koje se mogu pretraživati (kriterij: pretraga izbornika).
        private List<SearchSuggestion> SearchPages(string term)
        {
            var pages = new List<SearchSuggestion>
            {
                Page("Početna", "Home", "bi-house-door", Url.Action("Index", "Home")),
                Page("Igrači", "Players", "bi-people", Url.RouteUrl("PlayerIndex")),
                Page("Timovi", "Teams", "bi-shield", Url.Action("Index", "FantasyTeam")),
                Page("Lige", "Leagues", "bi-diagram-3", Url.RouteUrl("LeagueIndex")),
                Page("Kola", "Gameweeks", "bi-calendar-week", Url.RouteUrl("GameweekIndex")),
                Page("Transferi", "Transfers", "bi-arrow-left-right", Url.RouteUrl("TransferIndex")),
                Page("Moj tim", "My Team", "bi-shield-fill-check", Url.Action("MyTeam", "FantasyTeam")),
            };

            return pages
                .Where(p =>
                    p.Label.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.Sublabel.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static SearchSuggestion Page(string label, string englishLabel, string icon, string? url) => new()
        {
            Label = label,
            Sublabel = englishLabel,
            Type = "Stranica",
            Icon = icon,
            Url = url ?? "#"
        };
    }
}
