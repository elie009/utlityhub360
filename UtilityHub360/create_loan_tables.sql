-- =============================================
-- Loan Management System Database Tables
-- Created for UtilityHub360
-- =============================================

USE [DBUTILS]
GO

-- =============================================
-- 1. BORROWERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Borrowers] (
        [BorrowerId] [int] IDENTITY(1,1) NOT NULL,
        [FirstName] [nvarchar](100) NOT NULL,
        [LastName] [nvarchar](100) NOT NULL,
        [Email] [nvarchar](200) NULL,
        [Phone] [nvarchar](20) NULL,
        [Address] [nvarchar](255) NULL,
        [GovernmentId] [nvarchar](50) NULL,
        [Status] [nvarchar](20) NULL,
        [CreatedAt] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Borrowers] PRIMARY KEY ([BorrowerId])
    )
    PRINT 'Borrowers table created successfully'
END
ELSE
    PRINT 'Borrowers table already exists'
GO

-- =============================================
-- 2. LOANS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Loans] (
        [LoanId] [int] IDENTITY(1,1) NOT NULL,
        [BorrowerId] [int] NOT NULL,
        [LoanType] [nvarchar](50) NULL,
        [PrincipalAmount] [decimal](18,2) NOT NULL,
        [InterestRate] [decimal](5,2) NOT NULL,
        [TermMonths] [int] NOT NULL,
        [RepaymentFrequency] [nvarchar](20) NULL,
        [AmortizationType] [nvarchar](20) NULL,
        [StartDate] [datetime] NOT NULL,
        [EndDate] [datetime] NULL,
        [Status] [nvarchar](20) NULL,
        [CreatedAt] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Loans] PRIMARY KEY ([LoanId])
    )
    PRINT 'Loans table created successfully'
END
ELSE
    PRINT 'Loans table already exists'
GO

-- =============================================
-- 3. REPAYMENT SCHEDULES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[RepaymentSchedules] (
        [ScheduleId] [int] IDENTITY(1,1) NOT NULL,
        [LoanId] [int] NOT NULL,
        [DueDate] [datetime] NOT NULL,
        [AmountDue] [decimal](18,2) NOT NULL,
        [PrincipalPortion] [decimal](18,2) NULL,
        [InterestPortion] [decimal](18,2) NULL,
        [IsPaid] [bit] NOT NULL DEFAULT (0),
        [PaidDate] [datetime] NULL,
        CONSTRAINT [PK_RepaymentSchedules] PRIMARY KEY ([ScheduleId])
    )
    PRINT 'RepaymentSchedules table created successfully'
END
ELSE
    PRINT 'RepaymentSchedules table already exists'
GO

-- =============================================
-- 4. PAYMENTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Payments] (
        [PaymentId] [int] IDENTITY(1,1) NOT NULL,
        [LoanId] [int] NOT NULL,
        [ScheduleId] [int] NULL,
        [PaymentDate] [datetime] NOT NULL,
        [AmountPaid] [decimal](18,2) NOT NULL,
        [PaymentMethod] [nvarchar](50) NULL,
        [Notes] [nvarchar](255) NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId])
    )
    PRINT 'Payments table created successfully'
END
ELSE
    PRINT 'Payments table already exists'
GO

-- =============================================
-- 5. PENALTIES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Penalties] (
        [PenaltyId] [int] IDENTITY(1,1) NOT NULL,
        [LoanId] [int] NOT NULL,
        [ScheduleId] [int] NOT NULL,
        [Amount] [decimal](18,2) NOT NULL,
        [AppliedDate] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
        [IsPaid] [bit] NOT NULL DEFAULT (0),
        CONSTRAINT [PK_Penalties] PRIMARY KEY ([PenaltyId])
    )
    PRINT 'Penalties table created successfully'
END
ELSE
    PRINT 'Penalties table already exists'
GO

-- =============================================
-- 6. NOTIFICATIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [NotificationId] [int] IDENTITY(1,1) NOT NULL,
        [BorrowerId] [int] NOT NULL,
        [LoanId] [int] NULL,
        [Message] [nvarchar](255) NOT NULL,
        [NotificationType] [nvarchar](20) NULL,
        [SentDate] [datetime] NULL,
        [CreatedAt] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId])
    )
    PRINT 'Notifications table created successfully'
END
ELSE
    PRINT 'Notifications table already exists'
GO

-- =============================================
-- FOREIGN KEY CONSTRAINTS
-- =============================================

-- Loans -> Borrowers
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Borrowers')
BEGIN
    ALTER TABLE [dbo].[Loans] 
    ADD CONSTRAINT [FK_Loans_Borrowers] 
    FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[Borrowers] ([BorrowerId])
    PRINT 'FK_Loans_Borrowers constraint added'
