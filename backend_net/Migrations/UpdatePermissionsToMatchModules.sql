-- Migration Script: Update Permissions to Match Current Modules
-- This script updates permissions to match the actual modules in the application

-- Step 1: Remove old permissions that don't match current modules
-- (Users, Sales, Reports modules don't exist in current application)

-- Step 2: Add new permissions for Services and Roles/Permissions modules

-- Note: This is a reference script. 
-- In production, you should:
-- 1. Create a migration using: dotnet ef migrations add UpdatePermissionsToMatchModules
-- 2. Or manually update the database using this script
-- 3. Or delete all permissions and re-run the seeder (if no production data exists)

-- Option 1: Delete and re-seed (if no production data)
-- DELETE FROM RolePermissions;
-- DELETE FROM Permissions;
-- Then restart application to re-seed

-- Option 2: Manual update (if production data exists)
-- Update existing permissions and add new ones

-- For fresh database, the updated seeder will handle everything automatically.

