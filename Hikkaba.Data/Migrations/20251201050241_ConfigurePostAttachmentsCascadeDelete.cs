using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hikkaba.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurePostAttachmentsCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Posts_PostId",
                table: "Attachments");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Posts_PostId",
                table: "Attachments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Posts_PostId",
                table: "Attachments");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Posts_PostId",
                table: "Attachments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }
    }
}
