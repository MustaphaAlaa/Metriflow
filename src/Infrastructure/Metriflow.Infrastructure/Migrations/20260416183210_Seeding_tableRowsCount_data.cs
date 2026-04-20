using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Seeding_tableRowsCount_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TableRowsCounts",
                columns: new[] { "Id", "RowsCount", "TableName" },
                values: new object[,]
                {
                    { 1, 0, "GARecords" },
                    { 2, 0, "PSIRecords" },
                    { 3, 0, "AggregationProgresses" },
                    { 4, 0, "PageAnalytics" },
                    { 5, 0, "TimeIntervalsAnalytics" },
                    { 6, 0, "DailyAnalytics" },
                    { 7, 0, "MonthlyAnalytics" },
                    { 8, 0, "YearlyAnalytics" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TableRowsCounts",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
