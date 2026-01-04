using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupFlow_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerPicToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerPic",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerPic",
                table: "Users");
        }
    }
}
