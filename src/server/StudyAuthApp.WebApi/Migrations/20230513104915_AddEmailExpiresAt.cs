using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAuthApp.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailVerificationTokeExpiresAt",
                table: "Users",
                newName: "EmailTokenExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailTokenExpiresAt",
                table: "Users",
                newName: "EmailVerificationTokeExpiresAt");
        }
    }
}
