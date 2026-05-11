using FantasyFootball.Models;
using FantasyFootball.Repositories;
using FantasyFootball.Services;

namespace FantasyFootball.DAL
{
    public static class DbSeeder
    {
        public static void Seed(FantasyFootballDbContext context)
        {
            SeedUsers(context);

            // Ako baza već ima podatke, preskoči seeding
            if (context.Players.Any())
                return;

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

        private static void SeedUsers(FantasyFootballDbContext context)
        {
            var teams = context.FantasyTeams.ToList();
            if (teams.Count == 0)
                return;

            var existingEmails = context.Users.Select(u => u.Email).ToHashSet();
            var toAdd = new List<User>();

            foreach (var team in teams)
            {
                if (string.IsNullOrWhiteSpace(team.OwnerName)) continue;

                var slug = team.OwnerName.Trim().ToLowerInvariant();
                var email = $"{slug}@gmail.com";
                var password = $"{slug}pass";

                if (existingEmails.Contains(email)) continue;

                toAdd.Add(new User
                {
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password),
                    CreatedAt = DateTime.UtcNow,
                    Budget = 100.0,
                    FantasyTeamId = team.Id
                });
                existingEmails.Add(email);
            }

            if (toAdd.Count > 0)
            {
                context.Users.AddRange(toAdd);
                context.SaveChanges();
            }
        }
    }
}
