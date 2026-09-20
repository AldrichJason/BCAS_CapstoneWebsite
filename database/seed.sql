/*
    BCAS Capstone Website - seed data (BW-9)
    Run after schema.sql. Safe to re-run: skips rows that already exist.
*/

SET NOCOUNT ON;
GO

------------------------------------------------------------
-- Departments (TODO: rename to the school's actual four departments)
------------------------------------------------------------
INSERT INTO dbo.Departments (Name)
SELECT v.Name
FROM (VALUES
    ('College of Computer Studies'),
    ('College of Business Administration'),
    ('College of Engineering'),
    ('College of Arts and Sciences')
) AS v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Departments d WHERE d.Name = v.Name);
GO

------------------------------------------------------------
-- Roles
------------------------------------------------------------
INSERT INTO dbo.Roles (Name)
SELECT v.Name
FROM (VALUES
    ('SuperAdmin'),
    ('AcademicHead'),
    ('AdminOfficeRegistrar'),
    ('VpOfOperations')
) AS v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.Name = v.Name);
GO

------------------------------------------------------------
-- Initial Super Admin account
-- Username: superadmin
-- Email:    superadmin@bcas.edu.ph
-- Password: ChangeMe123!   <-- change immediately after first login
-- Hash generated with the same PBKDF2-HMACSHA256 (100,000 iterations, 32-byte
-- key, 16-byte salt) as Helper/PasswordHasher.cs.
------------------------------------------------------------
INSERT INTO dbo.Users (FirstName, LastName, Username, Email, PasswordHash, PasswordSalt, RoleId, DepartmentId, IsActive)
SELECT
    'Super',
    'Admin',
    'superadmin',
    'superadmin@bcas.edu.ph',
    'RBUj1Tn/ZIKAxn4UunwOO61xFKu82+i9t+P2SIpDnNg=',
    'zW4eqJ43sqC6Y62cROe/YA==',
    r.Id,
    NULL,
    1
FROM dbo.Roles r
WHERE r.Name = 'SuperAdmin'
  AND NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Email = 'superadmin@bcas.edu.ph');
GO
