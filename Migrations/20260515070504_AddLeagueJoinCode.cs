using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyFootball.Migrations
{
    /// <inheritdoc />
    public partial class AddLeagueJoinCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Season",
                table: "Leagues",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Leagues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Leagues",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CreatorUserId",
                table: "Leagues",
                type: "int",
                nullable: true);

            // 1) Prvo dodaj JoinCode kao nullable kako bi postojeći redovi mogli
            //    dobiti vrijednost prije nego što se postavi NOT NULL.
            migrationBuilder.AddColumn<string>(
                name: "JoinCode",
                table: "Leagues",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            // 2) Backfill — seedani redovi dobivaju fiksne kodove (usklađeno s
            //    LeagueMockRepository), ostatak dobiva slučajno generirani kod
            //    izveden iz NEWID()-a (uppercase, 6 znakova).
            migrationBuilder.Sql("UPDATE Leagues SET JoinCode = 'PREM01' WHERE Id = 1 AND JoinCode IS NULL;");
            migrationBuilder.Sql("UPDATE Leagues SET JoinCode = 'FRND02' WHERE Id = 2 AND JoinCode IS NULL;");
            migrationBuilder.Sql("UPDATE Leagues SET JoinCode = 'STUD03' WHERE Id = 3 AND JoinCode IS NULL;");
            migrationBuilder.Sql(@"
                UPDATE Leagues
                SET JoinCode = UPPER(LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 6))
                WHERE JoinCode IS NULL;");

            // 3) Sada se može postaviti NOT NULL constraint.
            migrationBuilder.AlterColumn<string>(
                name: "JoinCode",
                table: "Leagues",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);

            // 4) Unique index nakon što svi redovi imaju jedinstvenu vrijednost.
            migrationBuilder.CreateIndex(
                name: "IX_Leagues_JoinCode",
                table: "Leagues",
                column: "JoinCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leagues_JoinCode",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "JoinCode",
                table: "Leagues");

            migrationBuilder.AlterColumn<string>(
                name: "Season",
                table: "Leagues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Leagues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Leagues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
