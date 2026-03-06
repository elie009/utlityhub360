-- =============================================
-- Script: Allow reusing BankAccount name/number when existing account is soft-deleted
-- Run this on your database (e.g. the one used by localhost:5000) to fix
-- "An account with the name 'X' already exists" when the existing account has IsDeleted = 1.
-- =============================================

BEGIN TRANSACTION;

-- Drop existing unique indexes (they block duplicate UserId+AccountName even for deleted rows)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BankAccounts_UserId_AccountName' AND object_id = OBJECT_ID('BankAccounts'))
    DROP INDEX [IX_BankAccounts_UserId_AccountName] ON [BankAccounts];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BankAccounts_UserId_AccountNumber' AND object_id = OBJECT_ID('BankAccounts'))
    DROP INDEX [IX_BankAccounts_UserId_AccountNumber] ON [BankAccounts];

-- Recreate with filter: uniqueness only for non-deleted rows (IsDeleted = 0)
CREATE UNIQUE NONCLUSTERED INDEX [IX_BankAccounts_UserId_AccountName]
    ON [BankAccounts] ([UserId], [AccountName])
    WHERE [IsDeleted] = 0;

CREATE UNIQUE NONCLUSTERED INDEX [IX_BankAccounts_UserId_AccountNumber]
    ON [BankAccounts] ([UserId], [AccountNumber])
    WHERE [IsDeleted] = 0 AND [AccountNumber] IS NOT NULL;

COMMIT TRANSACTION;
GO
