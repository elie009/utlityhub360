-- Migration: Create Journal Entry Tables
-- Description: Creates tables for double-entry accounting system
-- Date: December 2024

-- Create JournalEntries table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JournalEntries] (
        [Id] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [LoanId] [nvarchar](450) NULL,
        [EntryType] [nvarchar](50) NOT NULL,
        [EntryDate] [datetime2](7) NOT NULL,
        [Description] [nvarchar](500) NOT NULL,
        [Reference] [nvarchar](100) NULL,
        [TotalDebit] [decimal](18,2) NOT NULL,
        [TotalCredit] [decimal](18,2) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id])
    );
    
    -- Create indexes
    CREATE INDEX [IX_JournalEntries_LoanId] ON [dbo].[JournalEntries] ([LoanId]);
    CREATE INDEX [IX_JournalEntries_UserId] ON [dbo].[JournalEntries] ([UserId]);
    CREATE INDEX [IX_JournalEntries_EntryType] ON [dbo].[JournalEntries] ([EntryType]);
    CREATE INDEX [IX_JournalEntries_EntryDate] ON [dbo].[JournalEntries] ([EntryDate]);
    
    -- Foreign key to Users
    ALTER TABLE [dbo].[JournalEntries]
    ADD CONSTRAINT [FK_JournalEntries_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
    
    -- Foreign key to Loans
    ALTER TABLE [dbo].[JournalEntries]
    ADD CONSTRAINT [FK_JournalEntries_Loans_LoanId] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([Id]) ON DELETE CASCADE;
    
    PRINT 'Created JournalEntries table';
END
ELSE
BEGIN
    PRINT 'JournalEntries table already exists';
END
GO

-- Create JournalEntryLines table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntryLines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JournalEntryLines] (
        [Id] [nvarchar](450) NOT NULL,
        [JournalEntryId] [nvarchar](450) NOT NULL,
        [AccountName] [nvarchar](100) NOT NULL,
        [AccountType] [nvarchar](50) NOT NULL,
        [EntrySide] [nvarchar](10) NOT NULL,
        [Amount] [decimal](18,2) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY ([Id])
    );
    
    -- Create indexes
    CREATE INDEX [IX_JournalEntryLines_JournalEntryId] ON [dbo].[JournalEntryLines] ([JournalEntryId]);
    CREATE INDEX [IX_JournalEntryLines_AccountName] ON [dbo].[JournalEntryLines] ([AccountName]);
    CREATE INDEX [IX_JournalEntryLines_AccountType] ON [dbo].[JournalEntryLines] ([AccountType]);
    
    -- Foreign key to JournalEntries
    ALTER TABLE [dbo].[JournalEntryLines]
    ADD CONSTRAINT [FK_JournalEntryLines_JournalEntries_JournalEntryId] 
    FOREIGN KEY ([JournalEntryId]) REFERENCES [dbo].[JournalEntries] ([Id]) ON DELETE CASCADE;
    
    PRINT 'Created JournalEntryLines table';
END
ELSE
BEGIN
    PRINT 'JournalEntryLines table already exists';
END
GO

PRINT 'Journal entry tables migration completed successfully!';
GO




