-- Migration: Create Investment Tracking Tables
-- Run this script to add comprehensive investment tracking support
-- This script creates the Investments, InvestmentPositions, and InvestmentTransactions tables

-- Create Investments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Investments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Investments] (
        [Id] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [AccountName] [nvarchar](100) NOT NULL,
        [InvestmentType] [nvarchar](50) NOT NULL,
        [AccountType] [nvarchar](50) NULL,
        [BrokerName] [nvarchar](100) NULL,
        [AccountNumber] [nvarchar](100) NULL,
        [InitialInvestment] [decimal](18,2) NOT NULL DEFAULT 0,
        [CurrentValue] [decimal](18,2) NOT NULL DEFAULT 0,
        [TotalCostBasis] [decimal](18,2) NOT NULL DEFAULT 0,
        [UnrealizedGainLoss] [decimal](18,2) NULL,
        [RealizedGainLoss] [decimal](18,2) NULL,
        [TotalReturnPercentage] [decimal](5,2) NULL,
        [Currency] [nvarchar](10) NOT NULL DEFAULT 'USD',
        [Description] [nvarchar](500) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](450) NULL,
        [DeleteReason] [nvarchar](500) NULL,
        CONSTRAINT [PK_Investments] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_Investments_UserId] ON [dbo].[Investments] ([UserId]);
    CREATE INDEX [IX_Investments_IsActive] ON [dbo].[Investments] ([IsActive]);
    
    -- Add foreign key constraint only if Users table exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Investments_Users_UserId')
        BEGIN
            ALTER TABLE [dbo].[Investments]
            ADD CONSTRAINT [FK_Investments_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
            
            PRINT 'Foreign key FK_Investments_Users_UserId created successfully.';
        END
        ELSE
        BEGIN
            PRINT 'Foreign key FK_Investments_Users_UserId already exists.';
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Users table does not exist. Foreign key FK_Investments_Users_UserId not created.';
    END
    
    PRINT 'Investments table created successfully.';
END
ELSE
BEGIN
    PRINT 'Investments table already exists.';
    
    -- Add foreign key constraint if it doesn't exist
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Investments_Users_UserId')
        BEGIN
            ALTER TABLE [dbo].[Investments]
            ADD CONSTRAINT [FK_Investments_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
            
            PRINT 'Foreign key FK_Investments_Users_UserId created successfully.';
        END
    END
END
GO

-- Create InvestmentPositions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvestmentPositions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InvestmentPositions] (
        [Id] [nvarchar](450) NOT NULL,
        [InvestmentId] [nvarchar](450) NOT NULL,
        [Symbol] [nvarchar](50) NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [AssetType] [nvarchar](50) NOT NULL,
        [Quantity] [decimal](18,4) NOT NULL,
        [AverageCostBasis] [decimal](18,2) NOT NULL,
        [TotalCostBasis] [decimal](18,2) NOT NULL,
        [CurrentPrice] [decimal](18,2) NULL,
        [CurrentValue] [decimal](18,2) NULL,
        [UnrealizedGainLoss] [decimal](18,2) NULL,
        [GainLossPercentage] [decimal](5,2) NULL,
        [DividendsReceived] [decimal](18,2) NULL,
        [InterestReceived] [decimal](18,2) NULL,
        [LastPriceUpdate] [datetime2](7) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_InvestmentPositions] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_InvestmentPositions_InvestmentId] ON [dbo].[InvestmentPositions] ([InvestmentId]);
    CREATE INDEX [IX_InvestmentPositions_Symbol] ON [dbo].[InvestmentPositions] ([Symbol]);
    
    ALTER TABLE [dbo].[InvestmentPositions]
    ADD CONSTRAINT [FK_InvestmentPositions_Investments_InvestmentId] 
    FOREIGN KEY ([InvestmentId]) REFERENCES [dbo].[Investments] ([Id]) ON DELETE CASCADE;
    
    PRINT 'InvestmentPositions table created successfully.';
