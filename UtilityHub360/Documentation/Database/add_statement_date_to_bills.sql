-- Migration: Add StatementDate column to Bills table
-- Date: 2025-01-15
-- Description: Adds StatementDate column to track when bills/invoices were issued

-- Check if column already exists before adding
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') 
    AND name = 'StatementDate'
)
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [StatementDate] datetime2 NULL;
    
    PRINT 'StatementDate column added successfully to Bills table';
END
ELSE
BEGIN
    PRINT 'StatementDate column already exists in Bills table';
END
GO

