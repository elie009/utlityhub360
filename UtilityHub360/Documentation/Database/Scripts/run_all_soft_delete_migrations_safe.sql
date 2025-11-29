-- =====================================================
-- Complete Soft Delete Migration for UtilityHub360 (Safe Version)
-- Adds soft delete columns to ALL required tables
-- This version uses a more robust checking method
-- =====================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT 'Starting soft delete migration...';

-- Helper function to check and add column safely
-- For BankAccounts table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankAccounts' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in BankAccounts table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankAccounts' 
    AND COLUMN_NAME = 'DeletedAt'
)
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in BankAccounts table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankAccounts' 
    AND COLUMN_NAME = 'DeletedBy'
)
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in BankAccounts table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankAccounts' 
    AND COLUMN_NAME = 'DeleteReason'
)
BEGIN
    ALTER TABLE [dbo].[BankAccounts] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in BankAccounts table';
END
GO

-- For BankTransactions table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankTransactions' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in BankTransactions table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankTransactions' 
    AND COLUMN_NAME = 'DeletedAt'
)
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in BankTransactions table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankTransactions' 
    AND COLUMN_NAME = 'DeletedBy'
)
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in BankTransactions table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'BankTransactions' 
    AND COLUMN_NAME = 'DeleteReason'
)
BEGIN
    ALTER TABLE [dbo].[BankTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in BankTransactions table';
END
GO

-- For Payments table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Payments' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Payments table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Payments table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Payments' 
    AND COLUMN_NAME = 'DeletedAt'
)
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to Payments table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in Payments table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Payments' 
    AND COLUMN_NAME = 'DeletedBy'
)
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to Payments table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in Payments table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Payments' 
    AND COLUMN_NAME = 'DeleteReason'
)
BEGIN
    ALTER TABLE [dbo].[Payments] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to Payments table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in Payments table';
END
GO

-- For Bills table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Bills' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Bills table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Bills table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Bills' 
    AND COLUMN_NAME = 'DeletedAt'
)
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to Bills table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in Bills table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Bills' 
    AND COLUMN_NAME = 'DeletedBy'
)
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to Bills table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in Bills table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'Bills' 
    AND COLUMN_NAME = 'DeleteReason'
)
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to Bills table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in Bills table';
END
GO

-- For IncomeSources table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'IncomeSources' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in IncomeSources table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'IncomeSources' 
    AND COLUMN_NAME = 'DeletedAt'
)
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in IncomeSources table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'IncomeSources' 
    AND COLUMN_NAME = 'DeletedBy'
)
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in IncomeSources table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'IncomeSources' 
    AND COLUMN_NAME = 'DeleteReason'
)
BEGIN
    ALTER TABLE [dbo].[IncomeSources] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in IncomeSources table';
END
GO

-- For VariableExpenses table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'VariableExpenses' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in VariableExpenses table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'VariableExpenses' 
    AND COLUMN_NAME = 'DeletedAt'
)
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in VariableExpenses table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'VariableExpenses' 
    AND COLUMN_NAME = 'DeletedBy'
)
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in VariableExpenses table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'VariableExpenses' 
    AND COLUMN_NAME = 'DeleteReason'
)
BEGIN
    ALTER TABLE [dbo].[VariableExpenses] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in VariableExpenses table';
END
GO

-- For SavingsTransactions table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'SavingsTransactions' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in SavingsTransactions table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'SavingsTransactions' 
    AND COLUMN_NAME = 'DeletedAt'
)
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in SavingsTransactions table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'SavingsTransactions' 
    AND COLUMN_NAME = 'DeletedBy'
)
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in SavingsTransactions table';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND TABLE_NAME = 'SavingsTransactions' 
    AND COLUMN_NAME = 'DeleteReason'
)
BEGIN
    ALTER TABLE [dbo].[SavingsTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in SavingsTransactions table';
END
GO

PRINT 'Soft delete migration completed successfully!';
GO

