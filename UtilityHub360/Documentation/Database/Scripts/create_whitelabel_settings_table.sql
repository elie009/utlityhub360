-- Create WhiteLabelSettings table
-- This table stores white-label branding settings for Enterprise users
-- Run this script to create the WhiteLabelSettings table in your database

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WhiteLabelSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WhiteLabelSettings] (
        [Id] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [CompanyName] [nvarchar](100) NOT NULL,
        [LogoUrl] [nvarchar](500) NULL,
        [PrimaryColor] [nvarchar](7) NOT NULL DEFAULT '#1976d2',
        [SecondaryColor] [nvarchar](7) NOT NULL DEFAULT '#424242',
        [CustomDomain] [nvarchar](255) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_WhiteLabelSettings] PRIMARY KEY ([Id])
    );
    
    -- Create indexes
    CREATE UNIQUE INDEX [IX_WhiteLabelSettings_UserId] ON [dbo].[WhiteLabelSettings] ([UserId]);
    CREATE INDEX [IX_WhiteLabelSettings_IsActive] ON [dbo].[WhiteLabelSettings] ([IsActive]);
    
    -- Create foreign key
    ALTER TABLE [dbo].[WhiteLabelSettings]
    ADD CONSTRAINT [FK_WhiteLabelSettings_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
    
    PRINT 'WhiteLabelSettings table created successfully.';
END
ELSE
BEGIN
    PRINT 'WhiteLabelSettings table already exists.';
END
GO

