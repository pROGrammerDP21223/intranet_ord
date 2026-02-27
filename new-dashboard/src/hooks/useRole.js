import { useAuth } from '../contexts/AuthContext';

export const useRole = () => {
  const { user } = useAuth();
  
  const roleName = user?.roleName || user?.role?.name || null;
  
  const isAdmin = () => roleName === 'Admin' || roleName === 'Owner';
  const isOwner = () => roleName === 'Owner';
  const isSalesManager = () => roleName === 'Sales Manager';
  const isSalesPerson = () => roleName === 'Sales Person';
  const isClient = () => roleName === 'Client';
  const isHOD = () => roleName === 'HOD';
  const isCallingStaff = () => roleName === 'Calling Staff';
  const isEmployee = () => roleName === 'Employee';
  
  const canViewAll = () => isAdmin() || isHOD() || isCallingStaff() || isEmployee();
  const canEdit = () => isAdmin();
  const canDelete = () => isAdmin();
  const canCreate = () => isAdmin();
  const canCreateClient = () => isAdmin() || isSalesPerson() || isSalesManager();
  const canManageProducts = () => isAdmin();
  const canManageCategories = () => isAdmin();
  const canManageIndustries = () => isAdmin();
  const canManageUsers = () => isAdmin();
  const canManageRelationships = () => isAdmin();
  const canAttachProducts = () => isAdmin() || isSalesPerson() || isSalesManager();
  const canViewTransactions = () => !isEmployee(); // Employee cannot view transactions
  
  return {
    roleName,
    isAdmin: isAdmin(),
    isOwner: isOwner(),
    isSalesManager: isSalesManager(),
    isSalesPerson: isSalesPerson(),
    isClient: isClient(),
    isHOD: isHOD(),
    isCallingStaff: isCallingStaff(),
    isEmployee: isEmployee(),
    canViewAll: canViewAll(),
    canEdit: canEdit(),
    canDelete: canDelete(),
    canCreate: canCreate(),
    canCreateClient: canCreateClient(),
    canManageProducts: canManageProducts(),
    canManageCategories: canManageCategories(),
    canManageIndustries: canManageIndustries(),
    canManageUsers: canManageUsers(),
    canManageRelationships: canManageRelationships(),
    canAttachProducts: canAttachProducts(),
    canViewTransactions: canViewTransactions(),
  };
};

