using System.Net;
using System.Net.Http.Json;
using FantasyFootball.Models.DTO;
using FluentAssertions;

namespace FantasyFootball.Tests
{
    public class LeagueApiTests : ApiTestBase
    {
        public LeagueApiTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task GetAll_ReturnsOkAndCollection()
        {
            await SeedLeagueAsync();

            var response = await Client.GetAsync("/api/league");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var leagues = await response.Content.ReadFromJsonAsync<List<LeagueDTO>>();
            leagues.Should().NotBeNull();
            leagues!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsLeague_WhenExists()
        {
            var league = await SeedLeagueAsync();

            var response = await Client.GetAsync($"/api/league/{league.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<LeagueDTO>();
            dto!.Id.Should().Be(league.Id);
            dto.Name.Should().Be(league.Name);
        }

        [Fact]
        public async Task GetById_Returns404_WhenMissing()
        {
            var response = await Client.GetAsync("/api/league/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Returns201_AndPersists()
        {
            var input = new LeagueInputDTO
            {
                Name = "API Liga",
                Season = "2026/2027",
                MaxTeams = 8,
                Description = "Kreirana preko API-ja"
            };

            var response = await Client.PostAsJsonAsync("/api/league", input);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto = await response.Content.ReadFromJsonAsync<LeagueDTO>();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Name.Should().Be("API Liga");
            dto.JoinCode.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Create_Returns400_WhenInvalid()
        {
            var invalid = new LeagueInputDTO
            {
                Name = "",      // Required
                MaxTeams = 1    // Range(2,20)
            };

            var response = await Client.PostAsJsonAsync("/api/league", invalid);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_ChangesLeague_WhenExists()
        {
            var league = await SeedLeagueAsync();

            var input = new LeagueInputDTO
            {
                Name = "Promijenjeno ime",
                Season = league.Season,
                MaxTeams = 12,
                Description = "Novi opis"
            };

            var response = await Client.PutAsJsonAsync($"/api/league/{league.Id}", input);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<LeagueDTO>();
            dto!.Name.Should().Be("Promijenjeno ime");
            dto.MaxTeams.Should().Be(12);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            var input = new LeagueInputDTO { Name = "Ghost", MaxTeams = 5 };

            var response = await Client.PutAsJsonAsync("/api/league/999999", input);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesLeague_WhenExists()
        {
            var league = await SeedLeagueAsync();

            var response = await Client.DeleteAsync($"/api/league/{league.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            await WithDbAsync(async ctx =>
            {
                var exists = ctx.Leagues.Any(l => l.Id == league.Id);
                exists.Should().BeFalse();
            });
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            var response = await Client.DeleteAsync("/api/league/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
