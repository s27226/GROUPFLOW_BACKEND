using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NAME_WIP_BACKEND.Migrations
{
    /// <inheritdoc />
    public partial class MakeProjectIdNullableForDirectMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Projects_ProjectId",
                table: "Chats");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Chats",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Projects_ProjectId",
                table: "Chats",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Projects_ProjectId",
                table: "Chats");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Chats",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Projects_ProjectId",
                table: "Chats",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
