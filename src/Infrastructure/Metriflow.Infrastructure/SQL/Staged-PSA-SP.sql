-- ============================================================================
-- Migration: 20260428023308_Create_StagePSARecords_Procudure.cs
-- ============================================================================

-- UP Migration: Create StagePSARecords Procedure
CREATE OR ALTER PROCEDURE StagePSARecords
    @BatchSize INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY

            BEGIN TRANSACTION;

            -- Step 1: Extract batch into temp table
            SELECT TOP (@BatchSize)
        Date,
        DateOnly,
        PageId,
        PerformanceScore,
        LCP_MS,
        Hash,
        IsCorrelation
    INTO #PSARecordsBatch
    FROM dbo.PSARecords WITH (UPDLOCK, READPAST)
    WHERE IsCorrelation = 0
    ORDER BY Date;

            -- Step 2: Mark source as processed
            UPDATE p
            SET IsCorrelation = 1
            FROM dbo.PSARecords p
        INNER JOIN #PSARecordsBatch b
        ON p.Hash = b.Hash;

            -- Step 3: Insert into staged table (deduped)
            INSERT INTO dbo.PSARecords_staged
        (
        Date,
        DateOnly,
        PageId,
        Interval,
        PerformanceScore,
        LCP_MS,
        Hash,
        IsCorrelation
        )
    SELECT
        b.Date,
        b.DateOnly,
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
    FROM #PSARecordsBatch b
    WHERE NOT EXISTS (
                SELECT 1
    FROM dbo.PSARecords_staged s
    WHERE s.Hash = b.Hash
            );

            DROP TABLE #PSARecordsBatch;

            COMMIT;

        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK;

            THROW;
        END CATCH
END;
GO

-- DOWN Migration: Drop StagePSARecords Procedure
DROP PROCEDURE StagePSARecords;
GO