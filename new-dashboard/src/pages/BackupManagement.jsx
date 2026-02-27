import React, { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { backupAPI, backgroundJobsAPI } from '../services/api';
import { toast } from 'react-toastify';
import { useRole } from '../hooks/useRole';

const BackupManagement = () => {
  const role = useRole();
  const [backups, setBackups] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showRestoreModal, setShowRestoreModal] = useState(false);
  const [selectedBackup, setSelectedBackup] = useState(null);
  const [scheduleForm, setScheduleForm] = useState({
    backupTime: '',
    recurrence: 'Once',
  });

  useEffect(() => {
    if (role.isAdmin || role.isOwner) {
      loadBackups();
    }
  }, []);

  const loadBackups = async () => {
    setLoading(true);
    try {
      const response = await backupAPI.listBackups();
      console.log('Backup API Response:', response);
      if (response && response.success !== false) {
        setBackups(response.data || []);
      } else {
        toast.error(response?.message || 'Failed to load backups.');
        setBackups([]);
      }
    } catch (error) {
      console.error('Error loading backups:', error);
      console.error('Error details:', error.response?.data || error.message);
      toast.error(error.response?.data?.message || 'An error occurred while loading backups.');
      setBackups([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateBackup = async () => {
    try {
      const response = await backupAPI.createBackup();
      if (response.success) {
        toast.success('Backup created successfully!');
        setShowCreateModal(false);
        loadBackups();
      } else {
        toast.error(response.message || 'Failed to create backup.');
      }
    } catch (error) {
      console.error('Error creating backup:', error);
      toast.error('An error occurred while creating backup.');
    }
  };

  const handleRestoreBackup = async (backupFilePath) => {
    if (!window.confirm('Are you sure you want to restore this backup? This will replace the current database.')) {
      return;
    }

    try {
      const response = await backupAPI.restoreBackup(backupFilePath);
      if (response.success) {
        toast.success('Backup restored successfully!');
        setShowRestoreModal(false);
      } else {
        toast.error(response.message || 'Failed to restore backup.');
      }
    } catch (error) {
      console.error('Error restoring backup:', error);
      toast.error('An error occurred while restoring backup.');
    }
  };

  const handleDeleteBackup = async (backupFilePath) => {
    if (!window.confirm('Are you sure you want to delete this backup?')) {
      return;
    }

    try {
      const response = await backupAPI.deleteBackup(backupFilePath);
      if (response.success) {
        toast.success('Backup deleted successfully!');
        loadBackups();
      } else {
        toast.error(response.message || 'Failed to delete backup.');
      }
    } catch (error) {
      console.error('Error deleting backup:', error);
      toast.error('An error occurred while deleting backup.');
    }
  };

  const handleScheduleBackup = async (e) => {
    e.preventDefault();
    try {
      const backupTime = new Date(scheduleForm.backupTime);
      const response = await backgroundJobsAPI.scheduleBackup(backupTime.toISOString());
      if (response.success) {
        toast.success('Backup scheduled successfully!');
        setShowCreateModal(false);
        setScheduleForm({ backupTime: '', recurrence: 'Once' });
      } else {
        toast.error(response.message || 'Failed to schedule backup.');
      }
    } catch (error) {
      console.error('Error scheduling backup:', error);
      toast.error('An error occurred while scheduling backup.');
    }
  };

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
            <h4 className="mb-sm-0">Backup Management</h4>
            <div className="page-title-right">
              <button className="btn btn-primary me-2" onClick={() => setShowCreateModal(true)}>
                <i className="fas fa-plus me-2"></i>Create Backup
              </button>
            </div>
          </div>
        </div>
      </div>

      <div className="row">
        <div className="col-lg-12">
          <div className="card">
            <div className="card-header">
              <h5 className="card-title mb-0">Database Backups</h5>
            </div>
            <div className="card-body">
              {loading ? (
                <Loader />
              ) : (
                <div className="table-responsive">
                  <table className="table table-striped">
                    <thead>
                      <tr>
                        <th>Backup File</th>
                        <th>Created</th>
                        <th>Size</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {backups.length === 0 ? (
                        <tr>
                          <td colSpan="4" className="text-center">No backups found.</td>
                        </tr>
                      ) : (
                        backups.map((backup, index) => {
                          const fileName = backup.split(/[/\\]/).pop();
                          const fileInfo = new File([], fileName);
                          return (
                            <tr key={index}>
                              <td>{fileName}</td>
                              <td>{new Date().toLocaleString()}</td>
                              <td>-</td>
                              <td>
                                <button
                                  className="btn btn-sm btn-primary me-2"
                                  onClick={() => {
                                    setSelectedBackup(backup);
                                    setShowRestoreModal(true);
                                  }}
                                >
                                  <i className="fas fa-undo me-1"></i>Restore
                                </button>
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={() => handleDeleteBackup(backup)}
                                >
                                  <i className="fas fa-trash me-1"></i>Delete
                                </button>
                              </td>
                            </tr>
                          );
                        })
                      )}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Create/Schedule Backup Modal */}
      <div
        className={`modal fade ${showCreateModal ? 'show d-block' : ''}`}
        tabIndex="-1"
        style={{ display: showCreateModal ? 'block' : 'none' }}
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Create or Schedule Backup</h5>
              <button
                type="button"
                className="btn-close"
                onClick={() => setShowCreateModal(false)}
              ></button>
            </div>
            <div className="modal-body">
              <ul className="nav nav-tabs">
                <li className="nav-item">
                  <button
                    className="nav-link active"
                    onClick={() => setScheduleForm({ ...scheduleForm, recurrence: 'Once' })}
                  >
                    Create Now
                  </button>
                </li>
                <li className="nav-item">
                  <button
                    className="nav-link"
                    onClick={() => setScheduleForm({ ...scheduleForm, recurrence: 'Scheduled' })}
                  >
                    Schedule
                  </button>
                </li>
              </ul>
              {scheduleForm.recurrence === 'Once' ? (
                <div className="mt-3">
                  <p>This will create a backup immediately.</p>
                  <button className="btn btn-primary" onClick={handleCreateBackup}>
                    Create Backup Now
                  </button>
                </div>
              ) : (
                <form onSubmit={handleScheduleBackup} className="mt-3">
                  <div className="mb-3">
                    <label className="form-label">Backup Date & Time</label>
                    <input
                      type="datetime-local"
                      className="form-control"
                      value={scheduleForm.backupTime}
                      onChange={(e) => setScheduleForm({ ...scheduleForm, backupTime: e.target.value })}
                      required
                    />
                  </div>
                  <button type="submit" className="btn btn-primary">
                    Schedule Backup
                  </button>
                </form>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Restore Backup Modal */}
      <div
        className={`modal fade ${showRestoreModal ? 'show d-block' : ''}`}
        tabIndex="-1"
        style={{ display: showRestoreModal ? 'block' : 'none' }}
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Restore Backup</h5>
              <button
                type="button"
                className="btn-close"
                onClick={() => setShowRestoreModal(false)}
              ></button>
            </div>
            <div className="modal-body">
              <p>Are you sure you want to restore from this backup?</p>
              <p className="text-danger">
                <strong>Warning:</strong> This will replace the current database with the backup data.
              </p>
              <p><strong>Backup File:</strong> {selectedBackup}</p>
            </div>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setShowRestoreModal(false)}
              >
                Cancel
              </button>
              <button
                type="button"
                className="btn btn-primary"
                onClick={() => handleRestoreBackup(selectedBackup)}
              >
                Restore Backup
              </button>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default BackupManagement;

