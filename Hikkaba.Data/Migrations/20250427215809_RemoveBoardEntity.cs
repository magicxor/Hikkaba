using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hikkaba.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBoardEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Boards_BoardId",
                table: "Categories");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Categories_BoardId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoardId",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BoardId",
                table: "Categories",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Boards_BoardId",
                table: "Categories",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
