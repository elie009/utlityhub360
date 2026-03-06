-- Add Email Verification Tokens for Existing Users
-- This script generates verification tokens for users who don't have them

-- Option 1: Mark all existing users as verified (recommended for existing users)
-- This allows them to continue using the system without needing to verify
UPDATE [Users] 
SET [EmailVerified] = 1,
    [EmailVerificationToken] = NULL,
    [EmailVerificationTokenExpiresAt] = NULL
WHERE [EmailVerified] = 0 
  AND ([EmailVerificationToken] IS NULL OR [EmailVerificationToken] = '');

PRINT 'Existing users marked as verified.';

-- Option 2: Generate verification tokens for existing users (if you want them to verify)
-- Uncomment the following if you want existing users to verify their emails:
/*
-- Generate unique tokens for each user
UPDATE [Users]
SET [EmailVerificationToken] = NEWID(),
    [EmailVerificationTokenExpiresAt] = DATEADD(HOUR, 24, GETUTCDATE()),
    [EmailVerified] = 0
WHERE [EmailVerificationToken] IS NULL 
  AND [EmailVerified] = 0;

PRINT 'Verification tokens generated for existing unverified users.';
*/

-- Verify the update
SELECT 
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN [EmailVerified] = 1 THEN 1 ELSE 0 END) AS VerifiedUsers,
    SUM(CASE WHEN [EmailVerified] = 0 THEN 1 ELSE 0 END) AS UnverifiedUsers,
    SUM(CASE WHEN [EmailVerificationToken] IS NOT NULL THEN 1 ELSE 0 END) AS UsersWithTokens
FROM [Users];

PRINT 'Script completed. Check the summary above.';


