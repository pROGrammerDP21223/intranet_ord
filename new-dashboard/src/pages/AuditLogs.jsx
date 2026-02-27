import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { auditLogsAPI } from '../services/api';
import { useRole } from '../hooks/useRole';
import { toast } from 'react-toastify';

const AuditLogs = () => {
  const role = useRole();
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [filters, setFilters] = useState({
    entityType: '',
    entityId: '',
    userId: '',
    startDate: '',
    endDate: '',
  });

  useEffect(() => {
    if (role.isAdmin || role.isOwner) {
      loadLogs();
    }
  }, [role.isAdmin, role.isOwner, filters]);

  const loadLogs = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await auditLogsAPI.getAuditLogs(filters);
      if (response.success && response.data) {
        setLogs(response.data);
      } else {
        setError(response.message || 'Failed to load audit logs');
      }
    } catch (err) {
      setError('An error occurred while loading audit logs');
      console.error('Failed to load audit logs:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (field, value) => {
    setFilters({ ...filters, [field]: value });
  };

  const clearFilters = () => {
    setFilters({
      entityType: '',
      entityId: '',
      userId: '',
      startDate: '',
      endDate: '',
    });
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleString();
  };

  if (!role.isAdmin && !role.isOwner) {
    return (
      <Layout>
        <div className="container-fluid">
          <div className="alert alert-danger">You don't have permission to access this page.</div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="container-fluid">
        <div className="row">
          <div className="col-12">
            <div className="card">
              <div className="card-header">
                <h4 className="card-title mb-0">Audit Logs</h4>
              </div>
              <div className="card-body">
                {/* Filters */}
                <div className="row mb-3">
                  <div className="col-md-3">
                    <label className="form-label">Entity Type</label>
                    <input
                      type="text"
                      className="form-control"
                      value={filters.entityType}
                      onChange={(e) => handleFilterChange('entityType', e.target.value)}
                      placeholder="e.g., Client, Ticket"
                    />
                  </div>
                  <div className="col-md-2">
                    <label className="form-label">Entity ID</label>
                    <input
                      type="number"
                      className="form-control"
                      value={filters.entityId}
                      onChange={(e) => handleFilterChange('entityId', e.target.value)}
                      placeholder="Entity ID"
                    />
                  </div>
                  <div className="col-md-2">
                    <label className="form-label">User ID</label>
                    <input
                      type="number"
                      className="form-control"
                      value={filters.userId}
                      onChange={(e) => handleFilterChange('userId', e.target.value)}
                      placeholder="User ID"
                    />
                  </div>
                  <div className="col-md-2">
                    <label className="form-label">Start Date</label>
                    <input
                      type="date"
                      className="form-control"
                      value={filters.startDate}
                      onChange={(e) => handleFilterChange('startDate', e.target.value)}
                    />
                  </div>
                  <div className="col-md-2">
                    <label className="form-label">End Date</label>
                    <input
                      type="date"
                      className="form-control"
                      value={filters.endDate}
                      onChange={(e) => handleFilterChange('endDate', e.target.value)}
                    />
                  </div>
                  <div className="col-md-1 d-flex align-items-end">
                    <button className="btn btn-secondary w-100" onClick={clearFilters}>
                      Clear
                    </button>
                  </div>
                </div>

                {error && <div className="alert alert-danger">{error}</div>}
                {loading ? (
                  <Loader />
                ) : (
                  <div className="table-responsive">
                    <table className="table table-striped">
                      <thead>
                        <tr>
                          <th>ID</th>
                          <th>Entity Type</th>
                          <th>Entity ID</th>
                          <th>Action</th>
                          <th>User</th>
                          <th>Timestamp</th>
                          <th>Changes</th>
                        </tr>
                      </thead>
                      <tbody>
                        {logs.length === 0 ? (
                          <tr>
                            <td colSpan="7" className="text-center">
                              No audit logs found
                            </td>
                          </tr>
                        ) : (
                          logs.map((log) => (
                            <tr key={log.id}>
                              <td>{log.id}</td>
                              <td>
                                <span className="badge bg-primary">{log.entityType}</span>
                              </td>
                              <td>{log.entityId}</td>
                              <td>
                                <span className="badge bg-info">{log.action}</span>
                              </td>
                              <td>{log.userName || log.userId}</td>
                              <td>{formatDate(log.timestamp)}</td>
                              <td>
                                <button
                                  className="btn btn-sm btn-info"
                                  data-bs-toggle="modal"
                                  data-bs-target={`#logModal${log.id}`}
                                >
                                  View Changes
                                </button>
                                {/* Modal for viewing changes */}
                                <div
                                  className="modal fade"
                                  id={`logModal${log.id}`}
                                  tabIndex="-1"
                                >
                                  <div className="modal-dialog modal-lg">
                                    <div className="modal-content">
                                      <div className="modal-header">
                                        <h5 className="modal-title">Audit Log Details</h5>
                                        <button
                                          type="button"
                                          className="btn-close"
                                          data-bs-dismiss="modal"
                                        ></button>
                                      </div>
                                      <div className="modal-body">
                                        <div className="mb-3">
                                          <strong>Old Values:</strong>
                                          <pre className="bg-light p-2 rounded">
                                            {JSON.stringify(log.oldValues, null, 2)}
                                          </pre>
                                        </div>
                                        <div className="mb-3">
                                          <strong>New Values:</strong>
                                          <pre className="bg-light p-2 rounded">
                                            {JSON.stringify(log.newValues, null, 2)}
                                          </pre>
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default AuditLogs;

