-- First, let's check what tables exist and find the Bills table
-- Run this first to identify the correct table name

-- Check if Bills table exists in dbo schema
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT '✓ Found Bills table in dbo schema';
    SELECT 'Bills table found in dbo schema' AS Result;
END
ELSE
BEGIN
    PRINT '⚠ Bills table NOT found in dbo schema';
    
    -- Check all schemas for Bills table
    SELECT 
        s.name AS SchemaName,
        t.name AS TableName
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name LIKE '%Bill%'
    ORDER BY s.name, t.name;
    
    PRINT 'Please check the results above to find the correct table name and schema.';
END
GO

-- Check current database
SELECT DB_NAME() AS CurrentDatabase;
GO

-- List all tables in current database
SELECT 
    SCHEMA_NAME(schema_id) AS SchemaName,
    name AS TableName
FROM sys.tables
WHERE name LIKE '%Bill%'
ORDER BY SchemaName, TableName;
GO

