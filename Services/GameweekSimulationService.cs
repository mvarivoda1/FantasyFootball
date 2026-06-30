using FantasyFootball.DAL;
using FantasyFootball.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Services
{
    // ===== Draft DTO-i (in-memory rezultat simulacije prije spremanja) =====

    public class SimulationDraft
    {
        public int Seed { get; set; }
        public List<FixtureDraft> Fixtures { get; set; } = new();
        public List<PerformanceDraft> Performances { get; set; } = new();
    }

    public class FixtureDraft
    {
        public string HomeClub { get; set; } = string.Empty;
        public string AwayClub { get; set; } = string.Empty;
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
    }

    public class PerformanceDraft
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public Position Position { get; set; }
        public string Club { get; set; } = string.Empty;
        public string Opponent { get; set; } = string.Empty;
        public int Goals { get; set; }
        public int Assists { get; set; }
        public bool CleanSheet { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int MinutesPlayed { get; set; }
        public int Saves { get; set; }
        public int GoalsConceded { get; set; }
        public int Bonus { get; set; }
        public int PointsEarned { get; set; }
    }

    // Simulira kolo: nasumično pari 20 klubova u 10 utakmica, generira rezultate i
    // pojedinačne učinke igrača, te računa fantasy bodove po FPL pravilima.
    // Deterministički je s obzirom na (seed, skup igrača) — pa je pregled (preview)
    // identičan onome što se sprema pri potvrdi.
    public class GameweekSimulationService
    {
        private readonly FantasyFootballDbContext _ctx;
        private readonly ILogger<GameweekSimulationService> _logger;

        public GameweekSimulationService(FantasyFootballDbContext ctx, ILogger<GameweekSimulationService> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        // ---- Javni API ----

        // Generira draft za pregled (ne sprema ništa).
        public async Task<SimulationDraft> PreviewAsync(int gameweekId, int seed)
        {
            var gw = await _ctx.Gameweeks.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gameweekId)
                     ?? throw new InvalidOperationException($"Kolo {gameweekId} ne postoji.");
            var players = await _ctx.Players.AsNoTracking().ToListAsync();
            return BuildDraft(seed, players, gw.StartDate);
        }

        // Potvrđuje simulaciju: regenerira draft iz istog seeda i sprema utakmice,
        // učinke igrača, ažurira bodove igrača i timova te snima rezultat svakog tima.
        public async Task ConfirmAsync(int gameweekId, int seed)
        {
            var gw = await _ctx.Gameweeks.Include(g => g.Fixtures)
                .FirstOrDefaultAsync(g => g.Id == gameweekId)
                ?? throw new InvalidOperationException($"Kolo {gameweekId} ne postoji.");

            if (gw.Fixtures.Any())
            {
                _logger.LogWarning("Kolo {Week} je već simulirano — potvrda preskočena.", gw.WeekNumber);
                return;
            }

            var players = await _ctx.Players.ToListAsync();
            var draft = BuildDraft(seed, players, gw.StartDate);
            var byId = players.ToDictionary(p => p.Id);

            foreach (var f in draft.Fixtures)
                _ctx.Fixtures.Add(new Fixture
                {
                    GameweekId = gw.Id,
                    HomeClub = f.HomeClub,
                    AwayClub = f.AwayClub,
                    HomeGoals = f.HomeGoals,
                    AwayGoals = f.AwayGoals
                });

            foreach (var pd in draft.Performances)
            {
                _ctx.MatchPerformances.Add(new MatchPerformance
                {
                    GameweekId = gw.Id,
                    PlayerId = pd.PlayerId,
                    MatchDate = gw.StartDate,
                    Opponent = pd.Opponent,
                    Goals = pd.Goals,
                    Assists = pd.Assists,
                    CleanSheet = pd.CleanSheet,
                    YellowCards = pd.YellowCards,
                    RedCards = pd.RedCards,
                    MinutesPlayed = pd.MinutesPlayed,
                    Saves = pd.Saves,
                    GoalsConceded = pd.GoalsConceded,
                    Bonus = pd.Bonus,
                    PointsEarned = pd.PointsEarned
                });

                if (byId.TryGetValue(pd.PlayerId, out var pl))
                {
                    pl.TotalPoints += pd.PointsEarned;
                    pl.Goals += pd.Goals;
                    pl.Assists += pd.Assists;
                    if (pd.CleanSheet && pd.Position != Position.Forward)
                        pl.CleanSheets += 1;
                }
            }

            // Bodovi po fantasy timu = zbroj učinaka njegovih 11 startera u ovom kolu.
            var pointsByPlayer = draft.Performances
                .GroupBy(p => p.PlayerId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.PointsEarned));

            var teams = await _ctx.FantasyTeams.Include(t => t.Players).ToListAsync();
            foreach (var team in teams)
            {
                var starterIds = GetStarterIds(team);
                int teamPts = starterIds.Sum(id => pointsByPlayer.GetValueOrDefault(id, 0));
                _ctx.GameweekTeamScores.Add(new GameweekTeamScore
                {
                    FantasyTeamId = team.Id,
                    GameweekId = gw.Id,
                    Points = teamPts,
                    LineupIds = string.Join(",", starterIds)
                });
                team.TotalPoints += teamPts;
            }

            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Kolo {Week} simulirano i potvrđeno: {Fixtures} utakmica, {Perf} učinaka.",
                gw.WeekNumber, draft.Fixtures.Count, draft.Performances.Count);
        }

        // Briše kolo i poništava sve njegove bodove (igrači + timovi).
        public async Task DeleteWithReversalAsync(int gameweekId)
        {
            var gw = await _ctx.Gameweeks.Include(g => g.Performances)
                .FirstOrDefaultAsync(g => g.Id == gameweekId);
            if (gw == null) return;

            var playerIds = gw.Performances.Select(p => p.PlayerId).Distinct().ToList();
            var players = await _ctx.Players.Where(p => playerIds.Contains(p.Id)).ToListAsync();
            var pById = players.ToDictionary(p => p.Id);

            foreach (var perf in gw.Performances)
            {
                if (!pById.TryGetValue(perf.PlayerId, out var pl)) continue;
                pl.TotalPoints -= perf.PointsEarned;
                pl.Goals = Math.Max(0, pl.Goals - perf.Goals);
                pl.Assists = Math.Max(0, pl.Assists - perf.Assists);
                if (perf.CleanSheet && pl.Position != Position.Forward)
                    pl.CleanSheets = Math.Max(0, pl.CleanSheets - 1);
            }

            var scores = await _ctx.GameweekTeamScores.Where(s => s.GameweekId == gameweekId).ToListAsync();
            var teamIds = scores.Select(s => s.FantasyTeamId).Distinct().ToList();
            var teams = await _ctx.FantasyTeams.Where(t => teamIds.Contains(t.Id)).ToListAsync();
            var tById = teams.ToDictionary(t => t.Id);
            foreach (var s in scores)
                if (tById.TryGetValue(s.FantasyTeamId, out var t))
                    t.TotalPoints -= s.Points;

            var fixtures = await _ctx.Fixtures.Where(f => f.GameweekId == gameweekId).ToListAsync();
            _ctx.Fixtures.RemoveRange(fixtures);
            _ctx.GameweekTeamScores.RemoveRange(scores);
            _ctx.MatchPerformances.RemoveRange(gw.Performances);
            _ctx.Gameweeks.Remove(gw);

            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Kolo {Week} obrisano i bodovi poništeni.", gw.WeekNumber);
        }

        // ---- Generiranje (čista funkcija (seed, igrači) -> draft) ----

        private SimulationDraft BuildDraft(int seed, List<Player> players, DateTime matchDate)
        {
            var rng = new Random(seed);

            // Kanonski poredak prije RNG-a => isti seed daje isti rezultat bez obzira
            // na poredak ulaznog popisa igrača.
            var clubs = players.Select(p => p.Club).Distinct()
                .OrderBy(c => c, StringComparer.Ordinal).ToList();
            Shuffle(clubs, rng);

            var byClub = players.GroupBy(p => p.Club)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<Player>)g.OrderBy(p => p.Id).ToList());

            var draft = new SimulationDraft { Seed = seed };

            for (int i = 0; i + 1 < clubs.Count; i += 2)
            {
                string home = clubs[i], away = clubs[i + 1];
                int hg = RollGoals(rng), ag = RollGoals(rng);
                draft.Fixtures.Add(new FixtureDraft { HomeClub = home, AwayClub = away, HomeGoals = hg, AwayGoals = ag });

                var hp = SimulateClub(byClub[home], away, hg, ag, rng);
                var ap = SimulateClub(byClub[away], home, ag, hg, rng);

                foreach (var p in hp) p.PointsEarned = BasePoints(p);
                foreach (var p in ap) p.PointsEarned = BasePoints(p);

                // Bonus: 3 najbolja igrača utakmice dobivaju +3/+2/+1.
                var ranked = hp.Concat(ap)
                    .OrderByDescending(p => p.PointsEarned)
                    .ThenByDescending(p => p.Goals)
                    .ThenByDescending(p => p.Assists)
                    .ThenBy(p => p.PlayerId)
                    .ToList();
                if (ranked.Count > 0) { ranked[0].Bonus = 3; ranked[0].PointsEarned += 3; }
                if (ranked.Count > 1) { ranked[1].Bonus = 2; ranked[1].PointsEarned += 2; }
                if (ranked.Count > 2) { ranked[2].Bonus = 1; ranked[2].PointsEarned += 1; }

                draft.Performances.AddRange(hp);
                draft.Performances.AddRange(ap);
            }

            return draft;
        }

        private List<PerformanceDraft> SimulateClub(
            IReadOnlyList<Player> clubPlayers, string opponent, int goalsFor, int goalsAgainst, Random rng)
        {
            List<Player> PickPos(Position pos, int n)
            {
                var pool = clubPlayers.Where(p => p.Position == pos).ToList();
                Shuffle(pool, rng);
                return pool.Take(Math.Min(n, pool.Count)).ToList();
            }

            // Početnih 11: 1 GK + 4 DEF + 4 MID + 2 FWD.
            var starters = PickPos(Position.Goalkeeper, 1)
                .Concat(PickPos(Position.Defender, 4))
                .Concat(PickPos(Position.Midfielder, 4))
                .Concat(PickPos(Position.Forward, 2))
                .ToList();

            var perfs = starters.Select(p => new PerformanceDraft
            {
                PlayerId = p.Id,
                PlayerName = $"{p.FirstName} {p.LastName}",
                Position = p.Position,
                Club = p.Club,
                Opponent = opponent,
                MinutesPlayed = MinutesFor(rng)
            }).ToList();

            if (perfs.Count == 0) return perfs;

            // Raspodjela golova kluba na strijelce (težinski po poziciji) + asistencije.
            for (int g = 0; g < goalsFor; g++)
            {
                var scorer = WeightedPick(perfs, x => AttackWeight(x.Position), rng);
                scorer.Goals++;

                var others = perfs.Where(x => x != scorer).ToList();
                if (others.Count > 0 && rng.NextDouble() < 0.6)
                {
                    var assister = WeightedPick(others, x => AssistWeight(x.Position), rng);
                    assister.Assists++;
                }
            }

            foreach (var perf in perfs)
            {
                if (rng.NextDouble() < 0.12) perf.YellowCards = 1;
                if (rng.NextDouble() < 0.015)
                {
                    perf.RedCards = 1;
                    perf.MinutesPlayed = Math.Min(perf.MinutesPlayed, rng.Next(20, 75));
                }

                if (perf.Position == Position.Goalkeeper)
                {
                    perf.Saves = Math.Min(9, rng.Next(1, 5) + goalsAgainst);
                    perf.GoalsConceded = goalsAgainst;
                }
                else if (perf.Position == Position.Defender)
                {
                    perf.GoalsConceded = goalsAgainst;
                }

                perf.CleanSheet = goalsAgainst == 0 && perf.MinutesPlayed >= 60;
            }

            return perfs;
        }

        // ---- FPL bodovanje (vidi FPL-Scoring-Rules.md) ----
        private static int BasePoints(PerformanceDraft p)
        {
            int pts = 0;
            if (p.MinutesPlayed >= 60) pts += 2;
            else if (p.MinutesPlayed >= 1) pts += 1;

            int goalPts = p.Position switch
            {
                Position.Goalkeeper => 10,
                Position.Defender => 6,
                Position.Midfielder => 5,
                Position.Forward => 4,
                _ => 4
            };
            pts += p.Goals * goalPts;
            pts += p.Assists * 3;

            if (p.CleanSheet)
            {
                if (p.Position == Position.Goalkeeper || p.Position == Position.Defender) pts += 4;
                else if (p.Position == Position.Midfielder) pts += 1;
            }

            if (p.Position == Position.Goalkeeper) pts += p.Saves / 3;
            if (p.Position == Position.Goalkeeper || p.Position == Position.Defender) pts -= p.GoalsConceded / 2;

            pts -= p.YellowCards;
            pts -= p.RedCards * 3;
            return pts;
        }

        // ---- Pomoćne ----

        private static int RollGoals(Random rng)
        {
            // Kumulativne težine za 0..6 golova (naglašava niske rezultate).
            double[] cum = { 0.25, 0.55, 0.77, 0.90, 0.96, 0.99, 1.00 };
            double r = rng.NextDouble();
            for (int g = 0; g < cum.Length; g++)
                if (r <= cum[g]) return g;
            return 0;
        }

        private static int MinutesFor(Random rng)
        {
            double r = rng.NextDouble();
            if (r < 0.75) return 90;
            if (r < 0.92) return rng.Next(60, 90);
            return rng.Next(25, 60);
        }

        private static double AttackWeight(Position p) => p switch
        {
            Position.Forward => 5.0,
            Position.Midfielder => 3.0,
            Position.Defender => 1.0,
            Position.Goalkeeper => 0.1,
            _ => 1.0
        };

        private static double AssistWeight(Position p) => p switch
        {
            Position.Midfielder => 3.0,
            Position.Forward => 2.0,
            Position.Defender => 1.0,
            Position.Goalkeeper => 0.1,
            _ => 1.0
        };

        private static T WeightedPick<T>(IList<T> items, Func<T, double> weight, Random rng)
        {
            double total = items.Sum(weight);
            if (total <= 0) return items[rng.Next(items.Count)];
            double r = rng.NextDouble() * total;
            double acc = 0;
            foreach (var it in items)
            {
                acc += weight(it);
                if (r <= acc) return it;
            }
            return items[items.Count - 1];
        }

        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // Startnih 11 fantasy tima: spremljeni sastav ili default (1 GK / 4 DEF /
        // 4 MID / 2 FWD po najvišoj cijeni).
        private static List<int> GetStarterIds(FantasyTeam team)
        {
            var squad = team.Players.ToList();
            var ids = (team.StartingLineupIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var n) ? n : -1)
                .Where(n => n > 0)
                .ToList();

            if (ids.Count == 11 && ids.All(id => squad.Any(p => p.Id == id)))
                return ids;

            List<int> Top(Position pos, int n) => squad
                .Where(p => p.Position == pos)
                .OrderByDescending(p => p.MarketValue)
                .Take(n).Select(p => p.Id).ToList();

            return Top(Position.Goalkeeper, 1)
                .Concat(Top(Position.Defender, 4))
                .Concat(Top(Position.Midfielder, 4))
                .Concat(Top(Position.Forward, 2))
                .ToList();
        }
    }
}
