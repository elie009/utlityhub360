-- Force Verify All Existing Users
-- This script marks all existing users as verified so they can continue using the system
-- Use this if you want existing users to bypass email verification

BEGIN TRANSACTION;

BEGIN TRY
    -- Mark all existing users as verified
    UPDATE [Users] 
    SET [EmailVerified] = 1,
        [EmailVerificationToken] = NULL,
        [EmailVerificationTokenExpiresAt] = NULL,
        [UpdatedAt] = GETUTCDATE()
    WHERE [EmailVerified] = 0;

    -- Get count of updated users
    DECLARE @UpdatedCount INT;
    SET @UpdatedCount = @@ROWCOUNT;

    -- Verify the update
    SELECT 
        COUNT(*) AS TotalUsers,
        SUM(CASE WHEN [EmailVerified] = 1 THEN 1 ELSE 0 END) AS VerifiedUsers,
        SUM(CASE WHEN [EmailVerified] = 0 THEN 1 ELSE 0 END) AS UnverifiedUsers
    FROM [Users];

    COMMIT TRANSACTION;
    
    PRINT 'SUCCESS: ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' users marked as verified.';
    PRINT 'All existing users can now log in without email verification.';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT 'ERROR: ' + ERROR_MESSAGE();
    PRINT 'Transaction rolled back. No changes were made.';
    
    THROW;
END CATCH;


