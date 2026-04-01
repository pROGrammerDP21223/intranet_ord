IF DB_ID('backend_net_db') IS NULL
BEGIN
    CREATE DATABASE [backend_net_db];
END
GO

SELECT name FROM sys.databases;
GO
