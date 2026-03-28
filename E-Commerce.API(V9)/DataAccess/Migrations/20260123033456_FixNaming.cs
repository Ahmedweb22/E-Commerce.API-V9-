using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Commerce.API_V9_.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Catgeories_CatgeoryId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Catgeories",
                table: "Catgeories");

            migrationBuilder.RenameTable(
                name: "Catgeories",
                newName: "Categories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CatgeoryId",
                table: "Products",
                column: "CatgeoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CatgeoryId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Catgeories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Catgeories",
                table: "Catgeories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Catgeories_CatgeoryId",
                table: "Products",
                column: "CatgeoryId",
                principalTable: "Catgeories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
