using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Staged_tables_for_RawData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            var gaStaged ="""

                          CREATE TABLE GARecords_staged
                          (
                              Date          datetime2 not null,
                              PageId        int,
                              Interval      int,
                              Users         bigint,
                              Views         bigint,
                              Sessions      bigint,
                              IsCorrelation bit,
                              INDEX CCI_GARecords_staged CLUSTERED COLUMNSTORE
                          );
                          """ ;
            migrationBuilder.Sql(gaStaged);
            var psaStaged ="""


                           CREATE TABLE PSARecords_staged
                           (
                               Date             datetime2   not null,
                               PageId           int    not null,
                               Interval         int    not null,
                               PerformanceScore int    not null,
                               LCP_MS           bigint not null,
                               IsCorrelation    bit    not null,
                               INDEX CCI_PSARecords_staged CLUSTERED COLUMNSTORE
                           );
                           """ ;
            migrationBuilder.Sql(psaStaged);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var psiStaged = "DROP TABLE PSARecords_staged";
            var gaStaged = "DROP TABLE GARecords_staged";
            migrationBuilder.Sql(psiStaged);
            migrationBuilder.Sql(gaStaged);
        }
    }
}
