using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Page",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Page", x => x.Id);
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
                name: "DailyStat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    TotalUsers = table.Column<int>(type: "int", nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    TotalViews = table.Column<int>(type: "int", nullable: false),
                    AvgPerformance = table.Column<double>(type: "float", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyStat", x => x.Id);
                    table.UniqueConstraint("AK_DailyStats_PageDay", x => new { x.PageId, x.Date });
                    table.ForeignKey(
                        name: "FK_DailyStat_Page_PageId",
                        column: x => x.PageId,
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyStat",
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
                        name: "FK_MonthlyStat_Page_PageId",
                        column: x => x.PageId,
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearlyStat",
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
                        name: "FK_YearlyStat_Page_PageId",
                        column: x => x.PageId,
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyStat_Date",
                table: "DailyStat",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyStat_YearMonth",
                table: "MonthlyStat",
                column: "YearMonth");

            migrationBuilder.CreateIndex(
                name: "IX_Page_Path",
                table: "Page",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyStat_Year",
                table: "YearlyStat",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyStat");

            migrationBuilder.DropTable(
                name: "MonthlyStat");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "YearlyStat");

            migrationBuilder.DropTable(
                name: "Page");
        }
    }
}
