-- ==========================================
-- NOTIFICATION ENHANCEMENTS TABLE CREATION
-- ==========================================
-- This script creates tables for notification preferences, templates, and history
--
-- IMPORTANT: This script uses DBUTILS as the database name
-- If your database has a different name, modify the USE statement below

USE [DBUTILS]
GO

-- Add new columns to Notifications table (only if table exists)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
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
END
ELSE
BEGIN
    PRINT 'WARNING: Notifications table does not exist. Cannot add new columns.';
    PRINT 'Please ensure the Notifications table exists before running this script.';
END
GO

-- Create NotificationPreferences table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NotificationPreferences] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [NotificationType] NVARCHAR(50) NOT NULL,
        [InAppEnabled] BIT NOT NULL DEFAULT 1,
        [EmailEnabled] BIT NOT NULL DEFAULT 0,
        [SmsEnabled] BIT NOT NULL DEFAULT 0,
        [PushEnabled] BIT NOT NULL DEFAULT 0,
        [ScheduledEnabled] BIT NOT NULL DEFAULT 0,
        [ScheduleTime] NVARCHAR(10) NULL,
        [ScheduleDays] NVARCHAR(MAX) NULL,
        [QuietHoursEnabled] BIT NOT NULL DEFAULT 0,
        [QuietHoursStart] NVARCHAR(10) NULL,
        [QuietHoursEnd] NVARCHAR(10) NULL,
        [MaxNotificationsPerDay] INT NULL,
        [MinMinutesBetweenNotifications] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'NotificationPreferences table created successfully';
END
ELSE
BEGIN
    PRINT 'NotificationPreferences table already exists';
END
GO

-- Create NotificationTemplates table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationTemplates]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NotificationTemplates] (
        [Id] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [NotificationType] NVARCHAR(50) NOT NULL,
        [Channel] NVARCHAR(50) NOT NULL,
        [Subject] NVARCHAR(200) NOT NULL,
        [Body] NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsSystemTemplate] BIT NOT NULL DEFAULT 0,
        [CreatedBy] NVARCHAR(450) NULL,
        [Variables] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_NotificationTemplates] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'NotificationTemplates table created successfully';
END
ELSE
BEGIN
    PRINT 'NotificationTemplates table already exists';
END
GO

-- Create NotificationHistories table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationHistories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NotificationHistories] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [NotificationId] NVARCHAR(450) NULL,
        [NotificationType] NVARCHAR(50) NOT NULL,
        [Channel] NVARCHAR(50) NOT NULL,
        [Subject] NVARCHAR(200) NULL,
        [Message] NVARCHAR(MAX) NULL,
        [Recipient] NVARCHAR(255) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
        [ErrorMessage] NVARCHAR(500) NULL,
        [Provider] NVARCHAR(100) NULL,
        [ExternalId] NVARCHAR(450) NULL,
        [SentAt] DATETIME2 NULL,
        [DeliveredAt] DATETIME2 NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_NotificationHistories] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'NotificationHistories table created successfully';
END
ELSE
BEGIN
    PRINT 'NotificationHistories table already exists';
END
GO

-- Create Foreign Keys (with proper existence checks)
-- FK_NotificationPreferences_Users
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]') AND type in (N'U'))
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_NotificationPreferences_Users')
        BEGIN
            ALTER TABLE [dbo].[NotificationPreferences]
            ADD CONSTRAINT [FK_NotificationPreferences_Users] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
            ON DELETE CASCADE;
            PRINT 'Foreign key FK_NotificationPreferences_Users created';
        END
        ELSE
        BEGIN
            PRINT 'Foreign key FK_NotificationPreferences_Users already exists';
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Users table does not exist. Cannot create FK_NotificationPreferences_Users.';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: NotificationPreferences table does not exist. Cannot create foreign key.';
END
GO

-- FK_Notifications_Templates
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationTemplates]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Templates')
        BEGIN
            ALTER TABLE [dbo].[Notifications]
            ADD CONSTRAINT [FK_Notifications_Templates] 
            FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[NotificationTemplates] ([Id]) 
            ON DELETE SET NULL;
            PRINT 'Foreign key FK_Notifications_Templates created';
        END
        ELSE
        BEGIN
            PRINT 'Foreign key FK_Notifications_Templates already exists';
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: NotificationTemplates table does not exist. Cannot create FK_Notifications_Templates.';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: Notifications table does not exist. Cannot create foreign key.';
END
GO

-- FK_NotificationHistories_Users
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationHistories]') AND type in (N'U'))
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_NotificationHistories_Users')
        BEGIN
            ALTER TABLE [dbo].[NotificationHistories]
            ADD CONSTRAINT [FK_NotificationHistories_Users] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
            ON DELETE CASCADE;
            PRINT 'Foreign key FK_NotificationHistories_Users created';
        END
        ELSE
        BEGIN
            PRINT 'Foreign key FK_NotificationHistories_Users already exists';
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Users table does not exist. Cannot create FK_NotificationHistories_Users.';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: NotificationHistories table does not exist. Cannot create foreign key.';
END
GO

-- FK_NotificationHistories_Notifications
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationHistories]') AND type in (N'U'))
BEGIN
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_NotificationHistories_Notifications')
        BEGIN
            ALTER TABLE [dbo].[NotificationHistories]
            ADD CONSTRAINT [FK_NotificationHistories_Notifications] 
            FOREIGN KEY ([NotificationId]) REFERENCES [dbo].[Notifications] ([Id]) 
            ON DELETE SET NULL;
            PRINT 'Foreign key FK_NotificationHistories_Notifications created';
        END
        ELSE
        BEGIN
            PRINT 'Foreign key FK_NotificationHistories_Notifications already exists';
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Notifications table does not exist. Cannot create FK_NotificationHistories_Notifications.';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: NotificationHistories table does not exist. Cannot create foreign key.';
END
GO

-- Create Indexes (with proper existence checks)
-- IX_NotificationPreferences_UserId_NotificationType
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotificationPreferences_UserId_NotificationType' AND object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]'))
    BEGIN
        CREATE UNIQUE INDEX [IX_NotificationPreferences_UserId_NotificationType] 
        ON [dbo].[NotificationPreferences] ([UserId], [NotificationType]);
        PRINT 'Index IX_NotificationPreferences_UserId_NotificationType created';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_NotificationPreferences_UserId_NotificationType already exists';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: NotificationPreferences table does not exist. Cannot create index.';
END
GO

-- IX_NotificationTemplates_NotificationType
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationTemplates]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotificationTemplates_NotificationType' AND object_id = OBJECT_ID(N'[dbo].[NotificationTemplates]'))
    BEGIN
        CREATE INDEX [IX_NotificationTemplates_NotificationType] 
        ON [dbo].[NotificationTemplates] ([NotificationType]);
        PRINT 'Index IX_NotificationTemplates_NotificationType created';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_NotificationTemplates_NotificationType already exists';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: NotificationTemplates table does not exist. Cannot create index.';
END
GO

-- IX_NotificationHistories_UserId_CreatedAt
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationHistories]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotificationHistories_UserId_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[NotificationHistories]'))
    BEGIN
        CREATE INDEX [IX_NotificationHistories_UserId_CreatedAt] 
        ON [dbo].[NotificationHistories] ([UserId], [CreatedAt]);
        PRINT 'Index IX_NotificationHistories_UserId_CreatedAt created';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_NotificationHistories_UserId_CreatedAt already exists';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: NotificationHistories table does not exist. Cannot create index.';
END
GO

PRINT '==========================================';
PRINT 'Notification enhancements completed successfully';
PRINT '==========================================';
GO

