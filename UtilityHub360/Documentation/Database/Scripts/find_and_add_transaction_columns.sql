-- Script to find the correct table name and add missing columns
-- This script first finds the table, then adds the columns

-- Step 1: Find the BankTransactions table (check different possible names)
PRINT 'Searching for BankTransactions table...';
PRINT '';

-- Check for BankTransactions (plural)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BankTransactions')
BEGIN
    PRINT 'Found table: BankTransactions';
    DECLARE @TableName NVARCHAR(128) = 'BankTransactions';
    DECLARE @SchemaName NVARCHAR(128) = SCHEMA_NAME();
    
    PRINT 'Schema: ' + @SchemaName;
    PRINT 'Full name: [' + @SchemaName + '].[' + @TableName + ']';
    PRINT '';
    PRINT 'Adding columns...';
    PRINT '';
    
    -- Add BillId column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(@SchemaName + '.' + @TableName) AND name = 'BillId')
    BEGIN
        EXEC('ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] ADD [BillId] NVARCHAR(450) NULL');
        PRINT '✓ Added BillId column';
    END
    ELSE
    BEGIN
        PRINT '○ BillId column already exists';
    END

    -- Add LoanId column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(@SchemaName + '.' + @TableName) AND name = 'LoanId')
    BEGIN
        EXEC('ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] ADD [LoanId] NVARCHAR(450) NULL');
        PRINT '✓ Added LoanId column';
    END
    ELSE
    BEGIN
        PRINT '○ LoanId column already exists';
    END

    -- Add SavingsAccountId column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(@SchemaName + '.' + @TableName) AND name = 'SavingsAccountId')
    BEGIN
        EXEC('ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] ADD [SavingsAccountId] NVARCHAR(450) NULL');
        PRINT '✓ Added SavingsAccountId column';
    END
    ELSE
    BEGIN
        PRINT '○ SavingsAccountId column already exists';
    END

    -- Add TransactionPurpose column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(@SchemaName + '.' + @TableName) AND name = 'TransactionPurpose')
    BEGIN
        EXEC('ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] ADD [TransactionPurpose] NVARCHAR(50) NULL');
        PRINT '✓ Added TransactionPurpose column';
    END
    ELSE
    BEGIN
        PRINT '○ TransactionPurpose column already exists';
    END

    -- Create indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_BillId' AND object_id = OBJECT_ID(@SchemaName + '.' + @TableName))
    BEGIN
        EXEC('CREATE INDEX [IX_BankTransactions_BillId] ON [' + @SchemaName + '].[' + @TableName + '] ([BillId])');
        PRINT '✓ Created index IX_BankTransactions_BillId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_LoanId' AND object_id = OBJECT_ID(@SchemaName + '.' + @TableName))
    BEGIN
        EXEC('CREATE INDEX [IX_BankTransactions_LoanId] ON [' + @SchemaName + '].[' + @TableName + '] ([LoanId])');
        PRINT '✓ Created index IX_BankTransactions_LoanId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_SavingsAccountId' AND object_id = OBJECT_ID(@SchemaName + '.' + @TableName))
    BEGIN
        EXEC('CREATE INDEX [IX_BankTransactions_SavingsAccountId] ON [' + @SchemaName + '].[' + @TableName + '] ([SavingsAccountId])');
        PRINT '✓ Created index IX_BankTransactions_SavingsAccountId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_TransactionPurpose' AND object_id = OBJECT_ID(@SchemaName + '.' + @TableName))
    BEGIN
        EXEC('CREATE INDEX [IX_BankTransactions_TransactionPurpose] ON [' + @SchemaName + '].[' + @TableName + '] ([TransactionPurpose])');
        PRINT '✓ Created index IX_BankTransactions_TransactionPurpose';
    END

    PRINT '';
    PRINT '========================================';
    PRINT 'Migration completed successfully!';
    PRINT '========================================';
END
ELSE
BEGIN
    PRINT 'ERROR: BankTransactions table not found!';
    PRINT '';
    PRINT 'Searching for similar table names...';
    PRINT '';
    
    -- List all tables that might be related
    SELECT 
        TABLE_SCHEMA,
        TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME LIKE '%Transaction%' 
       OR TABLE_NAME LIKE '%Bank%'
    ORDER BY TABLE_NAME;
    
    PRINT '';
    PRINT 'Please check the table name above and update the script accordingly.';
    PRINT 'Common alternatives:';
    PRINT '  - Transactions (without Bank prefix)';
    PRINT '  - BankTransaction (singular)';
    PRINT '  - Payments (if transactions are stored in Payments table)';
END

