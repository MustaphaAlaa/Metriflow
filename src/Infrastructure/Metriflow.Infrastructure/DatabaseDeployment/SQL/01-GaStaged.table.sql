IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GARecords_staged')
BEGIN
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
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_GARecords_staged_Hash')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_GARecords_staged_Hash
                          ON GARecords_staged(Hash);
END;



IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GARecords_staged')
BEGIN
    CREATE NONCLUSTERED INDEX IX_GARecords_staged ON GARecords_staged(PageId, Date, INTERVAL, IsCorrelation);
END;




IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GARecords_staged_Uncorrelated')
BEGIN
    CREATE INDEX IX_GARecords_staged_Uncorrelated
    ON GARecords_staged (PageId, [Date], Interval)
    WHERE IsCorrelation = 0;
END;