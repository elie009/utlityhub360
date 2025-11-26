-- Migration: Add AccountType, InterestRate, and Interest Calculation fields to SavingsAccounts
-- SAFE VERSION: Checks if columns exist before adding them (can be run multiple times)
-- Run this script to add support for different savings account types (High-yield, CD, Money Market, etc.)
-- and interest calculation automation

-- Add AccountType column (nullable, defaults to REGULAR for existing accounts)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'AccountType'
)
BEGIN
    ALTER TABLE SavingsAccounts
    ADD AccountType NVARCHAR(50) NULL;
    PRINT 'AccountType column added successfully.';
END
ELSE
BEGIN
    PRINT 'AccountType column already exists.';
END
GO

-- Add InterestRate column (nullable, for accounts that earn interest)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestRate'
)
BEGIN
    ALTER TABLE SavingsAccounts
    ADD InterestRate DECIMAL(5,4) NULL;
    PRINT 'InterestRate column added successfully.';
END
ELSE
BEGIN
    PRINT 'InterestRate column already exists.';
END
GO

-- Add InterestCompoundingFrequency column (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestCompoundingFrequency'
)
BEGIN
    ALTER TABLE SavingsAccounts
    ADD InterestCompoundingFrequency NVARCHAR(50) NULL;
    PRINT 'InterestCompoundingFrequency column added successfully.';
END
ELSE
BEGIN
    PRINT 'InterestCompoundingFrequency column already exists.';
END
GO

-- Add LastInterestCalculationDate column (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'LastInterestCalculationDate'
)
BEGIN
    ALTER TABLE SavingsAccounts
    ADD LastInterestCalculationDate DATETIME2 NULL;
    PRINT 'LastInterestCalculationDate column added successfully.';
END
ELSE
BEGIN
    PRINT 'LastInterestCalculationDate column already exists.';
END
GO

-- Add NextInterestCalculationDate column (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'NextInterestCalculationDate'
)
BEGIN
    ALTER TABLE SavingsAccounts
    ADD NextInterestCalculationDate DATETIME2 NULL;
    PRINT 'NextInterestCalculationDate column added successfully.';
END
ELSE
BEGIN
    PRINT 'NextInterestCalculationDate column already exists.';
END
GO

-- Set default AccountType to REGULAR for existing accounts
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'AccountType'
)
BEGIN
    UPDATE SavingsAccounts
    SET AccountType = 'REGULAR'
    WHERE AccountType IS NULL;
    PRINT 'Default AccountType set to REGULAR for existing accounts.';
END
GO

-- Optional: Set default InterestCompoundingFrequency to MONTHLY for accounts with interest rates
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestCompoundingFrequency'
) AND EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'InterestRate'
)
BEGIN
    UPDATE SavingsAccounts
    SET InterestCompoundingFrequency = 'MONTHLY'
    WHERE InterestRate IS NOT NULL AND InterestCompoundingFrequency IS NULL;
    PRINT 'Default InterestCompoundingFrequency set to MONTHLY for accounts with interest rates.';
END
GO

-- Add index for querying accounts that need interest calculation
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'NextInterestCalculationDate'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE name = 'IX_SavingsAccounts_NextInterestCalculationDate' 
        AND object_id = OBJECT_ID(N'SavingsAccounts')
    )
    BEGIN
        CREATE INDEX IX_SavingsAccounts_NextInterestCalculationDate 
        ON SavingsAccounts(NextInterestCalculationDate) 
        WHERE NextInterestCalculationDate IS NOT NULL;
        PRINT 'Index IX_SavingsAccounts_NextInterestCalculationDate created successfully.';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_SavingsAccounts_NextInterestCalculationDate already exists.';
    END
END
GO

-- Add index for querying by AccountType
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'SavingsAccounts') 
    AND name = 'AccountType'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE name = 'IX_SavingsAccounts_AccountType' 
        AND object_id = OBJECT_ID(N'SavingsAccounts')
    )
    BEGIN
        CREATE INDEX IX_SavingsAccounts_AccountType 
        ON SavingsAccounts(AccountType) 
        WHERE AccountType IS NOT NULL;
        PRINT 'Index IX_SavingsAccounts_AccountType created successfully.';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_SavingsAccounts_AccountType already exists.';
    END
END
GO

PRINT 'Migration completed successfully. Savings accounts now support account types and interest calculation.';
GO

