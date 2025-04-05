using System;
using System.Globalization;
using System.IO;
using Hikkaba.Data.Utils;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hikkaba.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostMessageFulltextIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = MigrationUtility.ReadSql(typeof(AddPostMessageFulltextIndex), "20250405100526_AddPostMessageFulltextIndex.Up.sql");
            migrationBuilder.Sql(sql, suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = MigrationUtility.ReadSql(typeof(AddPostMessageFulltextIndex), "20250405100526_AddPostMessageFulltextIndex.Down.sql");
            migrationBuilder.Sql(sql, suppressTransaction: true);
        }
    }
}
