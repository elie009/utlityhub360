-- =====================================================
-- Complete Soft Delete Migration for UtilityHub360
-- Adds soft delete columns to ALL required tables
-- Run this script against your database
-- =====================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

BEGIN TRANSACTION;

PRINT 'Starting soft delete migration...';

-- Add soft delete columns to BankAccounts table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [BankAccounts] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in BankAccounts table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [BankAccounts] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in BankAccounts table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [BankAccounts] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in BankAccounts table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND name = 'DeleteReason')
BEGIN
    ALTER TABLE [BankAccounts] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in BankAccounts table';
END

-- Add soft delete columns to BankTransactions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [BankTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in BankTransactions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [BankTransactions] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in BankTransactions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [BankTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in BankTransactions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'DeleteReason')
BEGIN
    ALTER TABLE [BankTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to BankTransactions table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in BankTransactions table';
END

-- Add soft delete columns to Payments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Payments] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Payments table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Payments table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [Payments] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to Payments table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in Payments table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [Payments] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to Payments table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in Payments table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND name = 'DeleteReason')
BEGIN
    ALTER TABLE [Payments] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to Payments table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in Payments table';
END

-- Add soft delete columns to Bills table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Bills] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Bills table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Bills table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [Bills] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to Bills table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in Bills table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [Bills] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to Bills table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in Bills table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'DeleteReason')
BEGIN
    ALTER TABLE [Bills] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to Bills table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in Bills table';
END

-- Add soft delete columns to IncomeSources table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[IncomeSources]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [IncomeSources] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in IncomeSources table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[IncomeSources]') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [IncomeSources] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in IncomeSources table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[IncomeSources]') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [IncomeSources] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in IncomeSources table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[IncomeSources]') AND name = 'DeleteReason')
BEGIN
    ALTER TABLE [IncomeSources] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to IncomeSources table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in IncomeSources table';
END

-- Add soft delete columns to VariableExpenses table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[VariableExpenses]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [VariableExpenses] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in VariableExpenses table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[VariableExpenses]') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [VariableExpenses] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in VariableExpenses table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[VariableExpenses]') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [VariableExpenses] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in VariableExpenses table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[VariableExpenses]') AND name = 'DeleteReason')
BEGIN
    ALTER TABLE [VariableExpenses] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to VariableExpenses table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in VariableExpenses table';
END

-- Add soft delete columns to SavingsTransactions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SavingsTransactions]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [SavingsTransactions] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in SavingsTransactions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SavingsTransactions]') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [SavingsTransactions] ADD [DeletedAt] datetime2 NULL;
    PRINT 'Added DeletedAt column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in SavingsTransactions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SavingsTransactions]') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [SavingsTransactions] ADD [DeletedBy] nvarchar(450) NULL;
    PRINT 'Added DeletedBy column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in SavingsTransactions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SavingsTransactions]') AND name = 'DeleteReason')
BEGIN
    ALTER TABLE [SavingsTransactions] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added DeleteReason column to SavingsTransactions table';
END
ELSE
BEGIN
    PRINT 'DeleteReason column already exists in SavingsTransactions table';
END

COMMIT TRANSACTION;
GO

PRINT 'Soft delete migration completed successfully!';
GO

