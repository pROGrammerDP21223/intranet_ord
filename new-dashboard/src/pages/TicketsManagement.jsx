import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import Loader from '../components/Loader';
import { ticketAPI, clientAPI, usersAPI } from '../services/api';
import { useRole } from '../hooks/useRole';

const TicketsManagement = () => {
  const role = useRole();
  const [tickets, setTickets] = useState([]);
  const [filteredTickets, setFilteredTickets] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [priorityFilter, setPriorityFilter] = useState('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedTicket, setSelectedTicket] = useState(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showAssignModal, setShowAssignModal] = useState(false);
  const [showCommentModal, setShowCommentModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [clients, setClients] = useState([]);
  const [users, setUsers] = useState([]);
  const [comments, setComments] = useState([]);
  const [loadingComments, setLoadingComments] = useState(false);

  const [createForm, setCreateForm] = useState({
    title: '',
    description: '',
    priority: 'Medium',
    clientId: null,
  });

  const [editForm, setEditForm] = useState({
    title: '',
    description: '',
    status: '',
    priority: '',
  });

  const [assignForm, setAssignForm] = useState({
    assignedToUserId: '',
  });

  const [commentForm, setCommentForm] = useState({
    comment: '',
    isInternal: false,
  });

  useEffect(() => {
    loadTickets();
    if (role.canViewAll || role.isAdmin) {
      loadClients();
      loadUsers();
    }
  }, [statusFilter]);

  useEffect(() => {
    filterTickets();
  }, [tickets, statusFilter, priorityFilter, searchTerm]);

  const loadTickets = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await ticketAPI.getTickets();
      if (response.success && response.data) {
        setTickets(response.data);
      } else {
        setError(response.message || 'Failed to load tickets');
      }
    } catch (err) {
      setError('An error occurred while loading tickets');
      console.error('Failed to load tickets:', err);
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

  const loadComments = async (ticketId) => {
    try {
      setLoadingComments(true);
      const response = await ticketAPI.getComments(ticketId);
      if (response.success && response.data) {
        setComments(response.data);
      }
    } catch (err) {
      console.error('Failed to load comments:', err);
    } finally {
      setLoadingComments(false);
    }
  };

  const filterTickets = () => {
    let filtered = tickets;

    if (statusFilter !== 'All') {
      filtered = filtered.filter(t => t.status === statusFilter);
    }

    if (priorityFilter !== 'All') {
      filtered = filtered.filter(t => t.priority === priorityFilter);
    }

    if (searchTerm) {
      const searchLower = searchTerm.toLowerCase();
      filtered = filtered.filter(t =>
        t.ticketNumber?.toLowerCase().includes(searchLower) ||
        t.title?.toLowerCase().includes(searchLower) ||
        t.description?.toLowerCase().includes(searchLower) ||
        t.creatorName?.toLowerCase().includes(searchLower) ||
        t.clientName?.toLowerCase().includes(searchLower)
      );
    }

    setFilteredTickets(filtered);
  };

  const handleCreate = () => {
    setCreateForm({
      title: '',
      description: '',
      priority: 'Medium',
      clientId: null,
    });
    setShowCreateModal(true);
  };

  const handleCreateSubmit = async (e) => {
    e.preventDefault();
    try {
      setError('');
      setSuccess('');
      const response = await ticketAPI.createTicket(createForm);
      if (response.success) {
        setSuccess('Ticket created successfully');
        setShowCreateModal(false);
        loadTickets();
        setCreateForm({
          title: '',
          description: '',
          priority: 'Medium',
          clientId: null,
        });
      } else {
        setError(response.message || 'Failed to create ticket');
      }
    } catch (err) {
      setError('An error occurred while creating ticket');
      console.error('Failed to create ticket:', err);
    }
  };

  const handleEdit = (ticket) => {
    setSelectedTicket(ticket);
    setEditForm({
      title: ticket.title || '',
      description: ticket.description || '',
      status: ticket.status || '',
      priority: ticket.priority || '',
    });
    setShowEditModal(true);
  };

  const handleEditSubmit = async (e) => {
    e.preventDefault();
    try {
      setError('');
      setSuccess('');
      const response = await ticketAPI.updateTicket(selectedTicket.id, editForm);
      if (response.success) {
        setSuccess('Ticket updated successfully');
        setShowEditModal(false);
        loadTickets();
      } else {
        setError(response.message || 'Failed to update ticket');
      }
    } catch (err) {
      setError('An error occurred while updating ticket');
      console.error('Failed to update ticket:', err);
    }
  };

  const handleAssign = (ticket) => {
    setSelectedTicket(ticket);
    setAssignForm({
      assignedToUserId: ticket.assignedTo || '',
    });
    setShowAssignModal(true);
  };

  const handleAssignSubmit = async (e) => {
    e.preventDefault();
    try {
      setError('');
      setSuccess('');
      const response = await ticketAPI.assignTicket(
        selectedTicket.id,
        assignForm.assignedToUserId
      );
      if (response.success) {
        setSuccess('Ticket assigned successfully');
        setShowAssignModal(false);
        loadTickets();
      } else {
        setError(response.message || 'Failed to assign ticket');
      }
    } catch (err) {
      setError('An error occurred while assigning ticket');
      console.error('Failed to assign ticket:', err);
    }
  };

  const handleViewDetails = async (ticket) => {
    setSelectedTicket(ticket);
    setShowDetailModal(true);
    await loadComments(ticket.id);
  };

  const handleAddComment = (ticket) => {
    setSelectedTicket(ticket);
    setCommentForm({
      comment: '',
      isInternal: false,
    });
    setShowCommentModal(true);
  };

  const handleCommentSubmit = async (e) => {
    e.preventDefault();
    try {
      setError('');
      setSuccess('');
      const response = await ticketAPI.addComment(
        selectedTicket.id,
        commentForm.comment,
        commentForm.isInternal
      );
      if (response.success) {
        setSuccess('Comment added successfully');
        setShowCommentModal(false);
        await loadComments(selectedTicket.id);
        loadTickets();
        setCommentForm({
          comment: '',
          isInternal: false,
        });
      } else {
        setError(response.message || 'Failed to add comment');
      }
    } catch (err) {
      setError('An error occurred while adding comment');
      console.error('Failed to add comment:', err);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this ticket?')) {
      return;
    }
    try {
      setError('');
      setSuccess('');
      const response = await ticketAPI.deleteTicket(id);
      if (response.success) {
        setSuccess('Ticket deleted successfully');
        loadTickets();
      } else {
        setError(response.message || 'Failed to delete ticket');
      }
    } catch (err) {
      setError('An error occurred while deleting ticket');
      console.error('Failed to delete ticket:', err);
    }
  };

  const getStatusBadgeClass = (status) => {
    switch (status) {
      case 'Open':
        return 'badge bg-primary';
      case 'InProgress':
        return 'badge bg-warning';
      case 'Resolved':
        return 'badge bg-success';
      case 'Closed':
        return 'badge bg-secondary';
      case 'Cancelled':
        return 'badge bg-danger';
      default:
        return 'badge bg-secondary';
    }
  };

  const getPriorityBadgeClass = (priority) => {
    switch (priority) {
      case 'Low':
        return 'badge bg-info';
      case 'Medium':
        return 'badge bg-warning';
      case 'High':
        return 'badge bg-danger';
      case 'Urgent':
        return 'badge bg-dark';
      default:
        return 'badge bg-secondary';
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <Layout>
      <div className="page-content">
        <div className="container-fluid">
          <div className="row">
            <div className="col-12">
              <div className="page-title-box d-sm-flex align-items-center justify-content-between">
                <h4 className="mb-sm-0">Tickets Management</h4>
                <div className="page-title-right">
                  <ol className="breadcrumb m-0">
                    <li className="breadcrumb-item">
                      <Link to="/dashboard">Dashboard</Link>
                    </li>
                    <li className="breadcrumb-item active">Tickets</li>
                  </ol>
                </div>
              </div>
            </div>
          </div>

          {error && (
            <div className="alert alert-danger alert-dismissible fade show" role="alert">
              <i className="fas fa-exclamation-circle me-2"></i>
              {error}
              <button
                type="button"
                className="btn-close"
                onClick={() => setError('')}
              ></button>
            </div>
          )}

          {success && (
            <div className="alert alert-success alert-dismissible fade show" role="alert">
              <i className="fas fa-check-circle me-2"></i>
              {success}
              <button
                type="button"
                className="btn-close"
                onClick={() => setSuccess('')}
              ></button>
            </div>
          )}

          <div className="row">
            <div className="col-12">
              <div className="card">
                <div className="card-header">
                  <div className="row align-items-center">
                    <div className="col">
                      <h4 className="card-title mb-0">Tickets</h4>
                      <p className="text-muted mb-0">Manage support tickets</p>
                    </div>
                    <div className="col-auto">
                      <div className="d-flex gap-2 flex-wrap">
                        <input
                          type="text"
                          className="form-control form-control-sm"
                          style={{ width: '200px' }}
                          placeholder="Search tickets..."
                          value={searchTerm}
                          onChange={(e) => setSearchTerm(e.target.value)}
                        />
                        <select
                          className="form-select form-select-sm"
                          style={{ width: 'auto' }}
                          value={statusFilter}
                          onChange={(e) => setStatusFilter(e.target.value)}
                        >
                          <option value="All">All Status</option>
                          <option value="Open">Open</option>
                          <option value="InProgress">In Progress</option>
                          <option value="Resolved">Resolved</option>
                          <option value="Closed">Closed</option>
                          <option value="Cancelled">Cancelled</option>
                        </select>
                        <select
                          className="form-select form-select-sm"
                          style={{ width: 'auto' }}
                          value={priorityFilter}
                          onChange={(e) => setPriorityFilter(e.target.value)}
                        >
                          <option value="All">All Priority</option>
                          <option value="Low">Low</option>
                          <option value="Medium">Medium</option>
                          <option value="High">High</option>
                          <option value="Urgent">Urgent</option>
                        </select>
                        <button
                          className="btn btn-primary btn-sm"
                          onClick={handleCreate}
                        >
                          <i className="fas fa-plus me-1"></i>Create Ticket
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="card-body">
                  {loading && filteredTickets.length === 0 ? (
                    <Loader fullScreen color="primary" />
                  ) : (
                    <DataTable
                      data={filteredTickets}
                      columns={[
                        {
                          key: 'ticketNumber',
                          header: 'Ticket #',
                          render: (value) => <strong>{value}</strong>
                        },
                        { key: 'title', header: 'Title' },
                        {
                          key: 'status',
                          header: 'Status',
                          render: (value) => (
                            <span className={getStatusBadgeClass(value)}>
                              {value}
                            </span>
                          )
                        },
                        {
                          key: 'priority',
                          header: 'Priority',
                          render: (value) => (
                            <span className={getPriorityBadgeClass(value)}>
                              {value}
                            </span>
                          )
                        },
                        { key: 'creatorName', header: 'Created By', render: (value) => value || '-' },
                        { key: 'clientName', header: 'Client', render: (value) => value || '-' },
                        { key: 'assigneeName', header: 'Assigned To', render: (value) => value || '-' },
                        {
                          key: 'createdAt',
                          header: 'Created At',
                          render: (value) => formatDate(value)
                        },
                        {
                          key: 'actions',
                          header: 'Actions',
                          cellStyle: { textAlign: 'center', width: '180px' },
                          render: (value, row) => (
                            <div className="d-flex gap-1 justify-content-center">
                              <button
                                className="btn btn-sm btn-info"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleViewDetails(row);
                                }}
                                title="View Details"
                              >
                                <i className="fas fa-eye"></i>
                              </button>
                              <button
                                className="btn btn-sm btn-primary"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleAddComment(row);
                                }}
                                title="Add Comment"
                              >
                                <i className="fas fa-comment"></i>
                              </button>
                              {(role.isAdmin || role.isCallingStaff || role.isHOD) && (
                                <>
                                  <button
                                    className="btn btn-sm btn-warning"
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      handleEdit(row);
                                    }}
                                    title="Edit"
                                  >
                                    <i className="fas fa-edit"></i>
                                  </button>
                                  <button
                                    className="btn btn-sm btn-success"
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      handleAssign(row);
                                    }}
                                    title="Assign"
                                  >
                                    <i className="fas fa-user-check"></i>
                                  </button>
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
                                </>
                              )}
                            </div>
                          )
                        }
                      ]}
                      pageSize={10}
                      showPagination={true}
                      showSearch={false}
                      emptyMessage="No tickets found"
                      className=""
                    />
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Create Ticket Modal */}
      {showCreateModal && (
        <div
          className="modal fade show"
          style={{ display: 'block' }}
          tabIndex="-1"
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Create New Ticket</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowCreateModal(false)}
                ></button>
              </div>
              <form onSubmit={handleCreateSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Title *</label>
                    <input
                      type="text"
                      className="form-control"
                      value={createForm.title}
                      onChange={(e) =>
                        setCreateForm({ ...createForm, title: e.target.value })
                      }
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Description *</label>
                    <textarea
                      className="form-control"
                      rows="4"
                      value={createForm.description}
                      onChange={(e) =>
                        setCreateForm({
                          ...createForm,
                          description: e.target.value,
                        })
                      }
                      required
                    ></textarea>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Priority</label>
                    <select
                      className="form-select"
                      value={createForm.priority}
                      onChange={(e) =>
                        setCreateForm({ ...createForm, priority: e.target.value })
                      }
                    >
                      <option value="Low">Low</option>
                      <option value="Medium">Medium</option>
                      <option value="High">High</option>
                      <option value="Urgent">Urgent</option>
                    </select>
                  </div>
                  {(role.canViewAll || role.isAdmin) && (
                    <div className="mb-3">
                      <label className="form-label">Client (Optional)</label>
                      <select
                        className="form-select"
                        value={createForm.clientId || ''}
                        onChange={(e) =>
                          setCreateForm({
                            ...createForm,
                            clientId: e.target.value ? parseInt(e.target.value) : null,
                          })
                        }
                      >
                        <option value="">Select Client</option>
                        {clients.map((client) => (
                          <option key={client.id} value={client.id}>
                            {client.companyName}
                          </option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowCreateModal(false)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Create Ticket
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Edit Ticket Modal */}
      {showEditModal && selectedTicket && (
        <div
          className="modal fade show"
          style={{ display: 'block' }}
          tabIndex="-1"
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Ticket</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowEditModal(false)}
                ></button>
              </div>
              <form onSubmit={handleEditSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Title *</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editForm.title}
                      onChange={(e) =>
                        setEditForm({ ...editForm, title: e.target.value })
                      }
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Description *</label>
                    <textarea
                      className="form-control"
                      rows="4"
                      value={editForm.description}
                      onChange={(e) =>
                        setEditForm({
                          ...editForm,
                          description: e.target.value,
                        })
                      }
                      required
                    ></textarea>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Status</label>
                    <select
                      className="form-select"
                      value={editForm.status}
                      onChange={(e) =>
                        setEditForm({ ...editForm, status: e.target.value })
                      }
                    >
                      <option value="Open">Open</option>
                      <option value="InProgress">In Progress</option>
                      <option value="Resolved">Resolved</option>
                      <option value="Closed">Closed</option>
                      <option value="Cancelled">Cancelled</option>
                    </select>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Priority</label>
                    <select
                      className="form-select"
                      value={editForm.priority}
                      onChange={(e) =>
                        setEditForm({ ...editForm, priority: e.target.value })
                      }
                    >
                      <option value="Low">Low</option>
                      <option value="Medium">Medium</option>
                      <option value="High">High</option>
                      <option value="Urgent">Urgent</option>
                    </select>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowEditModal(false)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Update Ticket
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Assign Ticket Modal */}
      {showAssignModal && selectedTicket && (
        <div
          className="modal fade show"
          style={{ display: 'block' }}
          tabIndex="-1"
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Assign Ticket</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowAssignModal(false)}
                ></button>
              </div>
              <form onSubmit={handleAssignSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Assign To *</label>
                    <select
                      className="form-select"
                      value={assignForm.assignedToUserId}
                      onChange={(e) =>
                        setAssignForm({
                          ...assignForm,
                          assignedToUserId: parseInt(e.target.value),
                        })
                      }
                      required
                    >
                      <option value="">Select User</option>
                      {users.map((user) => (
                        <option key={user.id} value={user.id}>
                          {user.name} ({user.email})
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowAssignModal(false)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Assign Ticket
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Add Comment Modal */}
      {showCommentModal && selectedTicket && (
        <div
          className="modal fade show"
          style={{ display: 'block' }}
          tabIndex="-1"
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Add Comment</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowCommentModal(false)}
                ></button>
              </div>
              <form onSubmit={handleCommentSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Comment *</label>
                    <textarea
                      className="form-control"
                      rows="4"
                      value={commentForm.comment}
                      onChange={(e) =>
                        setCommentForm({
                          ...commentForm,
                          comment: e.target.value,
                        })
                      }
                      required
                    ></textarea>
                  </div>
                  {(role.isAdmin || role.isCallingStaff || role.isHOD || role.isEmployee) && (
                    <div className="mb-3">
                      <div className="form-check">
                        <input
                          className="form-check-input"
                          type="checkbox"
                          checked={commentForm.isInternal}
                          onChange={(e) =>
                            setCommentForm({
                              ...commentForm,
                              isInternal: e.target.checked,
                            })
                          }
                        />
                        <label className="form-check-label">
                          Internal Comment (Staff Only)
                        </label>
                      </div>
                    </div>
                  )}
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowCommentModal(false)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Add Comment
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Ticket Details Modal */}
      {showDetailModal && selectedTicket && (
        <div
          className="modal fade show"
          style={{ display: 'block' }}
          tabIndex="-1"
        >
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  Ticket Details - {selectedTicket.ticketNumber}
                </h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowDetailModal(false)}
                ></button>
              </div>
              <div className="modal-body">
                <div className="row mb-3">
                  <div className="col-md-6">
                    <strong>Title:</strong> {selectedTicket.title}
                  </div>
                  <div className="col-md-6">
                    <strong>Status:</strong>{' '}
                    <span className={getStatusBadgeClass(selectedTicket.status)}>
                      {selectedTicket.status}
                    </span>
                  </div>
                </div>
                <div className="row mb-3">
                  <div className="col-md-6">
                    <strong>Priority:</strong>{' '}
                    <span
                      className={getPriorityBadgeClass(selectedTicket.priority)}
                    >
                      {selectedTicket.priority}
                    </span>
                  </div>
                  <div className="col-md-6">
                    <strong>Created By:</strong> {selectedTicket.creatorName}
                  </div>
                </div>
                {selectedTicket.clientName && (
                  <div className="row mb-3">
                    <div className="col-md-6">
                      <strong>Client:</strong> {selectedTicket.clientName}
                    </div>
                  </div>
                )}
                {selectedTicket.assigneeName && (
                  <div className="row mb-3">
                    <div className="col-md-6">
                      <strong>Assigned To:</strong> {selectedTicket.assigneeName}
                    </div>
                  </div>
                )}
                <div className="mb-3">
                  <strong>Description:</strong>
                  <p className="mt-2">{selectedTicket.description}</p>
                </div>
                <div className="mb-3">
                  <strong>Comments:</strong>
                  {loadingComments ? (
                    <Loader />
                  ) : (
                    <div className="mt-2">
                      {comments.length === 0 ? (
                        <p className="text-muted">No comments yet</p>
                      ) : (
                        comments.map((comment) => (
                          <div
                            key={comment.id}
                            className="card mb-2"
                            style={{
                              backgroundColor: comment.isInternal
                                ? '#fff3cd'
                                : '#f8f9fa',
                            }}
                          >
                            <div className="card-body">
                              <div className="d-flex justify-content-between">
                                <div>
                                  <strong>{comment.userName}</strong>
                                  {comment.isInternal && (
                                    <span className="badge bg-warning ms-2">
                                      Internal
                                    </span>
                                  )}
                                </div>
                                <small className="text-muted">
                                  {new Date(comment.createdAt).toLocaleString()}
                                </small>
                              </div>
                              <p className="mb-0 mt-2">{comment.comment}</p>
                            </div>
                          </div>
                        ))
                      )}
                    </div>
                  )}
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowDetailModal(false)}
                >
                  Close
                </button>
                <button
                  type="button"
                  className="btn btn-primary"
                  onClick={() => {
                    setShowDetailModal(false);
                    handleAddComment(selectedTicket);
                  }}
                >
                  Add Comment
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Modal Backdrop */}
      {(showCreateModal ||
        showEditModal ||
        showAssignModal ||
        showCommentModal ||
        showDetailModal) && (
        <div
          className="modal-backdrop fade show"
          onClick={() => {
            setShowCreateModal(false);
            setShowEditModal(false);
            setShowAssignModal(false);
            setShowCommentModal(false);
            setShowDetailModal(false);
          }}
        ></div>
      )}
    </Layout>
  );
};

export default TicketsManagement;

