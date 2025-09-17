-- Create Loan Management System Tables with Ln prefix
-- This script creates all loan-related tables with the Ln prefix

-- Create LnBorrowers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LnBorrowers')
BEGIN
    CREATE TABLE LnBorrowers (
        BorrowerId int IDENTITY(1,1) PRIMARY KEY,
        FirstName nvarchar(100) NOT NULL,
        LastName nvarchar(100) NOT NULL,
        Email nvarchar(200),
        Phone nvarchar(20),
        Address nvarchar(255),
        GovernmentId nvarchar(50),
        CreditScore int,
        Status nvarchar(20) DEFAULT 'Active',
        CreatedAt datetime2 DEFAULT GETUTCDATE()
    );
    
    -- Create indexes
    CREATE INDEX IX_LnBorrowers_Email ON LnBorrowers(Email);
    CREATE INDEX IX_LnBorrowers_Status ON LnBorrowers(Status);
    CREATE INDEX IX_LnBorrowers_GovernmentId ON LnBorrowers(GovernmentId);
END

-- Create LnLoans table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LnLoans')
BEGIN
    CREATE TABLE LnLoans (
        LoanId int IDENTITY(1,1) PRIMARY KEY,
        BorrowerId int NOT NULL,
        LoanType nvarchar(50),
        PrincipalAmount decimal(18,2) NOT NULL,
        InterestRate decimal(5,2) NOT NULL,
        TermMonths int NOT NULL,
        RepaymentFrequency nvarchar(20),
        AmortizationType nvarchar(20),
        StartDate datetime2 NOT NULL,
        EndDate datetime2,
        Status nvarchar(20) DEFAULT 'Active',
        CreatedAt datetime2 DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_LnLoans_BorrowerId FOREIGN KEY (BorrowerId) REFERENCES LnBorrowers(BorrowerId)
    );
    
    -- Create indexes
    CREATE INDEX IX_LnLoans_BorrowerId ON LnLoans(BorrowerId);
    CREATE INDEX IX_LnLoans_Status ON LnLoans(Status);
    CREATE INDEX IX_LnLoans_LoanType ON LnLoans(LoanType);
    CREATE INDEX IX_LnLoans_StartDate ON LnLoans(StartDate);
END

-- Create LnRepaymentSchedules table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LnRepaymentSchedules')
BEGIN
    CREATE TABLE LnRepaymentSchedules (
        ScheduleId int IDENTITY(1,1) PRIMARY KEY,
        LoanId int NOT NULL,
        DueDate datetime2 NOT NULL,
        AmountDue decimal(18,2) NOT NULL,
        PrincipalPortion decimal(18,2),
        InterestPortion decimal(18,2),
        IsPaid bit DEFAULT 0,
        PaidDate datetime2,
        
        CONSTRAINT FK_LnRepaymentSchedules_LoanId FOREIGN KEY (LoanId) REFERENCES LnLoans(LoanId)
    );
    
    -- Create indexes
    CREATE INDEX IX_LnRepaymentSchedules_LoanId ON LnRepaymentSchedules(LoanId);
    CREATE INDEX IX_LnRepaymentSchedules_DueDate ON LnRepaymentSchedules(DueDate);
    CREATE INDEX IX_LnRepaymentSchedules_IsPaid ON LnRepaymentSchedules(IsPaid);
END

-- Create LnPayments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LnPayments')
BEGIN
    CREATE TABLE LnPayments (
        PaymentId int IDENTITY(1,1) PRIMARY KEY,
        LoanId int NOT NULL,
        ScheduleId int,
        PaymentDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
        AmountPaid decimal(18,2) NOT NULL,
        PaymentMethod nvarchar(50),
        Notes nvarchar(255),
        
        CONSTRAINT FK_LnPayments_LoanId FOREIGN KEY (LoanId) REFERENCES LnLoans(LoanId),
        CONSTRAINT FK_LnPayments_ScheduleId FOREIGN KEY (ScheduleId) REFERENCES LnRepaymentSchedules(ScheduleId)
    );
    
    -- Create indexes
    CREATE INDEX IX_LnPayments_LoanId ON LnPayments(LoanId);
    CREATE INDEX IX_LnPayments_ScheduleId ON LnPayments(ScheduleId);
    CREATE INDEX IX_LnPayments_PaymentDate ON LnPayments(PaymentDate);
END

-- Create LnPenalties table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LnPenalties')
BEGIN
    CREATE TABLE LnPenalties (
        PenaltyId int IDENTITY(1,1) PRIMARY KEY,
        LoanId int NOT NULL,
        ScheduleId int NOT NULL,
        Amount decimal(18,2) NOT NULL,
        AppliedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
        IsPaid bit DEFAULT 0,
        
        CONSTRAINT FK_LnPenalties_LoanId FOREIGN KEY (LoanId) REFERENCES LnLoans(LoanId),
        CONSTRAINT FK_LnPenalties_ScheduleId FOREIGN KEY (ScheduleId) REFERENCES LnRepaymentSchedules(ScheduleId)
    );
    
    -- Create indexes
    CREATE INDEX IX_LnPenalties_LoanId ON LnPenalties(LoanId);
    CREATE INDEX IX_LnPenalties_ScheduleId ON LnPenalties(ScheduleId);
    CREATE INDEX IX_LnPenalties_IsPaid ON LnPenalties(IsPaid);
END

-- Create LnNotifications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LnNotifications')
BEGIN
    CREATE TABLE LnNotifications (
        NotificationId int IDENTITY(1,1) PRIMARY KEY,
        BorrowerId int NOT NULL,
        LoanId int,
        Message nvarchar(255) NOT NULL,
        NotificationType nvarchar(20),
        SentDate datetime2,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_LnNotifications_BorrowerId FOREIGN KEY (BorrowerId) REFERENCES LnBorrowers(BorrowerId),
        CONSTRAINT FK_LnNotifications_LoanId FOREIGN KEY (LoanId) REFERENCES LnLoans(LoanId)
    );
    
    -- Create indexes
    CREATE INDEX IX_LnNotifications_BorrowerId ON LnNotifications(BorrowerId);
    CREATE INDEX IX_LnNotifications_LoanId ON LnNotifications(LoanId);
    CREATE INDEX IX_LnNotifications_NotificationType ON LnNotifications(NotificationType);
    CREATE INDEX IX_LnNotifications_CreatedAt ON LnNotifications(CreatedAt);
END

PRINT 'Loan Management System tables created successfully with Ln prefix!';
PRINT 'Tables created: LnBorrowers, LnLoans, LnRepaymentSchedules, LnPayments, LnPenalties, LnNotifications';

