-- Add Email Verification Columns to Users Table
-- Migration: 20250120120000_AddEmailVerificationToUser

-- Check if columns already exist before adding
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'EmailVerified'
)
BEGIN
    ALTER TABLE [Users]
    ADD [EmailVerified] BIT NOT NULL DEFAULT 0;
    
    PRINT 'EmailVerified column added successfully.';
END
ELSE
BEGIN
    PRINT 'EmailVerified column already exists.';
END

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'EmailVerificationToken'
)
BEGIN
    ALTER TABLE [Users]
    ADD [EmailVerificationToken] NVARCHAR(255) NULL;
    
    PRINT 'EmailVerificationToken column added successfully.';
END
ELSE
BEGIN
    PRINT 'EmailVerificationToken column already exists.';
END

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'EmailVerificationTokenExpiresAt'
)
BEGIN
    ALTER TABLE [Users]
    ADD [EmailVerificationTokenExpiresAt] DATETIME2 NULL;
    
    PRINT 'EmailVerificationTokenExpiresAt column added successfully.';
END
ELSE
BEGIN
    PRINT 'EmailVerificationTokenExpiresAt column already exists.';
END

-- Set all existing users as verified (optional - you may want to keep them unverified)
-- Uncomment the following if you want to mark existing users as verified:
-- UPDATE [Users] SET [EmailVerified] = 1 WHERE [EmailVerified] = 0;

PRINT 'Email verification columns migration completed successfully.';


