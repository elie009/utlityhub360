-- QUICK FIX: Create ClosedMonths table WITHOUT foreign keys first
-- This will allow the table to be created immediately, even if referenced tables don't exist

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClosedMonths')
BEGIN
    CREATE TABLE [dbo].[ClosedMonths] (
        [Id] NVARCHAR(450) NOT NULL,
        [BankAccountId] NVARCHAR(450) NOT NULL,
        [Year] INT NOT NULL,
        [Month] INT NOT NULL,
        [ClosedBy] NVARCHAR(450) NOT NULL,
        [ClosedAt] DATETIME2 NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_ClosedMonths] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_ClosedMonths_Month_Range] CHECK ([Month] >= 1 AND [Month] <= 12),
        CONSTRAINT [CK_ClosedMonths_Year_Range] CHECK ([Year] >= 2000 AND [Year] <= 2100)
    );

    -- Create indexes
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ClosedMonths_BankAccountId_Year_Month] 
        ON [dbo].[ClosedMonths] ([BankAccountId], [Year], [Month]);
    
    CREATE NONCLUSTERED INDEX [IX_ClosedMonths_BankAccountId] 
        ON [dbo].[ClosedMonths] ([BankAccountId]);
    
    CREATE NONCLUSTERED INDEX [IX_ClosedMonths_Year_Month] 
        ON [dbo].[ClosedMonths] ([Year], [Month]);
    
    CREATE NONCLUSTERED INDEX [IX_ClosedMonths_ClosedAt] 
        ON [dbo].[ClosedMonths] ([ClosedAt]);
    
    PRINT 'ClosedMonths table created successfully!';
    PRINT 'Note: Foreign keys were not added. Add them manually after confirming the referenced table names.';
END
ELSE
BEGIN
    PRINT 'ClosedMonths table already exists';
END
GO

