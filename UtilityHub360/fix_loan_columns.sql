-- Add missing columns to Loans table
-- Database: DBUTILS

-- Add NextDueDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'NextDueDate')
BEGIN
    ALTER TABLE [dbo].[Loans] ADD [NextDueDate] datetime2 NULL;
    PRINT 'NextDueDate column added successfully';
END
ELSE
BEGIN
    PRINT 'NextDueDate column already exists';
END

-- Add FinalDueDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Loans]') AND name = 'FinalDueDate')
BEGIN
    ALTER TABLE [dbo].[Loans] ADD [FinalDueDate] datetime2 NULL;
    PRINT 'FinalDueDate column added successfully';
END
ELSE
BEGIN
    PRINT 'FinalDueDate column already exists';
END

-- Verify the columns were added
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Loans' 
    AND COLUMN_NAME IN ('NextDueDate', 'FinalDueDate')
ORDER BY COLUMN_NAME;

PRINT 'Migration complete!';


