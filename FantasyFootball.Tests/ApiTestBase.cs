using FantasyFootball.DAL;
using FantasyFootball.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyFootball.Tests
{
    // Zajednička osnova za API integracijske testove — dijeli jednu testnu aplikaciju
    // (InMemory baza) i nudi pomoćne metode za seedanje minimalnih podataka.
    public abstract class ApiTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected readonly HttpClient Client;

        protected ApiTestBase(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
        }

        // Izvrši akciju nad svježim DbContext-om (seed / provjera stanja baze).
        protected async Task WithDbAsync(Func<FantasyFootballDbContext, Task> action)
        {
            using var scope = Factory.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<FantasyFootballDbContext>();
            await action(ctx);
        }

        protected async Task<Player> SeedPlayerAsync(string? club = null)
        {
            var player = new Player
            {
                FirstName = "Test",
                LastName = "Player" + Guid.NewGuid().ToString("N")[..6],
                Position = Position.Midfielder,
                Club = club ?? "Test FC",
                Nationality = "Croatia",
                DateOfBirth = new DateTime(1995, 5, 5),
                MarketValue = 7.5,
                Goals = 1,
                Assists = 2,
                CleanSheets = 0,
                TotalPoints = 50
            };
            await WithDbAsync(async ctx =>
            {
                ctx.Players.Add(player);
                await ctx.SaveChangesAsync();
            });
            return player;
        }

        protected async Task<League> SeedLeagueAsync()
        {
            var league = new League
            {
                Name = "Test League " + Guid.NewGuid().ToString("N")[..6],
                Season = "2026/2027",
                MaxTeams = 10,
                Description = "Test",
                CreatedAt = DateTime.UtcNow,
                JoinCode = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()
            };
            await WithDbAsync(async ctx =>
            {
                ctx.Leagues.Add(league);
                await ctx.SaveChangesAsync();
            });
            return league;
        }

        protected async Task<FantasyTeam> SeedTeamAsync()
        {
            var team = new FantasyTeam
            {
                Name = "Test Team " + Guid.NewGuid().ToString("N")[..6],
                OwnerName = "Tester",
                CreatedAt = DateTime.UtcNow,
                SquadValue = 0,
                TotalPoints = 0
            };
            await WithDbAsync(async ctx =>
            {
                ctx.FantasyTeams.Add(team);
                await ctx.SaveChangesAsync();
            });
            return team;
        }

        protected async Task<Gameweek> SeedGameweekAsync()
        {
            var gw = new Gameweek
            {
                WeekNumber = NextWeekNumber(),
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(8)
            };
            await WithDbAsync(async ctx =>
            {
                ctx.Gameweeks.Add(gw);
                await ctx.SaveChangesAsync();
            });
            return gw;
        }

        // Jedinstveni broj kola unutar dopuštenog raspona (1-20), da prođe i kroz API
        // validaciju [Range(1,38)] kod PUT-a. Broj 30 je rezerviran za Create test.
        private static int _weekCounter = 0;
        protected static int NextWeekNumber()
            => ((System.Threading.Interlocked.Increment(ref _weekCounter) - 1) % 20) + 1;
    }
}
