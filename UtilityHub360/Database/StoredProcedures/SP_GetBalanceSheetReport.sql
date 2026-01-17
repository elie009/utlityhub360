USE [DBUTILS]
GO
/****** Object:  StoredProcedure [sa01].[SP_GetBalanceSheetReport]    Script Date: 1/16/2026 5:32:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Stored Procedure: SP_GetBalanceSheetReport
-- Description: Generates Balance Sheet data for RDLC reports
-- Parameters: 
--   @UserId - User identifier
--   @AsOfDate - Balance Sheet snapshot date
--   @StartDate - Optional start date for filtering transactions
--   @EndDate - Optional end date for filtering transactions
-- =============================================

ALTER   PROCEDURE [sa01].[SP_GetBalanceSheetReport]
    @UserId NVARCHAR(450),
    @AsOfDate DATETIME2,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Use AsOfDate as the end date if EndDate is not provided
    DECLARE @ReportEndDate DATETIME2 = ISNULL(@EndDate, @AsOfDate);
    DECLARE @ReportStartDate DATETIME2 = @StartDate;

    -- =============================================
    -- ASSETS SECTION
    -- =============================================
    
       -- Current Assets: Bank Accounts (excluding credit cards) - calculated from Payments (CREDIT - DEBIT)
    SELECT 
        'CURRENT_ASSET' AS ItemType,
        'Bank Account' AS Category,
        ba.AccountName,
        ba.AccountType,
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
        ), 0) AS Amount,
        ba.AccountType + ' - ' + ba.AccountName + ' (Balance as of ' + FORMAT(@AsOfDate, 'MMM dd, yyyy') + ')' AS Description,
        ba.Id AS ReferenceId
    FROM BankAccounts ba
    WHERE ba.UserId = @UserId
        AND ba.IsActive = 1
        AND LOWER(LTRIM(RTRIM(ISNULL(ba.AccountType, '')))) NOT IN ('credit_card', 'credit card', 'creditcard')
        AND (
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
            ), 0) > 0
        )
    
    UNION ALL


     -- Bank Statements Opening Balance (aggregated by bank account)
    SELECT 
        'CURRENT_ASSET' AS ItemType,
        'Bank Statement' AS Category,
        ba.AccountName + ' (Statement Balance)' AS AccountName,
        'Bank Statement' AS AccountType,
        ISNULL((
            SELECT SUM(bs.OpeningBalance)
            FROM BankStatements bs
            WHERE bs.BankAccountId = ba.Id
                AND bs.StatementEndDate <= @AsOfDate
        ), 0) AS Amount,
        'Statement Balance - ' + ba.AccountName + ' (as of ' + FORMAT(@AsOfDate, 'MMM dd, yyyy') + ')' AS Description,
        ba.Id AS ReferenceId
    FROM BankAccounts ba
    WHERE ba.UserId = @UserId
        AND ba.IsActive = 1
        AND EXISTS (
            SELECT 1
            FROM BankStatements bs
            WHERE bs.BankAccountId = ba.Id
                AND bs.StatementEndDate <= @AsOfDate
        )
        AND (
            ISNULL((
                SELECT SUM(bs.OpeningBalance)
                FROM BankStatements bs
                WHERE bs.BankAccountId = ba.Id
                    AND bs.StatementEndDate <= @AsOfDate
            ), 0) > 0
        )

    UNION ALL

    -- Savings Accounts - calculate balance as deposits minus withdrawals up to AsOfDate
    SELECT 
        'CURRENT_ASSET' AS ItemType,
        'Savings' AS Category,
        AccountName,
        'Savings Account' AS AccountType,
        Amount,
        Description,
        ReferenceId
    FROM (
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
            ), 0) AS Amount,
            'Savings - ' + sa.AccountName AS Description,
            sa.Id AS ReferenceId
        FROM SavingsAccounts sa
        WHERE sa.UserId = @UserId
            AND sa.IsActive = 1
    ) AS SavingsData
    WHERE Amount > 0

    UNION ALL

    -- Fixed Assets: Real Estate Investments
    SELECT 
        'FIXED_ASSET' AS ItemType,
        'Property' AS Category,
        inv.AccountName,
        'Property' AS AccountType,
        inv.CurrentValue AS Amount,
        'Property - ' + inv.AccountName AS Description,
        inv.Id AS ReferenceId
    FROM Investments inv
    WHERE inv.UserId = @UserId
        AND (inv.IsDeleted IS NULL OR inv.IsDeleted = 0)
        AND inv.IsActive = 1
        AND inv.InvestmentType = 'REAL_ESTATE'
        AND inv.CurrentValue > 0

    UNION ALL

    -- Other Assets: Non-Real Estate Investments
    SELECT 
        'OTHER_ASSET' AS ItemType,
        ISNULL(inv.InvestmentType, 'Investment') AS Category,
        inv.AccountName,
        ISNULL(inv.InvestmentType, 'Investment') AS AccountType,
        inv.CurrentValue AS Amount,
        ISNULL(inv.InvestmentType, 'Investment') + ' - ' + inv.AccountName AS Description,
        inv.Id AS ReferenceId
    FROM Investments inv
    WHERE inv.UserId = @UserId
        AND (inv.IsDeleted IS NULL OR inv.IsDeleted = 0)
        AND inv.IsActive = 1
        AND inv.InvestmentType <> 'REAL_ESTATE'
        AND inv.CurrentValue > 0

    UNION ALL

    -- =============================================
    -- LIABILITIES SECTION
    -- =============================================

    -- Current Liabilities: Credit Card Balances
    SELECT 
        'CURRENT_LIABILITY' AS ItemType,
        'Credit Card' AS Category,
        ba.AccountName,
        'Credit Card' AS AccountType,
        ba.CurrentBalance AS Amount,
        'Credit Card - ' + ba.AccountName + ' (Outstanding Balance)' AS Description,
        ba.Id AS ReferenceId
    FROM BankAccounts ba
    WHERE ba.UserId = @UserId
        AND ba.IsActive = 1
        AND LOWER(LTRIM(RTRIM(ISNULL(ba.AccountType, '')))) IN ('credit_card', 'credit card', 'creditcard')
        AND ba.CurrentBalance > 0

    UNION ALL

    -- Current Liabilities: Unpaid Bills due on/before AsOfDate
    SELECT 
        'CURRENT_LIABILITY' AS ItemType,
        ISNULL(b.BillType, 'Bill') AS Category,
        ISNULL(b.Provider, 'Unnamed Bill') AS AccountName,
        ISNULL(b.BillType, 'Bill') AS AccountType,
        b.Amount,
        ISNULL(b.BillType, 'Bill') + ' - ' + ISNULL(b.Provider, 'Unnamed Bill') + 
            ' (' + CASE WHEN b.DueDate <= @AsOfDate THEN 'Overdue' ELSE 'Pending' END + 
            ', Due: ' + FORMAT(b.DueDate, 'MMM dd, yyyy') + ')' AS Description,
        b.Id AS ReferenceId
    FROM Bills b
    WHERE b.UserId = @UserId
        AND (b.Status IS NULL OR UPPER(LTRIM(RTRIM(b.Status))) <> 'PAID')
        AND (b.IsDeleted IS NULL OR b.IsDeleted = 0)
        AND b.DueDate <= DATEADD(DAY, 1, @AsOfDate)

    UNION ALL

    -- Long-term Liabilities: Active Loans
    SELECT 
        'LONG_TERM_LIABILITY' AS ItemType,
        'Loan Payable' AS Category,
        ISNULL(l.Purpose, 'Loan') AS AccountName,
        'Loan Payable' AS AccountType,
        l.RemainingBalance AS Amount,
        'Loan - ' + ISNULL(l.Purpose, 'Loan') + ' (Remaining: ' + FORMAT(l.RemainingBalance, 'C') + ')' AS Description,
        l.Id AS ReferenceId
    FROM Loans l
    WHERE l.UserId = @UserId
        AND l.Status IS NOT NULL
        AND UPPER(LTRIM(RTRIM(l.Status))) NOT IN ('REJECTED', 'COMPLETED')
        AND l.RemainingBalance > 0;

    -- =============================================
    -- SUMMARY TOTALS
    -- =============================================
    
    -- Calculate totals for the report
    DECLARE @TotalCurrentAssets DECIMAL(18,2) = 0;
    DECLARE @TotalFixedAssets DECIMAL(18,2) = 0;
    DECLARE @TotalOtherAssets DECIMAL(18,2) = 0;
    DECLARE @TotalAssets DECIMAL(18,2) = 0;
    DECLARE @TotalCurrentLiabilities DECIMAL(18,2) = 0;
    DECLARE @TotalLongTermLiabilities DECIMAL(18,2) = 0;
    DECLARE @TotalLiabilities DECIMAL(18,2) = 0;
    DECLARE @TotalEquity DECIMAL(18,2) = 0;

    -- Current Assets Total - calculated from Payments (CREDIT - DEBIT)
    SELECT @TotalCurrentAssets = ISNULL(SUM(CalculatedBalance), 0)
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

    -- Add Savings Accounts to Current Assets Total
    SELECT @TotalCurrentAssets = @TotalCurrentAssets + ISNULL(SUM(Amount), 0)
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

    -- Add Bank Statements Opening Balance to Current Assets Total
    SELECT @TotalCurrentAssets = @TotalCurrentAssets + ISNULL(SUM(StatementBalance), 0)
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

    -- Fixed Assets Total
    SELECT @TotalFixedAssets = ISNULL(SUM(inv.CurrentValue), 0)
    FROM Investments inv
    WHERE inv.UserId = @UserId
        AND (inv.IsDeleted IS NULL OR inv.IsDeleted = 0)
        AND inv.IsActive = 1
        AND inv.InvestmentType = 'REAL_ESTATE'
        AND inv.CurrentValue > 0;

    -- Other Assets Total
    SELECT @TotalOtherAssets = ISNULL(SUM(inv.CurrentValue), 0)
    FROM Investments inv
    WHERE inv.UserId = @UserId
        AND (inv.IsDeleted IS NULL OR inv.IsDeleted = 0)
        AND inv.IsActive = 1
        AND inv.InvestmentType <> 'REAL_ESTATE'
        AND inv.CurrentValue > 0;

    -- Total Assets
    SET @TotalAssets = @TotalCurrentAssets + @TotalFixedAssets + @TotalOtherAssets;

    -- Current Liabilities Total
    SELECT @TotalCurrentLiabilities = ISNULL(SUM(ba.CurrentBalance), 0)
    FROM BankAccounts ba
    WHERE ba.UserId = @UserId
        AND ba.IsActive = 1
        AND LOWER(LTRIM(RTRIM(ISNULL(ba.AccountType, '')))) IN ('credit_card', 'credit card', 'creditcard')
        AND ba.CurrentBalance > 0;

    SELECT @TotalCurrentLiabilities = @TotalCurrentLiabilities + ISNULL(SUM(b.Amount), 0)
    FROM Bills b
    WHERE b.UserId = @UserId
        AND (b.Status IS NULL OR UPPER(LTRIM(RTRIM(b.Status))) <> 'PAID')
        AND (b.IsDeleted IS NULL OR b.IsDeleted = 0)
        AND b.DueDate <= DATEADD(DAY, 1, @AsOfDate);

    -- Long-term Liabilities Total
    SELECT @TotalLongTermLiabilities = ISNULL(SUM(l.RemainingBalance), 0)
    FROM Loans l
    WHERE l.UserId = @UserId
        AND l.Status IS NOT NULL
        AND UPPER(LTRIM(RTRIM(l.Status))) NOT IN ('REJECTED', 'COMPLETED')
        AND l.RemainingBalance > 0;

    -- Total Liabilities
    SET @TotalLiabilities = @TotalCurrentLiabilities + @TotalLongTermLiabilities;

    -- Total Equity (Assets - Liabilities)
    SET @TotalEquity = @TotalAssets - @TotalLiabilities;

    -- Return summary totals
    SELECT 
        @AsOfDate AS AsOfDate,
        @TotalCurrentAssets AS TotalCurrentAssets,
        @TotalFixedAssets AS TotalFixedAssets,
        @TotalOtherAssets AS TotalOtherAssets,
        @TotalAssets AS TotalAssets,
        @TotalCurrentLiabilities AS TotalCurrentLiabilities,
        @TotalLongTermLiabilities AS TotalLongTermLiabilities,
        @TotalLiabilities AS TotalLiabilities,
        @TotalEquity AS TotalEquity,
        CASE WHEN ABS(@TotalAssets - (@TotalLiabilities + @TotalEquity)) < 0.01 THEN 1 ELSE 0 END AS IsBalanced;
END;
