-- =============================================
-- Stored Procedure: SP_GetBankStatementBalanceScalar
-- Description: Calculates the total balance for a bank statement
--              (Opening Balance + Net Matched Transactions)
--              Returns only the balance as a scalar value
-- Parameters:
--   @BankStatementId - The ID of the bank statement
--   @UserId - The user ID for security validation
-- Returns: Single decimal value (TotalBalance)
-- =============================================

DROP PROCEDURE IF EXISTS SP_GetBankStatementBalanceScalar;
GO

CREATE PROCEDURE SP_GetBankStatementBalanceScalar
    @BankStatementId NVARCHAR(450),
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalBalance DECIMAL(18, 2) = 0;

    -- Validate and calculate
    SELECT @TotalBalance = 
        bs.OpeningBalance + ISNULL((
            SELECT SUM(CASE 
                WHEN TransactionType = 'CREDIT' THEN ABS(Amount)
                WHEN TransactionType = 'DEBIT' THEN -ABS(Amount)
                ELSE 0
            END)
            FROM BankStatementItems 
            WHERE BankStatementId = bs.Id
              AND IsMatched = 1
        ), 0)
    FROM BankStatements bs
    WHERE bs.Id = @BankStatementId
      AND bs.UserId = @UserId;

    -- Return the balance
    SELECT @TotalBalance AS TotalBalance;
END
GO
