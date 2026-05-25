IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PSARecords_staged')
BEGIN
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
END;



IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PSARecords_staged_Hash')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_PSARecords_staged_Hash
                         ON PSARecords_staged(Hash);
END;




IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PSARecords_staged')
BEGIN
    CREATE NONCLUSTERED INDEX IX_PSARecords_staged ON PSARecords_staged(PageId, Date, INTERVAL, IsCorrelation);
END;




IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PSARecords_staged_Uncorrelated')
BEGIN
    CREATE INDEX IX_PSARecords_staged_Uncorrelated
    ON PSARecords_staged (PageId, [Date], Interval)
    WHERE IsCorrelation = 0;
END;

