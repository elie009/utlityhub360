-- =============================================
-- COMPLETE LOAN ACCOUNTING MIGRATION SCRIPT
-- Description: Adds all loan accounting fields and creates journal entry tables
-- Date: December 2024
-- =============================================

PRINT '========================================';
PRINT 'Starting Loan Accounting Migration...';
PRINT '========================================';
GO

-- =============================================
-- STEP 1: Add Loan Accounting Columns
-- =============================================
PRINT '';
PRINT 'STEP 1: Adding Loan Accounting Columns...';
PRINT '';

-- Add InterestComputationMethod
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'InterestComputationMethod')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [InterestComputationMethod] [nvarchar](20) NOT NULL DEFAULT 'AMORTIZED';
    PRINT '✓ Added InterestComputationMethod column';
END
ELSE
BEGIN
    PRINT '⚠ InterestComputationMethod column already exists';
END
GO

-- Add TotalInterest
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'TotalInterest')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [TotalInterest] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT '✓ Added TotalInterest column';
END
ELSE
BEGIN
    PRINT '⚠ TotalInterest column already exists';
END
GO

-- Add DownPayment
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'DownPayment')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [DownPayment] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT '✓ Added DownPayment column';
END
ELSE
BEGIN
    PRINT '⚠ DownPayment column already exists';
END
GO

-- Add ProcessingFee
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'ProcessingFee')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [ProcessingFee] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT '✓ Added ProcessingFee column';
END
ELSE
BEGIN
    PRINT '⚠ ProcessingFee column already exists';
END
GO

-- Add ActualFinancedAmount
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'ActualFinancedAmount')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [ActualFinancedAmount] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT '✓ Added ActualFinancedAmount column';
END
ELSE
BEGIN
    PRINT '⚠ ActualFinancedAmount column already exists';
END
GO

-- Add PaymentFrequency
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'PaymentFrequency')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [PaymentFrequency] [nvarchar](20) NOT NULL DEFAULT 'MONTHLY';
    PRINT '✓ Added PaymentFrequency column';
END
ELSE
BEGIN
    PRINT '⚠ PaymentFrequency column already exists';
END
GO

-- Add StartDate
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'StartDate')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [StartDate] [datetime2](7) NULL;
    PRINT '✓ Added StartDate column';
END
ELSE
BEGIN
    PRINT '⚠ StartDate column already exists';
END
GO

-- Update existing loans: Set ActualFinancedAmount = Principal
UPDATE [dbo].[Loans]
SET [ActualFinancedAmount] = [Principal]
WHERE [ActualFinancedAmount] = 0 AND [Principal] > 0;
GO

PRINT '';
PRINT '✓ Updated existing loans with ActualFinancedAmount';
PRINT '';

-- =============================================
-- STEP 2: Create Journal Entries Table
-- =============================================
PRINT '';
PRINT 'STEP 2: Creating Journal Entries Table...';
PRINT '';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JournalEntries] (
        [Id] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [LoanId] [nvarchar](450) NULL,
        [EntryType] [nvarchar](50) NOT NULL,
        [EntryDate] [datetime2](7) NOT NULL,
        [Description] [nvarchar](500) NOT NULL,
        [Reference] [nvarchar](100) NULL,
        [TotalDebit] [decimal](18,2) NOT NULL,
        [TotalCredit] [decimal](18,2) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_JournalEntries_LoanId] ON [dbo].[JournalEntries] ([LoanId]);
    CREATE INDEX [IX_JournalEntries_UserId] ON [dbo].[JournalEntries] ([UserId]);
    CREATE INDEX [IX_JournalEntries_EntryType] ON [dbo].[JournalEntries] ([EntryType]);
    CREATE INDEX [IX_JournalEntries_EntryDate] ON [dbo].[JournalEntries] ([EntryDate]);
    
    ALTER TABLE [dbo].[JournalEntries]
    ADD CONSTRAINT [FK_JournalEntries_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[JournalEntries]
    ADD CONSTRAINT [FK_JournalEntries_Loans_LoanId] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([Id]) ON DELETE CASCADE;
    
    PRINT '✓ Created JournalEntries table with indexes and foreign keys';
END
ELSE
BEGIN
    PRINT '⚠ JournalEntries table already exists';
END
GO

-- =============================================
-- STEP 3: Create Journal Entry Lines Table
-- =============================================
PRINT '';
PRINT 'STEP 3: Creating Journal Entry Lines Table...';
PRINT '';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntryLines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JournalEntryLines] (
        [Id] [nvarchar](450) NOT NULL,
        [JournalEntryId] [nvarchar](450) NOT NULL,
        [AccountName] [nvarchar](100) NOT NULL,
        [AccountType] [nvarchar](50) NOT NULL,
        [EntrySide] [nvarchar](10) NOT NULL,
        [Amount] [decimal](18,2) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_JournalEntryLines_JournalEntryId] ON [dbo].[JournalEntryLines] ([JournalEntryId]);
    CREATE INDEX [IX_JournalEntryLines_AccountName] ON [dbo].[JournalEntryLines] ([AccountName]);
    CREATE INDEX [IX_JournalEntryLines_AccountType] ON [dbo].[JournalEntryLines] ([AccountType]);
    
    ALTER TABLE [dbo].[JournalEntryLines]
    ADD CONSTRAINT [FK_JournalEntryLines_JournalEntries_JournalEntryId] 
    FOREIGN KEY ([JournalEntryId]) REFERENCES [dbo].[JournalEntries] ([Id]) ON DELETE CASCADE;
    
    PRINT '✓ Created JournalEntryLines table with indexes and foreign keys';
END
ELSE
BEGIN
    PRINT '⚠ JournalEntryLines table already exists';
END
GO

-- =============================================
-- STEP 4: Verification
-- =============================================
PRINT '';
PRINT 'STEP 4: Verifying Migration...';
PRINT '';

-- Check Loan columns
DECLARE @LoanColumnsCount INT;
SELECT @LoanColumnsCount = COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Loans'
AND COLUMN_NAME IN (
    'InterestComputationMethod',
    'TotalInterest',
    'DownPayment',
    'ProcessingFee',
    'ActualFinancedAmount',
    'PaymentFrequency',
    'StartDate'
);

IF @LoanColumnsCount = 7
BEGIN
    PRINT '✓ All 7 Loan accounting columns exist';
END
ELSE
BEGIN
    PRINT '⚠ Only ' + CAST(@LoanColumnsCount AS VARCHAR) + ' out of 7 Loan columns found';
END

-- Check Journal Entry tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND type in (N'U'))
    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntryLines]') AND type in (N'U'))
BEGIN
    PRINT '✓ Both Journal Entry tables exist';
END
ELSE
BEGIN
    PRINT '⚠ Journal Entry tables missing';
END

-- =============================================
-- COMPLETE
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Migration Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Restart your application';
PRINT '2. Test loan operations';
PRINT '3. Verify journal entries are created';
PRINT '';
GO


