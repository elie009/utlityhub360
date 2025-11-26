-- Create TransactionCategories table
-- This migration creates the TransactionCategories table for managing transaction categories

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransactionCategories]') AND type in (N'U'))
BEGIN
    PRINT 'Creating TransactionCategories table...';
    CREATE TABLE [dbo].[TransactionCategories] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Type] NVARCHAR(50) NOT NULL DEFAULT 'EXPENSE',
        [Icon] NVARCHAR(50) NULL,
        [Color] NVARCHAR(20) NULL,
        [IsSystemCategory] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL
    );

    -- Create indexes
    CREATE INDEX [IX_TransactionCategories_UserId] ON [dbo].[TransactionCategories] ([UserId]);
    CREATE INDEX [IX_TransactionCategories_Type] ON [dbo].[TransactionCategories] ([Type]);
    CREATE INDEX [IX_TransactionCategories_IsActive] ON [dbo].[TransactionCategories] ([IsActive]);
    CREATE INDEX [IX_TransactionCategories_IsSystemCategory] ON [dbo].[TransactionCategories] ([IsSystemCategory]);
    
    -- Unique constraint for UserId and Name combination
    CREATE UNIQUE INDEX [IX_TransactionCategories_UserId_Name] 
        ON [dbo].[TransactionCategories] ([UserId], [Name])
        WHERE [IsDeleted] = 0;

    -- Only add foreign key if Users table exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[TransactionCategories]
            ADD CONSTRAINT [FK_TransactionCategories_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key FK_TransactionCategories_Users_UserId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Users table not found. Foreign key FK_TransactionCategories_Users_UserId skipped.';
    END

    PRINT 'TransactionCategories table created successfully!';
END
ELSE
BEGIN
    PRINT 'TransactionCategories table already exists.';
    
    -- Add foreign key if table exists but foreign key doesn't
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TransactionCategories_Users_UserId')
        BEGIN
            ALTER TABLE [dbo].[TransactionCategories]
                ADD CONSTRAINT [FK_TransactionCategories_Users_UserId] 
                FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
            PRINT 'Foreign key FK_TransactionCategories_Users_UserId added to existing table.';
        END
        ELSE
        BEGIN
            PRINT 'Foreign key FK_TransactionCategories_Users_UserId already exists.';
        END
    END
END
GO

