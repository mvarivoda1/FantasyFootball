using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyFootball.Migrations
{
    /// <inheritdoc />
    public partial class AddStartingLineupIdsToFantasyTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StartingLineupIds",
                table: "FantasyTeams",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartingLineupIds",
                table: "FantasyTeams");
        }
    }
}
