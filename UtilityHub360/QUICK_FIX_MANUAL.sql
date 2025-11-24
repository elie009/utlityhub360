-- QUICK FIX: Run these commands ONE AT A TIME
-- Copy and paste each ALTER TABLE statement separately
-- Check for errors after each one

-- 1. Add AccountType
ALTER TABLE SavingsAccounts ADD AccountType NVARCHAR(50) NULL;

-- 2. Add InterestRate  
ALTER TABLE SavingsAccounts ADD InterestRate DECIMAL(5,4) NULL;

-- 3. Add InterestCompoundingFrequency
ALTER TABLE SavingsAccounts ADD InterestCompoundingFrequency NVARCHAR(50) NULL;

-- 4. Add LastInterestCalculationDate
ALTER TABLE SavingsAccounts ADD LastInterestCalculationDate DATETIME2 NULL;

-- 5. Add NextInterestCalculationDate
ALTER TABLE SavingsAccounts ADD NextInterestCalculationDate DATETIME2 NULL;

-- 6. Set default AccountType
UPDATE SavingsAccounts SET AccountType = 'REGULAR' WHERE AccountType IS NULL;

-- 7. Create index for NextInterestCalculationDate
CREATE INDEX IX_SavingsAccounts_NextInterestCalculationDate 
ON SavingsAccounts(NextInterestCalculationDate) 
WHERE NextInterestCalculationDate IS NOT NULL;

-- 8. Create index for AccountType
CREATE INDEX IX_SavingsAccounts_AccountType 
ON SavingsAccounts(AccountType) 
WHERE AccountType IS NOT NULL;

