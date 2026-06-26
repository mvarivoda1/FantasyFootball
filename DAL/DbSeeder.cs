using FantasyFootball.Models;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.DAL
{
    public static class DbSeeder
    {
        // Cjenovni raspon u koji se mapiraju TotalPoints svih igrača (FPL-style).
        private const double MinPrice = 4.5;
        private const double MaxPrice = 13.0;

        public const string AdminRole = "Admin";
        public const string UserRole = "User";

        public static async Task SeedAsync(
            FantasyFootballDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await SeedRolesAsync(roleManager);

            // Ako baza već ima podatke, preskoči seeding (ali ipak rebalansiraj cijene)
            if (context.Players.Any())
            {
                RebalancePrices(context);
                await SeedUsersAsync(context, userManager);
                await EnsureAdminAsync(userManager);
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

            // Sinkroniziraj N-N back-reference: player.FantasyTeams mora odgovarati
            // team.Players kako EF ne bi spremio "duhove" iz originalnog mock seta.
            ResyncTeamPlayerBackrefs(teams, players);

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
            await SeedUsersAsync(context, userManager);
            await EnsureAdminAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in new[] { AdminRole, UserRole })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
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

            // Rebalans cijena može gurnuti nekoć valjan tim preko 100M (→ negativan
            // budget) ili je zatečeni zapis prekršio max-3-po-klubu; popravi takve.
            EnforceConstraints(teams, players);

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

        private static void ResyncTeamPlayerBackrefs(List<FantasyTeam> teams, List<Player> allPlayers)
        {
            foreach (var p in allPlayers) p.FantasyTeams.Clear();
            foreach (var team in teams)
                foreach (var p in team.Players)
                    if (!p.FantasyTeams.Contains(team))
                        p.FantasyTeams.Add(team);
        }

        // Popravi timove koji krše invarijante: max 3 igrača iz istog kluba i
        // ukupna vrijednost ≤ 100M (jer je budget = 100 − SquadValue i ne smije
        // biti negativan). Neispravan tim se prerađuje u nasumičan valjan 15-igrački
        // sastav. Pokreće se pri svakom seedingu pa čisti zatečene zapise i sprječava
        // da rebalansiranje cijena ostavi tim u nevažećem stanju.
        private static void EnforceConstraints(List<FantasyTeam> teams, List<Player> allPlayers)
        {
            var byPos = allPlayers
                .GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g.ToList());

            for (int idx = 0; idx < teams.Count; idx++)
            {
                var team = teams[idx];

                // Nepotpune timove je već obradio EnsureFullSquads (unutar pravila).
                if (team.Players.Count != SquadSize) continue;

                bool overClub = team.Players
                    .GroupBy(p => p.Club)
                    .Any(g => g.Count() > MaxPerClub);
                bool overBudget = team.Players.Sum(p => p.MarketValue) > SquadBudget + 1e-6;

                if (!overClub && !overBudget) continue;

                var rng = new Random((idx + 1) * 23 + 91);
                var fresh = BuildSquad(byPos, rng) ?? BuildValidSquad(byPos, rng);
                if (fresh == null) continue; // ne uspijem li složiti valjan tim, ostavi kako jest

                team.Players.Clear();
                foreach (var p in fresh) team.Players.Add(p);

                // Sastav se promijenio — resetiraj startnu postavu (MyTeam je posloži nanovo).
                team.StartingLineupIds = null;
            }
        }

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
                // BuildValidSquad je garantirani fallback ako nasumični BuildSquad promaši.
                var fresh = BuildSquad(byPos, rng) ?? BuildValidSquad(byPos, rng);
                if (fresh == null) continue;

                team.Players.Clear();
                foreach (var p in fresh) team.Players.Add(p);

                // Sastav se promijenio — resetiraj startnu postavu (MyTeam je posloži nanovo).
                team.StartingLineupIds = null;
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

        // Deterministički sastavi GARANTIRANO valjan 15-igrački tim (2/5/5/3,
        // ≤ 100M, ≤ 3 igrača po klubu). BuildSquad radi nasumične pokušaje koji za
        // neke bazene/seedove uvijek promaše (npr. minimalni sastav stane ~89M, ali
        // weighted-random stalno prebaci budžet) — zbog čega su neki timovi ostajali
        // na 11 igrača (bez klupe). Ova metoda kreće od najjeftinijeg valjanog sastava
        // (uvijek postoji) pa ga, dok budžet dopušta, nasumično "nadograđuje" radi
        // raznolikosti. Vraća null samo ako bazen igrača fizički nema dovoljno igrača.
        private static List<Player>? BuildValidSquad(
            Dictionary<Position, List<Player>> byPos, Random rng)
        {
            var chosen = new List<Player>();
            var chosenIds = new HashSet<int>();
            var clubCount = new Dictionary<string, int>();

            // 1) Najjeftiniji valjani sastav po pozicijama — unutar budžeta i club-limita.
            foreach (var kv in RequiredByPos)
            {
                if (!byPos.TryGetValue(kv.Key, out var pool)) return null;

                int taken = 0;
                foreach (var p in pool.OrderBy(pl => pl.MarketValue))
                {
                    if (taken == kv.Value) break;
                    if (chosenIds.Contains(p.Id)) continue;
                    if (clubCount.GetValueOrDefault(p.Club, 0) >= MaxPerClub) continue;

                    chosen.Add(p);
                    chosenIds.Add(p.Id);
                    clubCount[p.Club] = clubCount.GetValueOrDefault(p.Club, 0) + 1;
                    taken++;
                }
                if (taken < kv.Value) return null; // premalen bazen za ovu poziciju
            }

            double cost = chosen.Sum(p => p.MarketValue);
            if (cost > SquadBudget + 1e-6) return null; // teorijski nemoguće

            // 2) Nasumične nadogradnje radi raznolikosti — nikad ne prelaze budžet ni club-limit.
            for (int iter = 0; iter < 60; iter++)
            {
                int slot = rng.Next(chosen.Count);
                var current = chosen[slot];
                double headroom = SquadBudget - cost;

                var upgrade = byPos[current.Position]
                    .Where(p => !chosenIds.Contains(p.Id)
                             && p.MarketValue > current.MarketValue
                             && p.MarketValue - current.MarketValue <= headroom + 1e-6
                             && (p.Club == current.Club
                                 || clubCount.GetValueOrDefault(p.Club, 0) < MaxPerClub))
                    .OrderBy(_ => rng.Next())
                    .FirstOrDefault();
                if (upgrade == null) continue;

                chosenIds.Remove(current.Id);
                clubCount[current.Club]--;
                chosenIds.Add(upgrade.Id);
                clubCount[upgrade.Club] = clubCount.GetValueOrDefault(upgrade.Club, 0) + 1;
                cost += upgrade.MarketValue - current.MarketValue;
                chosen[slot] = upgrade;
            }

            return chosen;
        }

        private static async Task SeedUsersAsync(
            FantasyFootballDbContext context,
            UserManager<AppUser> userManager)
        {
            var teams = context.FantasyTeams.ToList();
            if (teams.Count == 0)
                return;

            // Timovi koji su već povezani s nekim korisnikom — ne smiju dobiti seed-user
            // jer je AppUser.FantasyTeamId UNIQUE (1-1 veza).
            var ownedTeamIds = context.Users
                .Where(u => u.FantasyTeamId != null)
                .Select(u => u.FantasyTeamId!.Value)
                .ToHashSet();

            int idx = 0;
            foreach (var team in teams)
            {
                idx++;
                if (string.IsNullOrWhiteSpace(team.OwnerName)) continue;
                if (ownedTeamIds.Contains(team.Id)) continue;

                var slug = new string(team.OwnerName.Trim().ToLowerInvariant()
                    .Where(char.IsLetterOrDigit).ToArray());
                if (slug.Length == 0) continue;

                var email = $"{slug}@gmail.com";
                var password = $"{slug}pass";

                if (await userManager.FindByEmailAsync(email) != null) continue;

                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    // Budget = 100 - SquadValue (transferski budget = preostalo od početnih 100M)
                    Budget = Math.Round(SquadBudget - team.SquadValue, 1),
                    FantasyTeamId = team.Id,
                    OIB = GenerateDigits(11, idx),
                    JMBG = GenerateDigits(13, idx)
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRole);
                    ownedTeamIds.Add(team.Id);
                }
            }
        }

        // Garantira da barem jedan korisnik ima Admin rolu (prvi po vremenu kreiranja).
        private static async Task EnsureAdminAsync(UserManager<AppUser> userManager)
        {
            var admins = await userManager.GetUsersInRoleAsync(AdminRole);
            if (admins.Count > 0) return;

            var first = userManager.Users
                .OrderBy(u => u.CreatedAt)
                .ThenBy(u => u.Email)
                .FirstOrDefault();

            if (first != null)
                await userManager.AddToRoleAsync(first, AdminRole);
        }

        // Deterministički generira numerički string zadane duljine (dummy OIB/JMBG).
        private static string GenerateDigits(int length, int seed)
        {
            var rng = new Random(seed * 7919 + length);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = (char)('0' + rng.Next(0, 10));
            return new string(chars);
        }
    }
}
