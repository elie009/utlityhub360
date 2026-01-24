-- =============================================
-- Stored Procedure: SP_GetCashFlowReport
-- Description: Generates Cash Flow Statement data for RDLC reports
-- Parameters: 
--   @UserId - User identifier
--   @StartDate - Report period start date
--   @EndDate - Report period end date
-- =============================================
-- Deploy to [sa01] schema if your tables (BankAccounts, etc.) are in sa01.
-- Use: CREATE OR ALTER PROCEDURE [sa01].[SP_GetCashFlowReport] ...
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[SP_GetCashFlowReport]
    @UserId NVARCHAR(450),
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- =============================================
    -- RESULT SET 1: CashFlowData (detail rows)
    -- SectionType: OPERATING | INVESTING | FINANCING
    -- ActivityType: INFLOW | OUTFLOW
    -- =============================================

    -- Operating: Income (inflows) - Payments CREDIT, no Loan/Bill/Savings
    SELECT 
        'OPERATING' AS SectionType,
        'INFLOW' AS ActivityType,
        ISNULL(p.Category, 'Income') AS CategoryName,
        ISNULL(p.Description, 'Income') AS Description,
        p.Amount,
        p.TransactionDate
    FROM Payments p
    WHERE p.UserId = @UserId
        AND p.TransactionType = 'CREDIT'
        AND p.TransactionDate >= @StartDate
        AND p.TransactionDate <= @EndDate
        AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
        AND p.LoanId IS NULL
        AND p.BillId IS NULL
        AND p.SavingsAccountId IS NULL

    UNION ALL

    -- Operating: Income from BankTransactions
    SELECT 
        'OPERATING' AS SectionType,
        'INFLOW' AS ActivityType,
        ISNULL(bt.Category, 'Income') AS CategoryName,
        ISNULL(bt.Description, 'Income') AS Description,
        bt.Amount,
        bt.TransactionDate
    FROM BankTransactions bt
    WHERE bt.UserId = @UserId
        AND bt.TransactionType = 'CREDIT'
        AND bt.TransactionDate >= @StartDate
        AND bt.TransactionDate <= @EndDate
        AND (bt.IsDeleted IS NULL OR bt.IsDeleted = 0)

    UNION ALL

    -- Operating: Expenses (outflows) - Payments DEBIT, no Bill/Loan/Savings
    SELECT 
        'OPERATING' AS SectionType,
        'OUTFLOW' AS ActivityType,
        ISNULL(p.Category, 'Expense') AS CategoryName,
        ISNULL(p.Description, 'Expense') AS Description,
        p.Amount,
        p.TransactionDate
    FROM Payments p
    WHERE p.UserId = @UserId
        AND p.TransactionType = 'DEBIT'
        AND p.TransactionDate >= @StartDate
        AND p.TransactionDate <= @EndDate
        AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
        AND p.BillId IS NULL
        AND p.LoanId IS NULL
        AND p.SavingsAccountId IS NULL

    UNION ALL

    -- Operating: Expenses from BankTransactions
    SELECT 
        'OPERATING' AS SectionType,
        'OUTFLOW' AS ActivityType,
        ISNULL(bt.Category, 'Expense') AS CategoryName,
        ISNULL(bt.Description, 'Expense') AS Description,
        bt.Amount,
        bt.TransactionDate
    FROM BankTransactions bt
    WHERE bt.UserId = @UserId
        AND bt.TransactionType = 'DEBIT'
        AND bt.TransactionDate >= @StartDate
        AND bt.TransactionDate <= @EndDate
        AND (bt.IsDeleted IS NULL OR bt.IsDeleted = 0)

    UNION ALL

    -- Operating: Bills paid (outflows)
    SELECT 
        'OPERATING' AS SectionType,
        'OUTFLOW' AS ActivityType,
        'Bill Payment' AS CategoryName,
        ISNULL(b.Provider, 'Bill') + ISNULL(' - ' + b.Notes, '') AS Description,
        p.Amount,
        p.TransactionDate
    FROM Payments p
    INNER JOIN Bills b ON p.BillId = b.Id
    WHERE p.UserId = @UserId
        AND p.TransactionType = 'DEBIT'
        AND p.TransactionDate >= @StartDate
        AND p.TransactionDate <= @EndDate
        AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
        AND p.BillId IS NOT NULL
        AND (b.IsDeleted IS NULL OR b.IsDeleted = 0)

    UNION ALL

    -- Investing: Savings deposits (outflow)
    SELECT 
        'INVESTING' AS SectionType,
        'OUTFLOW' AS ActivityType,
        'Savings Deposit' AS CategoryName,
        'Savings - ' + ISNULL(sa.AccountName, 'Deposit') AS Description,
        st.Amount,
        st.TransactionDate
    FROM SavingsTransactions st
    INNER JOIN SavingsAccounts sa ON st.SavingsAccountId = sa.Id
    WHERE sa.UserId = @UserId
        AND st.TransactionType = 'DEPOSIT'
        AND st.TransactionDate >= @StartDate
        AND st.TransactionDate <= @EndDate
        AND (st.IsDeleted IS NULL OR st.IsDeleted = 0)

    UNION ALL

    -- Investing: Savings withdrawals (inflow)
    SELECT 
        'INVESTING' AS SectionType,
        'INFLOW' AS ActivityType,
        'Savings Withdrawal' AS CategoryName,
        'Savings - ' + ISNULL(sa.AccountName, 'Withdrawal') AS Description,
        st.Amount,
        st.TransactionDate
    FROM SavingsTransactions st
    INNER JOIN SavingsAccounts sa ON st.SavingsAccountId = sa.Id
    WHERE sa.UserId = @UserId
        AND st.TransactionType = 'WITHDRAWAL'
        AND st.TransactionDate >= @StartDate
        AND st.TransactionDate <= @EndDate
        AND (st.IsDeleted IS NULL OR st.IsDeleted = 0)

    UNION ALL

    -- Financing: Loan disbursements (inflow)
    SELECT 
        'FINANCING' AS SectionType,
        'INFLOW' AS ActivityType,
        'Loan Disbursement' AS CategoryName,
        'Loan - ' + ISNULL(l.Purpose, 'Disbursement') AS Description,
        l.Principal AS Amount,
        ISNULL(l.DisbursedAt, l.AppliedAt) AS TransactionDate
    FROM Loans l
    WHERE l.UserId = @UserId
        AND l.DisbursedAt IS NOT NULL
        AND l.DisbursedAt >= @StartDate
        AND l.DisbursedAt <= @EndDate
        AND UPPER(ISNULL(l.Status, '')) IN ('APPROVED', 'ACTIVE')

    UNION ALL

    -- Financing: Loan payments (outflow)
    SELECT 
        'FINANCING' AS SectionType,
        'OUTFLOW' AS ActivityType,
        'Loan Payment' AS CategoryName,
        ISNULL(p.Description, 'Loan Payment') AS Description,
        p.Amount,
        p.TransactionDate
    FROM Payments p
    WHERE p.UserId = @UserId
        AND p.TransactionType = 'DEBIT'
        AND p.TransactionDate >= @StartDate
        AND p.TransactionDate <= @EndDate
        AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
        AND p.LoanId IS NOT NULL

    ORDER BY SectionType, ActivityType, TransactionDate;

    -- =============================================
    -- RESULT SET 2: SummaryData (one row)
    -- =============================================

    DECLARE @BeginningCash DECIMAL(18,2) = 0;
    DECLARE @EndingCash DECIMAL(18,2) = 0;
    DECLARE @OperatingNet DECIMAL(18,2) = 0;
    DECLARE @InvestingNet DECIMAL(18,2) = 0;
    DECLARE @FinancingNet DECIMAL(18,2) = 0;
    DECLARE @NetCashFlow DECIMAL(18,2) = 0;
    DECLARE @IsBalanced BIT = 0;

    -- Beginning cash: sum of last ClosingBalance per BankAccountId as of @StartDate
    SELECT @BeginningCash = ISNULL(SUM(ClosingBalance), 0)
    FROM (
        SELECT ClosingBalance,
               ROW_NUMBER() OVER (PARTITION BY BankAccountId ORDER BY StatementEndDate DESC, CreatedAt DESC) AS rn
        FROM BankStatements
        WHERE UserId = @UserId AND StatementEndDate <= @StartDate
    ) t
    WHERE rn = 1;

    -- If no statements, use 0 (or could use GetTotalBankAccountNetAmount - omitted for simplicity)
    IF @BeginningCash = 0
        SET @BeginningCash = 0;

    -- Ending cash: sum of last ClosingBalance per BankAccountId as of @EndDate
    SELECT @EndingCash = ISNULL(SUM(ClosingBalance), 0)
    FROM (
        SELECT ClosingBalance,
               ROW_NUMBER() OVER (PARTITION BY BankAccountId ORDER BY StatementEndDate DESC, CreatedAt DESC) AS rn
        FROM BankStatements
        WHERE UserId = @UserId AND StatementEndDate <= @EndDate
    ) t
    WHERE rn = 1;

    -- Operating net: inflows - outflows
    SELECT @OperatingNet = ISNULL(SUM(CASE WHEN ActivityType = 'INFLOW' THEN Amount ELSE -Amount END), 0)
    FROM (
        SELECT 'INFLOW' AS ActivityType, p.Amount
        FROM Payments p
        WHERE p.UserId = @UserId AND p.TransactionType = 'CREDIT'
            AND p.TransactionDate >= @StartDate AND p.TransactionDate <= @EndDate
            AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
            AND p.LoanId IS NULL AND p.BillId IS NULL AND p.SavingsAccountId IS NULL
        UNION ALL
        SELECT 'INFLOW', bt.Amount
        FROM BankTransactions bt
        WHERE bt.UserId = @UserId AND bt.TransactionType = 'CREDIT'
            AND bt.TransactionDate >= @StartDate AND bt.TransactionDate <= @EndDate
            AND (bt.IsDeleted IS NULL OR bt.IsDeleted = 0)
        UNION ALL
        SELECT 'OUTFLOW', p.Amount
        FROM Payments p
        WHERE p.UserId = @UserId AND p.TransactionType = 'DEBIT'
            AND p.TransactionDate >= @StartDate AND p.TransactionDate <= @EndDate
            AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
            AND p.BillId IS NULL AND p.LoanId IS NULL AND p.SavingsAccountId IS NULL
        UNION ALL
        SELECT 'OUTFLOW', bt.Amount
        FROM BankTransactions bt
        WHERE bt.UserId = @UserId AND bt.TransactionType = 'DEBIT'
            AND bt.TransactionDate >= @StartDate AND bt.TransactionDate <= @EndDate
            AND (bt.IsDeleted IS NULL OR bt.IsDeleted = 0)
        UNION ALL
        SELECT 'OUTFLOW', p.Amount
        FROM Payments p
        WHERE p.UserId = @UserId AND p.TransactionType = 'DEBIT' AND p.BillId IS NOT NULL
            AND p.TransactionDate >= @StartDate AND p.TransactionDate <= @EndDate
            AND (p.IsDeleted IS NULL OR p.IsDeleted = 0)
    ) op;

    -- Investing net
    SELECT @InvestingNet = ISNULL(SUM(CASE WHEN st.TransactionType = 'WITHDRAWAL' THEN st.Amount ELSE -st.Amount END), 0)
    FROM SavingsTransactions st
    INNER JOIN SavingsAccounts sa ON st.SavingsAccountId = sa.Id
    WHERE sa.UserId = @UserId
        AND st.TransactionDate >= @StartDate AND st.TransactionDate <= @EndDate
        AND (st.IsDeleted IS NULL OR st.IsDeleted = 0);

    -- Financing net: disbursements - payments
    SELECT @FinancingNet = ISNULL(SUM(Principal), 0)
    FROM Loans l
    WHERE l.UserId = @UserId AND l.DisbursedAt IS NOT NULL
        AND l.DisbursedAt >= @StartDate AND l.DisbursedAt <= @EndDate
        AND UPPER(ISNULL(l.Status, '')) IN ('APPROVED', 'ACTIVE');

    SELECT @FinancingNet = @FinancingNet - ISNULL(SUM(p.Amount), 0)
    FROM Payments p
    WHERE p.UserId = @UserId AND p.TransactionType = 'DEBIT' AND p.LoanId IS NOT NULL
        AND p.TransactionDate >= @StartDate AND p.TransactionDate <= @EndDate
        AND (p.IsDeleted IS NULL OR p.IsDeleted = 0);

    SET @NetCashFlow = @OperatingNet + @InvestingNet + @FinancingNet;
    SET @IsBalanced = CASE WHEN ABS(@EndingCash - (@BeginningCash + @NetCashFlow)) < 0.01 THEN 1 ELSE 0 END;

    SELECT 
        @StartDate AS PeriodStart,
        @EndDate AS PeriodEnd,
        @BeginningCash AS BeginningCash,
        @EndingCash AS EndingCash,
        @NetCashFlow AS NetCashFlow,
        @OperatingNet AS OperatingNet,
        @InvestingNet AS InvestingNet,
        @FinancingNet AS FinancingNet,
        @IsBalanced AS IsBalanced;
END;
GO
