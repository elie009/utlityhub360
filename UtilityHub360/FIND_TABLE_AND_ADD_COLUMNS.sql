-- FIND THE CORRECT TABLE NAME AND ADD COLUMNS
-- This script will find your savings table and add the columns

-- Step 1: Find all tables with "saving" in the name (case-insensitive)
PRINT '=== STEP 1: Finding Savings-related tables ===';
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    'Found table: ' + TABLE_SCHEMA + '.' + TABLE_NAME AS FullTableName
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%saving%' OR TABLE_NAME LIKE '%Saving%' OR TABLE_NAME LIKE '%SAVING%'
ORDER BY TABLE_SCHEMA, TABLE_NAME;

-- Step 2: Show all tables to help identify
PRINT '';
PRINT '=== STEP 2: All tables in database ===';
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;

-- Step 3: Try to find the table using sys.tables
PRINT '';
PRINT '=== STEP 3: Finding table using sys.tables ===';
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    'Found: ' + s.name + '.' + t.name AS FullName
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name LIKE '%saving%' OR t.name LIKE '%Saving%' OR t.name LIKE '%SAVING%'
ORDER BY s.name, t.name;

GO

-- Step 4: Once you know the table name, uncomment and modify the section below
-- Replace 'YourSchema' and 'YourTableName' with the actual values from above

/*
-- Example: If table is in 'dbo' schema and named 'SavingsAccounts'
ALTER TABLE dbo.SavingsAccounts ADD AccountType NVARCHAR(50) NULL;
ALTER TABLE dbo.SavingsAccounts ADD InterestRate DECIMAL(5,4) NULL;
ALTER TABLE dbo.SavingsAccounts ADD InterestCompoundingFrequency NVARCHAR(50) NULL;
ALTER TABLE dbo.SavingsAccounts ADD LastInterestCalculationDate DATETIME2 NULL;
ALTER TABLE dbo.SavingsAccounts ADD NextInterestCalculationDate DATETIME2 NULL;

UPDATE dbo.SavingsAccounts SET AccountType = 'REGULAR' WHERE AccountType IS NULL;
*/

PRINT '';
PRINT '=== INSTRUCTIONS ===';
PRINT '1. Look at the results above to find your table name';
PRINT '2. Note the SchemaName and TableName';
PRINT '3. Use the format: ALTER TABLE [SchemaName].[TableName] ADD ...';
PRINT '4. Or if no schema, just: ALTER TABLE [TableName] ADD ...';
GO

