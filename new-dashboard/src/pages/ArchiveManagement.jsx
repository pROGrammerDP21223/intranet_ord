import React, { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { archiveAPI, backgroundJobsAPI } from '../services/api';
import { toast } from 'react-toastify';
import { useRole } from '../hooks/useRole';

const ArchiveManagement = () => {
  const role = useRole();
  const [archivedData, setArchivedData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [entityType, setEntityType] = useState('Client');
  const [archiveForm, setArchiveForm] = useState({
    entityType: 'Client',
    daysOld: 365,
    scheduleDate: '',
  });
  const [showArchiveModal, setShowArchiveModal] = useState(false);
  const [showScheduleModal, setShowScheduleModal] = useState(false);

  const entityTypes = ['Client', 'Enquiry', 'Ticket', 'Transaction', 'AuditLog'];

  const handleArchive = async (type, daysOld) => {
    setLoading(true);
    try {
      let response;
      switch (type) {
        case 'Client':
          response = await archiveAPI.archiveOldClients(daysOld);
          break;
        case 'Enquiry':
          response = await archiveAPI.archiveOldEnquiries(daysOld);
          break;
        case 'Ticket':
          response = await archiveAPI.archiveOldTickets(daysOld);
          break;
        case 'Transaction':
          response = await archiveAPI.archiveOldTransactions(daysOld);
          break;
        case 'AuditLog':
          response = await archiveAPI.archiveOldAuditLogs(daysOld);
          break;
        default:
          throw new Error('Invalid entity type');
      }

      if (response.success) {
        toast.success(`${type}s archived successfully!`);
        setShowArchiveModal(false);
        loadArchivedData();
      } else {
        toast.error(response.message || `Failed to archive ${type}s.`);
      }
    } catch (error) {
      console.error(`Error archiving ${type}s:`, error);
      toast.error(`An error occurred while archiving ${type}s.`);
    } finally {
      setLoading(false);
    }
  };

  const handleScheduleArchiving = async (e) => {
    e.preventDefault();
    try {
      const archiveDate = new Date(archiveForm.scheduleDate);
      const response = await backgroundJobsAPI.scheduleArchiving(archiveDate.toISOString());
      if (response.success) {
        toast.success('Archiving scheduled successfully!');
        setShowScheduleModal(false);
        setArchiveForm({ entityType: 'Client', daysOld: 365, scheduleDate: '' });
      } else {
        toast.error(response.message || 'Failed to schedule archiving.');
      }
    } catch (error) {
      console.error('Error scheduling archiving:', error);
      toast.error('An error occurred while scheduling archiving.');
    }
  };

  const loadArchivedData = async () => {
    setLoading(true);
    try {
      const response = await archiveAPI.getArchivedData(entityType);
      console.log('Archive API Response:', response);
      if (response && response.success !== false) {
        setArchivedData(response.data || []);
      } else {
        toast.error(response?.message || 'Failed to load archived data.');
        setArchivedData([]);
      }
    } catch (error) {
      console.error('Error loading archived data:', error);
      console.error('Error details:', error.response?.data || error.message);
      toast.error(error.response?.data?.message || 'An error occurred while loading archived data.');
      setArchivedData([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (role.isAdmin || role.isOwner) {
      loadArchivedData();
    }
  }, [entityType]);

  if (!role.isAdmin && !role.isOwner) {
    return (
      <Layout>
        <div className="alert alert-danger">You don't have permission to access this page.</div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0">Archive Management</h4>
            <div className="page-title-right">
              <button
                className="btn btn-primary me-2"
                onClick={() => setShowArchiveModal(true)}
              >
                <i className="fas fa-archive me-2"></i>Archive Data
              </button>
              <button
                className="btn btn-secondary"
                onClick={() => setShowScheduleModal(true)}
              >
                <i className="fas fa-calendar me-2"></i>Schedule Archiving
              </button>
            </div>
          </div>
        </div>
      </div>

      <div className="row">
        <div className="col-lg-12">
          <div className="card">
            <div className="card-header">
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="card-title mb-0">Archived Data</h5>
                <select
                  className="form-select form-select-sm"
                  style={{ width: '200px' }}
                  value={entityType}
                  onChange={(e) => setEntityType(e.target.value)}
                >
                  {entityTypes.map((type) => (
                    <option key={type} value={type}>
                      {type}s
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="card-body">
              {loading ? (
                <Loader />
              ) : (
                <div className="table-responsive">
                  <table className="table table-striped">
                    <thead>
                      <tr>
                        <th>ID</th>
                        <th>Data</th>
                        <th>Archived Date</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {archivedData.length === 0 ? (
                        <tr>
                          <td colSpan="4" className="text-center">
                            No archived {entityType.toLowerCase()}s found.
                          </td>
                        </tr>
                      ) : (
                        archivedData.slice(0, 10).map((item, index) => (
                          <tr key={index}>
                            <td>{item.id || index + 1}</td>
                            <td>
                              <pre style={{ maxHeight: '100px', overflow: 'auto' }}>
                                {JSON.stringify(item, null, 2).substring(0, 200)}...
                              </pre>
                            </td>
                            <td>{new Date().toLocaleString()}</td>
                            <td>
                              <button
                                className="btn btn-sm btn-primary"
                                onClick={async () => {
                                  try {
                                    const response = await archiveAPI.restoreArchivedData(
                                      entityType,
                                      item.id || index + 1
                                    );
                                    if (response.success) {
                                      toast.success('Data restored successfully!');
                                      loadArchivedData();
                                    } else {
                                      toast.error(response.message || 'Failed to restore data.');
                                    }
                                  } catch (error) {
                                    console.error('Error restoring data:', error);
                                    toast.error('An error occurred while restoring data.');
                                  }
                                }}
                              >
                                <i className="fas fa-undo me-1"></i>Restore
                              </button>
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

      {/* Archive Modal */}
      <div
        className={`modal fade ${showArchiveModal ? 'show d-block' : ''}`}
        tabIndex="-1"
        style={{ display: showArchiveModal ? 'block' : 'none' }}
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Archive Data</h5>
              <button
                type="button"
                className="btn-close"
                onClick={() => setShowArchiveModal(false)}
              ></button>
            </div>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                handleArchive(archiveForm.entityType, archiveForm.daysOld);
              }}
            >
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Entity Type</label>
                  <select
                    className="form-select"
                    value={archiveForm.entityType}
                    onChange={(e) => setArchiveForm({ ...archiveForm, entityType: e.target.value })}
                    required
                  >
                    {entityTypes.map((type) => (
                      <option key={type} value={type}>
                        {type}s
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Archive records older than (days)</label>
                  <input
                    type="number"
                    className="form-control"
                    value={archiveForm.daysOld}
                    onChange={(e) =>
                      setArchiveForm({ ...archiveForm, daysOld: parseInt(e.target.value) })
                    }
                    required
                    min="1"
                  />
                </div>
                <div className="alert alert-warning">
                  This will archive {archiveForm.entityType.toLowerCase()}s that are older than{' '}
                  {archiveForm.daysOld} days.
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowArchiveModal(false)}
                >
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? 'Archiving...' : 'Archive Now'}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      {/* Schedule Archiving Modal */}
      <div
        className={`modal fade ${showScheduleModal ? 'show d-block' : ''}`}
        tabIndex="-1"
        style={{ display: showScheduleModal ? 'block' : 'none' }}
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Schedule Archiving</h5>
              <button
                type="button"
                className="btn-close"
                onClick={() => setShowScheduleModal(false)}
              ></button>
            </div>
            <form onSubmit={handleScheduleArchiving}>
              <div className="modal-body">
                <div className="mb-3">
                  <label className="form-label">Archive Date & Time</label>
                  <input
                    type="datetime-local"
                    className="form-control"
                    value={archiveForm.scheduleDate}
                    onChange={(e) =>
                      setArchiveForm({ ...archiveForm, scheduleDate: e.target.value })
                    }
                    required
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowScheduleModal(false)}
                >
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Schedule
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default ArchiveManagement;

