-- ==========================================
-- CREATE OR UPDATE NOTIFICATIONS TABLE
-- ==========================================
-- This script creates the Notifications table if it doesn't exist,
-- or adds missing enhanced columns if the table already exists
-- Run this script to fix the "Invalid column name" error

USE [DBUTILS]
GO

-- Step 1: Create Notifications table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
    PRINT 'Creating Notifications table...';
    
    CREATE TABLE [dbo].[Notifications] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [Type] NVARCHAR(20) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Message] NVARCHAR(1000) NOT NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL,
        [ReadAt] DATETIME2 NULL,
        -- Enhanced columns (included from the start)
        [Channel] NVARCHAR(50) NULL,
        [Priority] NVARCHAR(50) NULL DEFAULT 'NORMAL',
        [ScheduledFor] DATETIME2 NULL,
        [Status] NVARCHAR(50) NULL DEFAULT 'PENDING',
        [TemplateId] NVARCHAR(450) NULL,
        [TemplateVariables] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'Notifications table created successfully with all columns';
    
    -- Create foreign key to Users table if it exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Users_UserId')
        BEGIN
            ALTER TABLE [dbo].[Notifications]
            ADD CONSTRAINT [FK_Notifications_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
            ON DELETE CASCADE;
            PRINT 'Foreign key FK_Notifications_Users_UserId created';
        END
    END
    
    -- Create index on UserId for better query performance
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId' AND object_id = OBJECT_ID(N'[dbo].[Notifications]'))
    BEGIN
        CREATE INDEX [IX_Notifications_UserId] 
        ON [dbo].[Notifications] ([UserId]);
        PRINT 'Index IX_Notifications_UserId created';
    END
    
    PRINT '==========================================';
    PRINT 'Notifications table created successfully';
    PRINT '==========================================';
END
ELSE
BEGIN
    PRINT 'Notifications table already exists. Checking for missing columns...';
    
    -- Step 2: Add missing enhanced columns if table exists but columns are missing
    -- Add Channel column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'Channel')
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD [Channel] NVARCHAR(50) NULL;
        PRINT 'Added Channel column to Notifications table';
    END
    ELSE
    BEGIN
        PRINT 'Channel column already exists in Notifications table';
    END

    -- Add Priority column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'Priority')
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD [Priority] NVARCHAR(50) NULL DEFAULT 'NORMAL';
        PRINT 'Added Priority column to Notifications table';
    END
    ELSE
    BEGIN
        PRINT 'Priority column already exists in Notifications table';
    END

    -- Add ScheduledFor column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ScheduledFor')
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD [ScheduledFor] DATETIME2 NULL;
        PRINT 'Added ScheduledFor column to Notifications table';
    END
    ELSE
    BEGIN
        PRINT 'ScheduledFor column already exists in Notifications table';
    END

    -- Add Status column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'Status')
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD [Status] NVARCHAR(50) NULL DEFAULT 'PENDING';
        PRINT 'Added Status column to Notifications table';
    END
    ELSE
    BEGIN
        PRINT 'Status column already exists in Notifications table';
    END

    -- Add TemplateId column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'TemplateId')
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD [TemplateId] NVARCHAR(450) NULL;
        PRINT 'Added TemplateId column to Notifications table';
    END
    ELSE
    BEGIN
        PRINT 'TemplateId column already exists in Notifications table';
    END

    -- Add TemplateVariables column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'TemplateVariables')
    BEGIN
        ALTER TABLE [dbo].[Notifications]
        ADD [TemplateVariables] NVARCHAR(MAX) NULL;
        PRINT 'Added TemplateVariables column to Notifications table';
    END
    ELSE
    BEGIN
        PRINT 'TemplateVariables column already exists in Notifications table';
    END

    -- Ensure foreign key exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Users_UserId')
        BEGIN
            ALTER TABLE [dbo].[Notifications]
            ADD CONSTRAINT [FK_Notifications_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
            ON DELETE CASCADE;
            PRINT 'Foreign key FK_Notifications_Users_UserId created';
        END
    END
    
    -- Ensure index exists
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId' AND object_id = OBJECT_ID(N'[dbo].[Notifications]'))
    BEGIN
        CREATE INDEX [IX_Notifications_UserId] 
        ON [dbo].[Notifications] ([UserId]);
        PRINT 'Index IX_Notifications_UserId created';
    END

    PRINT '==========================================';
    PRINT 'All missing columns have been added successfully';
    PRINT '==========================================';
END
GO

