using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class create_AggregationsProgresses_indexes__create_TableRowsCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableRowsCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    RowsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableRowsCounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_PageId_Date_Correlation",
                table: "AggregationProgresses",
                columns: new[] { "PageId", "Date", "Correlation" });

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_PageId_Date_Daily",
                table: "AggregationProgresses",
                columns: new[] { "PageId", "Date", "Daily" });

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_PageId_Date_Interval",
                table: "AggregationProgresses",
                columns: new[] { "PageId", "Date", "Interval" });

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_PageId_Date_Monthly",
                table: "AggregationProgresses",
                columns: new[] { "PageId", "Date", "Monthly" });

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_PageId_Date_Quarterly",
                table: "AggregationProgresses",
                columns: new[] { "PageId", "Date", "Quarterly" });

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_PageId_Date_Yearly",
                table: "AggregationProgresses",
                columns: new[] { "PageId", "Date", "Yearly" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableRowsCounts");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_PageId_Date_Correlation",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_PageId_Date_Daily",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_PageId_Date_Interval",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_PageId_Date_Monthly",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_PageId_Date_Quarterly",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_PageId_Date_Yearly",
                table: "AggregationProgresses");
        }
    }
}
