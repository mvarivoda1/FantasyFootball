using System.Net;
using System.Net.Http.Json;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using FluentAssertions;

namespace FantasyFootball.Tests
{
    public class TransferApiTests : ApiTestBase
    {
        public TransferApiTests(CustomWebApplicationFactory factory) : base(factory) { }

        // Seeda transfer + povezani igrač i tim; vraća (transferId, playerId, teamId).
        private async Task<(int transferId, int playerId, int teamId)> SeedTransferAsync()
        {
            var player = await SeedPlayerAsync();
            var team = await SeedTeamAsync();
            var transfer = new Transfer
            {
                PlayerId = player.Id,
                TeamId = team.Id,
                Direction = TransferDirection.In,
                Price = player.MarketValue,
                TransferDate = DateTime.UtcNow
            };
            await WithDbAsync(async ctx =>
            {
                ctx.Transfers.Add(transfer);
                await ctx.SaveChangesAsync();
            });
            return (transfer.Id, player.Id, team.Id);
        }

        [Fact]
        public async Task GetAll_ReturnsOkAndCollection()
        {
            await SeedTransferAsync();

            var response = await Client.GetAsync("/api/transfer");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var transfers = await response.Content.ReadFromJsonAsync<List<TransferDTO>>();
            transfers.Should().NotBeNull();
            transfers!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsTransfer_WhenExists()
        {
            var (transferId, _, _) = await SeedTransferAsync();

            var response = await Client.GetAsync($"/api/transfer/{transferId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<TransferDTO>();
            dto!.Id.Should().Be(transferId);
            dto.PlayerName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_Returns404_WhenMissing()
        {
            var response = await Client.GetAsync("/api/transfer/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Returns201_AndPersists()
        {
            var player = await SeedPlayerAsync();
            var team = await SeedTeamAsync();

            var input = new TransferInputDTO
            {
                PlayerId = player.Id,
                TeamId = team.Id,
                Direction = TransferDirection.In,
                Price = 8.5
            };

            var response = await Client.PostAsJsonAsync("/api/transfer", input);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto = await response.Content.ReadFromJsonAsync<TransferDTO>();
            dto!.Id.Should().BeGreaterThan(0);
            dto.PlayerId.Should().Be(player.Id);
        }

        [Fact]
        public async Task Create_Returns400_WhenInvalid()
        {
            // PlayerId/TeamId = 0 (Range 1..) -> validacijska greška
            var invalid = new TransferInputDTO
            {
                PlayerId = 0,
                TeamId = 0,
                Direction = TransferDirection.In,
                Price = 5
            };

            var response = await Client.PostAsJsonAsync("/api/transfer", invalid);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_ChangesTransfer_WhenExists()
        {
            var (transferId, playerId, teamId) = await SeedTransferAsync();

            var input = new TransferInputDTO
            {
                PlayerId = playerId,
                TeamId = teamId,
                Direction = TransferDirection.Out,
                Price = 12.0
            };

            var response = await Client.PutAsJsonAsync($"/api/transfer/{transferId}", input);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<TransferDTO>();
            dto!.Direction.Should().Be("Out");
            dto.Price.Should().Be(12.0);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            var player = await SeedPlayerAsync();
            var team = await SeedTeamAsync();
            var input = new TransferInputDTO
            {
                PlayerId = player.Id,
                TeamId = team.Id,
                Direction = TransferDirection.In,
                Price = 5
            };

            var response = await Client.PutAsJsonAsync("/api/transfer/999999", input);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesTransfer_WhenExists()
        {
            var (transferId, _, _) = await SeedTransferAsync();

            var response = await Client.DeleteAsync($"/api/transfer/{transferId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            await WithDbAsync(async ctx =>
            {
                var exists = ctx.Transfers.Any(t => t.Id == transferId);
                exists.Should().BeFalse();
            });
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            var response = await Client.DeleteAsync("/api/transfer/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
