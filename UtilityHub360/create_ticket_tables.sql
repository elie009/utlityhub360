-- =============================================
-- Ticket Management System - Database Tables
-- =============================================
-- This script creates all tables required for the ticket management system
-- Run this script on your SQL Server database
-- 
-- IMPORTANT: Make sure you're connected to the correct database before running this script
-- If your database name is different, uncomment and modify the USE statement below
-- =============================================

-- Uncomment and modify the database name if needed:
-- USE [YourDatabaseName]
-- GO

-- =============================================
-- 1. Tickets Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND type in (N'U'))
BEGIN
    -- Check if Users table exists
    DECLARE @UsersTableExists BIT = 0;
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        SET @UsersTableExists = 1;
    END

    -- Create table without foreign keys first
    CREATE TABLE [dbo].[Tickets] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'OPEN',
        [Priority] NVARCHAR(50) NOT NULL DEFAULT 'NORMAL',
        [Category] NVARCHAR(50) NOT NULL DEFAULT 'GENERAL',
        [AssignedTo] NVARCHAR(450) NULL,
        [ResolutionNotes] NVARCHAR(MAX) NULL,
        [ResolvedAt] DATETIME2 NULL,
        [ResolvedBy] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Create indexes
    CREATE INDEX [IX_Tickets_UserId] ON [dbo].[Tickets]([UserId]);
    CREATE INDEX [IX_Tickets_Status] ON [dbo].[Tickets]([Status]);
    CREATE INDEX [IX_Tickets_Priority] ON [dbo].[Tickets]([Priority]);
    CREATE INDEX [IX_Tickets_Category] ON [dbo].[Tickets]([Category]);
    CREATE INDEX [IX_Tickets_AssignedTo] ON [dbo].[Tickets]([AssignedTo]);
    CREATE INDEX [IX_Tickets_CreatedAt] ON [dbo].[Tickets]([CreatedAt]);
    
    -- Add foreign key constraints if Users table exists
    IF @UsersTableExists = 1
    BEGIN
        ALTER TABLE [dbo].[Tickets]
        ADD CONSTRAINT [FK_Tickets_Users] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
        
        ALTER TABLE [dbo].[Tickets]
        ADD CONSTRAINT [FK_Tickets_AssignedUser] 
        FOREIGN KEY ([AssignedTo]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL;
        
        ALTER TABLE [dbo].[Tickets]
        ADD CONSTRAINT [FK_Tickets_ResolvedByUser] 
        FOREIGN KEY ([ResolvedBy]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL;
        
        PRINT 'Tickets table created successfully with foreign key constraints';
    END
    ELSE
    BEGIN
        PRINT 'Tickets table created successfully (Users table not found - foreign keys skipped)';
    END
END
ELSE
BEGIN
    PRINT 'Tickets table already exists';
END
GO

-- =============================================
-- 2. TicketComments Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TicketComments]') AND type in (N'U'))
BEGIN
    -- Check if referenced tables exist
    DECLARE @TicketsTableExists BIT = 0;
    DECLARE @UsersTableExists2 BIT = 0;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND type in (N'U'))
    BEGIN
        SET @TicketsTableExists = 1;
    END
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        SET @UsersTableExists2 = 1;
    END

    -- Create table without foreign keys first
    CREATE TABLE [dbo].[TicketComments] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [TicketId] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [Comment] NVARCHAR(MAX) NOT NULL,
        [IsInternal] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
    
    -- Create indexes
    CREATE INDEX [IX_TicketComments_TicketId] ON [dbo].[TicketComments]([TicketId]);
    CREATE INDEX [IX_TicketComments_UserId] ON [dbo].[TicketComments]([UserId]);
    CREATE INDEX [IX_TicketComments_CreatedAt] ON [dbo].[TicketComments]([CreatedAt]);
    
    -- Add foreign key constraints if referenced tables exist
    IF @TicketsTableExists = 1
    BEGIN
        ALTER TABLE [dbo].[TicketComments]
        ADD CONSTRAINT [FK_TicketComments_Tickets] 
        FOREIGN KEY ([TicketId]) REFERENCES [dbo].[Tickets]([Id]) ON DELETE CASCADE;
    END
    
    IF @UsersTableExists2 = 1
    BEGIN
        ALTER TABLE [dbo].[TicketComments]
        ADD CONSTRAINT [FK_TicketComments_Users] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
    END
    
    PRINT 'TicketComments table created successfully';
