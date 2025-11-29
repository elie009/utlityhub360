-- =====================================================
-- Add Soft Delete Columns to BankAccounts Table
-- =====================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

BEGIN TRANSACTION;

-- Add soft delete columns to BankAccounts table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [BankAccounts] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [BankAccounts] ADD [DeletedAt] datetime2 NULL;
    ALTER TABLE [BankAccounts] ADD [DeletedBy] nvarchar(450) NULL;
    ALTER TABLE [BankAccounts] ADD [DeleteReason] nvarchar(500) NULL;
    PRINT 'Added soft delete columns to BankAccounts table';
END
ELSE
BEGIN
    PRINT 'Soft delete columns already exist in BankAccounts table';
END

COMMIT TRANSACTION;
GO

