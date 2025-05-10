using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeammatoBackend.Migrations
{
    /// <inheritdoc />
    public partial class NewcolumnsforGameSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DurationFrom",
                table: "GameSessions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DurationTo",
                table: "GameSessions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationFrom",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "DurationTo",
                table: "GameSessions");
        }
    }
}
