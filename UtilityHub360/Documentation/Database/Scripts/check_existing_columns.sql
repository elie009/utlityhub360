-- =====================================================
-- Diagnostic Script: Check Existing Soft Delete Columns
-- Run this first to see what columns already exist
-- =====================================================

PRINT 'Checking existing soft delete columns in all tables...';
PRINT '';

-- Check BankAccounts
PRINT '=== BankAccounts Table ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'BankAccounts'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY COLUMN_NAME;

-- Check BankTransactions
PRINT '';
PRINT '=== BankTransactions Table ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'BankTransactions'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY COLUMN_NAME;

-- Check Payments
PRINT '';
PRINT '=== Payments Table ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'Payments'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY COLUMN_NAME;

-- Check Bills
PRINT '';
PRINT '=== Bills Table ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'Bills'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY COLUMN_NAME;

-- Check IncomeSources
PRINT '';
PRINT '=== IncomeSources Table ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'IncomeSources'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY COLUMN_NAME;

-- Check VariableExpenses
PRINT '';
PRINT '=== VariableExpenses Table ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'VariableExpenses'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY COLUMN_NAME;

-- Check SavingsTransactions
PRINT '';
PRINT '=== SavingsTransactions Table ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME = 'SavingsTransactions'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY COLUMN_NAME;

PRINT '';
PRINT 'Diagnostic check completed!';
PRINT 'Review the results above to see which columns already exist.';

