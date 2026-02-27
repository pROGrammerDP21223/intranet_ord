-- Test query to see what should be returned
SELECT 
    r.Id AS RoleId,
    r.Name AS RoleName,
    rp.Id AS RolePermissionId,
    rp.PermissionId,
    rp.IsDeleted AS RolePermissionDeleted,
    p.Id AS PermissionId,
    p.Name AS PermissionName,
    p.IsDeleted AS PermissionDeleted
FROM Roles r
LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId AND rp.IsDeleted = 0
LEFT JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.IsDeleted = 0
ORDER BY r.Id, rp.PermissionId;

-- Check specific role (Admin = 1)
SELECT 
    r.Id AS RoleId,
    r.Name AS RoleName,
    rp.Id AS RolePermissionId,
    rp.PermissionId,
    rp.IsDeleted AS RolePermissionDeleted,
    p.Id AS PermissionId,
    p.Name AS PermissionName,
    p.IsDeleted AS PermissionDeleted
FROM Roles r
LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId AND rp.IsDeleted = 0
LEFT JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.Id = 1 AND r.IsDeleted = 0;

