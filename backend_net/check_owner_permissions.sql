-- Check Owner role (ID = 2) permissions
SELECT 
    r.Id AS RoleId,
    r.Name AS RoleName,
    rp.Id AS RolePermissionId,
    rp.PermissionId,
    rp.IsDeleted AS RolePermissionDeleted,
    rp.CreatedAt,
    rp.UpdatedAt,
    p.Id AS PermissionId,
    p.Name AS PermissionName,
    p.IsDeleted AS PermissionDeleted
FROM Roles r
LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId
LEFT JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.Id = 2
ORDER BY rp.Id;

-- Check if view:clients permission exists
SELECT Id, Name, IsDeleted FROM Permissions WHERE Name = 'view:clients';

