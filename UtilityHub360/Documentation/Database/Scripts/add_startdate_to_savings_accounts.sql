-- Add StartDate column to SavingsAccounts table
-- Run this script on your database to add the StartDate column
-- This column tracks when the savings goal actually started

-- Step 1: Check if column already exists (without schema prefix)
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SavingsAccounts' 
    AND COLUMN_NAME = 'StartDate'
)
BEGIN
    PRINT 'Adding StartDate column to SavingsAccounts table...';
    
    -- Step 2: Add the column as nullable DateTime (try without schema prefix first)
    BEGIN TRY
        ALTER TABLE [SavingsAccounts]
        ADD [StartDate] DATETIME2 NULL;
        
        PRINT 'StartDate column added successfully.';
    END TRY
    BEGIN CATCH
        PRINT 'Error: Could not add StartDate column.';
        PRINT 'Error Message: ' + ERROR_MESSAGE();
        
        -- Try to find the actual table name
        PRINT 'Checking available tables...';
        SELECT TABLE_SCHEMA, TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_NAME LIKE '%Savings%'
        ORDER BY TABLE_NAME;
    END CATCH
END
ELSE
BEGIN
    PRINT 'StartDate column already exists in SavingsAccounts table.';
END
GO

-- Step 3: Update existing records to use CreatedAt as StartDate (backward compatibility)
-- This runs in a separate batch after the column is confirmed to exist
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SavingsAccounts' 
    AND COLUMN_NAME = 'StartDate'
)
BEGIN
    PRINT 'Updating existing records...';
    
    BEGIN TRY
        UPDATE [SavingsAccounts]
        SET [StartDate] = [CreatedAt]
        WHERE [StartDate] IS NULL;
        
        PRINT 'Existing records updated to use CreatedAt as StartDate.';
    END TRY
    BEGIN CATCH
        PRINT 'Error updating records: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'StartDate column not found. Cannot update records.';
END
GO

