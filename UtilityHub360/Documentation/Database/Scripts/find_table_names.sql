-- =====================================================
-- Find Table Names and Schemas
-- Run this to see what tables actually exist
-- =====================================================

PRINT '=== All Tables in Database ===';
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;

PRINT '';
PRINT '=== Looking for Bank-related Tables ===';
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND (TABLE_NAME LIKE '%Bank%' 
       OR TABLE_NAME LIKE '%Payment%'
       OR TABLE_NAME LIKE '%Bill%'
       OR TABLE_NAME LIKE '%Income%'
       OR TABLE_NAME LIKE '%Expense%'
       OR TABLE_NAME LIKE '%Savings%'
       OR TABLE_NAME LIKE '%Transaction%')
ORDER BY TABLE_SCHEMA, TABLE_NAME;

