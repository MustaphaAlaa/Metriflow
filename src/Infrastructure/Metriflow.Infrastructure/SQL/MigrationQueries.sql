-- ============================================================================
-- SQL Server Queries Extracted from EF Core Migrations
-- This file contains all custom SQL queries (not EF Core generated code)
-- ============================================================================

-- ============================================================================
-- Migration: 20260428023113_Create_StageGARecords_Procedure.cs
-- ============================================================================

-- UP Migration: Create StageGARecords Procedure
CREATE PROCEDURE StageGARecords
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        WITH
        Batch
        AS
        (
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
GO

-- DOWN Migration: Drop StageGARecords Procedure
DROP PROCEDURE StageGARecords;
GO

-- ============================================================================
-- Migration: 20260428023308_Create_StagePSARecords_Procudure.cs
-- ============================================================================

-- UP Migration: Create StagePSARecords Procedure
CREATE PROCEDURE StagePSARecords
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        WITH
        Batch
        AS
        (
            SELECT TOP (200000)
                Date,
                PageId,
                PerformanceScore,
                LCP_MS,
                IsCorrelation
            FROM dbo.PSARecords WITH (UPDLOCK, READPAST)
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
            (Date, PageId, Interval, PerformanceScore, LCP_MS,  IsCorrelation);

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

-- DOWN Migration: Drop StagePSARecords Procedure
DROP PROCEDURE StagePSARecords;
GO

-- ============================================================================
-- Migration: 20260507224611_alter_StoredProcedures.cs
-- ============================================================================

-- UP Migration: Alter StageGARecords Procedure (with batch size parameter)
ALTER PROCEDURE StageGARecords
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
GO

-- UP Migration: Alter StagePSARecords Procedure (with batch size parameter)
ALTER PROCEDURE StagePSARecords
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

-- DOWN Migration: Revert StagePSARecords Procedure (original version without batch size parameter)
ALTER PROCEDURE StagePSARecords
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        WITH
        Batch
        AS
        (
            SELECT TOP (200000)
                Date,
                PageId,
                PerformanceScore,
                LCP_MS,
                IsCorrelation
            FROM dbo.PSARecords WITH (UPDLOCK, READPAST)
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
GO

-- DOWN Migration: Revert StageGARecords Procedure (original version without batch size parameter)
ALTER PROCEDURE StageGARecords
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        WITH
        Batch
        AS
        (
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
GO

-- ============================================================================
-- Migration: 20260508052822_convert_PageAnalytics_to_clusted_columnstore.cs
-- ============================================================================

-- UP Migration: Create Clustered Columnstore Index on PageAnalytics
CREATE CLUSTERED COLUMNSTORE INDEX CCI_PageAnalytics ON PageAnalytics;
GO

-- DOWN Migration: Drop Clustered Columnstore Index
DROP INDEX CCI_PageAnalytics ON PageAnalytics;
GO





-- Create Table  raw data staged tables

CREATE TABLE GARecords_staged
(
    PageId int,
    [Date] datetime2 not null,
    DateOnly Date not null,
    Interval int,
    Users bigint,
    Views bigint,
    Sessions bigint,
    Hash uniqueidentifier not null,
    IsCorrelation bit,
    INDEX CCI_GARecords_staged CLUSTERED COLUMNSTORE
);
CREATE UNIQUE NONCLUSTERED INDEX UX_GARecords_staged_Hash
                      ON GARecords_staged(Hash);

CREATE NONCLUSTERED INDEX IX_GARecords_staged ON GARecords_staged(PageId, Date, INTERVAL, IsCorrelation);

CREATE TABLE PSARecords_staged
(
    PageId int not null,
    [Date] datetime2 not null,
    DateOnly Date not null,
    Interval int not null,
    PerformanceScore int not null,
    LCP_MS bigint not null,
    Hash uniqueidentifier not null,
    IsCorrelation bit not null,
    INDEX CCI_PSARecords_staged CLUSTERED COLUMNSTORE
);
CREATE UNIQUE NONCLUSTERED INDEX UX_PSARecords_staged_Hash
                     ON PSARecords_staged(Hash);


CREATE NONCLUSTERED INDEX IX_PSARecords_staged ON PSARecords_staged(PageId, Date, INTERVAL, IsCorrelation);
-- down
DROP TABLE PSARecords_staged
DROP TABLE GARecords_staged 







GO;

CREATE OR ALTER PROCEDURE correlateStagedData
    (
    @BatchSize INT
)
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        BEGIN TRANSACTION;

        /*
        =========================================================
        Temp Batch Table
        =========================================================
        */

        CREATE TABLE #Batch
    (
        PageId INT NOT NULL,
        [Date] DATETIME2 NOT NULL,
        DateOnly DATE NOT NULL,
        Intervals TINYINT NOT NULL,

        Users BIGINT NOT NULL,
        Sessions BIGINT NOT NULL,
        Views BIGINT NOT NULL,

        PerformanceScore FLOAT NOT NULL,
        LcpMs BIGINT NOT NULL
    );



        /*
        =========================================================
        Temp Table Index
        =========================================================
        */

        CREATE CLUSTERED INDEX IX_Batch
        ON #Batch(PageId, DateOnly, Intervals);



        /*
        =========================================================
        Fill Batch
        =========================================================

        Correlate staged GA + PSA rows.

        NOTE:
        Since you currently run only one worker instance,
        we keep logic simpler and skip row claiming.

        =========================================================
        */

        INSERT INTO #Batch
        (
        PageId,
        [Date],
        DateOnly,
        Intervals,

        Users,
        Sessions,
        Views,

        PerformanceScore,
        LcpMs
        )

    SELECT TOP (@BatchSize)

        ga.PageId,
        ga.[Date],

        CONVERT(DATE, ga.[Date]) AS DateOnly,

        ga.Interval,

        ga.Users,
        ga.Sessions,
        ga.Views,

        psa.PerformanceScore,
        psa.LCP_MS

    FROM GARecords_staged ga

        INNER JOIN PSARecords_staged psa
        ON ga.PageId = psa.PageId
            AND ga.[Date] = psa.[Date]
            AND ga.Interval = psa.Interval

    WHERE
            ga.IsCorrelation = 0
        AND psa.IsCorrelation = 0

    ORDER BY ga.[Date];



        /*
        =========================================================
        No Rows
        =========================================================
        */

        IF @@ROWCOUNT = 0
        BEGIN
        COMMIT TRANSACTION;
        RETURN;
    END;



        /*
        =========================================================
        Insert Into PageAnalytics
        =========================================================

        TABLOCK helps columnstore bulk loading behavior.

        =========================================================
        */

        INSERT INTO PageAnalytics WITH (TABLOCK)
        (
        PageId,
        [Date],
        DateOnly,
        Intervals,

        Users,
        Sessions,
        Views,

        PerformanceScore,
        LcpMs
        )

    SELECT
        b.PageId,
        b.[Date],
        b.DateOnly,
        b.Intervals,

        b.Users,
        b.Sessions,
        b.Views,

        b.PerformanceScore,
        b.LcpMs

    FROM #Batch b;



        /*
        =========================================================
        Detect Late Arrivals
        =========================================================

        Logic:

        If aggregate already exists in TimeIntervalAnalytics,
        then new incoming rows mean:

            late-arriving data

        Therefore:
            mark aggregate dirty

        =========================================================
        */

        INSERT INTO DirtyTimeIntervalAggregates
        (
        PageId,
        DateOnly,
        IntervalId
        )

    SELECT DISTINCT

        b.PageId,
        b.DateOnly,
        b.Intervals

    FROM #Batch b

        INNER JOIN TimeIntervalAnalytics tia
        ON tia.PageId = b.PageId
            AND tia.[Date] = b.DateOnly
            AND tia.TimeIntervalId = b.Intervals

    WHERE NOT EXISTS
        (
            SELECT 1
    FROM DirtyTimeIntervalAggregates d
    WHERE
                d.PageId = b.PageId
        AND d.DateOnly = b.DateOnly
        AND d.IntervalId = b.Intervals
        );



        /*
        =========================================================
        Mark Staged Rows As Correlated
        =========================================================
        */

        UPDATE ga
        SET ga.IsCorrelation = 1

        FROM GARecords_staged ga

        INNER JOIN #Batch b
        ON ga.PageId = b.PageId
            AND ga.[Date] = b.[Date]
            AND ga.Interval = b.Intervals;



        UPDATE psa
        SET psa.IsCorrelation = 1

        FROM PSARecords_staged psa

        INNER JOIN #Batch b
        ON psa.PageId = b.PageId
            AND psa.[Date] = b.[Date]
            AND psa.Interval = b.Intervals;



        /*
        =========================================================
        Cleanup
        =========================================================
        */

        DROP TABLE #Batch;

        COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;

    END CATCH

END;
GO
