END

-- RepaymentSchedules -> Loans
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RepaymentSchedules_Loans')
BEGIN
    ALTER TABLE [dbo].[RepaymentSchedules] 
    ADD CONSTRAINT [FK_RepaymentSchedules_Loans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    PRINT 'FK_RepaymentSchedules_Loans constraint added'
END

-- Payments -> Loans
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_Loans')
BEGIN
    ALTER TABLE [dbo].[Payments] 
    ADD CONSTRAINT [FK_Payments_Loans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    PRINT 'FK_Payments_Loans constraint added'
END

-- Payments -> RepaymentSchedules
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_RepaymentSchedules')
BEGIN
    ALTER TABLE [dbo].[Payments] 
    ADD CONSTRAINT [FK_Payments_RepaymentSchedules] 
    FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[RepaymentSchedules] ([ScheduleId])
    PRINT 'FK_Payments_RepaymentSchedules constraint added'
END

-- Penalties -> Loans
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_Loans')
BEGIN
    ALTER TABLE [dbo].[Penalties] 
    ADD CONSTRAINT [FK_Penalties_Loans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    PRINT 'FK_Penalties_Loans constraint added'
END

-- Penalties -> RepaymentSchedules
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_RepaymentSchedules')
BEGIN
    ALTER TABLE [dbo].[Penalties] 
    ADD CONSTRAINT [FK_Penalties_RepaymentSchedules] 
    FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[RepaymentSchedules] ([ScheduleId])
    PRINT 'FK_Penalties_RepaymentSchedules constraint added'
END

-- Notifications -> Borrowers
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Borrowers')
BEGIN
    ALTER TABLE [dbo].[Notifications] 
    ADD CONSTRAINT [FK_Notifications_Borrowers] 
    FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[Borrowers] ([BorrowerId])
    PRINT 'FK_Notifications_Borrowers constraint added'
END

-- Notifications -> Loans
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Loans')
BEGIN
    ALTER TABLE [dbo].[Notifications] 
    ADD CONSTRAINT [FK_Notifications_Loans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    PRINT 'FK_Notifications_Loans constraint added'
END

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

-- Indexes on foreign keys
CREATE NONCLUSTERED INDEX [IX_Loans_BorrowerId] ON [dbo].[Loans] ([BorrowerId])
CREATE NONCLUSTERED INDEX [IX_RepaymentSchedules_LoanId] ON [dbo].[RepaymentSchedules] ([LoanId])
CREATE NONCLUSTERED INDEX [IX_Payments_LoanId] ON [dbo].[Payments] ([LoanId])
CREATE NONCLUSTERED INDEX [IX_Payments_ScheduleId] ON [dbo].[Payments] ([ScheduleId])
CREATE NONCLUSTERED INDEX [IX_Penalties_LoanId] ON [dbo].[Penalties] ([LoanId])
CREATE NONCLUSTERED INDEX [IX_Penalties_ScheduleId] ON [dbo].[Penalties] ([ScheduleId])
CREATE NONCLUSTERED INDEX [IX_Notifications_BorrowerId] ON [dbo].[Notifications] ([BorrowerId])
CREATE NONCLUSTERED INDEX [IX_Notifications_LoanId] ON [dbo].[Notifications] ([LoanId])

-- Indexes on commonly queried fields
CREATE NONCLUSTERED INDEX [IX_Borrowers_Email] ON [dbo].[Borrowers] ([Email])
CREATE NONCLUSTERED INDEX [IX_Borrowers_Status] ON [dbo].[Borrowers] ([Status])
CREATE NONCLUSTERED INDEX [IX_Loans_Status] ON [dbo].[Loans] ([Status])
CREATE NONCLUSTERED INDEX [IX_Loans_StartDate] ON [dbo].[Loans] ([StartDate])
CREATE NONCLUSTERED INDEX [IX_RepaymentSchedules_DueDate] ON [dbo].[RepaymentSchedules] ([DueDate])
CREATE NONCLUSTERED INDEX [IX_RepaymentSchedules_IsPaid] ON [dbo].[RepaymentSchedules] ([IsPaid])

PRINT 'All indexes created successfully'
GO

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '============================================='
PRINT 'LOAN MANAGEMENT SYSTEM TABLES CREATED'
PRINT '============================================='
PRINT 'Tables created:'
PRINT '- Borrowers'
PRINT '- Loans' 
PRINT '- RepaymentSchedules'
PRINT '- Payments'
PRINT '- Penalties'
PRINT '- Notifications'
PRINT '============================================='
PRINT 'All foreign key constraints and indexes applied'
PRINT 'Database is ready for Loan Management System!'
GO
