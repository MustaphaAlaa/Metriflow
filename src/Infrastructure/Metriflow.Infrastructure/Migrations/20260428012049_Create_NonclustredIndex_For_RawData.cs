using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_NonclustredIndex_For_RawData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_PSIRecords_IsCorrelation",
                table: "PSIRecords",
                column: "IsCorrelation",
                filter: "[IsCorrelation] = 1")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_GARecords_IsCorrelation",
                table: "GARecords",
                column: "IsCorrelation",
                filter: "[IsCorrelation] = 1")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PSIRecords",
                table: "PSIRecords");

            migrationBuilder.DropIndex(
                name: "IX_PSIRecords_IsCorrelation",
                table: "PSIRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GARecords",
                table: "GARecords");

            migrationBuilder.DropIndex(
                name: "IX_GARecords_IsCorrelation",
                table: "GARecords");
        }
    }
}
