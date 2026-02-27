import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import Loader from '../components/Loader';
import { apiKeyAPI, clientAPI } from '../services/api';
import { useRole } from '../hooks/useRole';

const APIKeysManagement = () => {
  const role = useRole();
  const [apiKeys, setApiKeys] = useState([]);
  const [clients, setClients] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedApiKey, setSelectedApiKey] = useState(null);
  const [newApiKey, setNewApiKey] = useState(null);

  const [formData, setFormData] = useState({
    clientId: '',
    name: '',
    description: '',
    expiresAt: '',
    allowedOrigins: '',
  });

  const [editFormData, setEditFormData] = useState({
    name: '',
    description: '',
    expiresAt: '',
    allowedOrigins: '',
    isActive: true,
  });

  useEffect(() => {
    if (role.isAdmin || role.isOwner) {
      loadApiKeys();
      loadClients();
    }
  }, []);

  const loadApiKeys = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await apiKeyAPI.getAllApiKeys();
      if (response.success && response.data) {
        setApiKeys(response.data);
      } else {
        setError(response.message || 'Failed to load API keys');
      }
    } catch (err) {
      setError('An error occurred while loading API keys');
      console.error('Failed to load API keys:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadClients = async () => {
    try {
      const response = await clientAPI.getClients();
      if (response.success && response.data) {
        setClients(response.data);
      }
    } catch (err) {
      console.error('Failed to load clients:', err);
    }
  };

  const handleCreate = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await apiKeyAPI.createApiKey({
        clientId: parseInt(formData.clientId),
        name: formData.name,
        description: formData.description || null,
        expiresAt: formData.expiresAt || null,
        allowedOrigins: formData.allowedOrigins || null,
      });
      if (response.success && response.data) {
        setSuccess('API key created successfully');
        setNewApiKey(response.data);
        setShowCreateModal(false);
        resetForm();
        await loadApiKeys();
        setTimeout(() => {
          setSuccess('');
          setNewApiKey(null);
        }, 5000);
      } else {
        setError(response.message || 'Failed to create API key');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while creating API key');
      console.error('Failed to create API key:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (apiKey) => {
    setSelectedApiKey(apiKey);
    setEditFormData({
      name: apiKey.name || '',
      description: apiKey.description || '',
      expiresAt: apiKey.expiresAt ? apiKey.expiresAt.split('T')[0] : '',
      allowedOrigins: apiKey.allowedOrigins || '',
      isActive: apiKey.isActive,
    });
    setShowEditModal(true);
  };

  const handleUpdate = async () => {
    if (!selectedApiKey) return;

    try {
      setLoading(true);
      setError('');
      const response = await apiKeyAPI.updateApiKey(selectedApiKey.id, {
        name: editFormData.name,
        description: editFormData.description || null,
        expiresAt: editFormData.expiresAt || null,
        allowedOrigins: editFormData.allowedOrigins || null,
        isActive: editFormData.isActive,
      });
      if (response.success) {
        setSuccess('API key updated successfully');
        setShowEditModal(false);
        await loadApiKeys();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to update API key');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while updating API key');
      console.error('Failed to update API key:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleRevoke = async (id) => {
    if (!window.confirm('Are you sure you want to revoke this API key? It will no longer be usable.')) {
      return;
    }

    try {
      setLoading(true);
      setError('');
      const response = await apiKeyAPI.revokeApiKey(id);
      if (response.success) {
        setSuccess('API key revoked successfully');
        await loadApiKeys();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to revoke API key');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while revoking API key');
      console.error('Failed to revoke API key:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleReactivate = async (id) => {
    try {
      setLoading(true);
      setError('');
      const response = await apiKeyAPI.reactivateApiKey(id);
      if (response.success) {
        setSuccess('API key reactivated successfully');
        await loadApiKeys();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to reactivate API key');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while reactivating API key');
      console.error('Failed to reactivate API key:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this API key? This action cannot be undone.')) {
      return;
    }

    try {
      setLoading(true);
      setError('');
      const response = await apiKeyAPI.deleteApiKey(id);
      if (response.success) {
        setSuccess('API key deleted successfully');
        await loadApiKeys();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to delete API key');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while deleting API key');
      console.error('Failed to delete API key:', err);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      clientId: '',
      name: '',
      description: '',
      expiresAt: '',
      allowedOrigins: '',
    });
  };

  const formatDate = (dateString) => {
    if (!dateString) return 'Never';
    const date = new Date(dateString);
    return date.toLocaleString('en-GB');
  };

  const isExpired = (expiresAt) => {
    if (!expiresAt) return false;
    return new Date(expiresAt) < new Date();
  };

  const maskApiKey = (key) => {
    if (!key) return '';
    return key.substring(0, 8) + '...' + key.substring(key.length - 8);
  };

  if (!role.isAdmin && !role.isOwner) {
    return (
      <Layout>
        <div className="container-fluid">
          <div className="row">
            <div className="col-12">
              <div className="alert alert-danger">You don't have permission to manage API keys.</div>
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="container-fluid">
        <div className="row">
          <div className="col-12">
            <div className="page-title-box d-sm-flex align-items-center justify-content-between">
              <h4 className="mb-sm-0 font-size-18">API Keys Management</h4>
              <div className="page-title-right">
                <ol className="breadcrumb m-0">
                  <li className="breadcrumb-item"><a href="/dashboard">Dashboard</a></li>
                  <li className="breadcrumb-item active">API Keys</li>
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

        {/* New API Key Display */}
        {newApiKey && (
          <div className="alert alert-info alert-dismissible fade show" role="alert">
            <h5 className="alert-heading">API Key Created Successfully!</h5>
            <p className="mb-2"><strong>Key:</strong> <code>{newApiKey.key}</code></p>
            <p className="mb-0"><small className="text-danger">⚠️ Please copy this key now. You won't be able to see it again!</small></p>
            <button type="button" className="btn-close" onClick={() => setNewApiKey(null)}></button>
          </div>
        )}

        <div className="row">
          <div className="col-12">
            <div className="card">
              <div className="card-header">
                <div className="row align-items-center">
                  <div className="col">
                    <h4 className="card-title mb-0">API Keys</h4>
                    <p className="text-muted mb-0">Manage API keys for client integrations</p>
                  </div>
                  <div className="col-auto">
                    <button
                      className="btn btn-primary"
                      onClick={() => {
                        resetForm();
                        setShowCreateModal(true);
                      }}
                    >
                      <i className="fas fa-plus me-2"></i>Create API Key
                    </button>
                  </div>
                </div>
              </div>
              <div className="card-body">
                {loading && apiKeys.length === 0 ? (
                  <Loader fullScreen color="primary" />
                ) : (
                  <DataTable
                    data={apiKeys}
                    columns={[
                      {
                        key: 'name',
                        header: 'Name',
                        render: (value, row) => (
                          <div>
                            <strong>{row.name}</strong>
                            {row.description && (
                              <div className="text-muted small">{row.description}</div>
                            )}
                          </div>
                        )
                      },
                      { key: 'clientName', header: 'Client', render: (value, row) => value || `Client #${row.clientId}` },
                      {
                        key: 'key',
                        header: 'API Key',
                        render: (value) => <code className="text-primary">{maskApiKey(value)}</code>
                      },
                      {
                        key: 'isActive',
                        header: 'Status',
                        render: (value, row) => {
                          if (isExpired(row.expiresAt)) {
                            return <span className="badge bg-danger">Expired</span>;
                          } else if (value) {
                            return <span className="badge bg-success">Active</span>;
                          } else {
                            return <span className="badge bg-secondary">Revoked</span>;
                          }
                        }
                      },
                      {
                        key: 'expiresAt',
                        header: 'Expires At',
                        render: (value) => formatDate(value)
                      },
                      {
                        key: 'createdAt',
                        header: 'Created At',
                        render: (value) => formatDate(value)
                      },
                      {
                        key: 'actions',
                        header: 'Actions',
                        cellStyle: { textAlign: 'center', width: '200px' },
                        render: (value, row) => (
                          <div className="d-flex gap-1 justify-content-center">
                            <button
                              className="btn btn-sm btn-info"
                              onClick={(e) => {
                                e.stopPropagation();
                                handleEdit(row);
                              }}
                              title="Edit"
                              disabled={loading}
                            >
                              <i className="fas fa-edit"></i>
                            </button>
                            {row.isActive ? (
                              <button
                                className="btn btn-sm btn-warning"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleRevoke(row.id);
                                }}
                                title="Revoke"
                                disabled={loading}
                              >
                                <i className="fas fa-ban"></i>
                              </button>
                            ) : !isExpired(row.expiresAt) ? (
                              <button
                                className="btn btn-sm btn-success"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleReactivate(row.id);
                                }}
                                title="Reactivate"
                                disabled={loading}
                              >
                                <i className="fas fa-check"></i>
                              </button>
                            ) : null}
                            <button
                              className="btn btn-sm btn-danger"
                              onClick={(e) => {
                                e.stopPropagation();
                                handleDelete(row.id);
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
                    searchPlaceholder="Search API keys..."
                    emptyMessage="No API keys found"
                  />
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Create Modal */}
        {showCreateModal && (
          <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
            <div className="modal-dialog">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Create API Key</h5>
                  <button type="button" className="btn-close" onClick={() => setShowCreateModal(false)}></button>
                </div>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Client <span className="text-danger">*</span></label>
                    <select
                      className="form-select"
                      value={formData.clientId}
                      onChange={(e) => setFormData({ ...formData, clientId: e.target.value })}
                      required
                    >
                      <option value="">Select Client</option>
                      {clients.map((client) => (
                        <option key={client.id} value={client.id}>
                          {client.companyName} ({client.customerNo})
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Name <span className="text-danger">*</span></label>
                    <input
                      type="text"
                      className="form-control"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      placeholder="e.g., Production API Key"
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Description</label>
                    <textarea
                      className="form-control"
                      rows="3"
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                      placeholder="Optional description..."
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Expires At</label>
                    <input
                      type="date"
                      className="form-control"
                      value={formData.expiresAt}
                      onChange={(e) => setFormData({ ...formData, expiresAt: e.target.value })}
                    />
                    <small className="text-muted">Leave empty for no expiration</small>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Allowed Origins</label>
                    <input
                      type="text"
                      className="form-control"
                      value={formData.allowedOrigins}
                      onChange={(e) => setFormData({ ...formData, allowedOrigins: e.target.value })}
                      placeholder="e.g., https://example.com, https://app.example.com"
                    />
                    <small className="text-muted">Comma-separated list of allowed origins (optional)</small>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowCreateModal(false)}>
                    Cancel
                  </button>
                  <button type="button" className="btn btn-primary" onClick={handleCreate} disabled={loading || !formData.clientId || !formData.name}>
                    {loading ? 'Creating...' : 'Create API Key'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Edit Modal */}
        {showEditModal && selectedApiKey && (
          <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
            <div className="modal-dialog">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Edit API Key</h5>
                  <button type="button" className="btn-close" onClick={() => setShowEditModal(false)}></button>
                </div>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Name <span className="text-danger">*</span></label>
                    <input
                      type="text"
                      className="form-control"
                      value={editFormData.name}
                      onChange={(e) => setEditFormData({ ...editFormData, name: e.target.value })}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Description</label>
                    <textarea
                      className="form-control"
                      rows="3"
                      value={editFormData.description}
                      onChange={(e) => setEditFormData({ ...editFormData, description: e.target.value })}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Expires At</label>
                    <input
                      type="date"
                      className="form-control"
                      value={editFormData.expiresAt}
                      onChange={(e) => setEditFormData({ ...editFormData, expiresAt: e.target.value })}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Allowed Origins</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editFormData.allowedOrigins}
                      onChange={(e) => setEditFormData({ ...editFormData, allowedOrigins: e.target.value })}
                      placeholder="Comma-separated list"
                    />
                  </div>
                  <div className="mb-3">
                    <div className="form-check">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        checked={editFormData.isActive}
                        onChange={(e) => setEditFormData({ ...editFormData, isActive: e.target.checked })}
                        id="isActiveCheck"
                      />
                      <label className="form-check-label" htmlFor="isActiveCheck">
                        Active
                      </label>
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowEditModal(false)}>
                    Cancel
                  </button>
                  <button type="button" className="btn btn-primary" onClick={handleUpdate} disabled={loading || !editFormData.name}>
                    {loading ? 'Updating...' : 'Update API Key'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default APIKeysManagement;

