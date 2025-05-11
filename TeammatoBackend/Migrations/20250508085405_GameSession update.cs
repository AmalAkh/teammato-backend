using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeammatoBackend.Migrations
{
    /// <inheritdoc />
    public partial class GameSessionupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Langs",
                table: "GameSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "GameSessions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "GameSessions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Langs",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "GameSessions");
        }
    }
}
