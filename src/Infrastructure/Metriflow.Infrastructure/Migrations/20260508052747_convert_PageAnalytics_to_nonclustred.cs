using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class convert_PageAnalytics_to_nonclustred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PageAnalytics",
                table: "PageAnalytics");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PageAnalytics",
                table: "PageAnalytics",
                columns: new[] { "PageId", "Date", "Intervals" })
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PageAnalytics",
                table: "PageAnalytics");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PageAnalytics",
                table: "PageAnalytics",
                columns: new[] { "PageId", "Date", "Intervals" });
        }
    }
}
