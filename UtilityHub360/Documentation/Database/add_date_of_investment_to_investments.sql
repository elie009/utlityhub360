-- Add DateOfInvestment column to Investments table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Investments' 
    AND COLUMN_NAME = 'DateOfInvestment'
)
BEGIN
    -- Add the column as nullable DateTime
    ALTER TABLE [Investments]
    ADD [DateOfInvestment] DATETIME2 NULL;
    
    PRINT 'DateOfInvestment column added successfully to Investments table.';
END
ELSE
BEGIN
    PRINT 'DateOfInvestment column already exists in Investments table.';
END





