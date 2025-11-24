-- ============================================
-- ALLOCATION PLANNER TABLES
-- ============================================
-- This script creates all tables needed for the Allocation Planner feature
-- Run this script to add allocation planning capabilities to the system
--
-- IMPORTANT: This is a SQL script, NOT the C# file (Allocation.cs)
-- Run this file in SQL Server Management Studio (SSMS) or your SQL tool
-- ============================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Check if Users table exists (informational only - foreign keys will be skipped if not found)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
BEGIN
    DECLARE @UsersSchemaInfo NVARCHAR(128);
    SELECT @UsersSchemaInfo = SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = 'Users' AND type = 'U';
    PRINT 'Found Users table in schema: ' + ISNULL(@UsersSchemaInfo, 'dbo');
END
ELSE
BEGIN
    PRINT 'WARNING: Users table not found. Foreign keys to Users will be skipped.';
    PRINT 'You can add them later using ALTER TABLE statements.';
END
GO

-- Note: We use SET XACT_ABORT OFF and explicit commits to prevent transaction rollback
-- This allows the script to continue even if some foreign keys fail
SET XACT_ABORT OFF;
GO

-- Ensure we're not in a transaction
IF @@TRANCOUNT > 0
BEGIN
    COMMIT TRANSACTION;
END
GO

-- 1. AllocationTemplate - Stores allocation templates (50/30/20 rule, etc.)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[AllocationTemplates] (
        Id NVARCHAR(450) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsSystemTemplate BIT NOT NULL DEFAULT 1,
        UserId NVARCHAR(450) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_AllocationTemplates_UserId ON [dbo].[AllocationTemplates](UserId);
    CREATE INDEX IX_AllocationTemplates_IsSystemTemplate ON [dbo].[AllocationTemplates](IsSystemTemplate);
    CREATE INDEX IX_AllocationTemplates_IsActive ON [dbo].[AllocationTemplates](IsActive);
    
    PRINT 'Created AllocationTemplates table';
END
ELSE
BEGIN
    PRINT 'AllocationTemplates table already exists';
END

-- Commit after table creation
IF @@TRANCOUNT > 0 COMMIT;
GO

-- 2. AllocationTemplateCategories - Categories within templates
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplateCategories' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[AllocationTemplateCategories] (
        Id NVARCHAR(450) PRIMARY KEY,
        TemplateId NVARCHAR(450) NOT NULL,
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Percentage DECIMAL(5,2) NOT NULL,
        DisplayOrder INT NOT NULL,
        Color NVARCHAR(50) NULL
    );

    CREATE INDEX IX_AllocationTemplateCategories_TemplateId ON [dbo].[AllocationTemplateCategories](TemplateId);
    
    PRINT 'Created AllocationTemplateCategories table';
END
ELSE
BEGIN
    PRINT 'AllocationTemplateCategories table already exists';
END

-- Commit after table creation
IF @@TRANCOUNT > 0 COMMIT;
GO

-- 3. AllocationPlans - User's active allocation plans
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationPlans' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[AllocationPlans] (
        Id NVARCHAR(450) PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        TemplateId NVARCHAR(450) NULL,
        PlanName NVARCHAR(100) NOT NULL,
        MonthlyIncome DECIMAL(18,2) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        StartDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        EndDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_AllocationPlans_UserId ON [dbo].[AllocationPlans](UserId);
    CREATE INDEX IX_AllocationPlans_IsActive ON [dbo].[AllocationPlans](IsActive);
    CREATE INDEX IX_AllocationPlans_UserId_IsActive ON [dbo].[AllocationPlans](UserId, IsActive);
    
    PRINT 'Created AllocationPlans table';
END
ELSE
BEGIN
    PRINT 'AllocationPlans table already exists';
END

-- Commit after table creation
IF @@TRANCOUNT > 0 COMMIT;
GO

