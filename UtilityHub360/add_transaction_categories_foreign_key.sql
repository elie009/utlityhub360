-- Add Foreign Key to TransactionCategories table
-- Run this script after the Users table has been created
-- This script is safe to run multiple times (it checks if the foreign key already exists)

PRINT 'Checking for TransactionCategories table...';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransactionCategories]') AND type in (N'U'))
BEGIN
    PRINT 'TransactionCategories table found.';
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        PRINT 'Users table found.';
        
        -- Check if foreign key already exists
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TransactionCategories_Users_UserId')
        BEGIN
            PRINT 'Adding foreign key FK_TransactionCategories_Users_UserId...';
            
            ALTER TABLE [dbo].[TransactionCategories]
                ADD CONSTRAINT [FK_TransactionCategories_Users_UserId] 
                FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
            
            PRINT 'Foreign key FK_TransactionCategories_Users_UserId added successfully!';
        END
        ELSE
        BEGIN
            PRINT 'Foreign key FK_TransactionCategories_Users_UserId already exists.';
        END
    END
    ELSE
    BEGIN
        PRINT 'ERROR: Users table not found. Please create the Users table first.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: TransactionCategories table not found. Please run create_transaction_categories_table.sql first.';
END
GO

