using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeammatoBackend.Migrations
{
    /// <inheritdoc />
    public partial class LangsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Language_Users_UserId",
                table: "Language");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Language",
                table: "Language");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Language");

            migrationBuilder.RenameTable(
                name: "Language",
                newName: "Languages");

            migrationBuilder.RenameIndex(
                name: "IX_Language_UserId",
                table: "Languages",
                newName: "IX_Languages_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Languages",
                table: "Languages",
                columns: new[] { "ISOName", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Languages_Users_UserId",
                table: "Languages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Languages_Users_UserId",
                table: "Languages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Languages",
                table: "Languages");

            migrationBuilder.RenameTable(
                name: "Languages",
                newName: "Language");

            migrationBuilder.RenameIndex(
                name: "IX_Languages_UserId",
                table: "Language",
                newName: "IX_Language_UserId");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Language",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Language",
                table: "Language",
                columns: new[] { "ISOName", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Language_Users_UserId",
                table: "Language",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
