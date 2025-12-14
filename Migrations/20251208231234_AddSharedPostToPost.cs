using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NAME_WIP_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedPostToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SharedPostId",
                table: "Posts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SharedPostId",
                table: "Posts",
                column: "SharedPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Posts_SharedPostId",
                table: "Posts",
                column: "SharedPostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Posts_SharedPostId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_SharedPostId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SharedPostId",
                table: "Posts");
        }
    }
}
