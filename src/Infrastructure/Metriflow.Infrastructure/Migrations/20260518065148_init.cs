using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregateRecomputeQueue",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IntervalId = table.Column<int>(type: "int", nullable: false),
                    CeratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregateRecomputeQueue", x => new { x.PageId, x.Date, x.IntervalId });
                });

            migrationBuilder.CreateTable(
                name: "GARecords",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOnly = table.Column<DateOnly>(type: "date", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Users = table.Column<long>(type: "bigint", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    Sessions = table.Column<long>(type: "bigint", nullable: false),
                    IsCorrelation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Hash = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PSARecords",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOnly = table.Column<DateOnly>(type: "date", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    PerformanceScore = table.Column<int>(type: "int", nullable: false),
                    LCP_MS = table.Column<long>(type: "bigint", nullable: false),
                    IsCorrelation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Hash = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TableRowsCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableRowsCounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeIntervals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Interval = table.Column<byte>(type: "tinyint", nullable: false),
                    IntervalDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeIntervals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AggregationProgresses",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Interval = table.Column<bool>(type: "bit", nullable: false),
                    Correlation = table.Column<bool>(type: "bit", nullable: false),
                    Daily = table.Column<bool>(type: "bit", nullable: false),
                    Weekly = table.Column<bool>(type: "bit", nullable: false),
                    Monthly = table.Column<bool>(type: "bit", nullable: false),
                    Yearly = table.Column<bool>(type: "bit", nullable: false),
                    Quarterly = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregationProgress", x => new { x.PageId, x.Date });
                    table.ForeignKey(
                        name: "FK_AggregationProgresses_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyAnalytics",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAnalytics", x => new { x.PageId, x.Date });
                    table.ForeignKey(
                        name: "FK_DailyAnalytics_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyAnalytics",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "int", nullable: false),
                    YearMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyAnalytics", x => new { x.PageId, x.YearMonth });
                    table.ForeignKey(
                        name: "FK_MonthlyAnalytics_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearlyAnalytics",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearlyAnalytics", x => new { x.PageId, x.Year });
                    table.ForeignKey(
                        name: "FK_YearlyAnalytics_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageAnalytics",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOnly = table.Column<DateOnly>(type: "date", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Users = table.Column<long>(type: "bigint", nullable: false),
                    Sessions = table.Column<long>(type: "bigint", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    PerformanceScore = table.Column<double>(type: "float", nullable: false),
                    LcpMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PageAnalytics_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageAnalytics_TimeIntervals_Interval",
                        column: x => x.Interval,
                        principalTable: "TimeIntervals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeIntervalsAnalytics",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeIntervalId = table.Column<int>(type: "int", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeIntervalsAnalytics", x => new { x.PageId, x.Date, x.TimeIntervalId });
                    table.ForeignKey(
                        name: "FK_TimeIntervalsAnalytics_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeIntervalsAnalytics_TimeIntervals_TimeIntervalId",
                        column: x => x.TimeIntervalId,
                        principalTable: "TimeIntervals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Pages",
                columns: new[] { "Id", "Path" },
                values: new object[,]
                {
                    { 1, "home" },
                    { 2, "about" },
                    { 3, "contact" },
                    { 4, "books" },
                    { 5, "authors" },
                    { 6, "bestsellers" },
                    { 7, "highest_rate" },
                    { 8, "shop" },
                    { 9, "book" },
                    { 10, "historical_books" },
                    { 11, "fiction_books" },
                    { 12, "non_fiction_books" },
                    { 13, "dotnet_books" },
                    { 14, "javascript_books" },
                    { 15, "operating_system_books" },
                    { 16, "memory_management_books" },
                    { 17, "java_books" },
                    { 18, "software_engineering_books" },
                    { 19, "dotnet_5" },
                    { 20, "dotnet_6" },
                    { 21, "dotnet_7" },
                    { 22, "dotnet_8" },
                    { 23, "dotnet_9" },
                    { 24, "dotnet_10" }
                });

            migrationBuilder.InsertData(
                table: "TableRowsCounts",
                columns: new[] { "Id", "RowsCount", "TableName" },
                values: new object[,]
                {
                    { 1, 0, "GARecords" },
                    { 2, 0, "PSARecords" },
                    { 3, 0, "AggregationProgresses" },
                    { 4, 0, "PageAnalytics" },
                    { 5, 0, "TimeIntervalsAnalytics" },
                    { 6, 0, "DailyAnalytics" },
                    { 7, 0, "MonthlyAnalytics" },
                    { 8, 0, "YearlyAnalytics" }
                });

            migrationBuilder.InsertData(
                table: "TimeIntervals",
                columns: new[] { "Id", "Interval", "IntervalDescription" },
                values: new object[,]
                {
                    { 1, (byte)1, "12-hour: 12:00 AM – 3:59 AM | 24-hour: 00:00 – 03:59" },
                    { 2, (byte)2, "12-hour: 4:00 AM – 7:59 AM | 24-hour: 04:00 – 07:59" },
                    { 3, (byte)3, "12-hour: 8:00 AM – 11:59 AM | 24-hour: 08:00 – 11:59" },
                    { 4, (byte)4, "12-hour: 12:00 PM – 3:59 PM | 24-hour: 12:00 – 15:59" },
                    { 5, (byte)5, "12-hour: 4:00 PM – 7:59 PM | 24-hour: 16:00 – 19:59" },
                    { 6, (byte)6, "12-hour: 8:00 PM – 11:59 PM | 24-hour: 20:00 – 23:59" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_GARecords_IsCorrelation",
                table: "GARecords",
                column: "IsCorrelation",
                filter: "[IsCorrelation] = 1")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_GARecords_PageId_Date",
                table: "GARecords",
                columns: new[] { "PageId", "Date" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PageAnalytics_Interval",
                table: "PageAnalytics",
                column: "Interval");

            migrationBuilder.CreateIndex(
                name: "IX_PageAnalytics_ReAggregation",
                table: "PageAnalytics",
                columns: new[] { "PageId", "DateOnly", "Interval" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Path",
                table: "Pages",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PSARecords_IsCorrelation",
                table: "PSARecords",
                column: "IsCorrelation",
                filter: "[IsCorrelation] = 1")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PSARecords_PageId_Date",
                table: "PSARecords",
                columns: new[] { "PageId", "Date" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_TimeIntervals_Interval",
                table: "TimeIntervals",
                column: "Interval",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeIntervalsAnalytics_TimeIntervalId",
                table: "TimeIntervalsAnalytics",
                column: "TimeIntervalId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregateRecomputeQueue");

            migrationBuilder.DropTable(
                name: "AggregationProgresses");

            migrationBuilder.DropTable(
                name: "DailyAnalytics");

            migrationBuilder.DropTable(
                name: "GARecords");

            migrationBuilder.DropTable(
                name: "MonthlyAnalytics");

            migrationBuilder.DropTable(
                name: "PageAnalytics");

            migrationBuilder.DropTable(
                name: "PSARecords");

            migrationBuilder.DropTable(
                name: "TableRowsCounts");

            migrationBuilder.DropTable(
                name: "TimeIntervalsAnalytics");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "YearlyAnalytics");

            migrationBuilder.DropTable(
                name: "TimeIntervals");

            migrationBuilder.DropTable(
                name: "Pages");
        }
    }
}
