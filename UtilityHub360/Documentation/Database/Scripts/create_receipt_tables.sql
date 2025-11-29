-- Create Receipt Processing Tables
-- This script ensures ExpenseReceipt table exists with all necessary fields for OCR processing

-- Check if ExpenseReceipts table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExpenseReceipts')
BEGIN
    CREATE TABLE [dbo].[ExpenseReceipts] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [ExpenseId] NVARCHAR(450) NULL,
        [FileName] NVARCHAR(500) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [FileType] NVARCHAR(50) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [OriginalFileName] NVARCHAR(500) NULL,
        [ExtractedAmount] DECIMAL(18,2) NULL,
        [ExtractedDate] DATETIME2 NULL,
        [ExtractedMerchant] NVARCHAR(200) NULL,
        [ExtractedItems] NVARCHAR(500) NULL,
        [OcrText] NVARCHAR(1000) NULL,
        [IsOcrProcessed] BIT NOT NULL DEFAULT 0,
        [OcrProcessedAt] DATETIME2 NULL,
        [ThumbnailPath] NVARCHAR(500) NULL,
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        CONSTRAINT [FK_ExpenseReceipts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExpenseReceipts_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [dbo].[Expenses] ([Id]) ON DELETE SET NULL
    );

    -- Create indexes
    CREATE INDEX [IX_ExpenseReceipts_UserId] ON [dbo].[ExpenseReceipts] ([UserId]);
    CREATE INDEX [IX_ExpenseReceipts_ExpenseId] ON [dbo].[ExpenseReceipts] ([ExpenseId]);
    CREATE INDEX [IX_ExpenseReceipts_IsOcrProcessed] ON [dbo].[ExpenseReceipts] ([IsOcrProcessed]);
    CREATE INDEX [IX_ExpenseReceipts_CreatedAt] ON [dbo].[ExpenseReceipts] ([CreatedAt]);
    CREATE INDEX [IX_ExpenseReceipts_ExtractedDate] ON [dbo].[ExpenseReceipts] ([ExtractedDate]);
    CREATE INDEX [IX_ExpenseReceipts_ExtractedMerchant] ON [dbo].[ExpenseReceipts] ([ExtractedMerchant]);
    
    PRINT 'ExpenseReceipts table created successfully';
END
ELSE
BEGIN
    -- Add missing columns if table exists but columns are missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseReceipts') AND name = 'OcrText')
    BEGIN
        ALTER TABLE [dbo].[ExpenseReceipts] ADD [OcrText] NVARCHAR(1000) NULL;
        PRINT 'Added OcrText column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseReceipts') AND name = 'IsOcrProcessed')
    BEGIN
        ALTER TABLE [dbo].[ExpenseReceipts] ADD [IsOcrProcessed] BIT NOT NULL DEFAULT 0;
        PRINT 'Added IsOcrProcessed column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseReceipts') AND name = 'OcrProcessedAt')
    BEGIN
        ALTER TABLE [dbo].[ExpenseReceipts] ADD [OcrProcessedAt] DATETIME2 NULL;
        PRINT 'Added OcrProcessedAt column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseReceipts') AND name = 'ExtractedItems')
    BEGIN
        ALTER TABLE [dbo].[ExpenseReceipts] ADD [ExtractedItems] NVARCHAR(500) NULL;
        PRINT 'Added ExtractedItems column';
    END

    -- Create indexes if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExpenseReceipts_IsOcrProcessed' AND object_id = OBJECT_ID('dbo.ExpenseReceipts'))
    BEGIN
        CREATE INDEX [IX_ExpenseReceipts_IsOcrProcessed] ON [dbo].[ExpenseReceipts] ([IsOcrProcessed]);
        PRINT 'Created index IX_ExpenseReceipts_IsOcrProcessed';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExpenseReceipts_ExtractedDate' AND object_id = OBJECT_ID('dbo.ExpenseReceipts'))
    BEGIN
        CREATE INDEX [IX_ExpenseReceipts_ExtractedDate] ON [dbo].[ExpenseReceipts] ([ExtractedDate]);
        PRINT 'Created index IX_ExpenseReceipts_ExtractedDate';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExpenseReceipts_ExtractedMerchant' AND object_id = OBJECT_ID('dbo.ExpenseReceipts'))
    BEGIN
        CREATE INDEX [IX_ExpenseReceipts_ExtractedMerchant] ON [dbo].[ExpenseReceipts] ([ExtractedMerchant]);
        PRINT 'Created index IX_ExpenseReceipts_ExtractedMerchant';
    END

    PRINT 'ExpenseReceipts table already exists, columns and indexes updated';
END
GO

PRINT 'Receipt tables setup completed successfully';
GO

