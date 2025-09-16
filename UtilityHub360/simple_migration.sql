-- Simple Migration Script for Loan Management System
USE [DBUTILS]
GO

-- Create Borrowers table
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
    PRINT 'Borrowers table created'
END

-- Create Loans table
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
    PRINT 'Loans table created'
END

-- Create RepaymentSchedules table
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
    PRINT 'RepaymentSchedules table created'
END

-- Create Payments table
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
    PRINT 'Payments table created'
END

-- Create Penalties table
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
    PRINT 'Penalties table created'
END

-- Create Notifications table
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
    PRINT 'Notifications table created'
END

-- Add Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Borrowers')
    ALTER TABLE [dbo].[Loans] ADD CONSTRAINT [FK_Loans_Borrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[Borrowers] ([BorrowerId])

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RepaymentSchedules_Loans')
    ALTER TABLE [dbo].[RepaymentSchedules] ADD CONSTRAINT [FK_RepaymentSchedules_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_Loans')
    ALTER TABLE [dbo].[Payments] ADD CONSTRAINT [FK_Payments_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_RepaymentSchedules')
    ALTER TABLE [dbo].[Payments] ADD CONSTRAINT [FK_Payments_RepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[RepaymentSchedules] ([ScheduleId])

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_Loans')
    ALTER TABLE [dbo].[Penalties] ADD CONSTRAINT [FK_Penalties_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_RepaymentSchedules')
    ALTER TABLE [dbo].[Penalties] ADD CONSTRAINT [FK_Penalties_RepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[RepaymentSchedules] ([ScheduleId])

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Borrowers')
    ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_Notifications_Borrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[Borrowers] ([BorrowerId])

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Loans')
    ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_Notifications_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])

PRINT 'All tables and foreign keys created successfully!'
GO
