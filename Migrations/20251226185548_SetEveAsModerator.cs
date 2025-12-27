using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NAME_WIP_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class SetEveAsModerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update Eve to be a moderator
            migrationBuilder.Sql(
                @"UPDATE ""Users"" 
                  SET ""IsModerator"" = true 
                  WHERE ""Email"" = 'eve@example.com';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Eve's moderator status
            migrationBuilder.Sql(
                @"UPDATE ""Users"" 
                  SET ""IsModerator"" = false 
                  WHERE ""Email"" = 'eve@example.com';");
        }
    }
}
