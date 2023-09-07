using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAuthApp.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Relations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ResetTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResetTokens_UserId",
                table: "ResetTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetTokens_Users_UserId",
                table: "ResetTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResetTokens_Users_UserId",
                table: "ResetTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_ResetTokens_UserId",
                table: "ResetTokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ResetTokens");
        }
    }
}
