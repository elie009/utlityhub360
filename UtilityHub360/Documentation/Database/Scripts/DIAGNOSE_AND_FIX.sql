-- ============================================
-- STEP 1: DIAGNOSTIC - Check what exists
-- ============================================
PRINT '============================================';
PRINT 'STEP 1: DIAGNOSTIC INFORMATION';
PRINT '============================================';
PRINT '';

-- Check current database
PRINT 'Current Database:';
SELECT DB_NAME() AS CurrentDatabase;
PRINT '';

-- Check if Bills table exists (any schema)
PRINT 'Checking for Bills table...';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills')
BEGIN
    SELECT 
        '✓ FOUND' AS Status,
        SCHEMA_NAME(schema_id) AS SchemaName,
        name AS TableName,
        OBJECT_ID(SCHEMA_NAME(schema_id) + '.' + name) AS ObjectId
    FROM sys.tables 
    WHERE name = 'Bills';
    
    PRINT '✓ Bills table exists!';
END
ELSE
BEGIN
    PRINT '❌ Bills table NOT found!';
    PRINT '';
    PRINT 'Tables with "Bill" in name:';
    SELECT 
        SCHEMA_NAME(schema_id) AS SchemaName,
        name AS TableName
    FROM sys.tables 
    WHERE name LIKE '%Bill%'
    ORDER BY SchemaName, TableName;
END
GO

-- ============================================
-- STEP 2: ADD COLUMNS (Only if table exists)
-- ============================================
PRINT '';
PRINT '============================================';
PRINT 'STEP 2: ADDING COLUMNS';
PRINT '============================================';
PRINT '';

-- Check if table exists before trying to add columns
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Adding columns to dbo.Bills...';
    PRINT '';

    -- IsScheduledPayment
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'IsScheduledPayment')
    BEGIN
        ALTER TABLE dbo.Bills ADD IsScheduledPayment bit NOT NULL DEFAULT 0;
        PRINT '✓ Added IsScheduledPayment';
    END
    ELSE PRINT '⚠ IsScheduledPayment exists';

    -- ScheduledPaymentBankAccountId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'ScheduledPaymentBankAccountId')
    BEGIN
        ALTER TABLE dbo.Bills ADD ScheduledPaymentBankAccountId nvarchar(450) NULL;
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ScheduledPaymentBankAccountId' AND object_id = OBJECT_ID('dbo.Bills'))
            CREATE INDEX IX_Bills_ScheduledPaymentBankAccountId ON dbo.Bills (ScheduledPaymentBankAccountId);
        PRINT '✓ Added ScheduledPaymentBankAccountId';
    END
    ELSE PRINT '⚠ ScheduledPaymentBankAccountId exists';

    -- ScheduledPaymentDaysBeforeDue
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'ScheduledPaymentDaysBeforeDue')
    BEGIN
        ALTER TABLE dbo.Bills ADD ScheduledPaymentDaysBeforeDue int NULL;
        PRINT '✓ Added ScheduledPaymentDaysBeforeDue';
    END
    ELSE PRINT '⚠ ScheduledPaymentDaysBeforeDue exists';

    -- LastScheduledPaymentAttempt
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'LastScheduledPaymentAttempt')
    BEGIN
        ALTER TABLE dbo.Bills ADD LastScheduledPaymentAttempt datetime2(7) NULL;
        PRINT '✓ Added LastScheduledPaymentAttempt';
    END
    ELSE PRINT '⚠ LastScheduledPaymentAttempt exists';

    -- ScheduledPaymentFailureReason
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'ScheduledPaymentFailureReason')
    BEGIN
        ALTER TABLE dbo.Bills ADD ScheduledPaymentFailureReason nvarchar(500) NULL;
        PRINT '✓ Added ScheduledPaymentFailureReason';
    END
    ELSE PRINT '⚠ ScheduledPaymentFailureReason exists';

    -- ApprovalStatus
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'ApprovalStatus')
    BEGIN
        ALTER TABLE dbo.Bills ADD ApprovalStatus nvarchar(20) NULL;
        UPDATE dbo.Bills SET ApprovalStatus = 'APPROVED' WHERE ApprovalStatus IS NULL;
        ALTER TABLE dbo.Bills ALTER COLUMN ApprovalStatus nvarchar(20) NOT NULL;
        IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Bills_ApprovalStatus')
            ALTER TABLE dbo.Bills ADD CONSTRAINT DF_Bills_ApprovalStatus DEFAULT 'APPROVED' FOR ApprovalStatus;
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ApprovalStatus' AND object_id = OBJECT_ID('dbo.Bills'))
            CREATE INDEX IX_Bills_ApprovalStatus ON dbo.Bills (ApprovalStatus);
        PRINT '✓ Added ApprovalStatus';
    END
    ELSE PRINT '⚠ ApprovalStatus exists';

    -- ApprovedBy
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'ApprovedBy')
    BEGIN
        ALTER TABLE dbo.Bills ADD ApprovedBy nvarchar(450) NULL;
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ApprovedBy' AND object_id = OBJECT_ID('dbo.Bills'))
            CREATE INDEX IX_Bills_ApprovedBy ON dbo.Bills (ApprovedBy);
        PRINT '✓ Added ApprovedBy';
    END
    ELSE PRINT '⚠ ApprovedBy exists';

    -- ApprovedAt
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'ApprovedAt')
    BEGIN
        ALTER TABLE dbo.Bills ADD ApprovedAt datetime2(7) NULL;
        PRINT '✓ Added ApprovedAt';
    END
    ELSE PRINT '⚠ ApprovedAt exists';

    -- ApprovalNotes
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bills') AND name = 'ApprovalNotes')
    BEGIN
        ALTER TABLE dbo.Bills ADD ApprovalNotes nvarchar(500) NULL;
        PRINT '✓ Added ApprovalNotes';
    END
    ELSE PRINT '⚠ ApprovalNotes exists';

    PRINT '';
    PRINT '============================================';
    PRINT '✓ MIGRATION COMPLETED SUCCESSFULLY!';
    PRINT '============================================';
END
ELSE IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills')
BEGIN
    -- Table exists but in different schema
    DECLARE @SchemaName NVARCHAR(128);
    SELECT @SchemaName = SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = 'Bills';
    PRINT '⚠ Bills table found in schema: ' + @SchemaName;
    PRINT 'Please modify the script to use: [' + @SchemaName + '].[Bills]';
END
ELSE
BEGIN
    PRINT '❌ ERROR: Bills table does not exist!';
    PRINT 'Please check:';
    PRINT '1. Are you connected to the correct database (DBUTILS)?';
    PRINT '2. Does the Bills table exist?';
    PRINT '3. Do you have proper permissions?';
END
GO

