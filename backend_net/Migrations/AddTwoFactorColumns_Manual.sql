-- Manual SQL script to add Two-Factor Authentication columns to Users table
-- Run this if the migration was recorded but columns weren't created

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TwoFactorSecret')
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorSecret] nvarchar(255) NULL;
    PRINT 'Added TwoFactorSecret column';
END
ELSE
    PRINT 'TwoFactorSecret column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TwoFactorEnabled')
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT 0;
    PRINT 'Added TwoFactorEnabled column';
END
ELSE
    PRINT 'TwoFactorEnabled column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TwoFactorEmailEnabled')
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorEmailEnabled] bit NOT NULL DEFAULT 0;
    PRINT 'Added TwoFactorEmailEnabled column';
END
ELSE
    PRINT 'TwoFactorEmailEnabled column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'BackupCodes')
BEGIN
    ALTER TABLE [Users] ADD [BackupCodes] nvarchar(500) NULL;
    PRINT 'Added BackupCodes column';
END
ELSE
    PRINT 'BackupCodes column already exists';

PRINT 'Two-Factor Authentication columns check completed.';

