using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyFootball.Migrations
{
    /// <inheritdoc />
    public partial class AddFreeTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreeTransfers",
                table: "FantasyTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransferPointHits",
                table: "FantasyTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeTransfers",
                table: "FantasyTeams");

            migrationBuilder.DropColumn(
                name: "TransferPointHits",
                table: "FantasyTeams");
        }
    }
}