-- 4. AllocationCategories - Categories within user's plans
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationCategories' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[AllocationCategories] (
        Id NVARCHAR(450) PRIMARY KEY,
        PlanId NVARCHAR(450) NOT NULL,
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        AllocatedAmount DECIMAL(18,2) NOT NULL,
        Percentage DECIMAL(5,2) NOT NULL,
        DisplayOrder INT NOT NULL,
        Color NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_AllocationCategories_PlanId ON [dbo].[AllocationCategories](PlanId);
    
    PRINT 'Created AllocationCategories table';
END
ELSE
BEGIN
    PRINT 'AllocationCategories table already exists';
END

-- Commit after table creation
IF @@TRANCOUNT > 0 COMMIT;
GO

-- 5. AllocationHistories - Historical tracking of allocation performance
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationHistories' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[AllocationHistories] (
        Id NVARCHAR(450) PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        PlanId NVARCHAR(450) NOT NULL,
        CategoryId NVARCHAR(450) NULL,
        PeriodDate DATETIME2 NOT NULL,
        AllocatedAmount DECIMAL(18,2) NOT NULL,
        ActualAmount DECIMAL(18,2) NOT NULL,
        Variance DECIMAL(18,2) NOT NULL,
        VariancePercentage DECIMAL(5,2) NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_AllocationHistories_UserId ON [dbo].[AllocationHistories](UserId);
    CREATE INDEX IX_AllocationHistories_PlanId ON [dbo].[AllocationHistories](PlanId);
    CREATE INDEX IX_AllocationHistories_CategoryId ON [dbo].[AllocationHistories](CategoryId);
    CREATE INDEX IX_AllocationHistories_PeriodDate ON [dbo].[AllocationHistories](PeriodDate);
    -- Note: Composite index removed due to size limit (NVARCHAR(450) x 3 = 2700 bytes, exceeds 1700 byte limit)
    -- Use individual indexes instead for better performance
    
    PRINT 'Created AllocationHistories table';
END
ELSE
BEGIN
    PRINT 'AllocationHistories table already exists';
END

-- Commit after table creation
IF @@TRANCOUNT > 0 COMMIT;
GO

-- 6. AllocationRecommendations - Recommendations for allocation adjustments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationRecommendations' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[AllocationRecommendations] (
        Id NVARCHAR(450) PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        PlanId NVARCHAR(450) NOT NULL,
        RecommendationType NVARCHAR(50) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(2000) NOT NULL,
        CategoryId NVARCHAR(450) NULL,
        SuggestedAmount DECIMAL(18,2) NULL,
        SuggestedPercentage DECIMAL(5,2) NULL,
        Priority NVARCHAR(20) NOT NULL DEFAULT 'medium',
        IsRead BIT NOT NULL DEFAULT 0,
        IsApplied BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        AppliedAt DATETIME2 NULL
    );

    CREATE INDEX IX_AllocationRecommendations_UserId ON [dbo].[AllocationRecommendations](UserId);
    CREATE INDEX IX_AllocationRecommendations_PlanId ON [dbo].[AllocationRecommendations](PlanId);
    CREATE INDEX IX_AllocationRecommendations_CategoryId ON [dbo].[AllocationRecommendations](CategoryId);
    CREATE INDEX IX_AllocationRecommendations_IsApplied ON [dbo].[AllocationRecommendations](IsApplied);
    CREATE INDEX IX_AllocationRecommendations_IsRead ON [dbo].[AllocationRecommendations](IsRead);
    CREATE INDEX IX_AllocationRecommendations_Priority ON [dbo].[AllocationRecommendations](Priority);
    
    PRINT 'Created AllocationRecommendations table';
END
ELSE
BEGIN
    PRINT 'AllocationRecommendations table already exists';
END

-- Commit after table creation
IF @@TRANCOUNT > 0 COMMIT;
GO

-- ============================================
-- ADD FOREIGN KEY CONSTRAINTS
-- ============================================
-- Add foreign keys after all tables are created
-- Note: Foreign keys to Users are optional - they will be skipped if Users table doesn't exist

-- Check if Users table exists and get its schema
DECLARE @UsersSchema NVARCHAR(128) = NULL;
SELECT @UsersSchema = SCHEMA_NAME(schema_id)
FROM sys.tables 
WHERE name = 'Users' AND type = 'U';

