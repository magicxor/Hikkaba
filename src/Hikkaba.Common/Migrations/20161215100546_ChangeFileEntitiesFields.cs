using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hikkaba.Common.Migrations
{
    public partial class ChangeFileEntitiesFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailFilePath",
                table: "Video");

            migrationBuilder.DropColumn(
                name: "ThumbnailFilePath",
                table: "Pictures");

            migrationBuilder.DropColumn(
                name: "ThumbnailFilePath",
                table: "Audio");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailFilePath",
                table: "Video",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailFilePath",
                table: "Pictures",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailFilePath",
                table: "Audio",
                nullable: false,
                defaultValue: "");
        }
    }
}
