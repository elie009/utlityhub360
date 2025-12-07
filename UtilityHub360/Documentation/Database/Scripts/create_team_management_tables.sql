-- Create Team Management tables
-- This script creates the Team, TeamMember, and TeamInvitation tables
-- Run this script to add team management functionality to the database

-- Teams Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Teams]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Teams] (
        [Id] [nvarchar](450) NOT NULL,
        [OwnerId] [nvarchar](450) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [MaxMembers] [int] NOT NULL DEFAULT 10,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_Teams_OwnerId] ON [dbo].[Teams] ([OwnerId]);
    CREATE INDEX [IX_Teams_IsActive] ON [dbo].[Teams] ([IsActive]);
    
    ALTER TABLE [dbo].[Teams]
    ADD CONSTRAINT [FK_Teams_Users_OwnerId] 
    FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION;
    
    PRINT 'Teams table created successfully.';
END
ELSE
BEGIN
    PRINT 'Teams table already exists.';
END
GO

-- TeamMembers Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TeamMembers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TeamMembers] (
        [Id] [nvarchar](450) NOT NULL,
        [TeamId] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [Role] [nvarchar](50) NOT NULL DEFAULT 'MEMBER',
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [JoinedAt] [datetime2](7) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_TeamMembers] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_TeamMembers_TeamId] ON [dbo].[TeamMembers] ([TeamId]);
    CREATE INDEX [IX_TeamMembers_UserId] ON [dbo].[TeamMembers] ([UserId]);
    CREATE UNIQUE INDEX [IX_TeamMembers_TeamId_UserId] ON [dbo].[TeamMembers] ([TeamId], [UserId]);
    CREATE INDEX [IX_TeamMembers_IsActive] ON [dbo].[TeamMembers] ([IsActive]);
    
    ALTER TABLE [dbo].[TeamMembers]
    ADD CONSTRAINT [FK_TeamMembers_Teams_TeamId] 
    FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]) ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[TeamMembers]
    ADD CONSTRAINT [FK_TeamMembers_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
    
    PRINT 'TeamMembers table created successfully.';
END
ELSE
BEGIN
    PRINT 'TeamMembers table already exists.';
END
GO

-- TeamInvitations Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TeamInvitations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TeamInvitations] (
        [Id] [nvarchar](450) NOT NULL,
        [TeamId] [nvarchar](450) NOT NULL,
        [InvitedByUserId] [nvarchar](450) NOT NULL,
        [Email] [nvarchar](255) NOT NULL,
        [Role] [nvarchar](50) NOT NULL DEFAULT 'MEMBER',
        [Token] [nvarchar](100) NOT NULL,
        [ExpiresAt] [datetime2](7) NOT NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT 'PENDING',
        [AcceptedByUserId] [nvarchar](450) NULL,
        [AcceptedAt] [datetime2](7) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_TeamInvitations] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_TeamInvitations_TeamId] ON [dbo].[TeamInvitations] ([TeamId]);
    CREATE INDEX [IX_TeamInvitations_Email] ON [dbo].[TeamInvitations] ([Email]);
    CREATE UNIQUE INDEX [IX_TeamInvitations_Token] ON [dbo].[TeamInvitations] ([Token]);
    CREATE INDEX [IX_TeamInvitations_Status] ON [dbo].[TeamInvitations] ([Status]);
    CREATE INDEX [IX_TeamInvitations_ExpiresAt] ON [dbo].[TeamInvitations] ([ExpiresAt]);
    
    ALTER TABLE [dbo].[TeamInvitations]
    ADD CONSTRAINT [FK_TeamInvitations_Teams_TeamId] 
    FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]) ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[TeamInvitations]
    ADD CONSTRAINT [FK_TeamInvitations_Users_InvitedByUserId] 
    FOREIGN KEY ([InvitedByUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION;
    
    ALTER TABLE [dbo].[TeamInvitations]
    ADD CONSTRAINT [FK_TeamInvitations_Users_AcceptedByUserId] 
    FOREIGN KEY ([AcceptedByUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION;
    
    PRINT 'TeamInvitations table created successfully.';
END
ELSE
BEGIN
    PRINT 'TeamInvitations table already exists.';
END
GO

