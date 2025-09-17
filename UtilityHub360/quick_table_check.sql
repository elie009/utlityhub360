-- Quick Table Check
USE [DBUTILS]
GO

PRINT 'Current Loan Management Tables:'
PRINT '=============================='

SELECT 
    name as 'Table Name',
    CASE 
        WHEN name LIKE 'Ln%' THEN '✓ Ln Prefixed'
        ELSE '⚠ Needs Migration'
    END as 'Status'
FROM sys.tables 
WHERE name IN ('Borrowers', 'LnBorrowers', 'Loans', 'LnLoans', 'Payments', 'LnPayments', 
               'RepaymentSchedules', 'LnRepaymentSchedules', 'Penalties', 'LnPenalties', 
               'Notifications', 'LnNotifications')
ORDER BY name

PRINT ''
PRINT 'Migration Status:'
PRINT '================'

DECLARE @LnBorrowersExists BIT = 0
DECLARE @LnLoansExists BIT = 0
DECLARE @LnPaymentsExists BIT = 0
DECLARE @LnRepaymentSchedulesExists BIT = 0
DECLARE @LnPenaltiesExists BIT = 0
DECLARE @LnNotificationsExists BIT = 0

IF EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U') SET @LnBorrowersExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U') SET @LnLoansExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U') SET @LnPaymentsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U') SET @LnRepaymentSchedulesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U') SET @LnPenaltiesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U') SET @LnNotificationsExists = 1

PRINT 'Borrowers: ' + CASE WHEN @LnBorrowersExists = 1 THEN '✓ MIGRATED' ELSE '✗ NOT MIGRATED' END
PRINT 'Loans: ' + CASE WHEN @LnLoansExists = 1 THEN '✓ MIGRATED' ELSE '✗ NOT MIGRATED' END
PRINT 'Payments: ' + CASE WHEN @LnPaymentsExists = 1 THEN '✓ MIGRATED' ELSE '✗ NOT MIGRATED' END
PRINT 'RepaymentSchedules: ' + CASE WHEN @LnRepaymentSchedulesExists = 1 THEN '✓ MIGRATED' ELSE '✗ NOT MIGRATED' END
PRINT 'Penalties: ' + CASE WHEN @LnPenaltiesExists = 1 THEN '✓ MIGRATED' ELSE '✗ NOT MIGRATED' END
PRINT 'Notifications: ' + CASE WHEN @LnNotificationsExists = 1 THEN '✓ MIGRATED' ELSE '✗ NOT MIGRATED' END

GO
