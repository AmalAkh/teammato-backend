using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeammatoBackend.Migrations
{
    /// <inheritdoc />
    public partial class DurationfieldcolumnforGameSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationFrom",
                table: "GameSessions");

            migrationBuilder.RenameColumn(
                name: "DurationTo",
                table: "GameSessions",
                newName: "Duration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "GameSessions",
                newName: "DurationTo");

            migrationBuilder.AddColumn<double>(
                name: "DurationFrom",
                table: "GameSessions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
