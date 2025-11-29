-- Migration: Add Loan Accounting Fields
-- Description: Adds accounting-related fields to the Loans table
-- Date: December 2024

-- Add InterestComputationMethod column (FLAT_RATE or AMORTIZED)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'InterestComputationMethod')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [InterestComputationMethod] [nvarchar](20) NOT NULL DEFAULT 'AMORTIZED';
    PRINT 'Added InterestComputationMethod column';
END
ELSE
BEGIN
    PRINT 'InterestComputationMethod column already exists';
END
GO

-- Add TotalInterest column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'TotalInterest')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [TotalInterest] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT 'Added TotalInterest column';
END
ELSE
BEGIN
    PRINT 'TotalInterest column already exists';
END
GO

-- Add DownPayment column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'DownPayment')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [DownPayment] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT 'Added DownPayment column';
END
ELSE
BEGIN
    PRINT 'DownPayment column already exists';
END
GO

-- Add ProcessingFee column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'ProcessingFee')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [ProcessingFee] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT 'Added ProcessingFee column';
END
ELSE
BEGIN
    PRINT 'ProcessingFee column already exists';
END
GO

-- Add ActualFinancedAmount column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'ActualFinancedAmount')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [ActualFinancedAmount] [decimal](18,2) NOT NULL DEFAULT 0;
    PRINT 'Added ActualFinancedAmount column';
END
ELSE
BEGIN
    PRINT 'ActualFinancedAmount column already exists';
END
GO

-- Add PaymentFrequency column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'PaymentFrequency')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [PaymentFrequency] [nvarchar](20) NOT NULL DEFAULT 'MONTHLY';
    PRINT 'Added PaymentFrequency column';
END
ELSE
BEGIN
    PRINT 'PaymentFrequency column already exists';
END
GO

-- Add StartDate column (nullable)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'StartDate')
BEGIN
    ALTER TABLE [dbo].[Loans]
    ADD [StartDate] [datetime2](7) NULL;
    PRINT 'Added StartDate column';
END
ELSE
BEGIN
    PRINT 'StartDate column already exists';
END
GO

-- Update existing loans: Set ActualFinancedAmount = Principal for existing loans
UPDATE [dbo].[Loans]
SET [ActualFinancedAmount] = [Principal]
WHERE [ActualFinancedAmount] = 0 AND [Principal] > 0;
GO

PRINT 'Migration completed successfully!';
GO