IF @UsersSchema IS NULL
BEGIN
    PRINT 'WARNING: Users table not found. Skipping all foreign keys to Users table.';
    PRINT 'You can add them later manually if needed.';
    SET @UsersSchema = 'dbo'; -- Default for dynamic SQL
END
ELSE
BEGIN
    PRINT 'Found Users table in schema: ' + @UsersSchema;
END
GO

-- Foreign key: AllocationTemplates -> Users
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationTemplates_Users')
    BEGIN
        DECLARE @UsersSchema1 NVARCHAR(128);
        SELECT @UsersSchema1 = SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = 'Users' AND type = 'U';
        
        DECLARE @FK_SQL1 NVARCHAR(MAX) = 'ALTER TABLE [dbo].[AllocationTemplates] ADD CONSTRAINT FK_AllocationTemplates_Users FOREIGN KEY (UserId) REFERENCES [' + @UsersSchema1 + '].[Users](Id) ON DELETE CASCADE';
        EXEC sp_executesql @FK_SQL1;
        PRINT 'Added FK_AllocationTemplates_Users';
    END
    ELSE IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
    BEGIN
        PRINT 'Skipped FK_AllocationTemplates_Users - Users table not found';
    END
    ELSE IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationTemplates_Users')
    BEGIN
        PRINT 'FK_AllocationTemplates_Users already exists';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationTemplates_Users: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationTemplateCategories -> AllocationTemplates
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplateCategories' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationTemplateCategories_Templates')
    BEGIN
        ALTER TABLE [dbo].[AllocationTemplateCategories]
        ADD CONSTRAINT FK_AllocationTemplateCategories_Templates 
        FOREIGN KEY (TemplateId) REFERENCES [dbo].[AllocationTemplates](Id) ON DELETE CASCADE;
        PRINT 'Added FK_AllocationTemplateCategories_Templates';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationTemplateCategories_Templates: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationPlans -> Users
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationPlans' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationPlans_Users')
    BEGIN
        DECLARE @UsersSchema2 NVARCHAR(128);
        SELECT @UsersSchema2 = SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = 'Users' AND type = 'U';
        
        DECLARE @FK_SQL2 NVARCHAR(MAX) = 'ALTER TABLE [dbo].[AllocationPlans] ADD CONSTRAINT FK_AllocationPlans_Users FOREIGN KEY (UserId) REFERENCES [' + @UsersSchema2 + '].[Users](Id) ON DELETE CASCADE';
        EXEC sp_executesql @FK_SQL2;
        PRINT 'Added FK_AllocationPlans_Users';
    END
    ELSE IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
    BEGIN
        PRINT 'Skipped FK_AllocationPlans_Users - Users table not found';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationPlans_Users: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationPlans -> AllocationTemplates
-- Note: This FK is optional - TemplateId can be NULL, so we skip it if there are issues
BEGIN TRY
    -- Ensure we're not in a transaction before attempting FK creation
    IF @@TRANCOUNT > 0 COMMIT;
    
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationPlans' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationPlans_Templates')
    BEGIN
        -- Check if there are any invalid TemplateId values
        DECLARE @InvalidCount INT = 0;
        SELECT @InvalidCount = COUNT(*) 
        FROM [dbo].[AllocationPlans] 
        WHERE TemplateId IS NOT NULL 
          AND TemplateId NOT IN (SELECT Id FROM [dbo].[AllocationTemplates]);
        
        IF @InvalidCount = 0
        BEGIN
            ALTER TABLE [dbo].[AllocationPlans]
            ADD CONSTRAINT FK_AllocationPlans_Templates 
            FOREIGN KEY (TemplateId) REFERENCES [dbo].[AllocationTemplates](Id) ON DELETE SET NULL;
            PRINT 'Added FK_AllocationPlans_Templates';
        END
        ELSE
        BEGIN
            PRINT 'Skipped FK_AllocationPlans_Templates - Found ' + CAST(@InvalidCount AS VARCHAR(10)) + ' invalid TemplateId values';
            PRINT 'You can clean up the data and add this foreign key manually later.';
        END
    END
    ELSE IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
    BEGIN
        PRINT 'Skipped FK_AllocationPlans_Templates - AllocationTemplates table not found in dbo schema';
    END
    ELSE IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationPlans_Templates')
    BEGIN
        PRINT 'FK_AllocationPlans_Templates already exists';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationPlans_Templates: ' + ERROR_MESSAGE();
    -- Rollback any partial transaction and continue
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

