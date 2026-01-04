using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GroupFlow_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class AddBlobStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BannerPicBlobId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfilePicBlobId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BannerBlobId",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageBlobId",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageBlobId",
                table: "Posts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlobFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    BlobPath = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: true),
                    PostId = table.Column<int>(type: "integer", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlobFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlobFiles_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlobFiles_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlobFiles_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_BannerPicBlobId",
                table: "Users",
                column: "BannerPicBlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProfilePicBlobId",
                table: "Users",
                column: "ProfilePicBlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BannerBlobId",
                table: "Projects",
                column: "BannerBlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ImageBlobId",
                table: "Projects",
                column: "ImageBlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ImageBlobId",
                table: "Posts",
                column: "ImageBlobId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobFiles_PostId",
                table: "BlobFiles",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobFiles_ProjectId",
                table: "BlobFiles",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobFiles_UploadedByUserId",
                table: "BlobFiles",
                column: "UploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_BlobFiles_ImageBlobId",
                table: "Posts",
                column: "ImageBlobId",
                principalTable: "BlobFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_BlobFiles_BannerBlobId",
                table: "Projects",
                column: "BannerBlobId",
                principalTable: "BlobFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_BlobFiles_ImageBlobId",
                table: "Projects",
                column: "ImageBlobId",
                principalTable: "BlobFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_BlobFiles_BannerPicBlobId",
                table: "Users",
                column: "BannerPicBlobId",
                principalTable: "BlobFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_BlobFiles_ProfilePicBlobId",
                table: "Users",
                column: "ProfilePicBlobId",
                principalTable: "BlobFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_BlobFiles_ImageBlobId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_BlobFiles_BannerBlobId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_BlobFiles_ImageBlobId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_BlobFiles_BannerPicBlobId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_BlobFiles_ProfilePicBlobId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "BlobFiles");

            migrationBuilder.DropIndex(
                name: "IX_Users_BannerPicBlobId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProfilePicBlobId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Projects_BannerBlobId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ImageBlobId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ImageBlobId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "BannerPicBlobId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePicBlobId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BannerBlobId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ImageBlobId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ImageBlobId",
                table: "Posts");
        }
    }
}
