using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hikkaba.Common.Migrations
{
    public partial class ChangeFileStorageRelatedEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Video",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Pictures",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Documents",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Audio",
                newName: "FileName");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Bans",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Video",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Pictures",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Documents",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Audio",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Video",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Pictures",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Documents",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Audio",
                newName: "FilePath");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Bans",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Video",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Pictures",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Documents",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Audio",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
