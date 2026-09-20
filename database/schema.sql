/*
    BCAS Capstone Website - SQL Server schema (BW-9)
    Repeatable from empty: drops (if present) then recreates every object.
    Run this before seed.sql.
*/

SET NOCOUNT ON;
GO

------------------------------------------------------------
-- Drop existing objects (dependency-safe order)
------------------------------------------------------------
IF OBJECT_ID('dbo.SearchAnalytics', 'U') IS NOT NULL DROP TABLE dbo.SearchAnalytics;
IF OBJECT_ID('dbo.ActivityLog', 'U') IS NOT NULL DROP TABLE dbo.ActivityLog;
IF OBJECT_ID('dbo.Images', 'U') IS NOT NULL DROP TABLE dbo.Images;
IF OBJECT_ID('dbo.FaqInquiries', 'U') IS NOT NULL DROP TABLE dbo.FaqInquiries;
IF OBJECT_ID('dbo.ContactInquiries', 'U') IS NOT NULL DROP TABLE dbo.ContactInquiries;
IF OBJECT_ID('dbo.Faqs', 'U') IS NOT NULL DROP TABLE dbo.Faqs;
IF OBJECT_ID('dbo.ServicesPolicies', 'U') IS NOT NULL DROP TABLE dbo.ServicesPolicies;
IF OBJECT_ID('dbo.AdmissionRequirements', 'U') IS NOT NULL DROP TABLE dbo.AdmissionRequirements;
IF OBJECT_ID('dbo.AcademicPrograms', 'U') IS NOT NULL DROP TABLE dbo.AcademicPrograms;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Announcements', 'U') IS NOT NULL DROP TABLE dbo.Announcements;
IF OBJECT_ID('dbo.News', 'U') IS NOT NULL DROP TABLE dbo.News;
IF OBJECT_ID('dbo.PasswordResetTokens', 'U') IS NOT NULL DROP TABLE dbo.PasswordResetTokens;
IF OBJECT_ID('dbo.RevokedTokens', 'U') IS NOT NULL DROP TABLE dbo.RevokedTokens;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

------------------------------------------------------------
-- Core identity tables
------------------------------------------------------------
CREATE TABLE dbo.Departments
(
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    Name    NVARCHAR(150) NOT NULL UNIQUE
);
GO

CREATE TABLE dbo.Roles
(
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    Name    NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE dbo.Users
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(200) NOT NULL,
    Email           NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256) NOT NULL,
    PasswordSalt    NVARCHAR(256) NOT NULL,
    RoleId          INT NOT NULL,
    DepartmentId    INT NULL,               -- required for AcademicHead only, enforced in application layer
    IsActive        BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id),
    CONSTRAINT FK_Users_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id)
);
GO

CREATE INDEX IX_Users_DepartmentId ON dbo.Users(DepartmentId);
CREATE INDEX IX_Users_RoleId ON dbo.Users(RoleId);
GO

-- Single-use, hashed password reset tokens (BW-13).
CREATE TABLE dbo.PasswordResetTokens
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    TokenHash       NVARCHAR(256) NOT NULL,
    ExpiresAtUtc    DATETIME2 NOT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UsedAtUtc       DATETIME2 NULL,
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_PasswordResetTokens_UserId ON dbo.PasswordResetTokens(UserId);
GO

-- JWTs revoked before their natural expiry, e.g. via logout (BW-11).
CREATE TABLE dbo.RevokedTokens
(
    Jti             NVARCHAR(64) NOT NULL PRIMARY KEY,
    ExpiresAtUtc    DATETIME2 NOT NULL,
    RevokedAtUtc    DATETIME2 NOT NULL CONSTRAINT DF_RevokedTokens_RevokedAt DEFAULT (SYSUTCDATETIME())
);
GO

------------------------------------------------------------
-- Content tables
-- Status: Draft | Scheduled | Published | Archived
-- DepartmentId NULL = school-wide content
------------------------------------------------------------
CREATE TABLE dbo.News
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(300) NOT NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    DepartmentId    INT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_News_Status DEFAULT ('Draft'),
    PublishAtUtc    DATETIME2 NULL,
    CreatedBy       INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_News_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       INT NULL,
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_News_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_News_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_News_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_News_Status CHECK (Status IN ('Draft','Scheduled','Published','Archived'))
);
GO

-- Supports department-scoped list views filtered by status (BW-16, BW-17).
CREATE INDEX IX_News_DepartmentId_Status ON dbo.News(DepartmentId, Status);
GO

CREATE TABLE dbo.Announcements
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(300) NOT NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    DepartmentId    INT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_Announcements_Status DEFAULT ('Draft'),
    PublishAtUtc    DATETIME2 NULL,
    CreatedBy       INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Announcements_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       INT NULL,
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_Announcements_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Announcements_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Announcements_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Announcements_Status CHECK (Status IN ('Draft','Scheduled','Published','Archived'))
);
GO

-- Supports department-scoped list views filtered by status and sorted by date (BW-18).
CREATE INDEX IX_Announcements_DepartmentId_Status ON dbo.Announcements(DepartmentId, Status);
GO

