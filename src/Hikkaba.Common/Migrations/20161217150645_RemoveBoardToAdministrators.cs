using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hikkaba.Common.Migrations
{
    public partial class RemoveBoardToAdministrators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardsToAdministrators");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Bans",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bans_CategoryId",
                table: "Bans",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bans_Categories_CategoryId",
                table: "Bans",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bans_Categories_CategoryId",
                table: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Bans_CategoryId",
                table: "Bans");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Bans");

            migrationBuilder.CreateTable(
                name: "BoardsToAdministrators",
                columns: table => new
                {
                    BoardId = table.Column<Guid>(nullable: false),
                    ApplicationUserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardsToAdministrators", x => new { x.BoardId, x.ApplicationUserId });
                    table.ForeignKey(
                        name: "FK_BoardsToAdministrators_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardsToAdministrators_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardsToAdministrators_ApplicationUserId",
                table: "BoardsToAdministrators",
                column: "ApplicationUserId");
        }
    }
}
