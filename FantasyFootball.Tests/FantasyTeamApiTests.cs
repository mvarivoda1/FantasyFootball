using System.Net;
using System.Net.Http.Json;
using FantasyFootball.Models.DTO;
using FluentAssertions;

namespace FantasyFootball.Tests
{
    public class FantasyTeamApiTests : ApiTestBase
    {
        public FantasyTeamApiTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task GetAll_ReturnsOkAndCollection()
        {
            await SeedTeamAsync();

            var response = await Client.GetAsync("/api/fantasyteam");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var teams = await response.Content.ReadFromJsonAsync<List<FantasyTeamDTO>>();
            teams.Should().NotBeNull();
            teams!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsTeam_WhenExists()
        {
            var team = await SeedTeamAsync();

            var response = await Client.GetAsync($"/api/fantasyteam/{team.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<FantasyTeamDTO>();
            dto!.Id.Should().Be(team.Id);
            dto.Name.Should().Be(team.Name);
            dto.Players.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_Returns404_WhenMissing()
        {
            var response = await Client.GetAsync("/api/fantasyteam/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Returns201_AndPersists()
        {
            var input = new FantasyTeamInputDTO
            {
                Name = "API Tim",
                OwnerName = "Vlasnik"
            };

            var response = await Client.PostAsJsonAsync("/api/fantasyteam", input);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto = await response.Content.ReadFromJsonAsync<FantasyTeamDTO>();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Name.Should().Be("API Tim");
        }

        [Fact]
        public async Task Create_Returns400_WhenInvalid()
        {
            var invalid = new FantasyTeamInputDTO
            {
                Name = "",       // Required
                OwnerName = ""   // Required
            };

            var response = await Client.PostAsJsonAsync("/api/fantasyteam", invalid);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_ChangesTeam_WhenExists()
        {
            var team = await SeedTeamAsync();

            var input = new FantasyTeamInputDTO
            {
                Name = "Novo ime tima",
                OwnerName = "Novi vlasnik"
            };

            var response = await Client.PutAsJsonAsync($"/api/fantasyteam/{team.Id}", input);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<FantasyTeamDTO>();
            dto!.Name.Should().Be("Novo ime tima");
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            var input = new FantasyTeamInputDTO { Name = "Ghost", OwnerName = "None" };

            var response = await Client.PutAsJsonAsync("/api/fantasyteam/999999", input);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesTeam_WhenExists()
        {
            var team = await SeedTeamAsync();

            var response = await Client.DeleteAsync($"/api/fantasyteam/{team.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            await WithDbAsync(async ctx =>
            {
                var exists = ctx.FantasyTeams.Any(t => t.Id == team.Id);
                exists.Should().BeFalse();
            });
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            var response = await Client.DeleteAsync("/api/fantasyteam/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
