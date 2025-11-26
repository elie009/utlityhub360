-- UNIVERSAL SCRIPT: Add columns to savings table regardless of name/schema
-- This will try common table names and add columns

-- Try different table name variations
DECLARE @TableName NVARCHAR(255);
DECLARE @SchemaName NVARCHAR(255);
DECLARE @FullTableName NVARCHAR(500);
DECLARE @SQL NVARCHAR(MAX);

-- Find the table
SELECT TOP 1
    @SchemaName = s.name,
    @TableName = t.name
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name LIKE '%saving%' 
   OR t.name LIKE '%Saving%' 
   OR t.name LIKE '%SAVING%'
   OR t.name = 'SavingsAccounts'
   OR t.name = 'savings_accounts'
ORDER BY 
    CASE WHEN t.name = 'SavingsAccounts' THEN 1 ELSE 2 END;

IF @TableName IS NULL
BEGIN
    PRINT 'ERROR: Could not find savings table!';
    PRINT 'Please run FIND_TABLE_AND_ADD_COLUMNS.sql first to find your table name.';
    RETURN;
END

SET @FullTableName = QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName);
PRINT 'Found table: ' + @FullTableName;
PRINT '';

-- Add AccountType
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = @SchemaName AND t.name = @TableName AND c.name = 'AccountType'
)
BEGIN
    SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD AccountType NVARCHAR(50) NULL;';
    EXEC sp_executesql @SQL;
    PRINT '✓ Added AccountType to ' + @FullTableName;
END
ELSE
    PRINT 'AccountType already exists';

-- Add InterestRate
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = @SchemaName AND t.name = @TableName AND c.name = 'InterestRate'
)
BEGIN
    SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD InterestRate DECIMAL(5,4) NULL;';
    EXEC sp_executesql @SQL;
    PRINT '✓ Added InterestRate to ' + @FullTableName;
END
ELSE
    PRINT 'InterestRate already exists';

-- Add InterestCompoundingFrequency
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = @SchemaName AND t.name = @TableName AND c.name = 'InterestCompoundingFrequency'
)
BEGIN
    SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD InterestCompoundingFrequency NVARCHAR(50) NULL;';
    EXEC sp_executesql @SQL;
    PRINT '✓ Added InterestCompoundingFrequency to ' + @FullTableName;
END
ELSE
    PRINT 'InterestCompoundingFrequency already exists';

-- Add LastInterestCalculationDate
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = @SchemaName AND t.name = @TableName AND c.name = 'LastInterestCalculationDate'
)
BEGIN
    SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD LastInterestCalculationDate DATETIME2 NULL;';
    EXEC sp_executesql @SQL;
    PRINT '✓ Added LastInterestCalculationDate to ' + @FullTableName;
END
ELSE
    PRINT 'LastInterestCalculationDate already exists';

-- Add NextInterestCalculationDate
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = @SchemaName AND t.name = @TableName AND c.name = 'NextInterestCalculationDate'
)
BEGIN
    SET @SQL = 'ALTER TABLE ' + @FullTableName + ' ADD NextInterestCalculationDate DATETIME2 NULL;';
    EXEC sp_executesql @SQL;
    PRINT '✓ Added NextInterestCalculationDate to ' + @FullTableName;
END
ELSE
    PRINT 'NextInterestCalculationDate already exists';

-- Set default AccountType
IF EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = @SchemaName AND t.name = @TableName AND c.name = 'AccountType'
)
BEGIN
    SET @SQL = 'UPDATE ' + @FullTableName + ' SET AccountType = ''REGULAR'' WHERE AccountType IS NULL;';
    EXEC sp_executesql @SQL;
    PRINT '✓ Set default AccountType to REGULAR';
END

PRINT '';
PRINT '✅ Migration completed!';
PRINT 'Table used: ' + @FullTableName;

-- Verify
PRINT '';
PRINT 'Verification - Columns added:';
SET @SQL = 'SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = ''' + @SchemaName + ''' AND TABLE_NAME = ''' + @TableName + ''' AND COLUMN_NAME IN (''AccountType'', ''InterestRate'', ''InterestCompoundingFrequency'', ''LastInterestCalculationDate'', ''NextInterestCalculationDate'') ORDER BY COLUMN_NAME;';
EXEC sp_executesql @SQL;

