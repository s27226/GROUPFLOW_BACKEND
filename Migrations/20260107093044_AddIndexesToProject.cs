using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NAME_WIP_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_Created",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsPublic",
                table: "Projects",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsPublic_Created",
                table: "Projects",
                columns: new[] { "IsPublic", "Created" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_LastUpdated",
                table: "Projects",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Created",
                table: "Posts",
                column: "Created",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Public_Created",
                table: "Posts",
                columns: new[] { "Public", "Created" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships",
                columns: new[] { "UserId", "FriendId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_Sent",
                table: "Entries",
                column: "Sent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_IsPublic",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_IsPublic_Created",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_LastUpdated",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Created",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Public_Created",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Entries_Sent",
                table: "Entries");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Created",
                table: "Posts",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships",
                columns: new[] { "UserId", "FriendId" });
        }
    }
}
