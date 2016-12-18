using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hikkaba.Common.Migrations
{
    public partial class RemoveUnusedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClosedForMaintenance",
                table: "Boards");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Posts",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 8000);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Posts",
                maxLength: 8000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 8000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosedForMaintenance",
                table: "Boards",
                nullable: false,
                defaultValue: false);
        }
    }
}
