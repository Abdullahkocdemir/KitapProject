using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitapProject.Migrations
{
    /// <inheritdoc />
    public partial class Mig_Image : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageURl",
                table: "Products",
                newName: "ImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Products",
                newName: "ImageURl");
        }
    }
}
