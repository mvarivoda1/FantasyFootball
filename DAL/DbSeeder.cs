using FantasyFootball.Models;
using FantasyFootball.Repositories;
using FantasyFootball.Services;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.DAL
{
    public static class DbSeeder
    {
        // Cjenovni raspon u koji se mapiraju TotalPoints svih igrača (FPL-style).
        private const double MinPrice = 4.5;
        private const double MaxPrice = 13.0;

        public static void Seed(FantasyFootballDbContext context)
        {
            SeedUsers(context);

            // Ako baza već ima podatke, preskoči seeding (ali ipak rebalansiraj cijene)
            if (context.Players.Any())
            {
                RebalancePrices(context);
                return;
            }

            // Mock repozitoriji već grade cjeloviti objektni graf s navigacijskim
            // svojstvima, pa ga možemo iskoristiti kao izvor za seeding.
            var playerRepo = new PlayerMockRepository();
            var teamRepo = new FantasyTeamMockRepository(playerRepo);
            var transferRepo = new TransferMockRepository(playerRepo, teamRepo);
            var leagueRepo = new LeagueMockRepository(teamRepo, transferRepo);
            var gameweekRepo = new GameweekMockRepository(playerRepo);

            var players = playerRepo.GetAll();
            var teams = teamRepo.GetAll();
            var leagues = leagueRepo.GetAll();
            var transfers = transferRepo.GetAll();
            var gameweeks = gameweekRepo.GetAll();

            // Resetiraj ID-eve na 0 — neka EF/SQL dodijeli nove IDENTITY vrijednosti.
            // Navigacijska svojstva zadržavaju reference na iste objekte pa će EF
            // ispravno popuniti FK vrijednosti nakon SaveChanges.
            foreach (var p in players) p.Id = 0;
            foreach (var t in teams) t.Id = 0;
            foreach (var l in leagues) l.Id = 0;
            foreach (var tr in transfers) tr.Id = 0;
            foreach (var g in gameweeks) g.Id = 0;
            foreach (var mp in gameweeks.SelectMany(g => g.Performances)) mp.Id = 0;

            // Poveži transfere s ligama (1-N) kako bi League.Transfers bio ispunjen u DB.
            foreach (var league in leagues)
                foreach (var tr in league.Transfers)
                    tr.League = league;

            // Mapiraj MarketValue u raspon [MinPrice, MaxPrice] na temelju TotalPoints
            // prije nego objekti odu u bazu.
            ApplyPointBasedPrices(players);

            // Svaki tim mora imati točno 15 igrača (2 GK / 5 DEF / 5 MID / 3 FWD),
            // unutar budžeta i max 3 po klubu. Mock timovi imaju 11 — dopunjavamo.
            EnsureFullSquads(teams, players);

            foreach (var t in teams)
                t.SquadValue = Math.Round(t.Players.Sum(p => p.MarketValue), 1);

            // Dovoljno je dodati "root" entitete — EF prati graf preko navigacijskih
            // svojstava i automatski dodaje sve povezane entitete.
            context.Players.AddRange(players);
            context.Leagues.AddRange(leagues);
            context.Gameweeks.AddRange(gameweeks);

            context.SaveChanges();

            // Nakon što su timovi spremljeni i dobili Id, kreiraj korisničke račune
            // za svaki postojeći OwnerName (npr. Marko -> marko@gmail.com / markopass).
            SeedUsers(context);
        }

        private static void ApplyPointBasedPrices(IEnumerable<Player> players)
        {
            var list = players.ToList();
            if (list.Count == 0) return;

            var minPts = list.Min(p => p.TotalPoints);
            var maxPts = list.Max(p => p.TotalPoints);
            var range = Math.Max(1, maxPts - minPts);

            foreach (var p in list)
            {
                var t = (double)(p.TotalPoints - minPts) / range;
                p.MarketValue = Math.Round(MinPrice + t * (MaxPrice - MinPrice), 1);
            }
        }

        private static void RebalancePrices(FantasyFootballDbContext context)
        {
            var players = context.Players.ToList();
            if (players.Count == 0) return;

            ApplyPointBasedPrices(players);

            // Dopuni timove ispod 15 igrača (ostavi netaknute one s točno 15 — npr.
            // timove koje su korisnici sami kreirali kroz UI).
            var teams = context.FantasyTeams.Include(t => t.Players).ToList();
            EnsureFullSquads(teams, players);

            foreach (var team in teams)
                team.SquadValue = Math.Round(team.Players.Sum(p => p.MarketValue), 1);

            // Korisnički budget = 100 - SquadValue: invarijanta koja se mora osvježiti
            // nakon rebalansiranja cijena ili dopune squad-a.
            AdjustUserBudgets(context, teams);

            context.SaveChanges();
        }

        private static void AdjustUserBudgets(FantasyFootballDbContext context, List<FantasyTeam> teams)
        {
            var squadValueByTeamId = teams.ToDictionary(t => t.Id, t => t.SquadValue);
            var users = context.Users.Where(u => u.FantasyTeamId != null).ToList();
            foreach (var user in users)
            {
                if (!user.FantasyTeamId.HasValue) continue;
                if (!squadValueByTeamId.TryGetValue(user.FantasyTeamId.Value, out var sv)) continue;
                user.Budget = Math.Round(SquadBudget - sv, 1);
            }
        }

        // ===== Squad builder =====

        private const int SquadSize = 15;
        private const double SquadBudget = 100.0;
        private const int MaxPerClub = 3;
        private static readonly Dictionary<Position, int> RequiredByPos = new()
        {
            [Position.Goalkeeper] = 2,
            [Position.Defender]   = 5,
            [Position.Midfielder] = 5,
            [Position.Forward]    = 3,
        };

        private static void EnsureFullSquads(List<FantasyTeam> teams, List<Player> allPlayers)
        {
            var byPos = allPlayers
                .GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g.ToList());

            for (int idx = 0; idx < teams.Count; idx++)
            {
                var team = teams[idx];

                // Tim već ima točno 15 igrača — vjerojatno user-created, ne diraj.
                if (team.Players.Count == SquadSize) continue;

                // Deterministički seed po indeksu tima — različite kombinacije, reproducibilno.
                var rng = new Random((idx + 1) * 17 + 31);

                if (TryFillMissing(team, byPos, rng))
                    continue;

                // Postojeća jezgra ne dopušta proširenje (npr. već prelazi budžet
                // nakon rebalansa cijena) — sastavi cijeli novi 15-igrački tim.
                var fresh = BuildSquad(byPos, rng);
                if (fresh == null) continue;

                team.Players.Clear();
                foreach (var p in fresh) team.Players.Add(p);
            }
        }

        private static bool TryFillMissing(FantasyTeam team,
            Dictionary<Position, List<Player>> byPos, Random rng)
        {
            var currentByPos = team.Players
                .GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g.Count());
            var clubCount = team.Players
                .GroupBy(p => p.Club)
                .ToDictionary(g => g.Key, g => g.Count());
            var chosenIds = team.Players.Select(p => p.Id).ToHashSet();
            double cost = team.Players.Sum(p => p.MarketValue);

            // Pozicije koje trebaju dopunu
            var needed = new List<Position>();
            foreach (var kv in RequiredByPos)
            {
                int have = currentByPos.GetValueOrDefault(kv.Key, 0);
                int diff = kv.Value - have;
                if (diff < 0) return false; // postojeći squad već ima previše ove pozicije
                for (int i = 0; i < diff; i++) needed.Add(kv.Key);
            }

            if (needed.Count == 0) return team.Players.Count == SquadSize;

            // Provjeri da postojeća jezgra ne krši club-limit
            if (clubCount.Values.Any(c => c > MaxPerClub)) return false;

            var newPicks = new List<Player>();
            for (int k = 0; k < needed.Count; k++)
            {
                var pos = needed[k];
                int remainingAfter = needed.Count - k - 1;
                double maxAllowed = SquadBudget - cost - remainingAfter * MinPrice;
                if (maxAllowed < MinPrice - 1e-6) return false;

                double targetAvg = (SquadBudget - cost) / (remainingAfter + 1);
                double target = targetAvg + (rng.NextDouble() - 0.5) * 1.6;

                if (!byPos.TryGetValue(pos, out var pool)) return false;
                var candidates = pool
                    .Where(pl => !chosenIds.Contains(pl.Id)
                              && pl.MarketValue <= maxAllowed + 1e-6
                              && clubCount.GetValueOrDefault(pl.Club, 0) < MaxPerClub)
                    .ToList();

                if (candidates.Count == 0) return false;

                var pick = PickToward(candidates, target, rng);
                newPicks.Add(pick);
                chosenIds.Add(pick.Id);
                clubCount[pick.Club] = clubCount.GetValueOrDefault(pick.Club, 0) + 1;
                cost += pick.MarketValue;
            }

            foreach (var p in newPicks) team.Players.Add(p);
            return true;
        }

        private static List<Player>? BuildSquad(
            Dictionary<Position, List<Player>> byPos, Random rng)
        {
            var picks = new List<Position>();
            foreach (var kv in RequiredByPos)
                for (int i = 0; i < kv.Value; i++) picks.Add(kv.Key);

            for (int attempt = 0; attempt < 1000; attempt++)
            {
                var chosen = new List<Player>();
                var chosenIds = new HashSet<int>();
                var clubCount = new Dictionary<string, int>();
                double cost = 0;
                bool ok = true;

                for (int k = 0; k < picks.Count; k++)
                {
                    var pos = picks[k];
                    int remainingAfter = picks.Count - k - 1;
                    double maxAllowed = SquadBudget - cost - remainingAfter * MinPrice;
                    double targetAvg = (SquadBudget - cost) / (remainingAfter + 1);
                    double target = targetAvg + (rng.NextDouble() - 0.5) * 1.6;

                    if (!byPos.TryGetValue(pos, out var pool)) { ok = false; break; }
                    var candidates = pool
                        .Where(pl => !chosenIds.Contains(pl.Id)
                                  && pl.MarketValue <= maxAllowed + 1e-6
                                  && clubCount.GetValueOrDefault(pl.Club, 0) < MaxPerClub)
                        .ToList();

                    if (candidates.Count == 0) { ok = false; break; }

                    var pick = PickToward(candidates, target, rng);
                    chosen.Add(pick);
                    chosenIds.Add(pick.Id);
                    clubCount[pick.Club] = clubCount.GetValueOrDefault(pick.Club, 0) + 1;
                    cost += pick.MarketValue;
                }

                if (ok) return chosen;
            }
            return null;
        }

        private static Player PickToward(List<Player> candidates, double target, Random rng)
        {
            var weights = candidates
                .Select(pl => 1.0 / (0.4 + Math.Abs(pl.MarketValue - target)))
                .ToList();
            double total = weights.Sum();
            double r = rng.NextDouble() * total;
            double acc = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                acc += weights[i];
                if (r <= acc) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }

        private static void SeedUsers(FantasyFootballDbContext context)
        {
            var teams = context.FantasyTeams.ToList();
            if (teams.Count == 0)
                return;

            var existingEmails = context.Users.Select(u => u.Email).ToHashSet();
            // Timovi koji su već povezani s nekim korisnikom — ne smiju dobiti seed-user
            // jer je User.FantasyTeamId UNIQUE (1-1 veza).
            var ownedTeamIds = context.Users
                .Where(u => u.FantasyTeamId != null)
                .Select(u => u.FantasyTeamId!.Value)
                .ToHashSet();
            var toAdd = new List<User>();

            foreach (var team in teams)
            {
                if (string.IsNullOrWhiteSpace(team.OwnerName)) continue;
                if (ownedTeamIds.Contains(team.Id)) continue;

                var slug = team.OwnerName.Trim().ToLowerInvariant();
                var email = $"{slug}@gmail.com";
                var password = $"{slug}pass";

                if (existingEmails.Contains(email)) continue;

                toAdd.Add(new User
                {
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password),
                    CreatedAt = DateTime.UtcNow,
                    // Budget = 100 - SquadValue (transferski budget = preostalo od početnih 100M)
                    Budget = Math.Round(SquadBudget - team.SquadValue, 1),
                    FantasyTeamId = team.Id
                });
                existingEmails.Add(email);
                ownedTeamIds.Add(team.Id);
            }

            if (toAdd.Count > 0)
            {
                context.Users.AddRange(toAdd);
                context.SaveChanges();
            }
        }
    }
}
