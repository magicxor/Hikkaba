using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hikkaba.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentFileContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileContentType",
                table: "Attachments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContentType",
                table: "Attachments");
        }
    }
}
