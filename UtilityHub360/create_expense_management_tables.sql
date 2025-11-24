-- ============================================
-- Expense Management System - Database Migration
-- ============================================
-- This script creates all tables for the comprehensive expense management system
-- Run this script to add expense management functionality to the database
-- ============================================

USE [UtilityHub360]
GO

-- ============================================
-- 1. ExpenseCategories Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseCategories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExpenseCategories] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Icon] NVARCHAR(50) NULL,
        [Color] NVARCHAR(20) NULL,
        [MonthlyBudget] DECIMAL(18,2) NULL,
        [YearlyBudget] DECIMAL(18,2) NULL,
        [ParentCategoryId] NVARCHAR(450) NULL,
        [IsTaxDeductible] BIT NOT NULL DEFAULT 0,
        [TaxCategory] NVARCHAR(50) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        CONSTRAINT [FK_ExpenseCategories_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExpenseCategories_ParentCategory] FOREIGN KEY ([ParentCategoryId]) REFERENCES [dbo].[ExpenseCategories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [UQ_ExpenseCategories_User_Name] UNIQUE ([UserId], [Name])
    );

    CREATE INDEX [IX_ExpenseCategories_UserId] ON [dbo].[ExpenseCategories] ([UserId]);
    CREATE INDEX [IX_ExpenseCategories_ParentCategoryId] ON [dbo].[ExpenseCategories] ([ParentCategoryId]);
    CREATE INDEX [IX_ExpenseCategories_IsActive] ON [dbo].[ExpenseCategories] ([IsActive]);
END
GO

-- ============================================
-- 2. ExpenseBudgets Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseBudgets]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExpenseBudgets] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [CategoryId] NVARCHAR(450) NOT NULL,
        [BudgetAmount] DECIMAL(18,2) NOT NULL,
        [PeriodType] NVARCHAR(20) NOT NULL DEFAULT 'MONTHLY',
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [AlertThreshold] DECIMAL(5,2) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        CONSTRAINT [FK_ExpenseBudgets_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExpenseBudgets_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[ExpenseCategories] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ExpenseBudgets_UserId] ON [dbo].[ExpenseBudgets] ([UserId]);
    CREATE INDEX [IX_ExpenseBudgets_CategoryId] ON [dbo].[ExpenseBudgets] ([CategoryId]);
    CREATE INDEX [IX_ExpenseBudgets_StartDate] ON [dbo].[ExpenseBudgets] ([StartDate]);
    CREATE INDEX [IX_ExpenseBudgets_EndDate] ON [dbo].[ExpenseBudgets] ([EndDate]);
    CREATE INDEX [IX_ExpenseBudgets_IsActive] ON [dbo].[ExpenseBudgets] ([IsActive]);
    CREATE INDEX [IX_ExpenseBudgets_User_Category_Dates] ON [dbo].[ExpenseBudgets] ([UserId], [CategoryId], [StartDate], [EndDate]);
END
GO

-- ============================================
-- 3. ExpenseReceipts Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseReceipts]') AND type in (N'U'))
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
        CONSTRAINT [FK_ExpenseReceipts_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ExpenseReceipts_UserId] ON [dbo].[ExpenseReceipts] ([UserId]);
    CREATE INDEX [IX_ExpenseReceipts_ExpenseId] ON [dbo].[ExpenseReceipts] ([ExpenseId]);
    CREATE INDEX [IX_ExpenseReceipts_IsOcrProcessed] ON [dbo].[ExpenseReceipts] ([IsOcrProcessed]);
END
GO

