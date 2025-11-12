-- Add PreferredCurrency column to UserProfiles table
-- Run this script on your database to add the PreferredCurrency column

-- Check if column already exists before adding
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserProfiles' 
    AND COLUMN_NAME = 'PreferredCurrency'
)
BEGIN
    -- Add the column with a default value of 'USD'
    ALTER TABLE [UserProfiles]
    ADD [PreferredCurrency] NVARCHAR(10) NOT NULL DEFAULT 'USD';
    
    PRINT 'PreferredCurrency column added successfully to UserProfiles table.';
END
ELSE
BEGIN
    PRINT 'PreferredCurrency column already exists in UserProfiles table.';
END
GO


