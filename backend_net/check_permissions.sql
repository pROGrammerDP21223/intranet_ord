-- Check RolePermissions table
SELECT 
    rp.Id,
    rp.RoleId,
    rp.PermissionId,
    rp.IsDeleted,
    rp.CreatedAt,
    rp.UpdatedAt,
    r.Name AS RoleName,
    p.Name AS PermissionName
FROM RolePermissions rp
LEFT JOIN Roles r ON rp.RoleId = r.Id
LEFT JOIN Permissions p ON rp.PermissionId = p.Id
ORDER BY rp.RoleId, rp.PermissionId;

-- Count RolePermissions by Role
SELECT 
    r.Id AS RoleId,
    r.Name AS RoleName,
    COUNT(CASE WHEN rp.IsDeleted = 0 THEN 1 END) AS ActivePermissions,
    COUNT(CASE WHEN rp.IsDeleted = 1 THEN 1 END) AS DeletedPermissions,
    COUNT(*) AS TotalPermissions
FROM Roles r
LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId
WHERE r.IsDeleted = 0
GROUP BY r.Id, r.Name
ORDER BY r.Id;

-- Check if any RolePermissions exist at all
SELECT COUNT(*) AS TotalRolePermissions FROM RolePermissions;
SELECT COUNT(*) AS ActiveRolePermissions FROM RolePermissions WHERE IsDeleted = 0;

-- Check Permissions table
SELECT Id, Name, Category, IsDeleted FROM Permissions ORDER BY Id;

