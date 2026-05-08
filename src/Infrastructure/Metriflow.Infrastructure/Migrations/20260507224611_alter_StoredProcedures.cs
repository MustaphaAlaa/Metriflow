using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class alter_StoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var stagedGAProcedure = """
                alter PROCEDURE StageGARecords
                    @BatchSize INT
                AS
                BEGIN
                     SET NOCOUNT ON;

                    BEGIN TRY

                        BEGIN TRANSACTION;

                        -- Step 1: Extract batch into temp table
                        SELECT TOP (@BatchSize)
                            Date,
                            PageId,
                            Users,
                            Views,
                            Sessions,
                            Hash,
                            IsCorrelation
                        INTO #GARecordsBatch
                        FROM dbo.GARecords WITH (UPDLOCK, READPAST)
                        WHERE IsCorrelation = 0
                        ORDER BY Date;

                        -- Step 2: Mark source as processed
                        UPDATE g
                        SET IsCorrelation = 1
                        FROM dbo.GARecords g
                        INNER JOIN #GARecordsBatch b
                            ON g.Hash = b.Hash;

                        -- Step 3: Insert into staged table (deduped)
                        INSERT INTO dbo.GARecords_staged
                        (
                            Date,
                            PageId,
                            Interval,
                            Users,
                            Views,
                            Sessions,
                            Hash,
                            IsCorrelation
                        )
                        SELECT
                            b.Date,
                            b.PageId,
                            CASE
                                WHEN DATEPART(HOUR, b.Date) < 4 THEN 1
                                WHEN DATEPART(HOUR, b.Date) < 8 THEN 2
                                WHEN DATEPART(HOUR, b.Date) < 12 THEN 3
                                WHEN DATEPART(HOUR, b.Date) < 16 THEN 4
                                WHEN DATEPART(HOUR, b.Date) < 20 THEN 5
                                ELSE 6
                            END,
                            b.Users,
                            b.Views,
                            b.Sessions,
                            b.Hash,
                            b.IsCorrelation
                        FROM #GARecordsBatch b
                        WHERE NOT EXISTS (
                            SELECT 1
                            FROM dbo.GARecords_staged s
                            WHERE s.Hash = b.Hash
                        );

                        DROP TABLE #GARecordsBatch;

                        COMMIT;

                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0
                            ROLLBACK;

                        THROW;
                    END CATCH
                END;
                """;

            migrationBuilder.Sql(stagedGAProcedure);

            var stagedPsaProcedure = """
                alter PROCEDURE StagePSARecords
                    @BatchSize INT
                AS
                BEGIN
                     SET NOCOUNT ON;
                        BEGIN TRY

                            BEGIN TRANSACTION;

                            -- Step 1: Extract batch into temp table
                            SELECT TOP (@BatchSize)
                                Date,
                                PageId,
                                PerformanceScore,
                                LCP_MS,
                                Hash,
                                IsCorrelation
                            INTO #PSIRecordsBatch
                            FROM dbo.PSIRecords WITH (UPDLOCK, READPAST)
                            WHERE IsCorrelation = 0
                            ORDER BY Date;

                            -- Step 2: Mark source as processed
                            UPDATE p
                            SET IsCorrelation = 1
                            FROM dbo.PSIRecords p
                            INNER JOIN #PSIRecordsBatch b
                                ON p.Hash = b.Hash;

                            -- Step 3: Insert into staged table (deduped)
                            INSERT INTO dbo.PSARecords_staged
                            (
                                Date,
                                PageId,
                                Interval,
                                PerformanceScore,
                                LCP_MS,
                                Hash,
                                IsCorrelation
                            )
                            SELECT
                                b.Date,
                                b.PageId,
                                CASE
                                    WHEN DATEPART(HOUR, b.Date) < 4 THEN 1
                                    WHEN DATEPART(HOUR, b.Date) < 8 THEN 2
                                    WHEN DATEPART(HOUR, b.Date) < 12 THEN 3
                                    WHEN DATEPART(HOUR, b.Date) < 16 THEN 4
                                    WHEN DATEPART(HOUR, b.Date) < 20 THEN 5
                                    ELSE 6
                                END,
                                b.PerformanceScore,
                                b.LCP_MS,
                                b.Hash,
                                b.IsCorrelation
                            FROM #PSIRecordsBatch b
                            WHERE NOT EXISTS (
                                SELECT 1
                                FROM dbo.PSARecords_staged s
                                WHERE s.Hash = b.Hash
                            );

                            DROP TABLE #PSIRecordsBatch;

                            COMMIT;

                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0
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
            var stagedPsaProcedure = """
                alter PROCEDURE StagePSARecords
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
                            (Date, PageId, Interval, PerformanceScore, LCP_MS,   IsCorrelation);

                        COMMIT;
                    END TRY
                    BEGIN CATCH
                        ROLLBACK;
                        THROW;
                    END CATCH
                END;
                """;
            migrationBuilder.Sql(stagedPsaProcedure);

            var stagedGAProcedure = """
                alter PROCEDURE StageGARecords
                AS
                BEGIN
                    SET NOCOUNT ON;

                    BEGIN TRY
                        BEGIN TRANSACTION;

                        WITH Batch AS (
                            SELECT TOP (200000)
                                Date,
                                PageId,
                                Users,
                                Views,
                                Sessions,
                                 IsCorrelation
                            FROM dbo.GARecords WITH (UPDLOCK, READPAST)
                            WHERE IsCorrelation = 0
                            ORDER BY Date
                        )
                        UPDATE Batch
                        SET IsCorrelation = 1
                        OUTPUT
                             inserted.Date ,
                            inserted.PageId,
                            CASE
                                WHEN DATEPART(HOUR, inserted.Date) < 4 THEN 1
                                WHEN DATEPART(HOUR, inserted.Date) < 8 THEN 2
                                WHEN DATEPART(HOUR, inserted.Date) < 12 THEN 3
                                WHEN DATEPART(HOUR, inserted.Date) < 16 THEN 4
                                WHEN DATEPART(HOUR, inserted.Date) < 20 THEN 5
                                ELSE 6
                            END,
                            inserted.Users,
                            inserted.Views,
                            inserted.Sessions, 
                            inserted.IsCorrelation
                        INTO dbo.GARecords_staged
                            (Date, PageId, Interval, Users, Views, Sessions,  IsCorrelation);

                        COMMIT;
                    END TRY
                    BEGIN CATCH
                        ROLLBACK;
                        THROW;
                    END CATCH
                END;
                """;

            migrationBuilder.Sql(stagedGAProcedure);
        }
    }
}
