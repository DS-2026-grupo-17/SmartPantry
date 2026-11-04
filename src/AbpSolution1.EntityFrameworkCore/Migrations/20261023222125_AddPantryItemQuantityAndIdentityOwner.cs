using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbpSolution1.Migrations
{
    /// <inheritdoc />
    public partial class AddPantryItemQuantityAndIdentityOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppPantries_AppUsers_OwnerId",
                table: "AppPantries");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "AppPantryItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "AppPantryItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "AppPantryItems");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "AppPantryItems");

            migrationBuilder.AddForeignKey(
                name: "FK_AppPantries_AppUsers_OwnerId",
                table: "AppPantries",
                column: "OwnerId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
