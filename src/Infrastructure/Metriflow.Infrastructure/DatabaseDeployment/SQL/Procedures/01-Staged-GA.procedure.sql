CREATE
OR ALTER PROCEDURE StageGARecords
    @BatchSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;


    DROP TABLE IF EXISTS #GARecordsBatch;

    BEGIN TRY BEGIN TRANSACTION;
-- 1. Explicitly create temp table to assign a PRIMARY KEY
CREATE TABLE #GARecordsBatch
    (
        PageId INT NOT NULL,
        [Date] DATETIME2 NOT NULL,
        DateOnly DATE NOT NULL,
        Users BIGINT NOT NULL,
        Views BIGINT NOT NULL,
        Sessions BIGINT NOT NULL,
        [Hash] uniqueidentifier NOT NULL PRIMARY KEY,
        IsCorrelation BIT NOT NULL
    );

-- 2. Atomic Queue Pop using CTE + UPDATE OUTPUT
;WITH
        UnprocessedBatch
        AS
        (
            SELECT TOP (@BatchSize)
                PageId,
                [Date],
                DateOnly,
                Users,
                Views,
                Sessions,
                [Hash],
                IsCorrelation
            FROM dbo.GARecords WITH (UPDLOCK, READPAST)
            WHERE IsCorrelation = 0
        )
UPDATE UnprocessedBatch
SET IsCorrelation = 1 
OUTPUT 
    inserted.PageId,
    inserted.[Date],
    inserted.DateOnly,
    inserted.Users,
    inserted.Views,
    inserted.Sessions,
    inserted.[Hash],
    inserted.IsCorrelation
INTO #GARecordsBatch (
    PageId,
    [Date],
    DateOnly,
    Users,
    Views,
    Sessions,
    [Hash],
    IsCorrelation
);
-- 3. Early exit if no rows found
IF @@ROWCOUNT = 0 BEGIN
        COMMIT TRANSACTION;
        SELECT 0 AS RowsProcessed;
        RETURN;
    END;
-- 4. Insert into staging
INSERT INTO dbo.GARecords_staged
        (
        PageId,
        [Date],
        DateOnly,
        Interval,
        Users,
        Views,
        Sessions,
        [Hash],
        IsCorrelation
        )
    SELECT
        batch.PageId,
        batch.[Date],
        batch.DateOnly,
        CASE
                WHEN DATEPART(HOUR, batch.Date) < 4 THEN 1
                WHEN DATEPART(HOUR, batch.Date) < 8 THEN 2
                WHEN DATEPART(HOUR, batch.Date) < 12 THEN 3
                WHEN DATEPART(HOUR, batch.Date) < 16 THEN 4
                WHEN DATEPART(HOUR, batch.Date) < 20 THEN 5
                ELSE 6
            END,
        batch.Users,
        batch.Views,
        batch.Sessions,
        batch.[Hash],
        0
    FROM #GARecordsBatch batch
    WHERE NOT EXISTS (
        SELECT 1
    FROM dbo.GARecords_staged ga
    WHERE ga.[Hash] = batch.[Hash]
    );

DECLARE @InsertedRows INT = @@ROWCOUNT;

IF @InsertedRows = 0
BEGIN
        COMMIT TRANSACTION;
        RETURN;
    END;

    INSERT INTO StagingReadiness
        (Source, BatchId)
    VALUES
        ('GA', NEWID());

        SELECT @InsertedRows AS RowsProcessed; 
    
-- 5. Cleanup and Commit
DROP TABLE IF EXISTS #GARecordsBatch;
COMMIT TRANSACTION;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
-- IF OBJECT_ID('tempdb..#GARecordsBatch') IS NOT NULL
DROP TABLE IF EXISTS #GARecordsBatch;
THROW;
END CATCH
END;