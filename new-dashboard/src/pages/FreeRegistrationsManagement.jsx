import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { freeRegistrationAPI } from '../services/api';
import { useRole } from '../hooks/useRole';

const STATUS_COLORS = {
  Pending: 'warning',
  Approved: 'success',
  Rejected: 'danger',
};

const FreeRegistrationsManagement = () => {
  const role = useRole();
  const [registrations, setRegistrations] = useState([]);
  const [filtered, setFiltered] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [stats, setStats] = useState(null);

  // Modals
  const [selectedReg, setSelectedReg] = useState(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [showNotesModal, setShowNotesModal] = useState(false);
  const [approveNotes, setApproveNotes] = useState('');
  const [rejectReason, setRejectReason] = useState('');
  const [notesValue, setNotesValue] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, [statusFilter]);

  useEffect(() => {
    applySearch();
  }, [registrations, searchTerm]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      const [regRes, statsRes] = await Promise.all([
        freeRegistrationAPI.getAll(statusFilter === 'All' ? null : statusFilter),
        freeRegistrationAPI.getStats(),
      ]);
      if (regRes.success !== false) setRegistrations(regRes.data || regRes || []);
      if (statsRes.success !== false) setStats(statsRes.data || statsRes);
    } catch (err) {
      setError('Failed to load registrations');
    } finally {
      setLoading(false);
    }
  };

  const applySearch = () => {
    if (!searchTerm.trim()) {
      setFiltered(registrations);
      return;
    }
    const q = searchTerm.toLowerCase();
    setFiltered(registrations.filter(r =>
      r.companyName?.toLowerCase().includes(q) ||
      r.contactPerson?.toLowerCase().includes(q) ||
      r.phone?.toLowerCase().includes(q) ||
      r.email?.toLowerCase().includes(q)
    ));
  };

  const handleApprove = async () => {
    try {
      setActionLoading(true);
      await freeRegistrationAPI.approve(selectedReg.id, approveNotes);
      setSuccess('Registration approved successfully');
      setShowApproveModal(false);
      setApproveNotes('');
      loadData();
    } catch {
      setError('Failed to approve registration');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!rejectReason.trim()) {
      setError('Please provide a rejection reason');
      return;
    }
    try {
      setActionLoading(true);
      await freeRegistrationAPI.reject(selectedReg.id, rejectReason);
      setSuccess('Registration rejected');
      setShowRejectModal(false);
      setRejectReason('');
      loadData();
    } catch {
      setError('Failed to reject registration');
    } finally {
      setActionLoading(false);
    }
  };

  const handleUpdateNotes = async () => {
    try {
      setActionLoading(true);
      await freeRegistrationAPI.updateNotes(selectedReg.id, notesValue);
      setSuccess('Notes updated');
      setShowNotesModal(false);
      loadData();
    } catch {
      setError('Failed to update notes');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this registration? This cannot be undone.')) return;
    try {
      await freeRegistrationAPI.delete(id);
      setSuccess('Registration deleted');
      loadData();
    } catch {
      setError('Failed to delete registration');
    }
  };

  const openApprove = (reg) => {
    setSelectedReg(reg);
    setApproveNotes('');
    setShowApproveModal(true);
  };

  const openReject = (reg) => {
    setSelectedReg(reg);
    setRejectReason('');
    setShowRejectModal(true);
  };

  const openNotes = (reg) => {
    setSelectedReg(reg);
    setNotesValue(reg.notes || '');
    setShowNotesModal(true);
  };

  const openDetail = (reg) => {
    setSelectedReg(reg);
    setShowDetailModal(true);
  };

  const formatDate = (dt) => dt ? new Date(dt).toLocaleString('en-IN') : '-';

  const parseProducts = (json) => {
    if (!json) return [];
    try { return JSON.parse(json); } catch { return [json]; }
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-flex align-items-center justify-content-between">
            <h4 className="mb-0">Free Registrations</h4>
          </div>
        </div>
      </div>

      {/* Stats */}
      {stats && (
        <div className="row mb-3">
          {[
            { label: 'Total', value: stats.total ?? 0, color: 'primary' },
            { label: 'Pending', value: stats.pending ?? 0, color: 'warning' },
            { label: 'Approved', value: stats.approved ?? 0, color: 'success' },
            { label: 'Rejected', value: stats.rejected ?? 0, color: 'danger' },
          ].map(s => (
            <div key={s.label} className="col-6 col-md-3 mb-2">
              <div className={`card border-${s.color}`}>
                <div className="card-body py-2 text-center">
                  <h4 className={`text-${s.color} mb-0`}>{s.value}</h4>
                  <small className="text-muted">{s.label}</small>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {error && <div className="alert alert-danger alert-dismissible"><button type="button" className="btn-close" onClick={() => setError('')}></button>{error}</div>}
      {success && <div className="alert alert-success alert-dismissible"><button type="button" className="btn-close" onClick={() => setSuccess('')}></button>{success}</div>}

      <div className="card">
        <div className="card-body">
          <div className="row mb-3 g-2">
            <div className="col-md-4">
              <input
                type="text"
                className="form-control"
                placeholder="Search company, contact, phone, email..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
              />
            </div>
            <div className="col-md-3">
              <select className="form-select" value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
                <option value="All">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
              </select>
            </div>
          </div>

          {loading ? (
            <Loader />
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-sm">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Company</th>
                    <th>Contact Person</th>
                    <th>Phone</th>
                    <th>Email</th>
                    <th>Domain</th>
                    <th>Status</th>
                    <th>Date</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.length === 0 ? (
                    <tr><td colSpan={9} className="text-center text-muted py-4">No registrations found</td></tr>
                  ) : filtered.map((reg, i) => (
                    <tr key={reg.id}>
                      <td>{i + 1}</td>
                      <td><strong>{reg.companyName}</strong></td>
                      <td>{reg.contactPerson}</td>
                      <td>{reg.phone}</td>
                      <td>{reg.email || '-'}</td>
                      <td>{reg.domainName || '-'}</td>
                      <td>
                        <span className={`badge bg-${STATUS_COLORS[reg.status] || 'secondary'}`}>{reg.status}</span>
                      </td>
                      <td>{formatDate(reg.createdAt)}</td>
                      <td>
                        <div className="d-flex gap-1 flex-wrap">
                          <button className="btn btn-xs btn-outline-info" onClick={() => openDetail(reg)} title="View Details">
                            <i className="fas fa-eye"></i>
                          </button>
                          {reg.status === 'Pending' && (role.isAdmin || role.isOwner || role.isSalesManager) && (
                            <>
                              <button className="btn btn-xs btn-outline-success" onClick={() => openApprove(reg)} title="Approve">
                                <i className="fas fa-check"></i>
                              </button>
                              <button className="btn btn-xs btn-outline-danger" onClick={() => openReject(reg)} title="Reject">
                                <i className="fas fa-times"></i>
                              </button>
                            </>
                          )}
                          <button className="btn btn-xs btn-outline-secondary" onClick={() => openNotes(reg)} title="Notes">
                            <i className="fas fa-sticky-note"></i>
                          </button>
                          {(role.isAdmin || role.isOwner) && (
                            <button className="btn btn-xs btn-outline-danger" onClick={() => handleDelete(reg.id)} title="Delete">
                              <i className="fas fa-trash"></i>
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {/* Detail Modal */}
      {showDetailModal && selectedReg && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Registration Details</h5>
                <button type="button" className="btn-close" onClick={() => setShowDetailModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="row g-3">
                  <div className="col-md-6"><strong>Company:</strong> {selectedReg.companyName}</div>
                  <div className="col-md-6"><strong>Contact Person:</strong> {selectedReg.contactPerson}</div>
                  <div className="col-md-6"><strong>Designation:</strong> {selectedReg.designation || '-'}</div>
                  <div className="col-md-6"><strong>Phone:</strong> {selectedReg.phone}</div>
                  <div className="col-md-6"><strong>Email:</strong> {selectedReg.email || '-'}</div>
                  <div className="col-md-6"><strong>WhatsApp:</strong> {selectedReg.whatsAppNumber || '-'}</div>
                  <div className="col-md-6"><strong>Domain:</strong> {selectedReg.domainName || '-'}</div>
                  <div className="col-md-6"><strong>Status:</strong> <span className={`badge bg-${STATUS_COLORS[selectedReg.status] || 'secondary'}`}>{selectedReg.status}</span></div>
                  <div className="col-12"><strong>Address:</strong> {selectedReg.address || '-'}</div>
                  <div className="col-12">
                    <strong>Products Interested:</strong>
                    <div className="mt-1">
                      {parseProducts(selectedReg.productsInterested).map((p, i) => (
                        <span key={i} className="badge bg-soft-primary text-primary me-1 mb-1">{p}</span>
                      ))}
                    </div>
                  </div>
                  {selectedReg.approvedBy && <div className="col-md-6"><strong>Approved By:</strong> {selectedReg.approvedBy}</div>}
                  {selectedReg.approvedAt && <div className="col-md-6"><strong>Approved At:</strong> {formatDate(selectedReg.approvedAt)}</div>}
                  {selectedReg.rejectionReason && <div className="col-12"><strong>Rejection Reason:</strong> {selectedReg.rejectionReason}</div>}
                  {selectedReg.notes && <div className="col-12"><strong>Notes:</strong> {selectedReg.notes}</div>}
                  <div className="col-md-6"><strong>Submitted:</strong> {formatDate(selectedReg.createdAt)}</div>
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setShowDetailModal(false)}>Close</button>
                {selectedReg.status === 'Pending' && (role.isAdmin || role.isOwner || role.isSalesManager) && (
                  <>
                    <button className="btn btn-success" onClick={() => { setShowDetailModal(false); openApprove(selectedReg); }}>Approve</button>
                    <button className="btn btn-danger" onClick={() => { setShowDetailModal(false); openReject(selectedReg); }}>Reject</button>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Approve Modal */}
      {showApproveModal && selectedReg && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Approve Registration</h5>
                <button type="button" className="btn-close" onClick={() => setShowApproveModal(false)}></button>
              </div>
              <div className="modal-body">
                <p>Approve registration for <strong>{selectedReg.companyName}</strong>?</p>
                <div className="mb-3">
                  <label className="form-label">Notes (optional)</label>
                  <textarea className="form-control" rows={3} value={approveNotes} onChange={e => setApproveNotes(e.target.value)} placeholder="Add any notes..." />
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setShowApproveModal(false)}>Cancel</button>
                <button className="btn btn-success" onClick={handleApprove} disabled={actionLoading}>
                  {actionLoading ? 'Approving...' : 'Approve'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal */}
      {showRejectModal && selectedReg && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Reject Registration</h5>
                <button type="button" className="btn-close" onClick={() => setShowRejectModal(false)}></button>
              </div>
              <div className="modal-body">
                <p>Reject registration for <strong>{selectedReg.companyName}</strong>?</p>
                <div className="mb-3">
                  <label className="form-label">Rejection Reason <span className="text-danger">*</span></label>
                  <textarea className="form-control" rows={3} value={rejectReason} onChange={e => setRejectReason(e.target.value)} placeholder="Provide a reason for rejection..." />
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setShowRejectModal(false)}>Cancel</button>
                <button className="btn btn-danger" onClick={handleReject} disabled={actionLoading || !rejectReason.trim()}>
                  {actionLoading ? 'Rejecting...' : 'Reject'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Notes Modal */}
      {showNotesModal && selectedReg && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Notes — {selectedReg.companyName}</h5>
                <button type="button" className="btn-close" onClick={() => setShowNotesModal(false)}></button>
              </div>
              <div className="modal-body">
                <textarea className="form-control" rows={5} value={notesValue} onChange={e => setNotesValue(e.target.value)} placeholder="Add internal notes..." />
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setShowNotesModal(false)}>Cancel</button>
                <button className="btn btn-primary" onClick={handleUpdateNotes} disabled={actionLoading}>
                  {actionLoading ? 'Saving...' : 'Save Notes'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default FreeRegistrationsManagement;
