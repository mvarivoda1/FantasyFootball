using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyFootball.Migrations
{
    /// <inheritdoc />
    public partial class AddGameweekSimulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bonus",
                table: "MatchPerformances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoalsConceded",
                table: "MatchPerformances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Saves",
                table: "MatchPerformances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Fixtures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameweekId = table.Column<int>(type: "int", nullable: false),
                    HomeClub = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AwayClub = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HomeGoals = table.Column<int>(type: "int", nullable: false),
                    AwayGoals = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixtures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fixtures_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameweekTeamScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FantasyTeamId = table.Column<int>(type: "int", nullable: false),
                    GameweekId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    LineupIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameweekTeamScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameweekTeamScores_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameweekTeamScores_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_GameweekId",
                table: "Fixtures",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_GameweekTeamScores_FantasyTeamId",
                table: "GameweekTeamScores",
                column: "FantasyTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_GameweekTeamScores_GameweekId",
                table: "GameweekTeamScores",
                column: "GameweekId");

            // Jednokratni reset sezone: obriši 6 seedanih kola i njihove učinke te
            // resetiraj sve akumulirane statistike igrača i timova na nulu. Bodovi
            // od sada rastu isključivo iz simuliranih kola. Cijene (MarketValue)
            // ostaju netaknute — DbSeeder.RefreshPricesFromMock pri startu postavlja
            // stvarne FPL cijene iz player-prices.md. Na praznoj bazi su ovo no-op-ovi.
            migrationBuilder.Sql("DELETE FROM [MatchPerformances];");
            migrationBuilder.Sql("DELETE FROM [Gameweeks];");
            migrationBuilder.Sql("UPDATE [Players] SET [TotalPoints] = 0, [Goals] = 0, [Assists] = 0, [CleanSheets] = 0;");
            migrationBuilder.Sql("UPDATE [FantasyTeams] SET [TotalPoints] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fixtures");

            migrationBuilder.DropTable(
                name: "GameweekTeamScores");

            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "MatchPerformances");

            migrationBuilder.DropColumn(
                name: "GoalsConceded",
                table: "MatchPerformances");

            migrationBuilder.DropColumn(
                name: "Saves",
                table: "MatchPerformances");
        }
    }
}
