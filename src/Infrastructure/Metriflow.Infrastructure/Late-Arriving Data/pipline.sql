CREATE OR ALTER PROCEDURE dbo.sp_ProcessAnalyticsPipeline
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Critical: Ensures transactions abort on errors to prevent partial commits

    -- ---------------------------------------------------------
    -- PHASE 1: ATOMIC SNAPSHOT & CLEAR
    -- ---------------------------------------------------------
    -- We capture the dirty keys and delete them in one atomic operation.
    -- New writes can continue adding to the queue table immediately after this.
    
    DECLARE @DirtySnapshot AS dbo.DirtyKeyList;
    
    BEGIN TRANSACTION;
        -- Atomic Delete + Snapshot
        DELETE FROM dbo.AggregateRecomputeQueue
        OUTPUT 
            deleted.PageId, 
            deleted.Date,   -- Maps to ProcessDate
            deleted.IntervalId -- Maps to IntervalId
        INTO @DirtySnapshot;
    COMMIT TRANSACTION;

    -- If nothing was dirty, exit early
    IF NOT EXISTS (SELECT 1 FROM @DirtySnapshot)
    BEGIN
        PRINT 'No dirty keys found. Exiting.';
        RETURN 0;
    END

    -- ---------------------------------------------------------
    -- ERROR SAFETY NET
    -- ---------------------------------------------------------
    -- If the cascade fails, we must restore the work items 
    -- so they aren't lost.
    BEGIN TRY
        -- ---------------------------------------------------------
        -- PHASE 2: LAYER 1 - TimeIntervalAnalytics
        -- ---------------------------------------------------------
        -- Source: PageAnalytics (Clustered Columnstore)
        -- Target: TimeIntervalAnalytic
        
        PRINT 'Processing TimeIntervalAnalytics...';

        -- 1. Delete existing aggregates for the dirty keys
        -- Using EXISTS against the snapshot is fast and precise
        DELETE FROM dbo.TimeIntervalAnalytic
        WHERE EXISTS (
            SELECT 1 
            FROM @DirtySnapshot s 
            WHERE s.PageId = dbo.TimeIntervalAnalytic.PageId
              AND s.ProcessDate = dbo.TimeIntervalAnalytic.Date
              AND s.IntervalId = dbo.TimeIntervalAnalytic.TimeIntervalId
        );

        -- 2. Recompute and Insert
        -- Note: Assumes PageAnalytics has corresponding columns.
        -- AvgPerformance calculation requires weighted average logic or simple AVG depending on grain.
        -- Using simple AVG here for clarity.
        INSERT INTO dbo.TimeIntervalAnalytic (
            PageId, Date, TimeIntervalId, 
            TotalUsers, TotalSessions, TotalViews, AvgPerformance
        )
        SELECT 
            p.PageId,
            p.DateOnly AS Date,
            p.Interval AS TimeIntervalId,
            SUM(p.Users) AS TotalUsers,
            SUM(p.Sessions) AS TotalSessions,
            SUM(p.Views) AS TotalViews,
            AVG(CAST(p.PerformanceScore AS DECIMAL(10,2))) AS AvgPerformance -- Simplistic avg
        FROM dbo.PageAnalytics p
        INNER JOIN @DirtySnapshot s 
            ON p.PageId = s.PageId 
            AND p.DateOnly = s.ProcessDate 
            AND p.Interval = s.IntervalId
        GROUP BY p.PageId, p.DateOnly, p.Interval;

        -- ---------------------------------------------------------
        -- PHASE 3: LAYER 2 - DailyAnalytics
        -- ---------------------------------------------------------
        -- Source: TimeIntervalAnalytic (Just updated)
        -- Target: DailyAnalytics
        -- Dirty Scope: All dates touched in the snapshot
        
        PRINT 'Processing DailyAnalytics...';

        -- Derive unique (PageId, Date) pairs from the snapshot
        -- We use a CTE to identify which Daily rows need refresh
        ;WITH DatesToRefresh AS (
            SELECT DISTINCT PageId, ProcessDate
            FROM @DirtySnapshot
        )
        DELETE FROM dbo.DailyAnalytics
        WHERE EXISTS (
            SELECT 1 
            FROM DatesToRefresh d 
            WHERE d.PageId = dbo.DailyAnalytics.PageId
              AND d.ProcessDate = dbo.DailyAnalytics.Date
        );

        -- Aggregate from TimeInterval (Layer 1) up to Daily
        INSERT INTO dbo.DailyAnalytics (
            PageId, Date, ReceivedAt,
            TotalUsers, TotalSessions, TotalViews, AvgPerformance
        )
        SELECT 
            t.PageId,
            t.Date,
            GETUTCDATE() AS ReceivedAt,
            SUM(t.TotalUsers) AS TotalUsers,
            SUM(t.TotalSessions) AS TotalSessions,
            SUM(t.TotalViews) AS TotalViews,
            AVG(t.AvgPerformance) AS AvgPerformance -- Simplistic avg of averages
        FROM dbo.TimeIntervalAnalytic t
        INNER JOIN (
            SELECT DISTINCT PageId, ProcessDate FROM @DirtySnapshot
        ) d ON t.PageId = d.PageId AND t.Date = d.ProcessDate
        GROUP BY t.PageId, t.Date;

        -- ---------------------------------------------------------
        -- PHASE 4: LAYER 3 - MonthlyAnalytics
        -- ---------------------------------------------------------
        -- Source: DailyAnalytics (Just updated)
        -- Target: MonthlyAnalytic
        -- Dirty Scope: Distinct (PageId, YearMonth) derived from snapshot
        
        PRINT 'Processing MonthlyAnalytics...';

        -- Identify affected months
        ;WITH MonthsToRefresh AS (
            SELECT DISTINCT 
                PageId, 
                DATEFROMPARTS(YEAR(ProcessDate), MONTH(ProcessDate), 1) AS MonthStart
            FROM @DirtySnapshot
        )
        DELETE FROM dbo.MonthlyAnalytic
        WHERE EXISTS (
            SELECT 1 
            FROM MonthsToRefresh m 
            WHERE m.PageId = dbo.MonthlyAnalytic.PageId
              AND m.MonthStart = dbo.MonthlyAnalytic.YearMonth -- Assuming YearMonth is stored as DATE (1st of month)
        );

        -- Aggregate from Daily (Layer 2) up to Monthly
        INSERT INTO dbo.MonthlyAnalytic (
            PageId, YearMonth,
            TotalUsers, TotalSessions, TotalViews, AvgPerformance
        )
        SELECT 
            d.PageId,
            DATEFROMPARTS(YEAR(d.Date), MONTH(d.Date), 1) AS YearMonth,
            SUM(d.TotalUsers) AS TotalUsers,
            SUM(d.TotalSessions) AS TotalSessions,
            SUM(d.TotalViews) AS TotalViews,
            AVG(d.AvgPerformance) AS AvgPerformance
        FROM dbo.DailyAnalytics d
        INNER JOIN MonthsToRefresh m 
            ON d.PageId = m.PageId 
            AND DATEFROMPARTS(YEAR(d.Date), MONTH(d.Date), 1) = m.MonthStart
        GROUP BY d.PageId, DATEFROMPARTS(YEAR(d.Date), MONTH(d.Date), 1);

        -- ---------------------------------------------------------
        -- PHASE 5: LAYER 4 - YearlyAnalytics
        -- ---------------------------------------------------------
        -- Source: MonthlyAnalytic (Just updated)
        -- Target: YearlyAnalytics
        -- Dirty Scope: Distinct (PageId, Year) derived from snapshot
        
        PRINT 'Processing YearlyAnalytics...';

        ;WITH YearsToRefresh AS (
            SELECT DISTINCT PageId, YEAR(ProcessDate) AS YearVal
            FROM @DirtySnapshot
        )
        DELETE FROM dbo.YearlyAnalytics
        WHERE EXISTS (
            SELECT 1 
            FROM YearsToRefresh y 
            WHERE y.PageId = dbo.YearlyAnalytics.PageId
              AND y.YearVal = dbo.YearlyAnalytics.Year
        );

        -- Aggregate from Monthly (Layer 3) up to Yearly
        INSERT INTO dbo.YearlyAnalytics (
            PageId, Year,
            TotalUsers, TotalSessions, TotalViews, AvgPerformance
        )
        SELECT 
            m.PageId,
            YEAR(m.YearMonth) AS Year,
            SUM(m.TotalUsers) AS TotalUsers,
            SUM(m.TotalSessions) AS TotalSessions,
            SUM(m.TotalViews) AS TotalViews,
            AVG(m.AvgPerformance) AS AvgPerformance
        FROM dbo.MonthlyAnalytic m
        INNER JOIN YearsToRefresh y 
            ON m.PageId = y.PageId 
            AND YEAR(m.YearMonth) = y.YearVal
        GROUP BY m.PageId, YEAR(m.YearMonth);

        PRINT 'Pipeline processing complete.';

    END TRY
    BEGIN CATCH
        -- ---------------------------------------------------------
        -- FAILURE HANDLING: RESTORE SNAPSHOT
        -- ---------------------------------------------------------
        PRINT 'Error occurred. Restoring dirty keys to queue...';
        
        -- Put the keys back so the next run can retry
        INSERT INTO dbo.AggregateRecomputeQueue (PageId, Date, IntervalId, CreatedAt)
        SELECT PageId, ProcessDate, IntervalId, GETUTCDATE()
        FROM @DirtySnapshot;

        -- Log the error (assuming an error log table exists, or just throw)
        -- THROW stops execution and bubbles up to the caller (ADO.NET)
        THROW; 
    END CATCH
END