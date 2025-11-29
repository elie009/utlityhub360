-- =====================================================
-- Soft Delete Migration for UtilityHub360
-- Adds soft delete columns to all transaction tables
-- =====================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

BEGIN TRANSACTION;

-- Add soft delete columns to Bills table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Bills] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [Bills] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [Bills] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [Bills] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to Bills table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in Bills table';
END

-- Add soft delete columns to Payments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Payments] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [Payments] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [Payments] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [Payments] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to Payments table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in Payments table';
END

-- Add soft delete columns to BankTransactions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [BankTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [BankTransactions] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [BankTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [BankTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in BankTransactions table';
END

-- Add soft delete columns to IncomeSources table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[IncomeSources]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [IncomeSources] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [IncomeSources] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [IncomeSources] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [IncomeSources] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in IncomeSources table';
END

-- Add soft delete columns to VariableExpenses table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[VariableExpenses]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [VariableExpenses] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [VariableExpenses] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [VariableExpenses] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [VariableExpenses] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in VariableExpenses table';
END

-- Add soft delete columns to SavingsTransactions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SavingsTransactions]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [SavingsTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [SavingsTransactions] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [SavingsTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [SavingsTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in SavingsTransactions table';
END

-- Add soft delete columns to BankAccounts table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [BankAccounts] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [BankAccounts] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [BankAccounts] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [BankAccounts] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in BankAccounts table';
END

-- Update migration history
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251028160148_AddSoftDeleteToTransactions')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251028160148_AddSoftDeleteToTransactions', N'8.0.0');
    PRINT 'Migration recorded in history';
END
ELSE
BEGIN
    PRINT 'Migration already recorded in history';
END

-- Update admin user timestamps
UPDATE [Users] 
SET [CreatedAt] = '2025-10-28T16:01:45.2133151Z', 
    [UpdatedAt] = '2025-10-28T16:01:45.2133152Z'
WHERE [Id] = N'admin-001';

COMMIT TRANSACTION;

PRINT 'Soft delete migration completed successfully!';
PRINT 'Tables updated: Bills, Payments, BankTransactions, IncomeSources, VariableExpenses, SavingsTransactions, BankAccounts';