-- Commit any pending transaction
IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationCategories -> AllocationPlans
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationPlans' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationCategories' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationCategories_Plans')
    BEGIN
        ALTER TABLE [dbo].[AllocationCategories]
        ADD CONSTRAINT FK_AllocationCategories_Plans 
        FOREIGN KEY (PlanId) REFERENCES [dbo].[AllocationPlans](Id) ON DELETE CASCADE;
        PRINT 'Added FK_AllocationCategories_Plans';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationCategories_Plans: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationHistories -> Users
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationHistories' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationHistories_Users')
    BEGIN
        DECLARE @UsersSchema3 NVARCHAR(128);
        SELECT @UsersSchema3 = SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = 'Users' AND type = 'U';
        
        DECLARE @FK_SQL3 NVARCHAR(MAX) = 'ALTER TABLE [dbo].[AllocationHistories] ADD CONSTRAINT FK_AllocationHistories_Users FOREIGN KEY (UserId) REFERENCES [' + @UsersSchema3 + '].[Users](Id) ON DELETE CASCADE';
        EXEC sp_executesql @FK_SQL3;
        PRINT 'Added FK_AllocationHistories_Users';
    END
    ELSE IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
    BEGIN
        PRINT 'Skipped FK_AllocationHistories_Users - Users table not found';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationHistories_Users: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationHistories -> AllocationPlans
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationPlans' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationHistories' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationHistories_Plans')
    BEGIN
        ALTER TABLE [dbo].[AllocationHistories]
        ADD CONSTRAINT FK_AllocationHistories_Plans 
        FOREIGN KEY (PlanId) REFERENCES [dbo].[AllocationPlans](Id) ON DELETE CASCADE;
        PRINT 'Added FK_AllocationHistories_Plans';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationHistories_Plans: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationHistories -> AllocationCategories
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationCategories' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationHistories' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationHistories_Categories')
    BEGIN
        ALTER TABLE [dbo].[AllocationHistories]
        ADD CONSTRAINT FK_AllocationHistories_Categories 
        FOREIGN KEY (CategoryId) REFERENCES [dbo].[AllocationCategories](Id) ON DELETE SET NULL;
        PRINT 'Added FK_AllocationHistories_Categories';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationHistories_Categories: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationRecommendations -> Users
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationRecommendations' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationRecommendations_Users')
    BEGIN
        DECLARE @UsersSchema4 NVARCHAR(128);
        SELECT @UsersSchema4 = SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = 'Users' AND type = 'U';
        
        DECLARE @FK_SQL4 NVARCHAR(MAX) = 'ALTER TABLE [dbo].[AllocationRecommendations] ADD CONSTRAINT FK_AllocationRecommendations_Users FOREIGN KEY (UserId) REFERENCES [' + @UsersSchema4 + '].[Users](Id) ON DELETE CASCADE';
        EXEC sp_executesql @FK_SQL4;
        PRINT 'Added FK_AllocationRecommendations_Users';
    END
    ELSE IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
    BEGIN
        PRINT 'Skipped FK_AllocationRecommendations_Users - Users table not found';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationRecommendations_Users: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationRecommendations -> AllocationPlans
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationPlans' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationRecommendations' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationRecommendations_Plans')
    BEGIN
        ALTER TABLE [dbo].[AllocationRecommendations]
        ADD CONSTRAINT FK_AllocationRecommendations_Plans 
        FOREIGN KEY (PlanId) REFERENCES [dbo].[AllocationPlans](Id) ON DELETE CASCADE;
        PRINT 'Added FK_AllocationRecommendations_Plans';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationRecommendations_Plans: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Foreign key: AllocationRecommendations -> AllocationCategories
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationCategories' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationRecommendations' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AllocationRecommendations_Categories')
    BEGIN
        ALTER TABLE [dbo].[AllocationRecommendations]
        ADD CONSTRAINT FK_AllocationRecommendations_Categories 
        FOREIGN KEY (CategoryId) REFERENCES [dbo].[AllocationCategories](Id) ON DELETE SET NULL;
        PRINT 'Added FK_AllocationRecommendations_Categories';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding FK_AllocationRecommendations_Categories: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- ============================================
