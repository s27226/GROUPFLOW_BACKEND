using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupFlow_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class RenameImageUrlToImageAndAddBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Projects",
                newName: "Image");

            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banner",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Projects",
                newName: "ImageUrl");
        }
    }
}
