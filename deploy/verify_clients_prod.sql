SELECT COUNT(*) AS ActiveClients
FROM Clients
WHERE IsDeleted = 0;

SELECT CustomerNo, CompanyName, Status
FROM Clients
WHERE IsDeleted = 0
ORDER BY CreatedAt;

