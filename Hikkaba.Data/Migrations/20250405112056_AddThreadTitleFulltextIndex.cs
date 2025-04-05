using Hikkaba.Data.Utils;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hikkaba.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThreadTitleFulltextIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = MigrationUtility.ReadSql(typeof(AddPostMessageFulltextIndex), "20250405112056_AddThreadTitleFulltextIndex.Up.sql");
            migrationBuilder.Sql(sql, suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = MigrationUtility.ReadSql(typeof(AddPostMessageFulltextIndex), "20250405112056_AddThreadTitleFulltextIndex.Down.sql");
            migrationBuilder.Sql(sql, suppressTransaction: true);
        }
    }
}
