-- =============================================
-- Simple Test Query - Run this to verify basic connectivity
-- =============================================

-- Test 1: Check if table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
BEGIN
    PRINT '✅ BankStatements table EXISTS';
    
    -- Test 2: Count rows
    DECLARE @Count INT;
    SELECT @Count = COUNT(*) FROM [dbo].[BankStatements];
    PRINT 'Total rows: ' + CAST(@Count AS VARCHAR(10));
    
    -- Test 3: Simple query (replace with your actual BankAccountId)
    DECLARE @TestAccountId NVARCHAR(450) = 'cac74e59-47a4-49bd-b0f2-78e3d8771bf7';
    
    PRINT '';
    PRINT 'Testing simple query...';
    SELECT TOP 5 
        Id,
        StatementName,
        StatementEndDate,
        TotalTransactions,
        CreatedAt
    FROM [dbo].[BankStatements]
    WHERE BankAccountId = @TestAccountId
    ORDER BY StatementEndDate DESC;
    
    IF @@ROWCOUNT = 0
    BEGIN
        PRINT '';
        PRINT '⚠️ No data found for BankAccountId: ' + @TestAccountId;
        PRINT 'Showing all BankAccountIds in table:';
        SELECT DISTINCT TOP 10 BankAccountId FROM [dbo].[BankStatements];
    END
    ELSE
    BEGIN
        PRINT '';
        PRINT '✅ Query executed successfully!';
    END
END
ELSE
BEGIN
    PRINT '❌ BankStatements table DOES NOT EXIST';
    PRINT '';
    PRINT 'ACTION REQUIRED:';
    PRINT '1. Run the migration script: run_migration_direct_FIXED.sql';
    PRINT '2. Verify the script executed successfully';
    PRINT '3. Run this test again';
END
GO

