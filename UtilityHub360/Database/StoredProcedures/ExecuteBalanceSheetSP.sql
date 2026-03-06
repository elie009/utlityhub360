-- Execute this script to create/update the Balance Sheet stored procedure in your database
-- Run this in SQL Server Management Studio or Azure Data Studio

USE [YourDatabaseName];  -- Replace with your actual database name
GO

-- First, check if the procedure exists and drop it
IF OBJECT_ID('SP_GetBalanceSheetReport', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE SP_GetBalanceSheetReport;
    PRINT 'Existing SP_GetBalanceSheetReport dropped.';
END
GO

-- Now execute the SP_GetBalanceSheetReport.sql file content
-- Copy and paste the entire content of SP_GetBalanceSheetReport.sql here
-- OR execute it directly from the file in your SQL tool

PRINT 'Please execute the SP_GetBalanceSheetReport.sql file now.';
PRINT 'After that, verify the procedure was created:';
GO

-- Verify the stored procedure was created
IF OBJECT_ID('SP_GetBalanceSheetReport', 'P') IS NOT NULL
BEGIN
    PRINT 'SUCCESS: SP_GetBalanceSheetReport has been created!';
    
    -- Show procedure details
    SELECT 
        OBJECT_NAME(object_id) AS ProcedureName,
        create_date AS CreatedDate,
        modify_date AS LastModifiedDate
    FROM sys.procedures
    WHERE name = 'SP_GetBalanceSheetReport';
END
ELSE
BEGIN
    PRINT 'ERROR: SP_GetBalanceSheetReport was not created. Please check for errors.';
END
GO
