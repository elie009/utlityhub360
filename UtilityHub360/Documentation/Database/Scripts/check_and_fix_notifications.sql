-- ==========================================
-- COMPREHENSIVE CHECK AND FIX FOR NOTIFICATIONS
-- ==========================================
-- This script verifies the table structure and fixes any issues

USE [DBUTILS]
GO

PRINT '==========================================';
PRINT 'STEP 1: Checking if Notifications table exists';
PRINT '==========================================';
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
    PRINT '✓ Notifications table EXISTS';
    PRINT '';
    
    PRINT 'Current columns:';
    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Notifications'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
    
    -- Check each required column
    DECLARE @missingColumns NVARCHAR(MAX) = '';
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Channel')
        SET @missingColumns = @missingColumns + 'Channel, ';
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Priority')
        SET @missingColumns = @missingColumns + 'Priority, ';
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'ScheduledFor')
        SET @missingColumns = @missingColumns + 'ScheduledFor, ';
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Status')
        SET @missingColumns = @missingColumns + 'Status, ';
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'TemplateId')
        SET @missingColumns = @missingColumns + 'TemplateId, ';
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'TemplateVariables')
        SET @missingColumns = @missingColumns + 'TemplateVariables, ';
    
    IF LEN(@missingColumns) > 0
    BEGIN
        PRINT 'Missing columns detected: ' + LEFT(@missingColumns, LEN(@missingColumns) - 1);
        PRINT 'Adding missing columns...';
        PRINT '';
        
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Channel')
        BEGIN
            ALTER TABLE [dbo].[Notifications] ADD [Channel] NVARCHAR(50) NULL;
            PRINT '✓ Added Channel';
        END
        
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Priority')
        BEGIN
            ALTER TABLE [dbo].[Notifications] ADD [Priority] NVARCHAR(50) NULL DEFAULT 'NORMAL';
            PRINT '✓ Added Priority';
        END
        
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'ScheduledFor')
        BEGIN
            ALTER TABLE [dbo].[Notifications] ADD [ScheduledFor] DATETIME2 NULL;
            PRINT '✓ Added ScheduledFor';
        END
        
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'Status')
        BEGIN
            ALTER TABLE [dbo].[Notifications] ADD [Status] NVARCHAR(50) NULL DEFAULT 'PENDING';
            PRINT '✓ Added Status';
        END
        
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'TemplateId')
        BEGIN
            ALTER TABLE [dbo].[Notifications] ADD [TemplateId] NVARCHAR(450) NULL;
            PRINT '✓ Added TemplateId';
        END
        
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'TemplateVariables')
        BEGIN
            ALTER TABLE [dbo].[Notifications] ADD [TemplateVariables] NVARCHAR(MAX) NULL;
            PRINT '✓ Added TemplateVariables';
        END
        
        PRINT '';
        PRINT 'All columns added successfully!';
    END
    ELSE
    BEGIN
        PRINT '✓ All required columns exist';
    END
    
    PRINT '';
    PRINT '==========================================';
    PRINT 'STEP 2: Verifying final structure';
    PRINT '==========================================';
    
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        CHARACTER_MAXIMUM_LENGTH as MaxLength,
        IS_NULLABLE as IsNullable
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Notifications'
    ORDER BY ORDINAL_POSITION;
    
    PRINT '';
    PRINT '==========================================';
    PRINT '✓ Verification complete!';
    PRINT 'Please RESTART your API now.';
    PRINT '==========================================';
END
ELSE
BEGIN
    PRINT '✗ Notifications table DOES NOT EXIST';
    PRINT 'Creating table with all columns...';
    
    CREATE TABLE [dbo].[Notifications] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Type] NVARCHAR(20) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Message] NVARCHAR(1000) NOT NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL,
        [ReadAt] DATETIME2 NULL,
        [Channel] NVARCHAR(50) NULL,
        [Priority] NVARCHAR(50) NULL DEFAULT 'NORMAL',
        [ScheduledFor] DATETIME2 NULL,
        [Status] NVARCHAR(50) NULL DEFAULT 'PENDING',
        [TemplateId] NVARCHAR(450) NULL,
        [TemplateVariables] NVARCHAR(MAX) NULL
    );
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD CONSTRAINT [FK_Notifications_Users_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
        ON DELETE CASCADE;
    END
    
    CREATE INDEX [IX_Notifications_UserId] ON [dbo].[Notifications] ([UserId]);
    
    PRINT '✓ Table created successfully';
END
GO

