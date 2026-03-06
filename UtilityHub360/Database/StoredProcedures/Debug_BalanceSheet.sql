-- Debug script to check Balance Sheet calculations
-- Replace '<YourUserId>' with your actual UserId
-- Replace '<AsOfDate>' with your report date (e.g., '2026-01-16')

DECLARE @UserId NVARCHAR(450) = '<YourUserId>';
DECLARE @AsOfDate DATETIME2 = '<AsOfDate>';

PRINT '=== BANK ACCOUNTS (from Payments) ==='
SELECT 
    ba.AccountName,
    ISNULL((
        SELECT SUM(CASE 
            WHEN p.TransactionType = 'CREDIT' THEN ABS(p.Amount)
            WHEN p.TransactionType = 'DEBIT' THEN -ABS(p.Amount)
            ELSE 0
        END)
        FROM Payments p
        WHERE p.BankAccountId = ba.Id
            AND p.TransactionDate <= @AsOfDate
            AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
    ), 0) AS Balance
FROM BankAccounts ba
WHERE ba.UserId = @UserId
    AND ba.IsActive = 1
    AND LOWER(LTRIM(RTRIM(ISNULL(ba.AccountType, '')))) NOT IN ('credit_card', 'credit card', 'creditcard');

PRINT ''
PRINT '=== BANK STATEMENTS ==='
SELECT 
    ba.AccountName,
    ISNULL((
        SELECT SUM(bs.OpeningBalance)
        FROM BankStatements bs
        WHERE bs.BankAccountId = ba.Id
            AND bs.StatementEndDate <= @AsOfDate
    ), 0) AS StatementBalance
FROM BankAccounts ba
WHERE ba.UserId = @UserId
    AND ba.IsActive = 1
    AND EXISTS (
        SELECT 1
        FROM BankStatements bs
        WHERE bs.BankAccountId = ba.Id
            AND bs.StatementEndDate <= @AsOfDate
    );

PRINT ''
PRINT '=== SAVINGS ACCOUNTS ==='
SELECT 
    sa.AccountName,
    ISNULL((
        SELECT SUM(CASE 
            WHEN st.TransactionType = 'DEPOSIT' THEN st.Amount
            WHEN st.TransactionType = 'WITHDRAWAL' THEN -st.Amount
            ELSE 0
        END)
        FROM SavingsTransactions st
        WHERE st.SavingsAccountId = sa.Id
            AND st.TransactionDate <= @AsOfDate
            AND (st.IsDeleted IS NULL OR st.IsDeleted = 0)
    ), 0) AS SavingsBalance
FROM SavingsAccounts sa
WHERE sa.UserId = @UserId
    AND sa.IsActive = 1;

PRINT ''
PRINT '=== TOTALS ==='
DECLARE @BankTotal DECIMAL(18,2) = 0;
DECLARE @StatementTotal DECIMAL(18,2) = 0;
DECLARE @SavingsTotal DECIMAL(18,2) = 0;

-- Bank Accounts Total
SELECT @BankTotal = ISNULL(SUM(CalculatedBalance), 0)
FROM (
    SELECT 
        ISNULL((
            SELECT SUM(CASE 
                WHEN p.TransactionType = 'CREDIT' THEN ABS(p.Amount)
                WHEN p.TransactionType = 'DEBIT' THEN -ABS(p.Amount)
                ELSE 0
            END)
            FROM Payments p
            WHERE p.BankAccountId = ba.Id
                AND p.TransactionDate <= @AsOfDate
                AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
        ), 0) AS CalculatedBalance
    FROM BankAccounts ba
    WHERE ba.UserId = @UserId
        AND ba.IsActive = 1
        AND LOWER(LTRIM(RTRIM(ISNULL(ba.AccountType, '')))) NOT IN ('credit_card', 'credit card', 'creditcard')
) AS BankBalances
WHERE CalculatedBalance > 0;

-- Bank Statements Total
SELECT @StatementTotal = ISNULL(SUM(StatementBalance), 0)
FROM (
    SELECT 
        ISNULL((
            SELECT SUM(bs.OpeningBalance)
            FROM BankStatements bs
            WHERE bs.BankAccountId = ba.Id
                AND bs.StatementEndDate <= @AsOfDate
        ), 0) AS StatementBalance
    FROM BankAccounts ba
    WHERE ba.UserId = @UserId
        AND ba.IsActive = 1
        AND EXISTS (
            SELECT 1
            FROM BankStatements bs
            WHERE bs.BankAccountId = ba.Id
                AND bs.StatementEndDate <= @AsOfDate
        )
) AS StatementTotals
WHERE StatementBalance > 0;

-- Savings Total
SELECT @SavingsTotal = ISNULL(SUM(Amount), 0)
FROM (
    SELECT 
        ISNULL((
            SELECT SUM(CASE 
                WHEN st.TransactionType = 'DEPOSIT' THEN st.Amount
                WHEN st.TransactionType = 'WITHDRAWAL' THEN -st.Amount
                ELSE 0
            END)
            FROM SavingsTransactions st
            WHERE st.SavingsAccountId = sa.Id
                AND st.TransactionDate <= @AsOfDate
                AND (st.IsDeleted IS NULL OR st.IsDeleted = 0)
        ), 0) AS Amount
    FROM SavingsAccounts sa
    WHERE sa.UserId = @UserId
        AND sa.IsActive = 1
) AS SavingsTotals
WHERE Amount > 0;

SELECT 
    @BankTotal AS [Bank Accounts Total],
    @StatementTotal AS [Bank Statements Total],
    @SavingsTotal AS [Savings Total],
    (@BankTotal + @StatementTotal + @SavingsTotal) AS [TOTAL CURRENT ASSETS];
