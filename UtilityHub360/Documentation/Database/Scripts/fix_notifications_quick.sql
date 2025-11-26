-- ==========================================
-- QUICK FIX: CREATE OR FIX NOTIFICATIONS TABLE
-- ==========================================
-- Copy and paste this entire script into SQL Server Management Studio
-- and run it on your DBUTILS database

USE [DBUTILS]
GO

-- Step 1: Create table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
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
    
    -- Create foreign key if Users table exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD CONSTRAINT [FK_Notifications_Users_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
        ON DELETE CASCADE;
    END
    
    -- Create index
    CREATE INDEX [IX_Notifications_UserId] ON [dbo].[Notifications] ([UserId]);
    
    PRINT 'Notifications table created successfully';
END
ELSE
BEGIN
    -- Step 2: Add missing columns if table exists
    PRINT 'Notifications table exists. Adding missing columns...';
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'Channel')
        ALTER TABLE [dbo].[Notifications] ADD [Channel] NVARCHAR(50) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'Priority')
        ALTER TABLE [dbo].[Notifications] ADD [Priority] NVARCHAR(50) NULL DEFAULT 'NORMAL';
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ScheduledFor')
        ALTER TABLE [dbo].[Notifications] ADD [ScheduledFor] DATETIME2 NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'Status')
        ALTER TABLE [dbo].[Notifications] ADD [Status] NVARCHAR(50) NULL DEFAULT 'PENDING';
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'TemplateId')
        ALTER TABLE [dbo].[Notifications] ADD [TemplateId] NVARCHAR(450) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'TemplateVariables')
        ALTER TABLE [dbo].[Notifications] ADD [TemplateVariables] NVARCHAR(MAX) NULL;
    
    PRINT 'All columns added successfully';
END
GO

PRINT '==========================================';
PRINT 'Fix completed! Please restart your API.';
PRINT '==========================================';
GO

