/*
    Migration 001: add Users.Username

    For an EXISTING database only (one already carrying real accounts). If you
    are setting up the database for the first time, just run schema.sql +
    seed.sql — this migration is already folded into schema.sql there and
    you do not need to run it separately.

    Non-destructive: backfills Username from the local part of each existing
    account's Email (e.g. 'jane.doe@bcas.edu.ph' -> 'jane.doe') so nobody's
    row is left without one before the NOT NULL / UNIQUE constraints are
    added. Rename any of the backfilled usernames afterwards if you'd like
    something nicer.
*/

SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.Users', 'Username') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Username NVARCHAR(100) NULL;
END
GO

UPDATE dbo.Users
SET Username = LEFT(Email, CHARINDEX('@', Email) - 1)
WHERE Username IS NULL;
GO

-- Only proceed to NOT NULL / UNIQUE once every row has a value, in case the
-- backfill above produced duplicates (e.g. two accounts sharing a local part
-- from different domains) — resolve those manually before re-running.
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username IS NULL)
   AND NOT EXISTS (
       SELECT Username FROM dbo.Users GROUP BY Username HAVING COUNT(*) > 1
   )
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN Username NVARCHAR(100) NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes WHERE name = 'UQ_Users_Username' AND object_id = OBJECT_ID('dbo.Users')
    )
    BEGIN
        ALTER TABLE dbo.Users ADD CONSTRAINT UQ_Users_Username UNIQUE (Username);
    END
END
GO
