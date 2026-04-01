IF COL_LENGTH('Clients', 'CompanyLogo') IS NULL
BEGIN
    ALTER TABLE [Clients] ADD [CompanyLogo] NVARCHAR(500) NULL;
END;

SELECT COL_LENGTH('Clients', 'CompanyLogo') AS CompanyLogoLength;
