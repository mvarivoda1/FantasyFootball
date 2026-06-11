using System.Net;
using System.Net.Http.Json;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using FluentAssertions;

namespace FantasyFootball.Tests
{
    public class PlayerApiTests : ApiTestBase
    {
        public PlayerApiTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task GetAll_ReturnsOkAndCollection()
        {
            await SeedPlayerAsync();

            var response = await Client.GetAsync("/api/player");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var players = await response.Content.ReadFromJsonAsync<List<PlayerDTO>>();
            players.Should().NotBeNull();
            players!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsPlayer_WhenExists()
        {
            var player = await SeedPlayerAsync();

            var response = await Client.GetAsync($"/api/player/{player.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<PlayerDTO>();
            dto!.Id.Should().Be(player.Id);
            dto.FullName.Should().Be(player.FullName);
        }

        [Fact]
        public async Task GetById_Returns404_WhenMissing()
        {
            var response = await Client.GetAsync("/api/player/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Search_ReturnsMatchingPlayers()
        {
            var player = await SeedPlayerAsync(club: "UniqueClubXYZ");

            var response = await Client.GetAsync("/api/player?q=UniqueClubXYZ");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var players = await response.Content.ReadFromJsonAsync<List<PlayerDTO>>();
            players!.Should().Contain(p => p.Id == player.Id);
        }

        [Fact]
        public async Task Create_Returns201_AndPersists()
        {
            var newPlayer = new Player
            {
                FirstName = "New",
                LastName = "Striker",
                Position = Position.Forward,
                Club = "FC Test",
                Nationality = "Brazil",
                DateOfBirth = new DateTime(1998, 3, 3),
                MarketValue = 9.0,
                Goals = 10,
                Assists = 5,
                CleanSheets = 0,
                TotalPoints = 120
            };

            var response = await Client.PostAsJsonAsync("/api/player", newPlayer);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto = await response.Content.ReadFromJsonAsync<PlayerDTO>();
            dto!.Id.Should().BeGreaterThan(0);
            dto.LastName.Should().Be("Striker");

            await WithDbAsync(async ctx =>
            {
                var exists = ctx.Players.Any(p => p.Id == dto.Id);
                exists.Should().BeTrue();
            });
        }

        [Fact]
        public async Task Create_Returns400_WhenInvalid()
        {
            var invalid = new Player
            {
                FirstName = "",     // Required
                LastName = "",      // Required
                Position = Position.Defender,
                Club = "",          // Required
                Nationality = "",   // Required
                MarketValue = 0     // Range(0.1, 200)
            };

            var response = await Client.PostAsJsonAsync("/api/player", invalid);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_ChangesPlayer_WhenExists()
        {
            var player = await SeedPlayerAsync();

            var body = new Player
            {
                Id = player.Id,
                FirstName = "Updated",
                LastName = "Name",
                Position = Position.Goalkeeper,
                Club = "New Club",
                Nationality = "Spain",
                DateOfBirth = player.DateOfBirth,
                MarketValue = 11.0,
                Goals = 0,
                Assists = 0,
                CleanSheets = 8,
                TotalPoints = 99
            };

            var response = await Client.PutAsJsonAsync($"/api/player/{player.Id}", body);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<PlayerDTO>();
            dto!.FirstName.Should().Be("Updated");
            dto.Club.Should().Be("New Club");
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            var body = new Player
            {
                Id = 999999,
                FirstName = "Ghost",
                LastName = "Player",
                Position = Position.Midfielder,
                Club = "Nowhere",
                Nationality = "None",
                DateOfBirth = new DateTime(1990, 1, 1),
                MarketValue = 5.0
            };

            var response = await Client.PutAsJsonAsync("/api/player/999999", body);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesPlayer_WhenExists()
        {
            var player = await SeedPlayerAsync();

            var response = await Client.DeleteAsync($"/api/player/{player.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            await WithDbAsync(async ctx =>
            {
                var exists = ctx.Players.Any(p => p.Id == player.Id);
                exists.Should().BeFalse();
            });
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            var response = await Client.DeleteAsync("/api/player/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
