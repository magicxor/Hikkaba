using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hikkaba.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThreadOriginalPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalPostId",
                table: "Threads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ThreadId1",
                table: "Posts",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ThreadId1",
                table: "Posts",
                column: "ThreadId1",
                unique: true,
                filter: "[ThreadId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Threads_ThreadId1",
                table: "Posts",
                column: "ThreadId1",
                principalTable: "Threads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Threads_ThreadId1",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ThreadId1",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "OriginalPostId",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "ThreadId1",
                table: "Posts");
        }
    }
}
