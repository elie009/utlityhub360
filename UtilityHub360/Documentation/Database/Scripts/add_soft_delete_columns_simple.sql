-- =====================================================
-- Simple Soft Delete Migration - One Column at a Time
-- Run this script - it will only add columns that don't exist
-- =====================================================

-- BankAccounts.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [dbo].[BankAccounts] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- BankAccounts.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [dbo].[BankAccounts] ADD [DeletedAt] datetime2 NULL;
GO

-- BankAccounts.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [dbo].[BankAccounts] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- BankAccounts.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [dbo].[BankAccounts] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- BankTransactions.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [dbo].[BankTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- BankTransactions.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [dbo].[BankTransactions] ADD [DeletedAt] datetime2 NULL;
GO

-- BankTransactions.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [dbo].[BankTransactions] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- BankTransactions.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [dbo].[BankTransactions] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- Payments.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [dbo].[Payments] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- Payments.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [dbo].[Payments] ADD [DeletedAt] datetime2 NULL;
GO

-- Payments.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [dbo].[Payments] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- Payments.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [dbo].[Payments] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- Bills.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [dbo].[Bills] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- Bills.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [dbo].[Bills] ADD [DeletedAt] datetime2 NULL;
GO

-- Bills.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [dbo].[Bills] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- Bills.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [dbo].[Bills] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- IncomeSources.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [dbo].[IncomeSources] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- IncomeSources.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [dbo].[IncomeSources] ADD [DeletedAt] datetime2 NULL;
GO

-- IncomeSources.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [dbo].[IncomeSources] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- IncomeSources.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [dbo].[IncomeSources] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- VariableExpenses.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [dbo].[VariableExpenses] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- VariableExpenses.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeletedAt] datetime2 NULL;
GO

-- VariableExpenses.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- VariableExpenses.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- SavingsTransactions.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [dbo].[SavingsTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- SavingsTransactions.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeletedAt] datetime2 NULL;
GO

-- SavingsTransactions.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- SavingsTransactions.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeleteReason] nvarchar(500) NULL;
GO

PRINT 'Soft delete columns migration completed!';

