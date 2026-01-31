-- Add PinHash column to Users table (mobile PIN login)
-- Migration: 20260131000000_AddPinHashToUser

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Users'
    AND COLUMN_NAME = 'PinHash'
)
BEGIN
    ALTER TABLE [Users]
    ADD [PinHash] NVARCHAR(255) NULL;

    PRINT 'PinHash column added successfully.';
END
ELSE
BEGIN
    PRINT 'PinHash column already exists.';
END

PRINT 'PinHash migration completed successfully.';
