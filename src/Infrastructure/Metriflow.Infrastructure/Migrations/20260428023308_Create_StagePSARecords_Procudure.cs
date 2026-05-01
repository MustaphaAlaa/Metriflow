using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_StagePSARecords_Procudure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                var stagedPsaProcedure = """
                                        CREATE PROCEDURE StagePSARecords
                                        AS
                                        BEGIN
                                            SET NOCOUNT ON;
                                        
                                            BEGIN TRY
                                                BEGIN TRANSACTION;
                                        
                                                WITH Batch AS (
                                                    SELECT TOP (200000)
                                                        Date,
                                                        PageId,
                                                        PerformanceScore,
                                                        LCP_MS,
                                                        IsCorrelation
                                                    FROM dbo.PSIRecords WITH (UPDLOCK, READPAST)
                                                    WHERE IsCorrelation = 0
                                                    ORDER BY Date
                                                )
                                                UPDATE Batch
                                                SET IsCorrelation = 1
                                                OUTPUT
                                                   inserted.Date,
                                                    inserted.PageId,
                                                    CASE
                                                        WHEN DATEPART(HOUR, inserted.Date) < 4 THEN 1
                                                        WHEN DATEPART(HOUR, inserted.Date) < 8 THEN 2
                                                        WHEN DATEPART(HOUR, inserted.Date) < 12 THEN 3
                                                        WHEN DATEPART(HOUR, inserted.Date) < 16 THEN 4
                                                        WHEN DATEPART(HOUR, inserted.Date) < 20 THEN 5
                                                        ELSE 6
                                                    END,
                                                    inserted.PerformanceScore,
                                                    inserted.LCP_MS,
                                                    inserted.IsCorrelation
                                                INTO dbo.PSARecords_staged
                                                    (Date, PageId, Interval, PerformanceScore, LCP_MS, IsCorrelation);
                                        
                                                COMMIT;
                                            END TRY
                                            BEGIN CATCH
                                                ROLLBACK;
                                                THROW;
                                            END CATCH
                                        END;
                                        """;
                migrationBuilder.Sql(stagedPsaProcedure);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE StagePSARecords");

        }
    }
}
