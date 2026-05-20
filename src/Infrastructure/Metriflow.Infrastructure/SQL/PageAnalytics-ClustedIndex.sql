CREATE CLUSTERED COLUMNSTORE INDEX CCI_PageAnalytics ON PageAnalytics;
GO

-- DOWN Migration: Drop Clustered Columnstore Index
DROP INDEX CCI_PageAnalytics ON PageAnalytics;
GO
