-- Script to add linking fields to BankTransactions table
-- This adds: BillId, LoanId, SavingsAccountId, TransactionPurpose

-- Check if columns already exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'BillId')
BEGIN
    ALTER TABLE [dbo].[BankTransactions]
    ADD [BillId] NVARCHAR(450) NULL;
    PRINT 'Added BillId column';
END
ELSE
BEGIN
    PRINT 'BillId column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'LoanId')
BEGIN
    ALTER TABLE [dbo].[BankTransactions]
    ADD [LoanId] NVARCHAR(450) NULL;
    PRINT 'Added LoanId column';
END
ELSE
BEGIN
    PRINT 'LoanId column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'SavingsAccountId')
BEGIN
    ALTER TABLE [dbo].[BankTransactions]
    ADD [SavingsAccountId] NVARCHAR(450) NULL;
    PRINT 'Added SavingsAccountId column';
END
ELSE
BEGIN
    PRINT 'SavingsAccountId column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'TransactionPurpose')
BEGIN
    ALTER TABLE [dbo].[BankTransactions]
    ADD [TransactionPurpose] NVARCHAR(50) NULL;
    PRINT 'Added TransactionPurpose column';
END
ELSE
BEGIN
    PRINT 'TransactionPurpose column already exists';
END

-- Create indexes if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_BillId' AND object_id = OBJECT_ID(N'[dbo].[BankTransactions]'))
BEGIN
    CREATE INDEX [IX_BankTransactions_BillId] ON [dbo].[BankTransactions] ([BillId]);
    PRINT 'Created index IX_BankTransactions_BillId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_LoanId' AND object_id = OBJECT_ID(N'[dbo].[BankTransactions]'))
BEGIN
    CREATE INDEX [IX_BankTransactions_LoanId] ON [dbo].[BankTransactions] ([LoanId]);
    PRINT 'Created index IX_BankTransactions_LoanId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_SavingsAccountId' AND object_id = OBJECT_ID(N'[dbo].[BankTransactions]'))
BEGIN
    CREATE INDEX [IX_BankTransactions_SavingsAccountId] ON [dbo].[BankTransactions] ([SavingsAccountId]);
    PRINT 'Created index IX_BankTransactions_SavingsAccountId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BankTransactions_TransactionPurpose' AND object_id = OBJECT_ID(N'[dbo].[BankTransactions]'))
BEGIN
    CREATE INDEX [IX_BankTransactions_TransactionPurpose] ON [dbo].[BankTransactions] ([TransactionPurpose]);
    PRINT 'Created index IX_BankTransactions_TransactionPurpose';
END

-- Add foreign key constraints if tables exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BankTransactions_Bills_BillId')
    BEGIN
        ALTER TABLE [dbo].[BankTransactions]
        ADD CONSTRAINT [FK_BankTransactions_Bills_BillId] 
        FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills] ([Id]);
        PRINT 'Added foreign key FK_BankTransactions_Bills_BillId';
    END
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Loans')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BankTransactions_Loans_LoanId')
    BEGIN
        ALTER TABLE [dbo].[BankTransactions]
        ADD CONSTRAINT [FK_BankTransactions_Loans_LoanId] 
        FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([Id]);
        PRINT 'Added foreign key FK_BankTransactions_Loans_LoanId';
    END
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SavingsAccounts')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BankTransactions_SavingsAccounts_SavingsAccountId')
    BEGIN
        ALTER TABLE [dbo].[BankTransactions]
        ADD CONSTRAINT [FK_BankTransactions_SavingsAccounts_SavingsAccountId] 
        FOREIGN KEY ([SavingsAccountId]) REFERENCES [dbo].[SavingsAccounts] ([Id]);
        PRINT 'Added foreign key FK_BankTransactions_SavingsAccounts_SavingsAccountId';
    END
END

-- Record migration in EF migrations history (optional, but recommended)
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20250101000000_AddTransactionLinkingFields')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250101000000_AddTransactionLinkingFields', N'8.0.0');
    PRINT 'Recorded migration in __EFMigrationsHistory';
END

PRINT 'Migration completed successfully!';

