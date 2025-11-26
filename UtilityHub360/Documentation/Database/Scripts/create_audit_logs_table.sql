-- ==========================================
-- AUDIT LOGS TABLE CREATION
-- ==========================================
-- This script creates the comprehensive audit logging system
-- for tracking user activities, system events, and compliance requirements
--
-- NOTE: Make sure you're connected to the correct database before running this script
-- The database name should be: DBUTILS (or your actual database name)
-- You can set it manually: USE [DBUTILS]; GO
--
-- If you need to use a different database, uncomment and modify the line below:
-- USE [YourDatabaseName]
-- GO

-- Create AuditLogs table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [UserEmail] NVARCHAR(100) NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [EntityType] NVARCHAR(50) NOT NULL,
        [EntityId] NVARCHAR(450) NULL,
        [EntityName] NVARCHAR(200) NULL,
        [LogType] NVARCHAR(50) NOT NULL,
        [Severity] NVARCHAR(50) NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [IpAddress] NVARCHAR(45) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [RequestMethod] NVARCHAR(100) NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [RequestId] NVARCHAR(450) NULL,
        [ComplianceType] NVARCHAR(50) NULL,
        [Category] NVARCHAR(100) NULL,
        [SubCategory] NVARCHAR(100) NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'AuditLogs table created successfully';
END
ELSE
BEGIN
    PRINT 'AuditLogs table already exists';
END
GO

-- Check if Users table exists before creating foreign key
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    -- Create Foreign Key to Users table
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AuditLogs_Users')
    BEGIN
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
        BEGIN
            ALTER TABLE [dbo].[AuditLogs]
            ADD CONSTRAINT [FK_AuditLogs_Users] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
            ON DELETE SET NULL; -- Don't cascade delete to preserve audit trail
            
            PRINT 'Foreign key FK_AuditLogs_Users created successfully';
        END
        ELSE
        BEGIN
            PRINT 'WARNING: AuditLogs table does not exist. Cannot create foreign key.';
        END
    END
    ELSE
    BEGIN
        PRINT 'Foreign key FK_AuditLogs_Users already exists';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: Users table does not exist. Foreign key FK_AuditLogs_Users will not be created.';
    PRINT 'Please ensure the Users table exists before creating the foreign key relationship.';
END
GO

-- Create Indexes for Performance
-- Only create indexes if the table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_UserId' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs] ([UserId]);
        PRINT 'Index IX_AuditLogs_UserId created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_UserId already exists';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: AuditLogs table does not exist. Cannot create indexes.';
END
GO

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityType' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_EntityType] ON [dbo].[AuditLogs] ([EntityType]);
        PRINT 'Index IX_AuditLogs_EntityType created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_EntityType already exists';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityId' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_EntityId] ON [dbo].[AuditLogs] ([EntityId]);
        PRINT 'Index IX_AuditLogs_EntityId created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_EntityId already exists';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_LogType' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_LogType] ON [dbo].[AuditLogs] ([LogType]);
        PRINT 'Index IX_AuditLogs_LogType created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_LogType already exists';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Action' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_Action] ON [dbo].[AuditLogs] ([Action]);
        PRINT 'Index IX_AuditLogs_Action created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_Action already exists';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_CreatedAt] ON [dbo].[AuditLogs] ([CreatedAt]);
        PRINT 'Index IX_AuditLogs_CreatedAt created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_CreatedAt already exists';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_ComplianceType' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_ComplianceType] ON [dbo].[AuditLogs] ([ComplianceType]);
        PRINT 'Index IX_AuditLogs_ComplianceType created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_ComplianceType already exists';
    END

    -- Composite Indexes for Common Queries
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_UserId_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_UserId_CreatedAt] ON [dbo].[AuditLogs] ([UserId], [CreatedAt]);
        PRINT 'Index IX_AuditLogs_UserId_CreatedAt created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_UserId_CreatedAt already exists';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityType_EntityId' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [dbo].[AuditLogs] ([EntityType], [EntityId]);
        PRINT 'Index IX_AuditLogs_EntityType_EntityId created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_EntityType_EntityId already exists';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_LogType_Severity' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
    BEGIN
        CREATE INDEX [IX_AuditLogs_LogType_Severity] ON [dbo].[AuditLogs] ([LogType], [Severity]);
        PRINT 'Index IX_AuditLogs_LogType_Severity created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Index IX_AuditLogs_LogType_Severity already exists';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: AuditLogs table does not exist. Cannot create indexes.';
END
GO

PRINT '==========================================';
PRINT 'Audit Logs table and indexes created successfully';
PRINT '==========================================';
GO

