-- Simple script to add columns - tries multiple table name variations
-- Run this script and it will find the correct table name automatically

-- Try BankTransactions first (most common)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BankTransactions')
BEGIN
    PRINT 'Using table: BankTransactions';
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BankTransactions') AND name = 'BillId')
        ALTER TABLE BankTransactions ADD BillId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BankTransactions') AND name = 'LoanId')
        ALTER TABLE BankTransactions ADD LoanId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BankTransactions') AND name = 'SavingsAccountId')
        ALTER TABLE BankTransactions ADD SavingsAccountId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BankTransactions') AND name = 'TransactionPurpose')
        ALTER TABLE BankTransactions ADD TransactionPurpose NVARCHAR(50) NULL;
    
    PRINT 'Columns added successfully to BankTransactions';
END
-- Try Transactions (without Bank prefix)
ELSE IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    PRINT 'Using table: Transactions';
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'BillId')
        ALTER TABLE Transactions ADD BillId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'LoanId')
        ALTER TABLE Transactions ADD LoanId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'SavingsAccountId')
        ALTER TABLE Transactions ADD SavingsAccountId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'TransactionPurpose')
        ALTER TABLE Transactions ADD TransactionPurpose NVARCHAR(50) NULL;
    
    PRINT 'Columns added successfully to Transactions';
END
-- Try Payments (if transactions are stored there)
ELSE IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    PRINT 'Using table: Payments';
    PRINT 'NOTE: If transactions are stored in Payments table, you may need to check if IsBankTransaction flag is used.';
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'BillId')
        ALTER TABLE Payments ADD BillId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'LoanId')
        ALTER TABLE Payments ADD LoanId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'SavingsAccountId')
        ALTER TABLE Payments ADD SavingsAccountId NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'TransactionPurpose')
        ALTER TABLE Payments ADD TransactionPurpose NVARCHAR(50) NULL;
    
    PRINT 'Columns added successfully to Payments';
END
ELSE
BEGIN
    PRINT 'ERROR: Could not find BankTransactions, Transactions, or Payments table.';
    PRINT '';
    PRINT 'Please run this query to find your transaction table:';
    PRINT 'SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE ''%Transaction%'' OR TABLE_NAME LIKE ''%Payment%''';
END

