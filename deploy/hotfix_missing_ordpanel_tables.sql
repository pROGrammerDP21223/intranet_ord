-- Hotfix: create missing ordpanel-related tables in production.
-- Safe to run multiple times.

IF OBJECT_ID('dbo.OrdpanelEnquiries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrdpanelEnquiries (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrdpanelEnquiries PRIMARY KEY,
        Name nvarchar(255) NULL,
        Email nvarchar(255) NULL,
        Phone nvarchar(50) NULL,
        ProductName nvarchar(500) NULL,
        ClientName nvarchar(255) NULL,
        ListingClientId nvarchar(50) NULL,
        Message nvarchar(max) NULL,
        PageType nvarchar(50) NOT NULL CONSTRAINT DF_OrdpanelEnquiries_PageType DEFAULT('general'),
        PageUrl nvarchar(1000) NULL,
        Status nvarchar(50) NOT NULL CONSTRAINT DF_OrdpanelEnquiries_Status DEFAULT('New'),
        IsDeleted bit NOT NULL CONSTRAINT DF_OrdpanelEnquiries_IsDeleted DEFAULT(0),
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_OrdpanelEnquiries_CreatedAt DEFAULT(SYSUTCDATETIME()),
        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_OrdpanelEnquiries_UpdatedAt DEFAULT(SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID('dbo.FreeRegistrations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FreeRegistrations (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FreeRegistrations PRIMARY KEY,
        CompanyName nvarchar(255) NOT NULL,
        ContactPerson nvarchar(255) NOT NULL,
        Designation nvarchar(100) NULL,
        Address nvarchar(500) NULL,
        Phone nvarchar(20) NOT NULL,
        Email nvarchar(255) NULL,
        WhatsAppNumber nvarchar(20) NULL,
        DomainName nvarchar(255) NULL,
        ProductsInterested nvarchar(max) NULL,
        Status nvarchar(50) NOT NULL CONSTRAINT DF_FreeRegistrations_Status DEFAULT('Pending'),
        ApprovedBy nvarchar(100) NULL,
        ApprovedAt datetime2 NULL,
        RejectionReason nvarchar(500) NULL,
        Notes nvarchar(max) NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_FreeRegistrations_IsDeleted DEFAULT(0),
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_FreeRegistrations_CreatedAt DEFAULT(SYSUTCDATETIME()),
        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_FreeRegistrations_UpdatedAt DEFAULT(SYSUTCDATETIME())
    );
END
GO
