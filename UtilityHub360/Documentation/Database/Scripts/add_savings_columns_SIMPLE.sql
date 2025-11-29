-- SIMPLE VERSION: Add columns one at a time
-- Run each section separately if you encounter errors
-- This is the safest approach

-- Step 1: Add AccountType
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'AccountType'
)
BEGIN
    ALTER TABLE SavingsAccounts ADD AccountType NVARCHAR(50) NULL;
    PRINT '✓ AccountType added';
END
ELSE
    PRINT 'AccountType already exists';

-- Step 2: Add InterestRate
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestRate'
)
BEGIN
    ALTER TABLE SavingsAccounts ADD InterestRate DECIMAL(5,4) NULL;
    PRINT '✓ InterestRate added';
END
ELSE
    PRINT 'InterestRate already exists';

-- Step 3: Add InterestCompoundingFrequency
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestCompoundingFrequency'
)
BEGIN
    ALTER TABLE SavingsAccounts ADD InterestCompoundingFrequency NVARCHAR(50) NULL;
    PRINT '✓ InterestCompoundingFrequency added';
END
ELSE
    PRINT 'InterestCompoundingFrequency already exists';

-- Step 4: Add LastInterestCalculationDate
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'LastInterestCalculationDate'
)
BEGIN
    ALTER TABLE SavingsAccounts ADD LastInterestCalculationDate DATETIME2 NULL;
    PRINT '✓ LastInterestCalculationDate added';
END
ELSE
    PRINT 'LastInterestCalculationDate already exists';

-- Step 5: Add NextInterestCalculationDate
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'NextInterestCalculationDate'
)
BEGIN
    ALTER TABLE SavingsAccounts ADD NextInterestCalculationDate DATETIME2 NULL;
    PRINT '✓ NextInterestCalculationDate added';
END
ELSE
    PRINT 'NextInterestCalculationDate already exists';

-- Step 6: Set defaults (only if columns exist)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SavingsAccounts') AND name = 'AccountType')
BEGIN
    UPDATE SavingsAccounts SET AccountType = 'REGULAR' WHERE AccountType IS NULL;
    PRINT '✓ Default AccountType set';
END

-- Step 7: Create indexes (only if columns exist)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SavingsAccounts') AND name = 'NextInterestCalculationDate')
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingsAccounts_NextInterestCalculationDate' AND object_id = OBJECT_ID(N'SavingsAccounts'))
BEGIN
    CREATE INDEX IX_SavingsAccounts_NextInterestCalculationDate 
    ON SavingsAccounts(NextInterestCalculationDate) 
    WHERE NextInterestCalculationDate IS NOT NULL;
    PRINT '✓ Index IX_SavingsAccounts_NextInterestCalculationDate created';
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SavingsAccounts') AND name = 'AccountType')
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingsAccounts_AccountType' AND object_id = OBJECT_ID(N'SavingsAccounts'))
BEGIN
    CREATE INDEX IX_SavingsAccounts_AccountType 
    ON SavingsAccounts(AccountType) 
    WHERE AccountType IS NOT NULL;
    PRINT '✓ Index IX_SavingsAccounts_AccountType created';
END

PRINT 'All done!';

