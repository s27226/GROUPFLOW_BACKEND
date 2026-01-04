using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NAME_WIP_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadBys_UserId",
                table: "ReadBys");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRecommendations_UserId",
                table: "ProjectRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_ProjectInvitations_ProjectId",
                table: "ProjectInvitations");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_UserId",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_EntryReactions_UserId",
                table: "EntryReactions");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UserId_ProjectId",
                table: "UserProjects",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedPosts_UserId_PostId",
                table: "SavedPosts",
                columns: new[] { "UserId", "PostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadBys_UserId_EntryId",
                table: "ReadBys",
                columns: new[] { "UserId", "EntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectViews_UserId_ProjectId",
                table: "ProjectViews",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRecommendations_UserId_ProjectId",
                table: "ProjectRecommendations",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ProjectId_InvitingId_InvitedId",
                table: "ProjectInvitations",
                columns: new[] { "ProjectId", "InvitingId", "InvitedId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_UserId_PostId",
                table: "PostLikes",
                columns: new[] { "UserId", "PostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntryReactions_UserId_EmoteId_EntryId",
                table: "EntryReactions",
                columns: new[] { "UserId", "EmoteId", "EntryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProjects_UserId_ProjectId",
                table: "UserProjects");

            migrationBuilder.DropIndex(
                name: "IX_SavedPosts_UserId_PostId",
                table: "SavedPosts");

            migrationBuilder.DropIndex(
                name: "IX_ReadBys_UserId_EntryId",
                table: "ReadBys");

            migrationBuilder.DropIndex(
                name: "IX_ProjectViews_UserId_ProjectId",
                table: "ProjectViews");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRecommendations_UserId_ProjectId",
                table: "ProjectRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_ProjectInvitations_ProjectId_InvitingId_InvitedId",
                table: "ProjectInvitations");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_UserId_PostId",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_EntryReactions_UserId_EmoteId_EntryId",
                table: "EntryReactions");

            migrationBuilder.CreateIndex(
                name: "IX_ReadBys_UserId",
                table: "ReadBys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRecommendations_UserId",
                table: "ProjectRecommendations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ProjectId",
                table: "ProjectInvitations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_UserId",
                table: "PostLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryReactions_UserId",
                table: "EntryReactions",
                column: "UserId");
        }
    }
}
