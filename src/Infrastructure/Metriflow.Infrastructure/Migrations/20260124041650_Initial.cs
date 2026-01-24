using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GARecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ticks = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    Users = table.Column<long>(type: "bigint", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    Sessions = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GARecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Path = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PSIRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ticks = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    PerformanceScore = table.Column<int>(type: "integer", nullable: false),
                    LCP_MS = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PSIRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeIntervals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Interval = table.Column<byte>(type: "smallint", nullable: false),
                    IntervalDescription = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeIntervals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AggregationProgresses",
                columns: table => new
                {
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Interval = table.Column<bool>(type: "boolean", nullable: false),
                    Correlation = table.Column<bool>(type: "boolean", nullable: false),
                    Daily = table.Column<bool>(type: "boolean", nullable: false),
                    Weekly = table.Column<bool>(type: "boolean", nullable: false),
                    Monthly = table.Column<bool>(type: "boolean", nullable: false),
                    Yearly = table.Column<bool>(type: "boolean", nullable: false),
                    Quarterly = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAnalytics", x => x.Id);
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearMonth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyAnalytics", x => x.Id);
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearlyAnalytics", x => x.Id);
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Intervals = table.Column<int>(type: "integer", nullable: false),
                    Users = table.Column<long>(type: "bigint", nullable: false),
                    Sessions = table.Column<long>(type: "bigint", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    PerformanceScore = table.Column<double>(type: "double precision", nullable: false),
                    LcpMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageAnalytics_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageAnalytics_TimeIntervals_Intervals",
                        column: x => x.Intervals,
                        principalTable: "TimeIntervals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeIntervalsAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeIntervalId = table.Column<int>(type: "integer", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeIntervalsAnalytics", x => x.Id);
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
                name: "IX_DailyAnalytics_Date",
                table: "DailyAnalytics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAnalytics_PageId",
                table: "DailyAnalytics",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_GARecords_Ticks_PageId",
                table: "GARecords",
                columns: new[] { "Ticks", "PageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyAnalytics_PageId_YearMonth",
                table: "MonthlyAnalytics",
                columns: new[] { "PageId", "YearMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_PageAnalytics_Intervals",
                table: "PageAnalytics",
                column: "Intervals");

            migrationBuilder.CreateIndex(
                name: "IX_PageAnalytics_PageId_Date",
                table: "PageAnalytics",
                columns: new[] { "PageId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Path",
                table: "Pages",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PSIRecords_Ticks_PageId",
                table: "PSIRecords",
                columns: new[] { "Ticks", "PageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeIntervals_Interval",
                table: "TimeIntervals",
                column: "Interval",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeIntervalsAnalytics_PageId",
                table: "TimeIntervalsAnalytics",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeIntervalsAnalytics_TimeIntervalId",
                table: "TimeIntervalsAnalytics",
                column: "TimeIntervalId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyAnalytics_PageId_Year",
                table: "YearlyAnalytics",
                columns: new[] { "PageId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "PSIRecords");

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
