-- =====================================================
-- Check and Add Soft Delete Columns (Safe Version)
-- This script checks existing columns and only adds missing ones
-- =====================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT 'Starting soft delete migration check...';
PRINT '';

-- Check existing columns in BankAccounts
PRINT 'Checking BankAccounts table...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'IsDeleted')
    PRINT '  - IsDeleted: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT '  - IsDeleted: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeletedAt')
    PRINT '  - DeletedAt: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [DeletedAt] datetime2 NULL;
    PRINT '  - DeletedAt: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeletedBy')
    PRINT '  - DeletedBy: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT '  - DeletedBy: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankAccounts' AND COLUMN_NAME = 'DeleteReason')
    PRINT '  - DeleteReason: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT '  - DeleteReason: ADDED';
END
GO

-- Check existing columns in BankTransactions
PRINT '';
PRINT 'Checking BankTransactions table...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'IsDeleted')
    PRINT '  - IsDeleted: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT '  - IsDeleted: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeletedAt')
    PRINT '  - DeletedAt: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [DeletedAt] datetime2 NULL;
    PRINT '  - DeletedAt: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeletedBy')
    PRINT '  - DeletedBy: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT '  - DeletedBy: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BankTransactions' AND COLUMN_NAME = 'DeleteReason')
    PRINT '  - DeleteReason: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT '  - DeleteReason: ADDED';
END
GO

-- Check existing columns in Payments
PRINT '';
PRINT 'Checking Payments table...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'IsDeleted')
    PRINT '  - IsDeleted: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT '  - IsDeleted: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeletedAt')
    PRINT '  - DeletedAt: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [DeletedAt] datetime2 NULL;
    PRINT '  - DeletedAt: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeletedBy')
    PRINT '  - DeletedBy: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT '  - DeletedBy: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'DeleteReason')
    PRINT '  - DeleteReason: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT '  - DeleteReason: ADDED';
END
GO

-- Check existing columns in Bills
PRINT '';
PRINT 'Checking Bills table...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'IsDeleted')
    PRINT '  - IsDeleted: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT '  - IsDeleted: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeletedAt')
    PRINT '  - DeletedAt: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [DeletedAt] datetime2 NULL;
    PRINT '  - DeletedAt: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeletedBy')
    PRINT '  - DeletedBy: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT '  - DeletedBy: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Bills' AND COLUMN_NAME = 'DeleteReason')
    PRINT '  - DeleteReason: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT '  - DeleteReason: ADDED';
END
GO

-- Check existing columns in IncomeSources
PRINT '';
PRINT 'Checking IncomeSources table...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'IsDeleted')
    PRINT '  - IsDeleted: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT '  - IsDeleted: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeletedAt')
    PRINT '  - DeletedAt: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [DeletedAt] datetime2 NULL;
    PRINT '  - DeletedAt: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeletedBy')
    PRINT '  - DeletedBy: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT '  - DeletedBy: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IncomeSources' AND COLUMN_NAME = 'DeleteReason')
    PRINT '  - DeleteReason: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT '  - DeleteReason: ADDED';
END
GO

-- Check existing columns in VariableExpenses
PRINT '';
PRINT 'Checking VariableExpenses table...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'IsDeleted')
    PRINT '  - IsDeleted: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT '  - IsDeleted: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeletedAt')
    PRINT '  - DeletedAt: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeletedAt] datetime2 NULL;
    PRINT '  - DeletedAt: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeletedBy')
    PRINT '  - DeletedBy: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT '  - DeletedBy: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'VariableExpenses' AND COLUMN_NAME = 'DeleteReason')
    PRINT '  - DeleteReason: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT '  - DeleteReason: ADDED';
END
GO

-- Check existing columns in SavingsTransactions
PRINT '';
PRINT 'Checking SavingsTransactions table...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'IsDeleted')
    PRINT '  - IsDeleted: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT '  - IsDeleted: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeletedAt')
    PRINT '  - DeletedAt: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeletedAt] datetime2 NULL;
    PRINT '  - DeletedAt: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeletedBy')
    PRINT '  - DeletedBy: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT '  - DeletedBy: ADDED';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SavingsTransactions' AND COLUMN_NAME = 'DeleteReason')
    PRINT '  - DeleteReason: EXISTS'
ELSE
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT '  - DeleteReason: ADDED';
END
GO

PRINT '';
PRINT 'Soft delete migration check completed!';
PRINT 'All missing columns have been added.';
GO

