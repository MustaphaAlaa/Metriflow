using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class alter_indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_Correlation",
                table: "AggregationProgresses",
                column: "Correlation");

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_Daily",
                table: "AggregationProgresses",
                column: "Daily");

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_Interval",
                table: "AggregationProgresses",
                column: "Interval");

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_Monthly",
                table: "AggregationProgresses",
                column: "Monthly");

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_Quarterly",
                table: "AggregationProgresses",
                column: "Quarterly");

            migrationBuilder.CreateIndex(
                name: "IX_AggregationProgresses_Yearly",
                table: "AggregationProgresses",
                column: "Yearly");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_Correlation",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_Daily",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_Interval",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_Monthly",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_Quarterly",
                table: "AggregationProgresses");

            migrationBuilder.DropIndex(
                name: "IX_AggregationProgresses_Yearly",
                table: "AggregationProgresses");

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
    }
}
