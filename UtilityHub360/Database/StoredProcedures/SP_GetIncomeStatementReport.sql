-- =============================================
-- Stored Procedure: SP_GetIncomeStatementReport
-- Description: Generates Income Statement data for RDLC reports
-- Parameters: 
--   @UserId - User identifier
--   @StartDate - Report period start date
--   @EndDate - Report period end date
-- =============================================

CREATE OR ALTER PROCEDURE SP_GetIncomeStatementReport
    @UserId NVARCHAR(450),
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare variables for totals
    DECLARE @TotalRevenue DECIMAL(18,2) = 0;
    DECLARE @TotalExpenses DECIMAL(18,2) = 0;
    DECLARE @NetIncome DECIMAL(18,2) = 0;

    -- =============================================
    -- REVENUE SECTION
    -- =============================================
    
    -- Get all income/revenue transactions
    SELECT 
        'REVENUE' AS SectionType,
        CASE 
            WHEN p.Category = 'SALARY' THEN 'Salary Income'
            WHEN p.Category = 'BUSINESS' THEN 'Business Income'
            WHEN p.Category = 'FREELANCE' THEN 'Freelance Income'
            WHEN p.Category = 'INVESTMENT' THEN 'Investment Income'
            WHEN p.Category = 'INTEREST' THEN 'Interest Income'
            WHEN p.Category = 'RENTAL' THEN 'Rental Income'
            WHEN p.Category = 'DIVIDEND' THEN 'Dividend Income'
            WHEN p.Category = 'OTHER_INCOME' THEN 'Other Income'
            ELSE 'Other Revenue'
        END AS CategoryName,
        p.Category AS CategoryCode,
        p.Description,
        p.Amount,
        p.TransactionDate,
        p.Method AS PaymentMethod,
        ba.AccountName AS AccountName,
        ba.AccountType AS AccountType
    FROM Payments p
    LEFT JOIN BankAccounts ba ON p.BankAccountId = ba.Id
    WHERE p.UserId = @UserId
        AND p.TransactionDate >= @StartDate 
        AND p.TransactionDate <= @EndDate
        AND p.IsDeleted = 0
        AND p.TransactionType = 'CREDIT'
        AND p.Category IN ('SALARY', 'BUSINESS', 'FREELANCE', 'INVESTMENT', 
                          'INTEREST', 'RENTAL', 'DIVIDEND', 'OTHER_INCOME')
    
    UNION ALL
    
    -- Get income from bank transactions
    SELECT 
        'REVENUE' AS SectionType,
        CASE 
            WHEN bt.Category = 'SALARY' THEN 'Salary Income'
            WHEN bt.Category = 'BUSINESS' THEN 'Business Income'
            WHEN bt.Category = 'FREELANCE' THEN 'Freelance Income'
            WHEN bt.Category = 'INVESTMENT' THEN 'Investment Income'
            WHEN bt.Category = 'INTEREST' THEN 'Interest Income'
            WHEN bt.Category = 'RENTAL' THEN 'Rental Income'
            WHEN bt.Category = 'DIVIDEND' THEN 'Dividend Income'
            ELSE 'Other Income'
        END AS CategoryName,
        bt.Category AS CategoryCode,
        bt.Description,
        bt.Amount,
        bt.TransactionDate,
        'Bank Transaction' AS PaymentMethod,
        ba.AccountName AS AccountName,
        ba.AccountType AS AccountType
    FROM BankTransactions bt
    INNER JOIN BankAccounts ba ON bt.BankAccountId = ba.Id
    WHERE bt.UserId = @UserId
        AND bt.TransactionDate >= @StartDate 
        AND bt.TransactionDate <= @EndDate
        AND bt.IsDeleted = 0
        AND bt.TransactionType = 'CREDIT'
        AND bt.Category IN ('SALARY', 'BUSINESS', 'FREELANCE', 'INVESTMENT', 
                           'INTEREST', 'RENTAL', 'DIVIDEND')
    
    UNION ALL
    
    -- =============================================
    -- EXPENSES SECTION
    -- =============================================
    
    -- Get all expense transactions from Payments
    SELECT 
        'EXPENSE' AS SectionType,
        CASE 
            WHEN p.Category = 'UTILITIES' THEN 'Utilities'
            WHEN p.Category = 'RENT' THEN 'Rent'
            WHEN p.Category = 'INSURANCE' THEN 'Insurance'
            WHEN p.Category = 'SUBSCRIPTION' THEN 'Subscriptions'
            WHEN p.Category = 'FOOD' THEN 'Food & Dining'
            WHEN p.Category = 'TRANSPORTATION' THEN 'Transportation'
            WHEN p.Category = 'HEALTHCARE' THEN 'Healthcare'
            WHEN p.Category = 'EDUCATION' THEN 'Education'
            WHEN p.Category = 'ENTERTAINMENT' THEN 'Entertainment'
            WHEN p.Category = 'SHOPPING' THEN 'Shopping'
            WHEN p.Category = 'TRAVEL' THEN 'Travel'
            WHEN p.Category = 'LOAN_PAYMENT' THEN 'Loan Payment'
            WHEN p.Category = 'INTEREST_EXPENSE' THEN 'Interest Expense'
            ELSE 'Other Expenses'
        END AS CategoryName,
        p.Category AS CategoryCode,
        p.Description,
        p.Amount,
        p.TransactionDate,
        p.Method AS PaymentMethod,
        ba.AccountName AS AccountName,
        ba.AccountType AS AccountType
    FROM Payments p
    LEFT JOIN BankAccounts ba ON p.BankAccountId = ba.Id
    WHERE p.UserId = @UserId
        AND p.TransactionDate >= @StartDate 
        AND p.TransactionDate <= @EndDate
        AND p.IsDeleted = 0
        AND p.TransactionType = 'DEBIT'
    
    UNION ALL
    
    -- Get expense transactions from BankTransactions
    SELECT 
        'EXPENSE' AS SectionType,
        CASE 
            WHEN bt.Category = 'UTILITIES' THEN 'Utilities'
            WHEN bt.Category = 'RENT' THEN 'Rent'
            WHEN bt.Category = 'INSURANCE' THEN 'Insurance'
            WHEN bt.Category = 'SUBSCRIPTION' THEN 'Subscriptions'
            WHEN bt.Category = 'FOOD' THEN 'Food & Dining'
            WHEN bt.Category = 'TRANSPORTATION' THEN 'Transportation'
            WHEN bt.Category = 'HEALTHCARE' THEN 'Healthcare'
            WHEN bt.Category = 'EDUCATION' THEN 'Education'
            WHEN bt.Category = 'ENTERTAINMENT' THEN 'Entertainment'
            WHEN bt.Category = 'SHOPPING' THEN 'Shopping'
            WHEN bt.Category = 'TRAVEL' THEN 'Travel'
            ELSE 'Other Expenses'
        END AS CategoryName,
        bt.Category AS CategoryCode,
        bt.Description,
        bt.Amount,
        bt.TransactionDate,
        'Bank Transaction' AS PaymentMethod,
        ba.AccountName AS AccountName,
        ba.AccountType AS AccountType
    FROM BankTransactions bt
    INNER JOIN BankAccounts ba ON bt.BankAccountId = ba.Id
    WHERE bt.UserId = @UserId
        AND bt.TransactionDate >= @StartDate 
        AND bt.TransactionDate <= @EndDate
        AND bt.IsDeleted = 0
        AND bt.TransactionType = 'DEBIT'
    
    UNION ALL
    
    -- Get paid bills as expenses
    SELECT 
        'EXPENSE' AS SectionType,
        CASE 
            WHEN b.BillType = 'ELECTRICITY' THEN 'Utilities - Electricity'
            WHEN b.BillType = 'WATER' THEN 'Utilities - Water'
            WHEN b.BillType = 'GAS' THEN 'Utilities - Gas'
            WHEN b.BillType = 'INTERNET' THEN 'Utilities - Internet'
            WHEN b.BillType = 'PHONE' THEN 'Utilities - Phone'
            WHEN b.BillType = 'RENT' THEN 'Rent'
            WHEN b.BillType = 'INSURANCE' THEN 'Insurance'
            WHEN b.BillType = 'SUBSCRIPTION' THEN 'Subscriptions'
            ELSE 'Other Bills'
        END AS CategoryName,
        b.BillType AS CategoryCode,
        b.Provider + ' - ' + ISNULL(b.Notes, '') AS Description,
        b.Amount,
        ISNULL(b.PaidAt, b.DueDate) AS TransactionDate,
        'Bill Payment' AS PaymentMethod,
        '' AS AccountName,
        'Bill' AS AccountType
    FROM Bills b
    WHERE b.UserId = @UserId
        AND b.Status = 'PAID'
        AND ISNULL(b.PaidAt, b.DueDate) >= @StartDate 
        AND ISNULL(b.PaidAt, b.DueDate) <= @EndDate
        AND b.IsDeleted = 0
    
    ORDER BY SectionType, CategoryName, TransactionDate;

    -- =============================================
    -- SUMMARY SECTION
    -- =============================================
    
    -- Calculate totals
    SELECT 
        @TotalRevenue = ISNULL(SUM(Amount), 0)
    FROM (
        -- Revenue from Payments
        SELECT Amount
        FROM Payments
        WHERE UserId = @UserId
            AND TransactionDate >= @StartDate 
            AND TransactionDate <= @EndDate
            AND IsDeleted = 0
            AND TransactionType = 'CREDIT'
            AND Category IN ('SALARY', 'BUSINESS', 'FREELANCE', 'INVESTMENT', 
                            'INTEREST', 'RENTAL', 'DIVIDEND', 'OTHER_INCOME')
        
        UNION ALL
        
        -- Revenue from BankTransactions
        SELECT Amount
        FROM BankTransactions
        WHERE UserId = @UserId
            AND TransactionDate >= @StartDate 
            AND TransactionDate <= @EndDate
            AND IsDeleted = 0
            AND TransactionType = 'CREDIT'
            AND Category IN ('SALARY', 'BUSINESS', 'FREELANCE', 'INVESTMENT', 
                            'INTEREST', 'RENTAL', 'DIVIDEND')
    ) AS RevenueData;

    SELECT 
        @TotalExpenses = ISNULL(SUM(Amount), 0)
    FROM (
        -- Expenses from Payments
        SELECT Amount
        FROM Payments
        WHERE UserId = @UserId
            AND TransactionDate >= @StartDate 
            AND TransactionDate <= @EndDate
            AND IsDeleted = 0
            AND TransactionType = 'DEBIT'
        
        UNION ALL
        
        -- Expenses from BankTransactions
        SELECT Amount
        FROM BankTransactions
        WHERE UserId = @UserId
            AND TransactionDate >= @StartDate 
            AND TransactionDate <= @EndDate
            AND IsDeleted = 0
            AND TransactionType = 'DEBIT'
        
        UNION ALL
        
        -- Expenses from Bills
        SELECT Amount
        FROM Bills
        WHERE UserId = @UserId
            AND Status = 'PAID'
            AND ISNULL(PaidAt, DueDate) >= @StartDate 
            AND ISNULL(PaidAt, DueDate) <= @EndDate
            AND IsDeleted = 0
    ) AS ExpenseData;

    SET @NetIncome = @TotalRevenue - @TotalExpenses;

    -- Return summary
    SELECT 
        @StartDate AS PeriodStart,
        @EndDate AS PeriodEnd,
        @TotalRevenue AS TotalRevenue,
        @TotalExpenses AS TotalExpenses,
        @NetIncome AS NetIncome,
        CASE WHEN @NetIncome >= 0 THEN 'Profit' ELSE 'Loss' END AS Status;

END;
GO
