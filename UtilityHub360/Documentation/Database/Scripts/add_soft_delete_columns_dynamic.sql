-- =====================================================
-- Dynamic Soft Delete Migration
-- This script finds tables dynamically and adds columns
-- =====================================================

DECLARE @sql NVARCHAR(MAX);
DECLARE @tableName NVARCHAR(255);
DECLARE @schemaName NVARCHAR(255);

-- List of tables to update
DECLARE @tables TABLE (
    SchemaName NVARCHAR(255),
    TableName NVARCHAR(255)
);

-- Find tables (try different possible names)
INSERT INTO @tables (SchemaName, TableName)
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND (
    TABLE_NAME = 'BankAccounts' OR
    TABLE_NAME = 'BankAccount' OR
    TABLE_NAME = 'bank_accounts' OR
    TABLE_NAME = 'BankTransactions' OR
    TABLE_NAME = 'BankTransaction' OR
    TABLE_NAME = 'bank_transactions' OR
    TABLE_NAME = 'Payments' OR
    TABLE_NAME = 'Payment' OR
    TABLE_NAME = 'payments' OR
    TABLE_NAME = 'Bills' OR
    TABLE_NAME = 'Bill' OR
    TABLE_NAME = 'bills' OR
    TABLE_NAME = 'IncomeSources' OR
    TABLE_NAME = 'IncomeSource' OR
    TABLE_NAME = 'income_sources' OR
    TABLE_NAME = 'VariableExpenses' OR
    TABLE_NAME = 'VariableExpense' OR
    TABLE_NAME = 'variable_expenses' OR
    TABLE_NAME = 'SavingsTransactions' OR
    TABLE_NAME = 'SavingsTransaction' OR
    TABLE_NAME = 'savings_transactions'
  );

-- Cursor to iterate through tables
DECLARE table_cursor CURSOR FOR
SELECT SchemaName, TableName FROM @tables;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @schemaName, @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Processing table: ' + @schemaName + '.' + @tableName;
    
    -- Add IsDeleted
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_SCHEMA = @schemaName 
        AND TABLE_NAME = @tableName 
        AND COLUMN_NAME = 'IsDeleted'
    )
    BEGIN
        SET @sql = 'ALTER TABLE [' + @schemaName + '].[' + @tableName + '] ADD [IsDeleted] bit NOT NULL DEFAULT 0;';
        EXEC sp_executesql @sql;
        PRINT '  Added IsDeleted column';
    END
    ELSE
        PRINT '  IsDeleted column already exists';
    
    -- Add DeletedAt
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_SCHEMA = @schemaName 
        AND TABLE_NAME = @tableName 
        AND COLUMN_NAME = 'DeletedAt'
    )
    BEGIN
        SET @sql = 'ALTER TABLE [' + @schemaName + '].[' + @tableName + '] ADD [DeletedAt] datetime2 NULL;';
        EXEC sp_executesql @sql;
        PRINT '  Added DeletedAt column';
    END
    ELSE
        PRINT '  DeletedAt column already exists';
    
    -- Add DeletedBy
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_SCHEMA = @schemaName 
        AND TABLE_NAME = @tableName 
        AND COLUMN_NAME = 'DeletedBy'
    )
    BEGIN
        SET @sql = 'ALTER TABLE [' + @schemaName + '].[' + @tableName + '] ADD [DeletedBy] nvarchar(450) NULL;';
        EXEC sp_executesql @sql;
        PRINT '  Added DeletedBy column';
    END
    ELSE
        PRINT '  DeletedBy column already exists';
    
    -- Add DeleteReason
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_SCHEMA = @schemaName 
        AND TABLE_NAME = @tableName 
        AND COLUMN_NAME = 'DeleteReason'
    )
    BEGIN
        SET @sql = 'ALTER TABLE [' + @schemaName + '].[' + @tableName + '] ADD [DeleteReason] nvarchar(500) NULL;';
        EXEC sp_executesql @sql;
        PRINT '  Added DeleteReason column';
    END
    ELSE
        PRINT '  DeleteReason column already exists';
    
    PRINT '';
    
    FETCH NEXT FROM table_cursor INTO @schemaName, @tableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

PRINT 'Migration completed!';

