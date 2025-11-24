-- =============================================
-- Database Status Check Script
-- Run this to diagnose the timeout issue
-- =============================================

-- 1. Check if BankStatements table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
BEGIN
    PRINT '✅ BankStatements table exists';
    
    -- Check row count
    DECLARE @RowCount INT;
    SELECT @RowCount = COUNT(*) FROM [dbo].[BankStatements];
    PRINT 'Total rows in BankStatements: ' + CAST(@RowCount AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT '❌ BankStatements table DOES NOT EXIST - Run migration script!';
END
GO

-- 2. Check indexes on BankStatements
PRINT '';
PRINT 'Checking indexes on BankStatements table:';
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columns
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('dbo.BankStatements')
    AND i.type > 0 -- Exclude heap
GROUP BY i.name, i.type_desc
ORDER BY i.name;
GO

-- 3. Check for composite index on (BankAccountId, UserId)
IF EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_BankStatements_BankAccountId_UserId' 
    AND object_id = OBJECT_ID('dbo.BankStatements')
)
BEGIN
    PRINT '';
    PRINT '✅ Composite index IX_BankStatements_BankAccountId_UserId exists';
END
ELSE
BEGIN
    PRINT '';
    PRINT '❌ Composite index IX_BankStatements_BankAccountId_UserId MISSING';
    PRINT '   Run: add_performance_index.sql to create it';
END
GO

-- 4. Test query performance (replace with actual IDs from your database)
PRINT '';
PRINT 'Testing query performance...';
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

-- Replace these with actual values from your database
DECLARE @TestBankAccountId NVARCHAR(450) = 'cac74e59-47a4-49bd-b0f2-78e3d8771bf7';
DECLARE @TestUserId NVARCHAR(450);

-- Get a sample UserId
SELECT TOP 1 @TestUserId = UserId FROM [dbo].[BankStatements] WHERE BankAccountId = @TestBankAccountId;

IF @TestUserId IS NOT NULL
BEGIN
    PRINT 'Testing with BankAccountId: ' + @TestBankAccountId;
    PRINT 'Testing with UserId: ' + @TestUserId;
    
    SELECT 
        Id, StatementName, StatementEndDate, TotalTransactions
    FROM [dbo].[BankStatements]
    WHERE BankAccountId = @TestBankAccountId 
        AND UserId = @TestUserId
    ORDER BY StatementEndDate DESC;
    
    PRINT 'Query completed - check execution time above';
END
ELSE
BEGIN
    PRINT '⚠️ No data found for test BankAccountId';
    PRINT '   Showing sample data:';
    SELECT TOP 5 BankAccountId, UserId, StatementName 
    FROM [dbo].[BankStatements];
END

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
GO

-- 5. Check for table locks
PRINT '';
PRINT 'Checking for active locks on BankStatements:';
SELECT 
    request_session_id,
    resource_type,
    resource_database_id,
    resource_associated_entity_id,
    request_mode,
    request_status
FROM sys.dm_tran_locks
WHERE resource_associated_entity_id = OBJECT_ID('dbo.BankStatements');
GO

-- 6. Check database connection timeout settings
PRINT '';
PRINT 'Current connection timeout settings:';
SELECT 
    connection_id,
    net_transport,
    protocol_type,
    auth_scheme,
    client_net_address,
    connect_time
FROM sys.dm_exec_connections
WHERE session_id = @@SPID;
GO

PRINT '';
PRINT '=========================================';
PRINT 'Diagnostic check complete!';
PRINT '=========================================';

