using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeammatoBackend.Migrations
{
    /// <inheritdoc />
    public partial class Newprimarykeyforfavoritegames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteGames",
                table: "FavoriteGames");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteGames",
                table: "FavoriteGames",
                columns: new[] { "GameId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteGames_UserId",
                table: "FavoriteGames",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteGames",
                table: "FavoriteGames");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteGames_UserId",
                table: "FavoriteGames");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteGames",
                table: "FavoriteGames",
                column: "UserId");
        }
    }
}
