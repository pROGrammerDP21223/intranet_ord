SELECT COUNT(*) AS BeforeClients FROM Clients WHERE IsDeleted = 0;

DECLARE @sql nvarchar(max)=N'
SELECT * INTO Clients_backup_' + CONVERT(varchar(8), GETDATE(), 112) + '_' + REPLACE(CONVERT(varchar(8), GETDATE(), 108), ':', '') + N'
FROM Clients;
';
EXEC(@sql);

SELECT TOP 5 CustomerNo, CompanyName, CreatedAt
FROM Clients
ORDER BY CreatedAt DESC;

