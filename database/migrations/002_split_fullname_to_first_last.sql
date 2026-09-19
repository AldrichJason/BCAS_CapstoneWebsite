/*
    Migration 002: split Users.FullName into FirstName + LastName

    For an EXISTING database only. If you are setting up the database for the
    first time, just run schema.sql + seed.sql — this is already folded in
    there and you do not need to run it separately.

    Non-destructive: backfills FirstName/LastName by splitting each existing
    account's FullName on the first space (e.g. 'Super Admin' -> 'Super' /
    'Admin'; a single-word name gets an empty LastName). Rename any of the
    backfilled values afterwards if you'd like something nicer. FullName
    itself is dropped at the end, once every row has a value.
*/

SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.Users', 'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD FirstName NVARCHAR(100) NULL;
END
GO

IF COL_LENGTH('dbo.Users', 'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD LastName NVARCHAR(100) NULL;
END
GO

UPDATE dbo.Users
SET FirstName = CASE
                    WHEN CHARINDEX(' ', FullName) > 0 THEN LEFT(FullName, CHARINDEX(' ', FullName) - 1)
                    ELSE FullName
                END,
    LastName = CASE
                    WHEN CHARINDEX(' ', FullName) > 0 THEN LTRIM(SUBSTRING(FullName, CHARINDEX(' ', FullName) + 1, LEN(FullName)))
                    ELSE ''
                END
WHERE FirstName IS NULL AND FullName IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE FirstName IS NULL OR LastName IS NULL)
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN FirstName NVARCHAR(100) NOT NULL;
    ALTER TABLE dbo.Users ALTER COLUMN LastName NVARCHAR(100) NOT NULL;
END
GO

IF COL_LENGTH('dbo.Users', 'FullName') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users DROP COLUMN FullName;
END
GO
