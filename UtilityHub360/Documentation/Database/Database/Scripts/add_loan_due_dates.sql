-- Add missing columns to Loans table
-- Run this script on your database to fix the "Invalid column name" error

USE [UtilityHub360_Dev]  -- Change this to your database name if different
GO

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'NextDueDate')
BEGIN
    ALTER TABLE [dbo].[Loans] ADD [NextDueDate] datetime2 NULL;
    PRINT 'NextDueDate column added successfully';
END
ELSE
BEGIN
    PRINT 'NextDueDate column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'FinalDueDate')
BEGIN
    ALTER TABLE [dbo].[Loans] ADD [FinalDueDate] datetime2 NULL;
    PRINT 'FinalDueDate column added successfully';
END
ELSE
BEGIN
    PRINT 'FinalDueDate column already exists';
END
GO

-- Verify the columns were added
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Loans' 
    AND COLUMN_NAME IN ('NextDueDate', 'FinalDueDate')
ORDER BY COLUMN_NAME;
GO

PRINT 'Migration complete! The Loans table now has NextDueDate and FinalDueDate columns.';
GO


