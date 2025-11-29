-- ==========================================
-- VERIFY NOTIFICATIONS TABLE STRUCTURE
-- ==========================================
-- Run this to check if columns actually exist in the database

USE [DBUTILS]
GO

-- Check if table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
    PRINT '✓ Notifications table EXISTS';
    PRINT '';
    
    -- List all columns in the table
    PRINT 'Current columns in Notifications table:';
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        CHARACTER_MAXIMUM_LENGTH as MaxLength,
        IS_NULLABLE as IsNullable,
        COLUMN_DEFAULT as DefaultValue
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Notifications'
    ORDER BY ORDINAL_POSITION;
    
    PRINT '';
    PRINT 'Checking for required columns:';
    
    -- Check each required column
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Channel')
        PRINT '✓ Channel column EXISTS'
    ELSE
        PRINT '✗ Channel column MISSING'
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Priority')
        PRINT '✓ Priority column EXISTS'
    ELSE
        PRINT '✗ Priority column MISSING'
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'ScheduledFor')
        PRINT '✓ ScheduledFor column EXISTS'
    ELSE
        PRINT '✗ ScheduledFor column MISSING'
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Status')
        PRINT '✓ Status column EXISTS'
    ELSE
        PRINT '✗ Status column MISSING'
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'TemplateId')
        PRINT '✓ TemplateId column EXISTS'
    ELSE
        PRINT '✗ TemplateId column MISSING'
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'TemplateVariables')
        PRINT '✓ TemplateVariables column EXISTS'
    ELSE
        PRINT '✗ TemplateVariables column MISSING'
END
ELSE
BEGIN
    PRINT '✗ Notifications table DOES NOT EXIST';
    PRINT 'You need to create the table first.';
END
GO