END
ELSE
BEGIN
    PRINT 'InvestmentPositions table already exists.';
    
    -- Add foreign key constraint if it doesn't exist
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Investments]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InvestmentPositions_Investments_InvestmentId')
        BEGIN
            ALTER TABLE [dbo].[InvestmentPositions]
            ADD CONSTRAINT [FK_InvestmentPositions_Investments_InvestmentId] 
            FOREIGN KEY ([InvestmentId]) REFERENCES [dbo].[Investments] ([Id]) ON DELETE CASCADE;
            
            PRINT 'Foreign key FK_InvestmentPositions_Investments_InvestmentId created successfully.';
        END
    END
END
GO

-- Create InvestmentTransactions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvestmentTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InvestmentTransactions] (
        [Id] [nvarchar](450) NOT NULL,
        [InvestmentId] [nvarchar](450) NOT NULL,
        [PositionId] [nvarchar](450) NULL,
        [TransactionType] [nvarchar](50) NOT NULL,
        [Symbol] [nvarchar](50) NOT NULL,
        [Name] [nvarchar](200) NULL,
        [Quantity] [decimal](18,4) NULL,
        [PricePerShare] [decimal](18,2) NULL,
        [Amount] [decimal](18,2) NOT NULL,
        [Fees] [decimal](18,2) NULL,
        [Taxes] [decimal](18,2) NULL,
        [Currency] [nvarchar](10) NOT NULL DEFAULT 'USD',
        [Description] [nvarchar](500) NULL,
        [Reference] [nvarchar](100) NULL,
        [TransactionDate] [datetime2](7) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](450) NULL,
        [DeleteReason] [nvarchar](500) NULL,
        CONSTRAINT [PK_InvestmentTransactions] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_InvestmentTransactions_InvestmentId] ON [dbo].[InvestmentTransactions] ([InvestmentId]);
    CREATE INDEX [IX_InvestmentTransactions_TransactionDate] ON [dbo].[InvestmentTransactions] ([TransactionDate]);
    CREATE INDEX [IX_InvestmentTransactions_TransactionType] ON [dbo].[InvestmentTransactions] ([TransactionType]);
    
    ALTER TABLE [dbo].[InvestmentTransactions]
    ADD CONSTRAINT [FK_InvestmentTransactions_Investments_InvestmentId] 
    FOREIGN KEY ([InvestmentId]) REFERENCES [dbo].[Investments] ([Id]) ON DELETE CASCADE;
    
    -- Use NO ACTION to avoid cascade path conflicts
    -- PositionId is nullable, so if position is deleted, transaction remains but PositionId becomes NULL
    ALTER TABLE [dbo].[InvestmentTransactions]
    ADD CONSTRAINT [FK_InvestmentTransactions_InvestmentPositions_PositionId] 
    FOREIGN KEY ([PositionId]) REFERENCES [dbo].[InvestmentPositions] ([Id]) ON DELETE NO ACTION;
    
    PRINT 'InvestmentTransactions table created successfully.';
END
ELSE
BEGIN
    PRINT 'InvestmentTransactions table already exists.';
    
    -- Add foreign key constraints if they don't exist
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Investments]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InvestmentTransactions_Investments_InvestmentId')
        BEGIN
            ALTER TABLE [dbo].[InvestmentTransactions]
            ADD CONSTRAINT [FK_InvestmentTransactions_Investments_InvestmentId] 
            FOREIGN KEY ([InvestmentId]) REFERENCES [dbo].[Investments] ([Id]) ON DELETE CASCADE;
            
            PRINT 'Foreign key FK_InvestmentTransactions_Investments_InvestmentId created successfully.';
        END
    END
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvestmentPositions]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InvestmentTransactions_InvestmentPositions_PositionId')
        BEGIN
            ALTER TABLE [dbo].[InvestmentTransactions]
            ADD CONSTRAINT [FK_InvestmentTransactions_InvestmentPositions_PositionId] 
            FOREIGN KEY ([PositionId]) REFERENCES [dbo].[InvestmentPositions] ([Id]) ON DELETE NO ACTION;
            
            PRINT 'Foreign key FK_InvestmentTransactions_InvestmentPositions_PositionId created successfully.';
        END
    END
END
GO

PRINT 'Investment tracking tables migration completed.';

