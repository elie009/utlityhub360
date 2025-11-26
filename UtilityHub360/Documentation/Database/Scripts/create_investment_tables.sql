-- Migration: Create Investment Tracking Tables
-- Run this script to add comprehensive investment tracking support

-- Create Investments table
CREATE TABLE Investments (
    Id NVARCHAR(450) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    AccountName NVARCHAR(100) NOT NULL,
    InvestmentType NVARCHAR(50) NOT NULL, -- STOCK, BOND, MUTUAL_FUND, ETF, CRYPTO, REAL_ESTATE, OTHER
    AccountType NVARCHAR(50) NULL, -- BROKERAGE, RETIREMENT_401K, RETIREMENT_IRA, TAXABLE, etc.
    BrokerName NVARCHAR(100) NULL,
    AccountNumber NVARCHAR(100) NULL,
    InitialInvestment DECIMAL(18,2) NOT NULL DEFAULT 0,
    CurrentValue DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCostBasis DECIMAL(18,2) NOT NULL DEFAULT 0,
    UnrealizedGainLoss DECIMAL(18,2) NULL,
    RealizedGainLoss DECIMAL(18,2) NULL,
    TotalReturnPercentage DECIMAL(5,2) NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'USD',
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(450) NULL,
    DeleteReason NVARCHAR(500) NULL,
    CONSTRAINT FK_Investments_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create InvestmentPositions table
CREATE TABLE InvestmentPositions (
    Id NVARCHAR(450) PRIMARY KEY,
    InvestmentId NVARCHAR(450) NOT NULL,
    Symbol NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    AssetType NVARCHAR(50) NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    AverageCostBasis DECIMAL(18,2) NOT NULL,
    TotalCostBasis DECIMAL(18,2) NOT NULL,
    CurrentPrice DECIMAL(18,2) NULL,
    CurrentValue DECIMAL(18,2) NULL,
    UnrealizedGainLoss DECIMAL(18,2) NULL,
    GainLossPercentage DECIMAL(5,2) NULL,
    DividendsReceived DECIMAL(18,2) NULL,
    InterestReceived DECIMAL(18,2) NULL,
    LastPriceUpdate DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_InvestmentPositions_Investments FOREIGN KEY (InvestmentId) REFERENCES Investments(Id) ON DELETE CASCADE
);

-- Create InvestmentTransactions table
CREATE TABLE InvestmentTransactions (
    Id NVARCHAR(450) PRIMARY KEY,
    InvestmentId NVARCHAR(450) NOT NULL,
    PositionId NVARCHAR(450) NULL,
    TransactionType NVARCHAR(50) NOT NULL, -- BUY, SELL, DIVIDEND, INTEREST, DEPOSIT, WITHDRAWAL, FEE, SPLIT, MERGER
    Symbol NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NULL,
    Quantity DECIMAL(18,4) NULL,
    PricePerShare DECIMAL(18,2) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Fees DECIMAL(18,2) NULL,
    Taxes DECIMAL(18,2) NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'USD',
    Description NVARCHAR(500) NULL,
    Reference NVARCHAR(100) NULL,
    TransactionDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(450) NULL,
    DeleteReason NVARCHAR(500) NULL,
    CONSTRAINT FK_InvestmentTransactions_Investments FOREIGN KEY (InvestmentId) REFERENCES Investments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InvestmentTransactions_Positions FOREIGN KEY (PositionId) REFERENCES InvestmentPositions(Id) ON DELETE SET NULL
);

-- Create indexes for performance
CREATE INDEX IX_Investments_UserId ON Investments(UserId);
CREATE INDEX IX_Investments_IsActive ON Investments(IsActive) WHERE IsActive = 1;
CREATE INDEX IX_InvestmentPositions_InvestmentId ON InvestmentPositions(InvestmentId);
CREATE INDEX IX_InvestmentPositions_Symbol ON InvestmentPositions(Symbol);
CREATE INDEX IX_InvestmentTransactions_InvestmentId ON InvestmentTransactions(InvestmentId);
CREATE INDEX IX_InvestmentTransactions_TransactionDate ON InvestmentTransactions(TransactionDate);
CREATE INDEX IX_InvestmentTransactions_TransactionType ON InvestmentTransactions(TransactionType);

PRINT 'Investment tracking tables created successfully.';

