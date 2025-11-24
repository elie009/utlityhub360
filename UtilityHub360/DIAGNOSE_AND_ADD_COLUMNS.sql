-- DIAGNOSTIC AND FIX SCRIPT
-- This script will diagnose the issue and add columns safely

-- Step 1: Verify table exists
PRINT '=== STEP 1: Checking if SavingsAccounts table exists ===';
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SavingsAccounts')
BEGIN
    PRINT '✓ SavingsAccounts table found';
    
    -- Show current columns
    PRINT '';
    PRINT 'Current columns in SavingsAccounts:';
    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'SavingsAccounts'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '✗ ERROR: SavingsAccounts table does not exist!';
    PRINT 'Please check your database connection and table name.';
    RETURN;
END
GO

-- Step 2: Check permissions
PRINT '';
PRINT '=== STEP 2: Checking permissions ===';
IF HAS_PERMS_BY_NAME('SavingsAccounts', 'OBJECT', 'ALTER') = 1
BEGIN
    PRINT '✓ You have ALTER TABLE permission';
END
ELSE
BEGIN
    PRINT '✗ ERROR: You do NOT have ALTER TABLE permission!';
    PRINT 'Contact your database administrator.';
    RETURN;
END
GO

-- Step 3: Add AccountType column
PRINT '';
PRINT '=== STEP 3: Adding AccountType column ===';
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'AccountType'
)
BEGIN
    BEGIN TRY
        ALTER TABLE SavingsAccounts ADD AccountType NVARCHAR(50) NULL;
        PRINT '✓ AccountType column added successfully';
    END TRY
    BEGIN CATCH
        PRINT '✗ ERROR adding AccountType:';
        PRINT '  Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
        PRINT '  Error Message: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'AccountType column already exists';
END
GO

-- Step 4: Add InterestRate column
PRINT '';
PRINT '=== STEP 4: Adding InterestRate column ===';
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestRate'
)
BEGIN
    BEGIN TRY
        ALTER TABLE SavingsAccounts ADD InterestRate DECIMAL(5,4) NULL;
        PRINT '✓ InterestRate column added successfully';
    END TRY
    BEGIN CATCH
        PRINT '✗ ERROR adding InterestRate:';
        PRINT '  Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
        PRINT '  Error Message: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'InterestRate column already exists';
END
GO

-- Step 5: Add InterestCompoundingFrequency column
PRINT '';
PRINT '=== STEP 5: Adding InterestCompoundingFrequency column ===';
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestCompoundingFrequency'
)
BEGIN
    BEGIN TRY
        ALTER TABLE SavingsAccounts ADD InterestCompoundingFrequency NVARCHAR(50) NULL;
        PRINT '✓ InterestCompoundingFrequency column added successfully';
    END TRY
    BEGIN CATCH
        PRINT '✗ ERROR adding InterestCompoundingFrequency:';
        PRINT '  Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
        PRINT '  Error Message: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'InterestCompoundingFrequency column already exists';
END
GO

-- Step 6: Add LastInterestCalculationDate column
PRINT '';
PRINT '=== STEP 6: Adding LastInterestCalculationDate column ===';
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'LastInterestCalculationDate'
)
BEGIN
    BEGIN TRY
        ALTER TABLE SavingsAccounts ADD LastInterestCalculationDate DATETIME2 NULL;
        PRINT '✓ LastInterestCalculationDate column added successfully';
    END TRY
    BEGIN CATCH
        PRINT '✗ ERROR adding LastInterestCalculationDate:';
        PRINT '  Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
        PRINT '  Error Message: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'LastInterestCalculationDate column already exists';
END
GO

-- Step 7: Add NextInterestCalculationDate column
PRINT '';
PRINT '=== STEP 7: Adding NextInterestCalculationDate column ===';
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'NextInterestCalculationDate'
)
BEGIN
    BEGIN TRY
        ALTER TABLE SavingsAccounts ADD NextInterestCalculationDate DATETIME2 NULL;
        PRINT '✓ NextInterestCalculationDate column added successfully';
    END TRY
    BEGIN CATCH
        PRINT '✗ ERROR adding NextInterestCalculationDate:';
        PRINT '  Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
        PRINT '  Error Message: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'NextInterestCalculationDate column already exists';
END
GO

-- Step 8: Set default values
PRINT '';
PRINT '=== STEP 8: Setting default values ===';
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SavingsAccounts') AND name = 'AccountType')
BEGIN
    BEGIN TRY
        UPDATE SavingsAccounts SET AccountType = 'REGULAR' WHERE AccountType IS NULL;
        PRINT '✓ Default AccountType set to REGULAR';
    END TRY
    BEGIN CATCH
        PRINT '✗ ERROR setting default AccountType:';
        PRINT '  Error Message: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT '⚠ AccountType column does not exist, skipping default value';
END
GO

-- Step 9: Create indexes
PRINT '';
PRINT '=== STEP 9: Creating indexes ===';

-- Index for NextInterestCalculationDate
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SavingsAccounts') AND name = 'NextInterestCalculationDate')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingsAccounts_NextInterestCalculationDate' AND object_id = OBJECT_ID(N'SavingsAccounts'))
    BEGIN
        BEGIN TRY
            CREATE INDEX IX_SavingsAccounts_NextInterestCalculationDate 
            ON SavingsAccounts(NextInterestCalculationDate) 
            WHERE NextInterestCalculationDate IS NOT NULL;
            PRINT '✓ Index IX_SavingsAccounts_NextInterestCalculationDate created';
        END TRY
        BEGIN CATCH
            PRINT '✗ ERROR creating index:';
            PRINT '  Error Message: ' + ERROR_MESSAGE();
        END CATCH
    END
    ELSE
    BEGIN
        PRINT 'Index IX_SavingsAccounts_NextInterestCalculationDate already exists';
    END
END

-- Index for AccountType
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SavingsAccounts') AND name = 'AccountType')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingsAccounts_AccountType' AND object_id = OBJECT_ID(N'SavingsAccounts'))
    BEGIN
        BEGIN TRY
            CREATE INDEX IX_SavingsAccounts_AccountType 
            ON SavingsAccounts(AccountType) 
            WHERE AccountType IS NOT NULL;
            PRINT '✓ Index IX_SavingsAccounts_AccountType created';
        END TRY
        BEGIN CATCH
            PRINT '✗ ERROR creating index:';
            PRINT '  Error Message: ' + ERROR_MESSAGE();
        END CATCH
    END
    ELSE
    BEGIN
        PRINT 'Index IX_SavingsAccounts_AccountType already exists';
    END
END
GO

-- Final verification
PRINT '';
PRINT '=== FINAL VERIFICATION ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SavingsAccounts'
AND COLUMN_NAME IN (
    'AccountType',
    'InterestRate',
    'InterestCompoundingFrequency',
    'LastInterestCalculationDate',
    'NextInterestCalculationDate'
)
ORDER BY COLUMN_NAME;

IF @@ROWCOUNT = 5
BEGIN
    PRINT '';
    PRINT '✅ SUCCESS: All 5 columns have been added!';
END
ELSE
BEGIN
    PRINT '';
    PRINT '⚠ WARNING: Not all columns were added. Check the error messages above.';
END
GO

