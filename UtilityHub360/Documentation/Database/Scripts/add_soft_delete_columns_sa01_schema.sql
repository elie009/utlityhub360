-- =====================================================
-- Soft Delete Migration for sa01 Schema
-- Tables are in sa01 schema, not dbo
-- =====================================================

-- BankAccounts.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [sa01].[BankAccounts] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- BankAccounts.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [sa01].[BankAccounts] ADD [DeletedAt] datetime2 NULL;
GO

-- BankAccounts.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [sa01].[BankAccounts] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- BankAccounts.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [sa01].[BankAccounts] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- BankTransactions.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [sa01].[BankTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- BankTransactions.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [sa01].[BankTransactions] ADD [DeletedAt] datetime2 NULL;
GO

-- BankTransactions.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [sa01].[BankTransactions] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- BankTransactions.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [sa01].[BankTransactions] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- Payments.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [sa01].[Payments] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- Payments.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [sa01].[Payments] ADD [DeletedAt] datetime2 NULL;
GO

-- Payments.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [sa01].[Payments] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- Payments.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [sa01].[Payments] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- Bills.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [sa01].[Bills] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- Bills.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [sa01].[Bills] ADD [DeletedAt] datetime2 NULL;
GO

-- Bills.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [sa01].[Bills] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- Bills.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [sa01].[Bills] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- IncomeSources.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [sa01].[IncomeSources] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- IncomeSources.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [sa01].[IncomeSources] ADD [DeletedAt] datetime2 NULL;
GO

-- IncomeSources.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [sa01].[IncomeSources] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- IncomeSources.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [sa01].[IncomeSources] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- VariableExpenses.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [sa01].[VariableExpenses] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- VariableExpenses.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [sa01].[VariableExpenses] ADD [DeletedAt] datetime2 NULL;
GO

-- VariableExpenses.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [sa01].[VariableExpenses] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- VariableExpenses.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [sa01].[VariableExpenses] ADD [DeleteReason] nvarchar(500) NULL;
GO

-- SavingsTransactions.IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'IsDeleted')
    ALTER TABLE [sa01].[SavingsTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

-- SavingsTransactions.DeletedAt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeletedAt')
    ALTER TABLE [sa01].[SavingsTransactions] ADD [DeletedAt] datetime2 NULL;
GO

-- SavingsTransactions.DeletedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeletedBy')
    ALTER TABLE [sa01].[SavingsTransactions] ADD [DeletedBy] nvarchar(450) NULL;
GO

-- SavingsTransactions.DeleteReason
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'sa01' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeleteReason')
    ALTER TABLE [sa01].[SavingsTransactions] ADD [DeleteReason] nvarchar(500) NULL;
GO

PRINT 'Soft delete columns migration completed for sa01 schema!';

