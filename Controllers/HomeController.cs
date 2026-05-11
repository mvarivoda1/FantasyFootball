using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FantasyFootball.Controllers
{
    public class HomeController : Controller
    {
        private readonly PlayerRepository _playerRepo;
        private readonly LeagueRepository _leagueRepo;
        private readonly FantasyTeamRepository _teamRepo;
        private readonly GameweekRepository _gameweekRepo;
        private readonly TransferRepository _transferRepo;

        public HomeController(
            PlayerRepository playerRepo,
            LeagueRepository leagueRepo,
            FantasyTeamRepository teamRepo,
            GameweekRepository gameweekRepo,
            TransferRepository transferRepo)
        {
            _playerRepo = playerRepo;
            _leagueRepo = leagueRepo;
            _teamRepo = teamRepo;
            _gameweekRepo = gameweekRepo;
            _transferRepo = transferRepo;
        }

        public IActionResult Index(int? gw = null)
        {
            var vm = new DashboardViewModel();

            // --- Gameweek Status (latest) ---
            var allGameweeks = _gameweekRepo.GetAll().OrderBy(g => g.WeekNumber).ToList();
            var latest = allGameweeks.OrderByDescending(g => g.WeekNumber).FirstOrDefault();
            vm.LatestGameweek = latest;

            // --- TOTW Target Gameweek (prev/next navigation) ---
            var totwGw = gw.HasValue
                ? allGameweeks.FirstOrDefault(g => g.WeekNumber == gw.Value) ?? latest
                : latest;

            if (latest != null)
            {
                vm.MatchesInLatestGw = latest.Performances
                    .GroupBy(p => new { p.MatchDate.Date, p.Opponent })
                    .Count() / 2;
                // Fallback: count unique matchdates if grouping is odd
                if (vm.MatchesInLatestGw == 0)
                {
                    vm.MatchesInLatestGw = latest.Performances
                        .Select(p => p.MatchDate.Date)
                        .Distinct()
                        .Count();
                }

                vm.TotalGoalsInLatestGw = latest.Performances.Sum(p => p.Goals);
                vm.TotalAssistsInLatestGw = latest.Performances.Sum(p => p.Assists);
                vm.TotalCleanSheetsInLatestGw = latest.Performances.Count(p => p.CleanSheet);
            }

            // --- Next Deadline (simulated) ---
            vm.NextGameweekNumber = (latest?.WeekNumber ?? 0) + 1;
            // Pick EndDate of latest + 7 days as fake next deadline
            vm.NextDeadline = latest != null
                ? latest.EndDate.AddDays(7).AddHours(13).AddMinutes(30)
                : DateTime.Now.AddDays(3);

            // --- Transfers ---
            var acceptedTransfers = _transferRepo.GetAll()
                .Where(t => t.Status == TransferStatus.Accepted)
                .ToList();

            vm.TopTransfersIn = acceptedTransfers
                .GroupBy(t => t.Player)
                .Select(g => new TransferWidgetItem
                {
                    Player = g.Key,
                    TransferCount = g.Count(),
                    LatestPrice = g.OrderByDescending(t => t.TransferDate).First().Price
                })
                .OrderByDescending(x => x.TransferCount)
                .ThenByDescending(x => x.LatestPrice)
                .Take(5)
                .ToList();

            var outgoing = acceptedTransfers
                .Where(t => t.FromTeam != null)
                .ToList();

            vm.TopTransfersOut = outgoing
                .GroupBy(t => t.Player)
                .Select(g => new TransferWidgetItem
                {
                    Player = g.Key,
                    TransferCount = g.Count(),
                    LatestPrice = g.OrderByDescending(t => t.TransferDate).First().Price
                })
                .OrderByDescending(x => x.TransferCount)
                .ThenByDescending(x => x.LatestPrice)
                .Take(5)
                .ToList();

            // Fallback: if no outgoing, use top performers as "most transferred" proxy
            if (!vm.TopTransfersOut.Any())
            {
                vm.TopTransfersOut = _transferRepo.GetAll()
                    .Where(t => t.FromTeam != null)
                    .GroupBy(t => t.Player)
                    .Select(g => new TransferWidgetItem
                    {
                        Player = g.Key,
                        TransferCount = g.Count(),
                        LatestPrice = g.OrderByDescending(t => t.TransferDate).First().Price
                    })
                    .OrderByDescending(x => x.TransferCount)
                    .Take(5)
                    .ToList();
            }

            // --- Player of the Week ---
            vm.PlayerOfTheWeek = latest?.Performances
                .OrderByDescending(p => p.PointsEarned)
                .FirstOrDefault();

            // --- Team of the Week (by position) ---
            PopulateTotw(vm, totwGw, allGameweeks);

            // --- Player Availability ---
            if (latest != null)
            {
                var allPlayers = _playerRepo.GetAll();
                var latestPerfByPlayerId = latest.Performances
                    .GroupBy(p => p.Player.Id)
                    .ToDictionary(g => g.Key, g => g.First());

                var availability = new List<PlayerAvailabilityItem>();

                // Pick a curated sample: top-point players for visibility
                foreach (var player in allPlayers.OrderByDescending(p => p.TotalPoints).Take(8))
                {
                    if (latestPerfByPlayerId.TryGetValue(player.Id, out var perf))
                    {
                        if (perf.RedCards > 0)
                        {
                            availability.Add(new PlayerAvailabilityItem
                            {
                                Player = player,
                                Status = "Suspended",
                                StatusClass = "out",
                                ChancePercent = 0
                            });
                        }
                        else if (perf.MinutesPlayed < 60)
                        {
                            availability.Add(new PlayerAvailabilityItem
                            {
                                Player = player,
                                Status = "Doubt",
                                StatusClass = "doubt",
                                ChancePercent = 50
                            });
                        }
                        else
                        {
                            availability.Add(new PlayerAvailabilityItem
                            {
                                Player = player,
                                Status = "Available",
                                StatusClass = "available",
                                ChancePercent = 100
                            });
                        }
                    }
                    else
                    {
                        availability.Add(new PlayerAvailabilityItem
                        {
                            Player = player,
                            Status = "Knock",
                            StatusClass = "knock",
                            ChancePercent = 25
                        });
                    }
                }

                // Sort to show most interesting (doubts/out) first, then available
                vm.PlayerAvailability = availability
                    .OrderBy(a => a.ChancePercent)
                    .Take(6)
                    .ToList();
            }

            // --- Most Valuable Teams (by TotalPoints) ---
            vm.MostValuableTeams = _teamRepo.GetAll()
                .OrderByDescending(t => t.TotalPoints)
                .Take(5)
                .ToList();

            // --- Best Leagues ---
            vm.BestLeagues = _leagueRepo.GetAll()
                .Select(l =>
                {
                    var leagueTeams = _teamRepo.GetAll().Where(t => t.League?.Id == l.Id).ToList();
                    var totalPts = leagueTeams.Sum(t => t.TotalPoints);
                    return new LeagueWidgetItem
                    {
                        League = l,
                        TotalTeams = leagueTeams.Count,
                        TotalPoints = totalPts,
                        AveragePoints = leagueTeams.Count > 0 ? (double)totalPts / leagueTeams.Count : 0
                    };
                })
                .OrderByDescending(x => x.TotalPoints)
                .Take(3)
                .ToList();

            // --- Top Scorers ---
            vm.TopScorers = _playerRepo.GetAll()
                .OrderByDescending(p => p.Goals)
                .ThenByDescending(p => p.Assists)
                .Take(5)
                .ToList();

            return View(vm);
        }

        // AJAX endpoint — returns only the TOTW widget content so navigation arrows
        // don't cause a full page reload (which would scroll the user to the top).
        public IActionResult TotwPartial(int? gw = null)
        {
            var allGameweeks = _gameweekRepo.GetAll().OrderBy(g => g.WeekNumber).ToList();
            var latest = allGameweeks.OrderByDescending(g => g.WeekNumber).FirstOrDefault();
            var totwGw = gw.HasValue
                ? allGameweeks.FirstOrDefault(g => g.WeekNumber == gw.Value) ?? latest
                : latest;

            var vm = new DashboardViewModel();
            PopulateTotw(vm, totwGw, allGameweeks);
            return PartialView("_TotwContent", vm);
        }

        private static void PopulateTotw(DashboardViewModel vm, Gameweek? totwGw, List<Gameweek> allGameweeks)
        {
            // Formation rules: 1 GK + 3-5 DEF + 2-5 MID + 1-3 FWD = 11 players.
            // Pick the valid formation that maximizes total points this gameweek.
            if (totwGw == null) return;

            vm.TotwGameweekNumber = totwGw.WeekNumber;
            vm.TotwPrevGameweek = allGameweeks
                .Where(g => g.WeekNumber < totwGw.WeekNumber)
                .OrderByDescending(g => g.WeekNumber)
                .FirstOrDefault()?.WeekNumber;
            vm.TotwNextGameweek = allGameweeks
                .Where(g => g.WeekNumber > totwGw.WeekNumber)
                .OrderBy(g => g.WeekNumber)
                .FirstOrDefault()?.WeekNumber;

            var perfsByPos = totwGw.Performances
                .GroupBy(p => p.Player.Position)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.PointsEarned).ToList());

            var gkPerfs = perfsByPos.TryGetValue(Position.Goalkeeper, out var gks) ? gks : new List<MatchPerformance>();
            var defPerfs = perfsByPos.TryGetValue(Position.Defender, out var defs) ? defs : new List<MatchPerformance>();
            var midPerfs = perfsByPos.TryGetValue(Position.Midfielder, out var mids) ? mids : new List<MatchPerformance>();
            var fwdPerfs = perfsByPos.TryGetValue(Position.Forward, out var fwds) ? fwds : new List<MatchPerformance>();

            int PrefixPoints(List<MatchPerformance> list, int take)
                => list.Take(take).Sum(p => p.PointsEarned);

            (int d, int m, int f) bestFormation = (0, 0, 0);
            int bestScore = -1;

            for (int d = 3; d <= 5; d++)
            {
                for (int m = 2; m <= 5; m++)
                {
                    int f = 10 - d - m;
                    if (f < 1 || f > 3) continue;
                    if (defPerfs.Count < d || midPerfs.Count < m || fwdPerfs.Count < f) continue;

                    int score = PrefixPoints(defPerfs, d) + PrefixPoints(midPerfs, m) + PrefixPoints(fwdPerfs, f);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestFormation = (d, m, f);
                    }
                }
            }

            if (bestScore < 0)
            {
                bestFormation = (
                    Math.Min(5, Math.Max(0, defPerfs.Count)),
                    Math.Min(5, Math.Max(0, midPerfs.Count)),
                    Math.Min(3, Math.Max(0, fwdPerfs.Count))
                );
            }

            vm.TotwGoalkeepers = gkPerfs.Take(1).ToList();
            vm.TotwDefenders = defPerfs.Take(bestFormation.d).ToList();
            vm.TotwMidfielders = midPerfs.Take(bestFormation.m).ToList();
            vm.TotwForwards = fwdPerfs.Take(bestFormation.f).ToList();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