CREATE TABLE dbo.Events
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(300) NOT NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    EventStartUtc   DATETIME2 NOT NULL,
    EventEndUtc     DATETIME2 NULL,
    Location        NVARCHAR(300) NULL,
    DepartmentId    INT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_Status DEFAULT ('Draft'),
    PublishAtUtc    DATETIME2 NULL,
    CreatedBy       INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       INT NULL,
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_Events_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Events_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Events_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Events_Status CHECK (Status IN ('Draft','Scheduled','Published','Archived'))
);
GO

CREATE TABLE dbo.AcademicPrograms
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(300) NOT NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    DepartmentId    INT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_AcademicPrograms_Status DEFAULT ('Draft'),
    PublishAtUtc    DATETIME2 NULL,
    CreatedBy       INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_AcademicPrograms_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       INT NULL,
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_AcademicPrograms_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_AcademicPrograms_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_AcademicPrograms_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_AcademicPrograms_Status CHECK (Status IN ('Draft','Scheduled','Published','Archived'))
);
GO

CREATE TABLE dbo.AdmissionRequirements
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(300) NOT NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    DepartmentId    INT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_AdmissionRequirements_Status DEFAULT ('Draft'),
    PublishAtUtc    DATETIME2 NULL,
    CreatedBy       INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_AdmissionRequirements_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       INT NULL,
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_AdmissionRequirements_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_AdmissionRequirements_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_AdmissionRequirements_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_AdmissionRequirements_Status CHECK (Status IN ('Draft','Scheduled','Published','Archived'))
);
GO

CREATE TABLE dbo.ServicesPolicies
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(300) NOT NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    DepartmentId    INT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_ServicesPolicies_Status DEFAULT ('Draft'),
    PublishAtUtc    DATETIME2 NULL,
    CreatedBy       INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_ServicesPolicies_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       INT NULL,
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_ServicesPolicies_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_ServicesPolicies_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ServicesPolicies_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_ServicesPolicies_Status CHECK (Status IN ('Draft','Scheduled','Published','Archived'))
);
GO

CREATE TABLE dbo.Faqs
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Question        NVARCHAR(500) NOT NULL,
    Answer          NVARCHAR(MAX) NOT NULL,
    DepartmentId    INT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_Faqs_Status DEFAULT ('Draft'),
    PublishAtUtc    DATETIME2 NULL,
    CreatedBy       INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Faqs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedBy       INT NULL,
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_Faqs_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Faqs_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Faqs_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Faqs_Status CHECK (Status IN ('Draft','Scheduled','Published','Archived'))
);
GO

------------------------------------------------------------
-- Inquiries
------------------------------------------------------------
CREATE TABLE dbo.ContactInquiries
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(200) NOT NULL,
    Email           NVARCHAR(256) NOT NULL,
    Subject         NVARCHAR(300) NOT NULL,
    Message         NVARCHAR(MAX) NOT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_ContactInquiries_Status DEFAULT ('New'),
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_ContactInquiries_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_ContactInquiries_Status CHECK (Status IN ('New','InProgress','Resolved'))
);
GO

CREATE TABLE dbo.FaqInquiries
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(200) NULL,
    Email           NVARCHAR(256) NULL,
    Question        NVARCHAR(MAX) NOT NULL,
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_FaqInquiries_Status DEFAULT ('New'),
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_FaqInquiries_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_FaqInquiries_Status CHECK (Status IN ('New','InProgress','Resolved'))
);
GO

------------------------------------------------------------
-- Media, audit and analytics
------------------------------------------------------------
CREATE TABLE dbo.Images
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    FileName            NVARCHAR(300) NOT NULL,
    Url                 NVARCHAR(1000) NOT NULL,
    RelatedEntityType   NVARCHAR(50) NULL,   -- e.g. 'News', 'Events'
    RelatedEntityId     INT NULL,
    UploadedBy          INT NOT NULL,
    UploadedAt          DATETIME2 NOT NULL CONSTRAINT DF_Images_UploadedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Images_UploadedBy FOREIGN KEY (UploadedBy) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_Images_RelatedEntity ON dbo.Images(RelatedEntityType, RelatedEntityId);
GO

CREATE TABLE dbo.ActivityLog
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NULL,
    Action          NVARCHAR(100) NOT NULL,
    EntityType      NVARCHAR(50) NULL,
    EntityId        INT NULL,
    Details         NVARCHAR(MAX) NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_ActivityLog_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_ActivityLog_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_ActivityLog_UserId ON dbo.ActivityLog(UserId);
CREATE INDEX IX_ActivityLog_EntityType_EntityId ON dbo.ActivityLog(EntityType, EntityId);
GO

CREATE TABLE dbo.SearchAnalytics
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    SearchTerm      NVARCHAR(300) NOT NULL,
    ResultsCount    INT NOT NULL,
    UserId          INT NULL,
    SearchedAt      DATETIME2 NOT NULL CONSTRAINT DF_SearchAnalytics_SearchedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_SearchAnalytics_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_SearchAnalytics_SearchTerm ON dbo.SearchAnalytics(SearchTerm);
GO
