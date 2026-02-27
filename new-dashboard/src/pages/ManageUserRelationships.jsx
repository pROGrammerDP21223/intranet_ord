import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import Loader from '../components/Loader';
import { 
  usersAPI, 
  clientAPI, 
  userClientAPI, 
  salesPersonClientAPI, 
  salesManagerSalesPersonAPI,
  salesManagerClientAPI,
  ownerClientAPI
} from '../services/api';

const ManageUserRelationships = () => {
  const [activeTab, setActiveTab] = useState('client-user');
  const [users, setUsers] = useState([]);
  const [clients, setClients] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Client User tab state
  const [selectedClientUser, setSelectedClientUser] = useState(null);
  const [clientUserClients, setClientUserClients] = useState([]);
  const [showAttachModal, setShowAttachModal] = useState(false);
  const [attachForm, setAttachForm] = useState({ userId: '', clientId: '' });

  // Sales Person tab state
  const [selectedSalesPerson, setSelectedSalesPerson] = useState(null);
  const [salesPersonClients, setSalesPersonClients] = useState([]);
  const [showAttachSalesPersonModal, setShowAttachSalesPersonModal] = useState(false);
  const [attachSalesPersonForm, setAttachSalesPersonForm] = useState({ salesPersonId: '', clientId: '' });

  // Sales Manager tab state
  const [selectedSalesManager, setSelectedSalesManager] = useState(null);
  const [salesManagerSalesPersons, setSalesManagerSalesPersons] = useState([]);
  const [salesManagerClients, setSalesManagerClients] = useState([]);
  const [showAttachManagerModal, setShowAttachManagerModal] = useState(false);
  const [attachManagerForm, setAttachManagerForm] = useState({ managerId: '', salesPersonId: '' });
  const [showAttachManagerClientModal, setShowAttachManagerClientModal] = useState(false);
  const [attachManagerClientForm, setAttachManagerClientForm] = useState({ managerId: '', clientId: '' });

  // Owner tab state
  const [selectedOwner, setSelectedOwner] = useState(null);
  const [ownerClients, setOwnerClients] = useState([]);
  const [showAttachOwnerModal, setShowAttachOwnerModal] = useState(false);
  const [attachOwnerForm, setAttachOwnerForm] = useState({ ownerId: '', clientId: '' });

  useEffect(() => {
    loadUsers();
    loadClients();
  }, []);

  useEffect(() => {
    if (activeTab === 'client-user' && selectedClientUser) {
      loadClientUserClients();
    }
    if (activeTab === 'sales-person' && selectedSalesPerson) {
      loadSalesPersonClients();
    }
    if (activeTab === 'sales-manager' && selectedSalesManager) {
      loadSalesManagerSalesPersons();
      loadSalesManagerClients();
    }
    if (activeTab === 'owner' && selectedOwner) {
      loadOwnerClients();
    }
  }, [activeTab, selectedClientUser, selectedSalesPerson, selectedSalesManager, selectedOwner]);

  const loadUsers = async () => {
    try {
      const response = await usersAPI.getUsers();
      if (response.success && response.data) {
        setUsers(response.data);
      }
    } catch (err) {
      console.error('Failed to load users:', err);
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

  // Client User functions
  const loadClientUserClients = async () => {
    if (!selectedClientUser) return;
    try {
      setLoading(true);
      const response = await userClientAPI.getUserClients(selectedClientUser.id);
      if (response.success && response.data) {
        setClientUserClients(response.data);
      }
    } catch (err) {
      console.error('Failed to load client user clients:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAttachClientUser = async () => {
    if (!attachForm.userId || !attachForm.clientId) {
      setError('Please select both user and client');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const response = await userClientAPI.attachUserToClient(
        parseInt(attachForm.userId),
        parseInt(attachForm.clientId)
      );
      if (response.success) {
        setSuccess('User attached to client successfully');
        setShowAttachModal(false);
        setAttachForm({ userId: '', clientId: '' });
        if (selectedClientUser && parseInt(attachForm.userId) === selectedClientUser.id) {
          await loadClientUserClients();
        }
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to attach user');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleDetachClientUser = async (clientId) => {
    if (!selectedClientUser) return;
    if (!window.confirm('Are you sure you want to detach this client from the user?')) return;

    try {
      setLoading(true);
      const response = await userClientAPI.detachUserFromClient(selectedClientUser.id, clientId);
      if (response.success) {
        setSuccess('Client detached successfully');
        await loadClientUserClients();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to detach client');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  // Sales Person functions
  const loadSalesPersonClients = async () => {
    if (!selectedSalesPerson) return;
    try {
      setLoading(true);
      const response = await salesPersonClientAPI.getSalesPersonClients(selectedSalesPerson.id);
      if (response.success && response.data) {
        setSalesPersonClients(response.data);
      }
    } catch (err) {
      console.error('Failed to load sales person clients:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAttachSalesPersonToClient = async () => {
    if (!attachSalesPersonForm.salesPersonId || !attachSalesPersonForm.clientId) {
      setError('Please select both sales person and client');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const response = await salesPersonClientAPI.attachSalesPersonToClient(
        parseInt(attachSalesPersonForm.salesPersonId),
        parseInt(attachSalesPersonForm.clientId)
      );
      if (response.success) {
        setSuccess('Sales person attached to client successfully');
        setShowAttachSalesPersonModal(false);
        setAttachSalesPersonForm({ salesPersonId: '', clientId: '' });
        if (selectedSalesPerson && parseInt(attachSalesPersonForm.salesPersonId) === selectedSalesPerson.id) {
          await loadSalesPersonClients();
        }
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to attach sales person');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleDetachSalesPersonFromClient = async (clientId) => {
    if (!selectedSalesPerson) return;
    if (!window.confirm('Are you sure you want to detach this client from the sales person?')) return;

    try {
      setLoading(true);
      const response = await salesPersonClientAPI.detachSalesPersonFromClient(selectedSalesPerson.id, clientId);
      if (response.success) {
        setSuccess('Client detached successfully');
        await loadSalesPersonClients();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to detach client');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  // Sales Manager functions
  const loadSalesManagerSalesPersons = async () => {
    if (!selectedSalesManager) return;
    try {
      setLoading(true);
      const response = await salesManagerSalesPersonAPI.getSalesManagerSalesPersons(selectedSalesManager.id);
      if (response.success && response.data) {
        setSalesManagerSalesPersons(response.data);
      }
    } catch (err) {
      console.error('Failed to load sales manager sales persons:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAttachSalesManagerToSalesPerson = async () => {
    if (!attachManagerForm.managerId || !attachManagerForm.salesPersonId) {
      setError('Please select both sales manager and sales person');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const response = await salesManagerSalesPersonAPI.attachSalesManagerToSalesPerson(
        parseInt(attachManagerForm.managerId),
        parseInt(attachManagerForm.salesPersonId)
      );
      if (response.success) {
        setSuccess('Sales manager attached to sales person successfully');
        setShowAttachManagerModal(false);
        setAttachManagerForm({ managerId: '', salesPersonId: '' });
        if (selectedSalesManager && parseInt(attachManagerForm.managerId) === selectedSalesManager.id) {
          await loadSalesManagerSalesPersons();
        }
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to attach sales manager');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleDetachSalesManagerFromSalesPerson = async (salesPersonId) => {
    if (!selectedSalesManager) return;
    if (!window.confirm('Are you sure you want to detach this sales person from the manager?')) return;

    try {
      setLoading(true);
      const response = await salesManagerSalesPersonAPI.detachSalesManagerFromSalesPerson(selectedSalesManager.id, salesPersonId);
      if (response.success) {
        setSuccess('Sales person detached successfully');
        await loadSalesManagerSalesPersons();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to detach sales person');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  // Sales Manager clients
  const loadSalesManagerClients = async () => {
    if (!selectedSalesManager) return;
    try {
      setLoading(true);
      const response = await salesManagerClientAPI.getSalesManagerClients(selectedSalesManager.id);
      if (response.success && response.data) {
        setSalesManagerClients(response.data);
      }
    } catch (err) {
      console.error('Failed to load sales manager clients:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAttachSalesManagerToClient = async () => {
    if (!attachManagerClientForm.managerId || !attachManagerClientForm.clientId) {
      setError('Please select both sales manager and client');
      return;
    }
    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const response = await salesManagerClientAPI.attachSalesManagerToClient(
        parseInt(attachManagerClientForm.managerId),
        parseInt(attachManagerClientForm.clientId)
      );
      if (response.success) {
        setSuccess('Sales manager attached to client successfully');
        setShowAttachManagerClientModal(false);
        setAttachManagerClientForm({ managerId: '', clientId: '' });
        if (selectedSalesManager && parseInt(attachManagerClientForm.managerId) === selectedSalesManager.id) {
          await loadSalesManagerClients();
        }
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to attach sales manager');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleDetachSalesManagerFromClient = async (clientId) => {
    if (!selectedSalesManager) return;
    if (!window.confirm('Are you sure you want to detach this client from the sales manager?')) return;
    try {
      setLoading(true);
      const response = await salesManagerClientAPI.detachSalesManagerFromClient(selectedSalesManager.id, clientId);
      if (response.success) {
        setSuccess('Client detached successfully');
        await loadSalesManagerClients();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to detach client');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  // Owner functions
  const loadOwnerClients = async () => {
    if (!selectedOwner) return;
    try {
      setLoading(true);
      const response = await ownerClientAPI.getOwnerClients(selectedOwner.id);
      if (response.success && response.data) {
        setOwnerClients(response.data);
      }
    } catch (err) {
      console.error('Failed to load owner clients:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAttachOwnerToClient = async () => {
    if (!attachOwnerForm.ownerId || !attachOwnerForm.clientId) {
      setError('Please select both owner and client');
      return;
    }
    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const response = await ownerClientAPI.attachOwnerToClient(
        parseInt(attachOwnerForm.ownerId),
        parseInt(attachOwnerForm.clientId)
      );
      if (response.success) {
        setSuccess('Owner attached to client successfully');
        setShowAttachOwnerModal(false);
        setAttachOwnerForm({ ownerId: '', clientId: '' });
        if (selectedOwner && parseInt(attachOwnerForm.ownerId) === selectedOwner.id) {
          await loadOwnerClients();
        }
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to attach owner');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleDetachOwnerFromClient = async (clientId) => {
    if (!selectedOwner) return;
    if (!window.confirm('Are you sure you want to detach this client from the owner?')) return;
    try {
      setLoading(true);
      const response = await ownerClientAPI.detachOwnerFromClient(selectedOwner.id, clientId);
      if (response.success) {
        setSuccess('Client detached successfully');
        await loadOwnerClients();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to detach client');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  // Filter users by role
  const filteredClientUsers = users.filter(u => u.roleName === 'Client');
  const filteredSalesPersons = users.filter(u => u.roleName === 'Sales Person');
  const filteredSalesManagers = users.filter(u => u.roleName === 'Sales Manager');
  const filteredOwners = users.filter(u => u.roleName === 'Owner');

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Manage User Relationships</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">User Relationships</li>
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
          <i className="fas fa-check me-2"></i>
          {success}
          <button type="button" className="btn-close" onClick={() => setSuccess('')}></button>
        </div>
      )}

      {/* Tabs */}
      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-header">
              <ul className="nav nav-tabs card-header-tabs" role="tablist">
                <li className="nav-item">
                  <button
                    className={`nav-link ${activeTab === 'client-user' ? 'active' : ''}`}
                    onClick={() => {
                      setActiveTab('client-user');
                      setSelectedClientUser(null);
                      setClientUserClients([]);
                    }}
                  >
                    <i className="fas fa-user me-2"></i>Client User
                  </button>
                </li>
                <li className="nav-item">
                  <button
                    className={`nav-link ${activeTab === 'sales-person' ? 'active' : ''}`}
                    onClick={() => {
                      setActiveTab('sales-person');
                      setSelectedSalesPerson(null);
                      setSalesPersonClients([]);
                    }}
                  >
                    <i className="fas fa-users me-2"></i>Sales Person
                  </button>
                </li>
                <li className="nav-item">
                  <button
                    className={`nav-link ${activeTab === 'sales-manager' ? 'active' : ''}`}
                    onClick={() => {
                      setActiveTab('sales-manager');
                      setSelectedSalesManager(null);
                      setSalesManagerSalesPersons([]);
                      setSalesManagerClients([]);
                    }}
                  >
                    <i className="fas fa-user-shield me-2"></i>Sales Manager
                  </button>
                </li>
                <li className="nav-item">
                  <button
                    className={`nav-link ${activeTab === 'owner' ? 'active' : ''}`}
                    onClick={() => {
                      setActiveTab('owner');
                      setSelectedOwner(null);
                      setOwnerClients([]);
                    }}
                  >
                    <i className="fas fa-crown me-2"></i>Owner
                  </button>
                </li>
              </ul>
            </div>
            <div className="card-body">
              {/* Client User Tab */}
              {activeTab === 'client-user' && (
                <div>
                  <div className="row mb-4">
                    <div className="col-md-6">
                      <label className="form-label">Select Client User</label>
                      <select
                        className="form-select"
                        value={selectedClientUser?.id || ''}
                        onChange={(e) => {
                          const userId = parseInt(e.target.value);
                          const user = filteredClientUsers.find(u => u.id === userId);
                          setSelectedClientUser(user || null);
                        }}
                      >
                        <option value="">-- Select Client User --</option>
                        {filteredClientUsers.map((user) => (
                          <option key={user.id} value={user.id}>
                            {user.name} ({user.email})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-6 d-flex align-items-end">
                      <button
                        className="btn btn-primary"
                        onClick={() => {
                          setAttachForm({ userId: selectedClientUser?.id || '', clientId: '' });
                          setShowAttachModal(true);
                        }}
                        disabled={!selectedClientUser}
                      >
                        <i className="fas fa-plus me-1"></i>Attach Client
                      </button>
                    </div>
                  </div>

                  {selectedClientUser && (
                    <div>
                      <h5 className="mb-3">Clients for {selectedClientUser.name}</h5>
                      {loading ? (
                        <Loader fullScreen color="primary" />
                      ) : (
                        <DataTable
                          data={clientUserClients}
                          columns={[
                            {
                              key: 'customerNo',
                              header: 'Customer No',
                              render: (value) => <strong className="text-primary">{value}</strong>
                            },
                            { key: 'companyName', header: 'Company Name' },
                            { key: 'email', header: 'Email' },
                            { key: 'phone', header: 'Phone' },
                            {
                              key: 'actions',
                              header: 'Actions',
                              cellStyle: { textAlign: 'center', width: '100px' },
                              render: (value, row) => (
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    handleDetachClientUser(row.id);
                                  }}
                                  disabled={loading}
                                  title="Detach"
                                >
                                  <i className="fas fa-unlink"></i>
                                </button>
                              )
                            }
                          ]}
                          pageSize={10}
                          showPagination={true}
                          showSearch={true}
                          searchPlaceholder="Search clients..."
                          emptyMessage="No clients attached to this user"
                        />
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* Sales Person Tab */}
              {activeTab === 'sales-person' && (
                <div>
                  <div className="row mb-4">
                    <div className="col-md-6">
                      <label className="form-label">Select Sales Person</label>
                      <select
                        className="form-select"
                        value={selectedSalesPerson?.id || ''}
                        onChange={(e) => {
                          const userId = parseInt(e.target.value);
                          const user = filteredSalesPersons.find(u => u.id === userId);
                          setSelectedSalesPerson(user || null);
                        }}
                      >
                        <option value="">-- Select Sales Person --</option>
                        {filteredSalesPersons.map((user) => (
                          <option key={user.id} value={user.id}>
                            {user.name} ({user.email})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-6 d-flex align-items-end">
                      <button
                        className="btn btn-primary"
                        onClick={() => {
                          setAttachSalesPersonForm({ salesPersonId: selectedSalesPerson?.id || '', clientId: '' });
                          setShowAttachSalesPersonModal(true);
                        }}
                        disabled={!selectedSalesPerson}
                      >
                        <i className="fas fa-plus me-1"></i>Attach Client
                      </button>
                    </div>
                  </div>

                  {selectedSalesPerson && (
                    <div>
                      <h5 className="mb-3">Clients for {selectedSalesPerson.name}</h5>
                      {loading ? (
                        <Loader fullScreen color="primary" />
                      ) : (
                        <DataTable
                          data={salesPersonClients}
                          columns={[
                            {
                              key: 'customerNo',
                              header: 'Customer No',
                              render: (value) => <strong className="text-primary">{value}</strong>
                            },
                            { key: 'companyName', header: 'Company Name' },
                            { key: 'email', header: 'Email' },
                            { key: 'phone', header: 'Phone' },
                            {
                              key: 'actions',
                              header: 'Actions',
                              cellStyle: { textAlign: 'center', width: '100px' },
                              render: (value, row) => (
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    handleDetachSalesPersonFromClient(row.id);
                                  }}
                                  disabled={loading}
                                  title="Detach"
                                >
                                  <i className="fas fa-unlink"></i>
                                </button>
                              )
                            }
                          ]}
                          pageSize={10}
                          showPagination={true}
                          showSearch={true}
                          searchPlaceholder="Search clients..."
                          emptyMessage="No clients attached to this sales person"
                        />
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* Sales Manager Tab */}
              {activeTab === 'sales-manager' && (
                <div>
                  <div className="row mb-4">
                    <div className="col-md-6">
                      <label className="form-label">Select Sales Manager</label>
                      <select
                        className="form-select"
                        value={selectedSalesManager?.id || ''}
                        onChange={(e) => {
                          const userId = parseInt(e.target.value);
                          const user = filteredSalesManagers.find(u => u.id === userId);
                          setSelectedSalesManager(user || null);
                        }}
                      >
                        <option value="">-- Select Sales Manager --</option>
                        {filteredSalesManagers.map((user) => (
                          <option key={user.id} value={user.id}>
                            {user.name} ({user.email})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-6 d-flex align-items-end gap-2">
                      <button
                        className="btn btn-primary"
                        onClick={() => {
                          setAttachManagerForm({ managerId: selectedSalesManager?.id || '', salesPersonId: '' });
                          setShowAttachManagerModal(true);
                        }}
                        disabled={!selectedSalesManager}
                      >
                        <i className="fas fa-plus me-1"></i>Attach Sales Person
                      </button>
                      <button
                        className="btn btn-primary"
                        onClick={() => {
                          setAttachManagerClientForm({ managerId: selectedSalesManager?.id || '', clientId: '' });
                          setShowAttachManagerClientModal(true);
                        }}
                        disabled={!selectedSalesManager}
                      >
                        <i className="fas fa-plus me-1"></i>Attach Client
                      </button>
                    </div>
                  </div>

                  {selectedSalesManager && (
                    <div>
                      <h5 className="mb-3 mt-4">Sales Persons for {selectedSalesManager.name}</h5>
                      {loading ? (
                        <Loader fullScreen color="primary" />
                      ) : (
                        <DataTable
                          data={salesManagerSalesPersons}
                          columns={[
                            {
                              key: 'name',
                              header: 'Name',
                              render: (value) => <strong>{value}</strong>
                            },
                            { key: 'email', header: 'Email' },
                            {
                              key: 'actions',
                              header: 'Actions',
                              cellStyle: { textAlign: 'center', width: '100px' },
                              render: (value, row) => (
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    handleDetachSalesManagerFromSalesPerson(row.id);
                                  }}
                                  disabled={loading}
                                  title="Detach"
                                >
                                  <i className="fas fa-unlink"></i>
                                </button>
                              )
                            }
                          ]}
                          pageSize={10}
                          showPagination={true}
                          showSearch={true}
                          searchPlaceholder="Search sales persons..."
                          emptyMessage="No sales persons attached to this manager"
                        />
                      )}
                      <h5 className="mb-3 mt-4">Clients for {selectedSalesManager.name}</h5>
                      {loading ? (
                        <Loader fullScreen color="primary" />
                      ) : (
                        <DataTable
                          data={salesManagerClients}
                          columns={[
                            {
                              key: 'customerNo',
                              header: 'Customer No',
                              render: (value) => <strong className="text-primary">{value}</strong>
                            },
                            { key: 'companyName', header: 'Company Name' },
                            { key: 'email', header: 'Email' },
                            { key: 'phone', header: 'Phone' },
                            {
                              key: 'actions',
                              header: 'Actions',
                              cellStyle: { textAlign: 'center', width: '100px' },
                              render: (value, row) => (
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    handleDetachSalesManagerFromClient(row.id);
                                  }}
                                  disabled={loading}
                                  title="Detach"
                                >
                                  <i className="fas fa-unlink"></i>
                                </button>
                              )
                            }
                          ]}
                          pageSize={10}
                          showPagination={true}
                          showSearch={true}
                          searchPlaceholder="Search clients..."
                          emptyMessage="No clients attached to this sales manager"
                        />
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* Owner Tab */}
              {activeTab === 'owner' && (
                <div>
                  <div className="row mb-4">
                    <div className="col-md-6">
                      <label className="form-label">Select Owner</label>
                      <select
                        className="form-select"
                        value={selectedOwner?.id || ''}
                        onChange={(e) => {
                          const userId = parseInt(e.target.value);
                          const user = filteredOwners.find(u => u.id === userId);
                          setSelectedOwner(user || null);
                        }}
                      >
                        <option value="">-- Select Owner --</option>
                        {filteredOwners.map((user) => (
                          <option key={user.id} value={user.id}>
                            {user.name} ({user.email})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-6 d-flex align-items-end">
                      <button
                        className="btn btn-primary"
                        onClick={() => {
                          setAttachOwnerForm({ ownerId: selectedOwner?.id || '', clientId: '' });
                          setShowAttachOwnerModal(true);
                        }}
                        disabled={!selectedOwner}
                      >
                        <i className="fas fa-plus me-1"></i>Attach Client
                      </button>
                    </div>
                  </div>

                  {selectedOwner && (
                    <div>
                      <h5 className="mb-3">Clients for {selectedOwner.name}</h5>
                      {loading ? (
                        <Loader fullScreen color="primary" />
                      ) : (
                        <DataTable
                          data={ownerClients}
                          columns={[
                            {
                              key: 'customerNo',
                              header: 'Customer No',
                              render: (value) => <strong className="text-primary">{value}</strong>
                            },
                            { key: 'companyName', header: 'Company Name' },
                            { key: 'email', header: 'Email' },
                            { key: 'phone', header: 'Phone' },
                            {
                              key: 'actions',
                              header: 'Actions',
                              cellStyle: { textAlign: 'center', width: '100px' },
                              render: (value, row) => (
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    handleDetachOwnerFromClient(row.id);
                                  }}
                                  disabled={loading}
                                  title="Detach"
                                >
                                  <i className="fas fa-unlink"></i>
                                </button>
                              )
                            }
                          ]}
                          pageSize={10}
                          showPagination={true}
                          showSearch={true}
                          searchPlaceholder="Search clients..."
                          emptyMessage="No clients attached to this owner"
                        />
                      )}
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Attach Client User Modal */}
      {showAttachModal && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Attach Client to User</h5>
                <button type="button" className="btn-close" onClick={() => setShowAttachModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Client User <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachForm.userId}
                    onChange={(e) => setAttachForm({ ...attachForm, userId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Client User --</option>
                    {filteredClientUsers.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.name} ({user.email})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Client <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachForm.clientId}
                    onChange={(e) => setAttachForm({ ...attachForm, clientId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Client --</option>
                    {clients.map((client) => (
                      <option key={client.id} value={client.id}>
                        {client.customerNo} - {client.companyName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAttachModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleAttachClientUser} disabled={loading}>
                  {loading ? 'Attaching...' : 'Attach'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Attach Sales Person Modal */}
      {showAttachSalesPersonModal && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Attach Client to Sales Person</h5>
                <button type="button" className="btn-close" onClick={() => setShowAttachSalesPersonModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Sales Person <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachSalesPersonForm.salesPersonId}
                    onChange={(e) => setAttachSalesPersonForm({ ...attachSalesPersonForm, salesPersonId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Sales Person --</option>
                    {filteredSalesPersons.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.name} ({user.email})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Client <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachSalesPersonForm.clientId}
                    onChange={(e) => setAttachSalesPersonForm({ ...attachSalesPersonForm, clientId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Client --</option>
                    {clients.map((client) => (
                      <option key={client.id} value={client.id}>
                        {client.customerNo} - {client.companyName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAttachSalesPersonModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleAttachSalesPersonToClient} disabled={loading}>
                  {loading ? 'Attaching...' : 'Attach'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Attach Sales Manager to Client Modal */}
      {showAttachManagerClientModal && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Attach Client to Sales Manager</h5>
                <button type="button" className="btn-close" onClick={() => setShowAttachManagerClientModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Sales Manager <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachManagerClientForm.managerId}
                    onChange={(e) => setAttachManagerClientForm({ ...attachManagerClientForm, managerId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Sales Manager --</option>
                    {filteredSalesManagers.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.name} ({user.email})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Client <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachManagerClientForm.clientId}
                    onChange={(e) => setAttachManagerClientForm({ ...attachManagerClientForm, clientId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Client --</option>
                    {clients.map((client) => (
                      <option key={client.id} value={client.id}>
                        {client.customerNo} - {client.companyName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAttachManagerClientModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleAttachSalesManagerToClient} disabled={loading}>
                  {loading ? 'Attaching...' : 'Attach'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Attach Sales Manager Modal */}
      {showAttachManagerModal && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Attach Sales Person to Sales Manager</h5>
                <button type="button" className="btn-close" onClick={() => setShowAttachManagerModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Sales Manager <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachManagerForm.managerId}
                    onChange={(e) => setAttachManagerForm({ ...attachManagerForm, managerId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Sales Manager --</option>
                    {filteredSalesManagers.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.name} ({user.email})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Sales Person <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachManagerForm.salesPersonId}
                    onChange={(e) => setAttachManagerForm({ ...attachManagerForm, salesPersonId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Sales Person --</option>
                    {filteredSalesPersons.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.name} ({user.email})
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAttachManagerModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleAttachSalesManagerToSalesPerson} disabled={loading}>
                  {loading ? 'Attaching...' : 'Attach'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Attach Owner Modal */}
      {showAttachOwnerModal && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Attach Client to Owner</h5>
                <button type="button" className="btn-close" onClick={() => setShowAttachOwnerModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Owner <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachOwnerForm.ownerId}
                    onChange={(e) => setAttachOwnerForm({ ...attachOwnerForm, ownerId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Owner --</option>
                    {filteredOwners.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.name} ({user.email})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Client <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={attachOwnerForm.clientId}
                    onChange={(e) => setAttachOwnerForm({ ...attachOwnerForm, clientId: e.target.value })}
                    required
                  >
                    <option value="">-- Select Client --</option>
                    {clients.map((client) => (
                      <option key={client.id} value={client.id}>
                        {client.customerNo} - {client.companyName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAttachOwnerModal(false)}>
                  Cancel
                </button>
                <button type="button" className="btn btn-primary" onClick={handleAttachOwnerToClient} disabled={loading}>
                  {loading ? 'Attaching...' : 'Attach'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default ManageUserRelationships;
