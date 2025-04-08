using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hikkaba.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryShowOsShowBrowser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShowUserAgent",
                table: "Categories",
                newName: "ShowOs");

            migrationBuilder.AddColumn<bool>(
                name: "ShowBrowser",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowBrowser",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "ShowOs",
                table: "Categories",
                newName: "ShowUserAgent");
        }
    }
}
