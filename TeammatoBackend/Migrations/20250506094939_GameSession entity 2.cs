using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeammatoBackend.Migrations
{
    /// <inheritdoc />
    public partial class GameSessionentity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GameSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GameId",
                table: "GameSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GameName",
                table: "GameSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "GameSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "RequiredPlayersCount",
                table: "GameSessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "GameName",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "RequiredPlayersCount",
                table: "GameSessions");
        }
    }
}
