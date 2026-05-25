IF NOT EXISTS (SELECT 1
FROM sys.indexes
WHERE name = 'CCI_PageAnalytics' AND object_id = OBJECT_ID('PageAnalytics'))
BEGIN
    CREATE CLUSTERED COLUMNSTORE INDEX CCI_PageAnalytics ON PageAnalytics;

END;



-- TimeIntervalsAnalytics



IF NOT EXISTS (SELECT 1
FROM sys.indexes
WHERE name = 'CCI_TimeIntervalsAnalytics' AND object_id = OBJECT_ID('TimeIntervalsAnalytics'))
BEGIN
    CREATE CLUSTERED COLUMNSTORE INDEX CCI_TimeIntervalsAnalytics ON TimeIntervalsAnalytics
        ORDER (PageId, Date);

END;


-- DailyAnalytics


IF NOT EXISTS (SELECT 1
FROM sys.indexes
WHERE name = 'CCI_DailyAnalytics' AND object_id = OBJECT_ID('DailyAnalytics'))
BEGIN
    CREATE CLUSTERED COLUMNSTORE INDEX CCI_DailyAnalytics ON DailyAnalytics
    ORDER (PageId, Date);

END;



-- MonthlyAnalytics


IF NOT EXISTS (SELECT 1
FROM sys.indexes
WHERE name = 'CCI_MonthlyAnalytics' AND object_id = OBJECT_ID('MonthlyAnalytics'))
BEGIN
    CREATE CLUSTERED COLUMNSTORE INDEX CCI_MonthlyAnalytics ON MonthlyAnalytics
    ORDER (PageId, YearMonth);
END;


-- YearlyAnalytics


IF NOT EXISTS (SELECT 1
FROM sys.indexes
WHERE name = 'CCI_YearlyAnalytics' AND object_id = OBJECT_ID('YearlyAnalytics'))
BEGIN
    CREATE CLUSTERED COLUMNSTORE INDEX CCI_YearlyAnalytics ON YearlyAnalytics
      ORDER (PageId, Year);
END;