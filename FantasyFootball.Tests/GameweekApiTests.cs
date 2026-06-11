using System.Net;
using System.Net.Http.Json;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using FluentAssertions;

namespace FantasyFootball.Tests
{
    public class GameweekApiTests : ApiTestBase
    {
        public GameweekApiTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task GetAll_ReturnsOkAndCollection()
        {
            await SeedGameweekAsync();

            var response = await Client.GetAsync("/api/gameweek");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var gameweeks = await response.Content.ReadFromJsonAsync<List<GameweekDTO>>();
            gameweeks.Should().NotBeNull();
            gameweeks!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsGameweek_WhenExists()
        {
            var gw = await SeedGameweekAsync();

            var response = await Client.GetAsync($"/api/gameweek/{gw.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<GameweekDTO>();
            dto!.Id.Should().Be(gw.Id);
            dto.WeekNumber.Should().Be(gw.WeekNumber);
        }

        [Fact]
        public async Task GetById_Returns404_WhenMissing()
        {
            var response = await Client.GetAsync("/api/gameweek/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Returns201_AndPersists()
        {
            var newGw = new Gameweek
            {
                WeekNumber = 30,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(8)
            };

            var response = await Client.PostAsJsonAsync("/api/gameweek", newGw);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto = await response.Content.ReadFromJsonAsync<GameweekDTO>();
            dto!.Id.Should().BeGreaterThan(0);
            dto.WeekNumber.Should().Be(30);
        }

        [Fact]
        public async Task Create_Returns400_WhenInvalid()
        {
            // WeekNumber izvan raspona (1-38) i EndDate prije StartDate
            var invalid = new Gameweek
            {
                WeekNumber = 0,
                StartDate = DateTime.Today.AddDays(8),
                EndDate = DateTime.Today.AddDays(1)
            };

            var response = await Client.PostAsJsonAsync("/api/gameweek", invalid);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_ChangesGameweek_WhenExists()
        {
            var gw = await SeedGameweekAsync();

            var body = new Gameweek
            {
                Id = gw.Id,
                WeekNumber = gw.WeekNumber,
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(9)
            };

            var response = await Client.PutAsJsonAsync($"/api/gameweek/{gw.Id}", body);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<GameweekDTO>();
            dto!.StartDate.Date.Should().Be(DateTime.Today.AddDays(2));
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            var body = new Gameweek
            {
                Id = 999999,
                WeekNumber = 25,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(8)
            };

            var response = await Client.PutAsJsonAsync("/api/gameweek/999999", body);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesGameweek_WhenExists()
        {
            var gw = await SeedGameweekAsync();

            var response = await Client.DeleteAsync($"/api/gameweek/{gw.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            await WithDbAsync(async ctx =>
            {
                var exists = ctx.Gameweeks.Any(g => g.Id == gw.Id);
                exists.Should().BeFalse();
            });
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            var response = await Client.DeleteAsync("/api/gameweek/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