-- SEED DATA: System Templates
-- ============================================

-- 50/30/20 Rule Template
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM [dbo].[AllocationTemplates] WHERE Name = '50/30/20 Rule')
    BEGIN
        DECLARE @TemplateId50_30_20 NVARCHAR(450) = NEWID();
        
        INSERT INTO [dbo].[AllocationTemplates] (Id, Name, Description, IsSystemTemplate, IsActive, CreatedAt, UpdatedAt)
        VALUES (@TemplateId50_30_20, '50/30/20 Rule', 'Allocate 50% to Needs, 30% to Wants, and 20% to Savings', 1, 1, GETUTCDATE(), GETUTCDATE());
        
        INSERT INTO [dbo].[AllocationTemplateCategories] (Id, TemplateId, CategoryName, Description, Percentage, DisplayOrder, Color)
        VALUES 
            (NEWID(), @TemplateId50_30_20, 'Needs', 'Essential expenses: housing, utilities, food, transportation, insurance', 50.00, 1, '#F44336'),
            (NEWID(), @TemplateId50_30_20, 'Wants', 'Discretionary spending: entertainment, dining out, hobbies', 30.00, 2, '#FF9800'),
            (NEWID(), @TemplateId50_30_20, 'Savings', 'Savings, investments, debt repayment, emergency fund', 20.00, 3, '#4CAF50');
        
        PRINT 'Seeded 50/30/20 Rule template';
    END
END TRY
BEGIN CATCH
    PRINT 'Error seeding 50/30/20 Rule template: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Zero-Based Budget Template
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM [dbo].[AllocationTemplates] WHERE Name = 'Zero-Based Budget')
    BEGIN
        DECLARE @TemplateIdZeroBased NVARCHAR(450) = NEWID();
        
        INSERT INTO [dbo].[AllocationTemplates] (Id, Name, Description, IsSystemTemplate, IsActive, CreatedAt, UpdatedAt)
        VALUES (@TemplateIdZeroBased, 'Zero-Based Budget', 'Every dollar is allocated to a category, leaving zero unallocated', 1, 1, GETUTCDATE(), GETUTCDATE());
        
        INSERT INTO [dbo].[AllocationTemplateCategories] (Id, TemplateId, CategoryName, Description, Percentage, DisplayOrder, Color)
        VALUES 
            (NEWID(), @TemplateIdZeroBased, 'Housing', 'Rent/mortgage, utilities, maintenance', 25.00, 1, '#E91E63'),
            (NEWID(), @TemplateIdZeroBased, 'Food', 'Groceries and dining', 15.00, 2, '#9C27B0'),
            (NEWID(), @TemplateIdZeroBased, 'Transportation', 'Car payment, gas, insurance, maintenance', 15.00, 3, '#3F51B5'),
            (NEWID(), @TemplateIdZeroBased, 'Debt Payments', 'Credit cards, loans, other debts', 15.00, 4, '#FF5722'),
            (NEWID(), @TemplateIdZeroBased, 'Savings & Investments', 'Emergency fund, retirement, investments', 20.00, 5, '#009688'),
            (NEWID(), @TemplateIdZeroBased, 'Other Expenses', 'Entertainment, personal care, miscellaneous', 10.00, 6, '#795548');
        
        PRINT 'Seeded Zero-Based Budget template';
    END
