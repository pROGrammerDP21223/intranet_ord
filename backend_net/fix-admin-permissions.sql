-- Fix Admin Permissions - Assign all permissions to Admin role
-- This script will give the Admin role all available permissions

-- Step 1: Soft delete any existing admin permissions (to start fresh)
UPDATE RolePermissions
SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
WHERE RoleId = (SELECT Id FROM Roles WHERE Name = 'Admin' AND IsDeleted = 0)
  AND IsDeleted = 0;

-- Step 2: Insert all permissions for Admin role
INSERT INTO RolePermissions (RoleId, PermissionId, IsDeleted, CreatedAt, UpdatedAt)
SELECT
    (SELECT Id FROM Roles WHERE Name = 'Admin' AND IsDeleted = 0) AS RoleId,
    p.Id AS PermissionId,
    0 AS IsDeleted,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt
FROM Permissions p
WHERE p.IsDeleted = 0
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = (SELECT Id FROM Roles WHERE Name = 'Admin' AND IsDeleted = 0)
      AND rp.PermissionId = p.Id
  );

-- Step 3: Verify - Show all permissions assigned to Admin
SELECT
    r.Name as RoleName,
    p.Name as PermissionName,
    p.Category,
    p.Description
FROM Roles r
JOIN RolePermissions rp ON r.Id = rp.RoleId AND rp.IsDeleted = 0
JOIN Permissions p ON rp.PermissionId = p.Id AND p.IsDeleted = 0
WHERE r.Name = 'Admin' AND r.IsDeleted = 0
ORDER BY p.Category, p.Name;

-- Show count
SELECT COUNT(*) as TotalAdminPermissions
FROM RolePermissions rp
WHERE rp.RoleId = (SELECT Id FROM Roles WHERE Name = 'Admin' AND IsDeleted = 0)
  AND rp.IsDeleted = 0;
