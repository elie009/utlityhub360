-- ====================================================================
-- DELETE REPAYMENT SCHEDULES FROM JANUARY 2026 TO DECEMBER 2031
-- ====================================================================
-- ⚠️ WARNING: This will permanently delete repayment schedules
-- Make sure to backup your database before running this script!

USE UtilityHub360; -- Replace with your actual database name

BEGIN TRANSACTION;

PRINT 'Starting deletion of repayment schedules from January 2026 to December 2031...';

-- Step 1: Get count and details of schedules to be deleted
DECLARE @ScheduleCount INT;
DECLARE @LoanCount INT;

SELECT @ScheduleCount = COUNT(*)
FROM RepaymentSchedules rs
INNER JOIN Loans l ON rs.LoanId = l.Id
WHERE rs.DueDate >= '2026-01-01' 
  AND rs.DueDate <= '2031-12-31';

SELECT @LoanCount = COUNT(DISTINCT rs.LoanId)
FROM RepaymentSchedules rs
INNER JOIN Loans l ON rs.LoanId = l.Id
WHERE rs.DueDate >= '2026-01-01' 
  AND rs.DueDate <= '2031-12-31';

PRINT 'Found ' + CAST(@ScheduleCount AS VARCHAR(10)) + ' repayment schedules to delete';
PRINT 'Affecting ' + CAST(@LoanCount AS VARCHAR(10)) + ' loan(s)';

-- Step 2: Show what will be affected (for verification)
SELECT 
    l.Id as LoanId,
    l.UserId,
    COUNT(rs.Id) as SchedulesToDelete,
    SUM(rs.PrincipalAmount) as TotalPrincipalRemoved,
    SUM(rs.TotalAmount) as TotalAmountRemoved,
    MIN(rs.DueDate) as FirstScheduleDate,
    MAX(rs.DueDate) as LastScheduleDate
FROM RepaymentSchedules rs
INNER JOIN Loans l ON rs.LoanId = l.Id
WHERE rs.DueDate >= '2026-01-01' 
  AND rs.DueDate <= '2031-12-31'
GROUP BY l.Id, l.UserId
ORDER BY l.UserId, l.Id;

-- Step 3: Update loan remaining balances before deleting schedules
UPDATE l SET 
    RemainingBalance = l.RemainingBalance - ISNULL(summary.TotalPrincipalRemoved, 0),
    TotalAmount = l.TotalAmount - ISNULL(summary.TotalAmountRemoved, 0)
FROM Loans l
INNER JOIN (
    SELECT 
        rs.LoanId,
        SUM(rs.PrincipalAmount) as TotalPrincipalRemoved,
        SUM(rs.TotalAmount) as TotalAmountRemoved
    FROM RepaymentSchedules rs
    WHERE rs.DueDate >= '2026-01-01' 
      AND rs.DueDate <= '2031-12-31'
    GROUP BY rs.LoanId
) summary ON l.Id = summary.LoanId;

PRINT 'Updated loan balances';

-- Step 4: Mark loans as completed if remaining balance becomes 0
UPDATE Loans SET 
    Status = 'COMPLETED',
    CompletedAt = GETUTCDATE()
WHERE RemainingBalance <= 0 
  AND Status != 'COMPLETED'
  AND Id IN (
    SELECT DISTINCT rs.LoanId
    FROM RepaymentSchedules rs
    WHERE rs.DueDate >= '2026-01-01' 
      AND rs.DueDate <= '2031-12-31'
  );

PRINT 'Marked applicable loans as completed';

-- Step 5: Delete the repayment schedules
DELETE rs FROM RepaymentSchedules rs
INNER JOIN Loans l ON rs.LoanId = l.Id
WHERE rs.DueDate >= '2026-01-01' 
  AND rs.DueDate <= '2031-12-31';

PRINT 'Deleted ' + CAST(@ScheduleCount AS VARCHAR(10)) + ' repayment schedules';

-- Step 6: Verify deletion
DECLARE @RemainingSchedules INT;
SELECT @RemainingSchedules = COUNT(*)
FROM RepaymentSchedules rs
INNER JOIN Loans l ON rs.LoanId = l.Id
WHERE rs.DueDate >= '2026-01-01' 
  AND rs.DueDate <= '2031-12-31';

IF @RemainingSchedules = 0
BEGIN
    PRINT 'SUCCESS: All repayment schedules from 2026-2031 have been deleted';
    COMMIT TRANSACTION;
END
ELSE
BEGIN
    PRINT 'WARNING: ' + CAST(@RemainingSchedules AS VARCHAR(10)) + ' schedules still remain. Rolling back...';
    ROLLBACK TRANSACTION;
END

-- Final summary
SELECT 
    YEAR(DueDate) as Year,
    COUNT(*) as ScheduleCount,
    COUNT(DISTINCT LoanId) as LoanCount
FROM RepaymentSchedules
WHERE DueDate >= '2024-01-01' AND DueDate <= '2025-12-31'
GROUP BY YEAR(DueDate)
ORDER BY Year;

PRINT 'Summary of remaining repayment schedules in reasonable date range (2024-2025)';
PRINT 'Deletion process completed.';