-- ============================================
-- 4. Expenses Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Expenses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Expenses] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Description] NVARCHAR(200) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [CategoryId] NVARCHAR(450) NOT NULL,
        [ExpenseDate] DATETIME2 NOT NULL,
        [Currency] NVARCHAR(10) NOT NULL DEFAULT 'USD',
        [Notes] NVARCHAR(500) NULL,
        [Merchant] NVARCHAR(200) NULL,
        [PaymentMethod] NVARCHAR(50) NULL,
        [BankAccountId] NVARCHAR(450) NULL,
        [Location] NVARCHAR(100) NULL,
        [IsTaxDeductible] BIT NOT NULL DEFAULT 0,
        [IsReimbursable] BIT NOT NULL DEFAULT 0,
        [ReimbursementRequestId] NVARCHAR(450) NULL,
        [Mileage] DECIMAL(18,2) NULL,
        [MileageRate] DECIMAL(18,2) NULL,
        [PerDiemAmount] DECIMAL(18,2) NULL,
        [NumberOfDays] INT NULL,
        [ApprovalStatus] NVARCHAR(20) NOT NULL DEFAULT 'PENDING_APPROVAL',
        [ApprovedBy] NVARCHAR(450) NULL,
        [ApprovedAt] DATETIME2 NULL,
        [ApprovalNotes] NVARCHAR(500) NULL,
        [HasReceipt] BIT NOT NULL DEFAULT 0,
        [ReceiptId] NVARCHAR(450) NULL,
        [BudgetId] NVARCHAR(450) NULL,
        [IsRecurring] BIT NOT NULL DEFAULT 0,
        [RecurringFrequency] NVARCHAR(50) NULL,
        [ParentExpenseId] NVARCHAR(450) NULL,
        [Tags] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(450) NULL,
        [DeleteReason] NVARCHAR(500) NULL,
        CONSTRAINT [FK_Expenses_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Expenses_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[ExpenseCategories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Expenses_Receipts] FOREIGN KEY ([ReceiptId]) REFERENCES [dbo].[ExpenseReceipts] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Expenses_Budgets] FOREIGN KEY ([BudgetId]) REFERENCES [dbo].[ExpenseBudgets] ([Id]) ON DELETE SET NULL
    );

    CREATE INDEX [IX_Expenses_UserId] ON [dbo].[Expenses] ([UserId]);
    CREATE INDEX [IX_Expenses_CategoryId] ON [dbo].[Expenses] ([CategoryId]);
    CREATE INDEX [IX_Expenses_ExpenseDate] ON [dbo].[Expenses] ([ExpenseDate]);
    CREATE INDEX [IX_Expenses_ApprovalStatus] ON [dbo].[Expenses] ([ApprovalStatus]);
    CREATE INDEX [IX_Expenses_BudgetId] ON [dbo].[Expenses] ([BudgetId]);
    CREATE INDEX [IX_Expenses_User_Date] ON [dbo].[Expenses] ([UserId], [ExpenseDate]);
    CREATE INDEX [IX_Expenses_User_Category_Date] ON [dbo].[Expenses] ([UserId], [CategoryId], [ExpenseDate]);
END
GO

-- ============================================
-- 5. ExpenseApprovals Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseApprovals]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExpenseApprovals] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [ExpenseId] NVARCHAR(450) NOT NULL,
        [RequestedBy] NVARCHAR(450) NOT NULL,
        [ApprovedBy] NVARCHAR(450) NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'PENDING',
        [Notes] NVARCHAR(500) NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        [RequestedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ReviewedAt] DATETIME2 NULL,
        [ApprovalLevel] INT NOT NULL DEFAULT 1,
        [NextApproverId] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_ExpenseApprovals_Expenses] FOREIGN KEY ([ExpenseId]) REFERENCES [dbo].[Expenses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExpenseApprovals_RequestedBy] FOREIGN KEY ([RequestedBy]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ExpenseApprovals_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ExpenseApprovals_ExpenseId] ON [dbo].[ExpenseApprovals] ([ExpenseId]);
    CREATE INDEX [IX_ExpenseApprovals_RequestedBy] ON [dbo].[ExpenseApprovals] ([RequestedBy]);
    CREATE INDEX [IX_ExpenseApprovals_Status] ON [dbo].[ExpenseApprovals] ([Status]);
    CREATE INDEX [IX_ExpenseApprovals_RequestedAt] ON [dbo].[ExpenseApprovals] ([RequestedAt]);
END
GO

-- ============================================
-- Add Foreign Key for ExpenseReceipts.ExpenseId
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ExpenseReceipts_Expenses')
BEGIN
    ALTER TABLE [dbo].[ExpenseReceipts]
    ADD CONSTRAINT [FK_ExpenseReceipts_Expenses] FOREIGN KEY ([ExpenseId]) REFERENCES [dbo].[Expenses] ([Id]) ON DELETE SET NULL;
END
GO

-- ============================================
-- Migration Complete
-- ============================================
PRINT 'Expense Management tables created successfully!'
PRINT 'Tables created:'
PRINT '  - ExpenseCategories'
PRINT '  - ExpenseBudgets'
PRINT '  - ExpenseReceipts'
PRINT '  - Expenses'
PRINT '  - ExpenseApprovals'
GO

