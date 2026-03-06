-- Generate Verification Tokens for Existing Users
-- This script creates verification tokens for existing users so they can verify their emails
-- Use this if you want existing users to verify their emails

BEGIN TRANSACTION;

BEGIN TRY
    -- Generate unique tokens for users who don't have them
    UPDATE [Users]
    SET [EmailVerificationToken] = LOWER(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', '')),
        [EmailVerificationTokenExpiresAt] = DATEADD(HOUR, 24, GETUTCDATE()),
        [EmailVerified] = 0,
        [UpdatedAt] = GETUTCDATE()
    WHERE ([EmailVerificationToken] IS NULL OR [EmailVerificationToken] = '')
      AND [EmailVerified] = 0;

    -- Get count of updated users
    DECLARE @UpdatedCount INT;
    SET @UpdatedCount = @@ROWCOUNT;

    -- Show summary
    SELECT 
        COUNT(*) AS TotalUsers,
        SUM(CASE WHEN [EmailVerified] = 1 THEN 1 ELSE 0 END) AS VerifiedUsers,
        SUM(CASE WHEN [EmailVerified] = 0 AND [EmailVerificationToken] IS NOT NULL THEN 1 ELSE 0 END) AS UnverifiedUsersWithTokens,
        SUM(CASE WHEN [EmailVerified] = 0 AND [EmailVerificationToken] IS NULL THEN 1 ELSE 0 END) AS UnverifiedUsersWithoutTokens
    FROM [Users];

    COMMIT TRANSACTION;
    
    PRINT 'SUCCESS: ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' users now have verification tokens.';
    PRINT 'These users will need to verify their emails before logging in.';
    PRINT 'You can send them verification emails using the resend-verification endpoint.';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT 'ERROR: ' + ERROR_MESSAGE();
    PRINT 'Transaction rolled back. No changes were made.';
    
    THROW;
END CATCH;


