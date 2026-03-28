using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Commerce.API_V9_.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionPropToCategorymodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Catgeories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Catgeories");
        }
    }
}
