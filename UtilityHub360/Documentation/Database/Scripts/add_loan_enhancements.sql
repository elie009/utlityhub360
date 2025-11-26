-- Migration: Add Loan Type, Refinancing Tracking, and Effective Interest Rate
-- Date: 2024
-- Description: Adds loan type, refinancing tracking fields, and effective interest rate to Loans table

-- Add LoanType column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'LoanType')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [LoanType] NVARCHAR(50) NOT NULL DEFAULT 'PERSONAL';
    PRINT 'Added LoanType column';
END
GO

-- Add RefinancedFromLoanId column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'RefinancedFromLoanId')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [RefinancedFromLoanId] NVARCHAR(450) NULL;
    PRINT 'Added RefinancedFromLoanId column';
END
GO

-- Add RefinancedToLoanId column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'RefinancedToLoanId')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [RefinancedToLoanId] NVARCHAR(450) NULL;
    PRINT 'Added RefinancedToLoanId column';
END
GO

-- Add RefinancingDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'RefinancingDate')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [RefinancingDate] DATETIME2 NULL;
    PRINT 'Added RefinancingDate column';
END
GO

-- Add EffectiveInterestRate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'EffectiveInterestRate')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [EffectiveInterestRate] DECIMAL(5,2) NULL;
    PRINT 'Added EffectiveInterestRate column';
END
GO

-- Add foreign key constraint for RefinancedFromLoanId (self-referencing)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Loans_RefinancedFromLoanId')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD CONSTRAINT [FK_Loans_Loans_RefinancedFromLoanId]
    FOREIGN KEY ([RefinancedFromLoanId]) REFERENCES [dbo].[Loans] ([Id]);
    PRINT 'Added foreign key constraint for RefinancedFromLoanId';
END
GO

-- Add foreign key constraint for RefinancedToLoanId (self-referencing)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Loans_RefinancedToLoanId')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD CONSTRAINT [FK_Loans_Loans_RefinancedToLoanId]
    FOREIGN KEY ([RefinancedToLoanId]) REFERENCES [dbo].[Loans] ([Id]);
    PRINT 'Added foreign key constraint for RefinancedToLoanId';
END
GO

-- Create index on LoanType for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Loans_LoanType')
BEGIN
    CREATE INDEX [IX_Loans_LoanType] ON [dbo].[Loans] ([LoanType]);
    PRINT 'Created index on LoanType';
END
GO

-- Create index on RefinancedFromLoanId for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Loans_RefinancedFromLoanId')
BEGIN
    CREATE INDEX [IX_Loans_RefinancedFromLoanId] ON [dbo].[Loans] ([RefinancedFromLoanId]);
    PRINT 'Created index on RefinancedFromLoanId';
END
GO

-- Create index on RefinancedToLoanId for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Loans_RefinancedToLoanId')
BEGIN
    CREATE INDEX [IX_Loans_RefinancedToLoanId] ON [dbo].[Loans] ([RefinancedToLoanId]);
    PRINT 'Created index on RefinancedToLoanId';
END
GO

PRINT 'Migration completed successfully!';