END
ELSE
BEGIN
    PRINT 'TicketComments table already exists';
END
GO

-- =============================================
-- 3. TicketAttachments Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TicketAttachments]') AND type in (N'U'))
BEGIN
    -- Check if referenced tables exist
    DECLARE @TicketsTableExists2 BIT = 0;
    DECLARE @UsersTableExists3 BIT = 0;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND type in (N'U'))
    BEGIN
        SET @TicketsTableExists2 = 1;
    END
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        SET @UsersTableExists3 = 1;
    END

    -- Create table without foreign keys first
    CREATE TABLE [dbo].[TicketAttachments] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [TicketId] NVARCHAR(450) NOT NULL,
        [FileName] NVARCHAR(255) NOT NULL,
        [FileUrl] NVARCHAR(500) NOT NULL,
        [FileType] NVARCHAR(50) NULL,
        [FileSize] BIGINT NULL,
        [UploadedBy] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Create indexes
    CREATE INDEX [IX_TicketAttachments_TicketId] ON [dbo].[TicketAttachments]([TicketId]);
    CREATE INDEX [IX_TicketAttachments_UploadedBy] ON [dbo].[TicketAttachments]([UploadedBy]);
    
    -- Add foreign key constraints if referenced tables exist
    IF @TicketsTableExists2 = 1
    BEGIN
        ALTER TABLE [dbo].[TicketAttachments]
        ADD CONSTRAINT [FK_TicketAttachments_Tickets] 
        FOREIGN KEY ([TicketId]) REFERENCES [dbo].[Tickets]([Id]) ON DELETE CASCADE;
    END
    
    IF @UsersTableExists3 = 1
    BEGIN
        ALTER TABLE [dbo].[TicketAttachments]
        ADD CONSTRAINT [FK_TicketAttachments_Users] 
        FOREIGN KEY ([UploadedBy]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL;
    END
    
    PRINT 'TicketAttachments table created successfully';
END
ELSE
BEGIN
    PRINT 'TicketAttachments table already exists';
END
GO

-- =============================================
-- 4. TicketStatusHistories Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TicketStatusHistories]') AND type in (N'U'))
BEGIN
    -- Check if referenced tables exist
    DECLARE @TicketsTableExists3 BIT = 0;
    DECLARE @UsersTableExists4 BIT = 0;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND type in (N'U'))
    BEGIN
        SET @TicketsTableExists3 = 1;
    END
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        SET @UsersTableExists4 = 1;
    END

    -- Create table without foreign keys first
    CREATE TABLE [dbo].[TicketStatusHistories] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [TicketId] NVARCHAR(450) NOT NULL,
        [OldStatus] NVARCHAR(50) NOT NULL,
        [NewStatus] NVARCHAR(50) NOT NULL,
        [ChangedBy] NVARCHAR(450) NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [ChangedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Create indexes
    CREATE INDEX [IX_TicketStatusHistories_TicketId] ON [dbo].[TicketStatusHistories]([TicketId]);
    CREATE INDEX [IX_TicketStatusHistories_ChangedAt] ON [dbo].[TicketStatusHistories]([ChangedAt]);
    
    -- Add foreign key constraints if referenced tables exist
    IF @TicketsTableExists3 = 1
    BEGIN
        ALTER TABLE [dbo].[TicketStatusHistories]
        ADD CONSTRAINT [FK_TicketStatusHistories_Tickets] 
        FOREIGN KEY ([TicketId]) REFERENCES [dbo].[Tickets]([Id]) ON DELETE CASCADE;
    END
    
    IF @UsersTableExists4 = 1
    BEGIN
        ALTER TABLE [dbo].[TicketStatusHistories]
        ADD CONSTRAINT [FK_TicketStatusHistories_Users] 
        FOREIGN KEY ([ChangedBy]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL;
    END
    
    PRINT 'TicketStatusHistories table created successfully';
END
ELSE
BEGIN
    PRINT 'TicketStatusHistories table already exists';
END
GO

-- =============================================
-- Summary
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'Ticket Management System Tables Created';
PRINT '=============================================';
PRINT 'Tables created:';
PRINT '  1. Tickets';
PRINT '  2. TicketComments';
PRINT '  3. TicketAttachments';
PRINT '  4. TicketStatusHistories';
PRINT '=============================================';
GO

