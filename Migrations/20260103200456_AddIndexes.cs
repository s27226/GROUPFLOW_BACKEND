using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupFlow_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectLikes_ProjectId",
                table: "ProjectLikes");

            migrationBuilder.DropIndex(
                name: "IX_PostCommentLikes_PostCommentId",
                table: "PostCommentLikes");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_RequesterId",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRecommendations_RecommendedForId",
                table: "FriendRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_BlobFiles_PostId",
                table: "BlobFiles");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Nickname",
                table: "Users",
                column: "Nickname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLikes_ProjectId_UserId",
                table: "ProjectLikes",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Created",
                table: "Posts",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_PostCommentLikes_PostCommentId_UserId",
                table: "PostCommentLikes",
                columns: new[] { "PostCommentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships",
                columns: new[] { "UserId", "FriendId" });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_RequesterId_RequesteeId",
                table: "FriendRequests",
                columns: new[] { "RequesterId", "RequesteeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendRecommendations_RecommendedForId_RecommendedWhoId",
                table: "FriendRecommendations",
                columns: new[] { "RecommendedForId", "RecommendedWhoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlobFiles_PostId",
                table: "BlobFiles",
                column: "PostId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Nickname",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Name",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectLikes_ProjectId_UserId",
                table: "ProjectLikes");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Created",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_PostCommentLikes_PostCommentId_UserId",
                table: "PostCommentLikes");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_RequesterId_RequesteeId",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRecommendations_RecommendedForId_RecommendedWhoId",
                table: "FriendRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_BlobFiles_PostId",
                table: "BlobFiles");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLikes_ProjectId",
                table: "ProjectLikes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PostCommentLikes_PostCommentId",
                table: "PostCommentLikes",
                column: "PostCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId",
                table: "Friendships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_RequesterId",
                table: "FriendRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRecommendations_RecommendedForId",
                table: "FriendRecommendations",
                column: "RecommendedForId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobFiles_PostId",
                table: "BlobFiles",
                column: "PostId");
        }
    }
}
