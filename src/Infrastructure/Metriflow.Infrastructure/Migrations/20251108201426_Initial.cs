using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalUsers = table.Column<long>(type: "bigint", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyStats", x => x.Id);
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
                name: "MonthlyAnalytic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearMonth = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    TotalUsers = table.Column<int>(type: "int", nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    TotalViews = table.Column<int>(type: "int", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyStat", x => x.Id);
                    table.UniqueConstraint("AK_MonthlyStat_PageMonth", x => new { x.PageId, x.YearMonth });
                    table.ForeignKey(
                        name: "FK_MonthlyStat_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Users = table.Column<long>(type: "bigint", nullable: false),
                    Sessions = table.Column<long>(type: "bigint", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    PerformanceScore = table.Column<double>(type: "float", nullable: false),
                    LCP_ms = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawDatas_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearlyAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    TotalUsers = table.Column<int>(type: "int", nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    TotalViews = table.Column<int>(type: "int", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearlyStat", x => x.Id);
                    table.UniqueConstraint("AK_YearlyStats_PageYear", x => new { x.PageId, x.Year });
                    table.ForeignKey(
                        name: "FK_YearlyStat_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyStats_Date",
                table: "DailyAnalytics",
                column: "Ticks");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyStat_YearMonth",
                table: "MonthlyAnalytic",
                column: "YearMonth");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Path",
                table: "Pages",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawDatas_PageId",
                table: "PageAnalytics",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyStat_Year",
                table: "YearlyAnalytics",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyAnalytics");

            migrationBuilder.DropTable(
                name: "MonthlyAnalytic");

            migrationBuilder.DropTable(
                name: "PageAnalytics");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "YearlyAnalytics");

            migrationBuilder.DropTable(
                name: "Pages");
        }
    }
}
