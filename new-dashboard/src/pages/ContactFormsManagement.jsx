import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { contactFormAPI } from '../services/api';

const STATUS_COLORS = {
  New: 'danger',
  Read: 'warning',
  Responded: 'success',
};

const ContactFormsManagement = () => {
  const [forms, setForms] = useState([]);
  const [filtered, setFiltered] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [stats, setStats] = useState(null);

  const [selectedForm, setSelectedForm] = useState(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, [statusFilter]);

  useEffect(() => {
    applySearch();
  }, [forms, searchTerm]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      const status = statusFilter !== 'All' ? statusFilter : null;
      const [formsRes, statsRes] = await Promise.all([
        contactFormAPI.getAll(status),
        contactFormAPI.getStats(),
      ]);
      if (formsRes.success !== false) setForms(formsRes.data || formsRes || []);
      if (statsRes.success !== false) setStats(statsRes.data || statsRes);
    } catch {
      setError('Failed to load contact forms');
    } finally {
      setLoading(false);
    }
  };

  const applySearch = () => {
    if (!searchTerm.trim()) { setFiltered(forms); return; }
    const q = searchTerm.toLowerCase();
    setFiltered(forms.filter(f =>
      f.name?.toLowerCase().includes(q) ||
      f.email?.toLowerCase().includes(q) ||
      f.phone?.toLowerCase().includes(q) ||
      f.company?.toLowerCase().includes(q) ||
      f.subject?.toLowerCase().includes(q) ||
      f.message?.toLowerCase().includes(q)
    ));
  };

  const handleStatusChange = async (form, newStatus) => {
    try {
      setActionLoading(true);
      await contactFormAPI.updateStatus(form.id, newStatus);
      setSuccess(`Status updated to ${newStatus}`);
      loadData();
    } catch {
      setError('Failed to update status');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this contact message? This cannot be undone.')) return;
    try {
      await contactFormAPI.delete(id);
      setSuccess('Message deleted');
      loadData();
    } catch {
      setError('Failed to delete message');
    }
  };

  const openDetail = (form) => {
    setSelectedForm(form);
    if (form.status === 'New') handleStatusChange(form, 'Read');
    setShowDetailModal(true);
  };

  const formatDate = (dt) => dt ? new Date(dt).toLocaleString('en-IN') : '-';

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-flex align-items-center justify-content-between">
            <h4 className="mb-0">Contact Forms</h4>
            <small className="text-muted">Messages submitted via the public contact page</small>
          </div>
        </div>
      </div>

      {/* Stats */}
      {stats && (
        <div className="row mb-3">
          {[
            { label: 'Total',     value: stats.total     ?? 0, color: 'primary' },
            { label: 'New',       value: stats.newCount  ?? stats.new ?? 0, color: 'danger' },
            { label: 'Read',      value: stats.read      ?? 0, color: 'warning' },
            { label: 'Responded', value: stats.responded ?? 0, color: 'success' },
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
                placeholder="Search name, email, subject..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
              />
            </div>
            <div className="col-md-2">
              <select className="form-select" value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
                <option value="All">All Statuses</option>
                <option value="New">New</option>
                <option value="Read">Read</option>
                <option value="Responded">Responded</option>
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
                    <th>Name</th>
                    <th>Phone</th>
                    <th>Email</th>
                    <th>Company</th>
                    <th>Subject</th>
                    <th>Message</th>
                    <th>Status</th>
                    <th>Date</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.length === 0 ? (
                    <tr><td colSpan={10} className="text-center text-muted py-4">No contact messages found</td></tr>
                  ) : filtered.map((f, i) => (
                    <tr key={f.id} className={f.status === 'New' ? 'table-warning' : ''}>
                      <td>{i + 1}</td>
                      <td><strong>{f.name}</strong></td>
                      <td>{f.phone || '-'}</td>
                      <td>{f.email || '-'}</td>
                      <td>{f.company || '-'}</td>
                      <td>{f.subject}</td>
                      <td>
                        <span title={f.message}>{f.message?.length > 50 ? f.message.substring(0, 50) + '...' : f.message}</span>
                      </td>
                      <td>
                        <span className={`badge bg-${STATUS_COLORS[f.status] || 'secondary'}`}>{f.status}</span>
                      </td>
                      <td>{formatDate(f.createdAt)}</td>
                      <td>
                        <div className="d-flex gap-1 flex-wrap">
                          <button className="btn btn-xs btn-outline-info" onClick={() => openDetail(f)} title="View">
                            <i className="fas fa-eye"></i>
                          </button>
                          {f.status !== 'Responded' && (
                            <button
                              className="btn btn-xs btn-outline-success"
                              onClick={() => handleStatusChange(f, 'Responded')}
                              disabled={actionLoading}
                              title="Mark as Responded"
                            >
                              <i className="fas fa-check-double"></i>
                            </button>
                          )}
                          {f.status === 'New' && (
                            <button
                              className="btn btn-xs btn-outline-secondary"
                              onClick={() => handleStatusChange(f, 'Read')}
                              disabled={actionLoading}
                              title="Mark as Read"
                            >
                              <i className="fas fa-envelope-open"></i>
                            </button>
                          )}
                          <button className="btn btn-xs btn-outline-danger" onClick={() => handleDelete(f.id)} title="Delete">
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
      </div>

      {/* Detail Modal */}
      {showDetailModal && selectedForm && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Contact Message Details</h5>
                <button type="button" className="btn-close" onClick={() => setShowDetailModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="row g-3">
                  <div className="col-md-6"><strong>Name:</strong> {selectedForm.name}</div>
                  <div className="col-md-6"><strong>Phone:</strong> {selectedForm.phone || '-'}</div>
                  <div className="col-md-6"><strong>Email:</strong> {selectedForm.email || '-'}</div>
                  <div className="col-md-6"><strong>Company:</strong> {selectedForm.company || '-'}</div>
                  <div className="col-12"><strong>Subject:</strong> {selectedForm.subject}</div>
                  <div className="col-12">
                    <strong>Message:</strong>
                    <div className="mt-1 p-2 bg-light rounded" style={{ whiteSpace: 'pre-wrap' }}>{selectedForm.message}</div>
                  </div>
                  <div className="col-md-6">
                    <strong>Status:</strong>{' '}
                    <span className={`badge bg-${STATUS_COLORS[selectedForm.status] || 'secondary'}`}>{selectedForm.status}</span>
                  </div>
                  <div className="col-md-6"><strong>Received:</strong> {formatDate(selectedForm.createdAt)}</div>
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setShowDetailModal(false)}>Close</button>
                {selectedForm.status !== 'Responded' && (
                  <button
                    className="btn btn-success"
                    onClick={() => { handleStatusChange(selectedForm, 'Responded'); setShowDetailModal(false); }}
                    disabled={actionLoading}
                  >
                    Mark as Responded
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default ContactFormsManagement;
