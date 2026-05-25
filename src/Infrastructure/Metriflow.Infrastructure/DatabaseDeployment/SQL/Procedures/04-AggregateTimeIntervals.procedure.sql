CREATE OR ALTER PROCEDURE AggregateTimeIntervals
AS
BEGIN
    SET NOCOUNT ON;


    DROP TABLE IF EXISTS  #tmp__tia_aggregate;

    CREATE TABLE #tmp__tia_aggregate
    (
        PageId INT NOT NULL,
        [Date] DATE NOT NULL,
        TimeIntervalId INT NOT NULL,
        TotalUsers BIGINT NOT NULL,
        TotalSessions BIGINT NOT NULL,
        TotalViews BIGINT NOT NULL,
        AvgPerformance FLOAT NOT NULL,
        PRIMARY KEY (PageId, [Date], TimeIntervalId)
    );

    DECLARE @lastProcessed DATETIME2;
    DECLARE @currentDate DATETIME2 = SYSUTCDATETIME();

    SELECT @lastProcessed = LastProcessedAt
    FROM AggregationCheckpoint
    WHERE PipelineName = 'TimeIntervalAggregation';

    -- Step 1: Aggregate ONLY the new batch into a local temp table
    -- This keeps the working dataset tiny and highly optimized in memory
    -- SELECT
    --     pageAnalytics.PageId,
    --     pageAnalytics.DateOnly AS [Date],
    --     pageAnalytics.Interval AS [TimeIntervalId],
    --     SUM(pageAnalytics.Users) AS TotalUsers,
    --     SUM(pageAnalytics.Sessions) AS TotalSessions,
    --     SUM(pageAnalytics.Views) AS TotalViews,
    --     AVG(pageAnalytics.PerformanceScore) AS AvgPerformance
    -- INTO #NewAggregates
    -- FROM dbo.PageAnalytics AS pageAnalytics
    -- WHERE pageAnalytics.CreatedAt > @lastProcessed
    --     AND pageAnalytics.CreatedAt <= @currentDate
    -- GROUP BY pageAnalytics.PageId, pageAnalytics.DateOnly, pageAnalytics.Interval;


    INSERT INTO  #tmp__tia_aggregate
        (PageId, Date, TimeIntervalId, TotalUsers, TotalSessions, TotalViews, AvgPerformance)
    SELECT
        pageAnalytics.PageId,
        pageAnalytics.DateOnly AS [Date],
        pageAnalytics.Interval AS [TimeIntervalId],
        SUM(pageAnalytics.Users) AS TotalUsers,
        SUM(pageAnalytics.Sessions) AS TotalSessions,
        SUM(pageAnalytics.Views) AS TotalViews,
        AVG(pageAnalytics.PerformanceScore) AS AvgPerformance
    FROM dbo.PageAnalytics AS pageAnalytics
    WHERE pageAnalytics.CreatedAt > @lastProcessed
        AND pageAnalytics.CreatedAt <= @currentDate
    GROUP BY pageAnalytics.PageId, pageAnalytics.DateOnly, pageAnalytics.Interval;

    -- Add a temporary primary key to the small batch to make the exclusion lookup instantaneous
    -- ALTER TABLE #NewAggregates
    -- ADD CONSTRAINT PK_NewAggregates PRIMARY KEY CLUSTERED (PageId, [Date], TimeIntervalId);

    -- Step 2: Insert into Columnstore target, filtering against existing records
    INSERT INTO dbo.TimeIntervalsAnalytics
        (
        PageId, Date, TimeIntervalId,
        TotalUsers, TotalSessions, TotalViews, AvgPerformance
        )
    SELECT
        temp_aggregate.PageId, temp_aggregate.[Date], temp_aggregate.TimeIntervalId,
        temp_aggregate.TotalUsers, temp_aggregate.TotalSessions, temp_aggregate.TotalViews, temp_aggregate.AvgPerformance
    FROM #tmp__tia_aggregate AS temp_aggregate
    WHERE NOT EXISTS (
        SELECT 1
    FROM dbo.TimeIntervalsAnalytics AS tia
    WHERE temp_aggregate.PageId = tia.PageId
        AND temp_aggregate.[Date] = tia.Date
        AND temp_aggregate.TimeIntervalId = tia.TimeIntervalId
    );

    -- Step 3: Update checkpoint
    UPDATE AggregationCheckpoint
    SET LastProcessedAt = @currentDate
    WHERE PipelineName = 'TimeIntervalAggregation';

    DROP TABLE IF EXISTS #tmp__tia_aggregate;
END;