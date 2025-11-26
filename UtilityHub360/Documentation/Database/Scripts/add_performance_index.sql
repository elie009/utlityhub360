-- =============================================
-- Performance Optimization: Add Composite Index
-- =============================================
-- This index improves query performance for GetBankStatementsAsync
-- which filters by BankAccountId AND UserId

-- Check if index already exists before creating
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_BankStatements_BankAccountId_UserId' 
    AND object_id = OBJECT_ID('dbo.BankStatements')
)
BEGIN
    CREATE INDEX [IX_BankStatements_BankAccountId_UserId] 
    ON [dbo].[BankStatements] ([BankAccountId], [UserId]);
    
    PRINT 'Composite index IX_BankStatements_BankAccountId_UserId created successfully!';
END
ELSE
BEGIN
    PRINT 'Index IX_BankStatements_BankAccountId_UserId already exists.';
END
GO

PRINT 'Performance optimization complete!';
GO

