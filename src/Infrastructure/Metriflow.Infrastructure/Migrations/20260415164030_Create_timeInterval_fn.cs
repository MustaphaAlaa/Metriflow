using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_timeInterval_fn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""

                                 CREATE OR REPLACE FUNCTION get_timeInterval(hour numeric)
                                  RETURNS int
                                  LANGUAGE plpgsql
                                  AS $$
                                  BEGIN
                                      IF hour < 4 THEN
                                          RETURN 1;
                                      ELSEIF hour < 8 THEN
                                          RETURN 2;
                                      ELSEIF hour < 12 THEN
                                          RETURN 3;
                                      ELSEIF hour < 16 THEN
                                          RETURN 4;
                                      ELSEIF hour < 20 THEN
                                          RETURN 5;
                                     ELSE
                                          RETURN 6;
                                      END IF;
                                  END
                                  $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
