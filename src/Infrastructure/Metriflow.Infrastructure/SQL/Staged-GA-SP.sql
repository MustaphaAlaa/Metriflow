-- ============================================================================
-- Migration: 20260428023113_Create_StageGARecords_Procedure.cs
-- ============================================================================

-- UP Migration: Create StageGARecords Procedure
CREATE OR ALTER PROCEDURE StageGARecords
    @BatchSize INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
            SELECT TOP (@BatchSize)
        Date,
        DateOnly,
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

    -- update raw staged

      UPDATE g
        SET IsCorrelation = 1
        FROM dbo.GARecords g
        INNER JOIN #GARecordsBatch b
        ON g.Hash = b.Hash;



                INSERT INTO dbo.GARecords_staged
        (
        Date,
        DateOnly,
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
GO

-- DOWN Migration: Drop StageGARecords Procedure
DROP PROCEDURE StageGARecords;
GO