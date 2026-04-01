import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { ordpanelEnquiryAPI } from '../services/api';
import { useRole } from '../hooks/useRole';

const STATUS_COLORS = {
  New: 'danger',
  Read: 'warning',
  Responded: 'success',
};

const PAGE_TYPE_LABELS = {
  product: 'Product',
  client: 'Supplier',
  general: 'General',
};

const OrdpanelEnquiriesManagement = () => {
  const role = useRole();
  const [enquiries, setEnquiries] = useState([]);
  const [filtered, setFiltered] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [pageTypeFilter, setPageTypeFilter] = useState('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [stats, setStats] = useState(null);

  const [selectedEnq, setSelectedEnq] = useState(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, [statusFilter, pageTypeFilter, fromDate, toDate]);

  useEffect(() => {
    applySearch();
  }, [enquiries, searchTerm]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      const filters = {};
      if (statusFilter !== 'All') filters.status = statusFilter;
      if (pageTypeFilter !== 'All') filters.pageType = pageTypeFilter;
      if (fromDate) filters.from = fromDate;
      if (toDate) filters.to = toDate;

      const [enqRes, statsRes] = await Promise.all([
        ordpanelEnquiryAPI.getAll(filters),
        ordpanelEnquiryAPI.getStats(),
      ]);
      if (enqRes.success !== false) setEnquiries(enqRes.data || enqRes || []);
      if (statsRes.success !== false) setStats(statsRes.data || statsRes);
    } catch {
      setError('Failed to load enquiries');
    } finally {
      setLoading(false);
    }
  };

  const applySearch = () => {
    if (!searchTerm.trim()) { setFiltered(enquiries); return; }
    const q = searchTerm.toLowerCase();
    setFiltered(enquiries.filter(e =>
      e.name?.toLowerCase().includes(q) ||
      e.email?.toLowerCase().includes(q) ||
      e.phone?.toLowerCase().includes(q) ||
      e.productName?.toLowerCase().includes(q) ||
      e.clientName?.toLowerCase().includes(q) ||
      e.listingClientId?.toLowerCase().includes(q) ||
      e.message?.toLowerCase().includes(q)
    ));
  };

  const handleStatusChange = async (enq, newStatus) => {
    try {
      setActionLoading(true);
      await ordpanelEnquiryAPI.updateStatus(enq.id, newStatus);
      setSuccess(`Status updated to ${newStatus}`);
      loadData();
    } catch {
      setError('Failed to update status');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this enquiry? This cannot be undone.')) return;
    try {
      await ordpanelEnquiryAPI.delete(id);
      setSuccess('Enquiry deleted');
      loadData();
    } catch {
      setError('Failed to delete enquiry');
    }
  };

  const openDetail = (enq) => {
    setSelectedEnq(enq);
    // Auto-mark as Read if New
    if (enq.status === 'New') handleStatusChange(enq, 'Read');
    setShowDetailModal(true);
  };

  const formatDate = (dt) => dt ? new Date(dt).toLocaleString('en-IN') : '-';

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-flex align-items-center justify-content-between">
            <h4 className="mb-0">Ordpanel Enquiries</h4>
            <small className="text-muted">Public portal enquiries from Industrial Marketplace</small>
          </div>
        </div>
      </div>

      {/* Stats */}
      {stats && (
        <div className="row mb-3">
          {[
            { label: 'Total', value: stats.total ?? 0, color: 'primary' },
            { label: 'New', value: stats.new ?? 0, color: 'danger' },
            { label: 'Read', value: stats.read ?? 0, color: 'warning' },
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
            <div className="col-md-3">
              <input
                type="text"
                className="form-control"
                placeholder="Search name, email, product..."
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
            <div className="col-md-2">
              <select className="form-select" value={pageTypeFilter} onChange={e => setPageTypeFilter(e.target.value)}>
                <option value="All">All Types</option>
                <option value="product">Product</option>
                <option value="client">Supplier</option>
                <option value="general">General</option>
              </select>
            </div>
            <div className="col-md-2">
              <input type="date" className="form-control" value={fromDate} onChange={e => setFromDate(e.target.value)} title="From date" />
            </div>
            <div className="col-md-2">
              <input type="date" className="form-control" value={toDate} onChange={e => setToDate(e.target.value)} title="To date" />
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
                    <th>Type</th>
                    <th>Product / Supplier</th>
                    <th>Message</th>
                    <th>Status</th>
                    <th>Date</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.length === 0 ? (
                    <tr><td colSpan={10} className="text-center text-muted py-4">No enquiries found</td></tr>
                  ) : filtered.map((enq, i) => (
                    <tr key={enq.id} className={enq.status === 'New' ? 'table-warning' : ''}>
                      <td>{i + 1}</td>
                      <td><strong>{enq.name}</strong></td>
                      <td>{enq.phone || '-'}</td>
                      <td>{enq.email || '-'}</td>
                      <td><span className="badge bg-soft-secondary text-secondary">{PAGE_TYPE_LABELS[enq.pageType] || enq.pageType}</span></td>
                      <td>
                        {enq.productName && <div><small className="text-muted">Product:</small> {enq.productName}</div>}
                        {enq.clientName && <div><small className="text-muted">Supplier:</small> {enq.clientName}</div>}
                        {enq.listingClientId && (
                          <div><small className="text-muted">Supplier ID:</small> <code className="small">{enq.listingClientId}</code></div>
                        )}
                        {!enq.productName && !enq.clientName && !enq.listingClientId && '-'}
                      </td>
                      <td>
                        <span title={enq.message}>{enq.message?.length > 50 ? enq.message.substring(0, 50) + '...' : enq.message}</span>
                      </td>
                      <td>
                        <span className={`badge bg-${STATUS_COLORS[enq.status] || 'secondary'}`}>{enq.status}</span>
                      </td>
                      <td>{formatDate(enq.createdAt)}</td>
                      <td>
                        <div className="d-flex gap-1 flex-wrap">
                          <button className="btn btn-xs btn-outline-info" onClick={() => openDetail(enq)} title="View">
                            <i className="fas fa-eye"></i>
                          </button>
                          {enq.status !== 'Responded' && (
                            <button
                              className="btn btn-xs btn-outline-success"
                              onClick={() => handleStatusChange(enq, 'Responded')}
                              disabled={actionLoading}
                              title="Mark as Responded"
                            >
                              <i className="fas fa-check-double"></i>
                            </button>
                          )}
                          {enq.status === 'New' && (
                            <button
                              className="btn btn-xs btn-outline-secondary"
                              onClick={() => handleStatusChange(enq, 'Read')}
                              disabled={actionLoading}
                              title="Mark as Read"
                            >
                              <i className="fas fa-envelope-open"></i>
                            </button>
                          )}
                          <button className="btn btn-xs btn-outline-danger" onClick={() => handleDelete(enq.id)} title="Delete">
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
      {showDetailModal && selectedEnq && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Enquiry Details</h5>
                <button type="button" className="btn-close" onClick={() => setShowDetailModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="row g-3">
                  <div className="col-md-6"><strong>Name:</strong> {selectedEnq.name}</div>
                  <div className="col-md-6"><strong>Phone:</strong> {selectedEnq.phone || '-'}</div>
                  <div className="col-md-6"><strong>Email:</strong> {selectedEnq.email || '-'}</div>
                  <div className="col-md-6">
                    <strong>Type:</strong> {PAGE_TYPE_LABELS[selectedEnq.pageType] || selectedEnq.pageType}
                  </div>
                  {selectedEnq.productName && (
                    <div className="col-md-6"><strong>Product:</strong> {selectedEnq.productName}</div>
                  )}
                  {selectedEnq.clientName && (
                    <div className="col-md-6"><strong>Supplier:</strong> {selectedEnq.clientName}</div>
                  )}
                  {selectedEnq.listingClientId && (
                    <div className="col-md-6"><strong>Supplier listing ID:</strong> <code>{selectedEnq.listingClientId}</code></div>
                  )}
                  {selectedEnq.pageUrl && (
                    <div className="col-12">
                      <strong>Page URL:</strong>{' '}
                      <a href={selectedEnq.pageUrl} target="_blank" rel="noopener noreferrer">{selectedEnq.pageUrl}</a>
                    </div>
                  )}
                  <div className="col-12">
                    <strong>Message:</strong>
                    <div className="mt-1 p-2 bg-light rounded">{selectedEnq.message}</div>
                  </div>
                  <div className="col-md-6">
                    <strong>Status:</strong>{' '}
                    <span className={`badge bg-${STATUS_COLORS[selectedEnq.status] || 'secondary'}`}>{selectedEnq.status}</span>
                  </div>
                  <div className="col-md-6"><strong>Received:</strong> {formatDate(selectedEnq.createdAt)}</div>
                </div>
              </div>
              <div className="modal-footer">
                <button className="btn btn-secondary" onClick={() => setShowDetailModal(false)}>Close</button>
                {selectedEnq.status !== 'Responded' && (
                  <button
                    className="btn btn-success"
                    onClick={() => { handleStatusChange(selectedEnq, 'Responded'); setShowDetailModal(false); }}
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

export default OrdpanelEnquiriesManagement;
