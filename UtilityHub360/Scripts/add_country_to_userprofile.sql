-- Add Country column to UserProfiles table
-- This script adds the Country column that was added to the UserProfile entity

USE DBUTILS;
GO

-- Check if Country column already exists
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserProfiles' 
    AND COLUMN_NAME = 'Country'
)
BEGIN
    -- Add Country column
    ALTER TABLE UserProfiles
    ADD Country NVARCHAR(100) NULL;
    
    PRINT 'Country column added successfully to UserProfiles table.';
END
ELSE
BEGIN
    PRINT 'Country column already exists in UserProfiles table.';
END
GO

