CREATE OR ALTER PROCEDURE ProcessCorrelatedPageAnalyticsBatch
    @BatchSize INT
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /*
        Goals:
          - Join uncorrelated rows from staging tables and insert into PageAnalytics.
          - Enqueue distinct keys for aggregate recomputation.

        Steps:
          1. Defensive temp table cleanup (survives prior failed calls on pooled connections)
          2. Create temp table
          3. Select TOP @BatchSize uncorrelated joined rows (UPDLOCK+READPAST for concurrency safety)
          4. Early exit if no rows found
          5. Insert joined rows into PageAnalytics
          6. Enqueue new distinct keys into AggregateRecomputeQueue
          7. Mark staged rows as correlated on both source tables
          8. Drop temp table and commit
    */

    -- Step 1: Defensive cleanup — temp tables survive ROLLBACK on pooled connections.
    -- If a prior call failed after CREATE TABLE, this prevents "object already exists" on retry.
    DROP TABLE IF EXISTS #tmp__BatchesToConsume;
    DROP TABLE IF EXISTS #tmp__staged;

    DECLARE  @InsertedCount INT = 0;

    BEGIN TRY

        BEGIN TRANSACTION;



        -- Step 2
        CREATE TABLE #tmp__staged
    (
        PageId INT NOT NULL,
        [Date] DATETIME2 NOT NULL,
        DateOnly DATE NOT NULL,
        Interval INT NOT NULL,
        Users BIGINT NOT NULL,
        Sessions BIGINT NOT NULL,
        Views BIGINT NOT NULL,
        PerformanceScore FLOAT NOT NULL,
        LcpMs BIGINT NOT NULL,
        PRIMARY KEY (PageId, [Date], Interval)
    ); 

    CREATE TABLE #tmp__BatchesToConsume
    (
        Source VARCHAR(10) NOT NULL,
        BatchId UNIQUEIDENTIFIER NOT NULL,
        PRIMARY KEY (Source, BatchId)
    );

        -- Step 3


-- Step 3: Populate it ONLY with the rows that are currently unconsumed
    INSERT INTO #tmp__BatchesToConsume
        (Source, BatchId)
    SELECT Source, BatchId
    FROM StagingReadiness
    WHERE Consumed = 0;



IF NOT EXISTS (SELECT 1
        FROM #tmp__BatchesToConsume
        WHERE Source = 'GA')
        OR NOT EXISTS (SELECT 1
        FROM #tmp__BatchesToConsume
        WHERE Source = 'PSA')
        BEGIN
        COMMIT TRANSACTION;
        SELECT 0 AS CorrelatedCount;
        RETURN;
    END

 
     

        INSERT INTO #tmp__staged
        (
        PageId,
        [Date],
        DateOnly,
        Interval,
        Users,
        Sessions,
        Views,
        PerformanceScore,
        LcpMs
        )
    SELECT TOP (@BatchSize)
        ga.PageId,
        ga.[Date],
        ga.DateOnly,
        ga.Interval,
        ga.Users,
        ga.Sessions,
        ga.Views,
        psa.PerformanceScore,
        psa.LCP_MS
    FROM GARecords_staged ga WITH (UPDLOCK, READPAST)
        INNER JOIN PSARecords_staged psa WITH (UPDLOCK, READPAST)
        ON  ga.PageId   = psa.PageId
            AND ga.[Date]   = psa.[Date]
            AND ga.Interval = psa.Interval
    WHERE
            ga.IsCorrelation  = 0
        AND psa.IsCorrelation = 0;

        -- Step 4 if there's no data out early
        IF @@ROWCOUNT = 0
        BEGIN

        COMMIT TRANSACTION;
        SELECT 0 AS CorrelationCount;
        RETURN;
    END;

        -- Step 5
        INSERT INTO PageAnalytics
        (
        PageId,
        [Date],
        DateOnly,
        Interval,
        Users,
        Sessions,
        Views,
        PerformanceScore,
        LcpMs,
        CreatedAt
        )
    SELECT
        tmp.PageId,
        tmp.[Date],
        tmp.DateOnly,
        tmp.Interval,
        tmp.Users,
        tmp.Sessions,
        tmp.Views,
        tmp.PerformanceScore,
        tmp.LcpMs,
        SYSUTCDATETIME()
    FROM #tmp__staged tmp;

    SET @InsertedCount = @@ROWCOUNT;

        -- Step 6
        INSERT INTO AggregateRecomputeQueue
        (
        PageId,
        Date,
        Interval,
        CreatedAt
        )
    SELECT DISTINCT
        b.PageId,
        b.DateOnly,
        b.Interval,
        SYSUTCDATETIME()
    FROM #tmp__staged b
        INNER JOIN TimeIntervalsAnalytics tia
        ON  tia.PageId         = b.PageId
            AND tia.[Date]         = b.DateOnly
            AND tia.TimeIntervalId = b.Interval
    WHERE NOT EXISTS
        (
            SELECT 1
    FROM AggregateRecomputeQueue d
    WHERE
                d.PageId     = b.PageId
        AND d.Date       = b.DateOnly
        AND d.Interval = b.Interval
        );

        -- Step 7
        UPDATE ga
        SET ga.IsCorrelation = 1
        FROM GARecords_staged ga
        INNER JOIN #tmp__staged tmp
        ON  ga.PageId   = tmp.PageId
            AND ga.[Date]   = tmp.[Date]
            AND ga.Interval = tmp.Interval
            AND ga.IsCorrelation = 0 ;

        UPDATE psa
        SET psa.IsCorrelation = 1
        FROM PSARecords_staged psa
        INNER JOIN #tmp__staged tmp
        ON  psa.PageId   = tmp.PageId
            AND psa.[Date]   = tmp.[Date]
            AND psa.Interval = tmp.Interval
            AND psa.IsCorrelation = 0;

        -- Step 8

        UPDATE sr
    SET sr.Consumed = 1
    FROM StagingReadiness sr
        INNER JOIN #tmp__BatchesToConsume tmp
        ON sr.BatchId = tmp.BatchId
    WHERE sr.Source = 'GA'
        AND tmp.Source = 'GA'
        AND sr.Consumed = 0;

    -- Update PSA Batches
    UPDATE sr
    SET sr.Consumed = 1
    FROM StagingReadiness sr
        INNER JOIN #tmp__BatchesToConsume tmp
        ON sr.BatchId = tmp.BatchId
    WHERE sr.Source = 'PSA'
        AND tmp.Source = 'PSA'
        AND sr.Consumed = 0;

  
        DROP TABLE IF EXISTS #tmp__BatchesToConsume;
        DROP TABLE IF EXISTS #tmp__staged;

        COMMIT TRANSACTION;
 
        SELECT @InsertedCount AS CorrelatedCount;

      END TRY
    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;


                DROP TABLE IF EXISTS #tmp__BatchesToConsume;
                DROP TABLE IF EXISTS #tmp__staged;

        THROW;

    END CATCH

END;