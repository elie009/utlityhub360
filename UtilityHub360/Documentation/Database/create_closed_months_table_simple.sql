-- Create ClosedMonths table for month closing functionality
-- STEP 1: Check which table names exist
SELECT 'BankAccount tables:' AS Info
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%BankAccount%' 
  AND TABLE_TYPE = 'BASE TABLE'

SELECT 'User tables:' AS Info
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%User%' 
  AND TABLE_TYPE = 'BASE TABLE'
GO

-- STEP 2: Create ClosedMonths table WITHOUT foreign keys first
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClosedMonths')
BEGIN
    CREATE TABLE [dbo].[ClosedMonths] (
        [Id] NVARCHAR(450) NOT NULL,
        [BankAccountId] NVARCHAR(450) NOT NULL,
        [Year] INT NOT NULL,
        [Month] INT NOT NULL,
        [ClosedBy] NVARCHAR(450) NOT NULL,
        [ClosedAt] DATETIME2 NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_ClosedMonths] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_ClosedMonths_Month_Range] CHECK ([Month] >= 1 AND [Month] <= 12),
        CONSTRAINT [CK_ClosedMonths_Year_Range] CHECK ([Year] >= 2000 AND [Year] <= 2100)
    );

    PRINT 'ClosedMonths table created successfully (without foreign keys)';
END
ELSE
BEGIN
    PRINT 'ClosedMonths table already exists';
END
GO

-- STEP 3: Create indexes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClosedMonths')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_BankAccountId_Year_Month' AND object_id = OBJECT_ID('ClosedMonths'))
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [IX_ClosedMonths_BankAccountId_Year_Month] 
            ON [dbo].[ClosedMonths] ([BankAccountId], [Year], [Month]);
        PRINT 'Index IX_ClosedMonths_BankAccountId_Year_Month created';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_BankAccountId' AND object_id = OBJECT_ID('ClosedMonths'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_ClosedMonths_BankAccountId] 
            ON [dbo].[ClosedMonths] ([BankAccountId]);
        PRINT 'Index IX_ClosedMonths_BankAccountId created';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_Year_Month' AND object_id = OBJECT_ID('ClosedMonths'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_ClosedMonths_Year_Month] 
            ON [dbo].[ClosedMonths] ([Year], [Month]);
        PRINT 'Index IX_ClosedMonths_Year_Month created';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClosedMonths_ClosedAt' AND object_id = OBJECT_ID('ClosedMonths'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_ClosedMonths_ClosedAt] 
            ON [dbo].[ClosedMonths] ([ClosedAt]);
        PRINT 'Index IX_ClosedMonths_ClosedAt created';
    END
END
GO

-- STEP 4: Add foreign keys if the referenced tables exist
-- Uncomment and modify the table names based on what you found in STEP 1

/*
-- Replace 'BankAccounts' with the actual table name you found
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BankAccounts')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ClosedMonths_BankAccounts_BankAccountId')
BEGIN
    ALTER TABLE [dbo].[ClosedMonths]
    ADD CONSTRAINT [FK_ClosedMonths_BankAccounts_BankAccountId] 
        FOREIGN KEY ([BankAccountId]) 
        REFERENCES [dbo].[BankAccounts] ([Id]) 
        ON DELETE CASCADE;
    PRINT 'Foreign key FK_ClosedMonths_BankAccounts_BankAccountId added';
END
GO

-- Replace 'Users' with the actual table name you found
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ClosedMonths_Users_ClosedBy')
BEGIN
    ALTER TABLE [dbo].[ClosedMonths]
    ADD CONSTRAINT [FK_ClosedMonths_Users_ClosedBy] 
        FOREIGN KEY ([ClosedBy]) 
        REFERENCES [dbo].[Users] ([Id]) 
        ON DELETE NO ACTION;
    PRINT 'Foreign key FK_ClosedMonths_Users_ClosedBy added';
END
GO
*/

PRINT 'Script completed. Check the output above for table names, then uncomment and run STEP 4 with the correct table names.';

