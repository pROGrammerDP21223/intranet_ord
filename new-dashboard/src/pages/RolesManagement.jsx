import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import Loader from '../components/Loader';
import { authAPI } from '../services/api';

const RolesManagement = () => {
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Modal states
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [selectedRole, setSelectedRole] = useState(null);

  // Form states
  const [roleForm, setRoleForm] = useState({
    name: '',
    description: ''
  });
  const [isEditing, setIsEditing] = useState(false);

  useEffect(() => {
    loadRoles();
  }, []);

  const loadRoles = async () => {
    try {
      setLoading(true);
      const response = await authAPI.getRoles();
      if (response.success && response.data) {
        const normalizedRoles = response.data.map(role => ({
          ...role,
          id: role.id || role.Id,
          name: role.name || role.Name,
          description: role.description || role.Description
        }));
        setRoles(normalizedRoles);
      }
    } catch (error) {
      console.error('Failed to load roles:', error);
      setError('Failed to load roles');
    } finally {
      setLoading(false);
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
      if (isEditing && selectedRole) {
        response = await authAPI.updateRole(selectedRole.id, {
          name: roleForm.name,
          description: roleForm.description
        });
      } else {
        response = await authAPI.createRole({
          name: roleForm.name,
          description: roleForm.description
        });
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
      setError(err.response?.data?.message || 'An error occurred while saving role');
      console.error('Failed to save role:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteRole = async (roleId) => {
    if (!window.confirm('Are you sure you want to delete this role? This action cannot be undone.')) {
      return;
    }

    try {
      setLoading(true);
      setError('');
      const response = await authAPI.deleteRole(roleId);
      if (response.success) {
        setSuccess('Role deleted successfully');
        await loadRoles();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to delete role');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while deleting role');
      console.error('Failed to delete role:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Roles Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Roles</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {/* Alerts */}
      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          <i className="fas fa-exclamation-circle me-2"></i>
          {error}
          <button type="button" className="btn-close" onClick={() => setError('')}></button>
        </div>
      )}

      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          <i className="fas fa-check-circle me-2"></i>
          {success}
          <button type="button" className="btn-close" onClick={() => setSuccess('')}></button>
        </div>
      )}

      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-header">
              <div className="row align-items-center">
                <div className="col">
                  <h4 className="card-title mb-0">Roles</h4>
                  <p className="text-muted mb-0">Manage user roles</p>
                </div>
              </div>
            </div>
            <div className="card-body">
              {loading && roles.length === 0 ? (
                <Loader fullScreen color="primary" />
              ) : (
                <DataTable
                  data={roles}
                  columns={[
                    {
                      key: 'name',
                      header: 'Role Name',
                      render: (value) => <strong className="text-primary">{value}</strong>
                    },
                    { key: 'description', header: 'Description', render: (value) => value || '-' },
                    {
                      key: 'actions',
                      header: 'Actions',
                      cellStyle: { textAlign: 'center', width: '150px' },
                      render: (value, row) => (
                        <div className="d-flex gap-1 justify-content-center">
                          <button
                            className="btn btn-sm btn-info"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleEditRole(row);
                            }}
                            title="Edit"
                          >
                            <i className="fas fa-edit"></i>
                          </button>
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDeleteRole(row.id);
                            }}
                            title="Delete"
                            disabled={loading}
                          >
                            <i className="fas fa-trash"></i>
                          </button>
                        </div>
                      )
                    }
                  ]}
                  pageSize={10}
                  showPagination={true}
                  showSearch={true}
                  searchPlaceholder="Search roles..."
                  emptyMessage="No roles found"
                  actions={
                    <button className="btn btn-primary btn-sm" onClick={handleCreateRole}>
                      <i className="fas fa-plus me-1"></i>Add New Role
                    </button>
                  }
                />
              )}
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
    </Layout>
  );
};

export default RolesManagement;

