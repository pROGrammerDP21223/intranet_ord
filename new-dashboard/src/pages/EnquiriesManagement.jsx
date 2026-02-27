import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import Loader from '../components/Loader';
import { enquiryAPI } from '../services/api';
import { useRole } from '../hooks/useRole';

const EnquiriesManagement = () => {
  const role = useRole();
  const [enquiries, setEnquiries] = useState([]);
  const [filteredEnquiries, setFilteredEnquiries] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [selectedEnquiry, setSelectedEnquiry] = useState(null);
  const [showEditModal, setShowEditModal] = useState(false);
  const [statistics, setStatistics] = useState(null);

  const [editForm, setEditForm] = useState({
    status: '',
    notes: ''
  });

  useEffect(() => {
    // All roles can view enquiries (filtered by backend based on role)
    // Client (own), Sales Person (their clients), Sales Manager (own + sales persons' clients), Owner/Calling Staff (all)
    if (role.isClient || role.isSalesPerson || role.isSalesManager || role.isOwner || role.isCallingStaff || role.canViewAll) {
      loadEnquiries();
      loadStatistics();
    }
  }, [statusFilter, startDate, endDate]);

  useEffect(() => {
    filterEnquiries();
  }, [enquiries, statusFilter, searchTerm]);

  const loadEnquiries = async () => {
    try {
      setLoading(true);
      setError('');
      let response;
      if (statusFilter === 'All') {
        response = await enquiryAPI.getEnquiries(startDate || null, endDate || null);
      } else {
        response = await enquiryAPI.getEnquiriesByStatus(statusFilter, startDate || null, endDate || null);
      }
      if (response.success && response.data) {
        setEnquiries(response.data);
      } else {
        setError(response.message || 'Failed to load enquiries');
      }
    } catch (err) {
      setError('An error occurred while loading enquiries');
      console.error('Failed to load enquiries:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadStatistics = async () => {
    try {
      const response = await enquiryAPI.getStatistics();
      if (response.success && response.data) {
        setStatistics(response.data);
      }
    } catch (err) {
      console.error('Failed to load statistics:', err);
    }
  };

  const filterEnquiries = () => {
    let filtered = enquiries;

    if (statusFilter !== 'All') {
      filtered = filtered.filter(e => e.status === statusFilter);
    }

    if (searchTerm) {
      const searchLower = searchTerm.toLowerCase();
      filtered = filtered.filter(e =>
        e.fullName?.toLowerCase().includes(searchLower) ||
        e.emailId?.toLowerCase().includes(searchLower) ||
        e.mobileNumber?.toLowerCase().includes(searchLower) ||
        e.clientName?.toLowerCase().includes(searchLower) ||
        e.source?.toLowerCase().includes(searchLower)
      );
    }

    setFilteredEnquiries(filtered);
  };

  const handleEdit = (enquiry) => {
    setSelectedEnquiry(enquiry);
    setEditForm({
      status: enquiry.status || '',
      notes: enquiry.notes || ''
    });
    setShowEditModal(true);
  };

  const handleUpdate = async () => {
    if (!selectedEnquiry) return;

    try {
      setLoading(true);
      setError('');
      const response = await enquiryAPI.updateEnquiry(selectedEnquiry.id, editForm);
      if (response.success) {
        setSuccess('Enquiry updated successfully');
        setShowEditModal(false);
        await loadEnquiries();
        await loadStatistics();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to update enquiry');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while updating enquiry');
      console.error('Failed to update enquiry:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this enquiry? This action cannot be undone.')) {
      return;
    }

    try {
      setLoading(true);
      setError('');
      const response = await enquiryAPI.deleteEnquiry(id);
      if (response.success) {
        setSuccess('Enquiry deleted successfully');
        await loadEnquiries();
        await loadStatistics();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to delete enquiry');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while deleting enquiry');
      console.error('Failed to delete enquiry:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('en-GB');
  };

  const getStatusBadgeClass = (status) => {
    switch (status) {
      case 'New':
        return 'bg-primary';
      case 'In Progress':
        return 'bg-warning';
      case 'Resolved':
        return 'bg-success';
      case 'Closed':
        return 'bg-secondary';
      default:
        return 'bg-info';
    }
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Enquiries Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Enquiries</li>
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

      {/* Statistics Cards */}
      {statistics && (
        <div className="row mb-4">
          <div className="col-md-2 col-sm-6">
            <div className="card">
              <div className="card-body">
                <div className="d-flex align-items-center">
                  <div className="flex-grow-1">
                    <span className="text-muted text-uppercase font-size-12">Total</span>
                    <h3 className="mb-0">{statistics.total || 0}</h3>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="col-md-2 col-sm-6">
            <div className="card">
              <div className="card-body">
                <div className="d-flex align-items-center">
                  <div className="flex-grow-1">
                    <span className="text-muted text-uppercase font-size-12">New</span>
                    <h3 className="mb-0 text-primary">{statistics.new || 0}</h3>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="col-md-2 col-sm-6">
            <div className="card">
              <div className="card-body">
                <div className="d-flex align-items-center">
                  <div className="flex-grow-1">
                    <span className="text-muted text-uppercase font-size-12">In Progress</span>
                    <h3 className="mb-0 text-warning">{statistics.inProgress || 0}</h3>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="col-md-2 col-sm-6">
            <div className="card">
              <div className="card-body">
                <div className="d-flex align-items-center">
                  <div className="flex-grow-1">
                    <span className="text-muted text-uppercase font-size-12">Resolved</span>
                    <h3 className="mb-0 text-success">{statistics.resolved || 0}</h3>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="col-md-2 col-sm-6">
            <div className="card">
              <div className="card-body">
                <div className="d-flex align-items-center">
                  <div className="flex-grow-1">
                    <span className="text-muted text-uppercase font-size-12">Closed</span>
                    <h3 className="mb-0 text-secondary">{statistics.closed || 0}</h3>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-header">
              <div className="row align-items-center">
                <div className="col">
                  <h4 className="card-title mb-0">Enquiries</h4>
                  <p className="text-muted mb-0">Manage customer enquiries</p>
                </div>
                <div className="col-auto">
                  <div className="d-flex gap-2 flex-wrap">
                    <input
                      type="date"
                      className="form-control form-control-sm"
                      style={{ width: 'auto' }}
                      placeholder="Start Date"
                      value={startDate}
                      onChange={(e) => setStartDate(e.target.value)}
                      title="Start Date"
                    />
                    <input
                      type="date"
                      className="form-control form-control-sm"
                      style={{ width: 'auto' }}
                      placeholder="End Date"
                      value={endDate}
                      onChange={(e) => setEndDate(e.target.value)}
                      title="End Date"
                    />
                    <select
                      className="form-select form-select-sm"
                      style={{ width: 'auto' }}
                      value={statusFilter}
                      onChange={(e) => setStatusFilter(e.target.value)}
                    >
                      <option value="All">All Status</option>
                      <option value="New">New</option>
                      <option value="In Progress">In Progress</option>
                      <option value="Resolved">Resolved</option>
                      <option value="Closed">Closed</option>
                    </select>
                    <input
                      type="text"
                      className="form-control form-control-sm"
                      style={{ width: '250px' }}
                      placeholder="Search enquiries..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                    />
                  </div>
                </div>
              </div>
            </div>
            <div className="card-body">
              {loading && enquiries.length === 0 ? (
                <Loader fullScreen color="primary" />
              ) : (
                <DataTable
                  data={filteredEnquiries}
                  columns={[
                    {
                      key: 'fullName',
                      header: 'Full Name',
                      render: (value) => <strong>{value}</strong>
                    },
                    { key: 'emailId', header: 'Email' },
                    { key: 'mobileNumber', header: 'Mobile', render: (value) => value || '-' },
                    { key: 'clientName', header: 'Client', render: (value) => value || '-' },
                    {
                      key: 'status',
                      header: 'Status',
                      render: (value) => (
                        <span className={`badge ${getStatusBadgeClass(value)}`}>
                          {value}
                        </span>
                      )
                    },
                    { key: 'source', header: 'Source', render: (value) => value || 'Website' },
                    {
                      key: 'createdAt',
                      header: 'Created At',
                      render: (value) => formatDate(value)
                    },
                    {
                      key: 'actions',
                      header: 'Actions',
                      cellStyle: { textAlign: 'center', width: '120px' },
                      render: (value, row) => (
                        <div className="d-flex gap-1 justify-content-center">
                          <button
                            className="btn btn-sm btn-info"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleEdit(row);
                            }}
                            title="View/Edit"
                          >
                            <i className="fas fa-eye"></i>
                          </button>
                          {role.canDelete && (
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
                          )}
                        </div>
                      )
                    }
                  ]}
                  pageSize={10}
                  showPagination={true}
                  showSearch={false}
                  emptyMessage="No enquiries found"
                  className=""
                />
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Edit Modal */}
      {showEditModal && selectedEnquiry && (
        <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Enquiry Details</h5>
                <button type="button" className="btn-close" onClick={() => setShowEditModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="row mb-3">
                  <div className="col-md-6">
                    <strong>Full Name:</strong> {selectedEnquiry.fullName}
                  </div>
                  <div className="col-md-6">
                    <strong>Email:</strong> {selectedEnquiry.emailId}
                  </div>
                </div>
                <div className="row mb-3">
                  <div className="col-md-6">
                    <strong>Mobile:</strong> {selectedEnquiry.mobileNumber || '-'}
                  </div>
                  <div className="col-md-6">
                    <strong>Client:</strong> {selectedEnquiry.clientName || '-'}
                  </div>
                </div>
                {selectedEnquiry.rawPayload && (
                  <div className="mb-3">
                    <strong>Additional Data:</strong>
                    <div className="border p-2 mt-1" style={{ maxHeight: '150px', overflowY: 'auto' }}>
                      <pre style={{ margin: 0, fontSize: '12px' }}>
                        {JSON.stringify(JSON.parse(selectedEnquiry.rawPayload), null, 2)}
                      </pre>
                    </div>
                  </div>
                )}
                <div className="mb-3">
                  <label className="form-label">Status <span className="text-danger">*</span></label>
                  <select
                    className="form-select"
                    value={editForm.status}
                    onChange={(e) => setEditForm({ ...editForm, status: e.target.value })}
                  >
                    <option value="New">New</option>
                    <option value="In Progress">In Progress</option>
                    <option value="Resolved">Resolved</option>
                    <option value="Closed">Closed</option>
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Notes</label>
                  <textarea
                    className="form-control"
                    rows="4"
                    value={editForm.notes}
                    onChange={(e) => setEditForm({ ...editForm, notes: e.target.value })}
                    placeholder="Add internal notes..."
                  />
                </div>
                <div className="row">
                  <div className="col-md-6">
                    <strong>Source:</strong> {selectedEnquiry.source || 'Website'}
                  </div>
                  <div className="col-md-6">
                    <strong>Created:</strong> {formatDate(selectedEnquiry.createdAt)}
                  </div>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowEditModal(false)}>
                  Close
                </button>
                <button type="button" className="btn btn-primary" onClick={handleUpdate} disabled={loading}>
                  {loading ? 'Saving...' : 'Update Enquiry'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default EnquiriesManagement;

