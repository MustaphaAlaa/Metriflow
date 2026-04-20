using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class create_hypertables : Migration
    {
        /// <inheritdoc />
 

          protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                     SELECT create_hypertable(
                                    '"AggregationProgresses"'::regclass,
                                         'Date'::name,
                                         if_not_exists => TRUE
                                     );
                                 """);

            migrationBuilder.Sql("""
                                     ALTER TABLE "AggregationProgresses"
                                     SET (
                                         timescaledb.enable_columnstore = true,
                                         timescaledb.segmentby = '"PageId"',
                                         timescaledb.orderby = '"Date"'
                                     );
                                 """);

            migrationBuilder.Sql("""
                                     CALL add_columnstore_policy(
                                      '"AggregationProgresses"'::regclass,
                                         after => INTERVAL '3 months'
                                     );
                                 """);
            
            //GA
            migrationBuilder.Sql("""
                                     SELECT create_hypertable(
                                    '"GARecords"'::regclass,
                                         'Date'::name,
                                         if_not_exists => TRUE
                                     );
                                 """);

            migrationBuilder.Sql("""
                                     ALTER TABLE "GARecords"
                                     SET (
                                         timescaledb.enable_columnstore = true,
                                         timescaledb.segmentby = '"PageId"',
                                         timescaledb.orderby = '"Date"'
                                     );
                                 """);

            migrationBuilder.Sql("""
                                     CALL add_columnstore_policy(
                                      '"GARecords"'::regclass,
                                         after => INTERVAL '3 months'
                                     );
                                 """);
            //PSI
            migrationBuilder.Sql("""
                                     SELECT create_hypertable(
                                    '"PSIRecords"'::regclass,
                                         'Date'::name,
                                         if_not_exists => TRUE
                                     );
                                 """);

            migrationBuilder.Sql("""
                                     ALTER TABLE "PSIRecords"
                                     SET (
                                         timescaledb.enable_columnstore = true,
                                         timescaledb.segmentby = '"PageId"',
                                         timescaledb.orderby = '"Date"'
                                     );
                                 """);

            migrationBuilder.Sql("""
                                     CALL add_columnstore_policy(
                                      '"PSIRecords"'::regclass,
                                 after => INTERVAL '3 months'
                                     );
                                 """);
                                //PageAnalytics
                                migrationBuilder.Sql("""
                                     SELECT create_hypertable(
                                    '"PageAnalytics"'::regclass,
                                         'Date'::name,
                                         if_not_exists => TRUE
                                     );
                                 """);

                                 migrationBuilder.Sql("""
                                     ALTER TABLE "PageAnalytics"
                                     SET (
                                         timescaledb.enable_columnstore = true,
                                         timescaledb.segmentby = '"PageId"',
                                         timescaledb.orderby = '"Date"'
                                     );
                                 """);

                                  migrationBuilder.Sql("""
                                     CALL add_columnstore_policy(
                                      '"PageAnalytics"'::regclass,
                                         after => INTERVAL '3 months'
                                     );
                                 """);
        }




        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
