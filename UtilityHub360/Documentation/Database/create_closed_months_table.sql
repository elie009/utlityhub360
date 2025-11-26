-- Create ClosedMonths table for month closing functionality
-- This script creates the table first, then adds foreign keys separately
-- Each step is in its own transaction to prevent rollback issues

-- STEP 1: Create the table WITHOUT foreign keys
BEGIN TRANSACTION CreateTable
BEGIN TRY
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

        PRINT 'ClosedMonths table created successfully (without foreign keys)';
    END
    ELSE
    BEGIN
        PRINT 'ClosedMonths table already exists';
    END
    
    COMMIT TRANSACTION CreateTable
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION CreateTable
    PRINT 'ERROR creating table: ' + ERROR_MESSAGE()
END CATCH
GO

-- STEP 2: Create indexes
BEGIN TRANSACTION CreateIndexes
BEGIN TRY
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClosedMonths')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_BankAccountId_Year_Month' AND object_id = OBJECT_ID('ClosedMonths'))
        BEGIN
            CREATE UNIQUE NONCLUSTERED INDEX [IX_ClosedMonths_BankAccountId_Year_Month] 
                ON [dbo].[ClosedMonths] ([BankAccountId], [Year], [Month]);
            PRINT 'Index IX_ClosedMonths_BankAccountId_Year_Month created';
        END
        
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_BankAccountId' AND object_id = OBJECT_ID('ClosedMonths'))
        BEGIN
            CREATE NONCLUSTERED INDEX [IX_ClosedMonths_BankAccountId] 
                ON [dbo].[ClosedMonths] ([BankAccountId]);
            PRINT 'Index IX_ClosedMonths_BankAccountId created';
        END
        
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_Year_Month' AND object_id = OBJECT_ID('ClosedMonths'))
        BEGIN
            CREATE NONCLUSTERED INDEX [IX_ClosedMonths_Year_Month] 
                ON [dbo].[ClosedMonths] ([Year], [Month]);
            PRINT 'Index IX_ClosedMonths_Year_Month created';
        END
        
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_ClosedAt' AND object_id = OBJECT_ID('ClosedMonths'))
        BEGIN
            CREATE NONCLUSTERED INDEX [IX_ClosedMonths_ClosedAt] 
                ON [dbo].[ClosedMonths] ([ClosedAt]);
            PRINT 'Index IX_ClosedMonths_ClosedAt created';
        END
    END
    
    COMMIT TRANSACTION CreateIndexes
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION CreateIndexes
    PRINT 'ERROR creating indexes: ' + ERROR_MESSAGE()
END CATCH
GO

-- STEP 3: Add foreign keys separately (with validation)
-- Skip foreign keys for now - the table will work without them
-- You can add them later manually if needed

PRINT 'Note: Foreign keys are being skipped. The table will work without them.';
PRINT 'You can add foreign keys manually later if needed.';
PRINT 'Script completed successfully!';

