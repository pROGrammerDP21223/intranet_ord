import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import { authAPI, permissionsAPI } from '../services/api';

const PermissionsManagement = () => {
  const [activeTab, setActiveTab] = useState('roles'); // 'roles' or 'permissions'
  const [roles, setRoles] = useState([]);
  const [permissions, setPermissions] = useState([]);
  const [groupedPermissions, setGroupedPermissions] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Modal states
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [showPermissionsModal, setShowPermissionsModal] = useState(false);
  const [showPermissionModal, setShowPermissionModal] = useState(false);
  const [selectedRole, setSelectedRole] = useState(null);
  const [selectedPermissions, setSelectedPermissions] = useState([]);
  const [selectedPermission, setSelectedPermission] = useState(null);

  // Form states
  const [roleForm, setRoleForm] = useState({
    name: '',
    description: ''
  });
  const [permissionForm, setPermissionForm] = useState({
    name: '',
    description: '',
    category: ''
  });
  const [isEditing, setIsEditing] = useState(false);
  const [isEditingPermission, setIsEditingPermission] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    await Promise.all([loadRoles(), loadPermissions()]);
  };

  const loadRoles = async () => {
    try {
      setLoading(true);
      const response = await authAPI.getRoles();
      if (response.success && response.data) {
        // Normalize permissions array - handle both formats
        const normalizedRoles = response.data.map(role => ({
          ...role,
          permissions: role.permissions || role.Permissions || [],
          id: role.id || role.Id,
          name: role.name || role.Name,
          description: role.description || role.Description
        }));
        console.log('Loaded roles with permissions:', normalizedRoles);
        setRoles(normalizedRoles);
      }
    } catch (error) {
      console.error('Failed to load roles:', error);
      setError('Failed to load roles');
    } finally {
      setLoading(false);
    }
  };

  const loadPermissions = async () => {
    try {
      const response = await permissionsAPI.getPermissionsByCategory();
      if (response.success && response.data) {
        const grouped = {};
        response.data.forEach(group => {
          grouped[group.category] = group.permissions;
        });
        setGroupedPermissions(grouped);
        // Flatten for checkbox list
        const flat = response.data.flatMap(group =>
          group.permissions.map(p => ({ ...p, category: group.category }))
        );
        setPermissions(flat);
      }
    } catch (error) {
      console.error('Failed to load permissions:', error);
    }
  };

  const handleCreateRole = () => {
    setRoleForm({ name: '', description: '' });
    setIsEditing(false);
    setSelectedRole(null);
    setShowRoleModal(true);
  };

  const handleEditRole = (role) => {
    setRoleForm({
      name: role.name,
      description: role.description || ''
    });
    setIsEditing(true);
    setSelectedRole(role);
    setShowRoleModal(true);
  };

  const handleSaveRole = async () => {
    try {
      setLoading(true);
      setError('');

      if (!roleForm.name.trim()) {
        setError('Role name is required');
        return;
      }

      let response;
      if (isEditing) {
        response = await authAPI.updateRole(selectedRole.id, roleForm);
      } else {
        response = await authAPI.createRole(roleForm);
      }

      if (response.success) {
        setSuccess(isEditing ? 'Role updated successfully' : 'Role created successfully');
        setShowRoleModal(false);
        await loadRoles();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to save role');
      }
    } catch (err) {
      setError('An error occurred while saving role');
      console.error('Failed to save role:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreatePermission = () => {
    setPermissionForm({ name: '', description: '', category: '' });
    setIsEditingPermission(false);
    setSelectedPermission(null);
    setShowPermissionModal(true);
  };

  const handleEditPermission = (permission) => {
    setPermissionForm({
      name: permission.name,
      description: permission.description || '',
      category: permission.category || ''
    });
    setIsEditingPermission(true);
    setSelectedPermission(permission);
    setShowPermissionModal(true);
  };

  const handleSavePermission = async () => {
    try {
      setLoading(true);
      setError('');

      if (!permissionForm.name.trim()) {
        setError('Permission name is required');
        return;
      }

      if (!permissionForm.category.trim()) {
        setError('Category is required');
        return;
      }

      let response;
      if (isEditingPermission) {
        response = await permissionsAPI.updatePermission(selectedPermission.id, permissionForm);
      } else {
        response = await permissionsAPI.createPermission(permissionForm);
      }

      if (response.success) {
        setSuccess(`Permission ${isEditingPermission ? 'updated' : 'created'} successfully`);
        setShowPermissionModal(false);
        setSelectedPermission(null);
        loadPermissions();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || `Failed to ${isEditingPermission ? 'update' : 'create'} permission`);
      }
    } catch (err) {
      setError(`An error occurred while ${isEditingPermission ? 'updating' : 'creating'} permission`);
      console.error('Permission error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeletePermission = async (id) => {
    if (!window.confirm('Are you sure you want to delete this permission? This action cannot be undone.')) {
      return;
    }

    try {
      setLoading(true);
      setError('');
      const response = await permissionsAPI.deletePermission(id);
      if (response.success) {
        setSuccess('Permission deleted successfully');
        loadPermissions();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to delete permission');
      }
    } catch (err) {
      setError('An error occurred while deleting permission');
      console.error('Delete permission error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteRole = async (id) => {
    if (!window.confirm('Are you sure you want to delete this role? This action cannot be undone.')) {
      return;
    }

    try {
      setLoading(true);
      const response = await authAPI.deleteRole(id);
      if (response.success) {
        setSuccess('Role deleted successfully');
        await loadRoles();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to delete role');
      }
    } catch (err) {
      setError('An error occurred while deleting role');
      console.error('Failed to delete role:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleManagePermissions = async (role) => {
    setSelectedRole(role);
    setShowPermissionsModal(true);
    
    // Fetch fresh role data with permissions to ensure we have the latest
    try {
      const roleId = role.id || role.Id;
      const response = await authAPI.getRoleById(roleId);
      console.log('Role response:', response);
      
      if (response.success && response.data) {
        const roleWithPermissions = response.data;
        // Normalize permissions - handle both formats
        const permissions = roleWithPermissions.permissions || roleWithPermissions.Permissions || [];
        console.log('Role permissions:', permissions);
        
        // Get current permissions for this role - handle both id and Id
        const currentPermissionIds = permissions.map(p => {
          const permId = p.id || p.Id;
          console.log('Permission ID:', permId, 'Permission:', p);
          return permId;
        }).filter(id => id !== undefined && id !== null);
        
        console.log('Current permission IDs:', currentPermissionIds);
        setSelectedPermissions(currentPermissionIds);
        
        // Update selected role with fresh data
        setSelectedRole({
          ...roleWithPermissions,
          id: roleWithPermissions.id || roleWithPermissions.Id,
          permissions: permissions
        });
      } else {
        // Fallback to role from list
        const permissions = role.permissions || role.Permissions || [];
        const currentPermissionIds = permissions.map(p => p.id || p.Id).filter(id => id !== undefined);
        console.log('Using fallback permissions:', currentPermissionIds);
        setSelectedPermissions(currentPermissionIds);
      }
    } catch (error) {
      console.error('Failed to load role permissions:', error);
      // Fallback to role from list
      const permissions = role.permissions || role.Permissions || [];
      const currentPermissionIds = permissions.map(p => p.id || p.Id).filter(id => id !== undefined);
      setSelectedPermissions(currentPermissionIds);
    }
  };

  const handleTogglePermission = (permissionId) => {
    setSelectedPermissions(prev => {
      if (prev.includes(permissionId)) {
        return prev.filter(id => id !== permissionId);
      } else {
        return [...prev, permissionId];
      }
    });
  };

  const handleSavePermissions = async () => {
    try {
      setLoading(true);
      setError('');

      const response = await authAPI.assignPermissions(selectedRole.id, selectedPermissions);

      if (response.success) {
        setSuccess('Permissions assigned successfully');
        setShowPermissionsModal(false);
        await loadRoles();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to assign permissions');
      }
    } catch (err) {
      setError('An error occurred while assigning permissions');
      console.error('Failed to assign permissions:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Permissions Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Permissions</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {error}
          <button type="button" className="btn-close" onClick={() => setError('')}></button>
        </div>
      )}

      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          {success}
          <button type="button" className="btn-close" onClick={() => setSuccess('')}></button>
        </div>
      )}

      {/* Tab Navigation */}
      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-body">
              <ul className="nav nav-tabs nav-tabs-custom nav-justified" role="tablist">
                <li className="nav-item">
                  <a
                    className={`nav-link ${activeTab === 'roles' ? 'active' : ''}`}
                    onClick={() => setActiveTab('roles')}
                    style={{ cursor: 'pointer' }}
                  >
                    <i className="fas fa-user-tag me-2"></i>
                    <span className="d-none d-sm-inline">Roles Management</span>
                  </a>
                </li>
                <li className="nav-item">
                  <a
                    className={`nav-link ${activeTab === 'permissions' ? 'active' : ''}`}
                    onClick={() => setActiveTab('permissions')}
                    style={{ cursor: 'pointer' }}
                  >
                    <i className="fas fa-key me-2"></i>
                    <span className="d-none d-sm-inline">Permissions Management</span>
                  </a>
                </li>
              </ul>

              <div className="tab-content p-3">
                {/* Roles Tab */}
                {activeTab === 'roles' && (
                  <div className="tab-pane active">
                    <div className="row align-items-center mb-3">
                      <div className="col">
                        <h5 className="mb-0">Manage Roles</h5>
                        <p className="text-muted mb-0 small">Create, edit, delete roles and assign permissions</p>
                      </div>
                      <div className="col-auto">
                        <button className="btn btn-primary btn-sm" onClick={handleCreateRole}>
                          <i className="fas fa-plus me-1"></i>Add New Role
                        </button>
                      </div>
                    </div>

                    {loading && roles.length === 0 ? (
                      <div className="text-center py-5">
                        <div className="spinner-border text-primary" role="status">
                          <span className="visually-hidden">Loading...</span>
                        </div>
                      </div>
                    ) : roles.length === 0 ? (
                      <div className="text-center py-5">
                        <i className="fas fa-inbox fa-3x text-muted mb-3 d-block"></i>
                        <p className="text-muted mb-3">No roles found.</p>
                        <button className="btn btn-primary btn-sm" onClick={handleCreateRole}>
                          <i className="fas fa-plus me-1"></i>Add New Role
                        </button>
                      </div>
                    ) : (
                      <div className="table-responsive">
                        <table className="table table-bordered table-hover align-middle mb-0">
                          <thead className="table-light">
                            <tr>
                              <th style={{ width: '25%' }}>Role Name</th>
                              <th style={{ width: '35%' }}>Description</th>
                              <th className="text-center" style={{ width: '15%' }}>Permissions</th>
                              <th className="text-center" style={{ width: '25%' }}>Actions</th>
                            </tr>
                          </thead>
                          <tbody>
                            {roles.map((role) => (
                              <tr key={role.id}>
                                <td>
                                  <strong className="text-primary">{role.name}</strong>
                                </td>
                                <td>{role.description || '-'}</td>
                                <td className="text-center">
                                  <span className="badge bg-info">
                                    {role.permissions && Array.isArray(role.permissions) ? role.permissions.length : 0}
                                  </span>
                                </td>
                                <td className="text-center">
                                  <div className="d-flex gap-2 justify-content-center">
                                    <button
                                      className="btn btn-sm btn-info"
                                      onClick={() => handleManagePermissions(role)}
                                      title="Manage Permissions"
                                    >
                                      <i className="fas fa-key me-1"></i>Permissions
                                    </button>
                                    <button
                                      className="btn btn-sm btn-warning"
                                      onClick={() => handleEditRole(role)}
                                      title="Edit Role"
                                    >
                                      <i className="fas fa-edit"></i>
                                    </button>
                                    <button
                                      className="btn btn-sm btn-danger"
                                      onClick={() => handleDeleteRole(role.id)}
                                      title="Delete Role"
                                    >
                                      <i className="fas fa-trash"></i>
                                    </button>
                                  </div>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    )}
                  </div>
                )}

                {/* Permissions Tab */}
                {activeTab === 'permissions' && (
                  <div className="tab-pane active">
                    <div className="row align-items-center mb-3">
                      <div className="col">
                        <h5 className="mb-0">Manage Permissions</h5>
                        <p className="text-muted mb-0 small">Create, edit, and delete system permissions</p>
                      </div>
                      <div className="col-auto">
                        <button className="btn btn-primary btn-sm" onClick={handleCreatePermission}>
                          <i className="fas fa-plus me-1"></i>Add New Permission
                        </button>
                      </div>
                    </div>

                    {Object.keys(groupedPermissions).length === 0 ? (
                      <div className="text-center py-5">
                        <i className="fas fa-inbox fa-3x text-muted mb-3 d-block"></i>
                        <p className="text-muted mb-3">No permissions found.</p>
                        <button className="btn btn-primary btn-sm" onClick={handleCreatePermission}>
                          <i className="fas fa-plus me-1"></i>Add New Permission
                        </button>
                      </div>
                    ) : (
                      <div className="accordion" id="permissionsAccordion">
                        {Object.entries(groupedPermissions).map(([category, perms], index) => (
                          <div className="accordion-item" key={category}>
                            <h2 className="accordion-header">
                              <button
                                className={`accordion-button ${index === 0 ? '' : 'collapsed'}`}
                                type="button"
                                data-bs-toggle="collapse"
                                data-bs-target={`#collapse${category}`}
                              >
                                <i className="fas fa-folder me-2"></i>
                                <strong className="text-capitalize">{category}</strong>
                                <span className="badge bg-secondary ms-2">{perms.length}</span>
                              </button>
                            </h2>
                            <div
                              id={`collapse${category}`}
                              className={`accordion-collapse collapse ${index === 0 ? 'show' : ''}`}
                              data-bs-parent="#permissionsAccordion"
                            >
                              <div className="accordion-body">
                                <div className="table-responsive">
                                  <table className="table table-sm table-hover mb-0">
                                    <thead className="table-light">
                                      <tr>
                                        <th style={{ width: '30%' }}>Permission Name</th>
                                        <th style={{ width: '50%' }}>Description</th>
                                        <th className="text-center" style={{ width: '20%' }}>Actions</th>
                                      </tr>
                                    </thead>
                                    <tbody>
                                      {perms.map((perm) => (
                                        <tr key={perm.id}>
                                          <td><code>{perm.name}</code></td>
                                          <td>{perm.description || '-'}</td>
                                          <td className="text-center">
                                            <div className="d-flex gap-1 justify-content-center">
                                              <button
                                                className="btn btn-sm btn-warning"
                                                onClick={() => handleEditPermission(perm)}
                                                title="Edit Permission"
                                              >
                                                <i className="fas fa-edit"></i>
                                              </button>
                                              <button
                                                className="btn btn-sm btn-danger"
                                                onClick={() => handleDeletePermission(perm.id)}
                                                title="Delete Permission"
                                              >
                                                <i className="fas fa-trash"></i>
                                              </button>
                                            </div>
                                          </td>
                                        </tr>
                                      ))}
                                    </tbody>
                                  </table>
                                </div>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Role Modal */}
      {showRoleModal && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">{isEditing ? 'Edit Role' : 'Create Role'}</h5>
                <button type="button" className="btn-close" onClick={() => setShowRoleModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Role Name <span className="text-danger">*</span></label>
                  <input
                    type="text"
                    className="form-control"
                    value={roleForm.name}
                    onChange={(e) => setRoleForm({ ...roleForm, name: e.target.value })}
                    placeholder="Enter role name"
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Description</label>
                  <textarea
                    className="form-control"
                    rows="3"
                    value={roleForm.description}
                    onChange={(e) => setRoleForm({ ...roleForm, description: e.target.value })}
                    placeholder="Enter role description"
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowRoleModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleSaveRole} disabled={loading}>
                  {loading ? 'Saving...' : 'Save'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Assign Permissions Modal */}
      {showPermissionsModal && selectedRole && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Assign Permissions - {selectedRole.name}</h5>
                <button type="button" className="btn-close" onClick={() => setShowPermissionsModal(false)}></button>
              </div>
              <div className="modal-body" style={{ maxHeight: '60vh', overflowY: 'auto' }}>
                <div className="alert alert-info">
                  <i className="fas fa-info-circle me-2"></i>
                  Select permissions to assign to this role. Currently selected: <strong>{selectedPermissions.length}</strong>
                </div>
                <div className="row">
                  {Object.entries(groupedPermissions).map(([category, perms]) => (
                    <div key={category} className="col-md-6 mb-3">
                      <div className="card border">
                        <div className="card-header bg-light">
                          <h6 className="mb-0 text-capitalize">
                            <i className="fas fa-folder me-2"></i>{category}
                          </h6>
                        </div>
                        <div className="card-body">
                          {perms.map((perm) => (
                            <div key={perm.id} className="form-check mb-2">
                              <input
                                className="form-check-input"
                                type="checkbox"
                                checked={selectedPermissions.includes(perm.id) || selectedPermissions.includes(perm.Id)}
                                onChange={() => handleTogglePermission(perm.id || perm.Id)}
                                id={`perm-${perm.id || perm.Id}`}
                              />
                              <label className="form-check-label" htmlFor={`perm-${perm.id || perm.Id}`}>
                                <strong>{perm.name || perm.Name}</strong>
                                {(perm.description || perm.Description) && (
                                  <small className="text-muted d-block">{perm.description || perm.Description}</small>
                                )}
                              </label>
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowPermissionsModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleSavePermissions} disabled={loading}>
                  {loading ? 'Saving...' : `Assign (${selectedPermissions.length} selected)`}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Permission Modal */}
      {showPermissionModal && (
        <div className="modal show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {isEditingPermission ? 'Edit Permission' : 'Add New Permission'}
                </h5>
                <button type="button" className="btn-close" onClick={() => setShowPermissionModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Permission Name <span className="text-danger">*</span></label>
                  <input
                    type="text"
                    className="form-control"
                    value={permissionForm.name}
                    onChange={(e) => setPermissionForm({ ...permissionForm, name: e.target.value })}
                    placeholder="e.g., view:clients"
                    required
                  />
                  <small className="text-muted">Format: action:resource (e.g., view:clients, create:users)</small>
                </div>
                <div className="mb-3">
                  <label className="form-label">Description</label>
                  <textarea
                    className="form-control"
                    rows="2"
                    value={permissionForm.description}
                    onChange={(e) => setPermissionForm({ ...permissionForm, description: e.target.value })}
                    placeholder="Permission description"
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Category <span className="text-danger">*</span></label>
                  <input
                    type="text"
                    className="form-control"
                    value={permissionForm.category}
                    onChange={(e) => setPermissionForm({ ...permissionForm, category: e.target.value })}
                    placeholder="e.g., clients, users, admin"
                    required
                  />
                  <small className="text-muted">Category for grouping permissions</small>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowPermissionModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleSavePermission} disabled={loading}>
                  {loading ? 'Saving...' : (isEditingPermission ? 'Update' : 'Create')}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Modal Backdrop */}
      {(showRoleModal || showPermissionsModal || showPermissionModal) && (
        <div className="modal-backdrop fade show" onClick={() => {
          setShowRoleModal(false);
          setShowPermissionsModal(false);
          setShowPermissionModal(false);
        }}></div>
      )}
    </Layout>
  );
};

export default PermissionsManagement;
