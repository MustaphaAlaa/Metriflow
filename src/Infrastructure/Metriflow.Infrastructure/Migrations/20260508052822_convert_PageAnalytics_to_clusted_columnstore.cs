using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class convert_PageAnalytics_to_clusted_columnstore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF might try to create a standard index here; replace it with this:
            migrationBuilder.Sql(
                "CREATE CLUSTERED COLUMNSTORE INDEX CCI_PageAnalytics ON PageAnalytics;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX CCI_PageAnalytics ON PageAnalytics;");
        }
    }
}
