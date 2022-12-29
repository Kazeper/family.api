using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace family.api.Migrations
{
    /// <inheritdoc />
    public partial class fkUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_Users_FKUser",
                table: "PageItems");

            migrationBuilder.DropIndex(
                name: "IX_PageItems_FKUser",
                table: "PageItems");

            migrationBuilder.DropColumn(
                name: "FKUser",
                table: "PageItems");

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_UserId",
                table: "PageItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_Users_UserId",
                table: "PageItems",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_Users_UserId",
                table: "PageItems");

            migrationBuilder.DropIndex(
                name: "IX_PageItems_UserId",
                table: "PageItems");

            migrationBuilder.AddColumn<int>(
                name: "FKUser",
                table: "PageItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_FKUser",
                table: "PageItems",
                column: "FKUser");

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_Users_FKUser",
                table: "PageItems",
                column: "FKUser",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
