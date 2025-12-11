-- Migration script to add Country column to Users table
-- Run this script on your database to fix the "Invalid column name 'Country'" error

-- Check if column exists before adding it
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'Country'
)
BEGIN
    ALTER TABLE [Users]
    ADD [Country] NVARCHAR(100) NULL;
    
    PRINT 'Country column added successfully to Users table';
END
ELSE
BEGIN
    PRINT 'Country column already exists in Users table';
END
GO

