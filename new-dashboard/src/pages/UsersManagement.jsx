import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import Loader from '../components/Loader';
import { usersAPI, authAPI } from '../services/api';

const UsersManagement = () => {
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Modal states
  const [showUserModal, setShowUserModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);
  const [isEditing, setIsEditing] = useState(false);

  // Form state
  const [userForm, setUserForm] = useState({
    name: '',
    email: '',
    password: '',
    roleId: '',
    isActive: true
  });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    await Promise.all([loadUsers(), loadRoles()]);
  };

  const loadUsers = async () => {
    try {
      setLoading(true);
      const response = await usersAPI.getUsers();
      if (response.success && response.data) {
        setUsers(response.data);
      }
    } catch (error) {
      console.error('Failed to load users:', error);
      setError('Failed to load users');
    } finally {
      setLoading(false);
    }
  };

  const loadRoles = async () => {
    try {
      const response = await authAPI.getRoles();
      if (response.success && response.data) {
        setRoles(response.data);
      }
    } catch (error) {
      console.error('Failed to load roles:', error);
    }
  };

  const handleCreateUser = () => {
    setUserForm({
      name: '',
      email: '',
      password: '',
      roleId: roles.length > 0 ? roles[0].id : '',
      isActive: true
    });
    setIsEditing(false);
    setSelectedUser(null);
    setShowUserModal(true);
  };

  const handleEditUser = (user) => {
    setUserForm({
      name: user.name,
      email: user.email,
      password: '',
      roleId: user.roleId,
      isActive: user.isActive
    });
    setIsEditing(true);
    setSelectedUser(user);
    setShowUserModal(true);
  };

  const handleSaveUser = async () => {
    try {
      setLoading(true);
      setError('');

      if (!userForm.name.trim()) {
        setError('Name is required');
        return;
      }

      if (!userForm.email.trim()) {
        setError('Email is required');
        return;
      }

      if (!isEditing && !userForm.password.trim()) {
        setError('Password is required for new users');
        return;
      }

      if (!userForm.roleId) {
        setError('Role is required');
        return;
      }

      let response;
      if (isEditing) {
        response = await usersAPI.updateUser(selectedUser.id, userForm);
      } else {
        response = await usersAPI.createUser(userForm);
      }

      if (response.success) {
        setSuccess(isEditing ? 'User updated successfully' : 'User created successfully');
        setShowUserModal(false);
        await loadUsers();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to save user');
      }
    } catch (err) {
      setError('An error occurred while saving user');
      console.error('Failed to save user:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteUser = async (id) => {
    if (!window.confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
      return;
    }

    try {
      setLoading(true);
      const response = await usersAPI.deleteUser(id);
      if (response.success) {
        setSuccess('User deleted successfully');
        await loadUsers();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to delete user');
      }
    } catch (err) {
      setError('An error occurred while deleting user');
      console.error('Failed to delete user:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Users Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Users</li>
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

      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-header">
              <div className="row align-items-center">
                <div className="col">
                  <h4 className="card-title mb-0">Manage Users</h4>
                  <p className="text-muted mb-0 small">Create, edit, and manage system users</p>
                </div>
              </div>
            </div>
            <div className="card-body">
              {loading && users.length === 0 ? (
                <Loader fullScreen color="primary" />
              ) : (
                <DataTable
                  data={users}
                  columns={[
                    {
                      key: 'name',
                      header: 'Name',
                      render: (value, row) => (
                        <div className="d-flex align-items-center">
                          <div className="avatar-sm rounded-circle bg-soft-primary me-2">
                            <span className="avatar-title text-primary">
                              {row.name.charAt(0).toUpperCase()}
                            </span>
                          </div>
                          <strong>{row.name}</strong>
                        </div>
                      )
                    },
                    { key: 'email', header: 'Email' },
                    {
                      key: 'roleName',
                      header: 'Role',
                      render: (value) => (
                        <span className="badge bg-info">{value || 'N/A'}</span>
                      )
                    },
                    {
                      key: 'isActive',
                      header: 'Status',
                      cellStyle: { textAlign: 'center' },
                      render: (value) => (
                        value ? (
                          <span className="badge bg-success">Active</span>
                        ) : (
                          <span className="badge bg-danger">Inactive</span>
                        )
                      )
                    },
                    {
                      key: 'createdAt',
                      header: 'Created',
                      cellStyle: { textAlign: 'center' },
                      render: (value) => (
                        <small>{new Date(value).toLocaleDateString()}</small>
                      )
                    },
                    {
                      key: 'actions',
                      header: 'Actions',
                      cellStyle: { textAlign: 'center' },
                      render: (value, row) => (
                        <div className="d-flex gap-1 justify-content-center">
                          <button
                            className="btn btn-sm btn-warning"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleEditUser(row);
                            }}
                            title="Edit User"
                          >
                            <i className="fas fa-edit"></i>
                          </button>
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDeleteUser(row.id);
                            }}
                            title="Delete User"
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
                  searchPlaceholder="Search users..."
                  emptyMessage="No users found"
                  actions={
                    <button className="btn btn-primary btn-sm" onClick={handleCreateUser}>
                      <i className="fas fa-plus me-1"></i>Add New User
                    </button>
                  }
                />
              )}
            </div>
          </div>
        </div>
      </div>

      {/* User Modal */}
      {showUserModal && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">{isEditing ? 'Edit User' : 'Create User'}</h5>
                <button type="button" className="btn-close" onClick={() => setShowUserModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Name <span className="text-danger">*</span></label>
                  <input
                    type="text"
                    className="form-control"
                    value={userForm.name}
                    onChange={(e) => setUserForm({ ...userForm, name: e.target.value })}
                    placeholder="Enter full name"
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Email <span className="text-danger">*</span></label>
                  <input
                    type="email"
                    className="form-control"
                    value={userForm.email}
                    onChange={(e) => setUserForm({ ...userForm, email: e.target.value })}
                    placeholder="Enter email address"
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">
                    Password {!isEditing && <span className="text-danger">*</span>}
                    {isEditing && <small className="text-muted">(Leave blank to keep current)</small>}
                  </label>
                  <input
                    type="password"
                    className="form-control"
                    value={userForm.password}
                    onChange={(e) => setUserForm({ ...userForm, password: e.target.value })}
                    placeholder="Enter password"
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Role <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={userForm.roleId}
                    onChange={(e) => setUserForm({ ...userForm, roleId: parseInt(e.target.value) })}
                    required
                  >
                    <option value="">Select Role</option>
                    {roles.map((role) => (
                      <option key={role.id} value={role.id}>
                        {role.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <div className="form-check form-switch">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="isActive"
                      checked={userForm.isActive}
                      onChange={(e) => setUserForm({ ...userForm, isActive: e.target.checked })}
                    />
                    <label className="form-check-label" htmlFor="isActive">
                      Active User
                    </label>
                  </div>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowUserModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleSaveUser} disabled={loading}>
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

export default UsersManagement;
