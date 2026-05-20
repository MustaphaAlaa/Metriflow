using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Alter_PageAnalytics_to_Clustered_Columnstore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = """
                         CREATE CLUSTERED COLUMNSTORE INDEX CCI_PageAnalytics ON PageAnalytics;
                      """;
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = """ 
                      DROP INDEX CCI_PageAnalytics ON PageAnalytics; 
                      """;
            migrationBuilder.Sql(sql);
        }
    }
}
