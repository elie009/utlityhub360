-- SQL Script to create VariableExpenses table
-- Run this if the migration doesn't apply automatically

-- Create VariableExpenses table
CREATE TABLE [dbo].[VariableExpenses] (
    [Id] NVARCHAR(450) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [Description] NVARCHAR(200) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Category] NVARCHAR(50) NOT NULL,
    [Currency] NVARCHAR(10) NOT NULL,
    [ExpenseDate] DATETIME2 NOT NULL,
    [Notes] NVARCHAR(500) NULL,
    [Merchant] NVARCHAR(200) NULL,
    [PaymentMethod] NVARCHAR(50) NULL,
    [IsRecurring] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    CONSTRAINT [PK_VariableExpenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VariableExpenses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX [IX_VariableExpenses_UserId] ON [dbo].[VariableExpenses] ([UserId]);
CREATE INDEX [IX_VariableExpenses_Category] ON [dbo].[VariableExpenses] ([Category]);
CREATE INDEX [IX_VariableExpenses_ExpenseDate] ON [dbo].[VariableExpenses] ([ExpenseDate]);
CREATE INDEX [IX_VariableExpenses_UserId_ExpenseDate_Category] ON [dbo].[VariableExpenses] ([UserId], [ExpenseDate], [Category]);

GO

-- Insert migration record (update the timestamp as needed)
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251011192439_AddVariableExpensesTable', N'8.0.0');

GO

PRINT 'VariableExpenses table created successfully!';

