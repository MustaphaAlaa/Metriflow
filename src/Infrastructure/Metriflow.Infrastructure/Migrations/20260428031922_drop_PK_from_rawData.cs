using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class drop_PK_from_rawData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PSIRecords",
                table: "PSIRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GARecords",
                table: "GARecords");

            migrationBuilder.CreateIndex(
                name: "IX_PSIRecords_PageId_Date",
                table: "PSIRecords",
                columns: new[] { "PageId", "Date" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_GARecords_PageId_Date",
                table: "GARecords",
                columns: new[] { "PageId", "Date" })
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PSIRecords_PageId_Date",
                table: "PSIRecords");

            migrationBuilder.DropIndex(
                name: "IX_GARecords_PageId_Date",
                table: "GARecords");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PSIRecords",
                table: "PSIRecords",
                columns: new[] { "PageId", "Date" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GARecords",
                table: "GARecords",
                columns: new[] { "PageId", "Date" })
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
