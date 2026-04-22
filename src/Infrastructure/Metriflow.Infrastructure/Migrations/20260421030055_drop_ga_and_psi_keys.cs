using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class drop_ga_and_psi_keys : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_PSIRecords",
                table: "PSIRecords",
                columns: new[] { "PageId", "Date" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GARecords",
                table: "GARecords",
                columns: new[] { "PageId", "Date" });
        }
    }
}
