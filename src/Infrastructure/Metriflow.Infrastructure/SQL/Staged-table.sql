
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
CREATE UNIQUE NONCLUSTERED  INDEX UX_GARecords_staged_Hash
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


CREATE NONCLUSTERED  INDEX IX_PSARecords_staged ON PSARecords_staged(PageId, Date, INTERVAL, IsCorrelation);



CREATE INDEX IX_GARecords_staged_Uncorrelated
ON GARecords_staged (PageId, [Date], Interval)
WHERE IsCorrelation = 0;

CREATE INDEX IX_PSARecords_staged_Uncorrelated
ON PSARecords_staged (PageId, [Date], Interval)
WHERE IsCorrelation = 0;

-- down
DROP TABLE PSARecords_staged
DROP TABLE GARecords_staged 
