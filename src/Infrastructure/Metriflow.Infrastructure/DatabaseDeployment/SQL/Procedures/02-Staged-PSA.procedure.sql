CREATE
OR ALTER PROCEDURE StagePSARecords
    @BatchSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;


    DROP TABLE IF EXISTS #PSARecordsBatch;


    BEGIN TRY BEGIN TRANSACTION;

CREATE TABLE #PSARecordsBatch
    (
        PageId INT NOT NULL,
        [Date] DATETIME2 NOT NULL,
        DateOnly DATE NOT NULL,
        PerformanceScore FLOAT NOT NULL,
        LCP_MS BIGINT NOT NULL,
        [Hash] uniqueidentifier NOT NULL PRIMARY KEY,
        IsCorrelation BIT NOT NULL
    );
;WITH
        UnprocessedBatch
        AS
        (
            SELECT TOP (@BatchSize)
                PageId,
                [Date],
                DateOnly,
                PerformanceScore,
                LCP_MS,
                [Hash],
                IsCorrelation
            FROM dbo.PSARecords WITH (UPDLOCK, READPAST)
            WHERE IsCorrelation = 0
        )
UPDATE UnprocessedBatch
SET IsCorrelation = 1 OUTPUT inserted.PageId,
    inserted.[Date],
    inserted.DateOnly,
    inserted.PerformanceScore,
    inserted.LCP_MS,
    inserted.[Hash],
    inserted.IsCorrelation INTO #PSARecordsBatch (
    PageId,
    [Date],
    DateOnly,
    PerformanceScore,
    LCP_MS,
    [Hash],
    IsCorrelation
);
-- 3. Early exit if no rows found
IF @@ROWCOUNT = 0 BEGIN
        COMMIT TRANSACTION;
        SELECT 0 AS RowsProcessed;
        Return;
    END;

-- 4. Insert into staged table (deduped)

INSERT INTO dbo.PSARecords_staged
        (
        PageId,
        [Date],
        DateOnly,
        Interval,
        PerformanceScore,
        LCP_MS,
        [Hash],
        IsCorrelation
        )
    SELECT batch.PageId,
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
        batch.PerformanceScore,
        batch.LCP_MS,
        batch.[Hash],
        0
    FROM #PSARecordsBatch batch
    WHERE NOT EXISTS (
        SELECT 1
    FROM dbo.PSARecords_staged psa
    WHERE psa.[Hash] = batch.[Hash]
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
        ('PSA', NEWID());

     SELECT @InsertedRows AS RowsProcessed; 

-- 5. Cleanup and Commit
DROP TABLE IF EXISTS #PSARecordsBatch;
COMMIT TRANSACTION;
END TRY 
BEGIN CATCH
 IF @@TRANCOUNT > 0
     ROLLBACK TRANSACTION;
THROW;
END CATCH
    DROP TABLE IF EXISTS #PSARecordsBatch;
END;