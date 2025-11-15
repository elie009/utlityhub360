-- =============================================
-- QUICK FIX: Add Missing Loan Columns
-- Run this SQL script in your database to fix the error
-- =============================================

-- Add all missing columns to Loans table
ALTER TABLE [dbo].[Loans]
ADD [InterestComputationMethod] [nvarchar](20) NOT NULL DEFAULT 'AMORTIZED',
    [TotalInterest] [decimal](18,2) NOT NULL DEFAULT 0,
    [DownPayment] [decimal](18,2) NOT NULL DEFAULT 0,
    [ProcessingFee] [decimal](18,2) NOT NULL DEFAULT 0,
    [ActualFinancedAmount] [decimal](18,2) NOT NULL DEFAULT 0,
    [PaymentFrequency] [nvarchar](20) NOT NULL DEFAULT 'MONTHLY',
    [StartDate] [datetime2](7) NULL;

-- Update existing loans
UPDATE [dbo].[Loans]
SET [ActualFinancedAmount] = [Principal]
WHERE [ActualFinancedAmount] = 0 AND [Principal] > 0;

PRINT 'Loan columns added successfully!';




