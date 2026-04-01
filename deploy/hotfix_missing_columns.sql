-- Hotfix for production 500s caused by schema mismatch.
-- Safe to run multiple times.

IF COL_LENGTH('dbo.Clients', 'IsPremium') IS NULL
BEGIN
    ALTER TABLE dbo.Clients ADD IsPremium bit NOT NULL CONSTRAINT DF_Clients_IsPremium DEFAULT(0);
END
GO

IF COL_LENGTH('dbo.Categories', 'IsHome') IS NULL
BEGIN
    ALTER TABLE dbo.Categories ADD IsHome bit NOT NULL CONSTRAINT DF_Categories_IsHome DEFAULT(0);
END
GO

IF COL_LENGTH('dbo.Categories', 'IsTop') IS NULL
BEGIN
    ALTER TABLE dbo.Categories ADD IsTop bit NOT NULL CONSTRAINT DF_Categories_IsTop DEFAULT(0);
END
GO