END TRY
BEGIN CATCH
    PRINT 'Error seeding Zero-Based Budget template: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- 60/20/20 Rule Template (Conservative)
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM [dbo].[AllocationTemplates] WHERE Name = '60/20/20 Rule (Conservative)')
    BEGIN
        DECLARE @TemplateId60_20_20 NVARCHAR(450) = NEWID();
        
        INSERT INTO [dbo].[AllocationTemplates] (Id, Name, Description, IsSystemTemplate, IsActive, CreatedAt, UpdatedAt)
        VALUES (@TemplateId60_20_20, '60/20/20 Rule (Conservative)', 'Allocate 60% to Needs, 20% to Wants, and 20% to Savings', 1, 1, GETUTCDATE(), GETUTCDATE());
        
        INSERT INTO [dbo].[AllocationTemplateCategories] (Id, TemplateId, CategoryName, Description, Percentage, DisplayOrder, Color)
        VALUES 
            (NEWID(), @TemplateId60_20_20, 'Needs', 'Essential expenses: housing, utilities, food, transportation, insurance', 60.00, 1, '#F44336'),
            (NEWID(), @TemplateId60_20_20, 'Wants', 'Discretionary spending: entertainment, dining out, hobbies', 20.00, 2, '#FF9800'),
            (NEWID(), @TemplateId60_20_20, 'Savings', 'Savings, investments, debt repayment, emergency fund', 20.00, 3, '#4CAF50');
        
        PRINT 'Seeded 60/20/20 Rule (Conservative) template';
    END
END TRY
BEGIN CATCH
    PRINT 'Error seeding 60/20/20 Rule template: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- 70/20/10 Rule Template (Aggressive Savings)
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AllocationTemplates' AND type = 'U' AND SCHEMA_NAME(schema_id) = 'dbo')
       AND NOT EXISTS (SELECT * FROM [dbo].[AllocationTemplates] WHERE Name = '70/20/10 Rule (Aggressive Savings)')
    BEGIN
        DECLARE @TemplateId70_20_10 NVARCHAR(450) = NEWID();
        
        INSERT INTO [dbo].[AllocationTemplates] (Id, Name, Description, IsSystemTemplate, IsActive, CreatedAt, UpdatedAt)
        VALUES (@TemplateId70_20_10, '70/20/10 Rule (Aggressive Savings)', 'Allocate 70% to Needs, 20% to Savings, and 10% to Wants', 1, 1, GETUTCDATE(), GETUTCDATE());
        
        INSERT INTO [dbo].[AllocationTemplateCategories] (Id, TemplateId, CategoryName, Description, Percentage, DisplayOrder, Color)
        VALUES 
            (NEWID(), @TemplateId70_20_10, 'Needs', 'Essential expenses: housing, utilities, food, transportation, insurance', 70.00, 1, '#F44336'),
            (NEWID(), @TemplateId70_20_10, 'Savings', 'Savings, investments, debt repayment, emergency fund', 20.00, 2, '#4CAF50'),
            (NEWID(), @TemplateId70_20_10, 'Wants', 'Discretionary spending: entertainment, dining out, hobbies', 10.00, 3, '#FF9800');
        
        PRINT 'Seeded 70/20/10 Rule (Aggressive Savings) template';
    END
END TRY
BEGIN CATCH
    PRINT 'Error seeding 70/20/10 Rule template: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK;
END CATCH

IF @@TRANCOUNT > 0 COMMIT;
GO

-- Note: No transaction commit needed - each statement commits automatically

PRINT '========================================';
PRINT 'Allocation Planner Migration Complete!';
PRINT '========================================';
PRINT 'Tables created:';
PRINT '  - AllocationTemplates';
PRINT '  - AllocationTemplateCategories';
PRINT '  - AllocationPlans';
PRINT '  - AllocationCategories';
PRINT '  - AllocationHistories';
PRINT '  - AllocationRecommendations';
PRINT '';
PRINT 'System templates seeded:';
PRINT '  - 50/30/20 Rule';
PRINT '  - Zero-Based Budget';
PRINT '  - 60/20/20 Rule (Conservative)';
PRINT '  - 70/20/10 Rule (Aggressive Savings)';
PRINT '========================================';

