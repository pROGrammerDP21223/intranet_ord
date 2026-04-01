-- Clone all data/schema from backend_net_dev_db to backend_net_db
-- Runs inside SQL Server instance hosting both databases.

IF DB_ID('backend_net_dev_db') IS NULL
BEGIN
    RAISERROR('Source DB backend_net_dev_db does not exist.', 16, 1);
    RETURN;
END
GO

EXEC xp_create_subdir '/var/opt/mssql/backup';
GO

BACKUP DATABASE [backend_net_dev_db]
TO DISK = N'/var/opt/mssql/backup/backend_net_dev_db_full.bak'
WITH INIT, COPY_ONLY, COMPRESSION, STATS = 10;
GO

IF DB_ID('backend_net_db') IS NULL
BEGIN
    CREATE DATABASE [backend_net_db];
END
GO

ALTER DATABASE [backend_net_db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

RESTORE DATABASE [backend_net_db]
FROM DISK = N'/var/opt/mssql/backup/backend_net_dev_db_full.bak'
WITH REPLACE,
     RECOVERY,
     MOVE N'backend_net_dev_db' TO N'/var/opt/mssql/data/backend_net_db.mdf',
     MOVE N'backend_net_dev_db_log' TO N'/var/opt/mssql/data/backend_net_db_log.ldf',
     STATS = 10;
GO

ALTER DATABASE [backend_net_db] SET MULTI_USER;
GO
