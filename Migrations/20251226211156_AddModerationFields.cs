using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NAME_WIP_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_BannedByUserId",
                table: "Users",
                column: "BannedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users",
                column: "BannedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_BannedByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BannedByUserId",
                table: "Users");
        }
    }
}
